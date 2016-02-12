using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Lep
{
    public class Lexer
    {
        public static readonly string SpacePattern = @"(?<space>\s*)";
        public static readonly string CommentPattern = @"(?<comment>//.*)";
        public static readonly string NumberPattern = @"(?<number>[0-9]+)";
        public static readonly string StringPattern = @"(?<string>""(\\""|\\\\|\\n|[^""])*""|@""(""""|[^""])*"")";
        public static readonly string IdentifierPattern = @"(?<identifier>[A-Z_a-z][A-Z_a-z0-9]*|\+=|\-=|\*=|\/=|%=|&&=|\|\|=|\+\+|\-\-|==|>=|<=|!=|&&|\|\||->|!!|:!|[~!@#\$%\^&\*\(\)\-_\+=\|\\\}\]\{\[:;<,>\.\?\/""])";
        public static readonly string Pattern = SpacePattern + "(" + CommentPattern + "|" + NumberPattern + "|" + StringPattern + "|" + IdentifierPattern + ")?";

        private Regex _regex = new Regex(Pattern, RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        private List<Token> _tokenList = new List<Token>();
        private bool _hasMore = true;

        private TextReader _reader;
        private int _currentLine = 0;

        public Lexer(TextReader reader) { _reader = reader; }

        public Token Read()
        {
            if (FillList(0))
            {
                Token next = _tokenList[0];
                _tokenList.RemoveAt(0);

                return next;
            }
            else return Token.EndOfFile;
        }

        public Token Peek(int skip) { return FillList(skip) ? _tokenList[skip] : Token.EndOfFile; }

        private bool FillList(int count)
        {
            while (_tokenList.Count <= count)
            {
                if (_hasMore) ReadLine();
                else return false;
            }

            return true;
        }

        protected void ReadLine()
        {
            string line;

            try { line = _reader.ReadLine(); }
            catch (IOException e) { throw new ParseException(e); }

            if (line == null)
            {
                _hasMore = false;
                return;
            }

            ++_currentLine;
            int pos = 0;

            Match next = _regex.Match(line);
            while (pos < line.Length)
            {
                if (next.Success && next.Index == pos)
                {
                    AddToken(next);
                    pos += next.Length;

                    next = next.NextMatch();
                }
                else throw new ParseException("bad token at line " + _currentLine);
            }

            _tokenList.Add(new IdentifierToken(_currentLine, Token.EndOfLine));
        }

        protected void AddToken(Match match)
        {
            if (match == null) throw new ArgumentNullException("match", "null match");

            if (!match.Groups[2].Success)
            {
                Token next;

                if (match.Groups[3].Success) next = new NumberToken(_currentLine, Int32.Parse(match.Groups[3].Value, NumberStyles.None, CultureInfo.InvariantCulture));
                else if (match.Groups[4].Success) next = new StringToken(_currentLine, Translate(match.Groups[4].Value));
                else if (match.Groups[5].Success) next = new IdentifierToken(_currentLine, match.Groups[5].Value);
                else return;

                _tokenList.Add(next);
            }
        }

        protected static string Translate(string str)
        {
            if (str == null) return "";

            StringBuilder builder = new StringBuilder();

            int len = str.Length - 1;
            if (str[0] == '@') for (int i = 2; i < len; i++)
                {
                    char c = str[i];
                    if (c == '"' && i + 1 < len) ++i;

                    builder.Append(c);
                }
            else for (int i = 1; i < len; i++)
                {
                    char c = str[i];
                    if (c == '\\' && i + 1 < len)
                    {
                        c = str[i + 1] == '"' || str[i + 1] == '\\' ? str[i + 1] : '\n';
                        ++i;
                    }

                    builder.Append(c);
                }

            return builder.ToString();
        }

        protected class NumberToken : Token
        {
            private int _number;

            public override bool IsNumber { get { return true; } }

            public override int Number { get { return _number; } }

            public override string Text { get { return _number.ToString(CultureInfo.InvariantCulture); } }

            public NumberToken(int line, int number) : base(line) { _number = number; }
        }

        protected class StringToken : Token
        {
            private string _string;

            public override bool IsString { get { return true; } }

            public override string Text { get { return _string; } }

            public StringToken(int line, string str) : base(line) { _string = str; }
        }

        protected class IdentifierToken : Token
        {
            private string _identifier;

            public override bool IsIdentifier { get { return true; } }

            public override string Text { get { return _identifier; } }

            public IdentifierToken(int line, string identifier) : base(line) { _identifier = identifier; }
        }
    }
}
