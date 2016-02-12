namespace Lep
{
    public class NumberNode : AstLeaf
    {
        public int Number { get { return Token.Number; } }

        public NumberNode(Token token) : base(token) { }

        public override object Evaluate(Environment env) { return Number; }
    }
}
