using System.Runtime.InteropServices;
using System.Reflection;
using Compiler;

namespace Compiler.Ops
{
    internal class OpBase
    {
        public OpBase(int parameterSize)
        {
            ParameterSize = parameterSize;
        }

        public OpBase(int parameterSize, string command) : this(parameterSize)
        {
            Command = command;
        }

        public string Command { get; }
        public int ParameterSize { get; }

        public virtual string Emit(CompilerMethodContext context, object parameter)
        {
            if (parameter == null)
                return Command;
            return $"{Command} {parameter}";
        }

        public virtual object ConvertParameter(CompilerMethodContext context, int parameter) => null;
    }

    class OpLdstr : OpBase
    {
        public OpLdstr() : base(4, "+stack_push_pointer")
        {
        }

        public override object ConvertParameter(CompilerMethodContext context, int parameter)
        {
            context.CompilerContext.StringValues.Add(parameter, context.CompilerContext.Assembly.ManifestModule.ResolveString(parameter));
            return $".string_{parameter}";
        }
    }

    class OpLdftn : OpBase
    {
        public OpLdftn() : base(4, "+stack_push_pointer")
        {
        }

        public override object ConvertParameter(CompilerMethodContext context, int parameter)
        {
            return context.CompilerContext.Assembly.ManifestModule.ResolveMethod(parameter).GetLabel();
        }
    }

    class OpCall : OpBase
    {
        public OpCall() : base(4, "jsr")
        {
        }

        public override object ConvertParameter(CompilerMethodContext context, int parameter)
        {
            return context.CompilerContext.Assembly.ManifestModule.ResolveMethod(parameter).GetLabel();
        }
    }

    class OpNewObj : OpBase
    {
        public OpNewObj() : base(4, "+newObj")
        {
        }

        public override object ConvertParameter(CompilerMethodContext context, int parameter)
        {
            var method = context.CompilerContext.Assembly.ManifestModule.ResolveMethod(parameter);
            var t = method.DeclaringType;
            var size = 0;
            var referenceFields = 0;
            foreach (var f in t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (f.FieldType == typeof(string))
                {
                    size += 2;
                }
                else
                {
                    size += 1;
                }
                if (f.FieldType.IsReferenceCounted())
                    referenceFields++;
            }
            //var label = context.CompilerContext.Assembly.ManifestModule.ResolveMethod(parameter).GetLabel();

            return $"{size}, {referenceFields}";
        }
    }

    class OpNewArr : OpBase
    {
        public OpNewArr() : base(4, "+newArr")
        {
        }

        public override object ConvertParameter(CompilerMethodContext context, int parameter)
        {
            return "";
        }
    }

    class OpStfld : OpBase
    {
        public OpStfld() : base(4, "+stfld")
        {
        }

        public override object ConvertParameter(CompilerMethodContext context, int parameter)
        {
            var field = context.CompilerContext.Assembly.ManifestModule.ResolveField(parameter);
            var t = field.DeclaringType;
            var pos = 0;
            foreach (var f in t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (f == field)
                    break;
                if (f.FieldType == typeof(string))
                {
                    pos += 2;
                }
                else
                {
                    pos += 1;
                }
            }
            return $"{pos}";
        }
    }

    class OpStElem_ref : OpBase
    {
        public OpStElem_ref() : base(0, "+stelemRef")
        {
        }
    }

    class OpLdfld : OpBase
    {
        public OpLdfld() : base(4, "+ldfld")
        {
        }

        public override object ConvertParameter(CompilerMethodContext context, int parameter)
        {
            var field = context.CompilerContext.Assembly.ManifestModule.ResolveField(parameter);
            var t = field.DeclaringType;
            var pos = 0;
            foreach (var f in t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (f == field)
                    break;
                if (f.FieldType == typeof(string))
                {
                    pos += 2;
                }
                else
                {
                    pos += 1;
                }
            }
            return $"{pos}";
        }
    }

    class OpDup : OpBase
    {
        public OpDup() : base(0, "+stack_duplicate")
        {

        }
    }

