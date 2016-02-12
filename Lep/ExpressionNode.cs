using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lep
{
    public class ExpressionNode : AstBranch
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        protected static readonly HashSet<string> Assignment = new HashSet<string>() { "=", "+=", "-=", "*=", "/=", "%=", "&&=", "||=" };

        public IAstNode Left { get { return this[0]; } }

        public IAstNode Operator { get { return this[1]; } }

        public IAstNode Right { get { return this[2]; } }

        public ExpressionNode(Collection<IAstNode> children) : base(children) { }

        public override object Evaluate(Environment env)
        {
            string op = ((AstLeaf)Operator).Token.Text;

            if (Assignment.Contains(op))
            {
                object right = Right.Evaluate(env);
                return ComputeAssignment(env, op, right);
            }
            else
            {
                object left = Left.Evaluate(env), right = Right.Evaluate(env);
                return ComputeOperator(left, op, right);
            }
        }

        protected object ComputeAssignment(Environment env, string op, object rvalue)
        {
            TupleNode tuple = Left as TupleNode;
            if (tuple != null) return AssignTuple(env, tuple, op, rvalue);

            FactorNode factor = Left as FactorNode;
            if (factor != null && factor.IsAssignable) return Assign(env, (PrimaryNode)factor.Operand, op, rvalue, factor.AssignType);

            throw new LepException("bad assignment", this);
        }

        protected object ComputeOperator(object left, string op, object right)
        {
            if (left == null) throw new ArgumentNullException("left", "null left value");
            if (right == null) throw new ArgumentNullException("right", "null right value");

            if (left is int && right is int) return ComputeNumber((int)left, op, (int)right);
            else if (op == "+")
            {
                object[] leftTuple = left as object[], rightTuple = right as object[];

                if (leftTuple != null && rightTuple != null)
                {
                    object[] result = new object[leftTuple.Length + rightTuple.Length];
                    Array.Copy(leftTuple, result, leftTuple.Length);
                    Array.Copy(rightTuple, 0, result, rightTuple.Length, rightTuple.Length);

                    return result;
                }
                else if (leftTuple != null)
                {
                    object[] result = new object[leftTuple.Length + 1];
                    Array.Copy(leftTuple, result, leftTuple.Length);
                    result[leftTuple.Length] = right;

                    return result;
                }
                else if (rightTuple != null)
                {
                    object[] result = new object[rightTuple.Length + 1];
                    result[0] = left;
                    Array.Copy(rightTuple, 0, result, 1, rightTuple.Length);

                    return result;
                }
                else return left.ToString() + right.ToString();
            }
            else if (op == "==") return left == null ? (right == null ? 1 : 0) : left.Equals(right) ? 1 : 0;
            else if (op == "!=") return left == null ? (right != null ? 1 : 0) : !left.Equals(right) ? 1 : 0;
            else throw new LepException("bad type", this);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        protected object ComputeNumber(int left, string op, int right)
        {
            switch (op)
            {
                case "+": return left + right;
                case "-": return left - right;
                case "*": return left * right;
                case "/": return left / right;
                case "%": return left % right;
                case "==": return left == right ? 1 : 0;
                case "!=": return left != right ? 1 : 0;
                case "<": return left < right ? 1 : 0;
                case "<=": return left <= right ? 1 : 0;
                case ">": return left > right ? 1 : 0;
                case ">=": return left >= right ? 1 : 0;
                case "&&": return left != 0 && right != 0 ? 1 : 0;
                case "||": return left != 0 || right != 0 ? 1 : 0;
                default: throw new LepException("bad operator", this);
            }
        }

        protected object AssignTuple(Environment env, TupleNode left, string op, object rvalue)
        {
            if (left == null) throw new ArgumentNullException("left", "null left tuple");

            object[] right = rvalue as object[];
            if (right != null && left.Count == right.Length)
            {
                object result = 0;

                int count = 0;
                foreach (IAstNode node in left)
                {
                    TupleNode tuple = node as TupleNode;

                    if (node != null) result = AssignTuple(env, tuple, op, right[count++]);
                    else
                    {
                        FactorNode factor = node as FactorNode;

                        if (factor == null || !factor.IsAssignable) throw new LepException("bad assignment", this);
                        
                        PrimaryNode primary = factor.Operand as PrimaryNode;
                        if (primary == null) throw new LepException("bad assignment", this);

                        if (((NameNode)primary.Operand).IsAnonymous) ++count;
                        else result = Assign(env, primary, op, right[count++], factor.AssignType);
                    }
                }

                return result;
            }
            else throw new LepException("bad assignment", this);
        }

        protected object Assign(Environment env, PrimaryNode left, string op, object rvalue, int type)
        {
            if (env == null) throw new ArgumentNullException("env", "null environment");
            if (left == null) throw new ArgumentNullException("left", "null left name");
            if (op == null) throw new ArgumentNullException("op", "null operator");

            if (left.IsName)
            {
                NameNode var = (NameNode)left.Operand;

                if (op == "=")
                {
                    env.Set(var.Name, rvalue, type);
                    return rvalue;
                }
                else
                {
                    object name = env.Get(var.Name, type);

                    if (name == null) throw new LepException("undefined name: " + var.Name, this);
                    else
                    {
                        object result = ComputeOperator(name, op.Substring(0, op.Length - 1), rvalue);
                        env.Set(var.Name, result, type);

                        return result;
                    }
                }
            }
            else throw new LepException("bad assignment", this);
        }
    }
}
