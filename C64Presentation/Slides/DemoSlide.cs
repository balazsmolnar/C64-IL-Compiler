using C64Lib;

namespace C64Presentation.Slides;

class DemoSlide : Slide
{
    public override void Present()
    {
        Colors color = Colors.Black;

        C64.Write(15, 12, "D E M O", Colors.Black);
        for (ulong i=0; i<1000; i++)
        {
            C64.SetBackgroundColor(color);
            C64.SetBorderColor(color);
            color++;

            if (C64.IsKeyPressed(Keys.Space) || (C64.Joysticks.Joystick2.Pressed & JoystickButtons.Fire) == JoystickButtons.Fire)
                break;

        }
    }
}