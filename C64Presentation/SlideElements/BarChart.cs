using C64Lib;
using C64Presentation.Helper;

namespace C64Presentation.SlideElements;

// A horizontal bar chart: per bar a label (left), a bar (6 px tall) from
// the axis, and its value text just past the bar's end. Values set the
// lengths (scaled so Max fills the bar area), ValueTexts are what is
// printed ("10,478 B", "-12%"). One bar every Pitch rows (default 2);
// BarColors per bar (default Accent; e.g. Theme.Muted "before", Theme.Accent
// "after"). Wait: build bar by bar; a bar drawn live grows from the axis.
// Defaults: X Theme.Left, Y Theme.BodyTop, Width Theme.Width, LabelWidth 12
// cells, ValueWidth 7 cells (kept free right of the longest bar). For the
// storyboard's 38-column label|bar|value budget: X = 1, Width = 38.
//
//     new BarChart { Labels = new[] { "Oct 17", "Oct 31" }, Values = new ulong[] { 36847, 28157 },
//                    ValueTexts = new[] { "36,847 B", "28,157 B" }, Max = 37000, Wait = true }
class BarChart : SlideElement
{
    public uint X, Y, Width;
    public uint LabelWidth;
    public uint ValueWidth;
    public uint Pitch;
    public string[] Labels;
    public ulong[] Values;
    public string[] ValueTexts;
    public Colors[] BarColors;
    public ulong Max;
    public bool Wait;

    public override void Draw()
    {
        uint left = X == 0 ? Theme.Left : X;
        uint labelWidth = LabelWidth == 0 ? 12 : LabelWidth;
        uint cells = (Width == 0 ? Theme.Width : Width) - labelWidth - (ValueWidth == 0 ? 7 : ValueWidth);
        uint row = Y == 0 ? Theme.BodyTop : Y;
        uint pitch = Pitch == 0 ? 2 : Pitch;
        var labels = Labels;
        for (uint i = 0; i < labels.Length; i++)
        {
            Bar(Wait, left, left + labelWidth, cells, row, labels[i], Values[i], Max,
                ValueTexts != null ? ValueTexts[i] : "", BarColors != null ? BarColors[i] : Theme.Accent);
            row += pitch;
        }
    }

    // One bar: label at `left`, bar from column `barColumn` over at most
    // `cells` cells, value text after it.
    public static void Bar(bool wait, uint left, uint barColumn, uint cells, uint row, string label, ulong value, ulong max, string valueText, Colors color)
    {
        if (wait)
            Deck.Step();
        Text.Draw(Helper.Screen.X(left), row, label, Theme.Body);
        // value * area / max in 16 bits: halve both until max fits a byte.
        while (max > 255)
        {
            max = max / 2;
            value = value / 2;
        }
        ulong length = value * Helper.Screen.X(cells) / max;
        if (length == 0)
            length = 1;
        ulong x0 = Helper.Screen.X(barColumn);
        ulong y = Helper.Screen.X(row);
        Deck.Show();
        bool live = Deck.Live;
        Deck.Paint(barColumn, row, (uint)((length + 7) / 8), 1, color, Theme.Background);
        // Grow it two cells per frame when it can be seen.
        ulong done = 0;
        while (done < length)
        {
            ulong next = done + 16;
            if (next > length || !live)
                next = length;
            Helper.Screen.Fill(x0 + done, y + 1, x0 + next - 1, y + 6);
            done = next;
            if (live)
                C64.Screen.WaitForVBlank();
        }
        // "" (no ValueTexts) draws nothing
            Text.Draw((x0 + length + 11) / 8 * 8, row, valueText, Theme.Heading);
    }
}
