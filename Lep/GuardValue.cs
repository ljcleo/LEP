namespace Lep
{
    public class GuardValue
    {
        private bool m_GuardExpression;
        private object m_GuardBody;

        public bool GuardExpression { get { return m_GuardExpression; } }

        public object GuardBody { get { return m_GuardBody; } }

        public GuardValue(bool expression, object body)
        {
            m_GuardExpression = expression;
            m_GuardBody = body;
        }
    }
}
