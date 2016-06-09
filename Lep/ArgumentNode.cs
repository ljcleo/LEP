using System;
using System.Collections.ObjectModel;

namespace Lep
{
    public class ArgumentNode : TupleNode
    {
        public ArgumentNode(Collection<IAstNode> children) : base(children) { }

        public ArgumentNode(TupleNode tuple) : base(new Collection<IAstNode>())
        {
            if (tuple == null) throw new ArgumentNullException(nameof(tuple), "null tuple");
            foreach (IAstNode node in tuple) Add(node);
        }

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
            if (function == null) throw new ArgumentNullException(nameof(function), "null function");

            ParameterNode parameters = function.Parameters;
            if (Count != parameters.Count) throw new LepException("bad number of arguments", this);

            Environment inner = function.CreateEnvironment();

            int count = 0;
            foreach (IAstNode node in this) parameters.Evaluate(inner, count++, node.Evaluate(env));

            try { return function.Body.Evaluate(inner); }
            catch (JumpSignal signal)
            {
                if (signal.SignalType == JumpSignal.ReturnSignal) return signal.ReturnValue;
                else throw;
            }
        }

        protected object EvaluateNativeFunction(Environment env, NativeFunction function)
        {
            if (function == null) throw new ArgumentNullException(nameof(function), "null function");

            int pcount = function.ParametersCount;
            if (Count != pcount) throw new LepException("bad number of arguments", this);

            object[] arguments = new object[pcount];

            int count = 0;
            foreach (IAstNode node in this) arguments[count++] = node.Evaluate(env);

            return function.Invoke(arguments, this);
        }
    }
}
