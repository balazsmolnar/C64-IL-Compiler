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
                string outputLine = $".{context.Type.Name}_field_{field.Name} !byte 0";
                context.CompilerContext.OutputFile.WriteLine(outputLine);
            }
        }
    }
}
