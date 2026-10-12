using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using C64Lib;

namespace Compiler;

// Writes data.asm: the concatenation of every RawAssemblyAttribute (plain
// hand-written .asm text, either inline or from an embedded resource),
// every RawSpriteAttribute (one sprite converted out of an embedded
// SpritePad XML project file), every RawBitmapAttribute (one stencil
// bitmap cut out of an embedded Advanced Art Studio .aas picture file,
// RLE-compressed) and every RawImageAttribute (a full-colour image
// compressed by tools/c64img.py) on the assembly, in a single Order-sorted
// sequence -- these attribute kinds ultimately just contribute text to the
// same output file, so they share one ordering rather than being
// independent, unordered-relative-to-each-other passes.
//
// Also writes sprites.asm: every RawSpritesAttribute's frames (made by
// tools/c64spr.py). Those do not go into data.asm, because where they must
// live depends on the program's memory layout, which only the entry file
// knows (Templates/ProgramEntry.asm): sprites.asm just defines the frame
// counts, each sheet's labels relative to SPRITE_STORE_A/B, and two macros
// holding the bytes, and the entry file expands them in the right place.
class ILRawAssemblyPass : ICompilerPass
{
    public void Execute(CompilerContext context)
    {
        context.GlobalOutputFile.WriteLine(".include \"./data.asm\"");

        var asmAttributes = context.Assembly.GetCustomAttributes(typeof(RawAssemblyAttribute), false)
            .OfType<RawAssemblyAttribute>().Select(a => (object)a);
        var spriteAttributes = context.Assembly.GetCustomAttributes(typeof(RawSpriteAttribute), false)
            .OfType<RawSpriteAttribute>().Select(a => (object)a);
        var bitmapAttributes = context.Assembly.GetCustomAttributes(typeof(RawBitmapAttribute), false)
            .OfType<RawBitmapAttribute>().Select(a => (object)a);
        var imageAttributes = context.Assembly.GetCustomAttributes(typeof(RawImageAttribute), false)
            .OfType<RawImageAttribute>().Select(a => (object)a);
        var entries = asmAttributes.Concat(spriteAttributes).Concat(bitmapAttributes).Concat(imageAttributes).OrderBy(GetOrder);

        // Parsed/loaded once per distinct Resource, not once per attribute
        // instance -- a single .spt/.aas file is typically the source for
        // several sprites/bitmaps (see Hunchback's data.cs), and
        // re-parsing the same file that many times would be wasted work.
        var spriteDocumentCache = new Dictionary<string, List<SptSprite>>();
        var bitmapCache = new Dictionary<string, byte[]>();

        using (var outputFile = File.CreateText(Path.Combine(context.OutputDirectory, $"data.asm")))
        {
            foreach (var entry in entries)
            {
                if (entry is RawAssemblyAttribute attr)
                {
                    if (!string.IsNullOrEmpty(attr.Asm))
                        outputFile.WriteLine(attr.Asm);
                    else if (!string.IsNullOrEmpty(attr.Resource))
                    {
                        using (Stream stream = context.Assembly.GetManifestResourceStream(attr.Resource))
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            string result = reader.ReadToEnd();
                            outputFile.WriteLine(result);
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("Asm or Resource must be specified.");
                    }
                }
                else if (entry is RawSpriteAttribute spriteAttr)
                {
                    if (string.IsNullOrEmpty(spriteAttr.Resource) || string.IsNullOrEmpty(spriteAttr.Label))
                        throw new InvalidOperationException("RawSpriteAttribute requires both Resource and Label.");

                    if (!spriteDocumentCache.TryGetValue(spriteAttr.Resource, out var sprites))
                    {
                        using (Stream stream = context.Assembly.GetManifestResourceStream(spriteAttr.Resource))
                        {
                            if (stream == null)
                                throw new InvalidOperationException($"Sprite resource '{spriteAttr.Resource}' not found.");
                            sprites = SptFile.Parse(stream);
                        }
                        spriteDocumentCache[spriteAttr.Resource] = sprites;
                    }

                    if (spriteAttr.Index < 0 || spriteAttr.Index >= sprites.Count)
                        throw new InvalidOperationException(
                            $"Sprite index {spriteAttr.Index} out of range for '{spriteAttr.Resource}' ({sprites.Count} sprites).");

                    WriteSprite(outputFile, sprites[spriteAttr.Index], spriteAttr.Label);
                    // In the high graphics layout the VIC looks at bank 3,
                    // where in-line data is invisible.
                    outputFile.WriteLine($".cerror GRAPHICS_HIGH, \"[RawSprite] '{spriteAttr.Label}' is stored in line, where the VIC cannot see it in a program that uses bitmap graphics. Use [RawSprites] (tools/c64spr.py) instead.\"");
                }
                else if (entry is RawBitmapAttribute bitmapAttr)
                {
                    WriteBitmapEntry(context, outputFile, bitmapAttr, bitmapCache);
                }
                else if (entry is RawImageAttribute imageAttr)
                {
                    WriteImageEntry(context, outputFile, imageAttr);
                }
            }
        }

        WriteSpriteStore(context);

        // Bitmap validation errors are collected (unlike RawAssembly/
        // RawSprite's own immediate-throw error paths above, a pre-existing
        // wart in this same method not touched here) so a single build
        // reports every bad bitmap at once, matching this compiler's
        // general diagnostics philosophy (see CLAUDE.md).
        context.Diagnostics.ThrowIfErrors();
    }

