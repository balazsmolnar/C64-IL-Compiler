using C64Lib;

class Ball
{

    const int circle = 81;
    const int WIDTH = 38;
    const int HEIGHT = 24;

    public int X { get; set; }
    public int Y { get; set; }
    public int VX { get; set; }
    public int VY { get; set; }
    public Colors BallColor { get; set; }
    private int OldChar { get; set; }
    public void Clear()
    {

        if (OldChar != circle)
            C64.SetChar(X, Y, OldChar);
        X += VX;
        Y += VY;
        if (X == WIDTH)
            VX = -VX;
        if (X == 0)
            VX = -VX;
        if (Y == HEIGHT)
            VY = -VY;
        if (Y == 0)
            VY = -VY;

    }

    public void Set()
    {
        OldChar = C64.GetChar(X, Y);
        C64.SetChar(X, Y, circle, BallColor);
    }

}