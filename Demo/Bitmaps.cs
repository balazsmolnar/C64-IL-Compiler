using C64Lib;

// A single 16x16 hollow-square ("ring") stencil bitmap, hand-built as a
// minimal .aas fixture (TestDecoration.aas -- see git history for the
// one-off generator) rather than drawn in a real art tool: there's no
// existing .aas asset anywhere in this repo to reuse, and the exact shape
// doesn't matter here, only that it has an unmistakable transparent hole
// in the middle to prove Screen.DrawBitmap's "off" pixels really do leave
// the existing bitmap content untouched (see Program.cs's own call site,
// placed deliberately over the existing solid white rectangle).
[assembly: RawBitmap(Order = 2, Resource = "Demo.TestDecoration.aas", X = 0, Y = 0, Width = 16, Height = 16, Label = "Ring")]
