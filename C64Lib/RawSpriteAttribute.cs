using System;

namespace C64Lib
{
    // Embeds one sprite from a SpritePad XML project file (the format
    // SpritePad's older versions save as -- a flat <sprites><sprite>...
    // </sprite></sprites> document, one <sprite> per entry, each holding
    // <multi>/<spcolour>/<mcolour1>/<mcolour2> plus 21 <data> integers,
    // one per pixel row, each packing that row's 3 bytes into a single
    // 24-bit value (byte0 = bits 23-16, byte1 = bits 15-8, byte2 = bits
    // 7-0) -- e.g. 8388608 = $800000 = a single lit pixel in the top-left
    // corner. See ILRawAssemblyPass, which reads this the same way it
    // already reads RawAssemblyAttribute's plain .asm text resources, so
    // sprite data can be sourced from the original artist's own project
    // file instead of hand-transcribed .byte literals.
    //
    // Resource must be an embedded binary/text resource (not compiled
    // .asm); Index selects which <sprite> entry (0-based, document order)
    // to import; Label becomes that sprite's "spt_<Label>:" asm label,
    // same naming convention as every hand-authored sprite in
    // SpriteData.asm. Order is shared with RawAssemblyAttribute's own
    // Order (both are merged into one sequence before being written to
    // data.asm), so a sprite import can be positioned relative to
    // hand-written asm blocks if that ever matters -- it doesn't for
    // alignment (every sprite this attribute emits is padded to a full
    // 64-byte block on its own, so blocks can be freely interleaved with
    // any other already-64-byte-aligned content), only for readability of
    // the generated file.
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class RawSpriteAttribute : Attribute
    {
        public RawSpriteAttribute()
        {
        }
        public string Resource { get; set; }
        public int Index { get; set; }
        public string Label { get; set; }
        public int Order { get; set; } = 255;
    }
}
