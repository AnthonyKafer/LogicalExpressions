using LogicalExpressions.AST;
using LogicalExpressions.Parser;
using System;
using System.Collections.Generic;
using System.Text;

namespace LogicalExpressions.Evaluator
{
    public static class Evaluator
    {

        public static void EvaluateAstTree(this AstTreeNode root)
        {
            var tokens = new List<Token>();

            void FillTokens(AstTreeNode node)
            {
                tokens.Add(node.GetNodeValue());
            }

            root.TraverseAndExecute(FillTokens);

            tokens = tokens.OrderBy(x => x.Index).ToList();

            var variables = tokens
                .Where(x => x.OperatorType == OPERATOR_TYPE.VARIABLE)
                .GroupBy(x => x.Operator)
                .Select(x => x.First())
                .ToList();

            var combinations = GenerateTruthTable(variables.Count);

            foreach (var row in combinations)
            {
                var variableValues = new Dictionary<string, bool>();

                for (int i = 0; i < variables.Count; i++)
                {
                    variableValues[variables[i].Operator] = row[i];
                }

                foreach (var token in tokens)
                {
                    if (token.OperatorType == OPERATOR_TYPE.VARIABLE)
                    {
                        token.SetValue(variableValues[token.Operator]);
                        Console.Write($" {variableValues[token.Operator]} ");
                    }
                    else
                    {
                        Console.Write($" {token.Operator} ");
                    }
                }
                Console.Write($" = {root.GetCurrentResult()}");
                Console.WriteLine();
            }
        }


        public static List<bool[]> GenerateTruthTable(int variableCount)
        {
            int combinations = 1 << variableCount;

            var result = new List<bool[]>();

            for (int i = 0; i < combinations; i++)
            {
                var row = new bool[variableCount];

                for (int j = 0; j < variableCount; j++)
                {
                    row[j] = (i & (1 << (variableCount - 1 - j))) != 0;
                }

                result.Add(row);
            }

            return result;
        }
    }
}
