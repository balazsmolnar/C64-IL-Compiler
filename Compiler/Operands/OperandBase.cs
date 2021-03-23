using System.Runtime.InteropServices;
using System.Reflection;
using System.Linq;
using Compiler;
using System.Collections.Generic;

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

        public virtual string Command { get; }
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
        public OpLdstr() : base(4, "#stack_push_pointer")
        {
        }

        public override object ConvertParameter(CompilerMethodContext context, int parameter)
        {
            if (!context.CompilerContext.StringValues.ContainsKey(parameter))
                context.CompilerContext.StringValues.Add(parameter, context.CompilerContext.Assembly.ManifestModule.ResolveString(parameter));
            return $"string_{parameter}";
        }
    }

    class OpLdftn : OpBase
    {
        public OpLdftn() : base(4, "#stack_push_pointer")
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
        public OpNewObj() : base(4, "#newObj")
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
        public OpNewArr() : base(4, "#newArr")
        {
        }

        public override object ConvertParameter(CompilerMethodContext context, int parameter)
        {
            return "";
        }
    }

    class OpStfld : OpBase
    {
        public OpStfld() : base(4, "#stfld")
        {
        }

        public override object ConvertParameter(CompilerMethodContext context, int parameter)
        {
            var field = context.CompilerContext.Assembly.ManifestModule.ResolveField(parameter);
            var pos = context.CompilerContext.GetFieldPosition(field); return $"{pos}";
        }
    }

    class OpStElem_ref : OpBase
    {
        public OpStElem_ref() : base(0, "#stelemRef")
        {
        }
    }

    class OpLdfld : OpBase
    {
        public OpLdfld() : base(4, "#ldfld")
        {
        }

        public override object ConvertParameter(CompilerMethodContext context, int parameter)
        {
            var field = context.CompilerContext.Assembly.ManifestModule.ResolveField(parameter);
            var pos = context.CompilerContext.GetFieldPosition(field);
            return $"{pos}";
        }
    }

    class OpIncfld : OpBase
    {
        private readonly string thisVar;
        private readonly string pos;

        public OpIncfld(string thisVar, string pos) : base(0, "#incfld")
        {
            this.thisVar = thisVar;
            this.pos = pos;
        }

        public override object ConvertParameter(CompilerMethodContext context, int parameter)
        {
            return $"{pos}";
        }
    }

    class OpSetfld : OpBase
    {
        private readonly string pos;
        private readonly string objRelPos;
        private readonly string valueRelPos;

        public OpSetfld(string objRelPos, string valueRelPos, string pos) : base(0, "#setfld")
        {
            this.objRelPos = objRelPos;
            this.valueRelPos = valueRelPos;
            this.pos = pos;
        }

        public override object ConvertParameter(CompilerMethodContext context, int parameter)
        {
            return $"{objRelPos}, {valueRelPos}, {pos}";
        }
    }

    class OpPushFld : OpBase
    {
        private readonly string thisVar;
        private readonly string pos;

        public OpPushFld(string thisVar, string pos) : base(0, "#pushfld")
        {
            this.thisVar = thisVar;
            this.pos = pos;
        }

        public override object ConvertParameter(CompilerMethodContext context, int parameter)
        {
            return $"{pos}";
        }
    }

    class OpDup : OpBase
    {
        public OpDup() : base(0, "#stack_duplicate")
        {

        }
    }

    class OpPop : OpBase
    {
        public OpPop() : base(0, "#stack_pop")
        {

        }
    }

    class OpPushBase : OpBase
    {
        public OpPushBase(int parameterSize) : base(parameterSize, "#stack_push_int")
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
        public OpLdloc(int varIndex) : base(0, "#locals_push_value_8")
        {
            VarIndex = varIndex;
        }

        public override object ConvertParameter(CompilerMethodContext context, int parameter)
        {

            int relPos = context.GetLocalVariableReferencePosition(VarIndex);
            return $"{relPos}";
        }
    }

    class OpLdloc_s : OpPushBase
    {
        public OpLdloc_s() : base(1, "#locals_push_value_8")
        {
        }

        public override object ConvertParameter(CompilerMethodContext context, int parameter)
        {

            int relPos = context.GetLocalVariableReferencePosition(parameter);
            return $"{relPos}";
        }
    }

    class OpLdarg : OpPushBase
    {
        private int _argIndex;
        public OpLdarg(int argIndex) : base(0, "#locals_push_value_8")
        {
            _argIndex = argIndex;
        }

        public override object ConvertParameter(CompilerMethodContext context, int parameter)
        {
            int relPos = context.GetParameterReferencePosition(_argIndex);
            return $"{relPos}";
        }
        public override string Emit(CompilerMethodContext context, object parameter)
        {
            int size = context.GetParameterSize(_argIndex);
            var command = size == 2 ? "#locals_push_value_16" : "#locals_push_value_8";
            return $"{command} {parameter}";
        }
    }

    class OpLdsld : OpPushBase
    {
        public OpLdsld() : base(4, "#stack_push_var")
        {
        }

        public override object ConvertParameter(CompilerMethodContext context, int parameter)
        {
            var field = context.Method.DeclaringType.Module.ResolveField(parameter);
            return $"{field.DeclaringType.Name.ToValidName()}_field_{field.Name.ToValidName()}";
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
        public OpPullBase(int parameterSize) : base(parameterSize, "#stack_pull_int")
        {

        }
    }

    class OpStloc : OpBase
    {
        public int VarIndex { get; }
        public OpStloc(int varIndex) : base(0, "#locals_pull_value_8")
        {
            VarIndex = varIndex;
        }

        public override object ConvertParameter(CompilerMethodContext context, int parameter)
        {
            var body = context.Method.GetMethodBody();
            var variables = body.LocalVariables;
            var index = context.GetLocalVariableReferencePosition(VarIndex);
            var isRef = variables[VarIndex].LocalType.IsReferenceCounted() ? "1" : "0";
            return $"{index}, {isRef}";
        }
    }

    class OpStsfld : OpBase
    {
        public OpStsfld() : base(4, "#stack_pull_int_ref")
        {
        }
        public override object ConvertParameter(CompilerMethodContext context, int parameter)
        {
            var field = context.Method.DeclaringType.Module.ResolveField(parameter);
            var address = $"{field.DeclaringType.Name.ToValidName()}_field_{field.Name.ToValidName()}";
            string isRef = field.FieldType.IsReferenceCounted() ? "1" : "0";
            return $"{address}, {isRef}";
        }
    }

    class OpStloc_s : OpBase
    {
        public OpStloc_s() : base(1, "#locals_pull_value_8")
        {
        }

        public override object ConvertParameter(CompilerMethodContext context, int parameter)
        {
            var body = context.Method.GetMethodBody();
            var variables = body.LocalVariables;
            var index = context.GetLocalVariableReferencePosition(parameter);
            var isRef = variables[parameter].LocalType.IsReferenceCounted() ? "1" : "0";
            return $"{index}, {isRef}";
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
        public OpRet() : base(0, "#method_exit")
        {
        }

        public override object ConvertParameter(CompilerMethodContext context, int parameter)
        {
            List<string> refList = new List<string>();
            bool isInstance = !context.Method.IsStatic;

            for (int i = 0; i < context.Method.GetParameters().Length; i++)
            {
                if (context.Method.GetParameters()[i].ParameterType.IsReferenceCounted())
                    refList.Add(context.GetParameterReferencePosition(i + (isInstance ? 1 : 0)).ToString());
            }

            var body = context.Method.GetMethodBody();
            var variables = body.LocalVariables;
            for (int i = 0; i < variables.Count; i++)
            {
                if (variables[i].LocalType.IsReferenceCounted())
                    refList.Add(context.GetLocalVariableReferencePosition(i).ToString());
            }

            return $"{context.GetLocalStackSize()}, [{string.Join(',', refList)}]";
        }
    }

    class OpIncVar : OpBase
    {
        private int _varIndex;
        public OpIncVar(int varIndex) : base(0, "#inc_var")
        {
            _varIndex = varIndex;
        }

        public override object ConvertParameter(CompilerMethodContext context, int parameter)
        {
            var refPos = context.GetLocalVariableReferencePosition(_varIndex);
            return $"{refPos}";
        }
    }

    class OpInitVar : OpBase
    {
        private int _varIndex;
        private int _value;
        public OpInitVar(int varIndex, int value) : base(0, "#init_var")
        {
            _varIndex = varIndex;
            _value = value;
        }

        public override object ConvertParameter(CompilerMethodContext context, int parameter)
        {
            var refPos = context.GetLocalVariableReferencePosition(_varIndex);
            return $"{refPos}, {_value}";
        }
    }

    class OpBranchIfVarLess : OpBase
    {
        private int _varIndex;
        private int _value;
        private string _label;
        public OpBranchIfVarLess(int varIndex, int value, string label) : base(0, "#branch_if_var_less")
        {
            _varIndex = varIndex;
            _value = value;
            _label = label;
        }

        public override object ConvertParameter(CompilerMethodContext context, int parameter)
        {
            var refPos = context.GetLocalVariableReferencePosition(_varIndex);
            return $"{refPos}, {_value}, {_label}";
        }
    }

    class OpDeref : OpBase
    {
        private int _varIndex;
        public OpDeref(int varIndex) : base(0, "#deref")
        {
            _varIndex = varIndex;
        }

        public override object ConvertParameter(CompilerMethodContext context, int parameter)
        {
            return $"{context.Method.GetLabel()}_var{_varIndex}";
        }
    }

    class OpDerefParam : OpBase
    {
        private string _parameterName;
        public OpDerefParam(string parameterName) : base(0, "#deref")
        {
            _parameterName = parameterName;
        }

        public override object ConvertParameter(CompilerMethodContext context, int parameter)
        {
            return $"{context.Method.GetLabel()}_{_parameterName}";
        }
    }

    class OpBranchIfNotEqual : OpBase
    {
        private int _varIndex;
        private int _value;
        private string _label;
        public OpBranchIfNotEqual(int varIndex, int value, string label) : base(0, "#branch_if_not_equal")
        {
            _varIndex = varIndex;
            _value = value;
            _label = label;
        }

        public override object ConvertParameter(CompilerMethodContext context, int parameter)
        {
            var refPos = context.GetLocalVariableReferencePosition(_varIndex);
            return $"{refPos}, {_value}, {_label}";
        }
    }

    class OpLoadPointerFromLabel : OpBase
    {
        public OpLoadPointerFromLabel() : base(4, "#stack_push_pointer")
        {
        }

    }

}