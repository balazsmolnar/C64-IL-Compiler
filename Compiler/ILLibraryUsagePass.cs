using System.Reflection;
using System.Reflection.Metadata;

namespace Compiler;

// Scans every compiled method's Call/Callvirt targets for any reference
// into the C64Lib namespace, and records the target's own compiled label
// (MethodBaseExtensions.GetLabel()) on the shared CompilerContext -- read
// by ILLibraryFlagsPass once all methods are done, to decide which
// individual hand-written asm subroutines a program actually needs (each
// wrapped in its own `.weak Flag_X = 0 .endweak .if Flag_X ... .endif` in
// asm/C64*.asm; see that pass for the full mechanism, including the
// handful of asm-internal-only helpers this scan can never see directly).
// Runs in the setup group, right after ILMethodCodePass and before any
// optimizer pass gets a chance to rewrite/remove a Call/Callvirt
// operation, so it sees exactly what the original IL emitted.
//
// Declaring types are matched by name prefix rather than typeof(C64Lib.X)
// -- context.Assembly is loaded at runtime via Assembly.LoadFrom, which is
// not guaranteed to be the same load context as the C64Lib this compiler
// itself was built against, so a typeof() reference-equality check could
// silently never match. The rest of this codebase already works around the
// same hazard by name (see OperandBase.cs's ReflectedType.Name.StartsWith
// ("Func") check for Func<> delegates).
//
// System.MathF.Sin/Cos/Sqrt get the exact same tracking as C64Lib.* --
// they're real BCL methods, not C64Lib stand-ins (see asm/helper/float.asm's
// MathF_Sin/Cos/Sqrt), but they're still ordinary hand-written-asm-backed
// library calls this program either uses or doesn't. Named explicitly
// (method-by-method, not by namespace prefix like C64Lib.'s whole surface)
// to match OperandBase.cs's EnsureCallIsResolvable's own equally narrow
// allowlist for these three -- keeping both checks in sync is what makes an
// unsupported call like MathF.Tan a clear compiler error there instead of
// silently getting a dead-code flag here that nothing ever defines a
// routine for.
class ILLibraryUsagePass : ICompilerMethodPass
{
    public void Execute(CompilerMethodContext context)
    {
        if (context.Lines == null)
            return;

        foreach (var line in context.Lines)
        {
            if (line.OpCode != ILOpCode.Call && line.OpCode != ILOpCode.Callvirt)
                continue;

            var method = context.CompilerContext.Assembly.ManifestModule.ResolveMethod((int)line.OriginalParameter) as MethodBase;
            var declaringType = method?.DeclaringType?.FullName;
            bool isLibraryCall = declaringType != null && declaringType.StartsWith("C64Lib.");
            bool isMathF = declaringType == "System.MathF" && MathFSupport.SupportedMethods.Contains(method.Name);
            if (!isLibraryCall && !isMathF)
                continue;

            context.CompilerContext.UsedLibraryLabels.Add(method.GetLabel());
        }
    }
}
