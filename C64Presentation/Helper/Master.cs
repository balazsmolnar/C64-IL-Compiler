using C64Lib;

namespace C64Presentation.Helper;

// The master layouts: a content slide (title, rule, footer), the cover, a
// section divider, and a full-bleed picture (Deck.Picture). See Theme.
static class Master
{
    public const string DeckTitle = "C# on the C64";
    public const string Author = "Balazs Molnar";

    // Title at the top left, the accent rule under it, the footer.
    public static void Content(string title)
    {
        Text.Heading(16, Theme.TitleRow, title);
        // Accent bar (24 x 2 px) then a hairline to the right margin.
        Deck.Paint(Theme.Left, Theme.RuleRow, 3, 1, Theme.Accent, Theme.Background);
        Screen.Fill(16, 26, 39, 27);
        Deck.Paint(Theme.Left + 3, Theme.RuleRow, Theme.Width - 3, 1, Theme.Muted, Theme.Background);
        Screen.Line(44, 27, 303, 27);
        Footer();
    }

    // Deck title, "n / total" and the progress bar.
    public static void Footer()
    {
        Text.Draw(16, Theme.FooterRow, DeckTitle, Theme.Muted);
        Text.Right(303, Theme.FooterRow, $"{Deck.Number + 1} / {Deck.Count}", Theme.Muted);

        // Progress: accent up to this slide, a dark track after it.
        ulong end = 16 + 288 * (ulong)(Deck.Number + 1) / (ulong)Deck.Count;
        uint done = (uint)((end + 7) / 8);   // first cell that is all track
        Deck.Paint(2, 24, done - 2, 1, Theme.Accent, Theme.Background);
        Screen.Fill(16, 195, end - 1, 196);
        if (done < Theme.Right)
        {
            Deck.Paint(done, 24, Theme.Right - done, 1, Theme.Card, Theme.Background);
            Screen.Fill(Screen.X(done), 195, 303, 196);
        }
    }

    // The cover (and closing) slide: the big title, an accent rule, the
    // subtitle and the author, centred. No footer.
    public static void Cover(string title, string subtitle)
    {
        Text.CenterHeading(0, 320, 9, title);
        Deck.Paint(18, 12, 4, 1, Theme.Accent, Theme.Background);
        Screen.Fill(148, 98, 171, 99);
        Text.Center(0, 320, 13, subtitle, Theme.Body);
        Text.Center(0, 320, 16, Author, Theme.Muted);
    }

    // A section divider: a full-width band (rows 8-15) with an accent bar,
    // the part label, the title and a subtitle; footer below.
    public static void Section(string part, string title, string subtitle)
    {
        Deck.Paint(0, 8, 40, 8, Theme.Card, Theme.Card);
        Deck.Paint(Theme.Left, 9, 1, 6, Theme.Accent, Theme.Card);
        Screen.Fill(16, 74, 19, 121);
        Text.Draw(32, 9, part, Theme.Accent, Theme.Card);
        Text.Heading(32, 11, title, Theme.Heading, Theme.Card);
        Text.Draw(32, 14, subtitle, Theme.Body, Theme.Card);
        Footer();
    }

    // A multicolour picture with no chrome (no title, no footer), its
    // top-left cell at (column, row); from cell row `split` down the screen
    // is hi-res for text (Text works there as anywhere); 25 = no split.
    // Caption: centred one row below the split.
    //     Master.Picture(C64Address.FromLabel("img_ShotRoom"), 0, 0, 21, "A Commodore 64 game.");
    public static void Picture(ulong image, uint column, uint row, uint split, string caption)
    {
        Deck.Picture(image, column, row, split);
        if (caption != null)
            Text.Center(0, 320, split + 1, caption, Theme.Body);
    }
}
