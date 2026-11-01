using C64Lib;

namespace C64Presentation.Helper;

// The C64 and C# logos as sprites (art/engine/make_logos.py, frm_Logo):
// sprites 0-3 the Commodore logo (C halves, flag halves), 4-6 the C# logo
// (C halves, '#').
class Sprites
{
    public static void SetupLogo(bool expand)
    {
        uint frame = (uint)C64Address.FromLabel("frm_Logo");
        Setup(C64.Sprites.Sprite0, frame, Colors.LightBlue, expand);
        Setup(C64.Sprites.Sprite1, frame + 1, Colors.LightBlue, expand);
        Setup(C64.Sprites.Sprite2, frame + 2, Colors.LightBlue, expand);
        Setup(C64.Sprites.Sprite3, frame + 3, Colors.LightRed, expand);
        Setup(C64.Sprites.Sprite4, frame, Theme.Accent, expand);
        Setup(C64.Sprites.Sprite5, frame + 1, Theme.Accent, expand);
        Setup(C64.Sprites.Sprite6, frame + 4, Theme.Accent, expand);
    }

    static void Setup(Sprite sprite, uint frame, Colors color, bool expand)
    {
        sprite.Frame = frame;
        sprite.Color = color;
        sprite.ExpandX = expand;
        sprite.ExpandY = expand;
    }

    public static void SetVisible(bool visible)
    {
        C64.Sprites.VisibleMask = visible ? 0x7Fu : 0u;
    }
}
