namespace SimpleEmulator
{
    class Registers
    {
        byte a, x, y, p;

        public byte A
        {
            get
            {
                return a;
            }
            set
            {
                a = value;
                SetZN(value);

            }
        }

        public byte X
        {
            get
            {
                return x;
            }
            set
            {
                x = value;
                SetZN(value);

            }
        }
        public byte Y
        {
            get
            {
                return y;
            }
            set
            {
                y = value;
                SetZN(value);
            }
        }

        //+-+-+-+-+-+-+-+-+
        //|N|V| |B|D|I|Z|C|  PROCESSOR STATUS REG "P"
        //+-+-+-+-+-+-+-+-+
        // | |   | | | | |
        // | |   | | | | +>  CARRY         1=TRUE
        // | |   | | | +-->  ZERO          1=RESULT ZERO
        // | |   | | +---->  IRQ DISABLE   1=DISABLE
        // | |   | +------>  DECIMAL MODE  1=TRUE
        // | |   +-------->  BRK COMMAND
        // | |
        // | +------------>  OVERFLOW      1=TRUE
        // +-------------->  NEGATIVE      1=NEG

        private bool GetBit(byte bit)
        {
            return (p & (1 << bit)) > 0;
        }

        private void SetBit(byte bit, bool value)
        {
            if (value)
                p |= (byte)(1 << bit);
            else
                p &= (byte)(0xff - (1 << bit));
        }

        public bool N
        {
            get
            {
                return GetBit(7);
            }
            set
            {
                SetBit(7, value);
            }
        }

        public bool C
        {
            get
            {
                return GetBit(0);
            }
            set
            {
                SetBit(0, value);
            }
        }

        public bool Z
        {
            get
            {
                return GetBit(1);
            }
            set
            {
                SetBit(1, value);
            }
        }

        public bool V
        {
            get
            {
                return GetBit(6);
            }
            set
            {
                SetBit(6, value);
            }
        }

        public bool I
        {
            get
            {
                return GetBit(2);
            }
            set
            {
                SetBit(2, value);
            }
        }


        public byte P
        {
            get
            {
                return p;
            }
            set
            {
                p = value;
            }
        }

        public void SetZN(byte value)
        {
            Z = value == 0 ? true : false;
            N = (value & 0x80) > 0 ? true : false;

        }
    }
}