    class OpPushBase : OpBase
    {
        public OpPushBase(int parameterSize) : base(parameterSize, "+stack_push_int")
        {

        }

        public OpPushBase(int parameterSize, string command) : base(parameterSize, command) { }
    }

    class OpLdConst : OpPushBase
    {
        public OpLdConst(int parameterSize) : base(parameterSize)
        {

        }
    }

    class OpLdc_i4_const : OpLdConst
    {
        private int _value;
        public OpLdc_i4_const(int value) : base(0)
        {
            _value = value;
        }

        public override object ConvertParameter(CompilerMethodContext context, int parameter)
        {
            return _value;
        }
    }

    class OpLdc_i4 : OpLdConst
    {
        public OpLdc_i4() : base(4)
        {
        }

        public override object ConvertParameter(CompilerMethodContext context, int parameter)
        {
            return parameter;
        }
    }

    class OpLdc_i4_s : OpLdConst
    {
        public OpLdc_i4_s() : base(1)
        {
        }

        public override object ConvertParameter(CompilerMethodContext context, int parameter)
        {
            return parameter;
        }
    }

    class OpLdloc : OpPushBase
    {
        public int VarIndex;
        public OpLdloc(int varIndex) : base(0, "+stack_push_var")
        {
            VarIndex = varIndex;
        }

        public override object ConvertParameter(CompilerMethodContext context, int parameter) => $".{context.Method.GetLabel()}_var{VarIndex}";
    }

    class OpLdloc_s : OpPushBase
    {
        public OpLdloc_s() : base(1, "+stack_push_var")
        {
        }

        public override object ConvertParameter(CompilerMethodContext context, int parameter) => $".{context.Method.GetLabel()}_var{parameter}";
    }

    class OpLdarg : OpPushBase
    {
        private int _argIndex;
        public OpLdarg(int argIndex) : base(0, "+stack_push_var")
        {
            _argIndex = argIndex;
        }

        public override object ConvertParameter(CompilerMethodContext context, int parameter)
        {
            string paramName = "";
            bool isInstance = !context.Method.IsStatic;
            if (isInstance && _argIndex == 0)
                paramName = "this";
            else if (isInstance)
                paramName = context.Method.GetParameters()[_argIndex - 1].Name;
            else
                paramName = context.Method.GetParameters()[_argIndex].Name;
            return $".{context.Method.GetLabel()}_{paramName}";
        }
    }

    class OpLdsld : OpPushBase
    {
        public OpLdsld() : base(4, "+stack_push_var")
        {
        }

        public override object ConvertParameter(CompilerMethodContext context, int parameter)
        {
            var field = context.Method.DeclaringType.Module.ResolveField(parameter);
            return $".{context.Method.DeclaringType.Name}_field_{field.Name}";
        }
    }

    class OpLdnull : OpPushBase
    {
        public OpLdnull() : base(0)
        {
        }

        public override object ConvertParameter(CompilerMethodContext context, int parameter) => 0;
    }

    class OpPullBase : OpBase
    {
        public OpPullBase(int parameterSize) : base(parameterSize, "+stack_pull_int")
        {

        }
    }

    class OpStloc : OpBase
    {
        public int VarIndex { get; }
        public OpStloc(int varIndex) : base(0, "+stack_pull_int_ref")
        {
            VarIndex = varIndex;
        }

        public override object ConvertParameter(CompilerMethodContext context, int parameter)
        {

            var body = context.Method.GetMethodBody();
            var variables = body.LocalVariables;
            var isRef = variables[VarIndex].LocalType.IsReferenceCounted() ? "1" : "0";
            return $".{context.Method.GetLabel()}_var{VarIndex}, {isRef}";
        }
    }

    class OpStsfld : OpBase
    {
        public OpStsfld() : base(4, "+stack_pull_int_ref")
        {
        }
        public override object ConvertParameter(CompilerMethodContext context, int parameter)
        {
            var field = context.Method.DeclaringType.Module.ResolveField(parameter);
            var address = $".{context.Method.DeclaringType.Name}_field_{field.Name}";
            string isRef = field.FieldType.IsReferenceCounted() ? "1" : "0";
            return $"{address}, {isRef}";
        }
    }

