using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace Compiler.UnitTests;

// [assembly: RawImage] and [assembly: RawSprites]: what the compiler writes
// for good assets (data.asm / sprites.asm) and what it says about bad ones.
// Each test compiles a small C# program with Roslyn, with the assets as
// embedded resources, and runs the real compiler driver on it. Host-side
// only: whether the output assembles and draws is covered by
// SimpleEmulator.Test's GraphicsImageTests and by running on VICE.
[TestFixture]
public class RawAssetTests
{
    static int counter;

    // A minimal, valid tools/c64img.py file: 1x1 cell, bitmap plane only,
    // whose stream is "one literal byte $AA, end".
    static byte[] Image(byte version = 1) => new byte[]
    {
        (byte)'C', (byte)'6', (byte)'4', (byte)'I', version, 0, 1, 1, 0, 4, 0,
        0x01, 0x01, 0xAA, 0x00,
    };

    static byte[] Frames(int count, byte fill = 0x11) => Enumerable.Repeat(fill, count * 64).ToArray();

    static (int exitCode, string output, string dir) Compile(string attributes, params (string name, byte[] data)[] resources)
    {
        var dir = Path.Combine(Path.GetTempPath(), "c64_asset_tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var name = "AssetSnippet" + ++counter;
        var dll = Path.Combine(dir, name + ".dll");
        var pdb = Path.Combine(dir, name + ".pdb");

        var source = $@"using C64Lib;
{attributes}
class Program
{{
    static void Main()
    {{
        C64.FillMemory(0x0900UL, 1, 2);
    }}
}}";
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))
            .Split(Path.PathSeparator)
            .Select(p => MetadataReference.CreateFromFile(p))
            .Append(MetadataReference.CreateFromFile(Path.Combine(AppContext.BaseDirectory, "C64Lib.dll")))
            .ToList();
        var tree = CSharpSyntaxTree.ParseText(source, path: Path.Combine(dir, "snippet.cs"), encoding: System.Text.Encoding.UTF8);
        var compilation = CSharpCompilation.Create(name, new[] { tree }, references,
            new CSharpCompilationOptions(OutputKind.ConsoleApplication, optimizationLevel: OptimizationLevel.Release));
        var manifest = resources.Select(r => new ResourceDescription(r.name, () => new MemoryStream(r.data), isPublic: true)).ToList();

