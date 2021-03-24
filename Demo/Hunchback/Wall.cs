using C64Lib;

namespace Hunchback
{
    class Wall
    {
        const uint WallChar = 62;
        public static void Draw(Colors color)
        {
            C64.FillMemory(C64Address.FromLabel("screenMemory+$01B8"), 62, 0);
            C64.FillMemory(C64Address.FromLabel("screenMemory+$02B8"), 62, 0);
            C64.FillMemory(C64Address.FromLabel("screenMemory+$0300"), 62, 0);

            C64.FillMemory(C64Address.FromLabel("colorMemory+$01B8"), (uint)color, 0);
            C64.FillMemory(C64Address.FromLabel("colorMemory+$02B8"), (uint)color, 0);
            C64.FillMemory(C64Address.FromLabel("colorMemory+$0300"), (uint)color, 0);

            C64.CopyMemory(C64Address.FromLabel("screenMemory+$0320"), C64Address.FromLabel("tbl_WallBaseChars"), 199);

            uint d = 0;
            for (uint y = 3; y < 11; y++)
            {
                for (uint x = 37; x < 40; x++)
                {
                    var b = C64.GetMemory(C64Address.FromLabel("tbl_LevelEndBellChars"), d);

                    C64.SetChar(x, y, b, Colors.Grey3);
                    d++;
                }
            }

            d = 0;
            for (uint y = 0; y < 2; y++)
            {
                for (uint x = 0; x < 40; x++)
                {
                    var b = C64.GetMemory(C64Address.FromLabel("txt_PlayerStats"), d);
                    var c = C64.GetMemory(C64Address.FromLabel("tbl_PlayerStatsColours"), d);

                    C64.SetChar(x, y, b, (Colors)c);
                    d++;
                }
            }

            BuildKnightPit(9);
            BuildKnightPit(18);
            BuildKnightPit(27);

            // for (int i = 0; i < 20; i++)
            // {
            //     KnightPitFrame1(10);
            //     Wait();
            //     KnightPitFrame2(10);
            //     Wait();
            //     KnightPitFrame3(10);
            //     Wait();
            //     KnightPitFrame2(10);
            //     Wait();
            //     KnightPitFrame1(10);
            // }
        }

        private static void Wait()
        {
            for (uint i = 0; i < 255; i++)
            {
                for (uint j = 0; j < 255; j++)
                {
                    var a = i + j;
                }
            }
        }
        private static void BuildKnightPit(uint startX)
        {
            uint d = 0;
            for (uint y = 10; y < 14; y++)
            {
                for (uint x = startX; x < startX + 4; x++)
                {
                    var b = C64.GetMemory(C64Address.FromLabel("tbl_KnightChars"), d);
                    var c = C64.GetMemory(C64Address.FromLabel("tbl_KnightCharsColours"), d);

                    C64.SetChar(x, y, b, (Colors)c);
                    d++;
                }
            }
        }

        private static void KnightPitFrame1(uint startX)
        {
            uint d = 0;
            for (uint y = 8; y < 14; y++)
            {
                for (uint x = startX; x < startX + 3; x++)
                {
                    var b = C64.GetMemory(C64Address.FromLabel("tbl_PitKnightFrame1"), d);
                    // var c = C64.GetMemory(C64Address.FromLabel("tbl_KnightCharsColours"), d);

                    C64.SetChar(x, y, b, (Colors)10);
                    d++;
                }
            }
        }

        private static void KnightPitFrame2(uint startX)
        {
            uint d = 0;
            for (uint y = 8; y < 14; y++)
            {
                for (uint x = startX; x < startX + 3; x++)
                {
                    var b = C64.GetMemory(C64Address.FromLabel("tbl_PitKnightFrame2"), d);
                    // var c = C64.GetMemory(C64Address.FromLabel("tbl_KnightCharsColours"), d);

                    C64.SetChar(x, y, b, (Colors)10);
                    d++;
                }
            }
        }

        private static void KnightPitFrame3(uint startX)
        {
            uint d = 0;
            for (uint y = 8; y < 14; y++)
            {
                for (uint x = startX; x < startX + 3; x++)
                {
                    var b = C64.GetMemory(C64Address.FromLabel("tbl_PitKnightFrame3"), d);
                    // var c = C64.GetMemory(C64Address.FromLabel("tbl_KnightCharsColours"), d);

                    C64.SetChar(x, y, b, (Colors)10);
                    d++;
                }
            }
        }

    }
}