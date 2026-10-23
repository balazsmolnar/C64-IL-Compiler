using C64Lib;

namespace Catacombs;

class Program
{
    static uint px_, py_;   // current maze cell (see Maze.cs) -- rooms are
                            // always rendered north-up (see DungeonView's
                            // own comment), so there's no separate facing
                            // to track anymore.

    // Player's logical position within the CURRENT room -- see
    // RoomProjection.cs for what these mean and how they become a screen
    // position (roomX_ never itself needs to shrink near the far wall:
    // the projection does that, so hugging a side wall while walking
    // deeper naturally drifts the ON-SCREEN sprite toward center without
    // this code ever computing that drift itself).
    static uint roomX_, roomDepth_;
#if SCENE_TIMING || SCENE_TIMINGM
    static uint timing_;
#endif
    static uint playerHp_;
    static uint playerDamage_; // set in Main()/PlayerDied(), raised by Weapon.DamageAt on pickup
    const uint PlayerMaxHp = 100;
    const uint UnarmedDamage = 6;
    const uint Speed = 1;   // room units per video frame: 2-3 pixels across, a smooth walk

    // Cached once per room entry (EnterRoom) instead of recomputed every
    // frame -- HandleInput just reads these. Compass-absolute, matching
    // DungeonView's own fixed north-up orientation.
    static bool westOpen_, eastOpen_, northOpen_, southOpen_;
    static bool monsterHere_, itemHere_, weaponHere_;

    // The current room's own color (set by ApplyRoomColor) -- kept around
    // so UpdateCollision can restore the border to it after a collision
    // flash, without recomputing Maze.RoomColor every frame.
    static Colors roomColor_;

    static void Main()
    {
        // Scene builds ("dotnet build -p:Scene=COMBAT", see Catacombs.csproj
        // and tools/catacombs-shot.ps1) boot straight into a state that
        // otherwise takes playing to reach.
#if SCENE_TECHDEMO
        TechDemo.Run(); // never returns: the graphics tech showcase (TechDemo.cs)
#endif
#if SCENE_ROOM4 || SCENE_DEADEND || SCENE_THEME0 || SCENE_THEME1 || SCENE_THEME2 || SCENE_THEME3 || SCENE_THEME4 || SCENE_DOOROPEN || SCENE_SIDEOPEN || SCENE_ROOMTOUR
        RoomScenes.Run(); // never returns: the room scenes (RoomScenes.cs)
#endif
#if SCENE_CHARS
        CharScenes.Run(); // never returns: the sprite scenes, "-Scene CHAR_..." (CharScenes.cs)
#endif
#if SCENE_HUDFULL || SCENE_HUDMID || SCENE_HUDLOW || SCENE_HUDHALF
        HudScene.Run(); // never returns: the status panel in one fixed state (Hud.cs)
#endif
        // Title -> a run of the game -> game over / victory -> title, for
        // ever (GameFlow.cs, TitleScreen.cs). Play() starts every run from
        // scratch.
        Audio.Init();
        for (;;)
        {
            Audio.Title();
            GameFlow.BeforeGame();
            Play();
            GameFlow.AfterGame(playerHp_);
        }
    }

