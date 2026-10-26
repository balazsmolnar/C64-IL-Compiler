using System;

namespace Compiler.Ops;

// Built by ILMethodStaticArrayAccessOptimizer: element access through a
// static array field with a simple index -- the index straight into Y,
// the handle straight into X, no hardware-stack round trip for either.
abstract class OpStaticArrayAccess : OpBase
{
    private readonly ILOperation _array;
    private readonly ILOperation _index;
    protected readonly int ElementBytes;

    protected OpStaticArrayAccess(ILOperation array, ILOperation index, int elementBytes) : base(0)
    {
        _array = array;
        _index = index;
        ElementBytes = elementBytes;
    }

    // Y = index * element size; X = the array's handle; tmpPointer = its data.
    protected string Address()
    {
        string y;
        bool wide = ElementBytes == 2;
        switch (_index.Operation)
        {
            case OpLdConst:
                y = $"ldy #<{(wide ? (int)_index.RawParameter * 2 : (int)_index.RawParameter)}";
                break;
            case OpLdsld:
                y = wide ? $"lda {_index.RawParameter}\nasl\ntay" : $"ldy {_index.RawParameter}";
                break;
            default: // unpromoted local or parameter: RawParameter is its rel_pos
                y = $"ldy stackPointer\nlda localsStack-{_index.RawParameter},y\n{(wide ? "asl\n" : "")}tay";
                break;
        }
        return $"{y}\nldx {_array.RawParameter}\njsr resolveObjPtr";
    }
}

class OpStaticArrayLoad : OpStaticArrayAccess
{
    public OpStaticArrayLoad(ILOperation array, ILOperation index, int elementBytes) : base(array, index, elementBytes)
    {
    }

    public override string Emit(CompilerMethodContext context, ILOperation operation) =>
        Address() + (ElementBytes == 2
            ? "\niny\nlda (tmpPointer),y\npha\ndey\nlda (tmpPointer),y\npha"
            : "\nlda (tmpPointer),y\npha");
}

class OpStaticArrayStore : OpStaticArrayAccess
{
    public OpStaticArrayStore(ILOperation array, ILOperation index, int elementBytes) : base(array, index, elementBytes)
    {
    }

    public override string Emit(CompilerMethodContext context, ILOperation operation) =>
        Address() + (ElementBytes == 2
            ? "\npla\nsta (tmpPointer),y\niny\npla\nsta (tmpPointer),y"
            : "\npla\nsta (tmpPointer),y");
}
