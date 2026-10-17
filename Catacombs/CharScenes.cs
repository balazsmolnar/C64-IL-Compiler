#if SCENE_CHARS
using C64Lib;

namespace Catacombs;

// Scene builds for judging the sprites without playing ("-Scene CHAR_..."
// with tools/catacombs-shot.ps1; any scene name starting CHAR_ lands here).
// They run the game's own sprite code (SpriteStage, PlayerSprite, Spider,
// Monster, Item) on a stand-in room: dark dithered stone drawn to the fixed
// room geometry (art/standin_room.py). The stand-in is not in the normal
// build and is not the game's room art.
//
//   CHAR_WALK      an empty room: the adventurer walks a rectangle (right,
//                  away, left, towards), stopping at each corner; the spider
//                  scuttles, the bat circles
//   CHAR_SKELETON  \
//   CHAR_GHOUL      } a guarded room: the guardian patrols while the adventurer
//   CHAR_WRAITH     } walks the same rectangle around it, passing in front of
//   CHAR_SPIDER    /  it and behind it (depth sorting)
//   CHAR_TREASURE  \
//   CHAR_SWORD      } a room with that pickup on the floor
//   CHAR_AXE       /
//   CHAR_POSES     standing, striking, hit (white flash), slain; about 1.3 s each
//   CHAR_PARADE    every frame of both sheets, eight at a time, a new page
//                  every 2.5 s (6 pages)
static class CharScenes
{
#if SCENE_CHAR_SKELETON
    const uint RoomX = 5, RoomY = 3;
#elif SCENE_CHAR_GHOUL
    const uint RoomX = 9, RoomY = 5;
#elif SCENE_CHAR_WRAITH
    const uint RoomX = 1, RoomY = 8;
#elif SCENE_CHAR_SPIDER
    const uint RoomX = 7, RoomY = 9;
#elif SCENE_CHAR_TREASURE
    const uint RoomX = 7, RoomY = 3;
#elif SCENE_CHAR_SWORD
    const uint RoomX = 3, RoomY = 1;
#elif SCENE_CHAR_AXE
    const uint RoomX = 5, RoomY = 7;
#else
    const uint RoomX = 1, RoomY = 1;
#endif

    public static void Run()
    {
        C64.Screen.SetBorderColor(Colors.Black);
        C64.Screen.SetBackgroundColor(Colors.Black);
        C64.Screen.SetScreenMode(ScreenMode.MultiColor);
        C64.Screen.BeginUpdate();
        C64.Screen.ClearBitmap();
        C64.Screen.DrawImage(0, 0, C64Address.FromLabel("img_StandIn"));
        C64.Screen.EndUpdate();

        SpriteStage.Init();
#if SCENE_CHAR_PARADE
        Parade();
#else
        Spider.Init();
        Monster.Init();
        Item.Init();
        Weapon.Init();

        uint x = 28, depth = 22;
        PlayerSprite.Init(RoomProjection.ScreenX(x, depth), RoomProjection.ScreenY(depth));
        uint t = 0;
        for (;;)
        {
            C64.Screen.WaitForVBlank();
#if SCENE_CHAR_TIMING
            C64.Screen.SetBorderColor(Colors.Red);
            Spider.Animate();
            C64.Screen.SetBorderColor(Colors.Green);
            Monster.Animate(RoomX, RoomY);
            C64.Screen.SetBorderColor(Colors.Blue);
            SpriteStage.Update();
            C64.Screen.SetBorderColor(Colors.Yellow);
#else
            Spider.Animate();
            Monster.Animate(RoomX, RoomY);
            SpriteStage.Update();
#endif

            uint leg = t >> 6;
#if SCENE_CHAR_POSES
            if ((t & 63) == 0)
            {
                if (leg == 0)
                    PlayerSprite.SetPosition(RoomProjection.ScreenX(50, 40), RoomProjection.ScreenY(40));
                else if (leg == 1)
                    PlayerSprite.Strike();
                else if (leg == 2)
                    PlayerSprite.Hurt();
                else
                    PlayerSprite.Fall();
            }
#else
            if ((t & 63) < 48)
            {
                if (leg == 0)
                    x = x + 1;
                else if (leg == 1)
                    depth = depth + 1;
                else if (leg == 2)
                    x = x - 1;
                else
                    depth = depth - 1;
                PlayerSprite.SetPosition(RoomProjection.ScreenX(x, depth), RoomProjection.ScreenY(depth));
            }
#endif
            t = t + 1;
#if SCENE_CHAR_TIMING
            C64.Screen.SetBorderColor(Colors.Black);
#endif
        }
#endif
    }

#if SCENE_CHAR_PARADE
    const uint McFrames = 37, Frames = 47;

    // The own colour of every frame, in sheet order (art/characters_art.py).
    static readonly uint[] FrameColor =
    {
        2, 2, 2, 2, 2, 2, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 2,
        15, 15, 15, 5, 5, 5, 14, 14, 14, 4, 4, 4, 7, 15, 15, 2, 2, 2,
        7, 7, 7, 7, 7, 7, 0, 0, 0, 1,
    };
    static readonly uint[] Bit = { 1, 2, 4, 8, 16, 32, 64, 128 };

    static void Parade()
    {
        uint first = 0;
        for (;;)
        {
            uint multi = 0, shown = 0;
            for (uint i = 0; i < 8; i++)
            {
                uint f = first + i;
                if (f < Frames)
                {
                    uint frame = SpriteStage.Hi + f - McFrames;
                    if (f < McFrames)
                    {
                        frame = SpriteStage.Mc + f;
                        multi = multi | Bit[i];
                    }
                    shown = shown | Bit[i];
                    ulong column = i & 3;
                    uint row = 190;
                    if (i > 3)
                        row = 160;
                    C64.Sprites[i].Place(84 + column * 56, row, frame, (Colors)FrameColor[f]);
                }
            }
            C64.Sprites.MultiColorMask = multi;
            C64.Sprites.VisibleMask = shown;

            for (uint w = 0; w < 125; w++)
                C64.Screen.WaitForVBlank();
            first = first + 8;
            if (first >= Frames)
                first = 0;
        }
    }
#endif
}
#endif
