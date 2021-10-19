using C64Lib;
using C64Presentation.Helper;

namespace C64Presentation.SlideElements
{
    class MovingLogo
    {
        const int SPRITE_WIDTH = 48;
        const int SPRITE_HEIGHT = 42;

        public ulong MinX;
        public uint MinY;

        public ulong MaxX;
        public uint MaxY;

        public ulong X;
        public uint Y;
        public const ulong VX = 2;
        public const uint VY = 1;

        public Sprite SpriteTL;
        public Sprite SpriteTR;
        public Sprite SpriteBL;
        public Sprite SpriteBR;

        private bool left;
        private bool up;
        private uint counter;

        private ulong delay1X;
        private bool delay1Inc;
        private ulong delay2X;
        private bool delay2Inc;

        public void Init()
        {
            delay1X = 10;
            delay1Inc = true;

            delay2X = 10;
            delay2Inc = false;

            Sprites.SetupLogo(true);
            Sprites.SetVisible(true);
        }
        public void Step()
        {
            X = left ? X + VX : X - VX;
            Y = up ? Y + VY : Y - VY;

            if (X >= MaxX || X <MinX)
                left = !left ;

            if (Y >= MaxY || Y < MinY)
                up = !up;

            counter++;
            if (counter == 10)
            {
                counter = 0;
                delay1X = delay1Inc ? delay1X + 1 : delay1X - 1;
                if (delay1X >= 15 || delay1X < 5)
                    delay1Inc = !delay1Inc;

                delay2X = delay2Inc ? delay2X + 1 : delay2X - 1;
                if (delay2X >= 15 || delay2X < 5)
                    delay2Inc = !delay2Inc;

            }
            SpriteTL.X = X;
            SpriteTL.Y = Y;
            SpriteBL.X = X;
            SpriteBL.Y = Y+SPRITE_HEIGHT;

            SpriteTR.X = X + SPRITE_WIDTH + delay1X - 5;
            SpriteTR.Y = Y;

            if (SpriteBR != null)
            {
                SpriteBR.X = X + SPRITE_WIDTH + delay2X - 5;
                SpriteBR.Y = Y + SPRITE_HEIGHT;
            }

        }
    }
}