    // One run: sets the whole game up, then plays until GameFlow says it is
    // over (the player died, or escaped with the relics).
    static void Play()
    {
#if SCENE_COMBAT || SCENE_MONSTERROOM || SCENE_TIMINGM
        px_ = 5;
        py_ = 3;
#else
        px_ = 1;
        py_ = 1;
#endif
        playerHp_ = PlayerMaxHp;
        playerDamage_ = UnarmedDamage;
        Audio.Explore();

        // MultiColor (not plain Bitmap) so doors can be a color distinct
        // from the lines: MatrixHigh=White (wall/outline lines),
        // ColorRam=Brown (doors), MatrixLow=LightRed (the monster
        // silhouette) -- all three fixed for the whole game, set once
        // here. Everything else -- the screen background ($D021, what
        // every unset pixel shows: walls, floor, ceiling) and the border
        // ($D020) -- is the current room's own color (Maze.RoomColor, see
        // ApplyRoomColor), so the room reads as lines and doors on one
        // solid color, with no visible frame around it.
        C64.Screen.SetScreenMode(ScreenMode.MultiColor);
        ApplyRoomColor();
        C64.Screen.SetBitmapColors(0x1A);
        var colorRam = 0xD800UL;
        for (ulong offset = 0; offset < 1000UL; offset += 250UL)
            C64.FillMemory(colorRam + offset, (uint)Colors.Brown, 250);
        SpriteStage.Init(); // before everything that puts sprites on it
        Spider.Init();
        Spider.SetRoomColor(Maze.RoomColorValue(px_, py_));
        Monster.Init();
        Item.Init();
        Weapon.Init();
        Minimap.Init(); // the HUD: a new run's map
        Hud.SetHealth(playerHp_);
        Hud.SetWeapon(playerDamage_);

        roomX_ = RoomProjection.Center;
#if SCENE_TIMING
        roomX_ = 18;
#elif SCENE_TIMINGM
        roomX_ = 5;     // along the left wall, clear of the room's guardian
#endif
        roomDepth_ = 0;
        PlayerSprite.Init(RoomProjection.ScreenX(roomX_, roomDepth_), RoomProjection.ScreenY(roomDepth_));

        EnterRoom();
#if SCENE_COMBAT
        Combat();
#endif
#if SCENE_ENCOUNTER
        Encounter.Scene(); // never returns: the encounter's states on a loop (Encounter.cs)
#endif

        while (GameFlow.Playing)
        {
            C64.Screen.WaitForVBlank();
#if SCENE_TIMING || SCENE_TIMINGM
            C64.Screen.SetBorderColor(Colors.White); // the loop's work shows as a white band in the border
            TimingProbe.Start();
#endif
            DungeonView.Animate(); // torch flicker: first, while the beam is off screen
            DungeonView.Cue(DoorAt()); // the door the player stands at pulses
#if SCENE_TIMING || SCENE_TIMINGM
            TimingProbe.Mark(0);
#endif
            Spider.Animate();
#if SCENE_TIMING || SCENE_TIMINGM
            TimingProbe.Mark(1);
#endif
            Monster.Animate(px_, py_);
#if SCENE_TIMING || SCENE_TIMINGM
            TimingProbe.Mark(2);
#endif
            UpdateZOrder();
#if SCENE_TIMING || SCENE_TIMINGM
            TimingProbe.Mark(3);
#endif
            Hud.Tick();
#if SCENE_TIMING || SCENE_TIMINGM
            TimingProbe.Mark(4);
#endif
            HandleInput();
#if SCENE_TIMING || SCENE_TIMINGM
            C64.Screen.SetBorderColor(Colors.Black);
            TimingProbe.Mark(5);
            TimingProbe.Report();
#endif
        }
    }

    // Background and border both take the current room's color.
    static void ApplyRoomColor()
    {
        roomColor_ = Maze.RoomColor(px_, py_);
        C64.Screen.SetBorderColor(roomColor_);
        C64.Screen.SetBackgroundColor(roomColor_);
        Spider.SetRoomColor(Maze.RoomColorValue(px_, py_));
    }

    // (The old green-border door cue is gone: the border stays black, see
    // AAA-BRIEF.md. A door prompt belongs in the room art or the HUD.)

    // Animates the player and hands every sprite out to the hardware,
    // nearest first (SpriteStage.cs).
    static void UpdateZOrder()
    {
        SpriteStage.Update();
    }

    // Draws the room's walls/doors ONCE (see DungeonView's own comment on
    // why this never happens per frame) and caches what's open/present in
    // it. Also (re)positions the player sprite -- called both on a fresh
    // room entry and after combat removes a monster from the current one.
    //
    // No double buffering: single-buffer bitmap mode saves ~9KB of address
    // space (no second bitmap/matrix reservation) and -- just as
    // important -- collapses sprite data back down to ONE copy instead of
    // one per VIC bank, which is what was actually blocking more sprite
    // frames (see Sprites.cs). The bitmap is only ever drawn to here, at
    // room setup, never per frame (movement is pure sprite-register pokes,
    // see PlayerSprite/DungeonView's own comments) -- so the brief
    // in-progress flicker while these dozen-odd vector calls run is a
    // rare, one-off cost at a room transition, not a per-frame one.
    static void EnterRoom()
    {
        westOpen_ = !Maze.WestIsWall(px_, py_);
        eastOpen_ = !Maze.EastIsWall(px_, py_);
        northOpen_ = !Maze.NorthIsWall(px_, py_);
        southOpen_ = !Maze.SouthIsWall(px_, py_);
        monsterHere_ = Monster.IsAliveAt(px_, py_);
        itemHere_ = Item.IsPresentAt(px_, py_);
        weaponHere_ = Weapon.IsPresentAt(px_, py_);

        DungeonView.Render(px_, py_); // a whole picture, drawn with the screen blanked: nothing to clear first
        if (itemHere_)
            Item.Render();
        if (weaponHere_)
            Weapon.Render();
        Minimap.Enter(px_, py_);
        Hud.Render(); // the whole panel: ClearBitmap above wiped it

        Monster.Animate(px_, py_); // shows/hides/positions sprite 3 for this room immediately, rather than waiting a frame
        UpdateSpritePosition();
        UpdateZOrder();
    }

