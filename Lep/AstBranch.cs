using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Lep
{
    public class AstBranch : IAstNode
    {
        private List<IAstNode> m_Children;

        public AstBranch(Collection<IAstNode> children) { m_Children = new List<IAstNode>(children); }

        public void Add(IAstNode node) { m_Children.Add(node); }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder("(");

            string sep = "";
            foreach (IAstNode node in m_Children)
            {
                builder.Append(sep);
                sep = " ";

                builder.Append(node.ToString());
            }

            return builder.Append(")").ToString();
        }

        public IAstNode this[int index] { get { return m_Children[index]; } }

        public int Count { get { return m_Children.Count; } }

        public IEnumerator<IAstNode> Children { get { return m_Children.GetEnumerator(); } }

        public string Location
        {
            get
            {
                foreach (IAstNode node in m_Children)
                {
                    string location = node.Location;
                    if (location != null) return location;
                }

                return null;
            }
        }

        public IEnumerator<IAstNode> GetEnumerator() { return Children; }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return Children; }

        public virtual object Evaluate(Environment env) { throw new LepException("cannot evaluate: " + ToString(), this); }
    }
}
