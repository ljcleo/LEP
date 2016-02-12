using System;
using System.Collections.ObjectModel;

namespace Lep
{
    public class FunctionDefinitionNode : AstBranch
    {
        public IAstNode Name { get { return this[0]; } }

        public IAstNode Parameters { get { return this[1]; } }

        public IAstNode Body { get { return this[2]; } }

        public FunctionDefinitionNode(Collection<IAstNode> children) : base(children) { }

        public override string ToString() { return "(define function " + Name + " " + Parameters + " " + Body + ")"; }

        public override object Evaluate(Environment env)
        {
            if (env == null) throw new ArgumentNullException("env", "null environment");

            env.Set(Name.ToString(), new UserFunction((ParameterNode)Parameters, (BlockNode)Body, env), Environment.LocalVariable);
            return Name.ToString();
        }
    }
}
