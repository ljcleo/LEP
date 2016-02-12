using System;
using System.IO;
using System.Runtime.Serialization;

namespace Lep
{
    [Serializable]
    public class ParseException : Exception
    {
        public ParseException() : base() { }

        public ParseException(Token token) : this(token, "") { }

        public ParseException(Token token, string msg) : base("syntax error around " + (token == null ? "" : token.Location) + ". " + msg) { }

        public ParseException(IOException inner) : base("", inner) { }

        public ParseException(string msg) : base(msg) { }

        public ParseException(string msg, Exception inner) : base(msg, inner) { }

        protected ParseException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
