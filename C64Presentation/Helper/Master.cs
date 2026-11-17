using C64Lib;

namespace C64Presentation.Helper;

// The master layouts: a content slide (title, rule, footer), the cover, a
// section divider and a full-bleed picture. See Theme for the grid.
static class Master
{
    public const string DeckTitle = "C# on the C64";
    public const string Author = "Balazs Molnar";

    const ulong ScreenPixels = Screen.Columns * Theme.CellPixels;      // 320
    const ulong ContentLeft = Theme.Left * Theme.CellPixels;           // 16
    const ulong ContentRight = Theme.Right * Theme.CellPixels - 1;     // 303
    const ulong ProgressY = Theme.ProgressRow * Theme.CellPixels + 3;  // 195
    const uint RuleCells = 3;                                          // the accent stub

    // Title at the top left, the accent rule under it, the footer. The body
    // ends at Theme.BodyBottom: the row above the footer stays empty (a
    // scene build shows a red border when something draws there).
    public static void Content(string title)
    {
        Deck.KeepEmptyFrom(Theme.BodyBottom);
        Text.Heading(ContentLeft, Theme.TitleRow, title);
        Rule(Theme.RuleRow);
        Footer();
    }

    // The accent stub (24 x 2 px) and a Grey2 hairline to the right margin.
    static void Rule(uint row)
    {
        ulong y = Screen.Px(row) + 2;
        Deck.Paint(Theme.Left, row, RuleCells, 1, Theme.Accent, Theme.Background);
        Screen.Fill(ContentLeft, y, ContentLeft + Screen.Px(RuleCells) - 1, y + 1);
        Deck.Paint(Theme.Left + RuleCells, row, Theme.Width - RuleCells, 1, Theme.Muted, Theme.Background);
        Screen.Line(ContentLeft + Screen.Px(RuleCells) + 4, y + 1, ContentRight, y + 1);
    }

    // Deck title, "n / total" and the progress bar, all receding: the grey
    // track over the full width, then this slide's progress in cyan over it.
    public static void Footer()
    {
        Text.Draw(ContentLeft, Theme.FooterRow, DeckTitle, Theme.Muted);
        Text.RightNow(ContentRight, Theme.FooterRow, $"{Deck.Number + 1} / {Deck.Count}", Theme.Muted);

        uint done = Theme.Left + (uint)((ulong)Theme.Width * (Deck.Number + 1) / Deck.Count);
        Deck.Paint(Theme.Left, Theme.ProgressRow, Theme.Width, 1, Theme.Frame, Theme.Background);
        Screen.Fill(ContentLeft, ProgressY, ContentRight, ProgressY + 1);
        Deck.Paint(Theme.Left, Theme.ProgressRow, done - Theme.Left, 1, Theme.Accent, Theme.Background);
    }

    // The cover: the art's title picture (title.img, hi-res, 40x25: the
    // title lettering, the breadbox and a code card), nothing else.
    public static void Cover() => C64.Screen.DrawImage(0, 0, C64Address.FromLabel("img_Title"));

    // A section divider on black, centred as a group: the part number in
    // big Grey2 figures with the section's 6x6 icon on the right, the title
    // in the big face (white), a Grey3 subtitle. icon 0 = none.
    public static void Section(string number, string title, string subtitle, ulong icon)
    {
        Text.Big(ContentLeft, 4, number, Theme.Muted);
        if (icon != 0)
            C64.Screen.DrawImage(Theme.Right - 6, 1, icon);     // its base line level with the number's
        Text.Big(ContentLeft, 10, title);
        Text.Draw(ContentLeft, 15, subtitle, Theme.Body);
        Footer();
    }

    // A multicolour picture with no chrome (no title, no footer), its
    // top-left cell at (column, row); from cell row `split` down the screen
    // is hi-res for text; 25 = no split. Caption: centred one row below the
    // split ("" for none). The image is an address, so a picture loaded
    // from the disk (ArtDisk.Load) works as well as an embedded one.
    //     Master.Picture(ArtDisk.Load(DiskPicture.ShotRoom), 0, 0, 21, "A Commodore 64 game.");
    public static void Picture(ulong image, uint column, uint row, uint split, string caption)
    {
        Deck.Picture(image, column, row, split);
        Text.Center(0, ScreenPixels, split + 1, caption, Theme.Body);
    }
}
