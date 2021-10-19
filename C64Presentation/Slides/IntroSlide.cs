using C64Lib;
using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation
{
    class IntroSlide : Slide
    {
        public override void Present()
        {

            Screen.Clear(Colors.Green);
            C64.Write(17, 12, "F  O  R", Colors.Green);

            var c64Logo = new MovingLogo
            {
                SpriteTL = C64.Sprites.Sprite0,
                SpriteBL = C64.Sprites.Sprite1,
                SpriteTR = C64.Sprites.Sprite2,
                SpriteBR = C64.Sprites.Sprite3,
                MinX = 220,
                MaxX = 265,
                MinY = 50,
                MaxY = 170,
                X = 220,
                Y = 160
            };

            var csharpLogo = new MovingLogo
            {
                SpriteTL = C64.Sprites.Sprite4,
                SpriteBL = C64.Sprites.Sprite5,
                SpriteTR = C64.Sprites.Sprite6,
                SpriteBR = null,
                MinX = 30,
                MaxX = 90,
                MinY = 50,
                MaxY = 170,
                X = 50,
                Y = 50
            };

            c64Logo.Init();
            csharpLogo.Init();
            for (; ; )
            {
                Delay.Wait(100);
                if (C64.IsKeyPressed(Keys.Space))
                {
                    break;
                }

                c64Logo.Step();
                csharpLogo.Step();
            }


        }

        public  override void CleanUp()
        {
            C64.Sprites.Sprite0.Visible = false;
            C64.Sprites.Sprite1.Visible = false;
            C64.Sprites.Sprite2.Visible = false;
            C64.Sprites.Sprite3.Visible = false;
            C64.Sprites.Sprite4.Visible = false;
            C64.Sprites.Sprite5.Visible = false;
            C64.Sprites.Sprite6.Visible = false;
        }
    }
}