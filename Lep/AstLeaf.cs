using System.Collections.Generic;

namespace Lep
{
    public class AstLeaf : IAstNode
    {
        private static readonly List<IAstNode> Empty = new List<IAstNode>();

        private Token _token;

        public Token Token { get { return _token; } }

        public AstLeaf(Token token) { _token = token; }

        public override string ToString() { return _token.Text; }

        public IAstNode this[int index] { get { return Empty[index]; } }

        public int Count { get { return 0; } }

        public IEnumerator<IAstNode> Children { get { return Empty.GetEnumerator(); } }

        public string Location { get { return "at line " + _token.Line; } }

        public IEnumerator<IAstNode> GetEnumerator() { return Children; }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return Children; }

        public virtual object Evaluate(Environment env)  {  throw new LepException("cannot evaluate: " + ToString(), this); }
    }
}
