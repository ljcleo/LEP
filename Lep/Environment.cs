using System;
using System.Collections.Generic;
using System.Linq;

namespace Lep
{
    public class Environment
    {
        public const int LocalVariable = 0;
        public const int NormalVariable = 1;
        public const int OuterVariable = 2;
        public const int GlobalVariable = 3;

        private Dictionary<string, object> _values = new Dictionary<string,object>();
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
                default: throw new ArgumentException("bad type: " + type);
            }
        }

        public void Set(string name, object value, int type)
        {
            switch (type)
            {
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
                default: throw new ArgumentException("bad type: " + type);
            }
        }

        protected object GetLocal(string name)
        {
            object value;
            _values.TryGetValue(name, out value);

            return value;
        }

        protected object Get(string name)
        {
            object value;
            return _values.TryGetValue(name, out value) ? value : _father == null ? null : _father.Get(name);
        }

        protected object GetOuter(string name)
        {
            object value;
            return _father == null || (value = _father.GetOuter(name)) == null ? GetLocal(name) : value;
        }

        protected object GetGlobal(string name) { return _father == null ? GetLocal(name) : _father.GetGlobal(name); }

        protected void SetLocal(string name, object value) { _values[name] = value; }

        protected void Set(string name, object value)
        {
            if (_values.Keys.Contains(name) || _father == null) _values[name] = value;
            else _father.Set(name, value);
        }

        protected void SetOuter(string name, object value)
        {
            try
            {
                if (_father == null) throw new KeyNotFoundException(name);
                _father.SetOuter(name, value);
            }
            catch (KeyNotFoundException)
            {
                if (_values.Keys.Contains(name)) _values[name] = value;
                else throw;
            }
        }

        protected void SetGlobal(string name, object value)
        {
            if (_father == null) SetLocal(name, value);
            else _father.SetGlobal(name, value);
        }
    }
}
