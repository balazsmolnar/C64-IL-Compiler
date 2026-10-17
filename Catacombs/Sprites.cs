using C64Lib;

// Sprite frames. Every sheet is a .spr file under art/build/ (made from a
// PNG by tools/c64spr.py, or by a script next to it) embedded by the
// .csproj and declared here; the compiler puts the frames where the VIC can
// see them in bitmap mode (see TECH.md, "Sprites") -- no addresses to pick.
//
// A sheet's frames are consecutive sprite pointer values starting at
// (uint)C64Address.FromLabel("frm_<Label>").
//
// "Chars" (multicolour) and "CharsHi" (hi-res): everything that walks,
// crawls, flies or lies on the floor. Drawn by art/characters_art.py, which
// prints the frame order the numbers in Catacombs.Frame below repeat.
[assembly: RawSprites(Order = 0, Resource = "chars_mc.spr", Label = "Chars")]
[assembly: RawSprites(Order = 1, Resource = "chars_hi.spr", Label = "CharsHi")]


#if SCENE_TECHDEMO
[assembly: RawSprites(Order = 2, Resource = "techdemo_coin.spr", Label = "TechCoin")]
[assembly: RawSprites(Order = 3, Resource = "techdemo_walker.spr", Label = "TechWalker")]
[assembly: RawImage(Resource = "techdemo_room.img", Label = "TechRoom")]
[assembly: RawImage(Resource = "techdemo_chest.img", Label = "TechChest")]
[assembly: RawImage(Resource = "techdemo_panel.img", Label = "TechPanel")]
#endif

namespace Catacombs
{
    // Frame numbers within the two sheets (add SpriteStage.Mc / SpriteStage.Hi).
    static class Frame
    {
        // Chars (multicolour; shared colours black and orange)
        public const uint Top = 0;          // + facing: down, up, left, right
        public const uint TopAttack = 4;
        public const uint Legs = 5;         // down/up 5-8, left 9-12, right 13-16
        public const uint Fallen = 17;
        public const uint Guardian = 18;    // + 3 * kind: top, legs, legs (the spider: left, left, right, right)
        public const uint Treasure = 31;
        public const uint Sword = 32;       // then the axe

        // CharsHi (hi-res, one colour)
        public const uint Overlay = 0;      // + facing; 4 = attack
        public const uint Crawler = 5;      // two running stances, then at rest
        public const uint Flyer = 8;        // wings up, level, down
        public const uint Spark = 11;       // small, large
    }
}
