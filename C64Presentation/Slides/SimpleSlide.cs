using C64Lib;
using C64Presentation.Animations;
using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides;

class SimpleSlide : Slide
{
    public SlideElement[] Elements;
    public Animation Animation;



    public override void Present()
    {
        SetLogo();
        foreach (var slideElement in Elements)
        {
            slideElement.Draw();
        }
    }

    public override void CleanUp()
    {
        Sprites.SetVisible(false);
        Animation?.Animate();
    }

    private void SetLogo()
    {
        Sprites.SetupLogo(false);
        Sprites.SetVisible(true);

        ulong x = 30;
        uint y = 50;
        C64.Sprites.Sprite0.X = x;
        C64.Sprites.Sprite0.Y = y;
        C64.Sprites.Sprite1.X = x;
        C64.Sprites.Sprite1.Y = y + 21;
        C64.Sprites.Sprite2.X = x + 24;
        C64.Sprites.Sprite2.Y = y;
        C64.Sprites.Sprite3.X = x + 24;
        C64.Sprites.Sprite3.Y = y + 21;

        ulong x2 = 280;
        C64.Sprites.Sprite4.X = x2;
        C64.Sprites.Sprite4.Y = y;
        C64.Sprites.Sprite5.X = x2;
        C64.Sprites.Sprite5.Y = y + 21;
        C64.Sprites.Sprite6.X = x2 + 24;
        C64.Sprites.Sprite6.Y = y;

    }
}