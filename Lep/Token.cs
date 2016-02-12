namespace Lep
{
    public class Token
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Token EndOfFile = new Token(-1);
        public static readonly string EndOfLine = "\\n";

        private int _line;

        public int Line { get { return _line; } }

        public string Location { get { return _line == -1 ? "the last line" : "\"" + Text + "\" at line " + Line; } }

        public virtual bool IsNumber { get { return false; } }

        public virtual bool IsString { get { return false; } }

        public virtual bool IsIdentifier { get { return false; } }

        public virtual int Number { get { throw new LepException("not number token"); } }

        public virtual string Text { get { return ""; } }

        protected Token(int line) { _line = line; }
    }
}
