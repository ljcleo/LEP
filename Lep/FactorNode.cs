using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lep
{
    public class FactorNode : AstBranch
    {
        private static readonly HashSet<string> _assignablePrefix = new HashSet<string>() { "@", "$", "~" };

        public IAstNode Prefix { get { return this[0]; } }

        public IAstNode Operand { get { return this[1]; } }

        public bool IsNoPrefixPrimary { get { return Prefix is NullNode && Operand is PrimaryNode; } }

        public bool IsLocalPrimary { get { return Prefix is AstLeaf && ((AstLeaf)Prefix).Token.Text == "@" && Operand is PrimaryNode; } }

        public bool IsGlobalPrimary { get { return Prefix is AstLeaf && ((AstLeaf)Prefix).Token.Text == "$" && Operand is PrimaryNode; } }

        public bool IsOuterPrimary { get { return Prefix is AstLeaf && ((AstLeaf)Prefix).Token.Text == "~" && Operand is PrimaryNode; } }

        public bool IsDirectPrimary { get { return (Prefix is NullNode ||  Prefix is AstLeaf && _assignablePrefix.Contains(((AstLeaf)Prefix).Token.Text)) && Operand is PrimaryNode; } }

        public bool IsAssignable { get { return IsDirectPrimary && ((PrimaryNode)Operand).IsName; } }

        public int AssignType
        {
            get
            {
                if (IsNoPrefixPrimary) return Environment.NormalVariable;
                else if (IsLocalPrimary) return Environment.LocalVariable;
                else if (IsOuterPrimary) return Environment.OuterVariable;
                else if (IsGlobalPrimary) return Environment.GlobalVariable;
                else throw new LepException("not assignable", this);
            }
        }

        public FactorNode(Collection<IAstNode> children) : base(children) { }

        public override string ToString() { return Prefix is NullNode ? Operand.ToString() : ((AstLeaf)Prefix).Token.Text + Operand.ToString(); }

        public override object Evaluate(Environment env)
        {
            object value;

            if (Operand is PrimaryNode) value = ((PrimaryNode)Operand).Evaluate(env, AssignType);
            else value = Operand.Evaluate(env);

            if (Prefix is NullNode || IsDirectPrimary) return value;
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
                        default: throw new LepException("bad prefix: " + prefix, this);
                    }
                }
                else throw new LepException("bad type for " + prefix, this);
            }
        }
    }
}
