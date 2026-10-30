using System;

namespace C64Lib
{
    // Embeds one full-colour picture (or part of one) made by
    // tools/c64img.py from an ordinary PNG: a rectangle of 8x8 cells, each
    // with its bitmap bytes and its own colours (video matrix byte + colour
    // RAM byte), compressed. Draw it with
    //
    //     C64.Screen.DrawImage(column, row, C64Address.FromLabel("img_<Label>"));
    //
    // Unlike RawBitmap (a one-colour stencil) an image brings its colours
    // with it, in multicolour (160x200, three colours per cell plus the
    // screen background) or hi-res (320x200, two colours per cell) -- the
    // screen must be in the matching ScreenMode. Cells the tool left out
    // (transparent in the PNG, or equal to its --base picture) are not
    // touched, so an image can be an overlay of any shape. An image made
    // with only some planes (--planes mc: colours without pixels) recolours
    // what is already on screen.
    //
    // Resource is an embedded binary resource (the tool's .img output);
    // Label becomes "img_<Label>:" in data.asm. The stored size is what the
    // tool reports: typically a quarter to a half of the raw 10 bytes per
    // cell. Order is shared with the other Raw* attributes.
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class RawImageAttribute : Attribute
    {
        public RawImageAttribute()
        {
        }
        public string Resource { get; set; }
        public string Label { get; set; }
        public int Order { get; set; } = 255;
    }
}
