using C64Lib;
using System;

namespace Hunchback
{
    class Game
    {
        public void Start()
        {
            Init();
            TitleScreen.Display();
            Screen.Clear(Colors.Grey2);

            var levelPlay = new LevelPlay();
            var currentLevel = 0;
            var levels = LevelDescription.Levels;
            var playerStats = new PlayerStats();
            while (currentLevel < levels.Length)
            {

                if (levelPlay.Play(levels[currentLevel], playerStats))
                {
                    currentLevel++;
                    Delay.Wait(100);
                }
                GC.Collect();
            }
        }

        private void Init()
        {
            C64.SetCharSet(C64Address.FromLabel("charset"));
            C64.SetMultiColor();
            C64.SetBackgroundColor(Colors.Black);
            C64.SetBorderColor(Colors.Black);
            C64.SetCharBackgroundColor(0, Colors.Grey1);
            C64.SetCharBackgroundColor(1, Colors.White);
            C64.Sprites.CommonColor1 = Colors.Brown;
            C64.Sprites.CommonColor2 = Colors.Grey3;
        }
    }
}