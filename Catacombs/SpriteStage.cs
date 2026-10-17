using C64Lib;

namespace Catacombs;

// Everything drawn with hardware sprites 0-5, as up to four ACTORS standing
// on the floor, each made of up to three sprites (PARTS):
//
//   Player   the adventurer: hi-res torchlight overlay, top half, legs
//   Feature  what the room holds: its guardian (two sprites) or a pickup
//            (glint + item); never both (see Monster.cs)
//   Crawler  the spider on the floor
//   Flyer    the bat, only where there is no Feature (six sprites in all)
//
// An actor is placed by the point between its feet, in bitmap pixels (what
// RoomProjection.ScreenX / ScreenY return); each part is four bytes of
// Parts -- offset from that point, frame, colour -- exactly as
// C64.Sprites.PlaceGroup reads them, which writes a whole actor's sprite
// registers in one call.
//
// Depth: the VIC draws sprite 0 in front of sprite 1 and so on, by sprite
// NUMBER, so every video frame the actors are handed out to the hardware
// sprites nearest first (largest floor Y first). The order is kept from
// frame to frame and one neighbouring pair is put right per frame, which is
// enough: actors pass each other one at a time, slowly.
//
// Sprites 6 and 7 are never touched (they belong to the combat screen).
static class SpriteStage
{
    // Actors, as the offset of their first part in Parts.
    public const uint Player = 0, Feature = 16, Crawler = 32, Flyer = 48;

    // A part: Parts[part + ...]. The second and third parts of an actor
    // are at + 4 and + 8.
    public const uint Dx = 0, Dy = 1, Frame = 2, Color = 3;
    public const uint Multi = 0x80;         // in a part's colour: a multicolour sprite

    public static uint Mc, Hi;              // first frame of the Chars / CharsHi sheets

    public static ulong[] X;                // per actor (index = actor / 16): the feet, bitmap x
    public static uint[] Y;                 //   bitmap y; 0 = not on stage
    public static uint[] Count;             //   how many parts
    public static uint[] Parts;

    static uint[] order_;                   // the actors (/ 16), nearest first
    static uint pair_;
    static readonly uint[] Shown = { 0, 1, 3, 7, 15, 31, 63 };

    public static void Init()
    {
        Mc = (uint)C64Address.FromLabel("frm_Chars");
        Hi = (uint)C64Address.FromLabel("frm_CharsHi");
        X = new ulong[4];
        Y = new uint[4];
        Count = new uint[4];
        Parts = new uint[64];
        order_ = new uint[4];
        for (uint i = 0; i < 4; i++)
            order_[i] = i;

        // Shared by every multicolour sprite: black outlines and deep shadow,
        // brown for shading, leather, wood and rust (key light upper left).
        C64.Sprites.CommonColor1 = Colors.Black;
        C64.Sprites.CommonColor2 = Colors.Brown;
    }

    public static void Part(uint part, uint color, uint dx, uint dy)
    {
        Parts[part] = dx;
        Parts[part + Dy] = dy;
        Parts[part + Color] = color;
    }

    // True while the player's feet are within `radius` pixels of the room's
    // Feature (guardian or pickup), both ways.
    public static bool PlayerAtFeature(ulong radius)
    {
        uint fy = Y[1];
        if (fy == 0)
            return false;
        uint py = Y[0];
        uint dy = py > fy ? py - fy : fy - py;
        ulong px = X[0];
        ulong fx = X[1];
        ulong dx = px > fx ? px - fx : fx - px;
        return dx < radius && dy < radius;
    }

    // Once per video frame, right after the vertical blank.
    public static void Update()
    {
        PlayerSprite.Animate();

        uint i = pair_ + 1;
        if (i == 3)
            i = 0;
        pair_ = i;
        uint a = order_[i];
        uint b = order_[i + 1];
        if (Y[a] < Y[b])
        {
            order_[i] = b;
            order_[i + 1] = a;
        }

        uint slot = 0;
        for (i = 0; i < 4; i++)
        {
            a = order_[i];
            uint y = Y[a];
            if (y != 0)
            {
                uint n = Count[a];
                C64.Sprites.PlaceGroup(slot, n, X[a], y, Parts, a << 4);
                slot = slot + n;
            }
        }
        C64.Sprites.VisibleMask = (C64.Sprites.VisibleMask & 0xC0) | Shown[slot];
    }
}
