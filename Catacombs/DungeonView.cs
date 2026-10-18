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
//                   of flame, and the fire colours of the wall around them).
//   OpenNorthDoor   the north door swinging open, one step per call.
static class DungeonView
{
    public const uint North = 1, West = 2, East = 4, South = 8;                     // door bits
    public const uint Chains = 1, Banner = 2, Bones = 4, Puddle = 8, Grate = 16;    // prop bits

    // Indices into room_Table (RoomArt.cs): door overlays in door-bit order,
    // props in prop-bit order, the door-opening steps, four flame pictures,
    // four glow states for each torch, then the theme colour tables.
    const uint FirstDoor = 0, FirstProp = 4, FirstOpening = 9, FirstFlame = 12, FirstGlow = 16, FirstTheme = 23;

    static uint tick_;

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
        uint info = Maze.Info(px, py);
        Draw((info >> 5) - 1, doors, info & 31);
    }

    // theme 0..RoomArt.ThemeCount-1, doors and props as bit sets.
    public static void Draw(uint theme, uint doors, uint props)
    {
        C64.Screen.BeginUpdate();
        C64.Screen.DrawImage(0, 0, C64Address.FromLabel("img_Room"));
        Overlays(doors, FirstDoor);
        Overlays(props, FirstProp);
        if (theme != 0)
            C64.Screen.RemapCellColors(0, 0, 40, 21, Entry(FirstTheme + theme));
        C64.Screen.EndUpdate();
    }

    // Draws the overlay of every set bit of mask; bit 0 is picture `index`.
    static void Overlays(uint mask, uint index)
    {
        while (mask != 0)
        {
            if ((mask & 1) != 0)
                C64.Screen.DrawImage(0, 0, Entry(index));
            mask = mask >> 1;
            index = index + 1;
        }
    }

    static ulong Entry(uint index)
    {
        return C64.GetWord(C64Address.FromLabel("room_Table"), index);
    }

    // step 0..2: the door leaf swings further in. Drawn over the closed door
    // (each picture only holds the cells that change).
    public static void OpenNorthDoor(uint step)
    {
        C64.Screen.DrawImage(0, 0, Entry(FirstOpening + step));
    }

    // Call once per frame, right after WaitForVBlank. Every fourth frame one
    // of the two torches (they take turns) gets a new flame picture, and
    // half the time a new state for the glow on the wall around it.
    public static void Animate()
    {
        tick_ = tick_ + 1;
        if ((tick_ & 3) != 0)
            return;
        uint torch = tick_ & 4;             // 0 = left, 4 = right
        uint random = C64.Random();
        C64.Screen.DrawImage(torch == 0 ? RoomArt.FlameColumn0 : RoomArt.FlameColumn1, RoomArt.FlameRow, Entry(FirstFlame + (random & 3)));
        if ((random & 16) != 0)
            C64.Screen.DrawImage(0, 0, Entry(FirstGlow + torch + (random >> 6)));
    }
}
