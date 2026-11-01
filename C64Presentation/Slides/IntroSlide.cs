using C64Lib;
using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation;

// The cover: title, subtitle, author; the C# logo drifts in the left
// margin and the Commodore logo in the right one until the press.
class IntroSlide : Slide
{
    public override void Present()
    {
        Master.Cover("C# on the C64", "Compiling .NET IL to 6502");

        var csharpLogo = new MovingLogo
        {
            SpriteTL = C64.Sprites.Sprite4,
            SpriteBL = C64.Sprites.Sprite5,
            SpriteTR = C64.Sprites.Sprite6,
            MinX = 30, MaxX = 46, MinY = 70, MaxY = 170,
            X = 34, Y = 90
        };
        var c64Logo = new MovingLogo
        {
            SpriteTL = C64.Sprites.Sprite0,
            SpriteBL = C64.Sprites.Sprite1,
            SpriteTR = C64.Sprites.Sprite2,
            SpriteBR = C64.Sprites.Sprite3,
            MinX = 266, MaxX = 282, MinY = 70, MaxY = 170,
            X = 278, Y = 150
        };
        csharpLogo.Init();
        c64Logo.Init();

        Deck.Reveal();
        while (!KeyBoard.AdvancePressed())
        {
            C64.Screen.WaitForVBlank();
            C64.Screen.WaitForVBlank();
            csharpLogo.Step();
            c64Logo.Step();
        }
    }

    public override void CleanUp() => Sprites.SetVisible(false);
}
