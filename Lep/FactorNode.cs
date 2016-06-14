using System.Collections.ObjectModel;

namespace Lep
{
    public class FactorNode : AstBranch
    {
        public IAstNode Prefix { get { return this[0]; } }

        public IAstNode Operand { get { return this[1]; } }

        public bool IsNoPrefix { get { return Prefix is NullNode; } }

        public FactorNode(Collection<IAstNode> children) : base(children) { }

        public override string ToString() { return Prefix is NullNode ? Operand.ToString() : ((AstLeaf)Prefix).Token.Text + Operand.ToString(); }

        public override object Evaluate(Environment env)
        {
            object value = Operand.Evaluate(env);

            if (Prefix is NullNode) return value;
            else
            {
                string prefix = ((AstLeaf)Prefix).Token.Text;
                if (value is int)
                {
                    int tmp = (int)value;

                    switch (prefix)
                    {
                        case "+": return tmp;
                        case "-": return -tmp;
                        case "!": return tmp == 0 ? 1 : 0;
                        case "~": return ~tmp;
                        default: throw new LepException("bad prefix: " + prefix, this);
                    }
                }
                else throw new LepException("bad type for " + prefix, this);
            }
        }
    }
}
