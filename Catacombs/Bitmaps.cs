using C64Lib;

// A single 8x16 torch (a flickering flame over a handle -- see Torch.aas,
// hand-authored at 2px-pair resolution so it renders cleanly in this
// project's MultiColor bitmap mode, same reasoning as RawBitmapAttribute's
// own comment on multicolor pixel-pair alignment). DungeonView.cs mounts
// two of these on every room's far wall, flanking the north door.
[assembly: RawBitmap(Order = 1, Resource = "Catacombs.Torch.aas", X = 0, Y = 0, Width = 8, Height = 16, Label = "Torch")]
