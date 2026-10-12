using C64Lib;

namespace Catacombs;

// The room: a torch-lit stone chamber seen north-up (north is the far wall,
// west/east the side walls, south the edge of the floor nearest the viewer),
// in the fixed geometry of AAA-BRIEF.md. Nothing here is drawn with lines:
// the room is one full-viewport picture (rows 0-20) plus small overlay
// pictures, all made by art/room_art.py and declared in RoomArt.cs.
//
//   Render / Draw   once per room entry: backdrop, then a door overlay on
//                   each wall that has an exit, then the room's props, then
//                   the room's palette theme (a colour remap of the whole
//                   viewport). The screen is blanked while this happens, so
//                   a half-drawn room is never visible.
//   Animate         every video frame: flickers the two torches (a few cells
//                   of flame, and the colours of the lit wall around them).
//   OpenNorthDoor   the north door swinging open, one step per call.
static class DungeonView
{
    public const uint North = 1, West = 2, East = 4, South = 8;                     // door bits
    public const uint Chains = 1, Banner = 2, Bones = 4, Puddle = 8, Grate = 16;    // prop bits

    // Indices into room_Images (RoomArt.cs): door overlays in door-bit order,
    // then props in prop-bit order, then the door-opening and flame frames.
    const uint FirstDoor = 0, FirstProp = 4, FirstOpening = 9, FirstFlame = 12;

    static uint theme_;
    static uint tick_;
    static uint flame_;

    public static void Render(uint px, uint py)
    {
        uint doors = 0;
        if (!Maze.NorthIsWall(px, py))
            doors = North;
        if (!Maze.WestIsWall(px, py))
            doors = doors | West;
        if (!Maze.EastIsWall(px, py))
            doors = doors | East;
        if (!Maze.SouthIsWall(px, py))
            doors = doors | South;
        Draw(Maze.Theme(px, py), doors, Maze.Props(px, py));
    }

    public static void Draw(uint theme, uint doors, uint props)
    {
        C64.Screen.BeginUpdate();
        C64.Screen.DrawImage(0, 0, C64Address.FromLabel("img_Room"));
        Overlays(doors, FirstDoor);
        Overlays(props, FirstProp);
        if (theme != 0)
        {
            ulong offset = theme - 1;
            C64.Screen.RemapCellColors(0, 0, 40, 21, C64Address.FromLabel("pal_Themes") + offset * 16);
        }
        theme_ = theme;
        C64.Screen.EndUpdate();
    }

    // Draws the overlay of every set bit of mask; bit 0 is image `index`.
    static void Overlays(uint mask, uint index)
    {
        while (mask != 0)
        {
            if ((mask & 1) != 0)
                C64.Screen.DrawImage(0, 0, ImageAt(index));
            mask = mask >> 1;
            index = index + 1;
        }
    }

    static ulong ImageAt(uint index)
    {
        ulong table = C64Address.FromLabel("room_Images");
        ulong high = C64.GetMemory(table, index * 2 + 1);
        return high * 256 + C64.GetMemory(table, index * 2);
    }

    // step 0..2: the door leaf swings further in. Drawn over the closed door
    // (each picture only holds the cells that change).
    public static void OpenNorthDoor(uint step)
    {
        C64.Screen.DrawImage(0, 0, ImageAt(FirstOpening + step));
    }

    // Call once per frame, right after WaitForVBlank. Every fourth frame one
    // of the two torches (they take turns) gets a new flame picture, and
    // sometimes a new brightness for the wall around it.
    public static void Animate()
    {
        tick_ = tick_ + 1;
        if ((tick_ & 3) != 0)
            return;
        uint torch = (tick_ >> 2) & 1;
        uint random = C64.Random();

        flame_ = flame_ + 1 + (random & 1);
        if (flame_ >= 3)
            flame_ = flame_ - 3;
        C64.Screen.DrawImage(torch == 0 ? RoomArt.FlameColumn0 : RoomArt.FlameColumn1, RoomArt.FlameRow, ImageAt(FirstFlame + flame_));

        if ((random & 16) != 0)
            Glow(torch, random >> 6);
    }

    // Recolours the lit wall around one torch: state 0 brightest .. 3 dimmest.
    static void Glow(uint torch, uint state)
    {
        ulong rects = C64Address.FromLabel("glow_Rects");
        uint at = 0;
        if (torch != 0)
            at = C64.GetMemory(rects, 0) * 4 + 1;
        uint count = C64.GetMemory(rects, at);

        ulong colors = C64Address.FromLabel("glow_Colors");
        uint entry = theme_ * (RoomArt.GlowStates * 2) + state * 2;
        uint matrix = C64.GetMemory(colors, entry);
        Colors colorRam = (Colors)C64.GetMemory(colors, entry + 1);

        while (count != 0)
        {
            C64.Screen.SetCellColors(C64.GetMemory(rects, at + 1), C64.GetMemory(rects, at + 2), C64.GetMemory(rects, at + 3), C64.GetMemory(rects, at + 4), matrix, colorRam);
            at = at + 4;
            count = count - 1;
        }
    }
}
