using System.Linq;
using System.Reflection;

namespace Compiler;

class ILTypeStaticFieldInitPass : ICompilerTypePass
{
    public void Execute(CompilerTypeContext context)
    {
        var fields = context.Type.GetFields(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public);
        foreach (var field in fields.Where(f => !f.IsLiteral).OrderBy(f => f.FieldType.IsReferenceCounted() ? 0 : 1))
        {
            // FullName, not Name -- must match OpLdsfld/OpStsfld's own
            // label computation (Operands/OperandBase.cs) and ILCodePass's
            // per-type file naming exactly; see ILCodePass.cs's comment for
            // why short names alone collide (Roslyn's synthesized "<>O").
            string outputLine = $"{context.Type.FullName.ToValidName()}_field_{field.Name.ToValidName()} .byte 0";
            context.OutputFile.WriteLine(outputLine);
        }
    }
}