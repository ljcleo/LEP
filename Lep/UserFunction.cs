namespace Lep
{
    public class UserFunction
    {
        private ParameterNode _parameters;
        private BlockNode _body;
        private Environment _father;

        public ParameterNode Parameters { get { return _parameters; } }

        public BlockNode Body { get { return _body; } }

        public UserFunction(ParameterNode parameters, BlockNode body, Environment father)
        {
            _parameters = parameters;
            _body = body;
            _father = father;
        }

        public Environment CreateEnvironment() { return new Environment(_father); }

        public override string ToString() { return "<function: " + GetHashCode() + ">"; }
    }
}
