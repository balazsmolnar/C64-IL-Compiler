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
//                   wall that has an exit, the room's props, a torch put
//                   out if the room has one, and the room's palette theme (a
//                   colour remap of the whole viewport). A half-drawn room
//                   is never visible.
//   Animate         every video frame: flickers the torches and pulses the
//                   door cue. At most one small picture or one small remap
//                   per frame.
//   Cue             which door the player stands at (a door bit, 0 = none):
//                   that door's colours pulse, "SPACE goes through here".
//   OpenDoor        a door swinging open, one step per call.
static class DungeonView
{
    public const uint North = 1, West = 2, East = 4, South = 8;                     // door bits
    public const uint Banner = 1, Bones = 2, Puddle = 4, Grate = 8, Niche = 16;      // prop bits
    public const uint Dark = 64, DarkRight = 128;             // the left / right torch is out

    // Indices into room_Table: RoomArt.FirstProp etc., generated with the table.
    const uint FirstProp = RoomArt.FirstProp, FirstOpening = RoomArt.FirstOpening, FirstFlame = RoomArt.FirstFlame;
    const uint FirstGlow = RoomArt.FirstGlow;
    const uint DeadTorch = RoomArt.DeadTorch, FirstTheme = RoomArt.FirstTheme, Fade = RoomArt.Fade;
    const uint Unlit = RoomArt.Unlit, CueTable = RoomArt.CueTable, DarkRects = RoomArt.DarkRects, CueRects = RoomArt.CueRects;

    static uint tick_;
    static uint out_;           // Dark / DarkRight bits of the room on screen
    static bool shown_;
    static uint cue_;           // the door the player is at
    static uint cueShown_;      // the door whose colours are swapped right now (0 = none)

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
            // The room on screen goes a step darker (recoloured while
            // blanked, so no tear), shows for a few frames, then blanks.
            C64.Screen.BeginUpdate();
            Recolour(Fade);
            C64.Screen.EndUpdate();
            C64.Screen.WaitForVBlank();
            C64.Screen.WaitForVBlank();
            C64.Screen.WaitForVBlank();
        }
        C64.Screen.BeginUpdate();
        C64.Screen.DrawImage(0, 0, C64Address.FromLabel("img_Room"));
        Overlays(doors, 0);
        Overlays(props & 31, FirstProp);
        out_ = props >> 6;
        // A torch gone out: a charred stub for the flame, its light taken
        // off the stones it reached (rectangles along the joints).
        if ((out_ & 1) != 0)
            PutOut(RoomArt.FlameColumn0, DarkRects);
        if ((out_ & 2) != 0)
            PutOut(RoomArt.FlameColumn1, DarkRects + 1);
        if (theme != 0)
            Recolour(FirstTheme + theme);
        shown_ = true;
        cueShown_ = 0;
        C64.Screen.EndUpdate();
    }

    static void Recolour(uint table)
    {
        C64.Screen.RemapCellColors(0, 0, 40, 21, Entry(table));
    }

    static void PutOut(uint column, uint rects)
    {
        C64.Screen.DrawImage(column, RoomArt.FlameRow, Entry(DeadTorch));
        C64.Screen.RemapCellRects(Entry(rects), Entry(Unlit));
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

    // door North, West or East; step 0..1: the leaf swings further in. Each
    // picture holds only the cells that change from the step before.
    public static void OpenDoor(uint door, uint step)
    {
        Cue(0);
        CueUpdate(0);
        C64.Screen.DrawImage(0, 0, Entry(FirstOpening + step + (door & 6)));
    }

    public static void Cue(uint door)
    {
        cue_ = door;
    }

    // Shows the cue on `door` (0 = none): its colours swapped by an
    // involution, so swapping again restores them.
    static void CueUpdate(uint door)
    {
        if (door == cueShown_)
            return;
        if (cueShown_ != 0)
            C64.Screen.RemapCellRects(Entry(CueRects + cueShown_), Entry(CueTable));
        if (door != 0)
            C64.Screen.RemapCellRects(Entry(CueRects + door), Entry(CueTable));
        cueShown_ = door;
    }

    // Call once per frame, right after WaitForVBlank. Frames take turns:
    // a new flame for one torch (each torch every 8 frames), the door cue
    // (swapped for 16 frames, normal for 16), then the colours of the stones
    // that torch lights (one time in four dimmed) -- never two in one frame.
    // At most one small picture or a one-door remap per frame.
    public static void Animate()
    {
        tick_ = tick_ + 1;
        uint phase = tick_ & 3;
        if (phase == 1)
        {
            CueUpdate((tick_ & 16) != 0 ? cue_ : 0);
            return;
        }
        if (phase == 3)
            return;
        uint torch = tick_ & 4;             // 0 = left, 4 = right
        if ((out_ & (1 + (torch >> 2))) != 0)
            return;
        uint random = C64.Random();
        if (phase == 0)
            C64.Screen.DrawImage(RoomArt.FlameColumn0 + torch * 3, RoomArt.FlameRow, Entry(FirstFlame + (random & 3)));
        else
            C64.Screen.DrawImage(0, 0, Entry(FirstGlow + torch + (random >> 6)));     // the lit stones dim now and then
    }
}
