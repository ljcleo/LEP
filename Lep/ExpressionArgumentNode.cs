using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Lep
{
    public class ExpressionArgumentNode : AstBranch
    {
        public ExpressionArgumentNode(Collection<IAstNode> children) : base(children) { }

        public override object Evaluate(Environment env) { return this[0].Evaluate(env); }

        public object Evaluate(Environment env, object target)
        {
            UserFunction user = target as UserFunction;
            if (user != null) return EvaluateFunction(env, user);

            NativeFunction native = target as NativeFunction;
            if (native != null) return EvaluateNativeFunction(env, native);

            throw new LepException("bad function", this);
        }

        protected object EvaluateFunction(Environment env, UserFunction function)
        {
            if (function == null) throw new ArgumentNullException("function", "null function");

            ParameterNode parameters = function.Parameters;

            Tuple arguments = Evaluate(env) as Tuple;
            if (arguments == null) throw new LepException("bad expression argument", this);

            if (arguments.Count != parameters.Count) throw new LepException("bad number of argument", this);

            Environment inner = function.CreateEnvironment();
            for (int i = 0; i < arguments.Count; i++) parameters.Evaluate(inner, i, arguments[i]);

            try { return function.Body.Evaluate(inner); }
            catch (JumpSignal signal)
            {
                if (signal.SignalType == JumpSignal.ReturnSignal) return signal.ReturnValue;
                else throw;
            }
        }

        protected object EvaluateNativeFunction(Environment env, NativeFunction function)
        {
            if (function == null) throw new ArgumentNullException("function", "null function");

            int pcount = function.ParametersCount;

            Tuple arguments = Evaluate(env) as Tuple;
            if (arguments == null) throw new LepException("bad expression argument", this);

            if (arguments.Count != pcount) throw new LepException("bad number of argument", this);

            return function.Invoke(arguments.TupleArray, this);
        }
    }
}
