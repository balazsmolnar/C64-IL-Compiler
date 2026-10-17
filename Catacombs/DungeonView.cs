using C64Lib;

namespace Catacombs;

// The room: a torch-lit stone chamber seen north-up (north is the far wall,
// west/east the side walls, south the edge of the floor nearest the viewer),
// in the fixed geometry of AAA-BRIEF.md. Nothing here is drawn with lines:
// the room is one full-viewport picture (rows 0-20) plus small overlay
// pictures, all made by art/room_art.py and declared in RoomArt.cs.
//
//   Render / Draw   once per room entry: the old room fades out, then with
//                   the screen blanked the backdrop, a door overlay on each
//                   wall that has an exit, the room's props, and the room's
//                   palette theme (a colour remap of the whole viewport). A
//                   half-drawn room is never visible.
//   Animate         every video frame: flickers the torches (a few cells of
//                   flame, and the fire colours of the wall around them).
//   OpenNorthDoor   the north door swinging open, one step per call.
static class DungeonView
{
    public const uint North = 1, West = 2, East = 4, South = 8;                     // door bits
    public const uint Chains = 1, Banner = 2, Bones = 4, Puddle = 8, Grate = 16;    // prop bits
    public const uint Webs = 32, Dark = 64;     // cobwebs; Dark: the left torch is out

    // Indices into room_Table (RoomArt.cs): door overlays in door-bit order,
    // props in prop-bit order, the door-opening steps, four flame pictures,
    // four glow states for each torch, the theme colour tables, the fade.
    const uint FirstDoor = 0, FirstProp = 4, FirstOpening = 11, FirstFlame = 13, FirstGlow = 17;
    const uint FirstTheme = 24, Fade = 29;

    static uint tick_;
    static bool leftOut_;
    static bool shown_;

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
        Draw((Maze.Info(px, py) >> 5) - 1, doors, Maze.Props(px, py));
    }

    // theme 0..RoomArt.ThemeCount-1, doors and props as bit sets.
    public static void Draw(uint theme, uint doors, uint props)
    {
        if (shown_)
        {
            // Fade the room on screen out (two steps darker) before blanking.
            C64.Screen.RemapCellColors(0, 0, 40, 21, Entry(Fade));
            C64.Screen.RemapCellColors(0, 0, 40, 21, Entry(Fade));
        }
        C64.Screen.BeginUpdate();
        C64.Screen.DrawImage(0, 0, C64Address.FromLabel("img_Room"));
        Overlays(doors, FirstDoor);
        Overlays(props, FirstProp);
        if (theme != 0)
            C64.Screen.RemapCellColors(0, 0, 40, 21, Entry(FirstTheme + theme));
        leftOut_ = (props & Dark) != 0;
        shown_ = true;
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

    // step 0..1: the door leaf swings further in. Drawn over the closed door
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
        if (torch == 0 && leftOut_)
            return;
        uint random = C64.Random();
        C64.Screen.DrawImage(torch == 0 ? RoomArt.FlameColumn0 : RoomArt.FlameColumn1, RoomArt.FlameRow, Entry(FirstFlame + (random & 3)));
        if ((random & 16) != 0)
            C64.Screen.DrawImage(0, 0, Entry(FirstGlow + torch + (random >> 6)));
    }
}
