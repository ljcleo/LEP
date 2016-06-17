using System;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Globalization;
using System.Collections.Generic;

namespace Lep
{
    public static class Natives
    {
        private static DateTime _startTime = new DateTime();

        public static void Append(Environment env)
        {
            AppendNative(env, "write", typeof(Natives), nameof(Write), typeof(object));
            AppendNative(env, "write_line", typeof(Natives), nameof(WriteLine), typeof(object));
            AppendNative(env, "read", typeof(Natives), nameof(Read));
            AppendNative(env, "read_line", typeof(Natives), nameof(ReadLine));
            AppendNative(env, "length", typeof(Natives), nameof(Length), typeof(object));
            AppendNative(env, "to_string", typeof(Natives), nameof(ToString), typeof(object));
            AppendNative(env, "to_int", typeof(Natives), nameof(ToNumberValue), typeof(object));
            AppendNative(env, "time", typeof(Natives), nameof(ElapsedTime));
            AppendNative(env, "get_element", typeof(Natives), nameof(GetElement), typeof(Tuple), typeof(int));
            AppendNative(env, "string_to_tuple", typeof(Natives), nameof(StringToTuple), typeof(string));
            AppendNative(env, "tuple_to_string", typeof(Natives), nameof(TupleToString), typeof(Tuple));
            AppendNative(env, "string_to_array", typeof(Natives), nameof(StringToArray), typeof(string));
            AppendNative(env, "array_to_string", typeof(Natives), nameof(ArrayToString), typeof(object[]));
            AppendNative(env, "tuple_to_array", typeof(Natives), nameof(TupleToArray), typeof(Tuple));
            AppendNative(env, "array_to_tuple", typeof(Natives), nameof(ArrayToTuple), typeof(object[]));
            AppendNative(env, "table_index_to_array", typeof(Natives), nameof(TableIndexToArray), typeof(Dictionary<object, object>));
            AppendNative(env, "table_element_to_array", typeof(Natives), nameof(TableElementToArray), typeof(Dictionary<object, object>));
            AppendNative(env, "array_to_table", typeof(Natives), nameof(ArrayToTable), typeof(object[]));
            AppendNative(env, "contains_index", typeof(Natives), nameof(ContainsIndex), typeof(Dictionary<object, object>), typeof(object));
            AppendNative(env, "is_int", typeof(Natives), nameof(IsNumber), typeof(object));
            AppendNative(env, "is_string", typeof(Natives), nameof(IsString), typeof(object));
            AppendNative(env, "is_tuple", typeof(Natives), nameof(IsTuple), typeof(object));
            AppendNative(env, "is_array", typeof(Natives), nameof(IsArray), typeof(object));
            AppendNative(env, "is_table", typeof(Natives), nameof(IsTable), typeof(object));

            _startTime = DateTime.Now;
        }

        private static void AppendNative(Environment env, string name, Type from, string method, params Type[] parameters)
        {
            MethodInfo info;

            try { info = from.GetMethod(method, parameters); }
            catch (Exception) { throw new LepException("cannot find native function: " + method); }

            env.Set(name, new NativeFunction(method, info), Environment.Constant);
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
            char[] space = new char[] { ' ', '\t', '\r', '\n', '\u001a' };

            StringBuilder builder = new StringBuilder();

            char c;
            do c = (char)Console.Read(); while (space.Contains(c));

            do builder.Append(c); while (!space.Contains(c = (char)Console.Read()));

            return builder.ToString();
        }

        public static string ReadLine() { return Console.ReadLine(); }

        public static int Length(object value)
        {
            if (value == null) throw new LepException("internal error: null value", new ArgumentNullException(nameof(value), "null value"));

            string str = value as string;
            if (str != null) return str.Length;

            Tuple tuple = value as Tuple;
            if (tuple != null) return tuple.Count;

            object[] array = value as object[];
            if (array != null) return array.Length;

            Dictionary<object, object> table = value as Dictionary<object, object>;
            if (table != null) return table.Count;

            throw new LepException("bad argument type " + value.GetType().ToString(), new ArgumentException(value.GetType().ToString()));
        }

        public static string ToString(object value)
        {
            if (value == null) throw new LepException("null value", new ArgumentNullException(nameof(value), "null value"));
            else return value.ToString();
        }

        public static int ToNumberValue(object value)
        {
            if (value == null) throw new LepException("null value", new ArgumentNullException(nameof(value), "null value"));

            if (value is int) return (int)value;
            else return Lexer.ParseNumber(value.ToString());
        }

        public static int ElapsedTime() { return (int)(DateTime.Now - _startTime).TotalMilliseconds; }

        public static object GetElement(Tuple tuple, int index)
        {
            if (tuple == null) throw new LepException("null tuple", new ArgumentNullException(nameof(tuple), "null tuple"));
            return tuple[index];
        }

        public static Tuple StringToTuple(string value) { return new Tuple(Array.ConvertAll(value.ToArray(), new Converter<char, object>(x => (int)x))); }

        public static string TupleToString(Tuple tuple) { return tuple == null ? "" :  new string(Array.ConvertAll(tuple.GetArray(), new Converter<object, char>(x => (char)x))); }

        public static object[] StringToArray(string value) { return Array.ConvertAll(value.ToArray(), new Converter<char, object>(x => (int)x)); }

        public static string ArrayToString(object[] array) { return array == null ? "" : new string(Array.ConvertAll(array, new Converter<object, char>(x => (char)x))); }

        public static object[] TupleToArray(Tuple tuple)
        {
            if (tuple == null) throw new LepException("null tuple", new ArgumentNullException(nameof(tuple), "null tuple"));
            return tuple.GetArray();
        }

        public static Tuple ArrayToTuple(object[] array) { return new Tuple(array); }

        public static object[] TableIndexToArray(Dictionary<object, object> table)
        {
            if (table == null) throw new LepException("null table", new ArgumentNullException(nameof(table), "null table"));
            return table.Keys.ToArray();
        }

        public static object[] TableElementToArray(Dictionary<object, object> table)
        {
            if (table == null) throw new LepException("null table", new ArgumentNullException(nameof(table), "null table"));
            return table.Values.ToArray();
        }

        public static Dictionary<object, object> ArrayToTable(object[] array)
        {
            if (array == null) throw new LepException("null array", new ArgumentNullException(nameof(array), "null array"));

            Dictionary<object, object> table = new Dictionary<object, object>();
            for (int i = 0; i < array.Length; i++) table.Add(i, array[i]);

            return table;
        }

        public static int ContainsIndex(Dictionary<object, object> table, object index)
        {
            if (table == null) throw new LepException("null table", new ArgumentNullException(nameof(table), "null table"));
            return table.ContainsKey(index) ? 1 : 0;
        }

        public static int IsNumber(object value) { return value is int ? 1 : 0; }

        public static int IsString(object value) { return value is string ? 1 : 0; }

        public static int IsTuple(object value) { return value is Tuple ? 1 : 0; }

        public static int IsArray(object value) { return value is object[] ? 1 : 0; }

        public static int IsTable(object value) { return value is Dictionary<object, object> ? 1 : 0; }
    }
}
