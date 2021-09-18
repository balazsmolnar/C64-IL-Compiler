using System;
using System.IO;
using System.Linq;
using System.Resources;
using C64Lib;

namespace Compiler
{
    class ILRawAssemblyPass : ICompilerPass
    {
        public void Execute(CompilerContext context)
        {
            context.GlobalOutputFile.WriteLine(".include \"./data.asm\"");
            var attributes = context.Assembly.GetCustomAttributes(typeof(C64Lib.RawAssemblyAttribute), false).OfType<RawAssemblyAttribute>();

            using (var outputFile = File.CreateText(Path.Combine(context.OutputDirectory, $"data.asm")))
            {
                foreach (var attr in attributes.OrderBy(a => a.Order))
                {
                    if (!string.IsNullOrEmpty(attr.Asm))
                        outputFile.WriteLine(attr.Asm);
                    else if (!string.IsNullOrEmpty(attr.Resource))
                    {

                        using (Stream stream = context.Assembly.GetManifestResourceStream(attr.Resource))
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            string result = reader.ReadToEnd();
                            outputFile.WriteLine(result);
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("Asm or Resource must be specified.");
                    }
                }
            }
        }
    }
}
