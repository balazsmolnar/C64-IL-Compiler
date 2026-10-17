#if SCENE_ROOM4 || SCENE_DEADEND || SCENE_THEME0 || SCENE_THEME1 || SCENE_THEME2 || SCENE_THEME3 || SCENE_THEME4 || SCENE_DOOROPEN || SCENE_ROOMTOUR
using C64Lib;

namespace Catacombs;

// Scene builds for looking at the rooms without playing (AAA-BRIEF.md,
// "Looking at the game"): tools/catacombs-shot.ps1 -Scene NAME. Each shows
// the room alone, torches flickering, with no sprites and no HUD.
//
//   ROOM4      a room with all four exits, theme 0, bones and a banner
//   DEADEND    a dead end: only the south exit, with chains, a grate and cobwebs
//   THEME0-4   one per palette theme (moss, cold, rust, crypt, bone), each
//              with different doors and props; THEME1 has its left torch out
//   DOOROPEN   the north door opening, looping (closed, two steps 10 frames apart, open)
//   ROOMTOUR   walks through the maze's rooms, a new one every 1.5 seconds:
//              room entry transitions and the variety from room to room
static class RoomScenes
{
    public static void Run()
    {
        C64.Screen.SetBorderColor(Colors.Black);
        C64.Screen.SetBackgroundColor(Colors.Black);
        C64.Screen.SetScreenMode(ScreenMode.MultiColor);
        C64.Screen.ClearBitmap();
        C64.Screen.SetCellColors(0, 21, 40, 4, 0, Colors.Black);

#if SCENE_ROOM4
        Show(0, DungeonView.North | DungeonView.West | DungeonView.East | DungeonView.South, DungeonView.Bones | DungeonView.Banner | DungeonView.Webs);
#elif SCENE_DEADEND
        Show(0, DungeonView.South, DungeonView.Chains | DungeonView.Grate | DungeonView.Webs);
#elif SCENE_THEME0
        Show(0, DungeonView.North | DungeonView.East, DungeonView.Puddle | DungeonView.Chains);
#elif SCENE_THEME1
        Show(1, DungeonView.North | DungeonView.West, DungeonView.Dark | DungeonView.Puddle);
#elif SCENE_THEME2
        Show(2, DungeonView.North | DungeonView.East | DungeonView.South, DungeonView.Bones | DungeonView.Grate);
#elif SCENE_THEME3
        Show(3, DungeonView.West | DungeonView.East, DungeonView.Banner | DungeonView.Bones | DungeonView.Webs);
#elif SCENE_THEME4
        Show(4, DungeonView.North | DungeonView.West | DungeonView.South, DungeonView.Banner | DungeonView.Puddle);
#elif SCENE_DOOROPEN
        for (;;)
        {
            DungeonView.Draw(0, DungeonView.North | DungeonView.West, DungeonView.Bones);
            Frames(40);
            for (uint step = 0; step < 2; step++)
            {
                DungeonView.OpenNorthDoor(step);
                Frames(10);
            }
            Frames(40);
        }
#else
        for (;;)
        {
            for (uint y = 1; y < Maze.Height; y++)
            {
                for (uint x = 1; x < Maze.Width; x++)
                {
                    if (!Maze.IsWall(x, y))
                    {
                        DungeonView.Render(x, y);
                        Frames(75);
                    }
                }
            }
        }
#endif
    }

    static void Show(uint theme, uint doors, uint props)
    {
        DungeonView.Draw(theme, doors, props);
        for (;;)
            Frames(250);
    }

    static void Frames(uint count)
    {
        for (uint i = 0; i < count; i++)
        {
            C64.Screen.WaitForVBlank();
            DungeonView.Animate();
        }
    }
}
#endif
