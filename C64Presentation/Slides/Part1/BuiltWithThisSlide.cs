using C64Lib;
using C64Presentation.Helper;
using C64Presentation.SlideElements;

// The room's torch flames (2x2 cells, the same at both torches) and the
// characters (under I/O): art/slides/make_room.py, from Catacombs' art.
[assembly: RawImage(Resource = "room_flame0.img", Label = "RoomFlame0")]
[assembly: RawImage(Resource = "room_flame1.img", Label = "RoomFlame1")]
[assembly: RawImage(Resource = "room_flame2.img", Label = "RoomFlame2")]
[assembly: RawImage(Resource = "room_flame3.img", Label = "RoomFlame3")]
[assembly: RawSprites(Resource = "cast.spr", Label = "Cast")]

namespace C64Presentation.Slides;

// Slide 8: the Catacombs room the skeleton guards, alive above a raster
// split: the torches flicker, the adventurer walks across, the guardian
// paces in the doorway, a bat flits along the far wall. The lower third in
// the hi-res band: the title with an arrow up at the picture, the facts
// beside it, one per press. Row 21, the first under the split, stays blank.
sealed class BuiltWithThisSlide : Slide
{
    const uint Split = 21;
    const uint Top = Split + 1, Middle = Split + 2, Bottom = Split + 3;   // the band's rows
    const uint Facts = 18;                          // the facts' column, right of the title

    // The Cast sheet (make_room.py): the adventurer's top and four leg
    // frames, the guardian's top and two, the bat's four wing beats.
    const uint Hero = 0, HeroLegs = 1, Guard = 5, GuardLegs = 6, Bat = 8;
    const uint HeroPoses = 4, GuardPoses = 2, BatPoses = 4;
    // Hardware sprites, nearest first (a lower number is drawn in front).
    const uint HeroSprite = 0, GuardSprite = 2, BatSprite = 4;
    // Register y of each top above the feet, as in the game (SpriteStage parts).
    const uint HeroLift = 15, GuardLift = 12, BatLift = 42;
    const uint Walk = 1, Patrol = 3;                // paces: a pixel every 2 / 4 frames
    const uint HeroFloor = 156;                     // the front of the floor
    const ulong Offstage = 344;                     // past the right border: walk in again from the left
    // The guardian paces the doorway, x 160, nearer only to y 140, so the
    // adventurer (y 156) always passes in front of him.
    const ulong Doorway = 160, PatrolFar = 128, PatrolNear = 140;
    // The bat stays on the torch-lit band between the torches: black on
    // orange, never black on the dark blue stones.
    const ulong BatLeft = 96, BatRight = 224;
    const uint BatHeight = 64;
    const uint FlameRow = 7, LeftTorch = 13, RightTorch = 25;   // Catacombs' FLAME_CELLS
    const ulong FlameBytes = 54;    // the four flames are declared in a row, all this size (make_room.py checks)
    const uint Sprites = 0x1F;

    public static Slide Create() => new BuiltWithThisSlide();

    public override void Present()
    {
        SlideElement[] band =
        [
            new Arrow { FromCol = Theme.Left, FromRow = Bottom, ToCol = Theme.Left, ToRow = Top },   // up at the picture
            new TextElement { X = Theme.Left + 2, Y = Middle, Text = "Built with this", Color = Theme.Accent },
        ];
        SlideElement[] facts =
        [
            new TextElement { X = Facts, Y = Top, Text = "Catacombs: 3,477 lines of C#" },
            new TextElement { X = Facts, Y = Bottom, Text = "61 rooms, 10,478 bytes still free" },
        ];
        Deck.Picture(ArtDisk.Load(DiskPicture.Room), 0, 0, Split);
        foreach (var element in band)
            element.Draw();
        uint shown = 0;
#if SCENE_HOLD
        foreach (var fact in facts)         // a held scene shows every build at once
            fact.Draw();
        shown = (uint)facts.Length;
#endif
        Deck.Reveal();
        Live(facts, shown);
    }

    // The room comes alive until the last press. (A static method: its
    // locals get a fixed frame, far cheaper than a virtual one's.)
    static void Live(SlideElement[] facts, uint shown)
    {
        Actor[] cast =
        [
            // The adventurer walks across the front, in from the left again.
            new() { Sprite = HeroSprite, Color = Colors.Red, LegsColor = Colors.Grey2, Top = Hero, Legs = HeroLegs, Poses = HeroPoses, Lift = HeroLift, Pace = Walk, Wraps = true, From = 0, To = Offstage, Y = HeroFloor },
            // The guardian paces towards us and back, slowly.
            new() { Sprite = GuardSprite, Color = Colors.Grey3, LegsColor = Colors.Grey3, Top = Guard, Legs = GuardLegs, Poses = GuardPoses, Lift = GuardLift, Pace = Patrol, Vertical = true, From = PatrolFar, To = PatrolNear, X = Doorway },
            // The bat flits along the far wall.
            new() { Sprite = BatSprite, Color = Colors.LightRed, Top = Bat, Poses = BatPoses, Lift = BatLift, From = BatLeft, To = BatRight, Y = BatHeight },
        ];
        foreach (var actor in cast)
            actor.Start();
        C64.Sprites.CommonColor1 = Colors.Black;
        C64.Sprites.CommonColor2 = Colors.Brown;
        C64.Sprites.MultiColorMask = Sprites;
        ulong flames = C64Address.FromLabel("img_RoomFlame0");

        bool held = false;
        for (uint tick = 0; !KeyBoard.Jumping; tick++)
        {
            Deck.Wait(1);
            foreach (var actor in cast)
                actor.Step(tick);
            C64.Sprites.VisibleMask = Sprites;

            // The torches: one of them takes a random flame every fourth frame.
            if ((tick & 3) == 0)
                C64.Screen.DrawImage((tick & 4) == 0 ? LeftTorch : RightTorch, FlameRow, flames + FlameBytes * (C64.Random() & 3));

            // SPACE / fire (only through KeyBoard, so scene builds and N / B
            // work here too): the next fact fades in; after the last, the
            // next slide. N / B end the loop through KeyBoard.Jumping.
            bool pressed = KeyBoard.AdvancePressed();
            if (pressed && !held && !KeyBoard.Jumping)
            {
                if (shown == facts.Length)
                    break;
                Deck.Build(false);
                facts[shown++].Draw();
                Deck.Show();
            }
            held = pressed;
        }
        C64.Sprites.VisibleMask = 0;        // gone before the slide fades out
    }

    public override void CleanUp()
    {
        C64.Sprites.VisibleMask = 0;
        C64.Sprites.MultiColorMask = 0;
    }
}
