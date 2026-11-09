using System;
using C64Lib;
using C64Presentation.Helper;
using C64Presentation.Slides;

namespace C64Presentation;

// Runs the deck: each slide is drawn blanked, faded in, shown until
// SPACE/fire (B = back), then faded out (Helper/Deck).
class Presenter
{
    public static void Present(Func<Slide>[] slides)
    {
        int currentSlide = SceneConfig.StartSlide; // 0 unless a scene build says otherwise
        Deck.Init((uint)slides.Length);

        while (true)
        {
            GC.Collect();
            Deck.Begin((uint)currentSlide);

            var slide = slides[currentSlide]();
            slide.Present();
            Deck.Show();
            Deck.Reveal();

            var key = KeyBoard.WaitForKeys();
            slide.CleanUp();
            slide = null;
            Deck.FadeOut();

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
