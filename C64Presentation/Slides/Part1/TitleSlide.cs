using C64Lib;
using C64Presentation.Helper;

// Slide 1's typewriter: a black mask and a cursor (art/slides/make_caret.py).
[assembly: RawSprites(Resource = "caret.spr", Label = "Caret")]

namespace C64Presentation.Slides;

// Slide 1: the title picture; on the first press the caption types itself
// out under the code bubble's tail, like a C64 PRINT, and the cursor
// blinks after it until the next press.
//
// The caption is drawn once, in the code face (fixed 6 px pitch), under two
// black x-expanded sprites per line; typing moves the masks and the cursor
// one character to the right at a time, so nothing is redrawn.
sealed class TitleSlide : Slide
{
    const string Line1 = "This slide is";
    const string Line2 = "a C# program.";
    const uint Column = 20;              // under the bubble's tail (title.img)
    const uint Row = 20;
    const ulong Pitch = 6;               // the code face
    const ulong MaskWidth = 48;          // an x-expanded sprite
    const uint FramesPerKey = 3;
    const uint FramesPerLineBreak = 15;
    const uint BlinkFrames = 20;

    // Sprite registers: bitmap pixel + this offset (Catacombs/TECH.md).
    const ulong SpriteLeft = 24;
    const uint SpriteTop = 50;

    // Sprite 0 is the cursor (the lowest number is drawn in front), 1-4 the
    // masks: two per line.
    const uint CursorSprite = 0;
    const uint FirstMask = 1, LastMask = 4;
    const uint AllSprites = 0x1F;
    const uint CursorOnly = 0x01;

    public static Slide Create() => new TitleSlide();

    static uint MaskFrame => (uint)C64Address.FromLabel("frm_Caret");
    static uint CursorFrame => MaskFrame + 1;

    public override void Present()
    {
        Master.Cover();

        Deck.Step();
        Deck.Show();                     // what follows is drawn live
        ulong left = Screen.Px(Column);
        Mask(0, left);
        Mask(1, left);
        for (uint sprite = FirstMask; sprite <= LastMask; sprite++)
            C64.Sprites[sprite].ExpandX = true;
        C64.Sprites.VisibleMask = AllSprites;
        Text.Code(Column, Row, Line1, Theme.Body);
        Text.Code(Column, Row + 1, Line2, Theme.Heading);

        Type(0, Line1);
        WaitFrames(FramesPerLineBreak);
        Type(1, Line2);
        C64.Sprites.VisibleMask = CursorOnly;

        // The cursor blinks until the presenter moves on.
        uint frames = 0;
        bool lit = true;
        while (!KeyBoard.AdvancePressed())
        {
            C64.Screen.WaitForVBlank();
            if (++frames < BlinkFrames)
                continue;
            frames = 0;
            lit = !lit;
            C64.Sprites.VisibleMask = lit ? CursorOnly : 0u;
        }
    }

    public override void CleanUp()
    {
        C64.Sprites.VisibleMask = 0;
        for (uint sprite = FirstMask; sprite <= LastMask; sprite++)
            C64.Sprites[sprite].ExpandX = false;
    }

    // Types one line: the cursor and the line's masks step right one
    // character at a time.
    static void Type(uint line, string text)
    {
        ulong x = Screen.Px(Column);
        for (uint i = 0; i <= (uint)text.Length; i++)
        {
            Mask(line, x);
            C64.Sprites[CursorSprite].Place(SpriteLeft + x, SpriteTop + (uint)Screen.Px(Row + line), CursorFrame, Theme.Accent);
            WaitFrames(FramesPerKey);
            x += Pitch;
        }
    }

    // The two masks of a line, from pixel x on.
    static void Mask(uint line, ulong x)
    {
        uint y = SpriteTop + (uint)Screen.Px(Row + line);
        uint sprite = FirstMask + 2 * line;
        C64.Sprites[sprite].Place(SpriteLeft + x, y, MaskFrame, Theme.Background);
        C64.Sprites[sprite + 1].Place(SpriteLeft + x + MaskWidth, y, MaskFrame, Theme.Background);
    }

    static void WaitFrames(uint frames)
    {
        for (uint i = 0; i < frames; i++)
            C64.Screen.WaitForVBlank();
    }
}
