using System.Collections.ObjectModel;

namespace Lep
{
    public class ControlNode : AstBranch
    {
        public IAstNode Control { get { return this[0]; } }

        public ControlNode(Collection<IAstNode> children) : base(children) { }

        public override string ToString() { return Control.ToString(); }

        public override object Evaluate(Environment env)
        {
            int type;
            switch (((AstLeaf)Control).Token.Text)
            {
                case "!":
                    type = JumpSignal.BreakSignal;
                    break;
                case ":!":
                    type = JumpSignal.ContinueSignal;
                    break;
                case "!!":
                    type = JumpSignal.ReturnSignal;
                    break;
                default:
                    throw new LepException("bad control statement", this);
            }

            throw new JumpSignal(type, null);
        }
    }
}
