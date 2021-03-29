using C64Lib;

namespace Hunchback
{
    class Wall
    {
        const uint WallChar = 62;
        const uint Space = 32;
        const uint Wall3DChar = 60;
        const uint Wall3DTopChar = 59;
        const uint FireChar = 114;

        private WallType wallType_;
        private uint frameCounter_;

        private uint knightCounter_;
        private uint speedCounter_;
        public void Draw(Colors color, WallType wallType)
        {
            BuildBasicWall(color);
            wallType_ = wallType;
            if (wallType == WallType.KnightPits)
            {
                BuildKnightPit(9);
                BuildKnightPit(18);
                BuildKnightPit(27);
            }

            if (wallType == WallType.EmptyPits)
            {
                BuildEmptyPit(9);
                BuildEmptyPit(18);
                BuildEmptyPit(27);
            }

            if (wallType == WallType.Rope)
            {
                BuildRopePit();
            }
        }

        private static void BuildBasicWall(Colors color)
        {
            C64.FillMemory(C64Address.FromLabel("screenMemory+$01B8"), WallChar, 0);
            C64.FillMemory(C64Address.FromLabel("screenMemory+$02B8"), WallChar, 0);
            C64.FillMemory(C64Address.FromLabel("screenMemory+$0300"), WallChar, 0);

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
        }
        private static void BuildKnightPit(uint startX)
        {
            uint d = 0;
            for (uint y = 10; y < 15; y++)
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

        private static void BuildEmptyPit(uint startX)
        {
            uint d = 0;
            for (uint y = 10; y < 14; y++)
            {
                for (uint x = startX; x < startX + 4; x++)
                {
                    var b = C64.GetMemory(C64Address.FromLabel("tbl_EmptyPitChars"), d);
                    var c = C64.GetMemory(C64Address.FromLabel("tbl_EmptyPitCharColours"), d);

                    C64.SetChar(x, y, b, (Colors)c);
                    d++;
                }
            }
        }

        public bool IsHole(uint x)
        {
            if (wallType_ == WallType.EmptyPits || wallType_ == WallType.KnightPits)
            {
                if (x > 92 && x < 114)
                    return true;
                if (x > 164 && x < 186)
                    return true;
                if (x > 236 && x < 255)
                    return true;
            }

            if (wallType_ == WallType.Rope)
            {
                if (x > 106 && x < 238)
                    return true;
            }
            return false;
        }

        public void Move()
        {
            if (wallType_ != WallType.KnightPits)
                return;
            speedCounter_++;
            if (speedCounter_ < 2)
                return;
            speedCounter_ = 0;
            frameCounter_++;
            if (frameCounter_ == 5)
            {
                frameCounter_ = 0;
                knightCounter_++;
                if (knightCounter_ > 6)
                    knightCounter_ = 0;
            }

            if (knightCounter_ < 4)
                return;

            uint x = 10;
            if (knightCounter_ == 4)
                x = 28;
            if (knightCounter_ == 5)
                x = 19;

            switch (frameCounter_)
            {
                case 0:
                case 4:
                    KnightPitFrame1(x);
                    break;
                case 1:
                case 3:
                    KnightPitFrame2(x);
                    break;
                case 2:
                    KnightPitFrame3(x);
                    break;
            }
        }

        private static void BuildRopePit()
        {
            for (uint x = 12; x < 28; x++)
            {
                for (uint y = 11; y < 21; y++)
                {
                    C64.SetChar(x, y, Space, Colors.Black);
                }
                C64.SetChar(x, 21, FireChar, Colors.Red);
            }

            for (uint y = 12; y < 22; y++)
            {
                C64.SetChar(11, y, Wall3DChar, Colors.Red);
            }
            C64.SetChar(11, 11, Wall3DTopChar, Colors.Red);
        }

        private static void KnightPitFrame1(uint x)
        {
            C64.SetChar(x, 8, 0x20, Colors.LightRed);
            C64.SetChar(x, 9, 0x20, Colors.LightRed);
            C64.SetChar(x, 10, 0x45, Colors.LightRed);
            C64.SetChar(x, 11, 0x47, Colors.LightRed);
            C64.SetChar(x, 12, 0x4f, Colors.LightRed);
            C64.SetChar(x, 13, 0x50, Colors.LightRed);
            C64.SetChar(x, 14, 0x51, Colors.LightRed);
        }

        private static void KnightPitFrame2(uint x)
        {
            C64.SetChar(x, 8, 0x20, Colors.LightRed);
            C64.SetChar(x, 9, 0x45, Colors.LightRed);
            C64.SetChar(x, 10, 0x46, Colors.LightRed);
            C64.SetChar(x, 11, 0x47, Colors.LightRed);
            C64.SetChar(x, 12, 0x52, Colors.LightRed);
            C64.SetChar(x, 13, 0x53, Colors.LightRed);
            C64.SetChar(x, 14, 0x54, Colors.LightRed);
        }

        private static void KnightPitFrame3(uint x)
        {
            C64.SetChar(x, 8, 0x45, Colors.LightRed);
            C64.SetChar(x, 9, 0x46, Colors.LightRed);
            C64.SetChar(x, 10, 0x46, Colors.LightRed);
            C64.SetChar(x, 11, 0x56, Colors.LightRed);
            C64.SetChar(x, 12, 0x55, Colors.LightRed);
            C64.SetChar(x, 13, 0x57, Colors.LightRed);
            C64.SetChar(x, 14, 0x58, Colors.LightRed);
        }
    }
}