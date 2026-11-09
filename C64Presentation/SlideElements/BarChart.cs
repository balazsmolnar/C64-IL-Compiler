using C64Lib;
using C64Presentation.Helper;

namespace C64Presentation.SlideElements;

// A horizontal bar chart. Each Bar is a label (left), a 6 px bar from the
// axis scaled so Max fills the bar area, and its value text after it; a bar
// too short to show (under 4 px) gets only its value. One bar every Pitch
// rows (2; 1 for two series on consecutive rows). Legend: colour dots and
// names on a row above the bars, or right aligned on LegendRow (the title's
// second row, say). ValueColumn: the values right aligned at the chart's
// right edge in their bar's colour, instead of after each bar. GroupEvery:
// a 1 px Grey1 hairline under every that many bars (a label and its
// series), where it doesn't cross a bar or a value. Outline: each bar gets a Grey2 frame at
// full scale with ticks every TickEvery under it, and its value past the
// frame. Gap: a dashed box between two bars' ends on the second bar's row,
// with its text under it ("-8,690 B"). Wait: each bar is a build step; a
// bar drawn live grows from the axis.
// Defaults: X Theme.Left, Y Theme.BodyTop, Width Theme.Width, LabelWidth 12,
// ValueWidth 7. The storyboard's 38-column budget: X = 1, Width = 38.
//
//     new BarChart
//     {
//         Max = 37000, LabelWidth = 7, Wait = true,
//         Bars =
//         [
//             new() { Label = "Oct 17", Value = 36847, Text = "36,847 B", Color = Theme.Muted },
//             new() { Label = "Oct 31", Value = 28157, Text = "28,157 B" },
//         ],
//         Gap = new() { From = 0, To = 1, Text = "-8,690 B" },
//     }
sealed class BarChart : SlideElement
{
    public uint X { get; init; }
    public uint Y { get; init; }
    public uint Width { get; init; }
    public uint LabelWidth { get; init; }
    public uint ValueWidth { get; init; }
    public uint Pitch { get; init; }
    public required Bar[] Bars { get; init; }
    public LegendItem[]? Legend { get; init; }
    public BarGap? Gap { get; init; }
    public ulong Max { get; init; }
    public bool Outline { get; init; }
    public ulong TickEvery { get; init; }
    public uint LegendRow { get; init; }
    public bool ValueColumn { get; init; }
    public uint GroupEvery { get; init; }
    public bool Wait { get; init; }

    const ulong LegendGap = 16;     // pixels between legend items

    public override void Draw()
    {
        uint left = Theme.OrCells(X, Theme.Left);
        uint width = Theme.OrCells(Width, Theme.Width);
        uint labelWidth = Theme.OrCells(LabelWidth, 12);
        var scale = new BarScale
        {
            Column = left + labelWidth,
            Cells = width - labelWidth - Theme.OrCells(ValueWidth, 7),
            Max = Max,
            Outline = Outline,
            TickEvery = TickEvery,
        };
        ulong valueRight = ValueColumn ? Screen.Px(left + width) - 2 : 0;
        uint row = Theme.OrCells(Y, Theme.BodyTop);
        if (Legend != null)
        {
            if (LegendRow != 0)
            {
                ulong right = Screen.Px(left + width);
                DrawLegend((right - LegendWidth()) / Theme.CellPixels * Theme.CellPixels, LegendRow);
            }
            else
            {
                DrawLegend(Screen.Px(scale.Column), row);
                row += 2;
            }
        }
        uint pitch = Theme.OrCells(Pitch, 2);
        uint firstRow = row;
        for (uint i = 0; i < Bars.Length; i++)
        {
            if (Wait)
                Deck.Step();
            var bar = Bars[i];
            Text.Draw(Screen.Px(left), row, bar.Label, Theme.Body);
            // The gap's shorter bar: its value goes past the dashed box.
            bar.Draw(scale, row, Gap != null && i == Gap.To ? Bars[Gap.From].Value : bar.Value, valueRight);
            if (GroupEvery != 0 && (i + 1) % GroupEvery == 0 && i + 1 < Bars.Length)
                Hairline(left, row, scale, bar, valueRight);
            row += pitch;
        }
        if (Gap != null)
        {
            if (Wait)
                Deck.Step();
            Gap.Draw(scale, Bars, firstRow, pitch);
        }
    }

    // Each item: a dot in its colour (a cell of its own), its name in Grey3.
    void DrawLegend(ulong x, uint row)
    {
        foreach (var item in Legend!)
        {
            Text.Draw(x + 1, row, Theme.Dot, item.Color);
            x = Screen.CellAfter(Text.Draw(x + 10, row, item.Name, Theme.Body), LegendGap);
        }
    }

    ulong LegendWidth()
    {
        ulong x = 0, end = 0;
        foreach (var item in Legend!)
        {
            end = x + 10 + Text.Width(item.Name);
            x = Screen.CellAfter(end, LegendGap);
        }
        return end;
    }

