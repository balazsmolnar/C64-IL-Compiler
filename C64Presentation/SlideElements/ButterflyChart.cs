using C64Lib;
using C64Presentation.Helper;

namespace C64Presentation.SlideElements;

// Two series compared item by item: the labels in a centred column
// (LabelWidth cells, Grey3), the left series' bars growing left from it,
// the right series' bars growing right, one item every Pitch rows. Each
// value sits at its own bar's outer end in the bar's colour; a 0 gets no
// bar and no number. The legend (the two series names) is right aligned on
// LegendRow. Max fills the space left for a bar after its value. Wait: the
// labels and legend on entry, then each series is a build step (its bars
// fade in together).
// Defaults: Y Theme.BodyTop, LabelWidth 12, Pitch 2, BarCells 8.
//
//     new ButterflyChart
//     {
//         Max = 3420, Left = "Hunchback", Right = "Catacombs", LegendRow = 2,
//         Rows = [new() { Label = "Static frames", LeftValue = 2304, LeftText = "2,304", RightValue = 1818, RightText = "1,818" }],
//     }
sealed class ButterflyChart : SlideElement
{
    public uint Y { get; init; }
    public uint LabelWidth { get; init; }
    public uint Pitch { get; init; }
    public uint BarCells { get; init; }
    public ulong Max { get; init; }
    public required string Left { get; init; }
    public required string Right { get; init; }
    public Colors LeftColor { get; init; }      // Auto = Theme.Accent
    public Colors RightColor { get; init; }     // Auto = Theme.Accent2
    public uint LegendRow { get; init; }
    public required ButterflyRow[] Rows { get; init; }
    public bool Wait { get; init; }

    const ulong ValueGap = 4;       // pixels between a bar's end and its value
    const ulong LegendGap = 16;

    public override void Draw()
    {
        uint labelWidth = Theme.OrCells(LabelWidth, 12);
        uint labelColumn = (Screen.Columns - labelWidth) / 2;
        ulong axisLeft = Screen.Px(labelColumn), axisRight = Screen.Px(labelColumn + labelWidth);
        ulong area = Screen.Px(Theme.OrCells(BarCells, 8));
        var leftColor = Theme.Or(LeftColor, Theme.Accent);
        var rightColor = Theme.Or(RightColor, Theme.Accent2);
        if (LegendRow != 0)
            DrawLegend(leftColor, rightColor);
        // Three passes: the labels, the left series, the right series (with
        // Wait, each series is a build step: its bars fade in together).
        for (uint pass = 0; pass < 3; pass++)
        {
            if (Wait && pass != 0)
                Deck.Step();
            DrawPass(pass, axisLeft, axisRight, area, leftColor, rightColor);
        }
    }

    void DrawPass(uint pass, ulong axisLeft, ulong axisRight, ulong area, Colors leftColor, Colors rightColor)
    {
        uint row = Theme.OrCells(Y, Theme.BodyTop);
        foreach (var item in Rows)
        {
            ulong y = Screen.Px(row);
            if (pass == 0)
                Text.Center(axisLeft, axisRight - axisLeft, row, item.Label, Theme.Body);
            else if (pass == 1 && item.LeftValue != 0)
            {
                ulong start = axisLeft - Length(item.LeftValue, area);
                Bar(start, axisLeft - 1, row, y, leftColor);
                Text.Right(start - ValueGap, row, item.LeftText ?? "", leftColor);
            }
            else if (pass == 2 && item.RightValue != 0)
            {
                ulong end = axisRight + Length(item.RightValue, area);
                Bar(axisRight, end - 1, row, y, rightColor);
                Text.Draw(end + ValueGap, row, item.RightText ?? "", rightColor);
            }
            row += Theme.OrCells(Pitch, 2);
        }
    }

    // At least 2 px, so a small value still shows a bar.
    ulong Length(ulong value, ulong area)
    {
        ulong max = Max;
        while (max > 255)       // value * area / Max in 16 bits
        {
            max /= 2;
            value /= 2;
        }
        ulong length = value * area / max;
        return length < 2 ? 2 : length;
    }

    static void Bar(ulong x0, ulong x1, uint row, ulong y, Colors color)
    {
        uint first = (uint)(x0 / Theme.CellPixels);
        Deck.Paint(first, row, (uint)(x1 / Theme.CellPixels) - first + 1, 1, color, Theme.Background);
        Screen.Fill(x0, y + 1, x1, y + 6);
    }

    // "• Left   • Right", right aligned at the content's edge.
    void DrawLegend(Colors leftColor, Colors rightColor)
    {
        ulong rightWidth = 10 + Text.Width(Right);
        ulong x = Screen.CellAfter(10 + Text.Width(Left), LegendGap);
        x = (Screen.Px(Theme.Right) - x - rightWidth) / Theme.CellPixels * Theme.CellPixels;
        Text.Draw(x + 1, LegendRow, Theme.Dot, leftColor);
        x = Screen.CellAfter(Text.Draw(x + 10, LegendRow, Left, Theme.Body), LegendGap);
        Text.Draw(x + 1, LegendRow, Theme.Dot, rightColor);
        Text.Draw(x + 10, LegendRow, Right, Theme.Body);
    }
}

// One item of a ButterflyChart: a 0 value has no bar and no number.
sealed class ButterflyRow
{
    public required string Label { get; init; }
    public ulong LeftValue { get; init; }
    public string? LeftText { get; init; }
    public ulong RightValue { get; init; }
    public string? RightText { get; init; }
}
