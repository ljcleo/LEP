using System;
using System.Collections.ObjectModel;
using System.Text;

namespace Lep
{
    public class ParameterNode : AstBranch
    {
        public IAstNode Parameter(int index) { return this[index]; }

        public ParameterNode(Collection<IAstNode> children) : base(children) { }

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

        public void Evaluate(Environment env, int index, object value)
        {
            if (env == null) throw new ArgumentNullException(nameof(env), "null environment");

            NameNode name = Parameter(index) as NameNode;
            if (name == null) throw new LepException("bad parameter");

            if (name.IsAnonymous) return;
            env.Set(name.Token.Text, value, Environment.LocalVariable);
        }
    }
}
