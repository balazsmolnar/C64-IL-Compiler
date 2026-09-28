using C64Lib;
using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.RuntimeTest;

// Screen.SetDrawBuffer / SwapBuffers / SetBitmapColors end to end: C# call ->
// compiler flag detection -> the second-buffer layout in UnitTestEntry.asm
// (Graphics_Bitmap2/Graphics_ColorMatrix2 in VIC bank 1, code moved up to
// $6400) -> asm -> emulator. Lives in this project, not Test/, because that
// layout costs ~9 KB of address space and Test/'s program sits at its memory
// ceiling. SimpleEmulator has no VIC-II: what is checked is which memory the
// drawing lands in and the $D018/$DD00 values that select a buffer (the
// routines' own details are covered by SimpleEmulator.Test's
// GraphicsBufferTests; only VICE shows the picture actually flipping).
[TestFixture]
public class DoubleBufferTests
{
    [Test]
    public void Pixels_Land_In_The_Selected_Buffer_And_SwapBuffers_Selects_It_In_The_VIC()
    {
        ulong bitmap0 = C64Address.FromLabel("Graphics_Bitmap");
        ulong bitmap1 = C64Address.FromLabel("Graphics_Bitmap2");

        C64.Screen.SetScreenMode(ScreenMode.Bitmap);

        // Buffer 0 (the default draw target): (1,1) is byte 1, bit 6.
        C64.Screen.SetPixel(1, 1);
        // Buffer 1: (9,1) is byte 9 (cell x=8, scanline 1), bit 6.
        C64.Screen.SetDrawBuffer(1);
        C64.Screen.SetPixel(9, 1);

        Assert.AreEqual(C64.GetMemory(bitmap0, 1), 0x40u);
        Assert.AreEqual(C64.GetMemory(bitmap0, 9), 0u);
        Assert.AreEqual(C64.GetMemory(bitmap1, 9), 0x40u);
        Assert.AreEqual(C64.GetMemory(bitmap1, 1), 0u);

        // Buffer 1 was drawn into: show it (VIC bank 1, matrix $6000, bitmap $4000)...
        C64.Screen.SwapBuffers();
        Assert.AreEqual(C64.GetMemory(0xD018UL, 0), 0x80u);
        Assert.AreEqual(C64.GetMemory(0xDD00UL, 0) & 3u, 2u);

        // ... and drawing has moved to buffer 0.
        C64.Screen.SetPixel(17, 1);
        Assert.AreEqual(C64.GetMemory(bitmap0, 17), 0x40u);
        Assert.AreEqual(C64.GetMemory(bitmap1, 17), 0u);

        C64.Screen.SwapBuffers();
        Assert.AreEqual(C64.GetMemory(0xD018UL, 0), 0x38u);
        Assert.AreEqual(C64.GetMemory(0xDD00UL, 0) & 3u, 3u);
    }

    [Test]
    public void SetBitmapColors_Fills_Both_Color_Matrices()
    {
        ulong matrix0 = C64Address.FromLabel("Graphics_ColorMatrix");
        ulong matrix1 = C64Address.FromLabel("Graphics_ColorMatrix2");

        C64.Screen.SetBitmapColors(0x1E);

        Assert.AreEqual(C64.GetMemory(matrix0, 0), 0x1Eu);
        Assert.AreEqual(C64.GetMemory(matrix0 + 999UL, 0), 0x1Eu);
        Assert.AreEqual(C64.GetMemory(matrix1, 0), 0x1Eu);
        Assert.AreEqual(C64.GetMemory(matrix1 + 999UL, 0), 0x1Eu);
    }
}
