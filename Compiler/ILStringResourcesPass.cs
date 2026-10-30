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

        // One routine per constant divisor in use (OpDivConst); skip = the
        // leading dividend bits that can never reach the divisor.
        foreach (var (width, divisor) in context.DivConstRoutines)
        {
            int skip = 0;
            while ((2 << skip) <= divisor)
                skip++;
            context.GlobalOutputFile.WriteLine($"DivConst{width}_{divisor}");
            context.GlobalOutputFile.WriteLine($"    #div_const{width}_body {divisor}, {skip}");
        }

    }
}