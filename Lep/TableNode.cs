using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Lep
{
    public class TableNode : AstBranch
    {
        public TableNode(Collection<IAstNode> children) : base(children) { }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder("[");

            string sep = "";
            foreach (IAstNode node in this)
            {
                builder.Append(sep);
                sep = " ";

                builder.Append(node.ToString());
            }

            return builder.Append("]").ToString();
        }

        public override object Evaluate(Environment env)
        {
            Dictionary<object, object> table = new Dictionary<object, object>();

            foreach (IAstNode node in this)
            {
                Tuple pair = node.Evaluate(env) as Tuple;
                if (pair == null || pair.Count != 2) throw new LepException("bad table", this);

                table.Add(pair[0], pair[1]);
            }

            return table;
        }
    }
}
