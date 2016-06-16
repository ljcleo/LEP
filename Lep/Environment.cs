using System;
using System.Collections.Generic;
using System.Linq;

namespace Lep
{
    public class Environment
    {
        public const int Constant = -1;
        public const int LocalVariable = 0;
        public const int NormalVariable = 1;
        public const int OuterVariable = 2;
        public const int GlobalVariable = 3;

        private Dictionary<string, Tuple<bool, object>> _values = new Dictionary<string, Tuple<bool, object>>();
        private Environment _father;

        public Environment() : this(null) { }

        public Environment(Environment father) { _father = father; }

        public object Get(string name, int type)
        {
            switch (type)
            {
                case LocalVariable: return GetLocal(name);
                case NormalVariable: return Get(name);
                case OuterVariable: return GetOuter(name);
                case GlobalVariable: return GetGlobal(name);
                default: throw new LepException("internal error: bad type: " + type, new ArgumentException("bad type: " + type));
            }
        }

        public void Set(string name, object value, int type)
        {
            switch (type)
            {
                case Constant:
                    SetConstant(name, value);
                    break;
                case LocalVariable:
                    SetLocal(name, value);
                    break;
                case NormalVariable:
                    Set(name, value);
                    break;
                case OuterVariable:
                    SetOuter(name, value);
                    break;
                case GlobalVariable:
                    SetGlobal(name, value);
                    break;
                default: throw new LepException("internal error: bad type: " + type, new ArgumentException("bad type: " + type));
            }
        }

        public bool IsConstant(string name)
        {
            Tuple<bool, object> value;
            return (_values.TryGetValue(name, out value) && value.Item1);
        }

        private object GetLocal(string name)
        {
            Tuple<bool, object> value;
            return _values.TryGetValue(name, out value) ? value.Item2 : null;
        }

        private object Get(string name)
        {
            Tuple<bool, object> value;
            return _values.TryGetValue(name, out value) ? value.Item2 : _father == null ? null : _father.Get(name);
        }

        private object GetOuter(string name)
        {
            object value;
            return _father == null || (value = _father.GetOuter(name)) == null ? GetLocal(name) : value;
        }

        private object GetGlobal(string name) { return _father == null ? GetLocal(name) : _father.GetGlobal(name); }

        private void SetConstant(string name, object value)
        {
            if (_values.Keys.Contains(name)) throw new LepException("cannot override: " + name, new ArgumentException("cannot override: " + name));
            else _values[name] = new Tuple<bool, object>(true, value);
        }

        private void SetLocal(string name, object value)
        {
            Tuple<bool, object> old;
            if (_values.TryGetValue(name, out old) && old.Item1) throw new LepException("cannot assign constant: " + name, new ArgumentException("cannot assign constant: " + name));

            _values[name] = new Tuple<bool, object>(false, value);
        }

        private void Set(string name, object value)
        {
            Tuple<bool, object> old;

            if (_values.TryGetValue(name, out old))
            {
                if (old.Item1) throw new LepException("cannot assign constant: " + name, new ArgumentException("cannot assign constant: " + name));
                else _values[name] = new Tuple<bool, object>(false, value);
            }
            else if (_father == null) _values[name] = new Tuple<bool, object>(false, value);
            else _father.Set(name, value);
        }

        private void SetOuter(string name, object value)
        {
            try
            {
                if (_father == null) throw new LepException("outer name not found: " + name, new KeyNotFoundException(name));
                _father.SetOuter(name, value);
            }
            catch (LepException)
            {
                Tuple<bool, object> old;

                if (_values.TryGetValue(name, out old))
                {
                    if (old.Item1) throw new LepException("cannot assign constant: " + name, new ArgumentException("cannot assign constant: " + name));
                    else _values[name] = new Tuple<bool, object>(false, value);
                }
                else throw;
            }
        }

        private void SetGlobal(string name, object value)
        {
            if (_father == null) SetLocal(name, value);
            else _father.SetGlobal(name, value);
        }
    }
}
