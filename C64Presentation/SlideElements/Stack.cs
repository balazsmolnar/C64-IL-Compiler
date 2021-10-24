using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using C64Lib;
using C64Presentation.Helper;

namespace C64Presentation.SlideElements
{
    class Stack : SlideElement
    {
        private ObjectElem[] elements;
        private int pointer;
        private Box box;

        public uint X;
        public uint Y;

        public void Init(int maxSize)
        {
            elements = new ObjectElem[maxSize];
            pointer = -1;
            box = new Box() { Color = Colors.Violet, X = X, Y = Y - (uint)pointer, Width = 5, Height = (uint)(pointer + 3) };
        }

        public void Push(ObjectElem elem)
        {
            pointer++;
            elements[pointer] = elem;
        }

        public ObjectElem Pull()
        {
            var result = elements[pointer];
            pointer--;
            return result;
        }

        public bool IsEmpty => pointer == -1;
        public override void Draw()
        {
            this.box.Y = Y - (uint) pointer;
            this.box.Height = (uint) (pointer + 3);
            Screen.ClearRect(X, X+5, Y-(uint)pointer-1, Y+1);
            
            box.Draw();

            for (int p = 0; p <= pointer; p++)
            {
                C64.Write(X+2, Y-(uint)pointer + (uint)(p+1), elements[pointer-p].Id, Colors.Yellow);
            }
        }
    }
}
