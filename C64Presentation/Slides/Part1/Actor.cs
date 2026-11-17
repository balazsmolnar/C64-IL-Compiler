using C64Lib;

namespace C64Presentation.Slides;

// A Catacombs character as hardware sprites: a multicolour top and, for
// those that walk, legs one sprite (21 px) under it, placed by the point
// between its feet in bitmap pixels as in the game (Catacombs/SpriteStage).
// It goes back and forth along its beat, From to To on x (or on y if
// Vertical; or, if it Wraps, round again from From), a pixel every Pace + 1
// frames, cycling its frames (numbers in the "Cast" sheet,
// art/slides/make_room.py) as it goes. X or Y is where it stands across
// the beat.
sealed class Actor
{
    const ulong SpriteLeft = 12;      // register x = feet x + 12: 24 px wide, centred
    const uint LegsBelow = 21;
    const int PoseShift = 3;          // the cycle moves on every 8 video frames

    public required uint Sprite { get; init; }      // the top's hardware sprite; the legs' is the next
    public required Colors Color { get; init; }
    public Colors LegsColor { get; init; }          // Black: no legs
    public required uint Top { get; init; }         // the top's frame; or the first of its cycle, if no legs
    public uint Legs { get; init; }                 // the first frame of the legs' cycle
    public required uint Poses { get; init; }       // frames in the cycle: 2 or 4
    public uint Lift { get; init; }                 // register y of the top = feet y + Lift
    public uint Pace { get; init; }                 // 0, 1, 3: a pixel every 1, 2, 4 frames
    public bool Vertical { get; init; }             // its beat runs along y (towards us), not x
    public bool Wraps { get; init; }
    public required ulong From { get; init; }
    public required ulong To { get; init; }
    public ulong X { get; init; }
    public uint Y { get; init; }

    ulong _at;
    bool _back;

    public void Start() => _at = From;

    public void Step(uint tick)
    {
        if ((tick & Pace) == 0)
        {
            _at = _back ? _at - 1 : _at + 1;
            if (_at == To)
            {
                _back = !Wraps;
                if (Wraps)
                    _at = From;
            }
            else if (_at == From)
                _back = false;
        }

        uint pose = (tick >> PoseShift) & (Poses - 1);
        uint cast = (uint)C64Address.FromLabel("frm_Cast");
        ulong x = (Vertical ? X : _at) + SpriteLeft;
        uint y = (Vertical ? (uint)_at : Y) + Lift;
        if (LegsColor == Colors.Black)
        {
            C64.Sprites[Sprite].Place(x, y, cast + Top + pose, Color);
            return;
        }
        C64.Sprites[Sprite].Place(x, y + (pose & 1), cast + Top, Color);    // the body dips as a foot lands
        C64.Sprites[Sprite + 1].Place(x, y + LegsBelow, cast + Legs + pose, LegsColor);
    }
}
