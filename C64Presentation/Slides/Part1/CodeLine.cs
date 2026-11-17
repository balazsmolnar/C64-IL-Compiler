using C64Lib;
using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides;

// One line of code in the code face, at a cell position: to add lines to a
// CodeBlock panel already on the slide, or a column next to its code. Color
// is the line's first colour (Auto = white): a build step fades it in
// through that colour, so give it rather than a leading inline escape. Wait
// makes it a build step; lines that follow without Wait join that step.
//
//     new CodeLine { Column = 3, Row = 12, Text = "ldarg.0", Color = Theme.Brand, Wait = true }
sealed class CodeLine : SlideElement
{
    public required string Text { get; init; }
    public uint Column { get; init; }
    public uint Row { get; init; }
    public Colors Color { get; init; }
    public bool Wait { get; init; }

    public override void Draw()
    {
        if (Wait)
            Deck.Step();
        Helper.Text.Code(Column, Row, Text, Theme.Or(Color, Theme.Heading));
    }
}
