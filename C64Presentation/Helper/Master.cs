using C64Lib;

namespace C64Presentation.Helper;

// The master layouts: a content slide (title, rule, footer), the cover, a
// section divider and a full-bleed picture. See Theme for the grid.
static class Master
{
    public const string DeckTitle = "C# on the C64";
    public const string Author = "Balazs Molnar";

    const ulong ScreenPixels = 320;
    const ulong ContentLeft = Theme.Left * Theme.CellPixels;           // 16
    const ulong ContentRight = Theme.Right * Theme.CellPixels - 1;     // 303
    const ulong RuleY = Theme.RuleRow * Theme.CellPixels + 2;          // 26
    const ulong ProgressY = Theme.ProgressRow * Theme.CellPixels + 3;  // 195

    // Title at the top left, the accent rule under it, the footer.
    public static void Content(string title)
    {
        Text.Heading(ContentLeft, Theme.TitleRow, title);
        Rule(Theme.RuleRow);
        Footer();
    }

    // The accent stub (24 x 2 px) and a Grey2 hairline to the right margin.
    static void Rule(uint row)
    {
        ulong y = Screen.X(row) + 2;
        Deck.Paint(Theme.Left, row, 3, 1, Theme.Accent, Theme.Background);
        Screen.Fill(ContentLeft, y, ContentLeft + 23, y + 1);
        Deck.Paint(Theme.Left + 3, row, Theme.Width - 3, 1, Theme.Muted, Theme.Background);
        Screen.Line(ContentLeft + 28, y + 1, ContentRight, y + 1);
    }

    // Deck title, "n / total" and the progress bar, all receding (Grey2).
    public static void Footer()
    {
        Text.Draw(ContentLeft, Theme.FooterRow, DeckTitle, Theme.Muted);
        Text.Right(ContentRight, Theme.FooterRow, $"{Deck.Number + 1} / {Deck.Count}", Theme.Muted);

        // Progress: accent up to this slide, a dark track after it.
        ulong end = ContentLeft + (ContentRight + 1 - ContentLeft) * (Deck.Number + 1) / Deck.Count;
        uint done = (uint)((end + 7) / 8);   // first cell that is all track
        Deck.Paint(Theme.Left, Theme.ProgressRow, done - Theme.Left, 1, Theme.Accent, Theme.Background);
        Screen.Fill(ContentLeft, ProgressY, end - 1, ProgressY + 1);
        if (done < Theme.Right)
        {
            Deck.Paint(done, Theme.ProgressRow, Theme.Right - done, 1, Theme.Card, Theme.Background);
            Screen.Fill(Screen.X(done), ProgressY, ContentRight, ProgressY + 1);
        }
    }

    // The cover (and closing) slide: the title at twice the title size, an
    // accent rule, the subtitle and the author, centred. No footer. Rows
    // 2-8 are left for the logos (IntroSlide).
    public static void Cover(string title, string subtitle)
    {
        Text.Big((ScreenPixels - Text.BigWidth(title) + 4) / 2, 9, title);
        Deck.Paint(18, 14, 4, 1, Theme.Accent, Theme.Background);
        Screen.Fill(148, 114, 171, 115);
        Text.Center(0, ScreenPixels, 16, subtitle, Theme.Body);
        Text.Center(0, ScreenPixels, 19, Author, Theme.Muted);
    }

    // A section divider on black: the part number in big cyan figures, the
    // title in the heading face under an accent rule, the subtitle, and the
    // section's 6x6 icon on the right (0 = none). Footer as usual.
    public static void Section(string number, string title, string subtitle, ulong icon)
    {
        Text.Big(ContentLeft, 5, number, Theme.Accent);
        Rule(10);
        Text.Heading(ContentLeft, 12, title);
        Text.Draw(ContentLeft, 15, subtitle, Theme.Body);
        if (icon != 0)
            C64.Screen.DrawImage(31, 4, icon);
        Footer();
    }

    // A multicolour picture with no chrome (no title, no footer), its
    // top-left cell at (column, row); from cell row `split` down the screen
    // is hi-res for text; 25 = no split. Caption: centred one row below the
    // split. The image is an address, so a picture loaded from disk into a
    // buffer works as well as an embedded one.
    //     Master.Picture(C64Address.FromLabel("img_ShotRoom"), 0, 0, 21, "A Commodore 64 game.");
    public static void Picture(ulong image, uint column, uint row, uint split, string caption)
    {
        Deck.Picture(image, column, row, split);
        Text.Center(0, ScreenPixels, split + 1, caption, Theme.Body);
    }
}
