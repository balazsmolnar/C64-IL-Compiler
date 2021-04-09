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
        private byte[] memory = new byte[64 * 1024];
        private Registers registers = new Registers();
        private int pointer = 0;
        private byte sp = 255;

        public Emulator()
        {
            LoadRom();
        }
        private void Push(byte value)
        {
            memory[0x100 + sp] = value;
            sp--;
        }

        private byte Pull()
        {
            sp++;
            return memory[0x100 + sp];
        }

        private int GetAddress(AddressingMode mode, byte byte1, byte byte2)
        {
            switch (mode)
            {
                case AddressingMode.IndexedIndirectX:
                    {

                        int addr = byte1 + registers.X;
                        return memory[addr] + 256 * memory[addr + 1];
                    }
                case AddressingMode.IndirectIndexedY:
                    {
                        byte b1 = memory[byte1];
                        byte b2 = memory[byte1 + 1];
                        return GetAddress(AddressingMode.AbsoluteY, b1, b2);
                    }
                case AddressingMode.Indirect:
                    return GetAddress(AddressingMode.Absolute, memory[byte1 + byte2 * 256], memory[byte1 + byte2 * 256 + 1]);
                case AddressingMode.Absolute:
                    return byte1 + byte2 * 256;
                case AddressingMode.AbsoluteX:
                    return byte1 + byte2 * 256 + registers.X;
                case AddressingMode.AbsoluteY:
                    return byte1 + byte2 * 256 + registers.Y;
                case AddressingMode.ZeroPage:
                    return byte1;
                case AddressingMode.ZeroPageX:
                    return byte1 + registers.X;
                case AddressingMode.ZeroPageY:
                    return byte1 + registers.Y;
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
            return memory[GetAddress(mode, byte1, byte2)];
        }

        private void SetMemory(int address, byte value)
        {
            if ((address >= 0xa000 && address < 0xc000) || address > 0xe000)
                return;

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
            int sresult = (sbyte) value - (sbyte) GetValue(mode, byte1, byte2);
            registers.Z = uresult == 0;
            registers.C = uresult >= 0;
            registers.N = sresult < 0;
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

        private int counter = 0;
        private void Interrupt()
        {
            if (registers.I == true)
                return;
            counter++;
            if (counter == 100000)
            {
                counter = 0;
                Push((byte)(pointer / 256));
                Push((byte)(pointer % 256));
                Push(registers.P);
                pointer = 0xFF48;
            }
        }

        private bool Step()
        {
            Interrupt();
            var instruction = AssemblyInstructions.GetInstruction(memory[pointer]);
            var b1 = memory[pointer + 1];
            var b2 = memory[pointer + 2];

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
                        // SET_OVERFLOW(!((AC ^ src) & 0x80) && ((AC ^ temp) & 0x80)); 
                        //registry.V = (((oldA ^ value) & 0x80) == 0) && ((registry.A ^ value) & 0x80) > 0;
                        registers.V = (registers.A & 0x80) != (oldA & 0x80);
                        break;
                    }
                case AssemblyInstructionType.SBC:
                    {
                        byte value = GetValue(mode, b1, b2);
                        int result = registers.A - value - (registers.C ? 0 : 1);
                        registers.C = (result >= 0);
                        byte oldA = registers.A;
                        registers.A = (byte)(result >= 0 ? result : result + 0x100);
                        //SET_OVERFLOW(((AC ^ temp) & 0x80) && ((AC ^ src) & 0x80)); 
                        registers.V = (registers.A & 0x80) != (oldA & 0x80);
                        //registry.V = (((oldA ^ value) & 0x80) > 0) && ((registry.A ^ oldA) & 0x80) > 0;

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
                        {
                            SetMemory(GetAddress(mode, b1, b2), value);
                            registers.SetZN(value);
                        }
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
                        {
                            SetMemory(GetAddress(mode, b1, b2), value);
                            registers.SetZN(value);
                        }
                        break;
                    }
                case AssemblyInstructionType.ROL:
                    {
                        {
                            byte value = GetValue(mode, b1, b2);
                            bool c = (value & 0x80) > 0;
                            value = (byte)((value << 1) + (registers.C ? 1 : 0));
                            registers.C = c;
                            if (mode == AddressingMode.Accumulator)
                                registers.A = value;
                            else
                            {
                                SetMemory(GetAddress(mode, b1, b2), value);
                                registers.SetZN(value);
                            }
                            break;
                        }
                    }
                case AssemblyInstructionType.ROR:
                    {
                        {
                            byte value = GetValue(mode, b1, b2);
                            bool c = (value & 0x01) > 0;
                            value = (byte)((value >> 1) + (registers.C ? 0x80 : 0));
                            registers.C = c;
                            if (mode == AddressingMode.Accumulator)
                                registers.A = value;
                            else
                            {
                                SetMemory(GetAddress(mode, b1, b2), value);
                                registers.SetZN(value);
                            }
                            break;
                        }
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

            var basic_address = 0xA000;
            var kernal_address = 0xE000;

            for (int i = 0; i < 0x2000; i++)
            {
                memory[basic_address + i] = content[i];
                memory[kernal_address + i] = content[i + 0x2000];
            }
            // Start(memory[basic_address] + memory[basic_address + 1] * 256);
            // Start(0xFCE2);
        }

        public void SetMemory(int address, params byte[] code)
        {
            for (int i = 0; i < code.Length; i++)
            {
                memory[address + i] = code[i];
            }
        }

        public byte GetMemory(int address)
        {
            return memory[address];
        }

        public void Start(int address)
        {
            pointer = address;
            while (Step()) ;
        }
    }
}
