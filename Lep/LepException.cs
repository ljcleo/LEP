using System;
using System.Runtime.Serialization;

namespace Lep
{
    [Serializable]
    public class LepException : InvalidOperationException
    {
        public LepException() : base() { }

        public LepException(string msg) : base(msg) { }

        public LepException(string msg, IAstNode node) : base(msg + " " + (node == null ? "" : node.Location)) { }

        public LepException(string msg, Exception inner) : base(msg, inner) { }

        protected LepException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
