using Compiler;

namespace Compiler.Ops
{
    internal class OpBase
    {
        public OpBase(int parameterSize)
        {
            ParameterSize = parameterSize;
        }

        public OpBase(int parameterSize, string command):this(parameterSize)
        {
            Command = command;
        }

        public string Command { get; }
        public int ParameterSize { get; }

        public virtual string Emit(CompilerMethodContext context, object parameter)
        {
            if (parameter==null)
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

    class OpPushBase : OpBase
    {
        public OpPushBase(int parameterSize): base(parameterSize, "+stack_push_int")
        {
            
        }

        public OpPushBase(int parameterSize, string command): base(parameterSize, command) {}
    }

    class OpLdc_i4_const : OpPushBase
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

    class OpLdc_i4 : OpPushBase
    {
        public OpLdc_i4() : base(4)
        {           
        }

        public override object ConvertParameter(CompilerMethodContext context, int parameter)
        {
            return parameter;
        }
    }

    class OpLdc_i4_s : OpPushBase
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
        private int _varIndex;
        public OpLdloc(int varIndex) : base(0, "+stack_push_var")
        {           
            _varIndex = varIndex;
        }

        public override object ConvertParameter(CompilerMethodContext context, int parameter) => $".{context.Method.GetLabel()}_var{_varIndex}";
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

        public override object ConvertParameter(CompilerMethodContext context, int parameter) => $".{context.Method.GetLabel()}_{context.Method.GetParameters()[_argIndex].Name}";
    }

    class OpPullBase : OpBase
    {
        public OpPullBase(int parameterSize): base(parameterSize, "+stack_pull_int")
        {
            
        }
    }

    class OpStloc : OpPullBase
    {
        private int _varIndex;
        public OpStloc(int varIndex) : base(0)
        {           
            _varIndex = varIndex;
        }

        public override object ConvertParameter(CompilerMethodContext context, int parameter) => $".{context.Method.GetLabel()}_var{_varIndex}";
    }

    class OpStloc_s : OpPullBase
    {
        public OpStloc_s() : base(1)
        {           
        }

        public override object ConvertParameter(CompilerMethodContext context, int parameter) => $".{context.Method.GetLabel()}_var{parameter}";
    }

    class OpShortJump : OpBase
    {
        public OpShortJump(string command): base(1, command)
        {            
        }

        public override object ConvertParameter(CompilerMethodContext context, int parameter) => (parameter < 128 ? (int)parameter : (int)parameter-256) + 2;
    }

    class OpLongJump : OpBase
    {
        public OpLongJump(string command): base(4, command)
        {            
        }

        public override object ConvertParameter(CompilerMethodContext context, int parameter) =>  parameter + 5;
    }

    class OpArithmetic2 : OpBase
    {
        public OpArithmetic2(string command): base(0, command)
        {            
        }
    }

    class OpArithmetic1 : OpBase
    {
        public OpArithmetic1(string command): base(0, command)
        {            
        }
    }

    class OpRet : OpBase
    {
        public OpRet(): base(0, "+stack_return_to_saved_address")
        {            
        }

        public override object ConvertParameter(CompilerMethodContext context, int parameter) => $".{context.Method.GetLabel()}_ReturnAddress";
    }

}