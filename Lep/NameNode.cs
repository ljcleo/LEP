using System;

namespace Lep
{
    public class NameNode : AstLeaf
    {
        public string Name { get { return Token.Text; } }

        public bool IsAnonymous { get { return Name == "_"; } }

        public NameNode(Token token) : base(token) { }

        public override object Evaluate(Environment env) { return Evaluate(env, Environment.NormalVariable); }

        public object Evaluate(Environment env, int type)
        {
            if (env == null) throw new ArgumentNullException(nameof(env), "null environment");

            object value = env.Get(Name, type);

            if (value == null) throw new LepException("undefined name: " + Name, this);
            else return value;
        }
    }
}