    static void UpdateSpritePosition()
    {
        PlayerSprite.SetPosition(RoomProjection.ScreenX(roomX_, roomDepth_), RoomProjection.ScreenY(roomDepth_));
    }

    // W/S/A/D move the player continuously in ROOM space while held (no
    // wait-for-release -- that's what makes this feel like free movement
    // rather than a discrete step per press). roomX_/roomDepth_ are each
    // clamped to their own fixed [0,100] range regardless of the other --
    // RoomProjection is what makes that still look right against the
    // room's converging walls.
    static void HandleInput()
    {
        uint nx = roomX_, nd = roomDepth_;
        bool moved = false;

#if SCENE_TIMING || SCENE_TIMINGM
        // Timing scenes: walk diagonally back and forth by themselves, the
        // most expensive frame (both coordinates re-projected every frame).
        timing_ = timing_ + 1;
        moved = true;
        if ((timing_ & 64) == 0)
        {
            nd = roomDepth_ + Speed;
#if SCENE_TIMING
            nx = roomX_ + Speed;
#endif
        }
        else
        {
            nd = roomDepth_ - Speed;
#if SCENE_TIMING
            nx = roomX_ - Speed;
#endif
        }
        if (false)
#endif
        if (C64.IsKeyPressed(Keys.W))
        {
            nd = roomDepth_ + Speed < RoomProjection.DepthMax ? roomDepth_ + Speed : RoomProjection.DepthMax;
            moved = true;
        }
        else if (C64.IsKeyPressed(Keys.S))
        {
            nd = roomDepth_ > Speed ? roomDepth_ - Speed : 0;
            moved = true;
        }
        else if (C64.IsKeyPressed(Keys.A))
        {
            nx = roomX_ > Speed ? roomX_ - Speed : 0;
            moved = true;
        }
        else if (C64.IsKeyPressed(Keys.D))
        {
            nx = roomX_ + Speed < RoomProjection.Width ? roomX_ + Speed : RoomProjection.Width;
            moved = true;
        }

        if (moved)
        {
            roomX_ = nx;
            roomDepth_ = nd;
        }
        Audio.Step(moved);

        // Checked every frame, not just when moved: the player can stop
        // right at a door (Audio.Door is the cue)
        // and press SPACE separately, same as how Combat's own SPACE
        // press works independently of movement.
        if (TryCrossDoor())
        {
            while (C64.IsKeyPressed(Keys.Space))
                Delay.Wait(100);
            return; // Step already redrew/repositioned for the new room
        }

        if (!moved)
            return;

        if (monsterHere_ && Touching(Monster.CurrentRoomX(px_, py_), Monster.RoomDepth, Monster.RoomRadius))
        {
            if (!Combat())
                return; // player died -- PlayerDied already reset everything
        }
        else if (itemHere_ && Touching(Item.RoomX, Item.RoomDepth, Item.RoomRadius))
        {
            Item.CollectAt(px_, py_);
            Audio.Pickup();
            itemHere_ = false;
            Hud.SetTreasure(Item.CollectedCount);
        }
        else if (weaponHere_ && Touching(Weapon.RoomX, Weapon.RoomDepth, Weapon.RoomRadius))
        {
            playerDamage_ = Weapon.DamageAt(px_, py_);
            Weapon.CollectAt(px_, py_);
            Audio.WeaponPickup();
            weaponHere_ = false;
            Hud.SetWeapon(playerDamage_);
        }

        UpdateSpritePosition();
    }

    // Touch radius is given in SCREEN pixels (matching how big the marker
    // actually looks), so the comparison projects the player's room-space
    // position to screen-space first rather than comparing room-space
    // units against a pixel radius directly.
    static bool Touching(uint x, uint depth, ulong radius)
    {
        // What is at (x, depth) -- this room's guardian, treasure or weapon --
        // is on the sprite stage, which already knows where both it and the
        // player are on screen: no projecting anything again.
        return SpriteStage.PlayerAtFeature(radius);
    }

    // True while standing right at an open door's crossing edge -- the
    // same position checks TryCrossDoor itself uses, split out so
    // TryCrossDoor can sound the door without actually
    // crossing.
    static bool AtDoor()
    {
        return DoorAt() != 0;
    }

    // Which door (DungeonView's door bit) the player stands at, 0 if none.
    static uint DoorAt()
    {
        if (roomDepth_ >= RoomProjection.DepthMax && northOpen_ && NearCenterX())
            return DungeonView.North;
        if (roomDepth_ == 0 && southOpen_ && NearCenterX())
            return DungeonView.South;
        if (roomX_ == 0 && westOpen_)
            return DungeonView.West;
        if (roomX_ >= RoomProjection.Width && eastOpen_)
            return DungeonView.East;
        return 0;
    }

