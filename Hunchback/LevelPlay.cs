using System.Security.Cryptography.X509Certificates;
using C64Lib;

namespace Hunchback;

class LevelPlay
{
    public bool Play(LevelDescription description, PlayerStats playerStats, uint levelNumber)
    {
        Wall wall = new Wall();
        wall.Draw(description.Color, description.WallType);
        playerStats.Draw(levelNumber);
        DrawDebugLevelNumber(levelNumber);

        // Rope/Enemy/Enemy2 aren't set up until after GetReady() below (they
        // shouldn't appear until gameplay actually starts) -- but Screen.Clear()
        // only wiped the sprite POINTER bytes for the new level, not the
        // separate hardware enable bits, so whatever was left enabled from
        // the previous level's rope/enemies would otherwise still show,
        // now pointing at this level's (blank) pointer value, for the whole
        // Get Ready pause. Disable them upfront so nothing stale is visible
        // until each one's own Init() turns it back on with real data.
        C64.Sprites.Sprite2.Visible = false;
        C64.Sprites.Sprite3.Visible = false;
        C64.Sprites.Sprite4.Visible = false;
        C64.Sprites.Sprite5.Visible = false;

        Player player = new Player()
        {
            Sprite = C64.Sprites.Sprite0
        };
        player.Init(wall);
        Knight knight = new Knight()
        {
            Sprite = C64.Sprites.Sprite1
        };
        knight.Init();
        GetReady();
        Rope rope = null;
        if (description.WallType == WallType.Rope)
        {
            rope = new Rope
            {
                Sprite1 = C64.Sprites.Sprite3,
                Sprite2 = C64.Sprites.Sprite4,
            };
            rope.Init();
        }
        else
        {
            C64.Sprites.Sprite3.Visible = false;
            C64.Sprites.Sprite4.Visible = false;
        }

        Enemy enemy = new Enemy()
        {
            Sprite = C64.Sprites.Sprite2,
            EnemyType = description.EnemyType

        };
        enemy.Init();
        Enemy enemy2 = null;
        if (description.EnemyType2 != EnemyType.None)
        {
            enemy2 = new Enemy()
            {
                Sprite = C64.Sprites.Sprite5,
                EnemyType = description.EnemyType2
            };
            enemy2.Init();
            enemy2.MoveDelay = 20;
        }
        else
        {
            C64.Sprites.Sprite5.Visible = false;
        }
        var gameObjects = new GameObject[] { enemy, enemy2, knight, rope, wall };
        // Counts game-loop iterations for this level, feeding
        // LevelScoreMultiplier below -- the original's speed-bonus
        // equivalent (Restructure/Enemy.asm's knightCounter, incremented
        // once per knight-animation tick; Restructure/Player.asm's
        // Player_LevelCompleteScore reads it at completion time). Capped
        // rather than left to wrap: uint is 1 byte in this compiler, and an
        // uncapped wrap back to a small value would wrongly reward a slow
        // completion with the fast-finish bonus.
        uint ticks = 0;
        for (; ; )
        {
            player.Move();
            if (ticks < 255)
                ticks++;

            var collisions = C64.Sprites.Collisions;
            if ((collisions & 1u) > 0)
            {
                if ((collisions & 38u) > 0)  // Player-enemy collision (Knight, Enemy, or Enemy2)
                {
                    player.Die();
                }
                else
                {
                    player.SetOnRope(rope);
                }
            }
            if (player.Dead)
            {
                playerStats.Lives--;
                playerStats.Bonus = 0;
                playerStats.DrawLives();
                playerStats.DrawBonus();
                return false;
            }
            if (player.Complete)
            {
                // Speed-based per-level score, same shape as the original's
                // Player_LevelCompleteScore: a multiplier looked up from how
                // long the level took (tbl_ScoreMultiplier there), scaled by
                // (level index + 1) -- later levels are worth more. Shown in
                // its own on-screen box before folding into the running
                // total shown in the header.
                ulong pointsThisLevel = LevelScoreMultiplier(ticks) * (ulong)(levelNumber + 1);
                playerStats.Score += pointsThisLevel;
                // Hide the knight -- the box is drawn right at its own
                // position (see ShowLevelScoreBox), so left visible it sits
                // on top of/next to the box's own corner.
                C64.Sprites.Sprite1.Visible = false;
                ShowLevelScoreBox(knight.X, knight.Y, pointsThisLevel);

                playerStats.Bonus++;
                if (playerStats.Bonus == 5)
                {
                    BonusFanfare();
                    playerStats.Bonus = 0;
                    playerStats.Lives++;
                    // Matches the original's own super-bonus award exactly:
                    // Restructure/Quasi.asm's flying-hearts sequence calls
                    // Player_UpdateScore(digit 2) twice, i.e. +1 twice to
                    // the hundreds place of the BCD score array -- +200.
                    playerStats.Score += 200;
                    playerStats.DrawLives();
                }
                playerStats.DrawBonus();
                playerStats.DrawScore();
                for (int i=0; i<30; i++)
                    Delay.Wait(255);
                return true;
            }
            foreach (var go in gameObjects)
                go?.Move();

            Delay.Wait(100);
            // Debug/testing cheat: force the level complete instead of playing
            // it out. Sets Complete rather than returning true directly so it
            // still goes through the normal bonus/fanfare handling above.
            if (C64.IsKeyPressed(Keys.L))
                player.Complete = true;
        }
    }

