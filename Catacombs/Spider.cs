using C64Lib;

namespace Catacombs;

// The vermin every room has, for life and nothing else:
//   the Crawler (SpriteStage actor 2): a spider that scuttles across the
//     floor in bursts, legs going, and freezes in between;
//   the Flyer (actor 3): a bat circling up by the far wall. It only appears
//     in rooms with no guardian or pickup, which need its sprite.
// Both are single hi-res sprites.
static class Spider
{
    const uint FloorY = 152;                // the crawler's first line on the floor
    const ulong Left = 44, Right = 276;

    static ulong crawlerX_, flyerX_;
    static bool crawlerRight_, flyerRight_;
    static uint run_;                       // frames left of this burst (or of this pause)
    static bool running_;
    static uint tick_;

    static readonly uint[] Swoop = { 44, 46, 49, 53, 58, 62, 65, 67, 68, 67, 65, 62, 58, 53, 49, 46 };
    static readonly uint[] Flap = { 0, 1, 2, 1 };

    public static void Init()
    {
        crawlerX_ = 90;
        flyerX_ = 200;
        SpriteStage.Part(SpriteStage.Crawler, (uint)Colors.Black, 12, 31);
        SpriteStage.Part(SpriteStage.Flyer, SpriteStage.Multi | (uint)Colors.Red, 12, 42);
        SpriteStage.Count[2] = 1;
        SpriteStage.Count[3] = 1;
        SpriteStage.Y[2] = FloorY;
    }

    // Called on entering a room: the vermin there are somewhere else.
    public static void SetRoomColor(uint roomColor)
    {
        crawlerX_ = 60 + (ulong)(C64.Random() & 127);
    }

    // Once per video frame.
    public static void Animate()
    {
        tick_ = tick_ + 1;

        if (run_ == 0)
        {
            uint r = C64.Random();
            run_ = 10 + (r & 31);
            running_ = !running_;
            if (r > 127)
                crawlerRight_ = !crawlerRight_;
            if (running_)
                SpriteStage.Y[2] = 130 + (r & 31);     // a new line across the floor each dash
        }
        run_ = run_ - 1;

        uint pose = 2;                      // at rest, legs spread
        if (running_)
        {
            if (crawlerRight_)
            {
                crawlerX_ = crawlerX_ + 2;
                if (crawlerX_ > Right)
                    crawlerRight_ = false;
            }
            else
            {
                crawlerX_ = crawlerX_ - 2;
                if (crawlerX_ < Left)
                    crawlerRight_ = true;
            }
            pose = (tick_ >> 1) & 1;
        }
        SpriteStage.X[2] = crawlerX_;
        SpriteStage.Parts[SpriteStage.Crawler + SpriteStage.Frame] = SpriteStage.Hi + Frame.Crawler + pose;

        if (SpriteStage.Y[1] == 0)
        {
            if (flyerRight_)
            {
                flyerX_ = flyerX_ + 1;
                if (flyerX_ > Right)
                    flyerRight_ = false;
            }
            else
            {
                flyerX_ = flyerX_ - 1;
                if (flyerX_ < Left)
                    flyerRight_ = true;
            }
            SpriteStage.X[3] = flyerX_;
            SpriteStage.Y[3] = Swoop[(tick_ >> 2) & 15];
            SpriteStage.Parts[SpriteStage.Flyer + SpriteStage.Frame] = SpriteStage.Mc + Frame.Flyer + Flap[(tick_ >> 2) & 3];
        }
    }
}
