using C64Lib;
using C64Presentation.Helper;

namespace C64Presentation.SlideElements;

// Code on a card (Theme.Code): the fixed-pitch face, one character per cell (at most
// Width - 2 columns), with syntax colours written inline (Theme.Kw / Ty /
// Str / Num / Com / Pl; at a fixed pitch they cost no column):
//
//     Theme.Kw + "static " + Theme.Ty + "int" + Theme.Pl + " Add(...)"
//
// Caption: a label on the first row, like an editor tab ("Program.cs",
// "IL", "6502"); the code then starts a row lower. Drawing it again
// replaces the code: the old lines vanish and the new ones are typed in,
// the card stays. Color: the default text colour (Theme.Body).
class CodeBlock : SlideElement
{
    public uint X, Y, Width, Height;
    public string[] Lines;
    public string Caption;
    public Colors Color;
    public bool Wait;

    public override void Draw()
    {
        if (Wait)
            Deck.Step();
        Deck.Show();                // code keeps its own colours: drawn live
        Box.DrawCard(X, Y, Width, Height, Theme.Accent, Theme.Code);
        Helper.Screen.ClearRect(X, Y + 1, Width, Height - 1);
        uint row = Y + 1;
        if (Caption != null)
        {
            Text.Draw(Helper.Screen.X(X + 1), row, Caption, Theme.Muted, Theme.Code);
            row++;
        }
        Colors color = Theme.Or(Color, Theme.Body);
        foreach (var line in Lines)
        {
            Text.Code(X + 1, row, line, color);
            row++;
        }
    }
}