    // Crossing is now a deliberate action (SPACE), not automatic on
    // reaching the edge -- standing at the door is the signal
    // that SPACE will do something here. Fixed orientation (see
    // DungeonView's own comment) means crossing any given wall always
    // lands at the SAME edge of the next room -- the one opposite the
    // wall just crossed, since that's the wall the player is now standing
    // just inside of: north exit -> arrive at the new room's south edge,
    // south exit -> its north edge, west exit -> its east edge, east exit
    // -> its west edge (the common "enter from the left" case).
    static bool TryCrossDoor()
    {
        if (!C64.IsKeyPressed(Keys.Space))
            return false;
        if (AtDoor()) Audio.Door();

        if (roomDepth_ >= RoomProjection.DepthMax && northOpen_ && NearCenterX())
        {
            AnimateDoorOpening(DungeonView.North);
            Step(px_, py_ - 1, RoomProjection.Center, 0);
            return true;
        }
        if (roomDepth_ == 0 && southOpen_ && NearCenterX())
        {
            Step(px_, py_ + 1, RoomProjection.Center, RoomProjection.DepthMax);
            return true;
        }
        if (roomX_ == 0 && westOpen_)
        {
            AnimateDoorOpening(DungeonView.West);
            Step(px_ - 1, py_, RoomProjection.Width, 0);
            return true;
        }
        if (roomX_ >= RoomProjection.Width && eastOpen_)
        {
            AnimateDoorOpening(DungeonView.East);
            Step(px_ + 1, py_, 0, 0);
            return true;
        }
        return false;
    }

    static bool NearCenterX()
    {
        uint dx = roomX_ > RoomProjection.Center ? roomX_ - RoomProjection.Center : RoomProjection.Center - roomX_;
        return dx < 20;
    }

    // Plays a door (north, west or east) swinging open (DungeonView.OpenDoor:
    // two steps, ten frames apart) before the player steps through, with the
    // room's own animation and the sprites kept going meanwhile.
    static void AnimateDoorOpening(uint door)
    {
        for (uint i = 0; i < 30; i++)
        {
            C64.Screen.WaitForVBlank();
            DungeonView.Animate();
            if (i == 0)
                DungeonView.OpenDoor(door, 0);
            if (i == 10)
                DungeonView.OpenDoor(door, 1);
            Spider.Animate();
            UpdateZOrder();
        }
    }

    // Moves to (newPx,newPy) and places the player at the given room-space
    // entry point (the edge of the NEW room corresponding to whichever
    // wall was just crossed -- see TryCrossDoor's own comment).
    static void Step(uint newPx, uint newPy, uint entryRoomX, uint entryRoomDepth)
    {
        px_ = newPx;
        py_ = newPy;
        if (GameFlow.Escaped(px_, py_))
            return; // out with every relic: the run is won
        roomX_ = entryRoomX;
        roomDepth_ = entryRoomDepth;
        ApplyRoomColor();
        EnterRoom();
    }

    // Turn-based exchange with the monster guarding this room: SPACE
    // attacks, the monster hits back if it survives. Returns true once the
    // monster dies (and has already redrawn the room without it), false if
    // the player dies first (PlayerDied has already reset the run by the
    // time this returns).
    static bool Combat()
    {
        // The fight itself is Encounter's (Encounter.cs); it leaves the
        // viewport black, and DungeonView redraws the room behind its own
        // blank.
        Audio.CombatStart();
        Monster.HideSprite(); // the patrolling guardian: the encounter has its own sprites
        playerHp_ = Encounter.Fight(px_, py_, playerHp_, playerDamage_);
        // No blank: the HUD stays on screen (combat). The room is redrawn first
        // so a dead player falls where he stood, in the room (characters).
        ApplyRoomColor();
        EnterRoom(); // redraw (without the monster, if it died)
        if (playerHp_ == 0)
        {
            PlayerDied();
            return false;
        }
        return true;
    }

    // A flash, then the run is over: nothing is reset here, the next run's
    // Play() sets everything up again.
    static void PlayerDied()
    {
        Audio.PlayerDied();
        PlayerSprite.Fall();
        for (uint f = 0; f < 120; f++) // he lies there, the torch's ember cooling off
        {
            C64.Screen.WaitForVBlank();
            SpriteStage.Update();
        }
        for (uint i = 0; i < 6; i++)
        {
            C64.Screen.SetBorderColor(Colors.Red);
            Delay.Wait(80);
            C64.Screen.SetBorderColor(Colors.Black);
            Delay.Wait(80);
        }
        GameFlow.Lose(); // ends Play(); Main shows the game over screen, then the title
    }
}
