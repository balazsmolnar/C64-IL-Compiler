using C64Lib;


namespace Catacombs;

// The status panel: cell rows 21-24, multicolour like the viewport (black
// plus three colours per cell). Art and layout: art/hud_art.py, which also
// generates HudArt.cs (the panel picture and the tiles drawn into it).
//
//   Hud.Init()            at the start of every run: forgets the last run
//   Hud.Render()          draws the whole panel from the current state; only
//                         needed after something else drew over rows 21-24
//   Hud.SetHealth(hp)     0..100, five hearts in half-heart steps
//   Hud.SetTreasure(n)    treasures collected (of Item.Count)
//   Hud.SetWeapon(damage) weapon icon and attack power
//   Hud.MonsterSlain()    one more of the maze's guardians is dead
//   Minimap.Enter(x, y)   the player entered maze cell (x, y): reveals it on
//                         the minimap and moves the "you are here" mark
//   Hud.Tick()            once a frame: the mark's blink, the heartbeat
//
// The setters redraw only their own cells, by copying 8-byte tiles over the
// bitmap (Screen.DrawTiles): nothing is erased first, so nothing flickers.
// The cells' colours come with the panel picture; only the heartbeat
// changes some (the hearts' body colour). Tick does almost nothing on most
// frames: it only draws when the blink or the beat changes phase.
static class Hud
{
    const uint LowHealth = 20;

    // Tile numbers in hud_Tiles (see art/hud_art.py): every picture is its
    // top cell row, then its bottom cell row.
    const uint DigitTiles = 8;      // "0"-"9", "/", blank: 2 tiles each
    const uint Blank = 11;
    const uint WeaponTiles = 32;    // fist, sword, axe: 4 tiles each

    // Heart cells' video matrix byte: rim brown (high nibble), body (low nibble).
    const uint HeartBody = 0x92;    // red
    const uint HeartBeat = 0x9A;    // light red

    static uint hp_, treasure_, damage_, kills_, tick_, body_;

    public static void Init()
    {
        kills_ = 0;
        Minimap.Init();
#if SCENE_TIMING || SCENE_TIMINGM
        C64.Screen.SetSplit(21, ScreenMode.Bitmap); // the timing readout is hi-res text
#endif
    }

    public static void Render()
    {
        C64.Screen.DrawImage(0, 21, C64Address.FromLabel("img_HudPanel"));
        SetHealth(hp_);
        SetTreasure(treasure_);     // ("/3" and "/4" are in the picture)
        SetWeapon(damage_);
        Digit(25, kills_);
        Minimap.Draw();
    }

    public static void SetHealth(uint hp)
    {
        hp_ = hp;
        // Written with "<": see the note on unsigned "<=" in asm/helper/branch.asm's history.
        uint threshold = 1;
        for (uint half = 0; half < 10; half++)
        {
            uint tile = (half & 1) * 2;
            if (hp < threshold)
                tile = tile + 4;
            Tile(1 + half, 1, tile);
            threshold = threshold + 10;
        }
        Beat(HeartBody);
    }

    public static void SetTreasure(uint collected)
    {
        treasure_ = collected;
        Digit(14, collected);
    }

    public static void SetWeapon(uint damage)
    {
        // Fist below 10, sword 10-19, axe from 20 (the weapons are 6, 12
        // and 20): each ten is a tens digit and a better icon.
        damage_ = damage;
        uint icon = WeaponTiles;
        uint tens = Blank;
        if (damage >= 10)
        {
            damage = damage - 10;
            icon = WeaponTiles + 4;
            tens = 1;
        }
        if (damage >= 10)
        {
            damage = damage - 10;
            icon = WeaponTiles + 8;
            tens = 2;
        }
        Tile(18, 2, icon);
        Digit(20, tens);
        Digit(21, damage);
    }

    public static void MonsterSlain()
    {
        kills_ = kills_ + 1;
        Digit(25, kills_);
    }

    public static void Tick()
    {
        tick_ = tick_ + 1;
        if ((tick_ & 15) == 0)
            Minimap.Flip();
        if (hp_ < LowHealth + 1)
        {
            // Heartbeat: lub (4 frames), 4, dub (4), then a rest -- 64
            // frames a beat, 32 below 10 health.
            uint b = tick_ & 63;
            if (hp_ < 10)
                b = b & 31;
            uint body = HeartBody;
            if (b < 12 && (b & 4) == 0)
                body = HeartBeat;
            Beat(body);
        }
    }

    // The hearts' body colour, rewritten only when it changes.
    static void Beat(uint body)
    {
        if (body == body_)
            return;
        body_ = body;
        C64.FillMemory(C64Address.FromLabel("Graphics_ColorMatrix+880"), body, 10);  // writes bytes 1..10: columns 1-10
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
        Hud.Init();
#if SCENE_HUDFULL
        Hud.SetHealth(100);
        Hud.SetTreasure(0);
        Hud.SetWeapon(6);
        Minimap.Enter(1, 1);
#elif SCENE_HUDMID
        Hud.SetHealth(60);
        Hud.SetTreasure(2);
        Hud.SetWeapon(12);
        Hud.MonsterSlain();
        Explore(5);
        Minimap.Enter(5, 5);
#elif SCENE_HUDHALF
        Hud.SetHealth(45);          // two and a half hearts
        Hud.SetTreasure(1);
        Hud.SetWeapon(12);
        Hud.MonsterSlain();
        Hud.MonsterSlain();
        Explore(7);
        Minimap.Enter(7, 7);
#else
        Hud.SetHealth(8);
        Hud.SetTreasure(3);
        Hud.SetWeapon(20);
        Hud.MonsterSlain();
        Hud.MonsterSlain();
        Hud.MonsterSlain();
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
