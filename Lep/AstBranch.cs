using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Lep
{
    public class AstBranch : IAstNode
    {
        private List<IAstNode> _children;

        public AstBranch(Collection<IAstNode> children) { _children = new List<IAstNode>(children); }

        public void Add(IAstNode node) { _children.Add(node); }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder("(");

            string sep = "";
            foreach (IAstNode node in _children)
            {
                builder.Append(sep);
                sep = " ";

                builder.Append(node.ToString());
            }

            return builder.Append(")").ToString();
        }

        public IAstNode this[int index] { get { return _children[index]; } }

        public int Count { get { return _children.Count; } }

        public IEnumerator<IAstNode> Children { get { return _children.GetEnumerator(); } }

        public string Location
        {
            get
            {
                foreach (IAstNode node in _children)
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
