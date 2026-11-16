using C64Lib;
using C64Presentation.Helper;

namespace C64Presentation.SlideElements;

// Code on a black panel in a 1 px Grey1 frame, one cell of padding inside
// it: an optional Caption on the first row (Grey3, like a tab label) with
// an empty row under it, then the Lines in the code face (6 px a character,
// so (Width - 2) * 8 / 6 of them), syntax colours inline (Theme.Kw / Ty /
// Str / Num / Com / Pl; plain text is white; each colour starts on a cell
// edge). The panel is tall enough for
// Rows lines (or the lines it has). Drawing a CodeBlock where one is shown
// replaces the code row by row: each row is hidden, cleared and drawn anew
// in one go, so the panel is never empty.
//
//     new CodeBlock { X = 2, Y = 9, Width = 36, Rows = 7, Caption = "IL",
//                     Lines = [$"{Theme.Kw}ldarg.0", $"{Theme.Kw}add"] }
sealed class CodeBlock : SlideElement
{
    public uint X { get; init; }
    public uint Y { get; init; }
    public uint Width { get; init; }
    public uint Rows { get; init; }
    public required string[] Lines { get; init; }
    public string? Caption { get; init; }
    public uint Indent { get; init; }       // cells left free before the code: a gutter
    public bool Wait { get; init; }

    public override void Draw()
    {
        if (Wait)
            Deck.Step();
        uint width = Theme.OrCells(Width, Theme.Width);
        uint lines = Rows > (uint)Lines.Length ? Rows : (uint)Lines.Length;
        uint top = Y + (Caption != null ? 3u : 1u);      // the first code row
        uint height = top - Y + lines + 1;
        bool replacing = Deck.Live;
        if (!replacing)
            DrawPanel(width, height);
        if (Caption != null)
        {
            if (replacing)
                ClearRow(Y + 1, width);
            Text.Draw(Screen.Px(X + 1), Y + 1, Caption, Theme.Body, Theme.Code);
        }
        for (uint i = 0; i < lines; i++)
        {
            uint row = top + i;
            if (replacing)
                ClearRow(row, width);
            if (i < Lines.Length)
                Text.Code(X + 1 + Indent, row, Lines[i]);
        }
    }

    void DrawPanel(uint width, uint height)
    {
        Deck.Paint(X, Y, width, height, Theme.Frame, Theme.Code);
        Screen.Frame(Screen.Px(X), Screen.Px(Y), Screen.Px(X + width) - 1, Screen.Px(Y + height) - 1);
    }

    // When the code is being replaced: one row of the panel's inside hidden
    // and cleared, just before its new line is drawn (the frame stays).
    void ClearRow(uint row, uint width)
    {
        Deck.Paint(X + 1, row, width - 2, 1, Theme.Code, Theme.Code);
        Screen.ClearRect(X + 1, row, width - 2, 1);
    }
}
