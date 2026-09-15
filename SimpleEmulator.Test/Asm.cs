using System.Collections.Generic;

namespace SimpleEmulator.Test;

// Tiny hand-rolled 6502 assembler, just the handful of instructions these
// tests need to drive the BASIC ROM's float routines directly. Exists so
// test bodies read as "SEI, load pointer, call routine, ..." instead of
// raw hex byte arrays.
class Asm
{
    private readonly List<byte> bytes = new();

    public Asm Sei() { bytes.Add(0x78); return this; }
    public Asm Cli() { bytes.Add(0x58); return this; }
    public Asm Brk() { bytes.Add(0x00); return this; }
    public Asm Tya() { bytes.Add(0x98); return this; }
    public Asm LdaImm(byte v) { bytes.Add(0xA9); bytes.Add(v); return this; }
    public Asm LdxImm(byte v) { bytes.Add(0xA2); bytes.Add(v); return this; }
    public Asm LdyImm(byte v) { bytes.Add(0xA0); bytes.Add(v); return this; }
    public Asm StaZp(int zp) { bytes.Add(0x85); bytes.Add((byte)zp); return this; }

    public Asm StaAbs(int addr)
    {
        bytes.Add(0x8D);
        bytes.Add((byte)(addr & 0xFF));
        bytes.Add((byte)(addr >> 8));
        return this;
    }

    public Asm Jsr(int addr)
    {
        bytes.Add(0x20);
        bytes.Add((byte)(addr & 0xFF));
        bytes.Add((byte)(addr >> 8));
        return this;
    }

    // "A=lo,Y=hi of address" -- the calling convention MOVFM/FADD/FSUB/
    // FMULT/FDIV/FCOMP all take for their memory-pointer argument.
    public Asm PointerInAY(int address) => LdaImm((byte)(address & 0xFF)).LdyImm((byte)(address >> 8));

    // "X=lo,Y=hi of address" -- MOVMF's destination-pointer convention.
    public Asm PointerInXY(int address) => LdxImm((byte)(address & 0xFF)).LdyImm((byte)(address >> 8));

    public byte[] ToArray() => bytes.ToArray();
}
