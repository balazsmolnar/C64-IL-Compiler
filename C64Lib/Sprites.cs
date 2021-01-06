namespace C64Lib
{
    public class SpriteCollection
    {
        public Sprite Sprite0 { get; }
        public Sprite Sprite1 { get; }
    }

    public class Sprite
    {
        public bool Visible { get; set; }
        public Colors Color { get; set; }
        public int DataBlock { get; set; }
        public uint X { get; set; }
        public uint Y { get; set; }

    }
}