using System.Collections.ObjectModel;

namespace Lep
{
    public class ArrayReferenceNode : AstBranch
    {
        public IAstNode Index { get { return this[0]; } }

        public ArrayReferenceNode(Collection<IAstNode> children) : base(children) { }

        public object Evaluate(Environment env, object target)
        {
            object[] arr = target as object[];

            if (arr != null)
            {
                object index = Index.Evaluate(env);
                if (index is int) return arr[(int)index];
            }

            throw new LepException("bad array access", this);
        }

        public override string ToString() { return "[" + Index + "]"; }
    }
}
