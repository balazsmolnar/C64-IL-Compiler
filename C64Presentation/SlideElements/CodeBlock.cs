using C64Lib;
using C64Presentation.Helper;

namespace C64Presentation.SlideElements;

// Code on a Grey1 card: one cell of padding on every side, an optional
// Caption on the first row (Grey3, like a tab label) with an empty row
// under it, then the lines in the code face, one character per cell and
// at most Width - 2 columns, syntax colours inline (Theme.Kw / Ty / Str /
// Num / Com / Pl; plain text is white). Height = lines + 2 (+ 2 with a
// caption). Drawing a CodeBlock where one is already shown replaces it:
// the old code fades out, the new one fades in, the card stays.
//
//     new CodeBlock { X = 2, Y = 9, Width = 36, Caption = "IL",
//                     Lines = [$"{Theme.Kw}ldarg.0", $"{Theme.Kw}add"] }
class CodeBlock : SlideElement
{
    public uint X { get; init; }
    public uint Y { get; init; }
    public uint Width { get; init; }
    public string[] Lines { get; init; }
    public string Caption { get; init; }
    public bool Wait { get; init; }

    public override void Draw()
    {
        if (Wait)
            Deck.Step();
        uint height = (uint)Lines.Length + (Caption != null ? 4u : 2u);
        uint width = Theme.OrCells(Width, Theme.Width);
        bool replacing = Deck.Live;
        if (replacing)  // the old code goes out first
            Deck.Dim(X, Y + 1, width, height - 1, Theme.Heading, Theme.Card);
        Box.DrawCard(X, Y, width, height, Theme.Accent, Theme.Card);
        if (replacing)  // and the new code is drawn hidden, to fade in
        {
            Helper.Screen.ClearRect(X, Y + 1, width, height - 1);
            Deck.Build(true);
        }
        uint row = Y + 1;
        if (Caption != null)
        {
            Text.Draw(Helper.Screen.X(X + 1), row, Caption, Theme.Body, Theme.Card);
            row += 2;
        }
        foreach (var line in Lines)
            Text.Code(X + 1, row++, line);
    }
}
