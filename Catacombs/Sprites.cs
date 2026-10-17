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

#if SCENE_CHARS
// The stand-in room the character scenes are judged on (CharScenes.cs).
[assembly: RawImage(Resource = "standin_room.img", Label = "StandIn")]
#endif

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
        // Chars (multicolour; shared colours black and brown)
        public const uint Top = 0;          // + facing: down, up, left, right
        public const uint TopRaise = 4;     // striking: wind-up, then the slash
        public const uint TopSlash = 5;
        public const uint Legs = 6;         // down/up 6-9, left 10-13, right 14-17
        public const uint Fallen = 18;
        public const uint Guardian = 19;    // + 3 * kind: top, legs, legs
        public const uint Treasure = 31;
        public const uint Sword = 32;       // then the axe
        public const uint Flyer = 34;       // wings up, level, down

        // CharsHi (hi-res, one colour)
        public const uint Overlay = 0;      // + facing; 4 raise, 5 slash
        public const uint Crawler = 6;      // two running stances, then at rest
        public const uint Spark = 9;
    }
}
