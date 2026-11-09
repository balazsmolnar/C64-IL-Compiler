using C64Lib;
using C64Presentation.Helper;

namespace C64Presentation.SlideElements;

// Bullets: an accent dot (level 0) or a dash (level 1), 5 px of air, then
// the text; one item every Pitch rows from (Left, Top). Wait = build them
// in one by one. Defaults: Left Theme.Left, Top Theme.BodyTop, Pitch 2,
// Color Body.
//
//     new BulletList { Items = ["Structure beats peepholes", "Measure every change"], Wait = true }
class BulletList : SlideElement
{
    const ulong DotOffset = 1;     // the dot's left pixel in its cell
    const ulong TextOffset = 10;   // the text's left pixel: 5 px after the dot
    const uint DefaultPitch = 2;

    public string[] Items { get; init; }
    public uint Left { get; init; }
    public uint Top { get; init; }
    public uint Pitch { get; init; }
    public uint Level { get; init; }
    public Colors Color { get; init; }
    public bool Wait { get; init; }

    public override void Draw()
    {
        ulong x = Helper.Screen.X(Theme.OrCells(Left, Theme.Left) + Level + Level);
        uint row = Theme.OrCells(Top, Theme.BodyTop);
        uint pitch = Theme.OrCells(Pitch, DefaultPitch);
        var color = Theme.Or(Color, Theme.Body);
        string mark = Level == 0 ? Theme.Dot : "-";
        var markColor = Level == 0 ? Theme.Accent : Theme.Muted;
        foreach (var item in Items)
        {
            if (Wait)
                Deck.Step();
            Text.Draw(x + DotOffset, row, mark, markColor);
            Text.Draw(x + TextOffset, row, item, color);
            row += pitch;
        }
    }
}
