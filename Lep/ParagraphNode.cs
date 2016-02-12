using System.Collections.ObjectModel;

namespace Lep
{
    public class ParagraphNode : AstBranch
    {
        public ParagraphNode(Collection<IAstNode> children) : base(children) { }

        public override object Evaluate(Environment env)
        {
            object result = 0;
            foreach (IAstNode node in this) if (!(node is NullNode))
            {
                try { result = node.Evaluate(env); }
                catch (JumpSignal signal)
                {
                    if ((signal.SignalType == JumpSignal.BreakSignal || signal.SignalType == JumpSignal.ReturnSignal) && signal.ReturnValue == null) throw new JumpSignal(signal.SignalType, result);
                    else throw;
                }
            }

            return result;
        }
    }
}
