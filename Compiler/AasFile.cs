using System;
using System.Collections.Generic;
using System.IO;

namespace Compiler;

// Reads a standard Advanced Art Studio hi-res picture file (".aas": a
// 2-byte load address followed by 8000 bytes of 320x200 bitmap data, in
// the exact cell-interleaved layout asm/C64Graphics.asm's own
// Graphics_ComputePixelPointer already uses -- see RawBitmapAttribute's
// own comment) and extracts a cell-aligned sub-rectangle as a flat mask
// byte array, in the SAME row-major-per-cell-row-band order the 6502
// decompressor (Bitmap_Blit_Core) consumes: consecutive cells within one
// row-band are already contiguous in the source file (byte offset =
// (y/8)*320 + (x&~7) + (y&7), same formula the asm uses), so extraction is
// a straight per-band Array.Copy -- no reordering or pixel-format work
// anywhere in this class.
static class AasFile
{
    const int CanvasWidth = 320;
    const int CanvasHeight = 200;
    const int BitmapBytes = CanvasWidth * CanvasHeight / 8; // 8000
    const int FileLength = 2 + BitmapBytes;                 // 2-byte load address + bitmap

    public static byte[] Load(Stream stream, string resourceName, CompilerDiagnostics diagnostics)
    {
        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        byte[] bytes = memory.ToArray();
        if (bytes.Length != FileLength)
        {
            diagnostics.Add(new CompilerDiagnostic
            {
                Code = DiagnosticCodes.InvalidAsset,
                Message = $"Bitmap resource '{resourceName}' is {bytes.Length} bytes; expected {FileLength} " +
                    "(a 2-byte load address plus an 8000-byte 320x200 hi-res bitmap -- an Advanced Art Studio .aas file).",
            });
            return null;
        }
        byte[] bitmap = new byte[BitmapBytes];
        Array.Copy(bytes, 2, bitmap, 0, BitmapBytes);
        return bitmap;
    }

    public static byte[] ExtractSubRectangle(byte[] bitmap, int x, int y, int width, int height, string label, CompilerDiagnostics diagnostics)
    {
        if (x < 0 || y < 0 || width <= 0 || height <= 0 ||
            x % 8 != 0 || y % 8 != 0 || width % 8 != 0 || height % 8 != 0)
        {
            diagnostics.Add(new CompilerDiagnostic
            {
                Code = DiagnosticCodes.InvalidAsset,
                Message = $"Bitmap '{label}': X/Y/Width/Height must all be positive multiples of 8 " +
                    $"(got X={x}, Y={y}, Width={width}, Height={height}).",
            });
            return null;
        }
        if (x + width > CanvasWidth || y + height > CanvasHeight)
        {
            diagnostics.Add(new CompilerDiagnostic
            {
                Code = DiagnosticCodes.InvalidAsset,
                Message = $"Bitmap '{label}': sub-rectangle ({x},{y})-({x + width},{y + height}) falls outside " +
                    $"the source's {CanvasWidth}x{CanvasHeight} canvas.",
            });
            return null;
        }

        int heightCells = height / 8;
        byte[] result = new byte[width * heightCells];
        for (int band = 0; band < heightCells; band++)
        {
            int cellRow = y / 8 + band;
            int sourceOffset = cellRow * CanvasWidth + x;
            Array.Copy(bitmap, sourceOffset, result, band * width, width);
        }
        return result;
    }
}

// PackBits-style run-length encoding. Chosen specifically because the
// DECODER it needs (asm/C64Graphics.asm's Bitmap_Blit_Core) is tiny:
// one branch on the control byte's sign bit distinguishes a literal run
// from a repeat run, no lookup table, no nested state. Control byte:
// bit7=1 (repeat) -> low 7 bits = count-1, (count) copies of the ONE data
// byte that follows; bit7=0 (literal) -> low 7 bits = count-1, (count) raw
// data bytes follow, copied verbatim. A greedy encoder (longest run at
// each position) -- correctness matters here, not optimal compression.
static class PackBits
{
    const int MaxRun = 128; // 7-bit count field, 1-128 (encoded as count-1)

    public static byte[] Encode(byte[] data)
    {
        var output = new List<byte>();
        int n = data.Length;
        int i = 0;
        while (i < n)
        {
            int runLength = RunLengthAt(data, i, n);
            if (runLength >= 2)
            {
                output.Add((byte)(0x80 | (runLength - 1)));
                output.Add(data[i]);
                i += runLength;
                continue;
            }

            // Literal run: extend while the NEXT position doesn't itself
            // start a run of 2+ (at which point a repeat-run is cheaper),
            // capped at MaxRun bytes.
            int literalStart = i;
            i++;
            while (i < n && (i - literalStart) < MaxRun && RunLengthAt(data, i, n) < 2)
                i++;
            int literalLength = i - literalStart;
            output.Add((byte)(literalLength - 1));
            for (int j = 0; j < literalLength; j++)
                output.Add(data[literalStart + j]);
        }
        return output.ToArray();
    }

    static int RunLengthAt(byte[] data, int i, int n)
    {
        int length = 1;
        while (i + length < n && data[i + length] == data[i] && length < MaxRun)
            length++;
        return length;
    }
}