    class OpStloc_s : OpBase
    {
        public OpStloc_s() : base(1, "+stack_pull_int_ref")
        {
        }

        public override object ConvertParameter(CompilerMethodContext context, int parameter)
        {
            var body = context.Method.GetMethodBody();
            var variables = body.LocalVariables;
            var isRef = variables[parameter].LocalType.IsReferenceCounted() ? "1" : "0";
            return $".{context.Method.GetLabel()}_var{parameter}, {isRef}";
        }

    }

    class OpShortJump : OpBase
    {
        public OpShortJump(string command) : base(1, command)
        {
        }

        public override object ConvertParameter(CompilerMethodContext context, int parameter) => (parameter < 128 ? (int)parameter : (int)parameter - 256) + 2;
    }

    class OpLongJump : OpBase
    {
        public OpLongJump(string command) : base(4, command)
        {
        }

        public override object ConvertParameter(CompilerMethodContext context, int parameter) => parameter + 5;
    }

    class OpArithmetic2 : OpBase
    {
        public OpArithmetic2(string command) : base(0, command)
        {
        }
    }

    class OpArithmetic1 : OpBase
    {
        public OpArithmetic1(string command) : base(0, command)
        {
        }
    }

    class OpRet : OpBase
    {
        public OpRet() : base(0, "+stack_return_to_saved_address")
        {
        }

        public override object ConvertParameter(CompilerMethodContext context, int parameter) => $".{context.Method.GetLabel()}_ReturnAddress";
    }

    class OpIncVar : OpBase
    {
        private int _varIndex;
        public OpIncVar(int varIndex) : base(0, "+inc_var")
        {
            _varIndex = varIndex;
        }

        public override object ConvertParameter(CompilerMethodContext context, int parameter) => $".{context.Method.GetLabel()}_var{_varIndex}";
    }

    class OpInitVar : OpBase
    {
        private int _varIndex;
        private int _value;
        public OpInitVar(int varIndex, int value) : base(0, "+init_var")
        {
            _varIndex = varIndex;
            _value = value;
        }

        public override object ConvertParameter(CompilerMethodContext context, int parameter) => $".{context.Method.GetLabel()}_var{_varIndex}, {_value}";
    }

    class OpBranchIfVarLess : OpBase
    {
        private int _varIndex;
        private int _value;
        private string _label;
        public OpBranchIfVarLess(int varIndex, int value, string label) : base(0, "+branch_if_var_less")
        {
            _varIndex = varIndex;
            _value = value;
            _label = label;
        }

        public override object ConvertParameter(CompilerMethodContext context, int parameter) => $".{context.Method.GetLabel()}_var{_varIndex}, {_value}, {_label}";
    }

    class OpDeref : OpBase
    {
        private int _varIndex;
        public OpDeref(int varIndex) : base(0, "+deref")
        {
            _varIndex = varIndex;
        }

        public override object ConvertParameter(CompilerMethodContext context, int parameter)
        {
            return $".{context.Method.GetLabel()}_var{_varIndex}";
        }
    }

    class OpDerefParam : OpBase
    {
        private string _parameterName;
        public OpDerefParam(string parameterName) : base(0, "+deref")
        {
            _parameterName = parameterName;
        }

        public override object ConvertParameter(CompilerMethodContext context, int parameter)
        {
            return $".{context.Method.GetLabel()}_{_parameterName}";
        }
    }

    class OpBranchIfNotEqual : OpBase
    {
        private int _varIndex;
        private int _value;
        private string _label;
        public OpBranchIfNotEqual(int varIndex, int value, string label) : base(0, "+branch_if_not_equal")
        {
            _varIndex = varIndex;
            _value = value;
            _label = label;
        }

        public override object ConvertParameter(CompilerMethodContext context, int parameter) => $".{context.Method.GetLabel()}_var{_varIndex}, {_value}, {_label}";
    }
}