using System;
using System.Text;

namespace Lep
{
    public class Tuple
    {
        private object[] _tuple;

        public object this[int n]
        {
            get { return _tuple[n]; }
            set { _tuple[n] = value; }
        }

        public object[] TupleArray { get { return _tuple; } }

        public int Count { get { return _tuple.Length; } }

        public Tuple(int count) { _tuple = new object[count]; }

        public Tuple(object[] tuple)
        {
            _tuple = new object[tuple.Length];
            tuple.CopyTo(_tuple, 0);
        }

        public Tuple(Tuple tuple)
        {
            _tuple = new object[tuple.Count];
            tuple.TupleArray.CopyTo(_tuple, 0);
        }

        public static Tuple operator +(object l, Tuple r)
        {
            Tuple result = new Tuple(r.Count + 1);
            result[0] = l;
            Array.Copy(r.TupleArray, 0, result.TupleArray, 1, r.Count);

            return result;
        }

        public static Tuple operator +(Tuple l, object r)
        {
            Tuple result = new Tuple(l.Count + 1);
            l.TupleArray.CopyTo(result.TupleArray, 0);
            result[l.Count] = r;

            return result;
        }

        public static Tuple operator +(Tuple l, Tuple r)
        {
            Tuple result = new Tuple(l.Count + r.Count);
            Array.Copy(l.TupleArray, 0, result.TupleArray, 0, l.Count);
            Array.Copy(r.TupleArray, 0, result.TupleArray, l.Count, r.Count);

            return result;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder("{");

            string sep = "";
            foreach (object obj in _tuple)
            {
                builder.Append(sep);
                sep = " ";

                builder.Append(obj.ToString());
            }

            return builder.Append("}").ToString();
        }
    }
}
