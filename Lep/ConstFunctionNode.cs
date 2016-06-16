using System.Collections.ObjectModel;

namespace Lep
{
    public class ConstFunctionNode : AstBranch
    {
        public IAstNode Name { get { return this[0]; } }

        public IAstNode Parameters { get { return this[1]; } }

        public IAstNode Body { get { return this[2]; } }

        public ConstFunctionNode(Collection<IAstNode> children) : base(children) { }

        public override string ToString() { return "(define function " + Name + " " + Parameters + " " + Body + ")"; }

        public override object Evaluate(Environment env)
        {
            if (env == null) throw new LepException("internal error: null environment", this);

            env.Set(Name.ToString(), new UserFunction((ParameterNode)Parameters, (BlockNode)Body, env), Environment.Constant);
            return Name.ToString();
        }
    }
}
