using C64Lib;

namespace Catacombs;

// The adventurer: three sprites (SpriteStage's Player actor). From the front:
//   Light  hi-res overlay: the torch flame's core and the torchlight on his edges
//   Top    multicolour top half (hood, face, cloak, torch)
//   Legs   multicolour legs, with the ground shadow under them
//
// Program.cs only ever says where he stands (SetPosition, when he moved).
// Which way he faces is read off the movement itself, the walk cycle
// advances with the steps actually taken, and when no step arrives for a few
// frames he stands, breathing, the torch flickering.
static class PlayerSprite
{
    const uint Down = 0, Up = 1, Left = 2, Right = 3;
    const uint Walking = 0, Striking = 1, Fallen = 2;
    const uint Light = SpriteStage.Player, Top = SpriteStage.Player + 4, Legs = SpriteStage.Player + 8;


    // His cloak's colour: red, except in rooms that would swallow it (set
    // per room by Monster.Setup).
    public static uint Cloak = (uint)Colors.Red;

    static uint facing_, pose_;
    static uint step_;      // walk cycle position, one per step taken
    static uint still_;     // video frames since the last step
    static uint tick_;
    static uint xLow_;      // low byte of the last x: enough to see a step
    static uint hurt_;      // frames left of the white hurt flash
    static uint strike_;    // frames since the strike began

    public static void Init(ulong x, ulong y)
    {
        facing_ = Up;
        SpriteStage.Part(Light, 0, 12, 15);
        SpriteStage.Part(Top, 0, 12, 15);
        SpriteStage.Part(Legs, SpriteStage.Multi | (uint)Colors.Grey2, 12, 36);
        SetPosition(x, y);
    }

    // The point between his feet, in bitmap pixels (RoomProjection).
    public static void SetPosition(ulong x, ulong y)
    {
        uint ny = (uint)y;
        uint d = ny - SpriteStage.Y[0];
        if (d == 0)
        {
            // A step sideways is 2 pixels or more; less is the perspective
            // drift of walking in or out and must not turn him.
            d = (uint)x - xLow_ - 2;
            if (d < 6)
                facing_ = Right;
            else if (d > 246 && d < 253)
                facing_ = Left;
        }
        else if (d < 4)
            facing_ = Down;
        else if (d > 252)
            facing_ = Up;
        // (anything bigger is arriving through a door: keep facing)

        xLow_ = (uint)x;
        SpriteStage.X[0] = x;
        SpriteStage.Y[0] = ny;
        still_ = 0;
        step_ = step_ + 1;
        pose_ = Walking;
    }

    public static ulong X => SpriteStage.X[0];
    public static uint BitmapY => SpriteStage.Y[0];

    // A white flash for a few frames: he was hit.
    public static void Hurt()
    {
        hurt_ = 12;
    }

    // Wind-up, then the slash; held until he next moves.
    public static void Strike()
    {
        pose_ = Striking;
        strike_ = 0;
    }

    // Slain, until he is next placed.
    public static void Fall()
    {
        pose_ = Fallen;
        strike_ = 0;
        Animate();
    }

    // Once per video frame (SpriteStage.Update).
    public static void Animate()
    {
        tick_ = tick_ + 1;

        if (pose_ == Fallen)
        {
            // On the floor; by his head the dropped torch's ember cools off.
            uint ember = (uint)Colors.Orange;
            if (strike_ < 30)
                strike_ = strike_ + 1;
            else
                ember = (uint)Colors.Brown;
            SpriteStage.Count[0] = 2;
            SpriteStage.Parts[Light + SpriteStage.Frame] = SpriteStage.Hi + Frame.Spark;
            SpriteStage.Parts[Light + SpriteStage.Color] = ember;
            SpriteStage.Parts[Top + SpriteStage.Color] = SpriteStage.Multi | (uint)Colors.LightRed; // the body stands out as the room darkens
            SpriteStage.Parts[Light + SpriteStage.Dy] = 36; // by his outflung hand (the fallen frame's right edge, row 10)
            SpriteStage.Parts[Light] = 23;
            SpriteStage.Parts[Top + SpriteStage.Frame] = SpriteStage.Mc + Frame.Fallen;
            SpriteStage.Parts[Top + SpriteStage.Dy] = 36;
            return;
        }
        uint lightX = 12;

        uint legs = 1;                          // standing
        uint bob = 15 + ((tick_ >> 5) & 1);     // breathing
        if (still_ < 3)
        {
            still_ = still_ + 1;
            legs = (step_ >> 2) & 3;            // contact, passing, contact, passing
            bob = 16 - (legs & 1);              // the body dips as a foot lands
        }

        uint top = facing_;
        uint cycle = Frame.Legs;
        if (pose_ == Striking)
        {
            top = Frame.TopRaise;
            if (strike_ > 3)
            {
                top = Frame.TopSlash;
                lightX = 20;        // the overlay moves right: the blade swings out past him
            }
            else
                strike_ = strike_ + 1;
        }
        else if (facing_ > Up)
            cycle = Frame.Legs - 4 + (facing_ << 2);
        uint cloak = SpriteStage.Multi | Cloak;
        if (hurt_ != 0)
        {
            hurt_ = hurt_ - 1;
            cloak = SpriteStage.Multi | (uint)Colors.White;
        }
        // The flame gutters now and then: the light on him dims with it.
        uint light = (uint)Colors.Yellow;
        if (C64.Random() < 20)
            light = (uint)Colors.LightRed;
        if (lightX == 20)
            light = (uint)Colors.White;     // the slash: a white blade

        SpriteStage.Count[0] = 3;
        SpriteStage.Parts[Light] = lightX;
        SpriteStage.Parts[Light + SpriteStage.Frame] = SpriteStage.Hi + Frame.Overlay + top;
        SpriteStage.Parts[Light + SpriteStage.Color] = light;
        SpriteStage.Parts[Light + SpriteStage.Dy] = bob;
        SpriteStage.Parts[Top + SpriteStage.Frame] = SpriteStage.Mc + Frame.Top + top;
        SpriteStage.Parts[Top + SpriteStage.Color] = cloak;
        SpriteStage.Parts[Top + SpriteStage.Dy] = bob;
        SpriteStage.Parts[Legs + SpriteStage.Frame] = SpriteStage.Mc + cycle + legs;
    }
}
