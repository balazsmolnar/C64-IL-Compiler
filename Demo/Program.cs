using System;
using C64Lib;

namespace Demo;

// Small example project -- exercises the compiler end-to-end without the
// size/complexity of a full game. For the Hunchback game (previously here),
// see the Hunchback/ project.
//
// Just the rotating wireframe cube (see RotatingCube.cs) -- the float-
// arithmetic text demo and the bitmap-shapes scene that used to run
// alongside it here were dropped: not enough headroom for both this
// program's own code and the object/GC tables before the fixed $d000
// boundary (asm/helper/objectTables.asm) -- see RotatingCube.cs's own
// class comment for the heap-pressure history this already went through
// once with the bitmap scene alone.
class Program
{
    static void Main()
    {
        C64.Screen.SetScreenMode(ScreenMode.MultiColor);
        C64.Screen.SetBackgroundColor(Colors.LightBlue);
        // MatrixHigh=White(1), MatrixLow=Red(2) -- two of the cube's own
        // three axis-pair face colors (RotatingCube.ColorX/Y/Z); the third
        // (ColorRam, Green) is filled below.
        C64.Screen.SetBitmapColors(0x12);
        // ColorRam ($D800, 1000 cells, one nibble each; FillMemory maxes
        // at 256 bytes/call, hence 4 calls) -- LightGreen for the Z faces.
        C64.FillMemory(0xD800UL, 0x0D, 250);
        C64.FillMemory(0xD800UL + 250UL, 0x0D, 250);
        C64.FillMemory(0xD800UL + 500UL, 0x0D, 250);
        C64.FillMemory(0xD800UL + 750UL, 0x0D, 250);

        // See RotatingCube.cs. Created with a plain `new` (no constructor
        // body -- not supported, see OpNewObj) and set up via Init().
        var cube = new RotatingCube();
        cube.Init();
        for (;;)
        {
            cube.Step();
        }
    }
}
