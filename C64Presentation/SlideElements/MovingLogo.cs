using C64Lib;
using C64Presentation.Helper;

namespace C64Presentation.SlideElements;

// A two-by-two (or three-part) sprite logo drifting inside a box, bouncing
// off its edges (sprite coordinates). Scale 2 = expanded sprites.
class MovingLogo
{
    public ulong MinX;
    public uint MinY;

    public ulong MaxX;
    public uint MaxY;

    public ulong X;
    public uint Y;
    public uint Scale;          // 0 or 1 = normal, 2 = expanded; steps 1 px

    public Sprite SpriteTL;
    public Sprite SpriteTR;
    public Sprite SpriteBL;
    public Sprite SpriteBR;

    private bool right;
    private bool down;

    public void Init()
    {
        Sprites.SetupLogo(Scale == 2);
        if (Scale == 0)
            Scale = 1;
        Sprites.SetVisible(true);
        Place();
    }

    public void Step()
    {
        X = right ? X + 1 : X - 1;
        Y = down ? Y + 1 : Y - 1;

        if (X >= MaxX || X <= MinX)
            right = !right;

        if (Y >= MaxY || Y <= MinY)
            down = !down;
        Place();
    }

    void Place()
    {
        ulong w = Scale == 2 ? 48ul : 24ul;
        uint h = Scale == 2 ? 42u : 21u;
        SpriteTL.X = X;
        SpriteTL.Y = Y;
        SpriteBL.X = X;
        SpriteBL.Y = Y + h;
        SpriteTR.X = X + w;
        SpriteTR.Y = Y;
        if (SpriteBR != null)
        {
            SpriteBR.X = X + w;
            SpriteBR.Y = Y + h;
        }
    }
}
