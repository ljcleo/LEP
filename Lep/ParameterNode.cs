using System;
using System.Collections.ObjectModel;

namespace Lep
{
    public class ParameterNode : TupleNode
    {
        public ParameterNode(Collection<IAstNode> children) : base(children) { }

        public ParameterNode(TupleNode tuple) : base(new Collection<IAstNode>())
        {
            if (tuple == null) throw new ArgumentNullException(nameof(tuple), "null tuple");
            foreach (IAstNode node in tuple) Add(node);
        }

        public IAstNode Parameter(int index) { return this[index]; }

        public void Evaluate(Environment env, int index, object value)
        {
            if (env == null) throw new ArgumentNullException(nameof(env), "null environment");

            IAstNode current = Parameter(index);

            FactorNode factor = current as FactorNode;
            if (factor == null || !factor.IsNoPrefixPrimary) throw new LepException("bad parameter");

            PrimaryNode primary = factor.Operand as PrimaryNode;
            if (primary == null || !primary.IsName) throw new LepException("bad parameter");


            NameNode name = primary.Operand as NameNode;
            if (name == null) throw new LepException("bad parameter");

            if (name.IsAnonymous) return;
            env.Set(name.Token.Text, value, Environment.LocalVariable);
        }
    }
}
