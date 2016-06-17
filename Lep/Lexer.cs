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
        public static readonly string NumberPattern = @"(?<number>\d\w*)";
        public static readonly string CharPattern = @"(?<char>'(\\'|\\\\|\n|[^'])?')";
        public static readonly string StringPattern = @"(?<string>""(\\""|\\\\|\\n|[^""])*""|@""(""""|[^""])*"")";
        
        public static readonly string LatinPattern = @"\w\p{IsLatin-1Supplement}\p{IsLatinExtended-A}\p{IsLatinExtended-B}";
        public static readonly string CyrillicPattern = @"\p{IsCyrillic}\p{IsCyrillicSupplement}";
        public static readonly string ArabicPattern = @"\p{IsArabic}";
        public static readonly string KoreanPattern = @"\p{IsHangulJamo}\p{IsHangulCompatibilityJamo}";
        public static readonly string JapanesePattern = @"\p{IsHiragana}\p{IsKatakana}";
        public static readonly string ChinesePattern = @"\p{IsCJKUnifiedIdeographs}";

        public static readonly string SupportedCharPattern = LatinPattern + CyrillicPattern + ArabicPattern + KoreanPattern + JapanesePattern + ChinesePattern;
        public static readonly string NamePattern = "[" + SupportedCharPattern + @"-[\d]][" + SupportedCharPattern + "]*";

        public static readonly string DoubleCharOperatorPattern = @"<=>|\+=|-=|\*=|/=|%=|&&=|\|\|=|\+\+|--|==|>=|<=|!=|&&|\|\||<<|>>|\[:|:\]|->|!!|:!";
        public static readonly string SingleCharOperatorPattern = @"[\p{P}\p{S}]";
        public static readonly string IdentifierPattern = @"(?<identifier>" + NamePattern + "|" + DoubleCharOperatorPattern + "|" + SingleCharOperatorPattern + ")";

        public static readonly string Pattern = SpacePattern + "(" + CommentPattern + "|" + NumberPattern + "|" + CharPattern + "|" + StringPattern + "|" + IdentifierPattern + ")?";

        private Regex _regex = new Regex(Pattern, RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        private List<Token> _tokenList = new List<Token>();
        private bool _hasMore = true;

        private Dictionary<string, string> _definitionList = new Dictionary<string, string>();
        private Dictionary<string, string> _localizationList = new Dictionary<string, string>();
        private int _layer = 0;
        private bool _skip = false;

        private TextReader _reader;
        private int _currentLine = 0;
        private bool _console = false;
        private string _start = "";

        public Lexer(TextReader reader) { _reader = reader; }

        public Lexer(TextReader reader, bool console, string start)
        {
            _reader = reader;
            _console = console;
            _start = start;
        }

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

        public static int ParseNumber(string str)
        {
            if (str == null) throw new LepException("null string", new ArgumentNullException(nameof(str), str));

            if (str[0] == '\'')
            {
                if (str[1] == '\\') return str[2] == '"' || str[2] == '\\' ? str[2] : '\n';
                else return str[1];
            }
            else if (str[0] == '0')
            {
                if (str.Length == 1) return 0;
                else if (str.Length == 2) throw new ParseException("bad number");

                int p = str[2] == '0' ? 3 : 4;
                if (str.Length <= p) throw new ParseException("bad number");

                int k = p == 3 ? str[1] - '0' : (str[1] - '0') * 10 + str[2] - '0';
                if (k < 2 || k > 36) throw new ParseException("bad number");

                int result = 0;

                while (p < str.Length)
                {
                    if (str[p] >= '0' && str[p] <= '9' && str[p] - '0' < k) result = result * k + str[p] - '0';
                    else if (str[p] >= 'A' && str[p] <= 'Z' && str[p] - 'A' + 10 < k) result = result * k + str[p] - 'A' + 10;
                    else if (str[p] >= 'a' && str[p] <= 'z' && str[p] - 'a' + 10 < k) result = result * k + str[p] - 'a' + 10;
                    else throw new ParseException("bad number");

                    ++p;
                }

                return result;
            }
            else return int.Parse(str, NumberStyles.None, CultureInfo.InvariantCulture);
        }

        private bool FillList(int count)
        {
            while (_tokenList.Count <= count)
            {
                if (_hasMore) ReadLine();
                else return false;
            }

            return true;
        }

        private void ReadLine()
        {
            string line;

            try
            {
                if (_console) Console.Write(_start);
                line = _reader.ReadLine();
            }
            catch (IOException e) { throw new ParseException(e); }

            if (line == null || line == "\u001a")
            {
                _hasMore = false;
                return;
            }

            string nolp = DeleteLeftPad(line);
            if (nolp.StartsWith("--", StringComparison.Ordinal))
            {
                string[] parts = nolp.Split(new char[] { ' ', '\t', '\r', '\n', '\u001a' });

                switch (parts[0])
                {
                    case "--define":
                        Define(parts);
                        break;
                    case "--undef":
                        Undefine(parts);
                        break;
                    case "--ifdef":
                        IfDefine(parts);
                        break;
                    case "--ifndef":
                        IfNotDefine(parts);
                        break;
                    case "--if":
                        If(parts);
                        break;
                    case "--endif":
                        EndIf(parts);
                        break;
                    case "--local":
                        throw new ParseException("not implemented command at line " + _currentLine);
                    default:
                        throw new ParseException("bad control command at line " + _currentLine);
                }
                
                return;
            }

            ++_currentLine;
            if (!_skip) ParseString(line);
        }

        private void Define(string[] parts)
        {
            if (parts == null || parts.Length < 2) throw new ParseException("bad define command at line " + _currentLine);

            StringBuilder builder = new StringBuilder();

            if (parts.Length == 2) builder.Append("1");
            else for (int i = 2; i < parts.Length; i++) builder.Append(parts[i]);

            _definitionList.Add(parts[1], builder.ToString());
        }

        private void Undefine(string[] parts) { if (parts == null || parts.Length != 2 || !_definitionList.Remove(parts[1])) throw new ParseException("bad undef command at line " + _currentLine); }

        private void IfDefine(string[] parts)
        {
            if (parts == null || parts.Length != 2 || !_definitionList.ContainsKey(parts[1])) _skip = true;
            ++_layer;
        }

        private void IfNotDefine(string[] parts)
        {
            if (parts == null || parts.Length != 2 || _definitionList.ContainsKey(parts[1])) _skip = true;
            ++_layer;
        }

        private void If(string[] parts)
        {
            if (parts == null || parts.Length != 2 || !_definitionList.ContainsKey(parts[1]) || _definitionList[parts[1]] == "0") _skip = true;
            ++_layer;
        }

        private void EndIf(string[] parts)
        {
            if (parts == null || parts.Length != 1 || _layer == 0) throw new ParseException("bad endif command at line " + _currentLine);

            if (_skip) _skip = false;
            --_layer;
        }

        private void ParseString(string str) { ParseString(str, true); }

        private void ParseString(string str, bool eol)
        {
            if (str == null) return;

            int pos = 0;

            Match next = _regex.Match(str);
            while (pos < str.Length)
            {
                if (next.Success && next.Index == pos)
                {
                    AddToken(next);
                    pos += next.Length;

                    next = next.NextMatch();
                }
                else throw new ParseException("bad token at line " + _currentLine);
            }

            if (eol) _tokenList.Add(new IdentifierToken(_currentLine, Token.EndOfLine));
        }

        private void AddToken(Match match)
        {
            if (match == null) throw new ParseException("internal error: null match");

            if (!match.Groups[2].Success)
            {
                Token next;

                if (match.Groups[3].Success) next = new NumberToken(_currentLine, ParseNumber(match.Groups[3].Value));
                else if (match.Groups[4].Success) next = new NumberToken(_currentLine, ParseNumber(match.Groups[4].Value));
                else if (match.Groups[5].Success) next = new StringToken(_currentLine, Translate(match.Groups[5].Value));
                else if (match.Groups[6].Success)
                {
                    string id = match.Groups[6].Value;

                    if (_definitionList.ContainsKey(id))
                    {
                        ParseString(_definitionList[id], false);
                        return;
                    }
                    else if (_localizationList.ContainsKey(id))
                    {
                        ParseString(_localizationList[id], false);
                        return;
                    }

                    next = new IdentifierToken(_currentLine, id);
                }
                else return;

                _tokenList.Add(next);
            }
        }

        private static string DeleteLeftPad(string str)
        {
            if (str == null) return null;

            int pos = 0;
            while (pos < str.Length && (str[pos] == ' ' || str[pos] == '\n' || str[pos] == '\t' || str[pos] == '\u001a')) ++pos;

            return str.Substring(pos);
        }

        private static string Translate(string str)
        {
            if (str == null) return "";

            StringBuilder builder = new StringBuilder();

            int len = str.Length - 1;
            if (str[0] == '@')
            {
                for (int i = 2; i < len; i++)
                {
                    char c = str[i];
                    if (c == '"' && i + 1 < len) ++i;

                    builder.Append(c);
                }
            }
            else
            {
                for (int i = 1; i < len; i++)
                {
                    char c = str[i];
                    if (c == '\\' && i + 1 < len)
                    {
                        c = str[i + 1] == '"' || str[i + 1] == '\\' ? str[i + 1] : '\n';
                        ++i;
                    }

                    builder.Append(c);
                }
            }

            return builder.ToString();
        }

        private class NumberToken : Token
        {
            private int _number;

            public override bool IsNumber { get { return true; } }

            public override int Number { get { return _number; } }

            public override string Text { get { return _number.ToString(CultureInfo.InvariantCulture); } }

            public NumberToken(int line, int number) : base(line) { _number = number; }
        }

        private class StringToken : Token
        {
            private string _string;

            public override bool IsString { get { return true; } }

            public override string Text { get { return _string; } }

            public StringToken(int line, string str) : base(line) { _string = str; }
        }

        private class IdentifierToken : Token
        {
            private string _identifier;

            public override bool IsIdentifier { get { return true; } }

            public override string Text { get { return _identifier; } }

            public IdentifierToken(int line, string identifier) : base(line) { _identifier = identifier; }
        }
    }
}
