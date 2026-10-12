namespace C64Lib
{
    public class SpriteCollection
    {
        public Sprite Sprite0 { get; }
        public Sprite Sprite1 { get; }
        public Sprite Sprite2 { get; }
        public Sprite Sprite3 { get; }
        public Sprite Sprite4 { get; }
        public Sprite Sprite5 { get; }
        public Sprite Sprite6 { get; }
        public Sprite Sprite7 { get; }
        public Colors CommonColor1 { get; set; }
        public Colors CommonColor2 { get; set; }
        public uint Collisions { get; }

        // One bit per sprite (bit 0 = Sprite0), all eight at once.
        // MultiColorMask: which sprites are multicolour ($D01C) -- unlike
        // Sprite.MultiColor, which can only switch a sprite's multicolour
        // ON, this can also switch it off.
        // BehindBackgroundMask: which sprites are drawn behind the
        // picture's foreground pixels ($D01B) instead of in front of
        // everything. In multicolour bitmap mode "foreground" means the
        // pixels drawn from BitmapColorSource.MatrixLow and ColorRam (bit
        // pairs 10 and 11); Background and MatrixHigh pixels stay behind
        // the sprite.
        public uint MultiColorMask { set { } }
        public uint BehindBackgroundMask { set { } }

        // Which sprites are shown ($D015), all eight at once; reading it
        // back lets a program change some sprites' bits and keep the rest.
        public uint VisibleMask { get => 0; set { } }

        // Sprite number index (0-7): C64.Sprites[n] is C64.Sprites.SpriteN,
        // for code that picks the sprite at run time.
        public Sprite this[uint index] => null;

        // A character built from several sprites, placed in one call:
        // `count` hardware sprites starting with sprite `first`, as one
        // object at (x, y). Each sprite is four bytes of `parts` from
        // `offset` on: dx (0-255) and dy (wraps: 255 = one line up), added
        // to x and y; the frame (see Sprite.Frame); the colour, plus 128
        // for a multicolour sprite (the last byte of a .spr frame as it
        // is). Sets position, frame, colour and multicolour/hi-res of each;
        // VisibleMask says which sprites are shown.
        public void PlaceGroup(uint first, uint count, ulong x, uint y, uint[] parts, uint offset) { }
    }

    public class Sprite
    {
        public bool Visible { get; set; }
        public bool IsInCollision { get; }
        public bool IsInBackgroundCollision { get; }
        public Colors Color { get; set; }
        public bool MultiColor { get; set; }
        public bool ExpandX { get; set; }
        public bool ExpandY { get; set; }
        public ulong DataBlock { get; set; }

        // The sprite's data pointer: a block number (address / 64 within the
        // VIC's 16K bank). For frames embedded with [assembly: RawSprites],
        // (uint)C64Address.FromLabel("frm_<Label>") is the sheet's first
        // frame and the following frames are +1, +2, ... Cheaper than
        // DataBlock (no address arithmetic), and it works in bitmap mode
        // too, where the pointers are not at the text screen's $07F8.
        public uint Frame { set { } }
        public ulong X { get; set; }
        public uint Y { get; set; }

        // Position, frame (see Frame) and colour in one call: what a game
        // writes for every sprite every video frame, at a fraction of the
        // code of four property sets.
        public void Place(ulong x, uint y, uint frame, Colors color) { }

    }
}