using System.Collections.Generic;
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
    // Matches zeropage.asm's zp_local0..zp_local3 exactly (4, not 8 --
    // that pool is restricted to bytes inside OnInterrupt's saved/restored
    // range, and only 4 such bytes are genuinely free; see its comment).
    static readonly string[] Pool = { "zp_local0", "zp_local1", "zp_local2", "zp_local3" };

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
    // all confirmed inline. Still excluded, not yet checked: Conv_ovf_*,
    // and anything involving fields/arrays/strings/objects (Ldfld/Stfld/
    // Ldelem*/Stelem*/Newarr/Newobj/Box/Unbox/Castclass/Isinst/Switch/
    // Call/Callvirt -- the last two, plus Newarr/Ldelem*/Stelem* and Div/
    // Rem above, are already KNOWN to jsr, not just unchecked). There is
    // no reliable way to eyeball "does this opcode's Emit() ever jsr
    // anywhere" from outside Operands/OperandBase.cs, so an unrecognized
    // opcode disqualifies the method by default rather than being assumed
    // safe -- widen this set only by reading the macro body first, the
    // same way every entry already here was added.
    //
    // Measured, not assumed: widening this set from "just arithmetic/
    // branches" to this full list changed Hunchback/Demo/Catacombs'
    // compiled size by exactly zero bytes (still one qualifying method per
    // project, the same Delay.Wait-shaped helper -- see conversation
    // history). An unshipped experiment additionally allowing Call/
    // Callvirt to C64Lib.* (not Func<T>, not same-assembly, NOT audited
    // for whether those routines touch zp_local0-3 -- would need that
    // before ever shipping it) only bought 69/36/0 bytes respectively.
    // Conclusion: for this codebase's actual code shape, the ceiling on
    // LOCAL-only promotion is low regardless of how far the opcode/call
    // allowlist is widened -- most real state here lives in instance
    // fields, not method-local variables with tight, small loops. A
    // meaningfully bigger win would mean promoting FIELDS, a materially
    // different (shared/aliased, GC-tracked, concurrent-access) problem,
    // not a wider version of this one.
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
    };

    public void Execute(CompilerMethodContext context)
    {
        if (context.Lines == null || context.Method.IsAbstract)
            return;

        foreach (var line in context.Lines)
        {
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
            var type = body.LocalVariables[i].LocalType;
            if (type.GetStorageBytes() != 1 || type.IsReferenceCounted())
                continue;

            context.PromotedLocals[i] = Pool[slot];
            slot++;
        }
    }
}
