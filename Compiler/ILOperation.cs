using System.Reflection.Metadata;
using Compiler.Ops;

namespace Compiler
{
    class ILOperation
    {
        public string Label { get; set; }
        public ILOpCode OpCode { get; set; }
        public object RawParameter { get; set; }
        public int Position { get; set; }
        public int Size { get; set; }
        public bool Optimized { get; set; }
        public OpBase Operation { get; set; }
        public override string ToString()
        {
            return $"{Label?.ToString() ?? "" } {OpCode.ToString()} {RawParameter?.ToString() ?? ""}";
        }
    }
}