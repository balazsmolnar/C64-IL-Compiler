namespace Compiler
{
    class ILStringResourcesPass : ICompilerPass
    {
        public void Execute(CompilerContext context)
        {
            foreach (var stringResource in context.StringValues)
            {

                string outputLine = $"string_{stringResource.Key} .text \"{stringResource.Value}\",0";
                context.GlobalOutputFile.WriteLine(outputLine);
            }
        }
    }
}
