using C64Lib;

[assembly: RawAssembly(Asm = @"
proj_Side  .byte 0,0,1,1,2,2,3,3,4,4,5,5,6,6,6,7,7,8,8,9
           .byte 9,10,10,11,11,12,12,12,13,13,14,14,15,15,16,16,17,17,17,18
           .byte 18,19,19,20,20,21,21,22,22,23,23,23,24,24,25,25,26,26,27,27
           .byte 28,28,29,29,29,30,30,31,31,32,32,33,33,34,34,35,35,35,36,36
           .byte 37,37,38,38,39,39,40,40,40,41,41,42,42,43,43,44,44,45,45,46
           .byte 46
proj_Rise  .byte 0,0,1,1,1,2,2,3,3,3,4,4,4,5,5,5,6,6,6,7
           .byte 7,8,8,8,9,9,9,10,10,10,11,11,12,12,12,13,13,13,14,14
           .byte 14,15,15,15,16,16,17,17,17,18,18,18,19,19,19,20,20,21,21,21
           .byte 22,22,22,23,23,23,24,24,24,25,25,26,26,26,27,27,27,28,28,28
           .byte 29,29,30,30,30,31,31,31,32,32,32,33,33,33,34,34,35,35,35,36
           .byte 36
")]

namespace Catacombs;

// Converts a logical position within a room into the screen position of
// that point of the floor. Program.cs and Monster/Item only ever deal in
// room-space:
//   roomX:     0 (against the left wall) .. Width (against the right
//              wall), Width/2 = center -- ALWAYS this same range,
//              regardless of depth.
//   roomDepth: 0 (the near edge of the floor, by the south exit) ..
//              DepthMax (the foot of the far wall / the north door).
// The result is in bitmap pixels (x 0-319, y 0-199): where something
// standing on that spot touches the floor, i.e. the bottom centre of a
// sprite. (Sprite registers: X = ScreenX + 24 - half the sprite's width,
// Y = ScreenY + 50 - its height.)
//
// The floor of the room picture (DungeonView / art/room_art.py) is the
// trapezoid between the foot of the far wall (y 120, x 64..255) and the
// bottom of the viewport, its sides running along the lines through the
// vanishing point (160, 56). The walkable area is that trapezoid from y 124
// to y 160, kept a little inside the side walls so a sprite of ordinary
// width does not overlap them. Both edges are straight lines, so a plain
// linear interpolation in depth is exact.
static class RoomProjection
{
    public const uint Width = 100;     // 0=left wall, 50=center, 100=right wall
    public const uint Center = Width / 2;
    public const uint DepthMax = 100;  // 0=near edge, DepthMax=far wall

    // The floor's own edges are x = 4 / 315 at y 160 and x = 58 / 261 at
    // y 124; the walkable band is inset by 24 pixels near and 16 far.
    const ulong XLeftNear = 28, XRightNear = 291;
    const ulong XLeftFar = 74, XRightFar = 245;
    const ulong ScreenYNear = 160, ScreenYFar = 124;

    // Called every frame the player moves, so no division by 100 per
    // call: both edges and the floor line move linearly with depth, the
    // sides by 46 pixels over the whole depth and y by 36, read from two
    // 101-byte tables (proj_Side[d] = round(46 d / 100), proj_Rise[d] =
    // round(36 d / 100), see the RawAssembly above the namespace).
    public static ulong ScreenX(uint roomX, uint depth)
    {
        ulong side = C64.GetMemory(C64Address.FromLabel("proj_Side"), depth);
        ulong width = XRightNear - XLeftNear - side - side;
        return XLeftNear + side + width * roomX / Width;
    }

    public static ulong ScreenY(uint depth)
    {
        return ScreenYNear - C64.GetMemory(C64Address.FromLabel("proj_Rise"), depth);
    }
}
