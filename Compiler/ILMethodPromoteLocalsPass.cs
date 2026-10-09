using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Metadata;

namespace Compiler;

// PROTOTYPE: decides, once per method, which of its LOCAL VARIABLES (never
// a parameter, never `this`) can live in a fixed zero-page byte instead of
// a localsStack-relative slot -- see zp_local0's own comment in
// asm/helper/zeropage.asm for the pool itself and the full safety
// reasoning. Populates context.PromotedLocals; every operand class that
// reads/writes a local (OpLdloc/OpLdloc_s/OpStloc/OpStloc_s) and the
// zp-aware inc/dec/init-var peephole rules (ILMethodIncOptimizer/
// ILMethodDecOptimizer/ILMethodSetVariableOptimizer) consult it.
//
// Deliberately much narrower than the earlier (reverted) whole-method "fast
// locals" prototype: parameters, `this`, and the prologue/epilogue's
// return-address handling are never touched, so this can't repeat that
// prototype's bug (see git history / session notes -- the fast prologue
// pulled parameters off the hardware stack before lifting the jsr return
// address out of the way). Only individual ldloc/stloc accesses change.
//
// A setup pass (Compiler/Program.cs), so it runs exactly once per method,
// after ILMethodCodePass has populated context.Lines and before anything
// else reads PromotedLocals.
class ILMethodPromoteLocalsPass : ICompilerMethodPass
{
    // Matches zeropage.asm's own Pool exactly -- 7 slots, not contiguously
    // numbered: zp_local1 ($29) is RETIRED, not just unlisted -- it
    // aliases tmpPointer+1 (heap_tmp_pointer = $28), which resolveObjPtr
    // (and so every single field-access macro: pushfld8/16/flt,
    // setfld8/16(_const), incfld) overwrites on every field read or
    // write. Harmless for as long as Ldfld/Stfld were excluded from
    // SafeOpCodes below; became a silent, real data-corruption bug (a
    // promoted local's value stomped by the very next field access) the
    // moment they were added. zp_local4-7 ($49/$4a/$4c/$4d) were added
    // later, a second, disjoint batch of genuinely free bytes outside
    // zp_interrupt_save_start/len's own contiguous block -- OnInterrupt
    // (asm/C64.asm) saves/restores them separately, see its own comment.
    // See zeropage.asm's own comment on zp_local0 for the full incident
    // and the fuller reasoning for both batches.
    static readonly string[] Pool = { "zp_local0", "zp_local2", "zp_local3", "zp_local4", "zp_local5", "zp_local6", "zp_local7" };

