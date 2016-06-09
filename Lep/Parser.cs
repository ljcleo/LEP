using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Lep
{
    public class Parser
    {
        private static readonly HashSet<string> _reserved = new HashSet<string>() { ")", "]", "}", ",", ";", ".", Token.EndOfLine };
        private static readonly HashSet<string> _primarySuffix = new HashSet<string>() { "(", "[", "{" };
        private static readonly HashSet<string> _selfChangePrefix = new HashSet<string>() { "++", "--" };
        private static readonly HashSet<string> _factorPrefix = new HashSet<string>() { "+", "-", "!", "~", "@", "$", "^" };
        private static readonly HashSet<string> _seperator = new HashSet<string>() { ",", Token.EndOfLine };
        private static readonly HashSet<string> _blockEnding = new HashSet<string>() { ".", ";" };
        private static readonly HashSet<string> _controller = new HashSet<string>() { "!", ":!", "!!" };

        private static readonly Dictionary<string, Precedence> _operators = new Dictionary<string, Precedence>()
        {
            { "=", new Precedence(0, Precedence.Right) },
            { "+=", new Precedence(0, Precedence.Right) },
            { "-=", new Precedence(0, Precedence.Right) },
            { "*=", new Precedence(0, Precedence.Right) },
            { "/=", new Precedence(0, Precedence.Right) },
            { "%=", new Precedence(0, Precedence.Right) },
            { "&&=", new Precedence(0, Precedence.Right) },
            { "||=", new Precedence(0, Precedence.Right) },
            { "||", new Precedence(1, Precedence.Left) },
            { "&&", new Precedence(2, Precedence.Left) },
            { "|", new Precedence(3, Precedence.Left) },
            { "^", new Precedence(4, Precedence.Left) },
            { "&", new Precedence(5, Precedence.Left) },
            { "==", new Precedence(6, Precedence.Left) },
            { "!=", new Precedence(6, Precedence.Left) },
            { "<", new Precedence(7, Precedence.Left) },
            { "<=", new Precedence(7, Precedence.Left) },
            { ">", new Precedence(7, Precedence.Left) },
            { ">=", new Precedence(7, Precedence.Left) },
            { "<<", new Precedence(8, Precedence.Left) },
            { ">>", new Precedence(8, Precedence.Left) },
            { "+", new Precedence(9, Precedence.Left) },
            { "-", new Precedence(9, Precedence.Left) },
            { "*", new Precedence(10, Precedence.Left) },
            { "/", new Precedence(10, Precedence.Left) },
            { "%", new Precedence(10, Precedence.Left) }
        };

        private Lexer _lexer;

        public Parser(Lexer lexer) { _lexer = lexer; }

        public IAstNode Parse() { return Program(); }

        private IAstNode Tuple()
        {
            Skip("{");

            Collection<IAstNode> tuple = new Collection<IAstNode>();

            while (!IsNext("}"))
            {
                tuple.Add(Expression());
                if (IsNext("}")) break;

                Skip(":");
            }

            Skip("}");
            return new TupleNode(tuple);
        }

        private IAstNode Array()
        {
            Skip("[");

            Collection<IAstNode> array = new Collection<IAstNode>();

            while (!IsNext("]"))
            {
                array.Add(Expression());
                if (IsNext("]")) break;

                Skip(":");
            }

            Skip("]");

            if (IsNext("(", true))
            {
                array.Insert(0, Expression());
                Skip(")");
            }
            else array.Insert(0, new NullNode(new Collection<IAstNode>()));

            return new ArrayNode(array);
        }

        private IAstNode Table()
        {
            Skip("[:");

            Collection<IAstNode> table = new Collection<IAstNode>();

            while (!IsNext(":]"))
            {
                table.Add(Tuple());
                if (IsNext(":]")) break;

                Skip(":");
            }

            Skip(":]");
            return new TableNode(table);
        }

        private IAstNode VariableArgument()
        {
            Skip("(");
            IAstNode expr = Expression();
            Skip(")");

            return new ExpressionArgumentNode(new Collection<IAstNode>() { expr });
        }

        private IAstNode ArrayReference()
        {
            Skip("[");
            IAstNode expr = Expression();
            Skip("]");

            return new ArrayReferenceNode(new Collection<IAstNode>() { expr });
        }

        private IAstNode Primary()
        {
            if (IsNext("(", true))
            {
                IAstNode expr = Expression();
                Skip(")");

                return expr;
            }
            else
            {
                Collection<IAstNode> primary = new Collection<IAstNode>();

                Token next = _lexer.Read();
                if (next.IsNumber) primary.Add(new NumberNode(next));
                else if (next.IsString) primary.Add(new StringNode(next));
                else if (next.IsIdentifier && !_reserved.Contains(next.Text)) primary.Add(new NameNode(next));
                else throw new ParseException(next);

                while (IsNext(_primarySuffix))
                {
                    if (IsNext("{")) primary.Add(new ArgumentNode((TupleNode)Tuple()));
                    else if (IsNext("(")) primary.Add(VariableArgument());
                    else if (IsNext("[")) primary.Add(ArrayReference());
                    else throw new ParseException(_lexer.Read());
                }

                return new PrimaryNode(primary);
            }
        }

        private IAstNode SelfChange()
        {
            Token prefix = _lexer.Read();

            if (prefix.IsIdentifier && _selfChangePrefix.Contains(prefix.Text)) return new SelfChangeNode(new Collection<IAstNode>() { new AstLeaf(prefix), Factor() });
            else throw new ParseException(prefix);
        }

        private IAstNode Factor()
        {
            if (IsNext("{")) return Tuple();
            else if (IsNext("[")) return Array();
            else if (IsNext("[:")) return Table();

            IAstNode prefix = IsNext(_factorPrefix) ? (IAstNode)new AstLeaf(_lexer.Read()) : (IAstNode)new NullNode(new Collection<IAstNode>());
            IAstNode operand = IsNext(_selfChangePrefix) ? SelfChange() : Primary();

            return new FactorNode(new Collection<IAstNode>() { prefix, operand });
        }

        private IAstNode Expression()
        {
            IAstNode expr = Factor();

            Precedence next;
            while ((next = NextOperator()) != null) expr = Shift(expr, next.Level);

            return expr;
        }

        private IAstNode Block()
        {
            Skip("->");
            Collection<IAstNode> paragraph = new Collection<IAstNode>();

            while (!IsNext(_blockEnding)) if (!IsNext(_seperator, true))
                {
                    paragraph.Add(Statement());
                    if (IsNext(_blockEnding)) break;

                    Skip(_seperator);
                }

            if (IsNext(_blockEnding)) return new BlockNode(new Collection<IAstNode>() { new ParagraphNode(paragraph), new AstLeaf(_lexer.Read()) });
            else throw new ParseException(_lexer.Read());
        }

        private IAstNode Guard()
        {
            IAstNode condition = Expression();
            IAstNode body = Block();

            return new GuardNode(new Collection<IAstNode>() { condition, body });
        }

        private IAstNode GuardList()
        {
            Skip(":");
            while (IsNext(Token.EndOfLine, true)) ;

            Collection<IAstNode> guards = new Collection<IAstNode>();
            while (!IsNext("."))
            {
                guards.Add(Guard());
                while (IsNext(Token.EndOfLine, true)) ;
            }

            Skip(".");
            return new GuardListNode(guards);
        }

        private IAstNode Statement()
        {
            if (IsNext("?", true))
            {
                if (IsNext("=", true))
                {
                    IAstNode expression = Expression();
                    IAstNode guards = GuardList();

                    return new SwitchNode(new Collection<IAstNode>() { expression, guards });
                }
                else return GuardList();
            }
            else if (IsNext("*", true))
            {
                while (IsNext(Token.EndOfLine, true)) ;
                return new WhileNode(new Collection<IAstNode>() { Guard() });
            }
            else if (IsNext("->")) return Block();
            else if (IsNext(_controller)) return new ControlNode(new Collection<IAstNode>() { new AstLeaf(_lexer.Read()) });
            else return Expression();
        }

        private IAstNode FunctionDefinition()
        {
            Skip("#");

            Token name = _lexer.Read();
            if (!name.IsIdentifier || _reserved.Contains(name.Text)) throw new ParseException(name);

            IAstNode funcname = new NameNode(name);
            IAstNode parameters = new ParameterNode((TupleNode)Tuple());
            IAstNode body = Block();

            return new FunctionDefinitionNode(new Collection<IAstNode>() { funcname, parameters, body });
        }

        private IAstNode Program()
        {
            if (!IsNext(_seperator, true))
            {
                IAstNode body;

                if (IsNext("#")) body = FunctionDefinition();
                else body = Statement();

                if (IsNext(_seperator, true)) return body;
                else throw new ParseException(_lexer.Read());
            }
            else return new NullNode(new Collection<IAstNode>());
        }

        private void Skip(string name)
        {
            Token next = _lexer.Read();
            if (!next.IsIdentifier || next.Text != name) throw new ParseException(next);
        }

        private void Skip(HashSet<string> set)
        {
            Token next = _lexer.Read();
            if (!next.IsIdentifier || !set.Contains(next.Text)) throw new ParseException(next);
        }

        private bool IsNext(string name, bool readIfYes = false, int skip = 0)
        {
            Token next = _lexer.Peek(skip);
            if (next.IsIdentifier && next.Text == name)
            {
                if (readIfYes) _lexer.Read();
                return true;
            }
            else return false;
        }

        private bool IsNext(HashSet<string> set, bool readIfYes = false, int skip = 0)
        {
            Token next = _lexer.Peek(skip);
            if (next.IsIdentifier && set.Contains(next.Text))
            {
                if (readIfYes) _lexer.Read();
                return true;
            }
            else return false;
        }

        private IAstNode Shift(IAstNode left, int level)
        {
            AstLeaf op = new AstLeaf(_lexer.Read());
            IAstNode right = Factor();

            Precedence next;
            while ((next = NextOperator()) != null && RightFirst(level, next)) right = Shift(right, next.Level);

            return new ExpressionNode(new Collection<IAstNode>() { left, op, right });
        }

        private Precedence NextOperator()
        {
            Token next = _lexer.Peek(0);
            return next.IsIdentifier && _operators.Keys.Contains(next.Text) ? _operators[next.Text] : null;
        }

        private static bool RightFirst(int level, Precedence next) { return next.LeftAssociation ? level < next.Level : level <= next.Level; }

        protected class Precedence
        {
            public const bool Left = true;
            public const bool Right = false;

            public int Level { get; set; }

            public bool LeftAssociation { get; set; }

            public Precedence(int level, bool left)
            {
                Level = level;
                LeftAssociation = left;
            }
        }
    }
}
