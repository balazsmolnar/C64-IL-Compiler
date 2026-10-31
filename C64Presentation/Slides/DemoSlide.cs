using C64Lib;

namespace C64Presentation.Slides;

class DemoSlide : Slide
{
    public override void Present()
    {
        Colors color = Colors.Black;

        C64.Screen.Write(15, 12, "D E M O", Colors.Black);
        for (ulong i=0; i<1000; i++)
        {
            C64.Screen.SetBackgroundColor(color);
            C64.Screen.SetBorderColor(color);
            color++;

            if (Helper.KeyBoard.AdvancePressed())
                break;

        }
    }
}