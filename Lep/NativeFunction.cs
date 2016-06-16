using System;
using System.Linq;
using System.Reflection;

namespace Lep
{
    public class NativeFunction
    {
        private MethodInfo m_Method;
        private string m_Name;
        private int m_ParametersCount;
        
        public int ParametersCount { get { return m_ParametersCount; } }

        public NativeFunction(string name, MethodInfo method)
        {
            if (method == null) throw new LepException("internal error: null method", new ArgumentNullException(nameof(method), "null method"));

            m_Method = method;
            m_Name = name;

            m_ParametersCount = method.GetParameters().Count();
        }

        public object Invoke(object[] arguments, IAstNode node)
        {
            try { return m_Method.Invoke(null, arguments); }
            catch { throw new LepException("bad native function call: " + m_Name, node); }
        }

        public override string ToString() { return "<native function: " + GetHashCode() + ">"; }
    }
}