    // FAIL-CLOSED allowlist, not a denylist of "the calls we thought of":
    // an earlier version of this pass only excluded Call/Callvirt/Newobj,
    // reasoning "no calls, so nothing else can run mid-body" -- but missed
    // that Newarr/Ldelem*/Stelem* compile to #newArr8/#ldelem8/#stelem8
    // macros that themselves jsr into shared heap/array-indexing routines
    // (found via a real hang in Test/Compiler.Test's
    // FloatTests.TestArray_IndexedWriteAndReadInLoop -- a promoted loop
    // index corrupted by whatever scratch that routine used internally).
    //
    // Division ditto: asm/helper/division.asm's own comment says div8/
    // rem8 are "real subroutines, not macros with an inline loop the way
    // mul8/mul16 are" -- i.e. Div/Rem jsr out too, so they're excluded
    // even though they look like "plain arithmetic". Every opcode in this
    // set was individually checked against its asm/helper/*.asm macro
    // body for the absence of `jsr`:
    //   add8/sub8/mul8/and8/or8/xor8/not8 (arithmetic/bitwise)
    //   shift_left8/shift_right8/negate8 (Shl/Shr/Shr_un/Neg)
    //   compareLess8/compareLess_unsigned8/compareGreater8/
    //     compareGreater_unsigned8/compareEqual8 (Clt/Clt_un/Cgt/Cgt_un/Ceq)
    //   conv_16_8/conv_8_16 (Conv_i1/Conv_u1/Conv_i4/Conv_u4/Conv_i8/Conv_u8)
    // all confirmed inline. Ldfld/Stfld ALSO confirmed safe (added later):
    // both only ever jsr into resolveObjPtr (asm/helper/object.asm) --
    // `lda objTableLow,x / sta tmpPointer / lda objTableHigh,x / sta
    // tmpPointer+1 / rts` -- which touches only A and tmpPointer/
    // tmpPointer+1, and every macro that calls it (pushfld8/16/flt,
    // pushfld8/16/flt_at, setfld8/16(_const), incfld -- asm/helper/
    // optimized.asm) was read in full too: all of them only touch A/X/Y,
    // stackPointer, localsStack, tmpPointer, never zp_local0-3. Unlike
    // Call/Callvirt/Newobj below, neither opcode can ever recurse back
    // into another compiled C# method -- resolveObjPtr is a fixed,
    // non-reentrant leaf routine -- so this doesn't have that category's
    // pool-sharing hazard either.
    //
    // Enabling these two DID require one extra fix, not just the allowlist
    // entry: ILSetFieldOptimizer's `obj.field = someLocal;` rule builds
    // OpSetfld straight from the matched OpLdloc's plain, unguarded
    // RawParameter (the localsStack rel_pos) -- unlike ILMethodIncOptimizer/
    // DecOptimizer/SetVariableOptimizer, which all check
    // ctx.PromotedLocalSlot(...) first. That was harmless before this
    // change (a method with a Stfld could never ALSO have a promoted
    // local, since Stfld disqualified the whole method), but becomes a
    // live stale-read bug the moment Stfld is allowed here -- fixed in
    // that rule's own guard, not here; see its comment.
    //
    // Newobj added too (unconditionally safe, not just audited): OpNewObj.
    // ConvertParameter (Operands/OperandBase.cs) throws a compile-time
    // UnsupportedFeatureException for ANY constructor that does real work
    // (sets a field, calls a method) -- every constructor that survives to
    // codegen is GUARANTEED trivial (Ldarg*/Call/Nop/Ret only, hardcoded
    // "ctor" parameter of 0, no actual call emitted at all), so Newobj
    // always compiles to nothing but the #newObj allocation macro, which
    // (like every other routine in asm/helper/heap.asm and asm/GC.asm) was
    // grepped for zp_local/the raw $25/$29/$2a/$2b addresses and found
    // clean -- same as Ldfld/Stfld's own audit above, just for a
    // mandatorily-enforced-trivial opcode instead of an always-trivial one.
    //
    // Call/Callvirt: NOT blanket-allowed (still excluded by default, same
    // reasoning as before -- they can recurse into ANOTHER compiled
    // method, and zp_local0-3 is one small pool shared by EVERY leaf
    // method in the whole program, not indexed by call depth the way
    // localsStack is; zp_local0's own comment in zeropage.asm). But a
    // measured call-graph analysis (see conversation history) found that
    // in Hunchback specifically, 0 of the methods that call another
    // HUNCHBACK method ever call one that's itself a zero-call leaf --
    // the real same-assembly call graph just isn't shallow enough for a
    // simple "callee is also a leaf" check to help, and building the full
    // whole-program interference analysis that WOULD help is real,
    // unattempted work with a confirmed-near-zero payoff for this
    // codebase's actual shape. BUT 47 of 95 Hunchback methods call
    // EXCLUSIVELY into C64Lib/BCL -- never into another Hunchback method
    // -- and THOSE calls have exactly Ldfld/Stfld's own safety shape: a
    // fixed, external, non-reentrant routine, never another method this
    // pass itself might also be promoting locals in. A full grep of EVERY
    // asm file (not just helper/) for zp_local/$25/$29/$2a/$2b, same
    // technique as the Ldfld/Stfld audit, found zero hits anywhere outside
    // zeropage.asm's own definitions and two doc-comment mentions -- so
    // IsSafeExternalCall below allows a Call/devirtualizable-Callvirt site
    // specifically when its resolved target's declaring type lives outside
    // CompilerContext.Assembly (C64Lib, C64TestFramework, BCL -- anything
    // that was never, and will never be, run through this pass itself). A
    // non-devirtualizable Callvirt (real dynamic dispatch -- the actual
    // target depends on the runtime type, which COULD be a same-assembly
    // override) is deliberately NOT included in this relaxation: its real
    // target isn't statically known, so "lives outside this assembly"
    // can't be verified for it the way it can for a plain Call or a
    // devirtualized Callvirt.
    //
    // Ldstr/Ldsfld/Ldsflda/Switch ALSO confirmed safe (found by
    // instrumenting this pass directly against Hunchback and reading off
    // exactly which opcode rejected each method that had an otherwise-
    // eligible local -- the biggest single bucket among them, 12 of 26,
    // was Ldstr):
    //   OpLdstr compiles to #stack_push_pointer (asm/helper/stack.asm) --
    //     `lda #>ptr / pha / lda #<ptr / pha`, a pure compile-time-constant
    //     address push, same shape as Ldc_i4 and equally jsr-free.
    //   OpLdsld (Ldsfld/Ldsflda share this class -- same "address-of and
    //     value-of are the same path" reasoning as Ldloca/Ldloc already
    //     noted below) compiles to #stack_push_var/var16/var_mflpt (asm/
    //     helper/stack.asm) -- all `lda <fixed address>; pha` repeated,
    //     direct reads of a fixed static-field memory location, no jsr.
    //   OpSwitch compiles to #switch (asm/helper/branch.asm) -- `asl; tax;
    //     lda jump_table+1,x; pha; lda jump_table,x; pha; rts`, the
    //     standard "push target-1, rts-to-jump" indirect-jump idiom; the
    //     trailing rts jumps via the pushed address, it does not return
    //     from a call, and nothing here touches zp_local0-3.
    //
    // Newarr/Ldelem*/Stelem*/Ldlen ALSO confirmed safe, after re-auditing
    // the exact thing the pass's own history once regressed on (a promoted
    // loop index corrupted by array-indexing scratch, Test/Compiler.Test's
    // FloatTests.TestArray_IndexedWriteAndReadInLoop -- a real hang, not
    // just a wrong value, predating the current, carefully-audited
    // zp_local0-3 addresses). Read every macro body, not just grepped:
    //   ldelem/ldelem16/ldelemRef/ldelemflt and stelem/stelem16/stelemRef/
    //     stelemflt (asm/helper/heap.asm) each only ever jsr resolveObjPtr
    //     -- the exact same already-proven-clean routine Ldfld/Stfld rely
    //     on -- never newObjL/GC, so they can't allocate or collect.
    //   ldlen/ldlen16 (heap.asm) don't jsr anywhere at all -- a bare
    //     `lda objTableSize,x` (/ `lsr`), register-only.
    //   Newarr (OpNewArr -> #newArr/newArrRef/newArr16/newArrflt, heap.asm)
    //     does allocate -- jsr newObjL/newObjLInit (asm/helper/object.asm)
    //     -- but THAT routine only ever touches zp_param0-2_low/high,
    //     heapPointer, and the objTable* arrays, and its one failure path
    //     (Runtime_CheckHeapRoom -> Runtime_Fault, asm/helper/fault.asm)
    //     never returns to the caller at all (halts/breaks), so there's no
    //     "corrupt then continue" case either. Crucially: allocation in
    //     this runtime NEVER triggers GC.Collect() automatically -- out of
    //     room is a hard fault, not a collect-and-retry -- so the
    //     "allocation might reenter GC mid-flight" risk this pass's other
    //     entries (Call/Callvirt, same-assembly) have to reason about
    //     doesn't even apply here; GC.Collect only ever runs via an
    //     explicit user call, already excluded as an ordinary Call.
    //
    // Still excluded, not yet checked: Conv_ovf_*, Box/Unbox/Castclass/
    // Isinst, and same-assembly Call/Callvirt (the interprocedural case
    // above -- confirmed, via the same instrumented run, to still account
    // for 8 of the 26 rejected methods, consistent with the earlier
    // call-graph measurement's own conclusion that this case isn't worth
    // building). There is no reliable way to eyeball "does this opcode's
    // Emit() ever jsr anywhere" from outside Operands/OperandBase.cs, so an
    // unrecognized opcode disqualifies the method by default rather than
    // being assumed safe -- widen this set only by reading the macro body
    // first, the same way every entry already here was added.
    //
    // Measured, not assumed: widening this set from "just arithmetic/
    // branches" to the full list above (before Ldfld/Stfld) changed
    // Hunchback/Demo/Catacombs' compiled size by exactly zero bytes (still
    // one qualifying method per project, the same Delay.Wait-shaped helper
    // -- see conversation history). Adding Ldfld/Stfld, then Call/Callvirt-
    // to-external-assembly plus Newobj, then Ldstr/Ldsfld/Ldsflda/Switch,
    // were each real, separate data points (not zero). Newarr/Ldelem*/
    // Stelem*/Ldlen here is a FIFTH data point -- see Hunchback/measure.bat's
    // own before/after for the actual number; don't assume it follows any
    // prior pattern.
    static readonly HashSet<ILOpCode> SafeOpCodes = new()
    {
        ILOpCode.Nop, ILOpCode.Dup, ILOpCode.Pop,
        ILOpCode.Ldc_i4, ILOpCode.Ldc_i4_s,
        ILOpCode.Ldc_i4_0, ILOpCode.Ldc_i4_1, ILOpCode.Ldc_i4_2, ILOpCode.Ldc_i4_3,
        ILOpCode.Ldc_i4_4, ILOpCode.Ldc_i4_5, ILOpCode.Ldc_i4_6, ILOpCode.Ldc_i4_7,
        ILOpCode.Ldc_i4_8, ILOpCode.Ldc_i4_m1,
        ILOpCode.Ldloc, ILOpCode.Ldloc_s,
        ILOpCode.Ldloc_0, ILOpCode.Ldloc_1, ILOpCode.Ldloc_2, ILOpCode.Ldloc_3,
        ILOpCode.Stloc, ILOpCode.Stloc_s,
        ILOpCode.Stloc_0, ILOpCode.Stloc_1, ILOpCode.Stloc_2, ILOpCode.Stloc_3,
        ILOpCode.Ldarg, ILOpCode.Ldarg_s,
        ILOpCode.Ldarg_0, ILOpCode.Ldarg_1, ILOpCode.Ldarg_2, ILOpCode.Ldarg_3,
        ILOpCode.Add, ILOpCode.Sub, ILOpCode.Mul,
        ILOpCode.And, ILOpCode.Or, ILOpCode.Xor, ILOpCode.Not,
        ILOpCode.Shl, ILOpCode.Shr, ILOpCode.Shr_un, ILOpCode.Neg,
        ILOpCode.Ceq, ILOpCode.Cgt, ILOpCode.Cgt_un, ILOpCode.Clt, ILOpCode.Clt_un,
        ILOpCode.Conv_i1, ILOpCode.Conv_u1, ILOpCode.Conv_i4, ILOpCode.Conv_u4,
        ILOpCode.Conv_i8, ILOpCode.Conv_u8,
        ILOpCode.Br, ILOpCode.Br_s,
        ILOpCode.Brtrue, ILOpCode.Brtrue_s, ILOpCode.Brfalse, ILOpCode.Brfalse_s,
        ILOpCode.Beq, ILOpCode.Beq_s, ILOpCode.Bne_un, ILOpCode.Bne_un_s,
        ILOpCode.Blt, ILOpCode.Blt_s, ILOpCode.Blt_un, ILOpCode.Blt_un_s,
        ILOpCode.Ble, ILOpCode.Ble_s, ILOpCode.Ble_un, ILOpCode.Ble_un_s,
        ILOpCode.Bgt, ILOpCode.Bgt_s, ILOpCode.Bgt_un, ILOpCode.Bgt_un_s,
        ILOpCode.Bge, ILOpCode.Bge_s, ILOpCode.Bge_un, ILOpCode.Bge_un_s,
        ILOpCode.Ret,
        ILOpCode.Ldfld, ILOpCode.Stfld,
        ILOpCode.Newobj,
        ILOpCode.Ldstr,
        ILOpCode.Ldsfld, ILOpCode.Ldsflda,
        ILOpCode.Switch,
        ILOpCode.Newarr,
        ILOpCode.Ldelem_ref, ILOpCode.Stelem_ref,
        ILOpCode.Ldelem_i1, ILOpCode.Ldelem_u1, ILOpCode.Stelem_i1,
        ILOpCode.Ldelem_i4, ILOpCode.Ldelem_u4, ILOpCode.Stelem_i4,
        ILOpCode.Ldelem_i8, ILOpCode.Stelem_i8,
        ILOpCode.Ldelem_r4, ILOpCode.Stelem_r4,
        ILOpCode.Ldlen,
    };

