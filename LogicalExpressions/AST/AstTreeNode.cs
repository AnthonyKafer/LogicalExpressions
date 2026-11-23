using LogicalExpressions.Parser;
using System.Xml.Linq;
using static LogicalExpressions.AST.AstEvaluator;

namespace LogicalExpressions.AST
{
    public class AstTreeNode
    {
        const int MaxChildrenPerNode = 2;
        public bool IsRoot { get { return _parent is null; } }

        public AstTreeNode? _parent { get; set; } = null;
        private AstTreeNode?[] _children { get; set; } = new AstTreeNode[MaxChildrenPerNode] { null, null };
        private Token? _token;

        public AstTreeNode(AstTreeNode parent, Token token)
        {
            _parent = parent;
            _token = token;
        }

        public AstTreeNode(Token token)
        {
            _token = token;
        }

        public AstTreeNode(Token token, params Token[] children)
        {
            _token = token;

            foreach (var child in children)
                AddChild(child);
        }

        public AstTreeNode()
        {
            _token = null;
        }

        public AstTreeNode AddChild(Token val)
        {
            int index = 0;
            while (_children[index] is not null && index <= MaxChildrenPerNode)
                index++;

            if (index >= MaxChildrenPerNode)
                throw new InvalidDataException($"A expressão é inválida, o operador {_token.OperatorType.ToString()} está operando em mais de 2 variáveis");

            var child = new AstTreeNode(parent: this, token: val);
            _children[index] = child;
            return child;
        }

        public AstTreeNode AddChild(AstTreeNode val)
        {
            int index = 0;
            while (_children[index] is not null && index <= MaxChildrenPerNode)
                index++;

            if (index >= MaxChildrenPerNode)
                throw new InvalidDataException($"A expressão é inválida, o operador {_token.OperatorType.ToString()} está operando em mais de 2 variáveis");

            _children[index] = val;
            return val;
        }

        public AstTreeNode[] AddChildren(params Token[] vals)
        {
            if (vals.Length > 2)
                throw new InvalidOperationException($"Tentativa de inserir mais de 2 nós filhos em um nó pai");
            return vals.Select(AddChild).ToArray();
        }

        public AstTreeNode[] AddChildren(params AstTreeNode?[] vals)
        {
            if (vals.Length > 2)
                throw new InvalidOperationException($"Tentativa de inserir mais de 2 nós filhos em um nó pai");
            return _children = vals;
        }

        public bool RemoveChild(AstTreeNode child)
        {
            int index = 0;
            while (_children[index] != child)
                index++;

            if (index >= 2)
                return false;

            _children[index] = null;
            return true;
        }

        public Token GetNodeValue()
        {
            return _token;
        }

        public void TraverseAndExecute(Action<AstTreeNode> act)
        {
            if (_children[0] is not null)
                _children[0].TraverseAndExecute(act);

            if (_children.Length > 1 && _children[1] is not null)
                _children[1].TraverseAndExecute(act);

            act(this);
        }
        public bool GetCurrentResult()
        {
            GetCurrentResult(this);
            return this.GetNodeValue().GetValue();
        }

        public void GetCurrentResult(AstTreeNode node)
        {
            if (node._token.OperatorType == OPERATOR_TYPE.OPERADOR && node._children is null)
                return;
            if (node._token.OperatorType == OPERATOR_TYPE.OPERADOR)
            {
                AstTreeNode aVal = node._children[0], bVal = node._children[1];

                if (aVal._token.OperatorType == OPERATOR_TYPE.EXPRESSION_GROUPER)
                    GetCurrentResult(aVal);

                if (bVal._token.OperatorType == OPERATOR_TYPE.EXPRESSION_GROUPER)
                    GetCurrentResult(bVal);

                bool aTokenVal = aVal._token.GetValue(), bTokenVal = bVal._token.GetValue();

                if (node._token.Operator == "AND")
                    node._token.SetValue(aTokenVal && bTokenVal);
                else if (node._token.Operator == "OR")
                    node._token.SetValue(aTokenVal || bTokenVal);

                _parent?.GetNodeValue().SetValue(node._token.GetValue());
                return;
            }

            if (node._children[0] is not null)
                GetCurrentResult(node._children[0]);
            if (node._children[1] is not null)
                GetCurrentResult(node._children[1]);
        }

        public void PrintTree()
        {
            int leftDepth = GetDepth(0, this, TREE_DIRECTION.LEFT), right = GetDepth(0, this, TREE_DIRECTION.RIGHT);
            int maxDepth = Math.Max(leftDepth, right);
            string[] lines = new string[maxDepth * 3 + 2];

            var tabs = (maxDepth * 2 + 1);

            PrintTree(this, 0, tabs, lines, maxDepth);
            foreach (var line in lines)
            {
                Console.WriteLine(line);
            }
        }

        public void PrintTree(AstTreeNode node, int currDepth, int currPad, string[] lines, int maxDepth)
        {
            var pad = new String(' ', currPad);
            lines[currDepth] += $"{pad}{node._token.Operator}";
            currDepth++;
            lines[currDepth] += $"{pad}{(node._children[0] is not null ? "/" : "")}{(node._children[1] is not null ? "\\" : "")}";

            if (node._children[0] is not null)
                PrintTree(node._children[0], currDepth + 1, GetPadd(currPad, TREE_DIRECTION.LEFT), lines, maxDepth);
            if (node._children[1] is not null)
                PrintTree(node._children[1], currDepth + 1, GetPadd(currPad / 2, TREE_DIRECTION.RIGHT), lines, maxDepth);
        }

        private int GetPadd(int curr, TREE_DIRECTION direction)
        {
            if (direction == TREE_DIRECTION.LEFT)
                return curr--;
            if (direction == TREE_DIRECTION.RIGHT)
                return curr++;
            return curr;
        }

        private int GetDepth(int index, AstTreeNode? node, TREE_DIRECTION direction)
        {
            if (direction == TREE_DIRECTION.LEFT && node._children[0] is not null)
                return GetDepth(index + 1, node._children[0], direction);
            else if (direction == TREE_DIRECTION.RIGHT && node._children[1] is not null)
                return GetDepth(index + 1, node._children[1], direction);
            return index + 1;
        }

    }
}
