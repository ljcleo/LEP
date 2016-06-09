using System;
using System.Collections.ObjectModel;

namespace Lep
{
    public class SelfChangeNode : AstBranch
    {
        public IAstNode Prefix { get { return this[0]; } }

        public IAstNode Operand { get { return this[1]; } }

        public SelfChangeNode(Collection<IAstNode> children) : base(children) { }

        public override string ToString() { return ((AstLeaf)Prefix).Token.Text + Operand.ToString(); }

        public override object Evaluate(Environment env)
        {
            if (env == null) throw new ArgumentNullException(nameof(env), "null environment");

            if (Operand is FactorNode && ((FactorNode)Operand).IsAssignable)
            {
                string prefix = ((AstLeaf)Prefix).Token.Text, name = ((NameNode)(((PrimaryNode)((FactorNode)Operand).Operand).Operand)).Name;

                object value = env.Get(name, ((FactorNode)Operand).AssignType);

                if (value == null) throw new LepException("undefined name: " + name, this);
                else if (!(value is int)) throw new LepException("bad type for " + prefix, this);
                else
                {
                    int nvalue = (int)value + (prefix == "++" ? 1 : -1);
                    env.Set(name, nvalue, ((FactorNode)Operand).AssignType);

                    return nvalue;
                }
            }
            else throw new LepException("bad self change", this);
        }
    }
}