    public void Execute(CompilerMethodContext context)
    {
        if (context.Lines == null || context.Method.IsAbstract)
            return;

        foreach (var line in context.Lines)
        {
            if (line.OpCode == ILOpCode.Call || line.OpCode == ILOpCode.Callvirt)
            {
                if (!IsSafeExternalCall(context.CompilerContext, line))
                    return;
                continue;
            }
            if (!SafeOpCodes.Contains(line.OpCode))
                return;
        }

        var body = context.Method.GetMethodBody();
        if (body == null)
            return;

        // Ldloca_s/Ldloca (address-of a local) need no special exclusion
        // here: this compiler has no ref/out/pointer support at all, so
        // CommandMap.cs already maps both straight onto OpLdloc_s/OpLdarg_s
        // -- "address of a local" and "value of a local" are the same
        // Emit() path here, which already consults PromotedLocals.
        int slot = 0;
        for (int i = 0; i < body.LocalVariables.Count && slot < Pool.Length; i++)
        {
            // See TrivialSubstitutionSourceLocals's own comment
            // (ICompilerPass.cs): ILMethodInliningPass (runs before this
            // pass) may have already baked this exact local's rel_pos
            // into a trivially-substituted inlined call site as a plain
            // localsStack address -- promoting it here would move its
            // real value to zp_localN while that already-spliced read
            // keeps reading the now-never-written localsStack slot.
            if (context.TrivialSubstitutionSourceLocals.Contains(i))
                continue;

            var type = body.LocalVariables[i].LocalType;
            if (type.GetStorageBytes() != 1 || type.IsReferenceCounted())
                continue;

            context.PromotedLocals[i] = Pool[slot];
            slot++;
        }
    }

    // Safe specifically because the target can never be ANOTHER method
    // this same pass might also be promoting locals in -- see this
    // class's own top comment for the full reasoning (the asm-level audit
    // plus why a non-devirtualizable Callvirt is excluded).
    private static bool IsSafeExternalCall(CompilerContext compilerContext, ILOperation line)
    {
        var target = compilerContext.Assembly.ManifestModule.ResolveMethod((int)line.OriginalParameter) as MethodBase;
        if (target == null)
            return false;
        if (line.OpCode == ILOpCode.Callvirt && !target.IsDevirtualizable(compilerContext.Assembly))
            return false;
        return target.DeclaringType != null && target.DeclaringType.Assembly != compilerContext.Assembly;
    }
}
