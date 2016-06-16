namespace Lep
{
    public class Token
    {
        private static readonly Token _eof = new Token(-1);
        private const string _eoln = "\\n";

        private int _line;

        public static Token EndOfFile { get { return _eof; } }

        public static string EndOfLine { get { return _eoln; } }

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
