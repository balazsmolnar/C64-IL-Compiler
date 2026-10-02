using System;

namespace C64Lib
{
    // Embeds one static stencil bitmap, cut out of a standard C64 hi-res
    // picture file (Advanced Art Studio's ".aas" format -- a 2-byte load
    // address followed by 8000 bytes of bitmap data, 320x200, in the exact
    // same cell-interleaved byte layout asm/C64Graphics.asm's own bitmap
    // memory already uses -- both are dictated by VIC-II hardware, not tool
    // convention, so no pixel-format decoding happens anywhere in this
    // pipeline, just a byte-range copy). See ILRawAssemblyPass, which reads
    // this the same way it already reads RawAssemblyAttribute's plain .asm
    // text resources and RawSpriteAttribute's SpritePad project files.
    //
    // The extracted bitmap is a 1 bit/pixel STENCIL only -- no color data
    // at all. A set bit means "opaque, paint this pixel in whatever
    // BitmapColorSource the caller passes to Screen.DrawBitmap"; a clear
    // bit means "transparent, leave the bitmap's existing content at that
    // position untouched" (unlike DrawRectangle/DrawLine's `on: false`,
    // which clears to Background -- this never overwrites anything outside
    // its own "on" pixels). X/Y/Width/Height are all in pixels, each
    // required to be a multiple of 8 (the sub-rectangle must land on whole
    // 8x8 cell boundaries, inside the source file's 320x200 canvas) --
    // validated by the compiler, not at runtime.
    //
    // Resource must be an embedded binary resource (not compiled .asm);
    // Label becomes "bmp_<Label>:" in data.asm, the address
    // Screen.DrawBitmap expects via C64Address.FromLabel("bmp_<Label>"),
    // same naming convention RawSpriteAttribute's "spt_<Label>:" already
    // uses. Order is shared with RawAssemblyAttribute/RawSpriteAttribute's
    // own Order (all three are merged into one sequence before being
    // written to data.asm), only for readability of the generated file.
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class RawBitmapAttribute : Attribute
    {
        public RawBitmapAttribute()
        {
        }
        public string Resource { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Label { get; set; }
        public int Order { get; set; } = 255;
    }
}
