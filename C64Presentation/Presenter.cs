using System;
using C64Lib;
using C64Presentation.Helper;
using Screen = C64Presentation.Helper.Screen;

namespace C64Presentation;

class Presenter
{
    public static void Present(Func<Slide>[] slides)
    {
        int currentSlide = 0;

        while (true)
        {
            Screen.Clear();
            GC.Collect();

            C64.Screen.SetBackgroundColor(Colors.Black);
            C64.Screen.SetBorderColor(Colors.Black);

            var slide = slides[currentSlide]();
            slide.Present();

            // Drawn last (on top of whatever the slide itself rendered),
            // bottom-right corner -- no slide currently draws there (they
            // use sprites, not screen text, for anything near the very
            // bottom), and it's small enough (at most "48/48") to stay
            // clear of any slide's own text in that row.
            string counter = $"{currentSlide + 1}/{slides.Length}";
            C64.Screen.Write((uint)(Screen.Width - counter.Length), (uint)(Screen.Height - 1), counter, Colors.Grey2);

            var key = KeyBoard.WaitForKeys();
            slide.CleanUp();
            slide = null;

            if (key == Keys.B)
            {
                currentSlide--;
            }
            else
            {
                currentSlide++;
            }

            if (currentSlide == slides.Length)
                break;

            if (currentSlide < 0)
                currentSlide = 0;

        }
    }
}