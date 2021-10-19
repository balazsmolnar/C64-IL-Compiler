using System;
using C64Lib;
using C64Presentation.Helper;

namespace C64Presentation
{
    class Presenter
    {
        public static void Present(Func<Slide>[] slides)
        {
            int currentSlide = 0;

            while (true)
            {
                Screen.Clear();
                GC.Collect();

                C64.SetBackgroundColor(Colors.Black);
                C64.SetBorderColor(Colors.Black);

                slides[currentSlide]().Present();

                var key = KeyBoard.WaitForKeys();
                slides[currentSlide]().CleanUp();

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
}