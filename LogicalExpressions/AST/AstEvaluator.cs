using LogicalExpressions.Parser;
using System.Text;

namespace LogicalExpressions.AST
{
    public class AstEvaluator
    {
        private string[] _operatorPrecedence = new string[] { "AND", "OR" };
        private HashSet<int> _seenIndexes = new();
        public AstTreeNode MountAstTree(List<Token> tokens)
        {
            AstTreeNode root = new AstTreeNode(FindGreatesPrecedence(tokens));
            var rootToken = root.GetNodeValue();

            int left = rootToken.Index - 1, right = rootToken.Index + 1;

            root.AddChildren(
                FillTree(root, tokens, left, TREE_DIRECTION.LEFT),
                FillTree(root, tokens, right, TREE_DIRECTION.RIGHT)
                );

            return root;
        }

        public void TestePrint(AstTreeNode root)
        {
            var sb = new StringBuilder();
            List<Token> sla = new();

            void coloring(List<Token> tokens)
            {
                foreach (var tk in tokens)
                {
                    if (tk.OperatorType == OPERATOR_TYPE.EXPRESSION_GROUPER)
                        sb.Append("\u001b[33m");

                    else if (tk.OperatorType == OPERATOR_TYPE.VARIABLE)
                        sb.Append("\u001b[31m");

                    else if (tk.OperatorType == OPERATOR_TYPE.OPERADOR)
                        sb.Append("\u001b[32m");

                    sb.Append($" {tk.Operator} ");
                    sb.Append("\u001b[0m");
                }
            }
            void Append(AstTreeNode act)
            {
                sla.Add(act.GetNodeValue());
            }

            root.TraverseAndExecute(Append);
            var order = sla.OrderBy(x => x.Index)
                .ToList();

            coloring(order);
            var str = sb.ToString();
            Console.WriteLine(str);
        }

        private AstTreeNode? FillTree(AstTreeNode root, List<Token> tokens, int index, TREE_DIRECTION direction)
        {
            var thisMasterToken = tokens[index];
            var thisMasterRoot = new AstTreeNode(root, thisMasterToken);

            if (index == tokens.Count - 1 || index == 0)
                return thisMasterRoot;

            if (thisMasterToken.OperatorType == OPERATOR_TYPE.VARIABLE)
                return thisMasterRoot;

            if (thisMasterToken.OperatorType == OPERATOR_TYPE.EXPRESSION_GROUPER)
            {
                var closingGroupChildIndex = SetNewIndex(index, direction);
                var operatorIndex = SetNewIndex(index, direction);

                while (validateContinuePeeking(tokens, closingGroupChildIndex, direction) && tokens[closingGroupChildIndex].OperatorType != OPERATOR_TYPE.EXPRESSION_GROUPER)
                    closingGroupChildIndex = SetNewIndex(closingGroupChildIndex, direction);

                while (validateContinuePeeking(tokens, operatorIndex, direction) && tokens[operatorIndex].OperatorType != OPERATOR_TYPE.OPERADOR)
                    operatorIndex = SetNewIndex(operatorIndex, direction);

                var closingGroup = FillTree(thisMasterRoot, tokens, closingGroupChildIndex, direction);
                var operadorChild = FillTree(thisMasterRoot, tokens, operatorIndex, direction);
                thisMasterRoot.AddChildren(closingGroup, operadorChild);
            }

            if (thisMasterToken.OperatorType == OPERATOR_TYPE.OPERADOR)
            {
                var leftChild = FillTree(thisMasterRoot, tokens, SetNewIndex(index, TREE_DIRECTION.LEFT), direction);
                var rightChild = FillTree(thisMasterRoot, tokens, SetNewIndex(index, TREE_DIRECTION.RIGHT), direction);

                thisMasterRoot.AddChildren(leftChild, rightChild);
            }

            return thisMasterRoot;
        }

        private bool validateContinuePeeking(List<Token> tokens, int index, TREE_DIRECTION direction)
        {
            if (direction == TREE_DIRECTION.LEFT)
                return index > 0;
            else if (direction == TREE_DIRECTION.RIGHT)
                return index < tokens.Count - 1;
            return false;
        }

        private int SetNewIndex(int index, TREE_DIRECTION direction)
        {
            if (direction == TREE_DIRECTION.LEFT)
                return index - 1;
            else if (direction == TREE_DIRECTION.RIGHT)
                return index + 1;
            return 0;
        }

        private Token FindGreatesPrecedence(List<Token> tokens)
        {
            return tokens.Where(x => x.OperatorType == OPERATOR_TYPE.OPERADOR)
               .OrderBy(x => _operatorPrecedence.IndexOf(x.Operator)).ToList()
               .OrderBy(x => x.InGroupDepth)
               .First();
        }

        public enum TREE_DIRECTION
        {
            LEFT,
            RIGHT
        }
    }
}
