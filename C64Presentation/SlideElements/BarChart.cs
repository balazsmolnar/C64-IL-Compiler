using C64Lib;
using C64Presentation.Helper;

namespace C64Presentation.SlideElements;

// A horizontal bar chart. Each Bar is a label (left), a 6 px bar from the
// axis scaled so Max fills the bar area, and its value text after it; a bar
// too short to show (under 4 px) gets only its value. One bar every Pitch
// rows (2). Two series item by item: ButterflyChart. Outline: each bar gets a Grey2 frame at
// full scale with ticks every TickEvery under it, and its value past the
// frame. Gap: a dashed box between two bars' ends on the second bar's row,
// with its text under it ("-8,690 B"). Wait: each bar after the first, then
// the gap, is a build step (the first bar, the baseline, is there on entry);
// a bar drawn live grows from the axis.
// Defaults: X Theme.Left, Y Theme.BodyTop, Width Theme.Width, LabelWidth 12,
// ValueWidth 7.
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
    public BarGap? Gap { get; init; }
    public ulong Max { get; init; }
    public bool Outline { get; init; }
    public ulong TickEvery { get; init; }
    public bool Wait { get; init; }

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
        uint row = Theme.OrCells(Y, Theme.BodyTop);
        uint pitch = Theme.OrCells(Pitch, 2);
        uint firstRow = row;
        for (uint i = 0; i < Bars.Length; i++)
        {
            if (Wait && i != 0)     // the first bar is the baseline: on entry
                Deck.Step();
            var bar = Bars[i];
            Text.Draw(Screen.Px(left), row, bar.Label, Theme.Body);
            // The gap's shorter bar: its value goes past the dashed box.
            bar.Draw(scale, row, Gap != null && i == Gap.To ? Bars[Gap.From].Value : bar.Value);
            row += pitch;
        }
        if (Gap != null)
        {
            if (Wait)
                Deck.Step();
            Gap.Draw(scale, Bars, firstRow, pitch);
        }
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
    // or the longer bar's when a gap box sits after this one).
    public void Draw(BarScale scale, uint row, ulong valueAfter)
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
        ulong end = scale.Outline ? Screen.Px(scale.Cells) + 8 : scale.Length(valueAfter);
        Helper.Text.Draw(Screen.CellAfter(x0 + end, 4), row, Text ?? "", Theme.Heading);
    }
}

// A dashed box from the end of bar To to the end of bar From, on bar To's
// row, and its text under it (ending with it; centred under an outline).
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
        // Under an outline's bottom rule, centred on the box (the value is
        // past the frame). Without one, the value follows the box on its
        // row, so the text ends where the box ends, under the bar, clear of it.
        if (scale.Outline)
            Helper.Text.Center(start, end - start + 1, row + 2, Text, Theme.Heading);
        else
            Helper.Text.Right(end, row + 1, Text, Theme.Heading);
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
