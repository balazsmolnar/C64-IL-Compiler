using System.Linq;
using System.Reflection;

namespace Compiler
{
    class ILTypeStaticFieldInitPass : ICompilerTypePass
    {
        public void Execute(CompilerTypeContext context)
        {
            var fields = context.Type.GetFields(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public);
            foreach (var field in fields.Where(f => !f.IsLiteral).OrderBy(f => f.FieldType.IsReferenceCounted() ? 0 : 1))
            {
                string outputLine = $"{context.Type.Name.ToValidName()}_field_{field.Name.ToValidName()} .byte 0";
                context.OutputFile.WriteLine(outputLine);
            }
        }
    }
}
