using C64Lib;

namespace Catacombs;

// Everything drawn with hardware sprites 0-5, as up to four ACTORS standing
// on the floor, each made of up to three sprites (PARTS):
//
//   Player   the adventurer: hi-res torchlight overlay, top half, legs
//   Feature  what the room holds: its guardian (two sprites) or a pickup
//            (sparkle + item); never both (see Monster.cs)
//   Crawler  the spider on the floor
//   Flyer    the bat, only where there is no Feature (six sprites in all)
//
// An actor is placed by the point between its feet, in bitmap pixels (what
// RoomProjection.ScreenX / ScreenY return); each part has an offset from it.
//
// Depth: the VIC draws sprite 0 in front of sprite 1 and so on, by sprite
// NUMBER, so every video frame the actors are handed out to the hardware
// sprites nearest first (largest floor Y first). The order is kept from
// frame to frame and fixed up with one bubble pass, which is enough: actors
// pass each other one at a time.
//
// Sprites 6 and 7 are never touched (they belong to the combat screen).
static class SpriteStage
{
    public const uint Player = 0, Feature = 1, Crawler = 2, Flyer = 3;
    public const uint HiRes = 0x80;         // in a part's colour: a hi-res sprite

    public static uint Mc, Hi;              // first frame of the Chars / CharsHi sheets

    public static ulong[] X;                // per actor: the feet, bitmap x
    public static uint[] Y;                 //            bitmap y; 0 = not on stage
    public static uint[] Parts;             //            how many parts
    public static uint[] Frame;             // per part (actor * 3 + n), front one first
    public static uint[] Color;
    public static uint[] Dx;                // sprite X = feet x + Dx, sprite Y = feet y + Dy
    public static uint[] Dy;

    static uint[] order_;                   // the actors, nearest first
    static readonly uint[] Bit = { 1, 2, 4, 8, 16, 32, 64 };

    public static void Init()
    {
        Mc = (uint)C64Address.FromLabel("frm_Chars");
        Hi = (uint)C64Address.FromLabel("frm_CharsHi");
        X = new ulong[4];
        Y = new uint[4];
        Parts = new uint[4];
        Frame = new uint[12];
        Color = new uint[12];
        Dx = new uint[12];
        Dy = new uint[12];
        order_ = new uint[4];
        for (uint i = 0; i < 4; i++)
            order_[i] = i;

        // Shared by every multicolour sprite: black outlines and shadows,
        // orange for torch-lit skin, leather, wood and rust.
        C64.Sprites.CommonColor1 = Colors.Black;
        C64.Sprites.CommonColor2 = Colors.Orange;
    }

    public static void Part(uint index, uint color, uint dx, uint dy)
    {
        Color[index] = color;
        Dx[index] = dx;
        Dy[index] = dy;
    }

    // Once per video frame, right after the vertical blank.
    public static void Update()
    {
        PlayerSprite.Animate();

        for (uint i = 0; i < 3; i++)
        {
            uint a = order_[i];
            uint b = order_[i + 1];
            if (Y[a] < Y[b])
            {
                order_[i] = b;
                order_[i + 1] = a;
            }
        }

        uint slot = 0;
        uint multi = 0;
        for (uint i = 0; i < 4; i++)
        {
            uint a = order_[i];
            uint y = Y[a];
            if (y != 0)
            {
                uint p = a + a + a;
                uint end = p + Parts[a];
                while (p < end)
                {
                    uint c = Color[p];
                    if (c < HiRes)
                        multi = multi | Bit[slot];
                    C64.Sprites[slot].Place(X[a] + Dx[p], y + Dy[p], Frame[p], (Colors)c);
                    slot = slot + 1;
                    p = p + 1;
                }
            }
        }
        C64.Sprites.MultiColorMask = (C64.Sprites.MultiColorMask & 0xC0) | multi;
        C64.Sprites.VisibleMask = (C64.Sprites.VisibleMask & 0xC0) | (Bit[slot] - 1);
    }
}
