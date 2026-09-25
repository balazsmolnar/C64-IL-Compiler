using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using Compiler.Ops;

namespace Compiler;

class ILPropertyGettterOptimizer : PeepholeOptimizerPass
{
    // IL_0001: ldarg.0
    // IL_0002: ldfld uint32 Player::x_
    // IL_0007: stloc.0
    // // (no C# code)
    // IL_0008: br.s IL_000a
    // IL_000a: ldloc.0
    protected override IEnumerable<PeepholeRule> Rules => new[]
    {
        new PeepholeRule(
            (ctx, w) => new OpPushFld(w[0].RawParameter.ToString(), w[1].RawParameter.ToString(), w[1].Operation.Is16Bit(ctx, w[1])),
            l => l.Operation is OpLdarg,
            l => l.Operation is OpLdfld,
            l => l.Operation is OpStloc,
            l => l.OpCode == ILOpCode.Br_s,
            l => l.Operation is OpLdloc)
            // OpPushFld only ever emits #pushfld8/#pushfld16 (picked by the
            // single is16Bit bool) -- there's no #pushfldflt. A float field
            // (GetStorageBytes()==5, TypeExtensions.cs) is neither, so
            // without this guard it silently fell through to #pushfld8,
            // which pushes exactly 1 byte (asm/helper/optimized.asm) onto
            // the hardware evaluation stack instead of the 5 a float needs
            // -- desyncing that stack by 4 bytes per occurrence with no
            // immediate symptom, until enough of these accumulate (e.g. 8
            // float-field reads in one instance method called repeatedly)
            // that a later float op pulls stale/foreign bytes as its
            // operand, producing a bogus value a ROM float routine then
            // legitimately flags as overflow. Guarding it out here leaves
            // the window unoptimized, falling back to the always-correct
            // unfused Ldarg_0+Ldfld path (OpLdfld's own SizeSuffix already
            // handles float correctly via "flt").
            .WithGuard((ctx, w) => w[1].StackContent.Last() != typeof(float)),

        // NOTE: original never set StackContent for this second, shorter pattern
        // (leaving it null, ILOperation's default) -- preserved via the override
        // below rather than defaulting to the last matched line's StackContent.
        new PeepholeRule(
            (ctx, w) => new OpPushFld(w[0].RawParameter.ToString(), w[1].RawParameter.ToString(), w[1].Operation.Is16Bit(ctx, w[1])),
            l => l.OpCode == ILOpCode.Ldarg_0,
            l => l.Operation is OpLdfld)
            .WithStackContent(w => null)
            // See the guard comment on the rule above -- same float/#pushfld8
            // gap, same fix.
            .WithGuard((ctx, w) => w[1].StackContent.Last() != typeof(float)),
    };
}
