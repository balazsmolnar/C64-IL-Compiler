using System;
using C64Lib;
using C64Presentation.Helper;
using C64Presentation.Slides;

namespace C64Presentation;

// Runs the deck: each slide is drawn blanked, faded in, shown until
// SPACE / fire, then faded out (Helper/Deck). N and B jump to the next /
// previous slide from any point: the slide's rest is skipped (Deck), what
// is on screen fades out, and the jump is taken here.
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

            KeyBoard.WaitForAdvance();         // at once when a jump is pending
            var navigation = KeyBoard.Take();
            slide.CleanUp();
            slide = null;
            Deck.FadeOut();

            if (navigation == Navigation.Back)
            {
                if (currentSlide > 0)
                    currentSlide--;
            }
            else if (++currentSlide == slides.Length)
                break;
        }
    }
}
