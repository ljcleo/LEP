using System.Collections.ObjectModel;

namespace Lep
{
    public class WhileNode : AstBranch
    {
        public WhileNode(Collection<IAstNode> children) : base(children) { }

        public override string ToString() { return "(while " + this[0].ToString() + ")"; }

        public override object Evaluate(Environment env)
        {
            object result = 0;

            bool flag = true;
            while (flag)
            {
                try
                {
                    GuardValue guard = ((GuardNode)this[0]).Test(env);

                    flag = guard.GuardExpression;
                    if (flag) result = guard.GuardBody;
                }
                catch (JumpSignal signal)
                {
                    switch (signal.SignalType)
                    {
                        case JumpSignal.BreakSignal: return result;
                        case JumpSignal.ContinueSignal: break;
                        default: throw;
                    }
                }
            }

            return result;
        }
    }
}
