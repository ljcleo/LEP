using System.Collections.Generic;

namespace Lep
{
    public interface IAstNode : IEnumerable<IAstNode>
    {
        IAstNode this[int index] { get; }

        int Count { get; }

        IEnumerator<IAstNode> Children { get; }

        string Location { get; }

        object Evaluate(Environment env);
    }
}
