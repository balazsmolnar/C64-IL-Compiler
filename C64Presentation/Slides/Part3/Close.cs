using C64Lib;
using C64Presentation.Helper;

namespace C64Presentation.Slides.Part3;

// Slide 16 (DECK.md): thanks, the logos settling in, and the count-up to the deck's free
// memory, C64 boot-screen style.
sealed class ThankYouSlide : Slide
{
    public static Slide Create() => new ThankYouSlide();

    // Free memory before the heap of the final, combined deck (DECK.md slide
    // 16): $CC00 - the end of the last Data: segment - 1,080 (object tables,
    // locals stack, string buffers) - the static frames - 64 per sprite
    // frame beyond 64, from C64Presentation\measure.bat's output. 16,066 is
    // the part-3-only build: the integrator re-measures the combined deck.
    public const ulong BytesFree = 16066;

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
        Text.Center(0, ScreenWidth, LeadRow, "16 slides of C#, and still", Theme.Body);
        Deck.Step();
        Deck.Show();
        CountUp();
    }

    public override void CleanUp() => Sprites.SetVisible(false);

    // Count-up layout: one fixed slot per digit, as wide as a zero, and a
    // comma after the thousands; the line is centred for the final number.
    const uint Blank = 10;              // a leading slot not reached yet
    const uint FramesPerStep = 2;
    static readonly string[] Digits = ["0", "1", "2", "3", "4", "5", "6", "7", "8", "9"];

    // From 0 to BytesFree in about 25 steps of two frames (about a second),
    // fast and then slowing. Each step redraws only the digits that changed,
    // one glyph each, so the number is never seen half drawn.
    static void CountUp()
    {
        uint count = DigitCount(BytesFree);
        ulong slot = Text.BigWidth("0"), comma = Text.BigWidth(",");
        ulong width = count * slot + (count > 3 ? comma : 0) + Text.BigWidth(Words);
        ulong left = (ScreenWidth - width) / 2;
        ulong commaX = left + (count - 3) * slot;
        Deck.Paint(0, NumberRow, Screen.Columns, 4, Theme.Accent, Theme.Background);
        Text.Big(left + width - Text.BigWidth(Words), NumberRow, Words, Theme.Accent);
        uint[] shown = [Blank, Blank, Blank, Blank, Blank];   // ulong is 16 bits: 5 digits at most
        for (ulong remaining = BytesFree; ; remaining -= remaining / 3 + 1)
        {
            ulong value = BytesFree - remaining;
            if (count > 3 && value >= 1000 && shown[count - 4] == Blank)
                Text.Big(commaX, NumberRow, ",", Theme.Accent);
            for (uint i = 0; i < count; i++)     // from the right
            {
                uint slotIndex = count - 1 - i;
                uint digit = i != 0 && value == 0 ? Blank : (uint)(value % 10);
                value /= 10;
                if (digit != shown[slotIndex])
                {
                    ulong x = left + slotIndex * slot + (count > 3 && slotIndex >= count - 3 ? comma : 0);
                    DrawDigit(x, slot, digit);
                    shown[slotIndex] = digit;
                }
            }
            if (remaining == 0)
                break;
            for (uint frame = 0; frame < FramesPerStep; frame++)
                KeyBoard.Frame();
        }
    }

    static uint DigitCount(ulong n)
    {
        uint count = 1;
        for (; n >= 10; n /= 10)
            count++;
        return count;
    }

    // One slot: cleared, then its digit centred in it (Blank: left empty).
    static void DrawDigit(ulong x, ulong slot, uint digit)
    {
        ulong top = Screen.Px(NumberRow);
        C64.Screen.DrawRectangle(x, top, x + slot - 1, top + Screen.Px(4) - 1, true, false);
        if (digit == Blank)
            return;
        string glyph = Digits[digit];
        Text.Big(x + (slot - Text.BigWidth(glyph)) / 2, NumberRow, glyph, Theme.Accent);
    }
}
// The C# and Commodore logo sprites (Helper/Sprites), sliding in from the
// sides and settling side by side above the title, C# on the left.
static class Logos
{
    const ulong SpriteLeft = 24;        // screen pixel 0 in sprite coordinates
    const uint SpriteTop = 50;
    const ulong Half = 24;              // a sprite's width
    const uint HalfHeight = 21;
    const ulong CSharpX = 104;          // where they settle, in screen pixels:
    const ulong CommodoreX = 172;       // C# left, as on the cover
    const uint Top = 10;
    const ulong Travel = 96;            // how far each one slides in (at most CSharpX)

    public static void SettleIn()
    {
        Sprites.SetupLogo(false);
        for (ulong offset = Travel; ; offset -= offset / 5 + 1)
        {
            Place(CommodoreX + offset, CSharpX - offset);
            if (offset == 0)
                break;
            KeyBoard.Frame();
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
