using System;

namespace TestDebugger;

// One decoded value at any depth -- a local's own value, or (recursively,
// via ObjectInspector.Expand) a field's or array element's.
class InspectedValue
{
    public string Summary;   // one-line display text, e.g. "5", "TestObject#6", "uint[5]", "null"
    public Type StaticType;  // reflected type this was read as -- needed to expand children
    public bool IsReference; // true for object/array, even when IsNull
    public bool IsNull;      // handle == 0, or objTableHigh[handle] == 0 (never allocated)
    public int Handle;       // valid only when IsReference && !IsNull

    // Where this value's own byte(s) live -- a localsStack offset for a
    // local/parameter, or a heap offset for a field/array element (see
    // ObjectInspector.Read, the only place this gets set). Needed so a
    // SCALAR value can be written back in place (DapServer's
    // "setVariable" handling); never meaningful for IsReference (editing
    // which object a reference points to isn't supported).
    public int Address;
}
