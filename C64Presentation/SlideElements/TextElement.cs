using C64Lib;
using C64Presentation.Helper;

namespace C64Presentation.SlideElements;

// One line of text at a cell position, in one of the faces. Strike draws a
// 1 px rule through it (an old value next to its replacement; give the
// struck text cells of its own). Color default: Body (white for headings).
//
//     new TextElement { X = 2, Y = 12, Text = "Same game: " + Theme.Hi + "-8,690 B" }
//     new TextElement { X = 2, Y = 5, Text = "-24%", Face = TextFace.Big, Color = Theme.Accent }
class TextElement : SlideElement
{
    public required string Text { get; init; }
    public uint X { get; init; }
    public uint Y { get; init; }
    public Colors Color { get; init; }
    public TextFace Face { get; init; }
    public bool Strike { get; init; }
    public bool Wait { get; init; }

    public override void Draw()
    {
        if (Wait)
            Deck.Step();
        ulong x = Helper.Screen.Px(X);
        ulong end = Face switch
        {
            TextFace.Heading => Helper.Text.Heading(x, Y, Text, Theme.Or(Color, Theme.Heading)),
            TextFace.Big => Helper.Text.Big(x, Y, Text, Theme.Or(Color, Theme.Heading)),
            _ => Helper.Text.Draw(x, Y, Text, Theme.Or(Color, Theme.Body)),
        };
        if (Strike)
        {
            ulong y = Helper.Screen.Px(Y) + 4;
            Helper.Screen.Line(x, y, end - 2, y);
        }
    }
}

enum TextFace { Body, Heading, Big }
