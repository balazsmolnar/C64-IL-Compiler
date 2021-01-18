using System.Reflection;

namespace Compiler
{
    class ILTypeStaticFieldInitPass : ICompilerTypePass
    {
        public void Execute(CompilerTypeContext context)
        {
            var fields = context.Type.GetFields(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public);
            foreach (var field in fields)
            {
                string outputLine = $".{context.Type.Name}_field_{field.MetadataToken} !byte 0";
                context.CompilerContext.OutputFile.WriteLine(outputLine);
            }
        }
    }
}
