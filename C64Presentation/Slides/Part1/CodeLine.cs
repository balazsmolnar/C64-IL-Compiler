using C64Presentation.Helper;

namespace C64Presentation.SlideElements;

// One line of code in the code face, at a cell position: to add lines to a
// CodeBlock panel already on the slide, or a column next to its code. Wait
// makes it a build step; lines that follow without Wait join that step.
//
//     new CodeLine { Column = 3, Row = 12, Text = $"{Theme.Kw}foreach {Theme.Ty}(...)", Wait = true }
sealed class CodeLine : SlideElement
{
    public required string Text { get; init; }
    public uint Column { get; init; }
    public uint Row { get; init; }
    public bool Wait { get; init; }

    public override void Draw()
    {
        if (Wait)
            Deck.Step();
        Helper.Text.Code(Column, Row, Text);
    }
}
