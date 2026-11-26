using LogicalExpressions.Parser;
using LogicalExpressions.PrintHelpers;
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
            var distinct = new HashSet<AstTreeNode>();
            TraverseAndExecute(act, this, distinct);
        }
        public void TraverseAndExecute(Action<AstTreeNode> act, AstTreeNode node, HashSet<AstTreeNode> visiteds)
        {
            if (node is null)
                return;
            if (visiteds.Contains(node))
                return;
            else
                visiteds.Add(node);

            foreach (var c in node._children)
                TraverseAndExecute(act, c, visiteds);
            act(node);
        }
        public bool GetCurrentResult()
        {
            GetCurrentResult(this);
            return this.GetNodeValue().GetValue();
        }

        public void GetCurrentResult(AstTreeNode node)
        {
            if (node is null)
                return;
            if (node._token.OperatorType == OPERATOR_TYPE.EXPRESSION_GROUPER &&
                node._children[0] is null && node._children[1] is null)
                return;
            if (node._token.OperatorType == OPERATOR_TYPE.VARIABLE)
                return;

            if (node._token.OperatorType == OPERATOR_TYPE.OPERADOR && node._token.Operator == "NOT")
            {
                GetCurrentResult(node._children[0]);
                var tokenToInvert = node._children[0].GetNodeValue();
                var val = !tokenToInvert.GetValue();
                node._token.SetValue(val);
                return;
            }

            AstTreeNode aVal = node._children[0], bVal = node._children[1];

            bool shouldFetchA = shouldFetchDeeper(aVal);
            bool shouldFetchB = shouldFetchDeeper(bVal);

            if (shouldFetchA)
                GetCurrentResult(aVal);

            if (shouldFetchB)
                GetCurrentResult(bVal);

            if (!shouldFetchA && aVal._token.OperatorType != OPERATOR_TYPE.VARIABLE)
            {
                aVal._token.SetValue(bVal._token.GetValue());
            }
            if (!shouldFetchB && bVal._token.OperatorType != OPERATOR_TYPE.VARIABLE)
            {
                bVal._token.SetValue(aVal._token.GetValue());
            }

            bool aTokenVal = aVal._token.GetValue(), bTokenVal = bVal._token.GetValue();

            if (node._token.Operator == "AND")
                node._token.SetValue(aTokenVal && bTokenVal);
            else if (node._token.Operator == "OR")
                node._token.SetValue(aTokenVal || bTokenVal);

            node._parent?.GetNodeValue().SetValue(node._token.GetValue());
            return;
        }

        private bool shouldFetchDeeper(AstTreeNode node)
        {
            return node is not null && (node._children[0] is not null || node._children[1] is not null);
        }

        public void PrintTree()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            int depth = GetDepth(this);
            int labelMax = MaxLabel(this);
            int spacing = labelMax + 1;
            int width = CountNodes(this) * spacing + labelMax;
            int height = depth * 2 - 1;

            var canvas = new char[height, width];
            var colors = new string[height, width];

            for (int r = 0; r < height; r++)
                for (int c = 0; c < width; c++)
                {
                    canvas[r, c] = ' ';
                    colors[r, c] = PrintHelper.Reset;
                }

            int counter = 0;
            Layout(this, 0, ref counter, spacing, labelMax / 2, canvas, colors);

            var sb = new System.Text.StringBuilder();
            for (int r = 0; r < height; r++)
            {
                int last = width - 1;
                while (last >= 0 && canvas[r, last] == ' ') last--;

                string current = PrintHelper.Reset;
                for (int c = 0; c <= last; c++)
                {
                    if (colors[r, c] != current)
                    {
                        current = colors[r, c];
                        sb.Append(current);
                    }
                    sb.Append(canvas[r, c]);
                }
                sb.Append(PrintHelper.Reset).AppendLine();
            }
            Console.Write(sb.ToString());
        }

        private static AstTreeNode Child(AstTreeNode node, int i) =>
            node?._children != null && i < node._children.Length ? node._children[i] : null;

        private static string Label(AstTreeNode node) => node._token.Operator.ToString();

        private int GetDepth(AstTreeNode node) =>
            node == null ? 0 : 1 + Math.Max(GetDepth(Child(node, 0)), GetDepth(Child(node, 1)));

        private int CountNodes(AstTreeNode node) =>
            node == null ? 0 : 1 + CountNodes(Child(node, 0)) + CountNodes(Child(node, 1));

        private int MaxLabel(AstTreeNode node) =>
            node == null ? 0 : Math.Max(Label(node).Length,
                Math.Max(MaxLabel(Child(node, 0)), MaxLabel(Child(node, 1))));

        private int Layout(AstTreeNode node, int level, ref int counter,
                           int spacing, int offset, char[,] canvas, string[,] colors)
        {
            if (node == null) return -1;

            int leftCol = Layout(Child(node, 0), level + 1, ref counter, spacing, offset, canvas, colors);
            int col = counter++ * spacing + offset;
            int rightCol = Layout(Child(node, 1), level + 1, ref counter, spacing, offset, canvas, colors);

            int row = level * 2;

            string value = Label(node);
            string nodeColor = node._token.OperatorType != OPERATOR_TYPE.VARIABLE
                ? PrintHelper.Yellow + PrintHelper.Bold
                : PrintHelper.Red;

            int start = col - value.Length / 2;
            for (int i = 0; i < value.Length; i++)
            {
                canvas[row, start + i] = value[i];
                colors[row, start + i] = nodeColor;
            }

            if (leftCol >= 0 || rightCol >= 0)
            {
                int r = row + 1;

                if (leftCol >= 0)
                    for (int c = leftCol; c <= col; c++) { canvas[r, c] = '─'; colors[r, c] = PrintHelper.Green; }

                if (rightCol >= 0)
                    for (int c = col; c <= rightCol; c++) { canvas[r, c] = '─'; colors[r, c] = PrintHelper.Green; }

                if (leftCol >= 0) canvas[r, leftCol] = '┌';
                if (rightCol >= 0) canvas[r, rightCol] = '┐';

                canvas[r, col] = (leftCol >= 0 && rightCol >= 0) ? '┴'
                               : (leftCol >= 0) ? '┘'
                                                                  : '└';
                colors[r, col] = PrintHelper.Green;
            }

            return col;
        }

        private int GetNodesNumber(int index, AstTreeNode? node)
        {
            int left = 0, right = 0;
            if (node._children[0] is not null)
                left = GetNodesNumber(index + 1, node._children[0], TREE_DIRECTION.LEFT);
            if (node._children[1] is not null)
                right = GetNodesNumber(index + 1, node._children[1], TREE_DIRECTION.RIGHT);

            return left + right;
        }

        private int GetNodesNumber(int index, AstTreeNode? node, TREE_DIRECTION direction)
        {
            if (node._children[0] is not null)
                return index + GetNodesNumber(index + 1, node._children[0]);
            if (node._children[1] is not null)
                return index + GetNodesNumber(index + 1, node._children[1]);
            return index;
        }

    }
}
