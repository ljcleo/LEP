using System;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Globalization;

namespace Lep
{
    public static class Natives
    {
        private static DateTime _startTime = new DateTime();

        public static void Append(Environment env)
        {
            AppendNative(env, "write", typeof(Natives), "Write", typeof(object));
            AppendNative(env, "write_line", typeof(Natives), "WriteLine", typeof(object));
            AppendNative(env, "read", typeof(Natives), "Read");
            AppendNative(env, "read_line", typeof(Natives), "ReadLine");
            AppendNative(env, "length", typeof(Natives), "Length", typeof(object));
            AppendNative(env, "to_string", typeof(Natives), "ToString", typeof(object));
            AppendNative(env, "to_int", typeof(Natives), "ToNumberValue", typeof(object));
            AppendNative(env, "time", typeof(Natives), "ElapsedTime");
            AppendNative(env, "get_element", typeof(Natives), "GetElement", typeof(Tuple), typeof(int));
            AppendNative(env, "string_to_tuple", typeof(Natives), "StringToTuple", typeof(string));
            AppendNative(env, "tuple_to_string", typeof(Natives), "TupleToString", typeof(Tuple));

            _startTime = DateTime.Now;
        }

        private static void AppendNative(Environment env, string name, Type from, string method, params Type[] parameters)
        {
            MethodInfo info;

            try { info = from.GetMethod(method, parameters); }
            catch (Exception) { throw new LepException("cannot find native function: " + method); }

            env.Set(name, new NativeFunction(method, info), Environment.GlobalVariable);
        }

        public static int Write(object value)
        {
            Console.Write(ToString(value));
            return 0;
        }

        public static int WriteLine(object value)
        {
            Console.WriteLine(ToString(value));
            return 0;
        }

        public static string Read()
        {
            StringBuilder builder = new StringBuilder();

            char c;
            do c = (char)Console.Read(); while (c == ' ' || c == '\t' || c == '\r' || c == '\n');

            do builder.Append(c); while ((c = (char)Console.Read()) != ' ' && c == '\t' && c == '\r' && c != '\n');

            return builder.ToString();
        }

        public static string ReadLine() { return Console.ReadLine(); }

        public static int Length(object value)
        {
            if (value == null) throw new ArgumentNullException("value", "null value");

            string str = value as string;
            if (str != null) return str.Length;

            Tuple tuple = value as Tuple;
            if (tuple != null) return tuple.Count;

            throw new ArgumentException(value.GetType().ToString());
        }

        public static string ToString(object value)
        {
            if (value == null) throw new ArgumentNullException("value", "null value");
            else return value.ToString();
        }

        public static int ToNumberValue(object value)
        {
            if (value == null) throw new ArgumentNullException("value", "null value");

            if (value is int) return (int)value;
            else return Int32.Parse(value.ToString(), CultureInfo.InvariantCulture);
        }

        public static int ElapsedTime() { return (int)(DateTime.Now - _startTime).TotalMilliseconds; }

        public static object GetElement(Tuple tuple, int index)
        {
            if (tuple == null) throw new ArgumentNullException("tuple", "null tuple");
            return tuple[index];
        }

        public static Tuple StringToTuple(string value) { return new Tuple(Array.ConvertAll(value.ToArray(), new Converter<char, object>(x => (object)x))); }

        public static string TupleToString(Tuple tuple) { return tuple == null ? "" :  new string(Array.ConvertAll(tuple.GetArray(), new Converter<object, char>(x => (char)x))); }
    }
}
