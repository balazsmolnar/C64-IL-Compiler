using System.Reflection.Metadata;

namespace Compiler
{
    class ILLine
    {
        public string Label { get; set; }
        public ILOpCode OpCode { get; set; }
        public object Parameter { get; set; }
        public int Position { get; set; }
        public int Size { get; set; }

        public override string ToString()
        {
            return $"{Label?.ToString() ?? "" } {OpCode.ToString()} {Parameter?.ToString() ?? ""}";
        }
    }
}