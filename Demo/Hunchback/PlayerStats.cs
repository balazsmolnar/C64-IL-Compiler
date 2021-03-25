using C64Lib;

namespace Hunchback
{
    public class PlayerStats
    {
        public void Draw()
        {
            uint d = 0;
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

        }
    }
}