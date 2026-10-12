using C64Lib;

#if SCENE_HUDFULL || SCENE_HUDMID || SCENE_HUDLOW
// Stand-in viewport for the panel's own scene builds only (never in the game).
[assembly: RawImage(Resource = "techdemo_room.img", Label = "HudBackdrop")]
#endif

namespace Catacombs;

// The status panel: cell rows 21-24, shown hi-res under a raster split (the
// viewport above stays multicolour). Art and layout: art/hud_art.py, which
// also generates HudArt.cs (the panel picture, the tiles drawn into it and
// the minimap's cells).
//
//   Hud.Init()            once, after SetScreenMode: turns the split on
//   Hud.Render()          draws the whole panel from the current state; only
//                         needed after something else wiped rows 21-24
//   Hud.SetHealth(hp)     0..100, five hearts in half-heart steps
//   Hud.SetTreasure(n)    treasures collected (of Item.Count)
//   Hud.SetWeapon(damage) weapon icon and attack power
//   Minimap.Enter(x, y)   the player entered maze cell (x, y): reveals it on
//                         the minimap and moves the blinking marker
//   Hud.Tick()            once a frame: marker blink, low-health alarm
//
// The setters redraw only their own cells, by copying 8-byte tiles over the
// bitmap (Screen.DrawTiles): nothing is erased first, so nothing flickers.
// The cells' colours come with the panel picture and are never rewritten.
static class Hud
{
    const uint LowHealth = 20;

    // Tile numbers in hud_Tiles (see art/hud_art.py): every picture is its
    // top cell row, then its bottom cell row.
    const uint HeartTiles = 0;      // full left, full right, empty left, empty right: 2 tiles each
    const uint DigitTiles = 8;      // 0-9 and "/": 2 tiles each
    const uint WeaponTiles = 30;    // fist, sword, axe: 4 tiles each

    static uint hp_, treasure_, damage_, tick_;

    public static void Init()
    {
        C64.Screen.SetSplit(21, ScreenMode.Bitmap);
    }

    public static void Render()
    {
        C64.Screen.DrawImage(0, 21, C64Address.FromLabel("img_HudPanel"));
        Hearts(hp_);
        SetTreasure(treasure_);
        Digit(15, 10);              // "/"
        Digit(16, Item.Count);
        SetWeapon(damage_);
        Minimap.Draw();
    }

    public static void SetHealth(uint hp)
    {
        hp_ = hp;
        Hearts(hp);
    }

    public static void SetTreasure(uint collected)
    {
        treasure_ = collected;
        Digit(14, collected);
    }

    public static void SetWeapon(uint damage)
    {
        damage_ = damage;
        uint icon = WeaponTiles;
        if (damage >= 12)
            icon = WeaponTiles + 4;
        if (damage >= 20)
            icon = WeaponTiles + 8;
        Tile(18, 2, icon);

        uint tens = 0;
        while (damage >= 10)
        {
            damage = damage - 10;
            tens = tens + 1;
        }
        Digit(20, tens);
        Digit(21, damage);
    }

    public static void Tick()
    {
        tick_ = tick_ + 1;
        if ((tick_ & 15) != 0)
            return;
        Minimap.Flip();
        // Alarm: what is left of the hearts throbs between full and hollow.
        if (hp_ <= LowHealth)
            Hearts(Minimap.Hidden ? 0 : hp_);
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

    static void Hearts(uint hp)
    {
        uint threshold = 0;
        for (uint half = 0; half < 10; half++)
        {
            uint tile = (half & 1) * 2;
            if (hp <= threshold)
                tile = tile + 4;
            Tile(1 + half, 1, tile);
            threshold = threshold + 10;
        }
    }
}

#if SCENE_HUDFULL || SCENE_HUDMID || SCENE_HUDLOW
// "dotnet build -p:Scene=HUDFULL|HUDMID|HUDLOW": the panel in one fixed
// state over a stand-in backdrop, for judging it without playing.
static class HudScene
{
    public static void Run()
    {
        C64.Screen.SetBorderColor(Colors.Black);
        C64.Screen.SetBackgroundColor(Colors.Black);
        C64.Screen.SetScreenMode(ScreenMode.MultiColor);
        C64.Screen.BeginUpdate();
        C64.Screen.DrawImage(0, 0, C64Address.FromLabel("img_HudBackdrop"));
        Hud.Init();
#if SCENE_HUDFULL
        Hud.SetHealth(100);
        Hud.SetTreasure(0);
        Hud.SetWeapon(6);
        Minimap.Enter(1, 1);
#elif SCENE_HUDMID
        Hud.SetHealth(55);
        Hud.SetTreasure(2);
        Hud.SetWeapon(12);
        Explore(5);
        Minimap.Enter(5, 5);
#else
        Hud.SetHealth(15);
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
