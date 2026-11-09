using C64Lib;
using C64Presentation.Helper;

namespace C64Presentation.SlideElements;

// A 1 px rule: across Length cells through the middle of row Y, or, when
// Vertical, down Length cells along the left edge of column X (the middle of
// a two-cell gutter). Color: Auto = Theme.Frame, the panel-frame grey.
//
//     new Rule { X = 3, Y = 11, Length = 34 }                         // inside a code panel
//     new Rule { X = 20, Y = 5, Length = 11, Vertical = true }        // between two columns
sealed class Rule : SlideElement
{
    public uint X { get; init; }
    public uint Y { get; init; }
    public uint Length { get; init; }
    public bool Vertical { get; init; }
    public Colors Color { get; init; }
    public bool Wait { get; init; }

    public override void Draw()
    {
        if (Wait)
            Deck.Step();
        var color = Theme.Or(Color, Theme.Frame);
        ulong x = Screen.Px(X), y = Screen.Px(Y);
        if (Vertical)
        {
            Deck.Paint(X, Y, 1, Length, color, Theme.Background);
            Screen.Line(x, y, x, y + Screen.Px(Length) - 1);
        }
        else
        {
            Deck.Paint(X, Y, Length, 1, color, Theme.Background);
            Screen.Line(x, y + 3, x + Screen.Px(Length) - 1, y + 3);
        }
    }
}
