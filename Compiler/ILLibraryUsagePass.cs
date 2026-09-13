using System.Reflection;
using System.Reflection.Metadata;

namespace Compiler;

// Scans every compiled method's Call/Callvirt targets for references into
// C64Lib.Joystick/JoystickCollection, C64Lib.Sound, or C64Lib.Debug, and
// records the result on the shared CompilerContext. Runs in the setup group,
// right after ILMethodCodePass and before any optimizer pass gets a chance
// to rewrite/remove a Call/Callvirt operation, so it sees exactly what the
// original IL emitted.
//
// Declaring types are matched by full name rather than typeof(C64Lib.X) --
// context.Assembly is loaded at runtime via Assembly.LoadFrom, which is not
// guaranteed to be the same load context as the C64Lib this compiler itself
// was built against, so a typeof() reference-equality check could silently
// never match. The rest of this codebase already works around the same
// hazard by name (see OperandBase.cs's ReflectedType.Name.StartsWith("Func")
// check for Func<> delegates).
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
            if (declaringType == null)
                continue;

            if (declaringType == "C64Lib.Joystick" || declaringType == "C64Lib.JoystickCollection")
                context.CompilerContext.UsesJoystick = true;
            else if (declaringType == "C64Lib.Sound")
                context.CompilerContext.UsesSound = true;
            else if (declaringType == "C64Lib.Debug")
                context.CompilerContext.UsesDebug = true;
        }
    }
}
