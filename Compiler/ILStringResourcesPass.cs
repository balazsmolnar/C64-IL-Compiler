namespace Compiler;

class ILStringResourcesPass : ICompilerPass
{
    public void Execute(CompilerContext context)
    {
        foreach (var stringResource in context.StringValues)
        {
            if (context.OptimizedStringValues.Contains(stringResource.Value))
                continue;
            string outputLine = $"string_{stringResource.Key} .text \"{stringResource.Value}\",0";
            context.GlobalOutputFile.WriteLine(outputLine);
        }

        foreach (var initValue in context.InitValues)
        {
            var label = context.GetInitValueLabel(initValue);
            context.GlobalOutputFile.WriteLine($"{label} .byte {initValue}");
        }

    }
}