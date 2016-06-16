namespace Lep
{
    public class GuardValue
    {
        private bool _guardExpression;
        private object _guardBody;

        public bool GuardExpression { get { return _guardExpression; } }

        public object GuardBody { get { return _guardBody; } }

        public GuardValue(bool expression, object body)
        {
            _guardExpression = expression;
            _guardBody = body;
        }
    }
}
