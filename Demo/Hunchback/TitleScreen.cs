using C64Lib;

namespace Hunchback
{
    class TitleScreen
    {
        public static void Display()
        {
            Screen.Clear(Colors.White);
            C64.CopyMemory(C64Address.FromLabel("(screenMemory+$CD)"), C64Address.FromLabel("titleScreen"), 0);
            C64.CopyMemory(C64Address.FromLabel("(screenMemory+$1CD)"), C64Address.FromLabel("titleScreen+$100"), 0);
            C64.CopyMemory(C64Address.FromLabel("(screenMemory+$21B)"), C64Address.FromLabel("titleScreen+$14E"), 0);

            C64.Write(13, 1, "OCEAN SOFTWARE", Colors.Green);
            C64.Write(13, 3, "   PRESENTS   ", Colors.White);
            C64.Write(25, 21, "   BY J.STEELE", Colors.Green);
            C64.Write(7, 23, "PRESS F1 FOR INSTRUCTIONS", Colors.White);
            C64.Write(7, 24, "    OR SPACE TO START    ", Colors.White);

            for (; ; )
            {
                for (uint color = (uint)Colors.White; color < 8; color++)
                {
                    C64.FillMemory(C64Address.FromLabel("(colorMemory+$CD)"), color, 0);
                    C64.FillMemory(C64Address.FromLabel("(colorMemory+$1CD)"), color, 0);
                    C64.FillMemory(C64Address.FromLabel("(colorMemory+$21B)"), color, 0);
                    for (uint k = 0; k < 255u; k++)
                    {
                        for (uint kk = 0; kk < 10; kk++)
                        {
                            if (C64.IsKeyPressed(Keys.Space))
                                return;
                        }
                    }
                }
            }
        }
    }
}