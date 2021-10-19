using C64Lib;

namespace C64Presentation.Slides
{
    class DemoSlide : Slide
    {
        public override void Present()
        {
            Colors color = Colors.Black;

            C64.Write(15, 12, "D E M O", Colors.Black);
            while (true)
            {
                C64.SetBackgroundColor(color);
                C64.SetBorderColor(color);
                color++;
                if (C64.IsKeyPressed(Keys.Space))
                    break;
                
            }
        }
    }
}
