namespace Catacombs;

// Plain busy-wait, same shape as C64Presentation/Helper/Delay.cs -- this
// project doesn't share a library with that one, so it's duplicated rather
// than reaching across projects for four lines.
static class Delay
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
