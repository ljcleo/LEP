using System.Collections.ObjectModel;
using System.Text;

namespace Lep
{
    public class TupleNode : AstBranch
    {
        public TupleNode(Collection<IAstNode> children) : base(children) { }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder("{");

            string sep = "";
            foreach (IAstNode node in this)
            {
                builder.Append(sep);
                sep = " ";

                builder.Append(node.ToString());
            }

            return builder.Append("}").ToString();
        }

        public override object Evaluate(Environment env)
        {
            object[] tuple = new object[Count];

            int count = 0;
            foreach (IAstNode node in this) tuple[count++] = node.Evaluate(env);

            return tuple;
        }
    }
}
