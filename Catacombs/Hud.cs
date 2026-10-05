using C64Lib;

namespace Catacombs;

// Persistent exploration-time status display, top-left corner -- distinct
// from Monster.DrawHealthBars, which only appears during Combat() (and
// occupies roughly the same corner, but the two are never on screen at
// the same time: Combat() does its own ClearBitmap+redraw each turn,
// which simply doesn't include this). Redrawn once per room entry
// (EnterRoom, as part of its usual full redraw) and again immediately
// after an Item/Weapon pickup -- there's no event system to redraw
// automatically, so callers that mutate collection state redraw this
// themselves, same pattern as UpdateSpritePosition being called
// explicitly after a move.
static class Hud
{
    // 3 small squares (Item.Count, matching ColorRam -- the same color
    // Item's own in-room marker uses): outline-only = not yet collected,
    // filled = collected.
    const ulong PipSize = 10, PipGap = 4;
    const ulong PipX = 10, PipY = 8;

    // A thin proportional bar, same "outline + proportional fill" shape
    // as Monster.DrawHealthBars, showing current attack power out of the
    // strongest weapon's damage (Weapon.cs's own axe, 20) -- never
    // literally empty, since unarmed is still 6.
    const ulong BarX = 10, BarY = 24, BarWidth = 60, BarHeight = 6;
    const uint MaxWeaponDamage = 20;

    public static void Render(uint collectedItems, uint playerDamage)
    {
        for (uint i = 0; i < Item.Count; i++)
        {
            ulong wide = i; // widen before multiplying -- same reasoning as dmg below
            ulong x = PipX + wide * (PipSize + PipGap);
            bool collected = i < collectedItems;
            C64.Screen.DrawRectangle(x, PipY, x + PipSize, PipY + PipSize, collected, true, BitmapColorSource.ColorRam);
        }

        C64.Screen.DrawRectangle(BarX, BarY, BarX + BarWidth, BarY + BarHeight, false, true, BitmapColorSource.MatrixHigh);
        ulong dmg = playerDamage; // widen before multiplying -- dmg*BarWidth can exceed an 8-bit uint (see Monster.DrawHealthBars's own identical comment)
        ulong fill = (dmg * BarWidth) / MaxWeaponDamage;
        if (fill > 0)
            C64.Screen.DrawRectangle(BarX, BarY, BarX + fill, BarY + BarHeight, true, true, BitmapColorSource.MatrixLow);
    }
}
