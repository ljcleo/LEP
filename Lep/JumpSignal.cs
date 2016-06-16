using System;
using System.Runtime.Serialization;

namespace Lep
{
    [Serializable]
    public class JumpSignal : Exception
    {
        public const int UnknownSignal = 0;
        public const int BreakSignal = 1;
        public const int ContinueSignal = 2;
        public const int ReturnSignal = 3;

        private int _signalType;
        private object _returnValue;

        public int SignalType { get { return _signalType; } }

        public object ReturnValue { get { return _returnValue; } }

        public JumpSignal() : this(UnknownSignal, 0) { }

        public JumpSignal(int type) : this(type, 0) { }

        public JumpSignal(int type, object value)
            : base()
        {
            _signalType = type;
            _returnValue = value;
        }

        public JumpSignal(string msg) : base(msg) { }

        public JumpSignal(string msg, Exception inner) : base(msg, inner) { }

        protected JumpSignal(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public override void GetObjectData(SerializationInfo info, StreamingContext context) { base.GetObjectData(info, context); }
    }
}
