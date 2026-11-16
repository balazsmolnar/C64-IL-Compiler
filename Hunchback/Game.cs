using C64Lib;
using System;

namespace Hunchback;

class Game
{
    public void Start()
    {
        Init();
        HiScores.Init();
        for (; ; )
        {
#if !SCENE
            TitleScreen.Display();
            Intro.Play(false);
#endif
            RunGame();
        }
    }

    // One game: five lives, from the first wall on. The 48 walls come round
    // again after the last, for as long as the lives last.
    private void RunGame()
    {
        var levelPlay = new LevelPlay();
        uint currentLevel = 0;
#if SCENE_BELLS
        currentLevel = 9;
#elif SCENE_ROPE
        currentLevel = 1;
#elif SCENE_PITS
        currentLevel = 3;
#elif SCENE_TOWER
        currentLevel = 15;
#elif SCENE_PAIR
        currentLevel = 14;
#endif
        var playerStats = new PlayerStats { Lives = 5 };
        bool scrollIn = false;
        for (; ; )
        {
            if (levelPlay.Play(LevelDescription.Get(currentLevel), playerStats, currentLevel, scrollIn))
            {
                // Every sixteenth wall ends at Esmerelda's tower: she is
                // rescued, and the next wall is simply there.
                scrollIn = (currentLevel & 15) != 15;
                if (!scrollIn)
                    LevelPlay.Rescue(playerStats);
                currentLevel++;
                if (currentLevel == LevelDescription.Count)
                    currentLevel = 0;
            }
            else
            {
                scrollIn = false;
                if (playerStats.Lives == 0)
                {
                    Screen.Text(16, 6, C64Address.FromLabel("txt_GameOver"), 9, Colors.White);
                    HiScores.GameOver(playerStats.Score);
                    return;
                }
            }
            GC.Collect();
        }
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