    private static void WriteBitmapEntry(CompilerContext context, StreamWriter outputFile, RawBitmapAttribute bitmapAttr, Dictionary<string, byte[]> bitmapCache)
    {
        if (string.IsNullOrEmpty(bitmapAttr.Resource) || string.IsNullOrEmpty(bitmapAttr.Label))
            throw new InvalidOperationException("RawBitmapAttribute requires both Resource and Label.");

        if (!bitmapCache.TryGetValue(bitmapAttr.Resource, out var bitmap))
        {
            using (Stream stream = context.Assembly.GetManifestResourceStream(bitmapAttr.Resource))
            {
                if (stream == null)
                {
                    context.Diagnostics.Add(new CompilerDiagnostic
                    {
                        Code = DiagnosticCodes.InvalidAsset,
                        Message = $"Bitmap resource '{bitmapAttr.Resource}' not found.",
                    });
                    return;
                }
                bitmap = AasFile.Load(stream, bitmapAttr.Resource, context.Diagnostics);
            }
            bitmapCache[bitmapAttr.Resource] = bitmap;
        }
        if (bitmap == null)
            return; // Load already reported a diagnostic.

        byte[] mask = AasFile.ExtractSubRectangle(bitmap, bitmapAttr.X, bitmapAttr.Y, bitmapAttr.Width, bitmapAttr.Height, bitmapAttr.Label, context.Diagnostics);
        if (mask == null)
            return; // ExtractSubRectangle already reported a diagnostic.

        byte[] compressed = PackBits.Encode(mask);
        WriteBitmap(outputFile, compressed, bitmapAttr.Width, bitmapAttr.Height / 8, bitmapAttr.Label);
    }

    private static void WriteBitmap(StreamWriter outputFile, byte[] compressed, int width, int heightCells, string label)
    {
        int rowJump = 320 - width;
        int totalBytes = width * heightCells;
        outputFile.WriteLine($"bmp_{label}:");
        outputFile.WriteLine($".word {width}, {rowJump}, {totalBytes}");
        WriteBytes(outputFile, compressed, 0, compressed.Length);
    }

    private static void WriteBytes(TextWriter outputFile, byte[] data, int start, int length)
    {
        for (int i = 0; i < length; i += 16)
        {
            int count = Math.Min(16, length - i);
            var row = new string[count];
            for (int j = 0; j < count; j++)
                row[j] = $"${data[start + i + j]:X2}";
            outputFile.WriteLine($".byte {string.Join(", ", row)}");
        }
    }

    private static void AddAssetError(CompilerContext context, string message)
    {
        context.Diagnostics.Add(new CompilerDiagnostic
        {
            Code = DiagnosticCodes.InvalidAsset,
            Message = message,
        });
    }

    private static bool IsValidLabel(string label)
    {
        if (string.IsNullOrEmpty(label) || char.IsDigit(label[0]))
            return false;
        return label.All(c => c == '_' || (c < 128 && char.IsLetterOrDigit(c)));
    }

