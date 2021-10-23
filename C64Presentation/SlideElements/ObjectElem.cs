using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C64Presentation.SlideElements
{
    class ObjectElem
    {
        public uint Size;
        public string Id;
        public bool Alive;
        public bool Marked;
        public bool Root;
        public bool Processing;
        public ObjectElem[] References;
    }
}
