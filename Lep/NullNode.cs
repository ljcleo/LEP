using System.Collections.ObjectModel;

namespace Lep
{
    public class NullNode : AstBranch
    {
        public NullNode(Collection<IAstNode> children) : base(children) { }
    }
}
