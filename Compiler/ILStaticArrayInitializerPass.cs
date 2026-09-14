using System;
using System.Reflection;
using System.Reflection.Metadata;
using Compiler.Ops;

namespace Compiler;

// Recognizes Roslyn's codegen for a `static readonly <primitive>[] =
// { lit, lit, ... }` field initializer:
//   newarr; dup; ldtoken <PrivateImplementationDetails blob field>;
//   call RuntimeHelpers.InitializeArray(Array, RuntimeFieldHandle);
//   stsfld <field>
//
// This compiler has no mechanism that runs a type's static constructor at
// all (nothing ever called it -- see ILCodePass, which now compiles and
// ILEntryPointPass, which now calls it), so even once a .cctor is
// compiled, this specific IL shape would still be a dead end: parsing the
// blob field's raw embedded bytes needs low-level PE/metadata reading this
// compiler has no other use for. Sidestepped entirely: Compiler.exe is
// itself a normal .NET program that already loaded the target assembly via
// reflection (context.Assembly) -- reading the field's value with
// FieldInfo.GetValue(null) triggers the CLR's own real static initializer
// in the compiler's own process (a pure compile-time value extraction, no
// effect on the compiled C64 program) and hands back the fully-populated
// array directly, no blob parsing needed.
//
// Collapses the whole 4-op window (Newarr..Call) into one #newArrInit call
// referencing a baked .byte data label -- same technique
// ILObjectInitializerOptimizer already uses for object-initializer chains
// via #newObjInit. The Stsfld that follows is deliberately left untouched
// (same as that pass leaves its final store alone): #newArrInit pushes the
// new array's handle exactly like Newarr did, so the existing, unmodified
// Stsfld codegen just consumes it normally.
class ILStaticArrayInitializerPass : ICompilerMethodPass
{
    public void Execute(CompilerMethodContext context)
    {
        if (!context.CompilerContext.Optimize)
            return;

        var lines = context.Lines;
        for (int i = 0; i + 4 < lines.Count; i++)
        {
            if (lines[i].Optimized ||
                !(lines[i].Operation is OpNewArr) ||
                !(lines[i + 1].Operation is OpDup) ||
                lines[i + 2].OpCode != ILOpCode.Ldtoken ||
                !(lines[i + 3].Operation is OpCall) ||
                !(lines[i + 4].Operation is OpStsfld))
                continue;

            var callTarget = context.CompilerContext.Assembly.ManifestModule.ResolveMethod((int)lines[i + 3].OriginalParameter);
            if (callTarget.DeclaringType != typeof(System.Runtime.CompilerServices.RuntimeHelpers) ||
                callTarget.Name != "InitializeArray")
                continue;

            var field = context.Method.ReflectedType.Module.ResolveField((int)lines[i + 4].OriginalParameter);
            var array = field.GetValue(null) as Array;
            if (array == null)
                throw new InvalidOperationException(
                    $"ILStaticArrayInitializerPass: static field {field} looked like a constant array " +
                    "literal (Newarr;Dup;Ldtoken;Call InitializeArray;Stsfld) but reflecting its real " +
                    "value returned null -- can't bake its contents at compile time.");

            var mem = new string[array.Length];
            for (int k = 0; k < array.Length; k++)
                mem[k] = Convert.ToInt64(array.GetValue(k)).ToString();

            var memLabel = context.CompilerContext.GetInitValueLabel(string.Join(',', mem));

            var newOperation = new ILOperation
            {
                Operation = new OpNewArrInit(array.Length, memLabel),
            };
            newOperation.RawParameter = newOperation.Operation.ConvertParameter(context, null);
            newOperation.StackContent = lines[i].StackContent;

            lines.Insert(i + 4, newOperation);
            lines[i].Optimized = true;
            lines[i + 1].Optimized = true;
            lines[i + 2].Optimized = true;
            lines[i + 3].Optimized = true;
        }
    }
}
