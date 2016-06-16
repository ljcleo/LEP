using System.Collections.Generic;
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
                if (index is int && (int)index < arr.Length) return arr[(int)index];
            }

            Dictionary<object, object> table = target as Dictionary<object, object>;

            if (table != null)
            {
                object value;
                table.TryGetValue(Index.Evaluate(env), out value);
                if (value != null) return value;
            }

            throw new LepException("bad array access", this);
        }

        public override string ToString() { return "[" + Index + "]"; }
    }
}
