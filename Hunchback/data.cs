using C64Lib;

[assembly: RawAssembly(Order = 0, Resource = "Hunchback.CharSet.asm")]
// SpriteData.asm (Order 1) carries the #align_vic_safe 64 directive plus
// spt_heart -- it must come before every RawSprite import (SpriteImport.cs,
// Order 2) so the align directive lands before any sprite bytes, hand-
// authored or imported. GetCustomAttributes doesn't guarantee source
// order for same-Order entries across different files, so this can't rely
// on both being Order 1 even though declaration order happens to work
// today -- distinct Order values make it correct regardless.
[assembly: RawAssembly(Order = 1, Resource = "Hunchback.SpriteData.asm")]
[assembly: RawAssembly(Order = 3, Resource = "Hunchback.GameData.asm")]
