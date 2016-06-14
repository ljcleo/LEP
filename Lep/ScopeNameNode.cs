using System.Collections.ObjectModel;

namespace Lep
{
    public class ScopeNameNode : AstBranch
    {
        public IAstNode Scope { get { return this[0]; } }

        public IAstNode Name { get { return this[1]; } }

        public int AssignType
        {
            get
            {
                if (Scope is NullNode) return Environment.NormalVariable;
                else
                {
                    switch (((AstLeaf)Scope).Token.Text)
                    {
                        case "@": return Environment.LocalVariable;
                        case "^": return Environment.OuterVariable;
                        case "$": return Environment.GlobalVariable;
                        default: throw new LepException("bad scope", this);
                    }
                }
            }
        }

        public ScopeNameNode(Collection<IAstNode> children) : base(children) { }

        public override object Evaluate(Environment env) { return ((NameNode)Name).Evaluate(env, AssignType); }
    }
}
