using C64Lib;

namespace Catacombs;

// Assigns the 3 moving creatures (player + 2 spiders) to physical hardware
// sprites 0/1/2 fresh EVERY frame, nearest-first. VIC-II's sprite-to-sprite
// display priority is fixed by sprite NUMBER -- sprite 0 always draws in
// front of sprite 1, which always draws in front of sprite 2, wherever
// their pixels overlap -- and there's no runtime register to reorder that.
// So the only way to keep "whichever creature is actually closer to the
// viewer draws in front" correct as positions change is to physically move
// each creature's whole hardware state (position, color, multicolor/expand
// flags, pose pointer) into whichever slot its current depth rank calls
// for. Program.cs's own UpdateZOrder does the near/far ranking; this just
// does the writing, once per creature per frame.
//
// A "Sprite" (C64Lib.Sprite) isn't a real object at runtime -- see
// SpriteCollection_get_Sprite0..7 in asm/c64sprite.asm, each of which just
// pushes a literal 0..7: it's a plain sprite-number index that every
// Sprite_set_* routine indexes hardware registers with (`sta spriteX,x`
// etc.), so picking the sprite dynamically (SlotSprite below) by branching
// on a runtime slot number works exactly like picking it by name.
static class SpriteStage
{
    static ulong pointers_;

    public static void Init()
    {
        pointers_ = C64Address.FromLabel("Graphics_ColorMatrix") + 0x3F8UL;
    }

    public static void Place(uint slot, ulong x, uint rawY, uint pointerBlock, Colors color, bool multiColor, bool expandX, bool expandY)
    {
        Sprite s = SlotSprite(slot);
        s.MultiColor = multiColor;
        s.ExpandX = expandX;
        s.ExpandY = expandY;
        s.Color = color;
        s.X = x;
        s.Y = rawY;
        s.Visible = true;
        Poke(pointers_ + slot, pointerBlock);
    }

    static Sprite SlotSprite(uint slot)
    {
        if (slot == 0)
            return C64.Sprites.Sprite0;
        if (slot == 1)
            return C64.Sprites.Sprite1;
        return C64.Sprites.Sprite2;
    }

    // FillMemory writes offsets 1..size of its target (its loop never
    // touches offset 0), so writing one byte at `address` means filling one
    // byte starting one below it -- same trick PlayerSprite/Spider used
    // for this before this file centralized it.
    static void Poke(ulong address, uint value)
    {
        C64.FillMemory(address - 1UL, value, 1);
    }
}
