using C64Lib;
using C64Presentation.Helper;

namespace C64Presentation.Slides.Part3;

// Slide 27 (DECK.md): thanks, the logos settling in, and the count-up to the deck's free
// memory, C64 boot-screen style.
sealed class ThankYouSlide : Slide
{
    public static Slide Create() => new ThankYouSlide();

    // Free memory before the heap of the final, combined deck (DECK.md slide
    // 27): $CC00 - the end of the last Data: segment - 1,080 (object tables,
    // locals stack, string buffers) - the static frames - 64 per sprite
    // frame beyond 64, from C64Presentation\measure.bat's output. 18,201 is
    // the part-3-only build: the integrator re-measures the combined deck.
    public const ulong BytesFree = 18201;

    const uint TitleRow = 7;
    const uint LeadRow = 12;
    const uint NumberRow = 14;
    const uint LinkRow = 20;
    const string Title = "Thank you";
    const string Words = " BYTES FREE";
    const ulong ScreenWidth = Screen.Columns * Theme.CellPixels;

    public override void Present()
    {
        Text.Center(0, ScreenWidth, LinkRow, "github.com/balazsmolnar/C64-IL-Compiler", Theme.Muted);
        Text.Big((ScreenWidth - Text.BigWidth(Title)) / 2, TitleRow, Title);
        Deck.Reveal();
        Logos.SettleIn();
        Deck.Step();
        Text.Center(0, ScreenWidth, LeadRow, "27 slides of C#, and still", Theme.Body);
        Deck.Step();
        Deck.Show();
        CountUp();
    }

    public override void CleanUp() => Sprites.SetVisible(false);

    // The number counts up from 0, fast and then slowing (about a second),
    // right aligned against the fixed " BYTES FREE"; the line is centred for
    // the final number (all digits are taken as wide as a zero).
    static void CountUp()
    {
        ulong width = Text.BigWidth(BytesFree < 1000 ? "000" : BytesFree < 10000 ? "0,000" : "00,000") + Text.BigWidth(Words);
        ulong left = (ScreenWidth - width) / 2;
        ulong words = left + width - Text.BigWidth(Words);
        Deck.Paint(0, NumberRow, Screen.Columns, 4, Theme.Accent, Theme.Background);
        Text.Big(words, NumberRow, Words, Theme.Accent);
        ulong top = Screen.Px(NumberRow);
        for (ulong remaining = BytesFree; ; remaining -= remaining / 6 + 1)
        {
            C64.Screen.WaitForVBlank();
            C64.Screen.DrawRectangle(left, top, words - 1, top + Screen.Px(4) - 1, true, false);
            DrawNumber(words, BytesFree - remaining);
            if (remaining == 0)
                break;
        }
    }

    // n with a thousands comma (9,876), ending at pixel `right`. Each piece
    // is drawn as soon as it is made: a number's string lives in a shared
    // buffer until the next one.
    static void DrawNumber(ulong right, ulong n)
    {
        if (n < 1000)
        {
            DrawLeftOf(right, n.ToString());
            return;
        }
        ulong x = DrawLeftOf(right, (n % 1000).ToString().PadLeft(3, '0'));
        x = DrawLeftOf(x, ",");
        DrawLeftOf(x, (n / 1000).ToString());
    }

    static ulong DrawLeftOf(ulong right, string text)
    {
        ulong x = right - Text.BigWidth(text);
        Text.Big(x, NumberRow, text, Theme.Accent);
        return x;
    }
}
// The Commodore and C# logo sprites (Helper/Sprites), sliding in from the
// sides and settling side by side above the title.
static class Logos
{
    const ulong SpriteLeft = 24;        // screen pixel 0 in sprite coordinates
    const uint SpriteTop = 50;
    const ulong Half = 24;              // a sprite's width
    const uint HalfHeight = 21;
    const ulong CommodoreX = 104;       // where they settle, in screen pixels
    const ulong CSharpX = 172;
    const uint Top = 10;
    const ulong Travel = 96;            // how far each one slides in (at most CommodoreX)

    public static void SettleIn()
    {
        Sprites.SetupLogo(false);
        for (ulong offset = Travel; ; offset -= offset / 5 + 1)
        {
            Place(CommodoreX - offset, CSharpX + offset);
            if (offset == 0)
                break;
            C64.Screen.WaitForVBlank();
            if (offset == Travel)
                Sprites.SetVisible(true);
        }
    }

    // Sprites 0-3: the Commodore C (top, bottom) and flag (top, bottom);
    // 4-6: the C# logo's C (top, bottom) and its '#'.
    static void Place(ulong commodore, ulong csharp)
    {
        ulong x = commodore + SpriteLeft;
        uint y = Top + SpriteTop;
        Set(C64.Sprites.Sprite0, x, y);
        Set(C64.Sprites.Sprite1, x, y + HalfHeight);
        Set(C64.Sprites.Sprite2, x + Half, y);
        Set(C64.Sprites.Sprite3, x + Half, y + HalfHeight);
        x = csharp + SpriteLeft;
        Set(C64.Sprites.Sprite4, x, y);
        Set(C64.Sprites.Sprite5, x, y + HalfHeight);
        Set(C64.Sprites.Sprite6, x + Half, y + HalfHeight / 2);
    }

    static void Set(Sprite sprite, ulong x, uint y)
    {
        sprite.X = x;
        sprite.Y = y;
    }
}
