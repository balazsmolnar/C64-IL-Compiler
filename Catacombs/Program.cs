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
    static uint playerHp_;
    static uint playerDamage_; // set in Main()/PlayerDied(), raised by Weapon.DamageAt on pickup
    const uint PlayerMaxHp = 100;
    const uint UnarmedDamage = 6;
    const uint Speed = 4;

    // Cached once per room entry (EnterRoom) instead of recomputed every
    // frame -- HandleInput just reads these. Compass-absolute, matching
    // DungeonView's own fixed north-up orientation.
    static bool westOpen_, eastOpen_, northOpen_, southOpen_;
    static bool monsterHere_, itemHere_, weaponHere_;

    // The current room's own color (set by ApplyRoomColor) -- kept around
    // so UpdateCollision can restore the border to it after a collision
    // flash, without recomputing Maze.RoomColor every frame.
    static Colors roomColor_;

    // Screen-pixel touch radius for the player-vs-spider check (see
    // Spider.IsCollidingWith) -- same order of magnitude as Monster's own
    // RoomRadius (20), which is the established precedent in this codebase
    // for a screen-space, non-perspective-scaled hit radius.
    const ulong SpiderCollisionRadius = 12;

    static void Main()
    {
        px_ = 1;
        py_ = 1;
        playerHp_ = PlayerMaxHp;
        playerDamage_ = UnarmedDamage;

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
        Spider.Init();
        Spider.SetRoomColor(Maze.RoomColorValue(px_, py_));
        Monster.Init();
        Item.Init();
        Weapon.Init();
        SpriteStage.Init();

        roomX_ = RoomProjection.Center;
        roomDepth_ = 0;
        PlayerSprite.Init(RoomProjection.ScreenX(roomX_, roomDepth_), RoomProjection.ScreenY(roomDepth_));

        EnterRoom();

        for (;;)
        {
            C64.Screen.WaitForVBlank();
            Spider.Animate();
            Monster.Animate(px_, py_);
            UpdateZOrder();
            UpdateBorderCue();
            HandleInput();
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

    // Border is the feedback channel for two things, checked every frame
    // regardless of whether the player moved this frame (the spiders move
    // on their own, and standing still at a door should still show the
    // cue): red while touching a spider (danger -- see Spider.
    // IsCollidingWith's own comment for why this is calculated in
    // screen-space rather than trusting VIC-II hardware sprite collision),
    // green while standing at an open door (AtDoor -- "press SPACE"),
    // back to the room's own color otherwise. Collision takes priority
    // over the door cue if somehow both were ever true at once.
    static void UpdateBorderCue()
    {
        bool colliding = Spider.IsCollidingWith(PlayerSprite.X, PlayerSprite.BitmapY, SpiderCollisionRadius);
        if (colliding)
            C64.Screen.SetBorderColor(Colors.Red);
        else if (AtDoor())
            C64.Screen.SetBorderColor(Colors.Green);
        else
            C64.Screen.SetBorderColor(roomColor_);
    }

    // Re-ranks the 3 moving creatures (player + 2 spiders) by depth (larger
    // bitmap-space Y = nearer the viewer, same convention RoomProjection.
    // ScreenY already uses) and writes each into the physical hardware
    // sprite slot (0/1/2) that rank calls for -- see SpriteStage's own
    // comment for why this has to happen fresh every frame rather than
    // once. Spider1's own floor line (BitmapY1=195) is always nearer than
    // Spider0's (BitmapY0=185), so only the player's depth is ever in
    // question -- that collapses the 6 possible orderings of 3 items down
    // to these 3 cases.
    static void UpdateZOrder()
    {
        uint playerKey = PlayerSprite.BitmapY;

        if (playerKey >= Spider.BitmapY1)
        {
            PlacePlayer(0);
            PlaceSpider1(1);
            PlaceSpider0(2);
        }
        else if (playerKey >= Spider.BitmapY0)
        {
            PlaceSpider1(0);
            PlacePlayer(1);
            PlaceSpider0(2);
        }
        else
        {
            PlaceSpider1(0);
            PlaceSpider0(1);
            PlacePlayer(2);
        }
    }

    static void PlacePlayer(uint slot)
    {
        SpriteStage.Place(slot, PlayerSprite.X, PlayerSprite.RawY, PlayerSprite.Pointer, Colors.Black, true, true, true);
    }

    static void PlaceSpider0(uint slot)
    {
        SpriteStage.Place(slot, Spider.X0, Spider.RawY0, Spider.Pointer, Spider.Color, false, false, false);
    }

    static void PlaceSpider1(uint slot)
    {
        SpriteStage.Place(slot, Spider.X1, Spider.RawY1, Spider.Pointer, Spider.Color, false, false, false);
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

        C64.Screen.ClearBitmap();
        DungeonView.Render(px_, py_);
        if (itemHere_)
            Item.Render();
        if (weaponHere_)
            Weapon.Render();
        Hud.Render(Item.CollectedCount, playerDamage_);
        Minimap.Render(px_, py_);

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

        // Checked every frame, not just when moved: the player can stop
        // right at a door (see the green border cue from UpdateBorderCue)
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
            itemHere_ = false;
            Hud.Render(Item.CollectedCount, playerDamage_);
        }
        else if (weaponHere_ && Touching(Weapon.RoomX, Weapon.RoomDepth, Weapon.RoomRadius))
        {
            playerDamage_ = Weapon.DamageAt(px_, py_);
            Weapon.CollectAt(px_, py_);
            weaponHere_ = false;
            Hud.Render(Item.CollectedCount, playerDamage_);
        }

        UpdateSpritePosition();
    }

    // Touch radius is given in SCREEN pixels (matching how big the marker
    // actually looks), so the comparison projects the player's room-space
    // position to screen-space first rather than comparing room-space
    // units against a pixel radius directly.
    static bool Touching(uint x, uint depth, ulong radius)
    {
        ulong px = RoomProjection.ScreenX(roomX_, roomDepth_);
        ulong py = RoomProjection.ScreenY(roomDepth_);
        ulong tx = RoomProjection.ScreenX(x, depth);
        ulong ty = RoomProjection.ScreenY(depth);
        ulong dx = px > tx ? px - tx : tx - px;
        ulong dy = py > ty ? py - ty : ty - py;
        return dx < radius && dy < radius;
    }

    // True while standing right at an open door's crossing edge -- the
    // same position checks TryCrossDoor itself uses, split out so
    // UpdateBorderCue can show the "press SPACE" cue without actually
    // crossing.
    static bool AtDoor()
    {
        if (roomDepth_ >= RoomProjection.DepthMax && northOpen_ && NearCenterX())
            return true;
        if (roomDepth_ == 0 && southOpen_ && NearCenterX())
            return true;
        if (roomX_ == 0 && westOpen_)
            return true;
        if (roomX_ >= RoomProjection.Width && eastOpen_)
            return true;
        return false;
    }

    // Crossing is now a deliberate action (SPACE), not automatic on
    // reaching the edge -- UpdateBorderCue's green flash is the signal
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

        if (roomDepth_ >= RoomProjection.DepthMax && northOpen_ && NearCenterX())
        {
            AnimateNorthDoorOpening();
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
            Step(px_ - 1, py_, RoomProjection.Width, 0);
            return true;
        }
        if (roomX_ >= RoomProjection.Width && eastOpen_)
        {
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

    // The north door panel's far (right) edge at each animation frame -- an
    // x (receding from the closed door's right edge, 187, toward the
    // hinge, 131) AND a top/bottom row pair (see DungeonView.
    // RenderNorthDoorOpening/DrawTaperedPanel), shrinking symmetrically in
    // from the door's own top (92) and bottom (173) toward its vertical
    // center (132.5) at the same rate the edge recedes -- the two right
    // corners trace inward AND toward the middle together, a real 3D
    // swing rather than a flat edge sliding sideways, collapsing together
    // at the hinge on the last frame (fully open). Row pairs sum to 265
    // (92+173) by construction, keeping them symmetric around the center.
    static readonly ulong[] NorthDoorOpenFarX = { 171, 155, 143, 135, 131 };
    static readonly ulong[] NorthDoorOpenFarTopRow = { 104, 115, 124, 130, 132 };
    static readonly ulong[] NorthDoorOpenFarBottomRow = { 161, 150, 141, 135, 133 };

    // Plays the swing before the player actually steps through -- still
    // viewing the CURRENT room, door hinged at its left edge, opening away.
    // Not double-buffered (see EnterRoom's own comment on why nothing here
    // is anymore): each frame is a plain ClearBitmap+redraw, the same
    // one-off flicker tradeoff already accepted for an ordinary room
    // transition, just spread over 5 frames instead of 1.
    static void AnimateNorthDoorOpening()
    {
        for (uint i = 0; i < 5; i++)
        {
            C64.Screen.WaitForVBlank();
            C64.Screen.ClearBitmap();
            DungeonView.RenderNorthDoorOpening(px_, py_, NorthDoorOpenFarX[i], NorthDoorOpenFarTopRow[i], NorthDoorOpenFarBottomRow[i]);
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
        // Single loop condition, no break/return inside the loop body --
        // a for(;;)/while with multiple early exits hits a real limitation
        // in this compiler (ILMethodBuildEvaluationStackPass's traversal
        // order over a loop with more than one exit edge).
        bool monsterDied = false;
        bool playerDied = false;
        C64.Sprites.Sprite3.Visible = false; // the small patrol sprite -- Render() below draws its own, unrelated full-screen face
        while (!monsterDied && !playerDied)
        {
            C64.Screen.ClearBitmap();
            DungeonView.Render(px_, py_);
            Monster.Render();
            Monster.DrawHealthBars(playerHp_, PlayerMaxHp, Monster.HpAt(px_, py_));

            while (!C64.IsKeyPressed(Keys.Space))
            {
                C64.Screen.WaitForVBlank();
                Spider.Animate();
                UpdateZOrder();
            }
            while (C64.IsKeyPressed(Keys.Space))
                Delay.Wait(500);

            Monster.DamageAt(px_, py_, playerDamage_);
            if (!Monster.IsAliveAt(px_, py_))
            {
                monsterDied = true;
            }
            else
            {
                playerHp_ = playerHp_ <= Monster.MonsterDamage ? 0 : playerHp_ - Monster.MonsterDamage;
                if (playerHp_ == 0)
                    playerDied = true;
            }
        }

        if (playerDied)
        {
            PlayerDied();
            return false;
        }
        EnterRoom(); // redraw without the now-dead monster
        return true;
    }

    // No separate game-over screen/state machine yet -- just enough that
    // dying doesn't strand the player: a flash to signal it, then reset to
    // full health back at the starting room. The monster that killed them
    // stays alive (DamageAt was never reached this exchange), so the same
    // fight is waiting on a second attempt.
    static void PlayerDied()
    {
        for (uint i = 0; i < 6; i++)
        {
            C64.Screen.SetBorderColor(Colors.Red);
            Delay.Wait(80);
            C64.Screen.SetBorderColor(Colors.Black);
            Delay.Wait(80);
        }
        playerHp_ = PlayerMaxHp;
        playerDamage_ = UnarmedDamage;
        px_ = 1;
        py_ = 1;
        roomX_ = RoomProjection.Center;
        roomDepth_ = 0;
        ApplyRoomColor();
        EnterRoom();
    }
}
