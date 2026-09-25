---
title: Known Compiler Issues
description: Known bugs and quirks in the IL-to-6502 compiler, with reproduction status
tags:
  - compiler
  - bugs
---
Tracks real, confirmed issues found while working on the IL → 6502 compiler (`Compiler/`). Each entry records what's confirmed, how to reproduce it, and current status. Fixed issues are kept here too (not deleted) since they document real historical bugs and their regression tests.

## FIXED: Static field width bug

**Status:** Fixed. Covered by `Test/StaticFieldLayoutTest.cs` (2 passing regression tests).

**Symptom:** `ILTypeStaticFieldInitPass` reserved exactly 1 byte per static field regardless of its actual storage size (`GetStorageBytes()`). Any multi-byte static field (`float` = 5 bytes, `long`/`ulong` = 2 bytes) silently overlapped the storage of whatever static field was declared next, corrupting both.

**Fix:** Emit `GetStorageBytes()` copies of `.byte 0` per static field instead of always emitting exactly one.

**Note:** this is *not* the same bug as the next entry. Object field layout (`OpNewObj`, `GetFieldPosition`) was already width-correct — only *static* field layout had the width bug.

## FIXED: instance float-field read desyncs the hardware evaluation stack

**Status:** Fixed. Covered by `Test/InstanceFloatFieldBugTest.cs` (2 passing regression tests). Also verified end-to-end on real VICE: the class-based `Demo/RotatingCube.cs` (instance fields, 8 float-field-heavy `ComputeVertexProjection` calls per frame) renders and rotates indefinitely.

**Symptom:** A class with several `float` instance fields, whose method does chained float arithmetic (field reads, several multiplies/adds, a division, a cast to `uint` stored back into a field) called repeatedly in a loop, crashed: the emulated 6502 halted with `PC = $0001`, after roughly 30,000–40,000 executed instructions — well before any method could legitimately return. The identical math as **static** methods/fields ran fine indefinitely.

**Root cause (fully traced and confirmed):** `ILPropertyGettterOptimizer` (`Compiler/ILPropertyGetterOptimizer.cs`) fuses the IL pattern `Ldarg_0; Ldfld` into a single `OpPushFld` operation. `OpPushFld` only ever emits the asm macro `#pushfld8` or `#pushfld16` (picked by a single `is16Bit` bool) — `asm/helper/optimized.asm` has no `#pushfldflt` variant. A `float` field (`GetStorageBytes()==5`) is neither, so it silently fell through to `#pushfld8`, which pushes exactly **1 byte** onto the hardware evaluation stack (this compiler uses the real 6502 stack via `PHA`/`PLA` for expression evaluation — separate from its software `localsStack` used for locals/return addresses) instead of the 5 a float needs.

Each misfire desynced the hardware stack pointer by 4 bytes with no immediate symptom. Measured directly (`SimpleEmulator.Emulator.HardwareStackPointer` sampled at every method-entry breakpoint): drift of **exactly +32 bytes per call** to a method reading 8 such fields (8 × 4 dropped bytes) — clean, deterministic, linear. Eventually a later float op pulled stale bytes as its operand, producing a bogus value (an exponent byte of `$D2`, ~2^82) that a C64 ROM float routine legitimately flagged as overflow, dispatching through the ROM's standard error jump `JMP ($0300)` (the KERNAL IERROR vector). That vector is `00 00` because neither the `SimpleEmulator` harness nor the standalone repro runs the KERNAL cold-start sequence — so it lands at `$0000`, giving the `PC=$0001` signature.

**Fix:** guarded both `ILPropertyGettterOptimizer` rules to skip float fields (`.WithGuard((ctx, w) => w[1].StackContent.Last() != typeof(float))`), falling back to the always-correct unfused `Ldarg_0`+`Ldfld` path (`OpLdfld` already handles float via its `SizeSuffix`, emitting `#ldfldflt`).

**Verification:** standalone repro ran 2,000,000 instructions / 59 calls after the fix with the hardware stack pointer constant — zero drift, zero crash (previously crashed at ~35,000 steps). Full `Test/*.cs` suite: 565 passed, 1 pre-existing skip, 0 failed.

**Note — division is not special:** an earlier hypothesis blamed division for extra codegen cost. Disproved: `#divflt` and `#mulflt` expand identically at the call site, and `Float_Add/Sub/Mul/Div` (`asm/helper/float.asm`) are the same size and always included. The apparent signal came from an extra `addflt` and a duplicated constant in the compared expression.

