using System.Collections.ObjectModel;

namespace Lep
{
    class ExpressionFunctionNode : AstBranch
    {
        public IAstNode Parameters { get { return this[0]; } }

        public IAstNode Body { get { return this[1]; } }

        public ExpressionFunctionNode(Collection<IAstNode> children) : base(children) { }

        public override string ToString() { return "(function " + Parameters + " " + Body + ")"; }

        public override object Evaluate(Environment env)
        {
            if (env == null) throw new LepException("internal error: null environment", this);
            return new UserFunction((ParameterNode)Parameters, (BlockNode)Body, env);
        }
    }
}
