using System.Reflection.Metadata;
using Compiler.Ops;

namespace Compiler
{
    class ILLine
    {
        public string Label { get; set; }
        public ILOpCode OpCode { get; set; }
        public object Parameter { get; set; }
        public int Position { get; set; }
        public int Size { get; set; }
        public bool Optimized { get; set; }

        public OpBase Operand { get; set; }

        public override string ToString()
        {
            return $"{Label?.ToString() ?? "" } {OpCode.ToString()} {Parameter?.ToString() ?? ""}";
        }
    }
}