    private static byte[] ReadResource(CompilerContext context, string kind, string label, string resource)
    {
        if (!IsValidLabel(label))
        {
            AddAssetError(context, $"{kind}: Label '{label}' must be letters, digits and '_' (it becomes an assembly label).");
            return null;
        }
        if (string.IsNullOrEmpty(resource))
        {
            AddAssetError(context, $"{kind} '{label}': Resource must be specified.");
            return null;
        }
        using Stream stream = context.Assembly.GetManifestResourceStream(resource);
        if (stream == null)
        {
            var known = context.Assembly.GetManifestResourceNames();
            AddAssetError(context, $"{kind} '{label}': resource '{resource}' not found. " +
                (known.Length == 0
                    ? "The assembly has no embedded resources (add <EmbeddedResource Include=\"...\" /> to the .csproj)."
                    : $"Embedded resources are: {string.Join(", ", known)}."));
            return null;
        }
        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        return memory.ToArray();
    }

    // A tools/c64img.py file: "C64I", version, mode, width and height in
    // cells, background colour, payload length (16 bit), payload. Only the
    // payload (a flags byte + one compressed stream per plane -- see
    // Screen_DrawImage in asm/C64Graphics.asm) goes into the program.
    private static void WriteImageEntry(CompilerContext context, StreamWriter outputFile, RawImageAttribute imageAttr)
    {
        const int HeaderLength = 11;
        byte[] data = ReadResource(context, "RawImage", imageAttr.Label, imageAttr.Resource);
        if (data == null)
            return;
        if (data.Length < HeaderLength + 2 || data[0] != 'C' || data[1] != '6' || data[2] != '4' || data[3] != 'I')
        {
            AddAssetError(context, $"RawImage '{imageAttr.Label}': resource '{imageAttr.Resource}' is not an image made by tools/c64img.py (run: python tools/c64img.py convert picture.png picture.img).");
            return;
        }
        if (data[4] != 1)
        {
            AddAssetError(context, $"RawImage '{imageAttr.Label}': resource '{imageAttr.Resource}' has image format version {data[4]}; this compiler reads version 1. Convert it again with this repository's tools/c64img.py.");
            return;
        }
        int width = data[6], height = data[7];
        int length = data[9] | (data[10] << 8);
        if (width < 1 || width > 40 || height < 1 || height > 25 || length != data.Length - HeaderLength || (data[HeaderLength] & 0xF8) != 0)
        {
            AddAssetError(context, $"RawImage '{imageAttr.Label}': resource '{imageAttr.Resource}' is damaged (size {width}x{height} cells, {data.Length} bytes). Convert it again with tools/c64img.py.");
            return;
        }
        outputFile.WriteLine($"; {width}x{height} cells, {(data[5] == 0 ? "multicolour" : "hi-res")}, {length} bytes");
        outputFile.WriteLine($"img_{imageAttr.Label}:");
        WriteBytes(outputFile, data, HeaderLength, length);
    }

    // Frames 0..63 go to store A, the rest to store B (see RawSpritesAttribute
    // and Templates/ProgramEntry.asm for where those are). A sheet is never
    // split across the two: its frames must stay consecutive.
    const int SpriteStoreAFrames = 64;
    const int SpriteStoreBFrames = 48;

