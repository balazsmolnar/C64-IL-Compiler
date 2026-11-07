using C64Lib;
using C64Presentation.Helper;

namespace C64Presentation;

// The cover: the C# and Commodore logos side by side above the big title,
// gently bobbing together until the press.
class IntroSlide : Slide
{
    const ulong CSharpX = 128;     // sprite coordinates: bitmap x + 24
    const ulong CommodoreX = 196;
    const uint LogoTop = 70;       // bitmap y + 50: rows 2-7
    const uint SpriteHeight = 21;
    const uint SpriteWidth = 24;
    const uint Bob = 3;            // pixels up and down
    const uint FramesPerPixel = 6;

    public override void Present()
    {
        Master.Cover("C# on the C64", "Compiling .NET IL to 6502");
        Sprites.SetupLogo(false);
        Sprites.SetVisible(true);
        Place(LogoTop);
        Deck.Reveal();

        uint offset = 0, frame = 0;
        bool down = true;
        while (!KeyBoard.AdvancePressed())
        {
            C64.Screen.WaitForVBlank();
            if (++frame < FramesPerPixel)
                continue;
            frame = 0;
            offset = down ? offset + 1 : offset - 1;
            if (offset == 0 || offset == Bob)
                down = !down;
            Place(LogoTop + offset);
        }
    }

    // C# = sprites 4-6 (C halves, '#'), Commodore = sprites 0-3, one baseline.
    static void Place(uint y)
    {
        C64.Sprites.Sprite4.X = CSharpX;
        C64.Sprites.Sprite4.Y = y;
        C64.Sprites.Sprite5.X = CSharpX;
        C64.Sprites.Sprite5.Y = y + SpriteHeight;
        C64.Sprites.Sprite6.X = CSharpX + SpriteWidth;
        C64.Sprites.Sprite6.Y = y;
        C64.Sprites.Sprite0.X = CommodoreX;
        C64.Sprites.Sprite0.Y = y;
        C64.Sprites.Sprite1.X = CommodoreX;
        C64.Sprites.Sprite1.Y = y + SpriteHeight;
        C64.Sprites.Sprite2.X = CommodoreX + SpriteWidth;
        C64.Sprites.Sprite2.Y = y;
        C64.Sprites.Sprite3.X = CommodoreX + SpriteWidth;
        C64.Sprites.Sprite3.Y = y + SpriteHeight;
    }

    public override void CleanUp() => Sprites.SetVisible(false);
}
