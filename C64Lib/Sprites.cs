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
    }

    public class Sprite
    {
        public bool Visible { get; set; }
        public bool IsInCollision { get; set; }
        public Colors Color { get; set; }
        public bool MultiColor { get; set; }
        public int DataBlock { get; set; }
        public uint X { get; set; }
        public uint Y { get; set; }

    }
}