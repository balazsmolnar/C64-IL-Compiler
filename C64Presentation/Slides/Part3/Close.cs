using C64Lib;
using C64Presentation.Helper;

[assembly: RawAssembly(Resource = "big_digits.asm")]   // art/digits/make_digits.py: 960 bytes

namespace C64Presentation.Slides.Part3;

// Slide 16 (DECK.md): thanks, the logos settling in, and the count-up to
// the deck's free memory, C64 boot-screen style ("38911 BASIC BYTES FREE":
// no thousands comma).
sealed class ThankYouSlide : Slide
{
    public static Slide Create() => new ThankYouSlide();

    // Free memory before the heap of the final, combined deck (DECK.md slide
    // 16), the same quantity as slide 8's Catacombs figure (Catacombs/
    // TECH.md "Free memory"): $CC00 - the heap's start, i.e. the program's
    // end (prg\presentation.labels: Sprite_InitData, where the sprite frames
    // moved under I/O at startup were) + 56 (tables and string buffers,
    // no DrawText) + the static frames. Measured with the passes slide (and
    // without "Borrow, not build"): the program ends at $C4A1, 56 + 85
    // bytes on, the heap starts at $C52E. The heap peaks at 530 bytes
    // (slide 15; heapPointer at every KeyBoard.WaitForAdvance, in VICE).
    public const ulong BytesFree = 1746;

    const uint TitleRow = 7;
    const uint LeadRow = 12;
    const uint NumberRow = 14;
    const uint NumberRows = 4;              // the 32 px face
    const uint LinkRow = 21;            // clear of the words' fallback row (18-19)
    const string Title = "Thank you";
    const string Words = "BYTES FREE";
    const ulong ScreenWidth = Screen.Columns * Theme.CellPixels;

    // The number: one 3-cell slot per digit (a 32 px digit is 20 px wide),
    // so each digit owns its cells, then up to WordGap cells, then the words.
    const uint SlotCells = 3;
    const int WordGap = 2;
    const uint Blank = 10;                  // a leading slot not reached yet
    const uint MaxDigits = 5;               // ulong is 16 bits: 65535
    const uint TilesPerDigit = SlotCells * NumberRows;
    const uint FramesPerStep = 3;

    uint _count;                            // digits in BytesFree
    uint _left;                             // the first slot's column
    readonly uint[] _shown = new uint[MaxDigits];
    readonly uint[] _target = new uint[MaxDigits];

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
        LayOut();
        CountUp();
    }

    public override void CleanUp() => Sprites.SetVisible(false);

    // Centres digits, gap and words for the final number and draws the
    // words; if they could not share the row, the words go under it.
    void LayOut()
    {
        _count = DigitCount(BytesFree);
        int digitCells = (int)(_count * SlotCells);
        int wordCells = (int)((Text.BigWidth(Words) + Theme.CellPixels - 1) / Theme.CellPixels);
        int spare = (int)Screen.Columns - digitCells - wordCells;
        for (uint i = 0; i < MaxDigits; i++)
            _shown[i] = Blank;
        Deck.Paint(0, NumberRow, Screen.Columns, NumberRows, Theme.Background, Theme.Background);   // the words appear whole
        if (spare < WordGap)
        {
            _left = (uint)(((int)Screen.Columns - digitCells) / 2);
            Text.CenterHeading(0, ScreenWidth, NumberRow + NumberRows, Words, Theme.Accent);
            Deck.Paint(0, NumberRow, Screen.Columns, NumberRows, Theme.Accent, Theme.Background);
            return;
        }
        _left = (uint)(spare - WordGap) / 2;
        Text.Big(Screen.Px(_left + (uint)(digitCells + WordGap)), NumberRow, Words, Theme.Background, Theme.Background);
        Deck.Paint(0, NumberRow, Screen.Columns, NumberRows, Theme.Accent, Theme.Background);
    }

    // From 0 to BytesFree in about 15 steps, fast and then slowing.
    void CountUp()
    {
        for (ulong remaining = BytesFree; ; remaining -= remaining / 2 + 1)
        {
            DigitsOf(BytesFree - remaining);
            DrawChanged();
            if (remaining == 0 || KeyBoard.Jumping)
                return;
            Deck.Wait(FramesPerStep);
        }
    }
    static uint DigitCount(ulong n)
    {
        uint count = 1;
        for (; n >= 10; n /= 10)
            count++;
        return count;
    }

    // The value's digits into _target, right aligned; leading slots Blank.
    void DigitsOf(ulong value)
    {
        for (uint slot = _count; slot-- > 0;)
        {
            bool leading = value == 0 && slot != _count - 1;
            _target[slot] = leading ? Blank : (uint)(value % 10);
            value /= 10;
        }
    }

    // Redraws the slots whose digit changed. A digit is a block of ready
    // tiles (art/digits/make_digits.py): copying it replaces the old one
    // in place, never half drawn (DrawFontText at 2x takes most of a frame).
    void DrawChanged()
    {
        for (uint slot = 0; slot < _count; slot++)
        {
            uint digit = _target[slot];
            if (digit == _shown[slot])
                continue;
            C64.Screen.DrawTiles(_left + slot * SlotCells, NumberRow, SlotCells, NumberRows, C64Address.FromLabel("tiles_BigDigits"), digit * TilesPerDigit);
            _shown[slot] = digit;
        }
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
