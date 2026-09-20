using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using C64Lib;

namespace Compiler;

// Writes data.asm: the concatenation of every RawAssemblyAttribute (plain
// hand-written .asm text, either inline or from an embedded resource) and
// every RawSpriteAttribute (one sprite converted out of an embedded
// SpritePad XML project file) on the assembly, in a single Order-sorted
// sequence -- both attribute kinds ultimately just contribute text to the
// same output file, so they share one ordering rather than being two
// independent, unordered-relative-to-each-other passes.
class ILRawAssemblyPass : ICompilerPass
{
    public void Execute(CompilerContext context)
    {
        context.GlobalOutputFile.WriteLine(".include \"./data.asm\"");

        var asmAttributes = context.Assembly.GetCustomAttributes(typeof(RawAssemblyAttribute), false)
            .OfType<RawAssemblyAttribute>().Select(a => (object)a);
        var spriteAttributes = context.Assembly.GetCustomAttributes(typeof(RawSpriteAttribute), false)
            .OfType<RawSpriteAttribute>().Select(a => (object)a);
        var entries = asmAttributes.Concat(spriteAttributes).OrderBy(GetOrder);

        // Parsed once per distinct Resource, not once per RawSpriteAttribute
        // -- a single .spt file is typically the source for dozens of
        // sprites (see Hunchback's data.cs), and re-parsing the same XML
        // document that many times would be wasted work.
        var spriteDocumentCache = new Dictionary<string, List<SptSprite>>();

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
                }
            }
        }
    }

    private static object GetOrder(object entry) =>
        entry is RawAssemblyAttribute a ? a.Order : ((RawSpriteAttribute)entry).Order;

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
