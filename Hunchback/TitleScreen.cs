using C64Lib;

namespace Hunchback;

// Everything before a game: the title, the hi-score league and the demo,
// round and round until SPACE (or fire) starts a game; F1 shows the
// instructions (Restructure/Menu.asm). The original watches those two keys
// from an interrupt and jumps out of whatever is on; here every wait is a
// Pause, which watches them, and every screen gives up as soon as one is
// down.
class TitleScreen
{
    private static uint key_;       // 1 = F1, 2 = SPACE or fire

    // Returns when a game is to start.
    public static void Display()
    {
        for (; ; )
        {
            key_ = 0;
            Title();
            if (key_ == 0)
            {
                HiScores.Show(false);
                StartText();
                Pause(15);
            }
            if (key_ == 0)
                key_ = Intro.Play(true);
            if (key_ == 1)
                Instructions();
            if (key_ == 2)
            {
                C64.Sprites.VisibleMask = 0;
                while (C64.IsKeyPressed(Keys.Space) || Input.Fire)
                    Screen.Wait(1);
                return;
            }
        }
    }

    private static void Title()
    {
        C64.Sprites.VisibleMask = 0;
        Screen.Clear(Colors.White);
        StartText();
        C64.CopyMemory(C64Address.FromLabel("(screenMemory+$CD)"), C64Address.FromLabel("titleScreen"), 0);
        C64.CopyMemory(C64Address.FromLabel("(screenMemory+$1CD)"), C64Address.FromLabel("titleScreen+$100"), 0);
        C64.CopyMemory(C64Address.FromLabel("(screenMemory+$21B)"), C64Address.FromLabel("titleScreen+$14E"), 0);
        Screen.Text(13, 1, C64Address.FromLabel("txt_OceanSoftware"), 14, Colors.Green);
        Screen.Text(13, 3, C64Address.FromLabel("txt_Presents"), 14, Colors.White);
        Screen.Text(25, 21, C64Address.FromLabel("txt_ByJSteele"), 14, Colors.Green);

        // the title runs through the colours, then stands
        uint color = (uint)Colors.White;
        for (uint i = 0; i < 17; i++)
        {
            color = (color + 1) & 7;
            C64.FillMemory(C64Address.FromLabel("(colorMemory+$CD)"), color, 0);
            C64.FillMemory(C64Address.FromLabel("(colorMemory+$1CD)"), color, 0);
            C64.FillMemory(C64Address.FromLabel("(colorMemory+$21B)"), color, 0);
            if (Pause(1))
                return;
        }
        Pause(16);
    }

    private static void StartText()
    {
        Screen.Text(7, 23, C64Address.FromLabel("txt_F1Instructions"), 25, Colors.White);
        Screen.Text(7, 24, C64Address.FromLabel("txt_SpaceToStart"), 25, Colors.White);
    }

    private static void Instructions()
    {
        C64.Sprites.VisibleMask = 0;
        Screen.Clear(Colors.White);
        while (C64.IsKeyPressed(Keys.F1))
            Screen.Wait(1);
        key_ = 0;
        ulong text = C64Address.FromLabel("txt_Instructions");
        Screen.Text(7, 1, text, 26, Colors.White);
        Screen.Text(3, 4, C64Address.FromLabel("txt_InstructionsJoyKeys"), 34, Colors.White);
        Screen.Text(7, 5, text + 26, 26, Colors.White);
        Screen.Text(7, 8, text + 52, 26, Colors.White);
        Screen.Text(7, 11, text + 78, 26, Colors.White);
        Screen.Text(7, 14, text + 104, 26, Colors.White);
        Screen.Text(7, 17, text + 130, 26, Colors.White);
        Bell(20, 10);       // "hero wins a (bell) each time"
        Bell(12, 16);       // "five (bells) gives super bonus"
        Pause(31);
    }

    private static void Bell(uint x, uint y)
    {
        C64.Screen.SetChar(x, y, 63, Colors.Grey2);
        C64.Screen.SetChar(x + 1, y, 64, Colors.Grey2);
        C64.Screen.SetChar(x, y + 1, 65, Colors.Grey2);
        C64.Screen.SetChar(x + 1, y + 1, 66, Colors.Grey2);
    }

    // Waits `thirds` thirds of a second, or until F1 or SPACE (or fire) is
    // down: then it returns true, and key_ says which.
    private static bool Pause(uint thirds)
    {
        for (uint i = 0; i < thirds; i++)
        {
            for (uint tick = 0; tick < 20; tick++)
            {
                if (C64.IsKeyPressed(Keys.F1))
                    key_ = 1;
                if (C64.IsKeyPressed(Keys.Space) || Input.Fire)
                    key_ = 2;
                if (key_ != 0)
                    return true;
                C64.Delay(1);
            }
        }
        return false;
    }
}
