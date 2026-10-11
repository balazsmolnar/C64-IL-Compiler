using C64Lib;

// Sprite frames. Every sheet is a .spr file under art/build/ (made from a
// PNG by tools/c64spr.py, or by a script next to it) embedded by the
// .csproj and declared here; the compiler puts the frames where the VIC can
// see them in bitmap mode (see TECH.md, "Sprites") -- no addresses to pick.
//
// A sheet's frames are consecutive sprite pointer values starting at
// (uint)C64Address.FromLabel("frm_<Label>").
//
// "Legacy": the prototype's three placeholder frames (art/legacy_sprites.py):
// 0 = spider (hi-res), 1 and 2 = the player's two walk frames (multicolour,
// shared colours blue/yellow -- see PlayerSprite.cs).
[assembly: RawSprites(Order = 0, Resource = "legacy.spr", Label = "Legacy")]

#if SCENE_TECHDEMO
[assembly: RawSprites(Order = 1, Resource = "techdemo_coin.spr", Label = "TechCoin")]
[assembly: RawSprites(Order = 2, Resource = "techdemo_walker.spr", Label = "TechWalker")]
[assembly: RawImage(Resource = "techdemo_room.img", Label = "TechRoom")]
[assembly: RawImage(Resource = "techdemo_chest.img", Label = "TechChest")]
[assembly: RawImage(Resource = "techdemo_panel.img", Label = "TechPanel")]
#endif
