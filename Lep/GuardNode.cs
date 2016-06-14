using System.Collections.ObjectModel;

namespace Lep
{
    public class GuardNode : AstBranch
    {
        public IAstNode Condition { get { return this[0]; } }

        public IAstNode Body { get { return this[1]; } }

        public GuardNode(Collection<IAstNode> children) : base(children) { }

        public override string ToString() { return Condition.ToString() + " -> " + Body.ToString(); }

        public override object Evaluate(Environment env) { return Body.Evaluate(env); }
        
        public GuardValue Test(Environment env) { return Test(env, null); }
        
        public GuardValue Test(Environment env, object prefix)
        {
            Environment inner = new Environment(env);

            if (Condition is NullNode) return new GuardValue(true, Evaluate(inner));

            object guard = Condition.Evaluate(inner);
            if (prefix != null && prefix.Equals(guard) || prefix == null && IsTrue(guard)) return new GuardValue(true, Evaluate(inner));
            else return new GuardValue(false, 0);
        }

        private static bool IsTrue(object guard)
        {
            if (guard is int) return (int)guard != 0;

            string str = guard as string;
            if (str != null) return !string.IsNullOrEmpty(str);

            Tuple tuple = guard as Tuple;
            if (tuple != null) return tuple.Count != 0;

            object[] array = guard as object[];
            if (array != null) return array.Length != 0;

            return true;
        }
    }
}
