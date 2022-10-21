namespace C64Presentation.Helper;

internal static class Delay
{
    public static void Wait(uint elapsed)
    {
        for (uint j = 0; j < elapsed; j++)
        {
            for (uint i = 0; i < 10; i++)
            {
            }
        }
    }
}