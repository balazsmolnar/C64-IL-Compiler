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
    // frame beyond 64, from C64Presentation\measure.bat's output. 13,371 is
    // the part-3-only build: the integrator re-measures the combined deck.
    public const ulong BytesFree = 13371;

    const uint TitleRow = 7;
    const uint LeadRow = 12;
    const uint NumberRow = 14;
    const uint LinkRow = 20;
    const string Title = "Thank you";
    const string Words = "BYTES FREE";
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

    // Count-up layout, in whole cells so that each piece owns its cells: a
    // 3-cell slot per digit (a 32 px digit is 20 px wide), a 1-cell comma
    // after the thousands, a 2-cell gap and "BYTES FREE"; centred for the final number.
    const uint SlotCells = 3;
    const uint NumberRows = 4;
    const uint WordGap = 2;             // cells between the number and the words
    const uint Blank = 10;              // a leading slot not reached yet
    static readonly string[] Digits = ["0", "1", "2", "3", "4", "5", "6", "7", "8", "9"];

    // From 0 to BytesFree in about 15 steps, one frame apart (about a
    // second), fast and then slowing. A step redraws only the digits that
    // changed, each with its cells hidden while it is cleared and drawn, so
    // no digit is ever seen half drawn.
    static void CountUp()
    {
        uint count = DigitCount(BytesFree);
        uint comma = count > 3 ? 1u : 0u;
        uint digitCells = count * SlotCells + comma;
        uint wordCells = (uint)((Text.BigWidth(Words) + Theme.CellPixels - 1) / Theme.CellPixels);
        uint spare = Screen.Columns - digitCells - wordCells;
        uint gap = spare < WordGap ? spare : WordGap;           // narrower only for 5 digits
        uint cells = digitCells + gap + wordCells;
        uint left = (Screen.Columns - cells) / 2;
        uint wordsColumn = left + digitCells + gap;
        Hide(left, cells);
        Text.Big(Screen.Px(wordsColumn), NumberRow, Words, Theme.Background, Theme.Background);
        Show(wordsColumn, cells - (wordsColumn - left));
        uint[] shown = [Blank, Blank, Blank, Blank, Blank];   // ulong is 16 bits: 5 digits at most
        for (ulong remaining = BytesFree; ; remaining -= remaining / 2 + 1)
        {
            ulong value = BytesFree - remaining;
            if (comma != 0 && value >= 1000 && shown[count - 4] == Blank)
                DrawSlot(left + (count - 3) * SlotCells, 1, ",");
            for (uint i = 0; i < count; i++)     // from the right
            {
                uint slot = count - 1 - i;
                uint digit = i != 0 && value == 0 ? Blank : (uint)(value % 10);
                value /= 10;
                if (digit == shown[slot])
                    continue;
                shown[slot] = digit;
                uint column = left + slot * SlotCells + (slot >= count - 3 ? comma : 0);
                DrawSlot(column, SlotCells, digit == Blank ? "" : Digits[digit]);
            }
            if (remaining == 0 || KeyBoard.Jumping)
                break;
            Deck.Wait(1);
        }
    }

    static uint DigitCount(ulong n)
    {
        uint count = 1;
        for (; n >= 10; n /= 10)
            count++;
        return count;
    }

    // One slot: hidden, cleared, its glyph centred in it, shown again.
    static void DrawSlot(uint column, uint width, string glyph)
    {
        Hide(column, width);
        Screen.ClearRect(column, NumberRow, width, NumberRows);
        ulong x = Screen.Px(column), space = Screen.Px(width), glyphWidth = Text.BigWidth(glyph);
        Text.Big(glyphWidth < space ? x + (space - glyphWidth) / 2 : x, NumberRow, glyph, Theme.Background, Theme.Background);
        Show(column, width);
    }

    static void Hide(uint column, uint width) => Deck.Paint(column, NumberRow, width, NumberRows, Theme.Background, Theme.Background);

    static void Show(uint column, uint width) => Deck.Paint(column, NumberRow, width, NumberRows, Theme.Accent, Theme.Background);
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
            if (offset == 0 || KeyBoard.Jumping)
                break;
            Deck.Wait(1);
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
