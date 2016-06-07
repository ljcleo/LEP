using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Lep
{
    public class PrimaryNode : AstBranch
    {
        public IAstNode Operand { get { return this[0]; } }

        public bool IsName { get { return Operand is NameNode && Count == 1; } }

        public bool IsArrayReference { get { return Operand is NameNode && (from suffix in this where suffix is ArrayReferenceNode select suffix).Count() == Count - 1; } }

        public bool IsAssignable { get { return IsName || IsArrayReference; } }

        public PrimaryNode(Collection<IAstNode> children) : base(children) { }

        public IAstNode Suffix(int layer) { return this[Count - layer - 1]; }

        public bool HasSuffix(int layer) { return Count - layer > 1; }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder("(");

            string sep = "";
            foreach (IAstNode node in this)
            {
                builder.Append(sep);
                sep = " ";

                builder.Append(node.ToString());
            }

            return builder.Append(")").ToString();
        }

        public override object Evaluate(Environment env) { return EvaluateSub(env, 0, Environment.NormalVariable); }

        public object Evaluate(Environment env, int type) { return EvaluateSub(env, 0, type); }

        public object EvaluateSub(Environment env, int layer, int type)
        {
            if (HasSuffix(layer))
            {
                if (layer > 65535) throw new LepException("too much suffix", this);

                object target = EvaluateSub(env, layer + 1, type);

                IAstNode current = Suffix(layer);

                ArgumentNode arg = current as ArgumentNode;
                if (arg != null) return arg.Evaluate(env, target);

                ExpressionArgumentNode exprArg = current as ExpressionArgumentNode;
                if (exprArg != null) return exprArg.Evaluate(env, target);

                ArrayReferenceNode arrRef = current as ArrayReferenceNode;
                if (arrRef != null) return arrRef.Evaluate(env, target);
                
                throw new LepException("bad suffix", this);
            }
            else if (Operand is NameNode) return ((NameNode)Operand).Evaluate(env, type);
            else return Operand.Evaluate(env);
        }
    }
}
