using C64Lib;
using C64Presentation.Helper;

namespace C64Presentation.SlideElements;

// Bullets: an accent dot (level 0) or a dash (level 1) and the text, one
// item every Pitch rows from (Left, Top). Wait = build them in one by one.
// Defaults: Left = Theme.Left, Top = Theme.BodyTop, Pitch = 2, Color = Body.
//
//     new BulletList { Items = new[] { "One", "Two" }, Wait = true }
class BulletList : SlideElement
{
    public string[] Items;
    public uint Left;
    public uint Top;
    public uint Pitch;
    public uint Level;
    public Colors Color;
    public bool Wait;

    public override void Draw()
    {
        uint column = (Left == 0 ? Theme.Left : Left) + Level + Level;
        uint row = Top == 0 ? Theme.BodyTop : Top;
        uint pitch = Pitch == 0 ? 2 : Pitch;
        Colors color = Theme.Or(Color, Theme.Body);
        ulong x = Helper.Screen.X(column);
        foreach (var item in Items)
        {
            if (Wait)
                Deck.Step();
            if (Level == 0)
                Text.Draw(x + 1, row, Theme.Dot, Theme.Accent);
            else
                Text.Draw(x + 2, row, "-", Theme.Muted);
            Text.Draw(x + 9, row, item, color);
            row += pitch;
        }
    }
}