**Note — unit-test harness budget:** `Test/*.cs` compiles 560+ tests into ONE program (`asm/unittest.asm`) with an extremely tight budget (as little as ~150 bytes headroom before the `$d000` ceiling, `OBJ_TABLES_MAX_START` in `asm/helper/objectTables.asm`). Variants close to the original crashing shape failed to assemble at all, so the repro and verification used a standalone scratch project — see the recipe below.

## NOT YET FIXED, now a compile-time error: instance constructor bodies were never invoked

**Status:** the underlying gap remains (constructors with real logic are unsupported), but it is now a loud compile-time `NotSupportedException` instead of a silent miscompile.

**Symptom (before):** `OpNewObj.ConvertParameter` (`Compiler/Operands/OperandBase.cs`) had the constructor call commented out:

```csharp
// var ctor = $"{t.Name}_x_ctor";
var ctor = "0";
```

`ctor` was hardcoded to `"0"` for every ordinary class — only `Func<T>` (hand-written `Func_1_x_ctor` in `asm/system.asm`) ever had its constructor invoked. `asm/helper/heap.asm`'s `newObj` skips `jsr \ctor` when `ctor=0`. A constructor **body** with any real logic (field assignment, calls, arithmetic) silently never ran; fields stayed zero-initialized, with no error or warning.

**Confirmed via `SimpleEmulator`:** an instance whose constructor set two `float` fields read both as MFLPT zero afterwards, for both parameterized and parameterless constructors. This produced a crash of its own (again `PC=$0001` via the uninitialized `$0300` vector, triggered by a genuine `0f / 0f`) that initially looked identical to the bug above but had a different root cause.

**Why nothing caught it:** every object type across `Test/`, `Demo/`, `Hunchback/`, `C64Presentation/` either has no explicit constructor or uses C# object-initializer syntax (`new T { Field = value }`), which compiles to `Newobj; Dup; Ldc; Stfld…` at the call site and bypasses the constructor body — a separate working path (`ILObjectInitializerOptimizer.cs` → `#newObjInit`).

**What changed:** `OpNewObj.ConvertParameter` now decodes the constructor's IL (same opcode loop as `ILMethodCodePass.cs`) and throws `NotSupportedException` if the body contains anything beyond `Ldarg_0/1/2/3/Ldarg_s/Call/Nop/Ret`. Verified: throws for a constructor that sets a field; does not throw for no ctor, an empty ctor, or object-initializer construction. `Test/`, `Demo/`, `Hunchback/`, `C64Presentation/` all still build cleanly. Workaround for real code: use an explicit `Init()` method or object-initializer syntax (as `Demo/RotatingCube.cs` does).

**Not fixed (out of scope):** actually invoking the constructor (restoring the `jsr \ctor` call with the normal argument-passing convention) — a separate, larger piece of work.

## How to build a standalone repro outside the shared unittest.asm budget

1. Create a scratch project under `.debug-tmp/<name>/` referencing `C64Lib.csproj` (`Exe` if it needs `Main`, else `Library`) containing just the C# shape under test; `dotnet build` it.
2. Compile via `Compiler.exe <dll> <asmOutDir> <asmEntryFile> program` (or `unittest` for a library). Point the output and entry file at locations **under the repo's real `asm/` folder** (e.g. `asm/<name>/`, `asm/<name>_main.asm`) — the entry file's `.include` paths are relative to its own location. Create the output dir first; delete these when done.
3. Assemble: `64tass -o <prg> --long-branch --vice-labels -l <labels> --list <dump.asm> --no-monitor <asmEntryFile>`.
4. Run headlessly with `SimpleEmulator.Emulator`: `LoadPrg`, `SetProgramCounter(0x1000)` (the real entry point, per `RunInEmulatorAspect` — not the `.start` label), then `RunUntil(breakpoints, maxSteps, out stoppedAt, out stepsExecuted)` in a loop, or `Start(address, maxSteps, (pc, steps) => …)` for a ring-buffer trace of the last N instructions before a crash.
5. Useful drift probes sampled at a method-entry breakpoint: `emulator.HardwareStackPointer` (real 6502 SP) and `emulator.GetMemory(0x4b)` (the compiler's `stackPointer` into `localsStack`).
6. `prg/dump.asm` from a failed assembly is stale — 64tass doesn't rewrite it on a `.cerror`; only trust it after a successful build.
