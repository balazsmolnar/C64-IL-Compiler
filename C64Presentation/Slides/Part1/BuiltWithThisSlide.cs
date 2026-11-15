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
    const ulong Offstage = 344;                     // past the right border: walk in again from the left
    const uint PatrolFar = 128, PatrolNear = 152;   // the guardian's beat, floor y (as in the game)
    const ulong BatLeft = 64, BatRight = 256;
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
            new() { Sprite = 0, Color = Colors.Red, LegsColor = Colors.Grey2, Top = Hero, Legs = HeroLegs, Poses = 4, Lift = 15, Pace = 1, Wraps = true, From = 0, To = Offstage, Y = 156 },
            // The guardian paces towards us and back, slowly.
            new() { Sprite = 2, Color = Colors.Grey3, LegsColor = Colors.Grey3, Top = Guard, Legs = GuardLegs, Poses = 2, Lift = 12, Pace = 3, Vertical = true, From = PatrolFar, To = PatrolNear, X = 160 },
            // The bat flits along the far wall.
            new() { Sprite = 4, Color = Colors.LightRed, Top = Bat, Poses = 4, Lift = 42, From = BatLeft, To = BatRight, Y = 64 },
        ];
        C64.Sprites.CommonColor1 = Colors.Black;
        C64.Sprites.CommonColor2 = Colors.Brown;
        C64.Sprites.MultiColorMask = Sprites;
        ulong flames = C64Address.FromLabel("img_RoomFlame0");

        bool held = false;
        for (uint tick = 0; ; tick++)
        {
            C64.Screen.WaitForVBlank();
            foreach (var actor in cast)
                actor.Step(tick);
            C64.Sprites.VisibleMask = Sprites;

            // The torches: one of them takes a random flame every fourth frame.
            if ((tick & 3) == 0)
                C64.Screen.DrawImage((tick & 4) == 0 ? LeftTorch : RightTorch, FlameRow, flames + FlameBytes * (C64.Random() & 3));

            // SPACE / fire: the next fact fades in; after the last, the next slide.
            bool pressed = KeyBoard.AdvancePressed();
            if (pressed && !held)
            {
                if (shown == facts.Length)
                    return;
                Deck.Build(false);
                facts[shown++].Draw();
                Deck.Show();
            }
            held = pressed;
        }
    }
    public override void CleanUp()
    {
        C64.Sprites.VisibleMask = 0;
        C64.Sprites.MultiColorMask = 0;
    }
}
