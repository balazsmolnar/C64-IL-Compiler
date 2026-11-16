using C64Lib;

namespace Hunchback;

// The hi-score league: five entries of seven score digits and three letters
// (tbl_HiScores in GameData.asm, screen codes, as the original keeps it),
// its screen, and the name registration after a game that made the league
// (Restructure/Menu.asm's Menu_DisplayHiScores, Player.asm's
// Player_GameOver and Player_EnterHiScoreName).
class HiScores
{
    private static uint cursor_;
    private static uint letters_;

    // Once, at power on: 5,000 points four times and 500 once, all by JON.
    public static void Init()
    {
        C64.CopyMemory(C64Address.FromLabel("tbl_HiScores"), C64Address.FromLabel("tbl_HiScoreInit"), 50);
    }

    // The league screen. entry: for the name registration (nothing at the
    // bottom).
    public static void Show(bool entry)
    {
        Screen.Clear(Colors.White);
        ulong table = C64Address.FromLabel("tbl_HiScores");
        Screen.Text(16, 0, C64Address.FromLabel("txt_HiScore"), 7, Colors.White);
        Screen.Text(16, 1, table, 7, Colors.White);
        // the bands of colour: two rows each, from row 3
        ulong colours = C64Address.FromLabel("(colorMemory+120)");
        for (uint band = 0; band < 6; band++)
        {
            C64.FillMemory(colours, C64.GetMemory(C64Address.FromLabel("tbl_HiScoreColours"), 5 - band), 80);
            colours += 80;
        }
        C64.CopyMemory(C64Address.FromLabel("(screenMemory+129)"), C64Address.FromLabel("txt_League"), 7);
        C64.CopyMemory(C64Address.FromLabel("(screenMemory+169)"), C64Address.FromLabel("txt_Order"), 7);
        C64.CopyMemory(C64Address.FromLabel("(screenMemory+137)"), C64Address.FromLabel("txt_Score"), 7);
        C64.CopyMemory(C64Address.FromLabel("(screenMemory+146)"), C64Address.FromLabel("txt_Name"), 7);
        ulong row = C64Address.FromLabel("(screenMemory+200)");
        for (uint place = 0; place < 5; place++)
        {
            C64.CopyMemory(row + 10, C64Address.FromLabel("txt_No"), 3);
            C64.FillMemory(row + 13, 49 + place, 1);
            C64.CopyMemory(row + 17, table, 7);
            C64.CopyMemory(row + 27, table + 7, 3);
            row += 80;
            table += 10;
        }
        Screen.Text(11, 16, C64Address.FromLabel("txt_BonusMan"), 18, Colors.Red);
    }

    // After a game: if the score (in hundreds) makes the league, the
    // player's three letters are asked for and the league shown.
    public static void GameOver(ulong score)
    {
        Screen.Wait(240);
        C64.Sprites.VisibleMask = 0;

        // The score as seven digits, the way the league holds them: written
        // to the screen's last row in black and taken from there.
        C64.Screen.Write(0, 24, score.ToString().PadLeft(5, '0'), Colors.Black);
        C64.Screen.Write(5, 24, "00", Colors.Black);
        ulong digits = C64Address.FromLabel("(tmpScreen+40)");
        C64.CopyMemory(digits, C64Address.FromLabel("(screenMemory+960)"), 7);

        ulong table = C64Address.FromLabel("tbl_HiScores");
        uint place = 0;
        while (place < 5 && !Beats(digits, table + place * 10))
            place++;
        if (place == 5)
            return;

        // room for it: the entries below move down, the last one out
        for (uint p = 4; p > place; p--)
            C64.CopyMemory(table + p * 10, table + p * 10 - 10, 10);
        ulong entry = table + place * 10;
        C64.CopyMemory(entry, digits, 7);
        C64.FillMemory(entry + 7, 32, 3);

        Show(true);
        Register();
        C64.CopyMemory(entry + 7, C64Address.FromLabel("(screenMemory+668)"), 3);
        Show(true);
        Screen.Wait(240);
        Screen.Wait(240);
    }

    // True when the seven digits at `digits` are a higher number than those
    // at `entry`.
    private static bool Beats(ulong digits, ulong entry)
    {
        for (uint i = 0; i < 7; i++)
        {
            uint mine = C64.GetMemory(digits, i);
            uint theirs = C64.GetMemory(entry, i);
            if (mine > theirs)
                return true;
            if (mine < theirs)
                return false;
        }
        return false;
    }

    // The name registration: left and right move along three rows of
    // letters, jump takes the letter (up to three; "RUB" takes one back,
    // "END" is the end).
    private static void Register()
    {
        Screen.Text(8, 16, C64Address.FromLabel("txt_NameRegistration"), 24, Colors.Red);
        Screen.Text(8, 19, C64Address.FromLabel("txt_LettersTopRow"), 24, Colors.White);
        Screen.Text(8, 21, C64Address.FromLabel("txt_LettersMiddleRow"), 24, Colors.White);
        Screen.Text(8, 23, C64Address.FromLabel("txt_LettersBottomRow"), 24, Colors.White);
        cursor_ = 0;
        letters_ = 0;
        ulong name = C64Address.FromLabel("(screenMemory+668)");
        bool done = false;
        while (!done)
        {
            Highlight(Colors.Red);
            Screen.Wait(12);
            while (!Input.Fire && !Input.Left && !Input.Right)
                Screen.Wait(1);
            if (Input.Fire)
            {
                if (cursor_ == 29)
                {
                    done = true;
                }
                else if (cursor_ == 19)
                {
                    C64.FillMemory(name + letters_, 32, 1);
                    if (letters_ != 0)
                        letters_--;
                }
                else
                {
                    C64.FillMemory(name + letters_, C64.GetMemory(C64Address.FromLabel("tbl_HiScoreChars"), cursor_), 1);
                    if (letters_ != 2)
                        letters_++;
                }
                while (Input.Fire)
                    Screen.Wait(1);
            }
            else
            {
                Highlight(Colors.White);
                if (Input.Right)
                    cursor_ = cursor_ == 29 ? 0 : cursor_ + 1;
                else
                    cursor_ = cursor_ == 0 ? 29 : cursor_ - 1;
            }
        }
    }

    private static void Highlight(Colors color)
    {
        uint at = C64.GetMemory(C64Address.FromLabel("tbl_HiScoreCursorScnPos"), cursor_);
        C64.FillMemory(C64Address.FromLabel("(colorMemory+768)") + at, (uint)color, 1);
    }
}
