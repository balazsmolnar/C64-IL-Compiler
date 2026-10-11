using C64Lib;

#if SCENE_HUDFULL || SCENE_HUDMID || SCENE_HUDLOW
// Stand-in viewport for the panel's own scene builds only (never in the game).
[assembly: RawImage(Resource = "techdemo_room.img", Label = "HudBackdrop")]
#endif

namespace Catacombs;

// The status panel: cell rows 21-24, shown hi-res under a raster split (the
// viewport above stays multicolour). Art and layout: art/hud_art.py, which
// also generates HudArt.cs (the panel picture and the tiles copied into it).
//
//   Hud.Init()            once, after SetScreenMode: turns the split on
//   Hud.Render()          draws the whole panel from the current state; only
//                         needed after something else wiped rows 21-24
//   Hud.SetHealth(hp)     0..100, five hearts in half-heart steps
//   Hud.SetTreasure(n)    treasures collected (of Item.Count)
//   Hud.SetWeapon(damage) weapon icon and attack power
//   Hud.Visit(x, y)       the player entered maze cell (x, y): reveals it on
//                         the minimap and moves the blinking marker
//   Hud.Tick()            once a frame: marker blink, low-health alarm
//
// The setters redraw only their own few cells, by copying 8-byte tiles
// straight over the bitmap, so nothing flickers and nothing is erased first.
static class Hud
{
    const uint LowHealth = 20;

    static uint hp_, treasure_, damage_, tick_;

    public static void Init()
    {
        Minimap.Init();
        C64.Screen.SetSplit(21, ScreenMode.Bitmap);
    }

    public static void Render()
    {
        C64.Screen.DrawImage(0, 21, C64Address.FromLabel("img_HudPanel"));
        DrawHealth();
        DrawTreasure();
        Digit(15 * 8, 10);          // "/"
        Digit(16 * 8, Item.Count);
        DrawWeapon();
        Minimap.Render();
    }

    public static void SetHealth(uint hp)
    {
        hp_ = hp;
        DrawHealth();
        HeartColours(0xA0, 0x20);
    }

    public static void SetTreasure(uint collected)
    {
        treasure_ = collected;
        DrawTreasure();
    }

    public static void SetWeapon(uint damage)
    {
        damage_ = damage;
        DrawWeapon();
    }

    public static void Visit(uint x, uint y)
    {
        Minimap.Enter(x, y);
    }

    public static void Tick()
    {
        tick_ = tick_ + 1;
        if ((tick_ & 15) != 0)
            return;
        Minimap.Blink();
        if (hp_ <= LowHealth)
        {
            // Alarm: the hearts flare white-hot and back.
            if (Minimap.Lit)
                HeartColours(0xA0, 0x20);
            else
                HeartColours(0x10, 0xA0);
        }
    }

    // The heart cells' colours (matrix bytes): top cell row, bottom cell row.
    static void HeartColours(uint top, uint bottom)
    {
        C64.FillMemory(C64Address.FromLabel("Graphics_ColorMatrix+880"), top, 10);
        C64.FillMemory(C64Address.FromLabel("Graphics_ColorMatrix+920"), bottom, 10);
    }

    // Copies a two-cell-row tile (its top row's bytes, then its bottom
    // row's) to cell rows 22 and 23, `offset` bytes (8 a cell) from the
    // left. Addresses are one low: CopyMemory copies bytes 1..size.
    static void Put(uint offset, ulong tile, uint size)
    {
        ulong dest = C64Address.FromLabel("Graphics_Bitmap+7039") + offset;
        C64.CopyMemory(dest, tile, size);
        C64.CopyMemory(dest + 320, tile + size, size);
    }

    static void Digit(uint offset, uint digit)
    {
        Put(offset, C64Address.FromLabel("hud_Digits-1") + digit * 16, 8);
    }

    static void DrawHealth()
    {
        uint threshold = 0;
        for (uint half = 0; half < 10; half++)
        {
            uint tile = (half & 1) * 16;
            if (hp_ <= threshold)
                tile = tile + 32;
            Put(8 + half * 8, C64Address.FromLabel("hud_Hearts-1") + tile, 8);
            threshold = threshold + 10;
        }
    }

    static void DrawTreasure()
    {
        Digit(14 * 8, treasure_);
    }

    static void DrawWeapon()
    {
        uint icon = 0;
        if (damage_ >= 12)
            icon = 32;
        if (damage_ >= 20)
            icon = 64;
        Put(18 * 8, C64Address.FromLabel("hud_Weapons-1") + icon, 16);

        uint tens = 0;
        uint ones = damage_;
        while (ones >= 10)
        {
            ones = ones - 10;
            tens = tens + 1;
        }
        Digit(20 * 8, tens);
        Digit(21 * 8, ones);
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
        Hud.Visit(1, 1);
#elif SCENE_HUDMID
        Hud.SetHealth(55);
        Hud.SetTreasure(2);
        Hud.SetWeapon(12);
        Explore(5);
        Hud.Visit(5, 5);
#else
        Hud.SetHealth(15);
        Hud.SetTreasure(3);
        Hud.SetWeapon(20);
        Explore(9);
        Hud.Visit(9, 9);
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
                    Hud.Visit(x, y);
    }
}
#endif