        using (var dllStream = File.Create(dll))
        using (var pdbStream = File.Create(pdb))
        {
            var emit = compilation.Emit(dllStream, pdbStream, manifestResources: manifest,
                options: new Microsoft.CodeAnalysis.Emit.EmitOptions(debugInformationFormat: Microsoft.CodeAnalysis.Emit.DebugInformationFormat.PortablePdb));
            Assert.That(emit.Success, Is.True, "the test's own C# does not compile:\n" + string.Join("\n", emit.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
        }

        var outDir = Path.Combine(dir, "asm");
        Directory.CreateDirectory(outDir);
        var errors = new StringWriter();
        var exit = Program.Run(new[] { dll, outDir, Path.Combine(dir, "entry.asm"), "program" }, errors);
        return (exit, errors.ToString(), dir);
    }

    static string[] Lines(string dir, string file) => File.ReadAllLines(Path.Combine(dir, "asm", file));

    static string Rejected(string attributes, params (string name, byte[] data)[] resources)
    {
        var (exit, output, _) = Compile(attributes, resources);
        Assert.That(exit, Is.EqualTo(1), "expected the compiler to reject this program; output:\n" + output);
        Assert.That(output, Does.Contain("error C64005:"), output);
        return output;
    }

    // --- RawImage -------------------------------------------------------

    [Test]
    public void Image_Payload_Goes_Into_Data_Asm_Without_Its_File_Header()
    {
        var (exit, output, dir) = Compile(@"[assembly: RawImage(Resource = ""pic.img"", Label = ""Pic"")]", ("pic.img", Image()));
        Assert.That(exit, Is.EqualTo(0), output);
        var data = Lines(dir, "data.asm");
        int label = Array.IndexOf(data, "img_Pic:");
        Assert.That(label, Is.GreaterThan(0), string.Join("\n", data));
        Assert.That(data[label + 1], Is.EqualTo(".byte $01, $01, $AA, $00"));
        Assert.That(data[label - 1], Does.Contain("1x1 cells").And.Contain("4 bytes"));
    }

    [Test]
    public void Missing_Image_Resource_Lists_The_Resources_That_Exist()
    {
        var output = Rejected(@"[assembly: RawImage(Resource = ""nope.img"", Label = ""Pic"")]", ("pic.img", Image()));
        Assert.That(output, Does.Contain("resource 'nope.img' not found").And.Contain("pic.img"));
    }

    [Test]
    public void Missing_Image_Resource_In_An_Assembly_Without_Resources_Points_At_The_Csproj()
    {
        var output = Rejected(@"[assembly: RawImage(Resource = ""nope.img"", Label = ""Pic"")]");
        Assert.That(output, Does.Contain("EmbeddedResource"));
    }

    [Test]
    public void A_File_That_Is_Not_An_Image_Names_The_Tool()
    {
        var output = Rejected(@"[assembly: RawImage(Resource = ""pic.png"", Label = ""Pic"")]", ("pic.png", new byte[200]));
        Assert.That(output, Does.Contain("not an image made by tools/c64img.py"));
    }

    [Test]
    public void An_Image_Of_Another_Format_Version_Is_Rejected()
    {
        var output = Rejected(@"[assembly: RawImage(Resource = ""pic.img"", Label = ""Pic"")]", ("pic.img", Image(version: 2)));
        Assert.That(output, Does.Contain("format version 2"));
    }

    [Test]
    public void A_Truncated_Image_Is_Rejected()
    {
        var output = Rejected(@"[assembly: RawImage(Resource = ""pic.img"", Label = ""Pic"")]", ("pic.img", Image().Take(13).ToArray()));
        Assert.That(output, Does.Contain("damaged"));
    }

    [Test]
    public void A_Label_That_Is_Not_An_Identifier_Is_Rejected()
    {
        var output = Rejected(@"[assembly: RawImage(Resource = ""pic.img"", Label = ""my pic"")]", ("pic.img", Image()));
        Assert.That(output, Does.Contain("Label 'my pic'"));
    }

    [Test]
    public void Every_Bad_Asset_Is_Reported_In_One_Build()
    {
        var (exit, output, _) = Compile(@"[assembly: RawImage(Resource = ""a.img"", Label = ""A"")]
[assembly: RawImage(Resource = ""b.img"", Label = ""B"")]
[assembly: RawSprites(Resource = ""c.spr"", Label = ""C"")]", ("b.img", new byte[20]), ("c.spr", new byte[65]));
        Assert.That(exit, Is.EqualTo(1));
        Assert.That(output.Split('\n').Count(l => l.Contains("error C64005:")), Is.EqualTo(3), output);
    }

    // --- RawSprites -----------------------------------------------------

    [Test]
    public void A_Program_Without_Sprite_Sheets_Still_Gets_An_Empty_Sprites_Asm()
    {
        // The entry file includes it unconditionally.
        var (exit, output, dir) = Compile("");
        Assert.That(exit, Is.EqualTo(0), output);
        var sprites = Lines(dir, "sprites.asm");
        Assert.That(sprites, Does.Contain("SPRITE_FRAME_COUNT = 0"));
        Assert.That(sprites, Does.Contain("sprite_store_data_a .macro"));
        Assert.That(sprites.Any(l => l.StartsWith(".byte")), Is.False);
        Assert.That(File.ReadAllText(Path.Combine(dir, "entry.asm")), Does.Contain("/sprites.asm\""));
    }

    [Test]
    public void Sheets_Get_Consecutive_Frames_And_Address_And_Block_Labels()
    {
        var (exit, output, dir) = Compile(@"[assembly: RawSprites(Order = 2, Resource = ""b.spr"", Label = ""Second"")]
[assembly: RawSprites(Order = 1, Resource = ""a.spr"", Label = ""First"")]",
            ("a.spr", Frames(3, 0x11)), ("b.spr", Frames(2, 0x22)));
        Assert.That(exit, Is.EqualTo(0), output);
        var sprites = Lines(dir, "sprites.asm");
        Assert.That(sprites, Does.Contain("SPRITE_FRAMES_A = 5"));
        Assert.That(sprites, Does.Contain("SPRITE_FRAMES_B = 0"));
        Assert.That(sprites, Does.Contain("spt_First = SPRITE_STORE_A + 0"));      // Order decides, not source order
        Assert.That(sprites, Does.Contain("spt_Second = SPRITE_STORE_A + 192"));
        Assert.That(sprites, Does.Contain("frm_Second = (spt_Second >> 6) & $ff"));
        // 5 frames = 320 bytes = 20 rows of 16, all inside the first macro.
        int start = Array.IndexOf(sprites, "sprite_store_data_a .macro");
        int end = Array.IndexOf(sprites, ".endm");
        Assert.That(end - start - 1, Is.EqualTo(20));
        Assert.That(sprites[start + 1], Does.StartWith(".byte $11"));
        Assert.That(sprites[end - 1], Does.StartWith(".byte $22"));
    }

    [Test]
    public void A_Sheet_That_Does_Not_Fit_In_The_First_64_Frames_Goes_To_The_Second_Group_Whole()
    {
        var (exit, output, dir) = Compile(@"[assembly: RawSprites(Order = 1, Resource = ""a.spr"", Label = ""Big"")]
[assembly: RawSprites(Order = 2, Resource = ""b.spr"", Label = ""Spill"")]
[assembly: RawSprites(Order = 3, Resource = ""c.spr"", Label = ""Small"")]",
            ("a.spr", Frames(60)), ("b.spr", Frames(10)), ("c.spr", Frames(4)));
        Assert.That(exit, Is.EqualTo(0), output);
        var sprites = Lines(dir, "sprites.asm");
        Assert.That(sprites, Does.Contain("SPRITE_FRAMES_A = 64"));
        Assert.That(sprites, Does.Contain("SPRITE_FRAMES_B = 10"));
        Assert.That(sprites, Does.Contain("spt_Spill = SPRITE_STORE_B + 0"));
        Assert.That(sprites, Does.Contain("spt_Small = SPRITE_STORE_A + 3840"));
    }

    [Test]
    public void Too_Many_Frames_Are_Rejected()
    {
        var output = Rejected(@"[assembly: RawSprites(Order = 1, Resource = ""a.spr"", Label = ""A"")]
[assembly: RawSprites(Order = 2, Resource = ""a.spr"", Label = ""B"")]
[assembly: RawSprites(Order = 3, Resource = ""a.spr"", Label = ""C"")]", ("a.spr", Frames(50)));
        Assert.That(output, Does.Contain("RawSprites 'C': no room for its 50 frame(s)"));
    }

    [Test]
    public void A_Sprite_File_Whose_Size_Is_Not_A_Multiple_Of_64_Names_The_Tool()
    {
        var output = Rejected(@"[assembly: RawSprites(Resource = ""a.spr"", Label = ""A"")]", ("a.spr", new byte[63]));
        Assert.That(output, Does.Contain("63 bytes").And.Contain("tools/c64spr.py"));
    }

    [Test]
    public void The_Same_Sprite_Label_Twice_Is_Rejected()
    {
        var output = Rejected(@"[assembly: RawSprites(Resource = ""a.spr"", Label = ""A"")]
[assembly: RawSprites(Resource = ""b.spr"", Label = ""A"")]", ("a.spr", Frames(1)), ("b.spr", Frames(1)));
        Assert.That(output, Does.Contain("used more than once"));
    }
}
