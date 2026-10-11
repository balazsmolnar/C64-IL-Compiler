using C64Lib;

namespace Catacombs;

// The flow around a run of the game. Program.Main is
//
//     for (;;) { GameFlow.BeforeGame(); Play(); GameFlow.AfterGame(hp); }
//
// and Play()'s frame loop runs `while (GameFlow.Playing)`. A run ends by
// Lose() (Program.PlayerDied) or by Escaped() returning true (Program.Step):
// both only set the outcome, the stack unwinds normally to Main, and Play()
// starts every run from scratch (it re-runs every Init).
//
// The objective: recover all the relics (Item), then walk back to the room
// the run started in -- the way out.
//
// Scene builds (-p:Scene=NAME, see Catacombs.csproj) skip the title, so other
// scenes still boot straight into their state. The scenes of this file:
//   GAMEOVER, VICTORY  boot into that screen and stay on it
//   FLOW               title -> game -> game over -> title -> game -> victory,
//                      on a timer, for ever (no input needed)
//   RESTART            200 runs back to back, then the title: a run that
//                      leaked heap would stop with an OUT OF MEMORY fault
static class GameFlow
{
    const uint StartX = 1, StartY = 1;  // the entrance, and the way out

    static uint outcome_;               // 0 playing, else TitleScreen.GameOver / Victory
#if SCENE_FLOW || SCENE_RESTART
    static uint runs_;
    static uint clock_;
#endif

    public static bool Playing
    {
        get
        {
#if SCENE_FLOW
            // Three seconds of the real game, then lose and win in turn.
            clock_ = clock_ + 1;
            if (clock_ == 150)
                outcome_ = (runs_ & 1) == 1 ? TitleScreen.GameOver : TitleScreen.Victory;
#endif
#if SCENE_RESTART
            if (runs_ < 200)
                outcome_ = TitleScreen.GameOver;
#endif
            return outcome_ == 0;
        }
    }

    public static void BeforeGame()
    {
#if SCENE_GAMEOVER
        TitleScreen.Show(TitleScreen.GameOver, 1, 0);
#elif SCENE_VICTORY
        TitleScreen.Show(TitleScreen.Victory, 64, 0);
#elif SCENE_FLOW
        TitleScreen.Show(TitleScreen.Title, 0, 300);
        TitleScreen.FadeOut();
        runs_ = runs_ + 1;
        clock_ = 0;
#elif SCENE_RESTART
        runs_ = runs_ + 1;
        if (runs_ > 200)
        {
            TitleScreen.Show(TitleScreen.Title, 0, 0);
            TitleScreen.FadeOut();
        }
#elif !SCENE
        TitleScreen.Show(TitleScreen.Title, 0, 0);
        TitleScreen.FadeOut();
#endif
        outcome_ = 0;
    }

    public static void AfterGame(uint health)
    {
#if SCENE_RESTART
        if (runs_ <= 200)
            return;
#endif
        TitleScreen.FadeOut();
#if SCENE_FLOW
        TitleScreen.Show(outcome_, outcome_ == TitleScreen.Victory ? 64U : 1U, 300);
#else
        TitleScreen.Show(outcome_, outcome_ == TitleScreen.Victory ? health : Item.CollectedCount, 0);
#endif
        TitleScreen.FadeOut();
    }

    public static void Lose()
    {
        outcome_ = TitleScreen.GameOver;
    }

    // Called when the player steps into room (x, y): true (and the run is
    // over) once every relic is carried back to the entrance.
    public static bool Escaped(uint x, uint y)
    {
        if (x == StartX && y == StartY && Item.CollectedCount == Item.Count)
        {
            outcome_ = TitleScreen.Victory;
            return true;
        }
        return false;
    }
}
