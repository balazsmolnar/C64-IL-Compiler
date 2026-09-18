using C64Lib;
using System;

namespace Hunchback;

class Game
{
    public void Start()
    {
        Init();
        for (; ; )
        {
            TitleScreen.Display();
            RunGame();
        }
    }

    private void RunGame()
    {
        var levelPlay = new LevelPlay();
        var currentLevel = 0;
        var levels = LevelDescription.Levels;
        var playerStats = new PlayerStats { Lives = 5 };
        while (currentLevel < levels.Length)
        {
            Screen.Clear(Colors.Grey2);

            if (levelPlay.Play(levels[currentLevel], playerStats, (uint)currentLevel))
            {
                currentLevel++;
                Delay.Wait(100);
            }
            else if (playerStats.Lives == 0)
            {
                GameOver();
                return;
            }
            GC.Collect();
        }
    }

    private static void GameOver()
    {
        C64.Screen.Write(15, 12, "GAME OVER", Colors.Red);
        for (int i = 0; i < 60; i++)
            Delay.Wait(100);
    }

    private void Init()
    {
        C64.Screen.SetCharSet(C64Address.FromLabel("charset"));
        C64.Screen.SetMultiColor();
        C64.Screen.SetBackgroundColor(Colors.Black);
        C64.Screen.SetBorderColor(Colors.Black);
        C64.Screen.SetCharBackgroundColor(0, Colors.Grey1);
        C64.Screen.SetCharBackgroundColor(1, Colors.White);
        C64.Sprites.CommonColor1 = Colors.Brown;
        C64.Sprites.CommonColor2 = Colors.Grey3;

        C64.Sound.Volume = 15;
    }
}