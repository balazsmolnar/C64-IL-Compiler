using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleEmulator
{
    public class Emulator
    {
        // The one true copy of RAM -- always 64K, always reflects the last
        // write to any address regardless of what's currently banked in for
        // reads (exactly like real C64 hardware: ROM only ever hides RAM
        // from reads, ROM and RAM never share storage, and a write to a
        // ROM-shadowed address always lands in the RAM underneath it).
        private byte[] memory = new byte[64 * 1024];
        // BASIC ($a000-$bfff) and KERNAL ($e000-$ffff) ROM images, loaded
        // once in LoadRom -- kept separate from `memory` so banking BASIC
        // ROM out and back in can never lose or corrupt either the RAM or
        // the ROM content underneath/behind it (the previous model wrote
        // ROM bytes directly into `memory`, so any write to $a000-$bfff
        // while banked out permanently clobbered the ROM image, and reads
        // never checked banking at all -- $a000-$bfff/$e000-$ffff always
        // read as ROM no matter what was written to $01).
        private byte[] basicRom = new byte[0x2000];
        private byte[] kernalRom = new byte[0x2000];
        private Registers registers = new Registers();
        private int pointer = 0;
        private byte sp = 255;

        public Emulator()
        {
            LoadRom();
        }
        private void Push(byte value)
        {
            SetMemory(0x100 + sp, value);
            sp--;
        }

        private byte Pull()
        {
            sp++;
            return ReadByte(0x100 + sp);
        }

        private int GetAddress(AddressingMode mode, byte byte1, byte byte2)
        {
            switch (mode)
            {
                case AddressingMode.IndexedIndirectX:
                    {
                        // Real 6502: both the (byte1+X) pointer lookup and the
                        // hi-byte fetch stay within the zero page -- if
                        // byte1+X or the pointer itself is $FF, the wrap goes
                        // to $00, never spilling into page 1. Previously
                        // unmasked, so e.g. base=$29,X=$FD (byte1+X=$126)
                        // read/wrote real RAM at $126 instead of wrapping to
                        // zero page $26 -- found via BASIC ROM's FDIV, whose
                        // internal division loop stores its computed quotient
                        // bytes at a zero-page address computed exactly this
                        // way (X counts down from a negative offset), so the
                        // quotient silently landed outside the zero page and
                        // the routine read back stale zeros in its place.
                        int addr = (byte1 + registers.X) & 0xFF;
                        return ReadByte(addr) + 256 * ReadByte((addr + 1) & 0xFF);
                    }
                case AddressingMode.IndirectIndexedY:
                    {
                        byte b1 = ReadByte(byte1);
                        byte b2 = ReadByte((byte1 + 1) & 0xFF);
                        return GetAddress(AddressingMode.AbsoluteY, b1, b2);
                    }
                case AddressingMode.Indirect:
                    return GetAddress(AddressingMode.Absolute, ReadByte(byte1 + byte2 * 256), ReadByte(byte1 + byte2 * 256 + 1));
                case AddressingMode.Absolute:
                    return byte1 + byte2 * 256;
                case AddressingMode.AbsoluteX:
                    return byte1 + byte2 * 256 + registers.X;
                case AddressingMode.AbsoluteY:
                    return byte1 + byte2 * 256 + registers.Y;
                case AddressingMode.ZeroPage:
                    return byte1;
                case AddressingMode.ZeroPageX:
                    // Real 6502 zero-page,X always wraps within page 0 (the
                    // 8-bit addition's carry is discarded) -- previously
                    // unmasked here, so a base+X past 255 silently addressed
                    // real RAM/ROM at $100+ instead of wrapping back into the
                    // zero page. See IndexedIndirectX above for how this was
                    // actually found (BASIC ROM's FDIV).
                    return (byte1 + registers.X) & 0xFF;
                case AddressingMode.ZeroPageY:
                    return (byte1 + registers.Y) & 0xFF;
                case AddressingMode.Implied:
                case AddressingMode.Relative:
                case AddressingMode.Accumulator:
                case AddressingMode.Immidiate:
                default:
                    throw new InvalidOperationException("Invalid addressiong mode.");
            }
        }

        private int GetInstructionLength(AddressingMode mode)
        {
            switch (mode)
            {
                case AddressingMode.Relative:
                    return 0;
                case AddressingMode.Implied:
                case AddressingMode.Accumulator:
                    return 1;
                case AddressingMode.IndexedIndirectX:
                case AddressingMode.IndirectIndexedY:
                case AddressingMode.Immidiate:
                case AddressingMode.ZeroPage:
                case AddressingMode.ZeroPageX:
                case AddressingMode.ZeroPageY:
                    return 2;
                case AddressingMode.Indirect:
                case AddressingMode.Absolute:
                case AddressingMode.AbsoluteX:
                case AddressingMode.AbsoluteY:
                    return 3;
                default:
                    throw new ArgumentOutOfRangeException("mode");
            }
        }

        private byte GetValue(AddressingMode mode, byte byte1, byte byte2)
        {
            if (mode == AddressingMode.Immidiate)
                return byte1;
            if (mode == AddressingMode.Accumulator)
                return registers.A;

            //if (GetAddress(mode, byte1, byte2) == 0xd012)
            //    memory[0xd012] = IncByte(memory[0xd012]);
            return ReadByte(GetAddress(mode, byte1, byte2));
        }

        // Tracks the CPU I/O port ($01) LORAM/HIRAM bits: LORAM=0 banks out
        // BASIC ROM ($a000-$bfff reads as RAM), HIRAM=0 banks out KERNAL ROM
        // ($e000-$ffff reads as RAM). Both start true (the KERNAL-default
        // power-on state, $01=$37), matching real hardware/VICE before any
        // program writes to $01. CHAREN (I/O vs character ROM at
        // $d000-$dfff) isn't modeled -- this emulator has no memory-mapped
        // I/O chips or character ROM to bank in the first place, and this
        // project's own code never sets CHAREN to anything but 1 anyway.
        private bool basicRomMapped = true;
        private bool kernalRomMapped = true;

        // The only place ROM banking actually matters: reads of $a000-$bfff/
        // $e000-$ffff return the corresponding ROM image's bytes while
        // mapped in, and fall through to RAM otherwise -- everywhere else
        // (zero page, the stack, ordinary RAM) is untouched by banking.
        // Every instruction-fetch and operand read in Step()/GetAddress/
        // GetValue goes through this (not direct `memory[...]` indexing),
        // so this is a real, general banking model a test can exercise
        // through arbitrary compiled code, not a special case for one call.
        private byte ReadByte(int address)
        {
            if (basicRomMapped && address >= 0xa000 && address < 0xc000)
                return basicRom[address - 0xa000];
            if (kernalRomMapped && address >= 0xe000 && address <= 0xffff)
                return kernalRom[address - 0xe000];
            return memory[address];
        }

        // Writes always land in RAM, regardless of what's currently banked
        // in for reads -- exactly real hardware's behavior (and the reason
        // "hide data under ROM" is a real, common C64 technique). The
        // previous version instead silently DISCARDED writes to
        // $a000-$bfff while BASIC ROM was mapped in (and unconditionally
        // discarded every write above $e000, with no HIRAM check at all) --
        // plausible-looking but not what real hardware does, and unable to
        // model "this write should still work, the caller just can't see
        // its own ROM-shadowed write to RAM until banking it out again."
        private void SetMemory(int address, byte value)
        {
            if (address == 0x01)
            {
                basicRomMapped = (value & 0x01) != 0;
                kernalRomMapped = (value & 0x02) != 0;
            }

            memory[address] = value;
        }

        private void Branch(bool value, byte relOffset)
        {
            if (!value)
                pointer += 2;
            else
            {
                if ((relOffset & 128) > 0)
                    pointer -= 256 - relOffset - 2;
                else
                {
                    pointer += 2 + relOffset;
                }

            }
        }

        private void Compare(byte value, AddressingMode mode, byte byte1, byte byte2)
        {
            int uresult = value - GetValue(mode, byte1, byte2);
            registers.Z = uresult == 0;
            registers.C = uresult >= 0;
            // Real 6502 CMP/CPX/CPY sets N from bit 7 of the truncated 8-bit
            // subtraction result (value - operand, mod 256), not from a
            // signed-vs-signed comparison -- those diverge whenever the two
            // operands' sign bits differ from the result's, e.g. $80 vs $01:
            // real hardware computes $80-$01=$7F (N=0), but (sbyte)$80 -
            // (sbyte)$01 = -128-1 = -129 (out of sbyte range entirely, reads
            // as "negative" -> N=1 here previously). uresult & 0xFF gives the
            // correct truncated result regardless of how far uresult strayed
            // outside [-128,127] -- C#'s int is two's complement, so bitwise
            // AND on a negative uresult already yields the right low byte.
            registers.N = (uresult & 0x80) != 0;
        }

        private byte IncByte(byte value)
        {
            if (value == 255)
                return 0;
            return (byte)(value + 1);
        }

        private byte DecByte(byte value)
        {
            if (value == 0)
                return 255;
            return (byte)(value - 1);
        }

        // NOTE: this used to fire a synthetic IRQ every 100000 instructions
        // (when the I flag was clear), pushing PC/status and jumping to
        // $FF48 -- the real KERNAL ROM's IRQ entry point, which eventually
        // does JMP ($0314) through the user IRQ vector. This emulator never
        // runs the KERNAL's own reset/init routine (callers jump straight
        // to their program's own entry point, e.g. Start(0x1000)), so
        // $0314/$0315 are never initialized and read back as $0000 --
        // meaning the "interrupt" jumped into whatever 6502 opcode happens
        // to sit at address 0, corrupting execution non-deterministically
        // in any test that ran long enough to cross the 100000-instruction
        // mark (confirmed: this was the actual cause of GCTest's sustained
        // GC.Collect() failures, not a bug in the GC code itself). No test
        // in this suite exercises real interrupt-handler behavior through
        // this path, so removed rather than fixed properly (which would
        // need this emulator to actually initialize the KERNAL vector
        // table, e.g. by running LoadRom()'s reset routine first).
        private void Interrupt()
        {
        }

        private bool Step()
        {
            Interrupt();
            var instruction = AssemblyInstructions.GetInstruction(ReadByte(pointer));
            var b1 = ReadByte(pointer + 1);
            var b2 = ReadByte(pointer + 2);

            var intructionType = instruction.InstructionType;
            var mode = instruction.AddressingMode;

            pointer += GetInstructionLength(mode);

            switch (intructionType)
            {
                case AssemblyInstructionType.LDA:
                    registers.A = GetValue(mode, b1, b2);
                    break;
                case AssemblyInstructionType.LDX:
                    registers.X = GetValue(mode, b1, b2);
                    break;
                case AssemblyInstructionType.LDY:
                    registers.Y = GetValue(mode, b1, b2);
                    break;
                case AssemblyInstructionType.STA:
                    SetMemory(GetAddress(mode, b1, b2), registers.A);
                    break;
                case AssemblyInstructionType.STX:
                    SetMemory(GetAddress(mode, b1, b2), registers.X);
                    break;
                case AssemblyInstructionType.STY:
                    SetMemory(GetAddress(mode, b1, b2), registers.Y);
                    break;
                case AssemblyInstructionType.BNE:
                    Branch(!registers.Z, b1);
                    break;
                case AssemblyInstructionType.BEQ:
                    Branch(registers.Z, b1);
                    break;
                case AssemblyInstructionType.BCC:
                    Branch(!registers.C, b1);
                    break;
                case AssemblyInstructionType.BCS:
                    Branch(registers.C, b1);
                    break;
                case AssemblyInstructionType.BMI:
                    Branch(registers.N, b1);
                    break;
                case AssemblyInstructionType.BPL:
                    Branch(!registers.N, b1);
                    break;
                case AssemblyInstructionType.BVC:
                    Branch(!registers.V, b1);
                    break;
                case AssemblyInstructionType.BVS:
                    Branch(registers.V, b1);
                    break;
                case AssemblyInstructionType.INX:
                    registers.X = registers.X == 255 ? (byte)0 : (byte)(registers.X + 1);
                    break;
                case AssemblyInstructionType.INY:
                    registers.Y = registers.Y == 255 ? (byte)0 : (byte)(registers.Y + 1);
                    break;
                case AssemblyInstructionType.DEX:
                    registers.X = registers.X == 0 ? (byte)255 : (byte)(registers.X - 1);
                    break;
                case AssemblyInstructionType.DEY:
                    registers.Y = registers.Y == 0 ? (byte)255 : (byte)(registers.Y - 1);
                    break;
                case AssemblyInstructionType.TAX:
                    registers.X = registers.A;
                    break;
                case AssemblyInstructionType.TAY:
                    registers.Y = registers.A;
                    break;
                case AssemblyInstructionType.TXA:
                    registers.A = registers.X;
                    break;
                case AssemblyInstructionType.TYA:
                    registers.A = registers.Y;
                    break;
                case AssemblyInstructionType.CMP:
                    Compare(registers.A, mode, b1, b2);
                    break;
                case AssemblyInstructionType.CPX:
                    Compare(registers.X, mode, b1, b2);
                    break;
                case AssemblyInstructionType.CPY:
                    Compare(registers.Y, mode, b1, b2);
                    break;
                case AssemblyInstructionType.INC:
                    {
                        byte value = IncByte(GetValue(mode, b1, b2));
                        SetMemory(GetAddress(mode, b1, b2), value);
                        registers.SetZN(value);
                        break;
                    }
                case AssemblyInstructionType.DEC:
                    {
                        byte value = DecByte(GetValue(mode, b1, b2));
                        SetMemory(GetAddress(mode, b1, b2), value);
                        registers.SetZN(value);
                        break;
                    }

                case AssemblyInstructionType.JMP:
                    pointer = GetAddress(mode, b1, b2);
                    break;
                case AssemblyInstructionType.AND:
                    registers.A = (byte)(registers.A & GetValue(mode, b1, b2));
                    break;
                case AssemblyInstructionType.EOR:
                    registers.A = (byte)(registers.A ^ GetValue(mode, b1, b2));
                    break;
                case AssemblyInstructionType.ORA:
                    registers.A = (byte)(registers.A | GetValue(mode, b1, b2));
                    break;
                case AssemblyInstructionType.CLC:
                    registers.C = false;
                    break;
                case AssemblyInstructionType.CLV:
                    registers.V = false;
                    break;
                case AssemblyInstructionType.SEC:
                    registers.C = true;
                    break;
                case AssemblyInstructionType.CLI:
                    registers.I = false;
                    break;
                case AssemblyInstructionType.SEI:
                    registers.I = true;
                    break;
                case AssemblyInstructionType.NOP:
                    break;
                case AssemblyInstructionType.PHA:
                    Push(registers.A);
                    break;
                case AssemblyInstructionType.PHP:
                    Push(registers.P);
                    break;
                case AssemblyInstructionType.PLA:
                    registers.A = Pull();
                    break;
                case AssemblyInstructionType.PLP:
                    registers.P = Pull();
                    break;
                case AssemblyInstructionType.JSR:
                    pointer -= 1;
                    Push((byte)(pointer / 256));
                    Push((byte)(pointer % 256));
                    pointer = GetAddress(mode, b1, b2);
                    break;
                case AssemblyInstructionType.RTS:
                    pointer = Pull() + 256 * Pull() + 1;
                    break;
                case AssemblyInstructionType.RTI:
                    registers.P = Pull();
                    pointer = Pull() + 256 * Pull();
                    break;
                case AssemblyInstructionType.TSX:
                    registers.X = sp;
                    break;
                case AssemblyInstructionType.TXS:
                    sp = registers.X;
                    break;

                case AssemblyInstructionType.ADC:
                    {
                        byte value = GetValue(mode, b1, b2);
                        int result = registers.A + value + (registers.C ? 1 : 0);
                        registers.C = result > 0xFF;
                        byte oldA = registers.A;
                        registers.A = (byte)(result & 0xFF);
                        // Real 6502 overflow rule: set when the two operands share a sign
                        // but the (truncated) result's sign differs from theirs. The
                        // previous "V = did the sign bit change" here was wrong -- that
                        // fires on any sign flip, including plenty of non-overflowing
                        // cases, which silently breaks anything using BVC/BVS after
                        // ADC/SBC (e.g. the standard signed-compare-via-overflow-
                        // correction technique).
                        registers.V = ((~(oldA ^ value) & (oldA ^ registers.A)) & 0x80) != 0;
                        break;
                    }
                case AssemblyInstructionType.SBC:
                    {
                        byte value = GetValue(mode, b1, b2);
                        int result = registers.A - value - (registers.C ? 0 : 1);
                        registers.C = (result >= 0);
                        byte oldA = registers.A;
                        registers.A = (byte)(result >= 0 ? result : result + 0x100);
                        // See ADC above. SBC A,M is equivalent to ADC A,~M, so the
                        // overflow condition uses (oldA ^ value) rather than its
                        // complement.
                        registers.V = ((oldA ^ value) & (oldA ^ registers.A) & 0x80) != 0;
                        break;
                    }
                case AssemblyInstructionType.ASL:
                    {
                        byte value = GetValue(mode, b1, b2);
                        registers.C = (value & 0x80) > 0;
                        value = (byte)(value << 1);
                        if (mode == AddressingMode.Accumulator)
                            registers.A = value;
                        else
                            SetMemory(GetAddress(mode, b1, b2), value);
                        // On real 6502 hardware, ASL/LSR/ROL/ROR always update N/Z
                        // regardless of addressing mode -- this used to only call
                        // SetZN in the memory-operand branch, silently leaving N/Z
                        // stale after "ASL A"/"ROL A"/etc, which breaks any BMI/BPL/
                        // BEQ/BNE that follows one (found via a real C64 BASIC ROM
                        // routine, FDIV, whose internal long-division loop shifts the
                        // accumulator and branches on the result -- it was landing on
                        // the wrong branch and silently producing a zeroed FAC1
                        // instead of the actual quotient).
                        registers.SetZN(value);
                        break;
                    }
                case AssemblyInstructionType.LSR:
                    {
                        byte value = GetValue(mode, b1, b2);
                        registers.C = (value & 0x01) > 0;
                        value = (byte)(value >> 1);
                        if (mode == AddressingMode.Accumulator)
                            registers.A = value;
                        else
                            SetMemory(GetAddress(mode, b1, b2), value);
                        registers.SetZN(value);
                        break;
                    }
                case AssemblyInstructionType.ROL:
                    {
                        byte value = GetValue(mode, b1, b2);
                        bool c = (value & 0x80) > 0;
                        value = (byte)((value << 1) + (registers.C ? 1 : 0));
                        registers.C = c;
                        if (mode == AddressingMode.Accumulator)
                            registers.A = value;
                        else
                            SetMemory(GetAddress(mode, b1, b2), value);
                        registers.SetZN(value);
                        break;
                    }
                case AssemblyInstructionType.ROR:
                    {
                        byte value = GetValue(mode, b1, b2);
                        bool c = (value & 0x01) > 0;
                        value = (byte)((value >> 1) + (registers.C ? 0x80 : 0));
                        registers.C = c;
                        if (mode == AddressingMode.Accumulator)
                            registers.A = value;
                        else
                            SetMemory(GetAddress(mode, b1, b2), value);
                        registers.SetZN(value);
                        break;
                    }
                case AssemblyInstructionType.BIT:
                    {
                        byte value = GetValue(mode, b1, b2);
                        int result = registers.A & value;
                        registers.Z = result == 0;
                        //if (result != 0)
                        {
                            registers.N = (value & 0x80) > 0;
                            registers.V = (value & 0x40) > 0;
                        }
                        break;
                    }
                case AssemblyInstructionType.CLD:
                    break;
                case AssemblyInstructionType.BRK:
                    return false;
                case AssemblyInstructionType.UNDEF:
                case AssemblyInstructionType.SED:
                default:
                    throw new NotSupportedException(string.Format("Command: {0}", intructionType));


            }
            return true;
        }

        public void LoadPrg(string prgFile)
        {
            var content = File.ReadAllBytes(prgFile);
            var address = content[0] + content[1] * 256;

            for (int i = 0; i < content.Length - 2; i++)
            {
                memory[address + i] = content[i + 2];
            }
        }

        private void LoadRom()
        {
            byte[] content;
            var assembly = typeof(Emulator).Assembly;

            using (var stream = assembly.GetManifestResourceStream("SimpleEmulator.64c.251913-01.bin"))
            {

                using (var reader = new BinaryReader(stream))
                {
                    content = reader.ReadBytes((int)stream.Length);
                }
            }

            for (int i = 0; i < 0x2000; i++)
            {
                basicRom[i] = content[i];
                kernalRom[i] = content[i + 0x2000];
            }
            // Start(basicRom[0] + basicRom[1] * 256);
            // Start(0xFCE2);
        }

        // Routed through the same banking-aware SetMemory(int, byte) used
        // internally -- lets a test poke $01 directly and have it actually
        // take effect, e.g. to set up a specific banking state before
        // checking GetMemory's result.
        public void SetMemory(int address, params byte[] code)
        {
            for (int i = 0; i < code.Length; i++)
            {
                SetMemory(address + i, code[i]);
            }
        }

        // Banking-aware: reads $a000-$bfff/$e000-$ffff as ROM or RAM
        // depending on the last write to $01, exactly like a real read at
        // that address would see. Use this (not some other means) to
        // confirm banking is actually working -- e.g. GetMemory(0xa000)
        // should differ before and after toggling LORAM.
        public byte GetMemory(int address)
        {
            return ReadByte(address);
        }

        public void Start(int address)
        {
            pointer = address;
            while (Step()) ;
        }

        // Same as Start, but calls back before every instruction with the
        // current PC and step count (and stops after maxSteps if the
        // program never halts on its own) -- for diagnosing a specific
        // failing run (tracing where execution goes, catching an exception
        // with the PC it happened at) without needing a debugger attached.
        public void Start(int address, long maxSteps, Action<int, long> onStep)
        {
            pointer = address;
            long steps = 0;
            while (true)
            {
                onStep(pointer, steps);
                if (!Step())
                    break;
                steps++;
                if (steps >= maxSteps)
                    break;
            }
        }
    }
}
