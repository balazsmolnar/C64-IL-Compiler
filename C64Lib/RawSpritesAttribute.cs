using System;

namespace C64Lib
{
    // Embeds a sheet of sprite frames made by tools/c64spr.py from a PNG
    // (64 bytes per frame) and puts them where the VIC can see them --
    // without the program choosing addresses:
    //
    //  - in a program that uses bitmap graphics, in the RAM underneath the
    //    I/O area ($D000-$DFFF, 64 frames, costing no program memory) and,
    //    past 64 frames, just below the video matrix (up to 48 more);
    //  - in a text-mode program, in line on a 64-byte boundary (the frames
    //    must then end below $4000).
    //
    // For a sheet labelled L:
    //     C64Address.FromLabel("spt_L")   address of its first frame
    //     C64Address.FromLabel("frm_L")   block number of its first frame
    // The frames of one sheet are consecutive, so frame n is block
    // frm_L + n:
    //
    //     uint first = (uint)C64Address.FromLabel("frm_L");
    //     C64.Sprites.Sprite0.Frame = first + n;
    //
    // Byte 63 of each frame holds the frame's own colour (low nibble; bit 7
    // = multicolour), as printed by the tool.
    //
    // RawSprite (one sprite out of a SpritePad file, stored in line wherever
    // data.asm happens to be) stays as it is, for text-mode programs that
    // place their data themselves.
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class RawSpritesAttribute : Attribute
    {
        public RawSpritesAttribute()
        {
        }
        public string Resource { get; set; }
        public string Label { get; set; }
        public int Order { get; set; } = 255;
    }
}
