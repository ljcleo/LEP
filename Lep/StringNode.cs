namespace Lep
{
    public class StringNode : AstLeaf
    {
        public string String { get { return Token.Text; } }

        public StringNode(Token token) : base(token) { }

        public override object Evaluate(Environment env) { return String; }
    }
}
