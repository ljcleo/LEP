using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lep
{
    public class ExpressionNode : AstBranch
    {
        private static readonly HashSet<string> Assignment = new HashSet<string>() { "=", "+=", "-=", "*=", "/=", "%=", "<<=", ">>=", "&=", "^=", "|=", "&&=", "||=" };
        private static readonly HashSet<string> Calcuation = new HashSet<string>() { "+", "-", "*", "/", "%", "<<", ">>", "&", "^", "|" };
        private static readonly HashSet<string> Judgement = new HashSet<string>() { "==", "!=", "<", "<=", ">", ">=", "&&", "||" };

        public IAstNode Left { get { return this[0]; } }

        public IAstNode Operator { get { return this[1]; } }

        public IAstNode Right { get { return this[2]; } }

        public ExpressionNode(Collection<IAstNode> children) : base(children) { }

        public override object Evaluate(Environment env)
        {
            string op = ((AstLeaf)Operator).Token.Text;

            if (op == "<=>") return ComputeBinding(env, Right.Evaluate(env));
            if (Assignment.Contains(op)) return ComputeAssignment(env, op, Right.Evaluate(env)); 
            else return ComputeOperator(Left.Evaluate(env), op, Right.Evaluate(env));
        }

        private object ComputeBinding(Environment env, object rvalue)
        {
            FactorNode factor = Left as FactorNode;

            if (factor != null && factor.IsNoPrefix)
            {
                string name = ((NameNode)((ScopeNameNode)((PrimaryNode)factor.Operand).Operand).Name).Name;
                env.Set(name, rvalue, Environment.Constant);

                return rvalue;
            }

            throw new LepException("bad binding", this);
        }

        private object ComputeAssignment(Environment env, string op, object rvalue)
        {
            TupleNode tuple = Left as TupleNode;
            if (tuple != null) return AssignTuple(env, tuple, op, rvalue);

            FactorNode factor = Left as FactorNode;
            if (factor != null && factor.IsNoPrefix) return Assign(env, (PrimaryNode)factor.Operand, op, rvalue);

            throw new LepException("bad assignment", this);
        }

        private object ComputeOperator(object left, string op, object right)
        {
            if (left == null) throw new LepException("internal error: null left value", this);
            if (right == null) throw new LepException("internal error: null right value", this);

            if (left is int && right is int) return ComputeNumber((int)left, op, (int)right);
            else if (op == "+")
            {
                Tuple leftTuple = left as Tuple, rightTuple = right as Tuple;

                if (leftTuple != null && rightTuple != null) return leftTuple + rightTuple;
                else if (leftTuple != null) return leftTuple + right;
                else if (rightTuple != null) return left + rightTuple;
                else return left.ToString() + right.ToString();
            }
            else if (op == "==") return left == null ? (right == null ? 1 : 0) : left.Equals(right) ? 1 : 0;
            else if (op == "!=") return left == null ? (right != null ? 1 : 0) : !left.Equals(right) ? 1 : 0;
            else throw new LepException("bad type", this);
        }
        
        private object ComputeNumber(int left, string op, int right)
        {
            if (Calcuation.Contains(op)) return ComputeCalculation(left, op, right);
            else if (Judgement.Contains(op)) return ComputeJudgement(left, op, right);
            else throw new LepException("bad operator", this);
        }

        private object ComputeCalculation(int left, string op, int right)
        {
            switch (op)
            {
                case "+": return left + right;
                case "-": return left - right;
                case "*": return left * right;
                case "/":
                    if (right == 0) throw new LepException("number divided by zero", this);
                    return left / right;
                case "%": return left % right;
                case "<<": return left << right;
                case ">>": return left >> right;
                case "&": return left & right;
                case "^": return left ^ right;
                case "|": return left | right;
                default: throw new LepException("bad operator", this);
            }
        }

        private object ComputeJudgement(int left, string op, int right)
        {
            switch (op)
            {
                case "==": return BoolToInt(left == right);
                case "!=": return BoolToInt(left != right);
                case "<": return BoolToInt(left < right);
                case "<=": return BoolToInt(left <= right);
                case ">": return BoolToInt(left > right);
                case ">=": return BoolToInt(left >= right);
                case "&&": return BoolToInt(left * right != 0);
                case "||": return BoolToInt(left != 0 || right != 0);
                default: throw new LepException("bad operator", this);
            }
        }

        private object AssignTuple(Environment env, TupleNode left, string op, object rvalue)
        {
            if (left == null) throw new LepException("internal error: null left tuple", this);

            Tuple right = rvalue as Tuple;
            if (right != null)
            {
                object result = 0;

                int count = 0;
                foreach (IAstNode node in left)
                {
                    if (count >= right.Count) break;

                    TupleNode tuple = node as TupleNode;

                    if (node != null) result = AssignTuple(env, tuple, op, right[count++]);
                    else
                    {
                        FactorNode factor = node as FactorNode;
                        if (factor == null || !factor.IsNoPrefix) throw new LepException("bad assignment", this);

                        PrimaryNode primary = factor.Operand as PrimaryNode;
                        if (primary == null || !primary.IsAssignable) throw new LepException("bad assignment", this);

                        if (((NameNode)primary.Operand).IsAnonymous) ++count;
                        else result = Assign(env, primary, op, right[count++]);
                    }
                }

                return result;
            }
            else throw new LepException("bad assignment", this);
        }

        private object Assign(Environment env, PrimaryNode left, string op, object rvalue)
        {
            if (env == null) throw new LepException("internal error: null environment", this);
            if (left == null) throw new LepException("internal error: null left name", this);
            if (op == null) throw new LepException("internal error: null operator", this);

            ScopeNameNode scope = (ScopeNameNode)left.Operand;

            string name = ((NameNode)scope.Name).Name;
            if (env.IsConstant(name)) throw new LepException("cannot assign constant: " + name, this);

            if (left.IsNoSuffixName)
            {
                if (op == "=")
                {
                    env.Set(name, rvalue, scope.AssignType);
                    return rvalue;
                }
                else
                {
                    object old = env.Get(name, scope.AssignType);

                    if (old == null) throw new LepException("undefined name: " + name, this);
                    else
                    {
                        object result = ComputeOperator(name, op.Substring(0, op.Length - 1), rvalue);
                        env.Set(name, result, scope.AssignType);

                        return result;
                    }
                }
            }
            else if (left.IsAllArrayReference)
            {
                ArrayReferenceNode arrRef = left.Suffix(0) as ArrayReferenceNode;

                if (arrRef != null)
                {
                    object lvalue = left.EvaluateSub(env, 1);

                    object[] arr = lvalue as object[];

                    if (arr != null)
                    {
                        object index = arrRef.Index.Evaluate(env);

                        if (index is int)
                        {
                            int id = (int)index;

                            if (id < arr.Length) return arr[id] = op == "=" ? rvalue : ComputeOperator(arr[id], op.Substring(0, op.Length - 1), rvalue);
                            else throw new LepException("bad array access", this);
                        }
                    }

                    Dictionary<object, object> table = lvalue as Dictionary<object, object>;

                    if (table != null)
                    {
                        object index = arrRef.Index.Evaluate(env);

                        if (op == "=") return table[index] = rvalue;
                        else
                        {
                            object old = null;
                            table.TryGetValue(index, out old);

                            if (old == null) throw new LepException("bad array access", this);
                            else return table[index] = ComputeOperator(old, op.Substring(0, op.Length - 1), rvalue);
                        }
                    }
                }
            }

            throw new LepException("bad assignment", this);
        }

        private static int BoolToInt(bool value) { return value ? 1 : 0; }
    }
}
