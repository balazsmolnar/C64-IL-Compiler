using C64Lib;


namespace Catacombs;

// The status panel: cell rows 21-24, multicolour like the viewport (black
// plus three colours per cell). Art and layout: art/hud_art.py, which also
// generates HudArt.cs (the panel picture and the tiles drawn into it).
//
//   Minimap.Init()        at the start of every run: forgets the last run's map
//   Hud.Render()          draws the whole panel from the current state; only
//                         needed after something else drew over rows 21-24
//   Hud.SetHealth(hp)     0..100, five hearts in half-heart steps
//   Hud.SetTreasure(n)    treasures collected (of Item.Count)
//   Hud.SetWeapon(damage) weapon icon and attack power
//   Minimap.Enter(x, y)   the player entered maze cell (x, y): reveals it on
//                         the minimap and moves the "you are here" mark
//   Hud.Tick()            once a frame: the mark's blink, the heartbeat
//
// The setters redraw only their own cells, by copying 8-byte tiles over the
// bitmap (Screen.DrawTiles): nothing is erased first, so nothing flickers.
// The cells' colours come with the panel picture; only the heartbeat
// changes some (the hearts' body colour).
static class Hud
{
    const uint LowHealth = 20;

    // Tile numbers in hud_Tiles (see art/hud_art.py): every picture is its
    // top cell row, then its bottom cell row.
    const uint DigitTiles = 8;      // "0"-"9": 2 tiles each
    const uint WeaponTiles = 28;    // fist, sword, axe: 4 tiles each

    // Heart cells' video matrix byte: rim brown (high nibble), body (low nibble).
    const uint HeartBody = 0x92;    // red
    const uint HeartBeat = 0x9A;    // light red

    static uint hp_, treasure_, damage_, tick_;

    public static void Render()
    {
        C64.Screen.DrawImage(0, 21, C64Address.FromLabel("img_HudPanel"));
        SetHealth(hp_);
        SetTreasure(treasure_);     // ("/3" is in the picture)
        SetWeapon(damage_);
        C64.Screen.DrawTiles(31, 22, 6, 3, C64Address.FromLabel("hud_Map"), 0); // the minimap
    }

    public static void SetHealth(uint hp)
    {
        hp_ = hp;
        uint threshold = 1;
        for (uint half = 0; half < 10; half++)
        {
            uint tile = (half & 1) * 2;
            if (hp < threshold)
                tile = tile + 4;
            Tile(1 + half, 1, tile);
            threshold = threshold + 10;
        }
        // (No colour reset: health only goes up at a new run, whose
        // Render redraws the panel's colours.)
    }

    public static void SetTreasure(uint collected)
    {
        treasure_ = collected;
        Digit(14, collected);
    }

    public static void SetWeapon(uint damage)
    {
        // Fist below 10, sword 10-19, axe 20-29 (the weapons are 6, 12
        // and 20): the tens digit picks the icon.
        damage_ = damage;
        // Below 10 the tens cell keeps the picture's blank: power only ever
        // drops at a new run, which redraws the whole panel.
        uint tens = damage / 10;
        Tile(18, 2, WeaponTiles + tens * 4);
        if (tens != 0)
            Digit(20, tens);
        Digit(21, damage % 10);
    }

    public static void Tick()
    {
        // Most frames this is a counter and two tests: something is only
        // drawn when the blink or the beat changes.
        tick_ = tick_ + 1;
        uint t = tick_ & 31;
        if (t == 16)
            Minimap.Flip();         // "you are here": bright / dim, every 32 frames
        // Heartbeat, once every 32 frames: lub (frames 0-3), dub (8-11).
        // (t & 0x13) == 0 on frames 0, 4, 8 and 12: the four changes.
        if (hp_ < LowHealth + 1 && (t & 0x13) == 0)
            Beat((t & 4) == 0 ? HeartBeat : HeartBody);
    }

    // The hearts' body colour (columns 1-10 of rows 22 and 23).
    static void Beat(uint body)
    {
        C64.FillMemory(C64Address.FromLabel("Graphics_ColorMatrix+880"), body, 10); // writes bytes 1..10
        C64.FillMemory(C64Address.FromLabel("Graphics_ColorMatrix+920"), body, 10);
    }

    // A picture two cells tall (rows 22-23), `width` cells wide, at `column`.
    static void Tile(uint column, uint width, uint index)
    {
        C64.Screen.DrawTiles(column, 22, width, 2, C64Address.FromLabel("hud_Tiles"), index);
    }

    static void Digit(uint column, uint digit)
    {
        Tile(column, 1, DigitTiles + digit * 2);
    }
}

#if SCENE_HUDFULL || SCENE_HUDMID || SCENE_HUDLOW || SCENE_HUDHALF
// "dotnet build -p:Scene=HUDFULL|HUDMID|HUDLOW|HUDHALF": the panel in one
// fixed state over the start room, for judging it without playing.
static class HudScene
{
    public static void Run()
    {
        C64.Screen.SetBorderColor(Colors.Black);
        C64.Screen.SetBackgroundColor(Colors.Black);
        C64.Screen.SetScreenMode(ScreenMode.MultiColor);
        C64.Screen.BeginUpdate();
        DungeonView.Render(1, 1); // the real room (the stand-in no longer fits next to the whole game)
        Minimap.Init();
#if SCENE_HUDFULL
        Hud.SetHealth(100);
        Hud.SetTreasure(0);
        Hud.SetWeapon(6);
        Minimap.Enter(1, 1);
#elif SCENE_HUDMID
        Hud.SetHealth(60);
        Hud.SetTreasure(2);
        Hud.SetWeapon(12);
        Explore(5);
        Minimap.Enter(5, 5);
#elif SCENE_HUDHALF
        Hud.SetHealth(45);          // two and a half hearts
        Hud.SetTreasure(1);
        Hud.SetWeapon(12);
        Explore(7);
        Minimap.Enter(7, 7);
#else
        Hud.SetHealth(8);
        Hud.SetTreasure(3);
        Hud.SetWeapon(20);
        Explore(9);
        Minimap.Enter(9, 9);
#endif
        Hud.Render();
        C64.Screen.EndUpdate();
        for (;;)
        {
            C64.Screen.WaitForVBlank();
            Hud.Tick();
        }
    }

    static void Explore(uint rows)
    {
        for (uint y = 1; y <= rows; y++)
            for (uint x = 1; x <= 10; x++)
                if (!Maze.IsWall(x, y))
                    Minimap.Enter(x, y);
    }
}
#endif
