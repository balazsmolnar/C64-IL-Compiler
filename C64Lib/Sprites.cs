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
    }

    public class Sprite
    {
        public bool Visible { get; set; }
        public bool IsInCollision { get; }
        public bool IsInBackgroundCollision { get; }
        public Colors Color { get; set; }
        public bool MultiColor { get; set; }
        public bool HighPosition { get; set; }
        public bool ExpandX { get; set; }
        public bool ExpandY { get; set; }
        public ulong DataBlock { get; set; }
        public uint X { get; set; }
        public uint Y { get; set; }

    }
}