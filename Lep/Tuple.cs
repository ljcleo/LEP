using System;
using System.Text;

namespace Lep
{
    public class Tuple
    {
        private object[] _tuple;

        public object this[int index]
        {
            get { return _tuple[index]; }
            set { _tuple[index] = value; }
        }

        public int Count { get { return _tuple.Length; } }

        public Tuple(int count) { _tuple = new object[count]; }

        public Tuple(object[] tuple)
        {
            if (tuple == null) throw new LepException("null initialize array", new ArgumentNullException(nameof(tuple), "null initialize array"));

            _tuple = new object[tuple.Length];
            tuple.CopyTo(_tuple, 0);
        }

        public Tuple(Tuple tuple)
        {
            if (tuple == null) throw new LepException("null initialize array", new ArgumentNullException(nameof(tuple), "null initialize tuple"));

            _tuple = new object[tuple.Count];
            tuple.GetArray().CopyTo(_tuple, 0);
        }

        public object[] GetArray() { return _tuple; }

        public static Tuple Add(object left, Tuple right)
        {
            if (right == null) throw new LepException("null right value", new ArgumentNullException(nameof(right), "null right value"));

            Tuple result = new Tuple(right.Count + 1);
            result[0] = left;
            Array.Copy(right.GetArray(), 0, result.GetArray(), 1, right.Count);

            return result;
        }

        public static Tuple Add(Tuple left, object right)
        {
            if (left == null) throw new LepException("null left value", new ArgumentNullException(nameof(left), "null left value"));

            Tuple result = new Tuple(left.Count + 1);
            left.GetArray().CopyTo(result.GetArray(), 0);
            result[left.Count] = right;

            return result;
        }

        public static Tuple Add(Tuple left, Tuple right)
        {
            if (left == null) throw new LepException("null left value", new ArgumentNullException(nameof(left), "null left value"));
            if (right == null) throw new LepException("null right value", new ArgumentNullException(nameof(right), "null right value"));

            Tuple result = new Tuple(left.Count + right.Count);
            Array.Copy(left.GetArray(), 0, result.GetArray(), 0, left.Count);
            Array.Copy(right.GetArray(), 0, result.GetArray(), left.Count, right.Count);

            return result;
        }

        public static bool operator ==(Tuple left, Tuple right) { return Equals(left, right); }

        public static bool operator !=(Tuple left, Tuple right) { return !Equals(left, right); }

        public static Tuple operator +(object left, Tuple right) { return Add(left, right); }

        public static Tuple operator +(Tuple left, object right) { return Add(left, right); }

        public static Tuple operator +(Tuple left, Tuple right) { return Add(left, right); }

        public override bool Equals(object obj)
        {
            Tuple tuple = obj as Tuple;
            return obj != null && tuple != null && this == tuple;
        }

        public override int GetHashCode() { return _tuple.GetHashCode(); }

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
