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

    }
}