    // Along the bottom pixel line of a group's last row: under the (empty)
    // label cells, and from the bar's end to its value, in cells of its own.
    static void Hairline(uint left, uint row, BarScale scale, Bar bar, ulong valueRight)
    {
        ulong y = Screen.Px(row) + 7;
        Rule(Screen.Px(left), Screen.Px(scale.Column) - 8, row, y);
        ulong from = Screen.CellAfter(Screen.Px(scale.Column) + scale.Length(bar.Value), 0);
        ulong to = valueRight != 0 ? (valueRight + 1 - Text.Width(bar.Text ?? "")) / Theme.CellPixels * Theme.CellPixels : Screen.Px(scale.Column + scale.Cells);
        if (to > from)
            Rule(from, to - Theme.CellPixels, row, y);
    }

    static void Rule(ulong x0, ulong x1, uint row, ulong y)
    {
        Deck.Paint((uint)(x0 / Theme.CellPixels), row, (uint)((x1 - x0) / Theme.CellPixels) + 1, 1, Theme.Frame, Theme.Background);
        Screen.Line(x0, y, x1 + Theme.CellPixels - 1, y);
    }
}

// One bar of a BarChart.
sealed class Bar
{
    public required string Label { get; init; }
    public ulong Value { get; init; }
    public string? Text { get; init; }
    public Colors Color { get; init; }          // Auto = Theme.Accent

    // valueAfter: the value whose bar end the value text follows (Value,
    // or the longer bar's when a gap box sits after this one). valueRight:
    // where the value ends in a value column, in the bar's colour (0: after
    // the bar, in white).
    public void Draw(BarScale scale, uint row, ulong valueAfter, ulong valueRight)
    {
        ulong length = scale.Length(Value);
        ulong x0 = Screen.Px(scale.Column);
        ulong y = Screen.Px(row);
        Deck.Show();
        bool live = Deck.Live;
        if (scale.Outline)
            scale.Frame(row);
        if (length >= BarScale.Shortest)
        {
            Deck.Paint(scale.Column, row, (uint)((length + 7) / 8), 1, Theme.Or(Color, Theme.Accent), Theme.Background);
            for (ulong done = 0; done < length;)     // grows two cells a frame when live
            {
                ulong next = live && done + 16 < length ? done + 16 : length;
                Screen.Fill(x0 + done, y + 1, x0 + next - 1, y + 6);
                done = next;
                if (live)
                    C64.Screen.WaitForVBlank();
            }
        }
        if (valueRight != 0)
        {
            Helper.Text.Right(valueRight, row, Text ?? "", Theme.Or(Color, Theme.Accent));
            return;
        }
        ulong end = scale.Outline ? Screen.Px(scale.Cells) + 8 : scale.Length(valueAfter);
        Helper.Text.Draw(Screen.CellAfter(x0 + end, 4), row, Text ?? "", Theme.Heading);
    }
}

// A dashed box from the end of bar To to the end of bar From, on bar To's
// row, and its text centred under it.
sealed class BarGap
{
    public uint From { get; init; }
    public uint To { get; init; }
    public required string Text { get; init; }

    public void Draw(BarScale scale, Bar[] bars, uint firstRow, uint pitch)
    {
        uint row = firstRow + To * pitch;
        ulong x0 = Screen.Px(scale.Column);
        // From the first whole cell after the shorter bar (its last cell keeps its colour).
        ulong start = Screen.CellAfter(x0 + scale.Length(bars[To].Value), 0), end = x0 + scale.Length(bars[From].Value) - 1;
        uint first = (uint)(start / 8), last = (uint)(end / 8);
        Deck.Paint(first, row, last - first + 1, 1, Theme.Heading, Theme.Background);
        ulong y = Screen.Px(row);
        for (ulong x = start; x < end; x += 3)        // the top and bottom edges, level with the bar
        {
            Screen.Line(x, y + 1, x, y + 1);
            Screen.Line(x, y + 6, x, y + 6);
        }
        for (ulong yy = y + 1; yy < y + 7; yy += 2)      // the two ends
        {
            Screen.Line(start, yy, start, yy);
            Screen.Line(end, yy, end, yy);
        }
        // Under the box; under an outline's bottom rule too.
        Helper.Text.Center(start, end - start + 1, row + (scale.Outline ? 2u : 1u), Text, Theme.Heading);
    }
}

// The scale shared by a chart's bars.
sealed class BarScale
{
    public const ulong Shortest = 4;    // shorter bars show only their value

    public uint Column { get; init; }
    public uint Cells { get; init; }
    public ulong Max { get; init; }
    public bool Outline { get; init; }
    public ulong TickEvery { get; init; }

    // value * area / Max in 16 bits: halve both until Max fits a byte.
    public ulong Length(ulong value)
    {
        ulong max = Max;
        while (max > 255)
        {
            max /= 2;
            value /= 2;
        }
        return value * Screen.Px(Cells) / max;
    }

    // A Grey2 frame one pixel outside the full-scale bar, ticks under it.
    public void Frame(uint row)
    {
        ulong x0 = Screen.Px(Column), y = Screen.Px(row), area = Screen.Px(Cells);
        Deck.Paint(Column, row - 1, Cells + 1, 3, Theme.Muted, Theme.Background);
        Screen.Frame(x0, y - 1, x0 + area, y + 8);
        if (TickEvery == 0)
            return;
        for (ulong t = TickEvery; t < Max; t += TickEvery)
        {
            ulong tx = x0 + Length(t);
            Screen.Line(tx, y + 9, tx, y + 10);
        }
    }
}

// A colour and its meaning, for a chart's legend.
sealed class LegendItem
{
    public required string Name { get; init; }
    public Colors Color { get; init; }
}
