using System.Collections.ObjectModel;
using System.Text;

namespace Lep
{
    public class GuardListNode : AstBranch
    {
        public GuardListNode(Collection<IAstNode> children) : base(children) { }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder("(guard ");
            foreach (IAstNode node in this) builder.Append(node.ToString());

            return builder.Append(")").ToString();
        }

        public override object Evaluate(Environment env)
        {
            object result = 0;
            foreach (IAstNode node in this)
            {
                GuardNode guard = (GuardNode)node;

                GuardValue guardResult = guard.Test(env);
                if (guardResult.GuardExpression)
                {
                    result = guardResult.GuardBody;

                    if (((AstLeaf)((BlockNode)guard.Body).Ending).Token.Text == ".") break;
                }
            }

            return result;
        }
    }
}