    private static void WriteSpriteStore(CompilerContext context)
    {
        var sheets = context.Assembly.GetCustomAttributes(typeof(RawSpritesAttribute), false)
            .OfType<RawSpritesAttribute>().OrderBy(a => a.Order).ToList();

        var storeA = new List<byte>();
        var storeB = new List<byte>();
        var labels = new StringWriter();
        var seen = new HashSet<string>();
        foreach (var sheet in sheets)
        {
            byte[] data = ReadResource(context, "RawSprites", sheet.Label, sheet.Resource);
            if (data == null)
                continue;
            if (!seen.Add(sheet.Label))
            {
                AddAssetError(context, $"RawSprites: Label '{sheet.Label}' is used more than once.");
                continue;
            }
            if (data.Length == 0 || data.Length % 64 != 0)
            {
                AddAssetError(context, $"RawSprites '{sheet.Label}': resource '{sheet.Resource}' is {data.Length} bytes; sprite frames are 64 bytes each (run: python tools/c64spr.py convert sheet.png sheet.spr).");
                continue;
            }
            int frames = data.Length / 64;
            string store;
            int first;
            if (storeA.Count / 64 + frames <= SpriteStoreAFrames)
            {
                store = "SPRITE_STORE_A";
                first = storeA.Count / 64;
                storeA.AddRange(data);
            }
            else if (storeB.Count / 64 + frames <= SpriteStoreBFrames)
            {
                store = "SPRITE_STORE_B";
                first = storeB.Count / 64;
                storeB.AddRange(data);
            }
            else
            {
                AddAssetError(context, $"RawSprites '{sheet.Label}': no room for its {frames} frame(s). " +
                    $"There are {SpriteStoreAFrames} + {SpriteStoreBFrames} frame slots, and a sheet is never split between the two groups " +
                    $"({storeA.Count / 64} and {storeB.Count / 64} are taken so far).");
                continue;
            }
            labels.WriteLine($"; {frames} frame(s)");
            labels.WriteLine($"spt_{sheet.Label} = {store} + {first * 64}");
            labels.WriteLine($"frm_{sheet.Label} = (spt_{sheet.Label} >> 6) & $ff");
        }

        using var output = File.CreateText(Path.Combine(context.OutputDirectory, "sprites.asm"));
        output.WriteLine($"SPRITE_FRAMES_A = {storeA.Count / 64}");
        output.WriteLine($"SPRITE_FRAMES_B = {storeB.Count / 64}");
        output.WriteLine($"SPRITE_FRAME_COUNT = {(storeA.Count + storeB.Count) / 64}");
        output.WriteLine("sprite_store_data_a .macro");
        WriteBytes(output, storeA.ToArray(), 0, storeA.Count);
        output.WriteLine(".endm");
        output.WriteLine("sprite_store_data_b .macro");
        WriteBytes(output, storeB.ToArray(), 0, storeB.Count);
        output.WriteLine(".endm");
        output.Write(labels.ToString());
    }

    private static object GetOrder(object entry) => entry switch
    {
        RawAssemblyAttribute a => a.Order,
        RawSpriteAttribute s => s.Order,
        RawBitmapAttribute b => b.Order,
        RawImageAttribute i => i.Order,
        _ => throw new InvalidOperationException($"Unexpected raw-asset entry type {entry.GetType()}."),
    };

    private static void WriteSprite(StreamWriter outputFile, SptSprite sprite, string label)
    {
        outputFile.WriteLine($"; {(sprite.MultiColor ? "Multi" : "Single")} color mode, sprite colour: {sprite.SpriteColour}"
            + (sprite.MultiColor ? $", shared colours: {sprite.MultiColour1}/{sprite.MultiColour2}" : ""));
        outputFile.WriteLine($"spt_{label}:");
        foreach (var row in sprite.Rows)
            outputFile.WriteLine($".byte ${row[0]:X2}, ${row[1]:X2}, ${row[2]:X2}");
        outputFile.WriteLine(".byte $00");
    }
}

// One sprite decoded out of a SpritePad XML (.spt) project file -- see
// RawSpriteAttribute's own comment for the source format this reads.
class SptSprite
{
    public bool MultiColor;
    public int SpriteColour;
    public int MultiColour1;
    public int MultiColour2;
    public List<byte[]> Rows; // 21 rows, 3 bytes each
}

static class SptFile
{
    public static List<SptSprite> Parse(Stream stream)
    {
        var doc = XDocument.Load(stream);
        var result = new List<SptSprite>();
        foreach (var spriteEl in doc.Root.Elements("sprite"))
        {
            var rows = new List<byte[]>();
            foreach (var dataEl in spriteEl.Element("sdata").Elements("data"))
            {
                uint value = uint.Parse(dataEl.Value);
                rows.Add(new byte[]
                {
                    (byte)((value >> 16) & 0xFF),
                    (byte)((value >> 8) & 0xFF),
                    (byte)(value & 0xFF),
                });
            }
            if (rows.Count != 21)
                throw new InvalidOperationException($"Expected 21 sprite data rows, found {rows.Count}.");

            result.Add(new SptSprite
            {
                // bool.Parse, not XElement's own explicit bool operator
                // (XmlConvert.ToBoolean under the hood) -- this file's
                // <multi> values are "True"/"False" (C# ToString() casing),
                // not XML's own lowercase "true"/"false" boolean literals,
                // which XmlConvert.ToBoolean rejects.
                MultiColor = bool.Parse(spriteEl.Element("multi").Value),
                SpriteColour = (int)spriteEl.Element("spcolour"),
                MultiColour1 = (int)spriteEl.Element("mcolour1"),
                MultiColour2 = (int)spriteEl.Element("mcolour2"),
                Rows = rows,
            });
        }
        return result;
    }
}