    private static void GetReady()
    {
        // Save what's actually under the text (e.g. a row-of-bells rope
        // strand) instead of just blanking to spaces afterward, which used
        // to punch a visible gap through it.
        var c0 = (uint)C64.Screen.GetChar(16, 6);
        var c1 = (uint)C64.Screen.GetChar(17, 6);
        var c2 = (uint)C64.Screen.GetChar(18, 6);
        var c3 = (uint)C64.Screen.GetChar(19, 6);
        var c4 = (uint)C64.Screen.GetChar(20, 6);
        var c5 = (uint)C64.Screen.GetChar(21, 6);
        var c6 = (uint)C64.Screen.GetChar(22, 6);
        var c7 = (uint)C64.Screen.GetChar(23, 6);
        var c8 = (uint)C64.Screen.GetChar(24, 6);

        C64.Screen.Write(16, 6, "GET READY", Colors.White);
        for (int i = 0; i < 40; i++)
            Delay.Wait(100);

        // Grey2 matches the only decoration that can be here today (the
        // row-of-bells rope); harmless elsewhere since a space's color
        // isn't visible.
        C64.Screen.SetChar(16, 6, c0, Colors.Grey2);
        C64.Screen.SetChar(17, 6, c1, Colors.Grey2);
        C64.Screen.SetChar(18, 6, c2, Colors.Grey2);
        C64.Screen.SetChar(19, 6, c3, Colors.Grey2);
        C64.Screen.SetChar(20, 6, c4, Colors.Grey2);
        C64.Screen.SetChar(21, 6, c5, Colors.Grey2);
        C64.Screen.SetChar(22, 6, c6, Colors.Grey2);
        C64.Screen.SetChar(23, 6, c7, Colors.Grey2);
        C64.Screen.SetChar(24, 6, c8, Colors.Grey2);
    }

    // Debug aid: show the (0-based) level index at the top right of the
    // header, in the free columns after the lives markers.
    private static void DrawDebugLevelNumber(uint levelNumber)
    {
        uint tens = levelNumber / 10;
        uint ones = levelNumber % 10;
        C64.Screen.SetChar(36, 1, 76, Colors.Yellow); // 'L'
        C64.Screen.SetChar(37, 1, 48 + tens, Colors.Yellow);
        C64.Screen.SetChar(38, 1, 48 + ones, Colors.Yellow);
    }

    // Speed bonus lookup -- modeled on the original's tbl_ScoreMultiplier
    // (Restructure/Memory.asm), a descending set of thresholds searched to
    // find how many game-loop ticks the level took: fewer ticks (faster
    // completion) means a higher multiplier. Recalibrated for this port's
    // own tick granularity (one increment per LevelPlay.Play() main-loop
    // iteration, each already paced by a Delay.Wait(100)) rather than the
    // original's raw knight-animation-frame count, since the two aren't the
    // same unit -- the original's exact threshold values wouldn't mean the
    // same thing here.
    //
    // Values scaled to x10 of the original's own raw 1-8 index (that
    // routine literally stores the *loop index* that found a matching
    // threshold, via `stx scoreMultiplier`, into the per-level point total,
    // not the table's own byte values -- confirmed re-reading
    // Restructure/Player.asm's Player_LevelCompleteScore closely). Taken at
    // face value that reads as a tiny few-point-per-level bonus even at
    // high level numbers, which doesn't match: completing level 1 in the
    // real game awards a few hundred points, not 1-8.
    //
    // x10 (10-80), not x100 (100-800): PlayerStats.Score is ulong, 2 bytes
    // in this compiler (max 65535 -- see Compiler/TypeExtensions.cs's
    // GetStorageBytes()). pointsThisLevel is multiplier*(levelNumber+1), so
    // the running total across a full 48-level game is bounded by
    // multiplier * (1+2+...+48) = multiplier * 1176 in the worst case
    // (fastest tier on every single level). At x100's multiplier=800 that's
    // 940,800 -- overflows many times over. x10's multiplier=80 gives
    // 94,080 -- still technically over 65535 in that all-fastest-every-level
    // limit, but only reachable by winning every level at the single
    // fastest speed tier back to back; ordinary play (or even repeated
    // taps of the L debug-complete cheat, which still takes a few ticks
    // per press) stays comfortably under it. Two parallel static readonly
    // arrays, not one array of (threshold, multiplier) pairs -- this
    // compiler's array-literal support only bakes primitive-element arrays
    // (Compiler/ILStaticArrayInitializerPass.cs), not arrays of structs/
    // tuples.
    private static readonly uint[] TickThresholds = { 15, 25, 35, 50, 70, 100, 150 };
    private static readonly ulong[] ScoreMultipliers = { 80, 70, 60, 50, 40, 30, 20, 10 };

