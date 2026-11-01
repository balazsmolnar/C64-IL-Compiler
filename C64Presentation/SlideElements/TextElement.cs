using C64Lib;
using C64Presentation.Helper;

namespace C64Presentation.SlideElements;

// A line of text at a cell position: body face (default colour Body), or
// the 16 px title face with Heading = true (default colour white).
class TextElement : SlideElement
{
    public string Text;
    public uint X, Y;
    public Colors Color;
    public bool Heading;
    public bool Wait;

    public override void Draw()
    {
        if (Wait)
            Deck.Step();
        ulong x = Helper.Screen.X(X);
        if (Heading)
            Helper.Text.Heading(x, Y, Text, Theme.Or(Color, Theme.Heading));
        else
            Helper.Text.Draw(x, Y, Text, Theme.Or(Color, Theme.Body));
    }
}
