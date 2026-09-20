using C64Lib;

// Every sprite this port uses except spt_heart (which has no original --
// see SpriteData.asm's own comment on it) now comes straight from the
// original game's own SpritePad project file (sprHunchback.spt, copied
// unmodified from the disassembly reference at
// c:\temp\HunchBack\Restructure -- see RawSpriteAttribute/
// ILRawAssemblyPass), instead of hand-transcribed .byte literals.
//
// The Index values below aren't guesses -- SpriteData.asm's previous
// hand-authored bytes were themselves transcribed from this exact file
// (byte-for-byte, confirmed by direct comparison: matched all 57 of the
// 58 spt_ labels that existed before this change, only spt_heart had no
// match), so each Index here is simply where that same sprite already
// lived in the project file. The file's own sprite indices aren't
// contiguous per animation (e.g. player_right_3 is index 43 but
// player_jump_right_0 is index 44, arrow_right is 48 but fireball_0 is
// 50, not 49) -- the gaps (indices 32-35, 49) are other original frames
// this port doesn't use (alternate knight/fireball frames), left
// unimported rather than guessed at.
[assembly: RawSprite(Order = 2, Resource = "Hunchback.sprHunchback.spt", Index = 0, Label = "rope_0")]
[assembly: RawSprite(Order = 2, Resource = "Hunchback.sprHunchback.spt", Index = 1, Label = "rope_1")]
[assembly: RawSprite(Order = 2, Resource = "Hunchback.sprHunchback.spt", Index = 2, Label = "rope_2")]
[assembly: RawSprite(Order = 2, Resource = "Hunchback.sprHunchback.spt", Index = 3, Label = "rope_3")]
[assembly: RawSprite(Order = 2, Resource = "Hunchback.sprHunchback.spt", Index = 4, Label = "rope_4")]
[assembly: RawSprite(Order = 2, Resource = "Hunchback.sprHunchback.spt", Index = 5, Label = "rope_5")]
[assembly: RawSprite(Order = 2, Resource = "Hunchback.sprHunchback.spt", Index = 6, Label = "rope_6")]
[assembly: RawSprite(Order = 2, Resource = "Hunchback.sprHunchback.spt", Index = 7, Label = "rope_7")]
[assembly: RawSprite(Order = 2, Resource = "Hunchback.sprHunchback.spt", Index = 8, Label = "rope_8")]
[assembly: RawSprite(Order = 2, Resource = "Hunchback.sprHunchback.spt", Index = 9, Label = "rope_9")]
[assembly: RawSprite(Order = 2, Resource = "Hunchback.sprHunchback.spt", Index = 10, Label = "rope_10")]
[assembly: RawSprite(Order = 2, Resource = "Hunchback.sprHunchback.spt", Index = 11, Label = "rope_11")]
[assembly: RawSprite(Order = 2, Resource = "Hunchback.sprHunchback.spt", Index = 12, Label = "rope_12")]
[assembly: RawSprite(Order = 2, Resource = "Hunchback.sprHunchback.spt", Index = 13, Label = "rope_13")]
[assembly: RawSprite(Order = 2, Resource = "Hunchback.sprHunchback.spt", Index = 14, Label = "rope_14")]
[assembly: RawSprite(Order = 2, Resource = "Hunchback.sprHunchback.spt", Index = 15, Label = "rope_15")]
[assembly: RawSprite(Order = 2, Resource = "Hunchback.sprHunchback.spt", Index = 16, Label = "rope_16")]
[assembly: RawSprite(Order = 2, Resource = "Hunchback.sprHunchback.spt", Index = 17, Label = "rope_17")]
[assembly: RawSprite(Order = 2, Resource = "Hunchback.sprHunchback.spt", Index = 18, Label = "rope_18")]
[assembly: RawSprite(Order = 2, Resource = "Hunchback.sprHunchback.spt", Index = 19, Label = "rope_19")]
[assembly: RawSprite(Order = 2, Resource = "Hunchback.sprHunchback.spt", Index = 20, Label = "rope_20")]
[assembly: RawSprite(Order = 2, Resource = "Hunchback.sprHunchback.spt", Index = 21, Label = "rope_21")]
[assembly: RawSprite(Order = 2, Resource = "Hunchback.sprHunchback.spt", Index = 22, Label = "rope_22")]
[assembly: RawSprite(Order = 2, Resource = "Hunchback.sprHunchback.spt", Index = 23, Label = "rope_23")]
[assembly: RawSprite(Order = 2, Resource = "Hunchback.sprHunchback.spt", Index = 24, Label = "rope_24")]
[assembly: RawSprite(Order = 2, Resource = "Hunchback.sprHunchback.spt", Index = 25, Label = "rope_25")]
[assembly: RawSprite(Order = 2, Resource = "Hunchback.sprHunchback.spt", Index = 26, Label = "rope_26")]
[assembly: RawSprite(Order = 2, Resource = "Hunchback.sprHunchback.spt", Index = 27, Label = "rope_27")]
[assembly: RawSprite(Order = 2, Resource = "Hunchback.sprHunchback.spt", Index = 28, Label = "rope_28")]
[assembly: RawSprite(Order = 2, Resource = "Hunchback.sprHunchback.spt", Index = 29, Label = "rope_29")]
[assembly: RawSprite(Order = 2, Resource = "Hunchback.sprHunchback.spt", Index = 30, Label = "rope_30")]
[assembly: RawSprite(Order = 2, Resource = "Hunchback.sprHunchback.spt", Index = 31, Label = "rope_31")]
[assembly: RawSprite(Order = 2, Resource = "Hunchback.sprHunchback.spt", Index = 36, Label = "knight_climb_0")]
[assembly: RawSprite(Order = 2, Resource = "Hunchback.sprHunchback.spt", Index = 37, Label = "knight_climb_1")]
[assembly: RawSprite(Order = 2, Resource = "Hunchback.sprHunchback.spt", Index = 38, Label = "knight_walk_0")]
[assembly: RawSprite(Order = 2, Resource = "Hunchback.sprHunchback.spt", Index = 39, Label = "knight_walk_1")]
[assembly: RawSprite(Order = 2, Resource = "Hunchback.sprHunchback.spt", Index = 40, Label = "player_right_0")]
[assembly: RawSprite(Order = 2, Resource = "Hunchback.sprHunchback.spt", Index = 41, Label = "player_right_1")]
[assembly: RawSprite(Order = 2, Resource = "Hunchback.sprHunchback.spt", Index = 42, Label = "player_right_2")]
[assembly: RawSprite(Order = 2, Resource = "Hunchback.sprHunchback.spt", Index = 43, Label = "player_right_3")]
[assembly: RawSprite(Order = 2, Resource = "Hunchback.sprHunchback.spt", Index = 44, Label = "player_jump_right_0")]
[assembly: RawSprite(Order = 2, Resource = "Hunchback.sprHunchback.spt", Index = 45, Label = "player_jump_right_1")]
[assembly: RawSprite(Order = 2, Resource = "Hunchback.sprHunchback.spt", Index = 46, Label = "player_jump_right_2")]
[assembly: RawSprite(Order = 2, Resource = "Hunchback.sprHunchback.spt", Index = 47, Label = "player_rope_right")]
[assembly: RawSprite(Order = 2, Resource = "Hunchback.sprHunchback.spt", Index = 48, Label = "arrow_right")]
[assembly: RawSprite(Order = 2, Resource = "Hunchback.sprHunchback.spt", Index = 50, Label = "fireball_0")]
[assembly: RawSprite(Order = 2, Resource = "Hunchback.sprHunchback.spt", Index = 51, Label = "fireball_1")]
[assembly: RawSprite(Order = 2, Resource = "Hunchback.sprHunchback.spt", Index = 52, Label = "fireball_2")]
[assembly: RawSprite(Order = 2, Resource = "Hunchback.sprHunchback.spt", Index = 53, Label = "fireball_3")]
[assembly: RawSprite(Order = 2, Resource = "Hunchback.sprHunchback.spt", Index = 54, Label = "player_left_0")]
[assembly: RawSprite(Order = 2, Resource = "Hunchback.sprHunchback.spt", Index = 55, Label = "player_left_1")]
[assembly: RawSprite(Order = 2, Resource = "Hunchback.sprHunchback.spt", Index = 56, Label = "player_left_2")]
[assembly: RawSprite(Order = 2, Resource = "Hunchback.sprHunchback.spt", Index = 57, Label = "player_left_3")]
[assembly: RawSprite(Order = 2, Resource = "Hunchback.sprHunchback.spt", Index = 58, Label = "player_jump_left_0")]
[assembly: RawSprite(Order = 2, Resource = "Hunchback.sprHunchback.spt", Index = 59, Label = "player_jump_left_1")]
[assembly: RawSprite(Order = 2, Resource = "Hunchback.sprHunchback.spt", Index = 60, Label = "player_jump_left_2")]
[assembly: RawSprite(Order = 2, Resource = "Hunchback.sprHunchback.spt", Index = 61, Label = "player_rope_left")]
