using System.Collections.ObjectModel;

namespace Lep
{
    public class BlockNode : AstBranch
    {
        public IAstNode Body { get { return this[0]; } }

        public IAstNode Ending { get { return this[1]; } }

        public BlockNode(Collection<IAstNode> children) : base(children) { }

        public override string ToString() { return "(" + Body.ToString() + (((AstLeaf)Ending).Token.Text == "." ? " TM)" : " CT)"); }

        public override object Evaluate(Environment env) { return Body.Evaluate(new Environment(env)); }
    }
}
