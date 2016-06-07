using System.Collections.ObjectModel;

namespace Lep
{
    public class ArrayNode : AstBranch
    {
        public IAstNode ArrayLength { get { return this[0]; } }

        public ArrayNode(Collection<IAstNode> children) : base(children) { }

        public override object Evaluate(Environment env)
        {
            int total;

            if (ArrayLength is NullNode) total = Count - 1;
            else
            {
                object value = ArrayLength.Evaluate(env);

                if (value is int) total = (int)value;
                else throw new LepException("bad array length", this);
            }

            object[] array = new object[total];

            for (int i = 0; i < total; i++)
            {
                if (i >= Count - 1) array[i] = 0;
                else array[i] = this[i + 1].Evaluate(env);
            }

            return array;
        }
    }
}
