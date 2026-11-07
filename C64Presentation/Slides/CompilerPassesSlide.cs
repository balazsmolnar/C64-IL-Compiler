using C64Lib;
using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides;

// The pipeline C# -> IL -> macros -> 6502, one stage per press; each
// stage's code replaces the previous one on the card below.
static class CompilerPassesSlide
{
    const string K = Theme.Kw, T = Theme.Ty, N = Theme.Num, C = Theme.Com;
    const uint CodeTop = 10;

    public static Slide Create() => new SimpleSlide
    {
        Elements =
        [
            new Title { Text = "How does it work?" },
            Code("Program.cs", [$"{K}static uint {T}Add(uint a, uint b)", "    => a + b;"]),
            Stage(2, "C#", Theme.Brand),
            Step(7, ".NET"),
            Code("IL", [$"{K}ldarg.0", $"{K}ldarg.1", $"{K}add", $"{K}ret"]),
            Stage(12, "IL", Theme.Brand),
            Step(17, "ours"),
            Code("64tass macros", [
                $"{K}#locals_push_value8 {N}1 {C}; ldarg.0",
                $"{K}#locals_push_value8 {N}2 {C}; ldarg.1",
                $"{K}#add8                {C}; add",
                $"{K}#method_exit {N}4, []    {C}; ret"]),
            Stage(22, "asm", Theme.Accent2),
            Step(27, "64tass"),
            Code("6502, today: 12 bytes", [
                $"{T}Program_Add",
                $"  {K}lda {T}SF_TOP-512  {C}; b",
                $"  {K}sta {T}zp_param1_low",
                $"  {K}lda {T}SF_TOP-511  {C}; a",
                $"  {K}clc",
                $"  {K}adc {T}zp_param1_low",
                $"  {K}rts             {C}; sum in A"]),
            Stage(32, "prg", Theme.Accent, wait: false),
        ]
    };

    static Box Stage(uint x, string name, Colors color, bool wait = true)
        => new() { X = x, Y = 5, Width = 5, Height = 3, Text = name, Color = color, WaitAfter = wait };

    static Arrow Step(uint x, string label)
        => new() { FromCol = x, FromRow = 6, ToCol = x + 4, ToRow = 6, Text = label };

    static CodeBlock Code(string caption, string[] lines)
        => new() { X = Theme.Left, Y = CodeTop, Caption = caption, Lines = lines };
}