    private static ulong LevelScoreMultiplier(uint ticks)
    {
        for (uint i = 0; i < TickThresholds.Length; i++)
        {
            if (ticks < TickThresholds[i])
                return ScoreMultipliers[i];
        }
        return ScoreMultipliers[TickThresholds.Length];
    }

    // On-screen "SCORE" box shown right where the knight ended up, matching
    // the original's Player_LevelCompleteScore -- this port's Knight.Y uses
    // the exact same numbering that routine's knightY math assumes (197 at
    // climb start, 117 at the top, see Knight.cs), so the same
    // "(knightY-53)/8" pixel-to-row conversion carries over unchanged; the
    // column is centered 2 characters left of the knight's own column,
    // clamped so the whole 7-wide box stays on screen. The knight sprite
    // itself is hidden by the caller before this runs (see the
    // player.Complete branch above), since it would otherwise sit right on
    // top of/next to the box's own corner. Border/fill glyphs are the exact
    // codes from the original's tbl_ScoreBoxChars -- this port's charset
    // preserves the same numbering (confirmed: Wall.cs's
    // WallChar=62/Wall3DChar=60/Wall3DTopChar=59 already match those same
    // bytes from that table).
    private static void ShowLevelScoreBox(uint knightX, uint knightY, ulong points)
    {
        const uint TopLeft = 91, TopFill = 95, TopRight = 92;
        const uint LeftSideWall = 0x62, RightSideWall = 0x61;
        const uint BottomLeft = 93, BottomFill = 96, BottomRight = 94;

        // >> 3, not / 8 -- functionally identical (8 is a power of 2) and
        // kept as a shift rather than switched to / now that division
        // exists, since it's a fine, idiomatic optimization on its own
        // merits.
        uint col = 0;
        uint row = knightY > 53 ? (knightY - 53) >> 3 : 0;
        if (row > 20) row = 20;

        C64.Screen.SetChar(col, row, TopLeft, Colors.LightRed);
        for (uint x = 1; x < 6; x++)
            C64.Screen.SetChar(col + x, row, TopFill, Colors.LightRed);
        C64.Screen.SetChar(col + 6, row, TopRight, Colors.LightRed);

        C64.Screen.SetChar(col, row + 1, LeftSideWall, Colors.LightRed);
        C64.Screen.Write(col + 1, row + 1, "SCORE", Colors.White);
        C64.Screen.SetChar(col + 6, row + 1, RightSideWall, Colors.LightRed);

        C64.Screen.SetChar(col, row + 2, LeftSideWall, Colors.LightRed);
        DrawZeroPaddedScore(col + 1, row + 2, points);
        C64.Screen.SetChar(col + 6, row + 2, RightSideWall, Colors.LightRed);

        C64.Screen.SetChar(col, row + 3, BottomLeft, Colors.LightRed);
        for (uint x = 1; x < 6; x++)
            C64.Screen.SetChar(col + x, row + 3, BottomFill, Colors.LightRed);
        C64.Screen.SetChar(col + 6, row + 3, BottomRight, Colors.LightRed);
    }

    // Fills the box's 5-character digit field, leading-zero-padded (e.g.
    // "01800") to match the original's fixed-width BCD display.
    private static void DrawZeroPaddedScore(uint x, uint y, ulong points)
    {
        C64.Screen.Write(x, y, points.ToString().PadLeft(5, '0'), Colors.White);
    }

    private static void BonusFanfare()
    {
        for (uint i = 0; i < 5; i++)
        {
            C64.Sound.PlayEffectReg2(WaveForm.Triangle, 0x2864UL, 0x1000UL, 0x0a, 0x00, false);
            Delay.Wait(150);
        }
    }
}