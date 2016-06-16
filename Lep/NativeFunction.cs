using System;
using System.Linq;
using System.Reflection;

namespace Lep
{
    public class NativeFunction
    {
        private MethodInfo _method;
        private string _name;
        private int _parametersCount;
        
        public int ParametersCount { get { return _parametersCount; } }

        public NativeFunction(string name, MethodInfo method)
        {
            if (method == null) throw new LepException("internal error: null method", new ArgumentNullException(nameof(method), "null method"));

            _method = method;
            _name = name;

            _parametersCount = method.GetParameters().Count();
        }

        public object Invoke(object[] arguments, IAstNode node)
        {
            try { return _method.Invoke(null, arguments); }
            catch { throw new LepException("bad native function call: " + _name, node); }
        }

        public override string ToString() { return "<native function: " + GetHashCode() + ">"; }
    }
}
