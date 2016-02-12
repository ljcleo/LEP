using System.Collections.ObjectModel;

namespace Lep
{
    public class SwitchNode : AstBranch
    {
        public IAstNode Expression { get { return this[0]; } }

        public IAstNode Guards { get { return this[1]; } }

        public SwitchNode(Collection<IAstNode> children) : base(children) { }

        public override string ToString() { return "(switch " + Expression.ToString() + " : " + Guards.ToString() + ")"; }

        public override object Evaluate(Environment env)
        {
            Environment inner = new Environment(env);

            object expression = Expression.Evaluate(inner), result = 0;
            foreach (IAstNode node in Guards)
            {
                GuardNode guard = (GuardNode)node;

                GuardValue guardResult = (guard).Test(env, expression);
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
