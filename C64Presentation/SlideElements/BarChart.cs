using C64Lib;
using C64Presentation.Helper;

namespace C64Presentation.SlideElements;

// A horizontal bar chart: per bar a label (left), a 6 px bar from the
// axis, and its value text after it. Values set the lengths (Max fills the
// bar area), ValueTexts are what is printed. One bar every Pitch rows
// (default 2); BarColors per bar (default cyan). Legend: names for the
// first BarColors, in a row above the bars (which move down two rows).
// Outline: each bar gets a Grey2 frame at full scale, its empty part
// dashed, ticks every TickEvery under it, and the value past the frame
// ("208 of 311 lines"). Wait: build bar by bar; a bar drawn live grows.
// Defaults: X Theme.Left, Y Theme.BodyTop, Width Theme.Width, LabelWidth 12,
// ValueWidth 7. The storyboard's 38-column budget: X = 1, Width = 38.
//
//     new BarChart { Labels = ["Oct 17", "Oct 31"], Values = [36847, 28157],
//                    ValueTexts = ["36,847 B", "28,157 B"], Max = 37000,
//                    BarColors = [Theme.Muted, Theme.Accent], Wait = true }
class BarChart : SlideElement
{
    public uint X { get; init; }
    public uint Y { get; init; }
    public uint Width { get; init; }
    public uint LabelWidth { get; init; }
    public uint ValueWidth { get; init; }
    public uint Pitch { get; init; }
    public string[] Labels { get; init; }
    public ulong[] Values { get; init; }
    public string[] ValueTexts { get; init; }
    public Colors[] BarColors { get; init; }
    public string[] Legend { get; init; }
    public ulong Max { get; init; }
    public bool Outline { get; init; }
    public ulong TickEvery { get; init; }
    public bool Wait { get; init; }

    // Shared by every bar of the chart being drawn.
    static uint column_, cells_;
    static ulong max_, tickEvery_;
    static bool outline_;

    public override void Draw()
    {
        uint left = Theme.OrCells(X, Theme.Left);
        uint labelWidth = Theme.OrCells(LabelWidth, 12);
        column_ = left + labelWidth;
        cells_ = Theme.OrCells(Width, Theme.Width) - labelWidth - Theme.OrCells(ValueWidth, 7);
        max_ = Max;
        tickEvery_ = TickEvery;
        outline_ = Outline;
        uint row = Theme.OrCells(Y, Theme.BodyTop);
        if (Legend != null)
        {
            ulong x = Helper.Screen.X(column_);
            for (uint i = 0; i < Legend.Length; i++)
            {
                Text.Draw(x + 1, row, Theme.Dot, BarColors[i]);
                x = (Text.Draw(x + 10, row, Legend[i], Theme.Body) + 23) / 8 * 8;
            }
            row += 2;
        }
        for (uint i = 0; i < Labels.Length; i++)
        {
            if (Wait)
                Deck.Step();
            Text.Draw(Helper.Screen.X(left), row, Labels[i], Theme.Body);
            Bar(row, Values[i], ValueTexts != null ? ValueTexts[i] : "", BarColors != null ? BarColors[i] : Theme.Accent);
            row += Theme.OrCells(Pitch, 2);
        }
    }

    static void Bar(uint row, ulong value, string valueText, Colors color)
    {
        ulong area = Helper.Screen.X(cells_);
        ulong length = Scale(value, area);
        ulong x0 = Helper.Screen.X(column_);
        ulong y = Helper.Screen.X(row);
        Deck.Show();
        bool live = Deck.Live;
        if (outline_)
            Frame(row, x0, y, area, length);
        Deck.Paint(column_, row, (uint)((length + 7) / 8), 1, color, Theme.Background);
        for (ulong done = 0; done < length;)     // grows two cells a frame when live
        {
            ulong next = live && done + 16 < length ? done + 16 : length;
            Helper.Screen.Fill(x0 + done, y + 1, x0 + next - 1, y + 6);
            done = next;
            if (live)
                C64.Screen.WaitForVBlank();
        }
        Text.Draw((x0 + (outline_ ? area + 8 : length) + 11) / 8 * 8, row, valueText, Theme.Heading);
    }

    // value * area / max in 16 bits: halve both until max fits a byte.
    static ulong Scale(ulong value, ulong area)
    {
        ulong max = max_;
        while (max > 255)
        {
            max /= 2;
            value /= 2;
        }
        ulong length = value * area / max;
        return length == 0 ? 1 : length;
    }

    // A Grey2 frame one pixel outside the full-scale bar, ticks under it,
    // and the empty part of the bar's row dashed (from the next whole cell).
    static void Frame(uint row, ulong x0, ulong y, ulong area, ulong length)
    {
        Deck.Paint(column_, row - 1, cells_ + 1, 3, Theme.Muted, Theme.Background);
        C64.Screen.DrawRectangle(x0, y - 1, x0 + area, y + 8);
        for (ulong t = tickEvery_; t != 0 && t < max_; t += tickEvery_)
        {
            ulong tx = x0 + Scale(t, area);
            Helper.Screen.Line(tx, y + 9, tx, y + 10);
        }
        ulong x = x0 + (length + 15) / 8 * 8;
        for (; x + 1 < x0 + area; x += 4)
            Helper.Screen.Line(x, y + 3, x + 1, y + 3);
    }
}
