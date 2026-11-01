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
// cells, ValueWidth 6 cells (kept free right of the longest bar).
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
        uint barColumn = left + labelWidth;
        uint barCells = (Width == 0 ? Theme.Width : Width) - labelWidth - (ValueWidth == 0 ? 6 : ValueWidth);
        ulong area = Helper.Screen.X(barCells);
        uint row = Y == 0 ? Theme.BodyTop : Y;
        uint pitch = Pitch == 0 ? 2 : Pitch;
        for (uint i = 0; i < Labels.Length; i++)
        {
            if (Wait)
                Deck.Step();
            Colors color = BarColors != null ? BarColors[i] : Theme.Accent;
            Text.Draw(Helper.Screen.X(left), row, Labels[i], Theme.Body);

            // value * area / Max in 16 bits: halve both until Max fits a byte.
            ulong value = Values[i];
            ulong max = Max;
            while (max > 255)
            {
                max = max / 2;
                value = value / 2;
            }
            ulong length = value * area / max;
            if (length == 0)
                length = 1;
            ulong x0 = Helper.Screen.X(barColumn);
            ulong y = Helper.Screen.X(row);
            uint cells = (uint)((length + 7) / 8);
            if (Wait)
                Deck.Show();
            bool live = Deck.Live;
            Deck.Paint(barColumn, row, cells, 1, color, Theme.Background);
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
            if (ValueTexts != null)
                Text.Draw((x0 + length + 11) / 8 * 8, row, ValueTexts[i], Theme.Heading);
            row += pitch;
        }
    }
}
