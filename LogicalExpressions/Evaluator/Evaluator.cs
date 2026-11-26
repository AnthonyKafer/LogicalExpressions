using LogicalExpressions.AST;
using LogicalExpressions.Parser;
using LogicalExpressions.PrintHelpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace LogicalExpressions.Evaluator
{
    public static class Evaluator
    {
        public static async Task EvaluateAstTree(this AstTreeNode root, List<Token> tokens)
        {
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
                        var currVal = variableValues[token.Operator];
                        token.SetValue(currVal);
                        Console.Write($"{PrintHelper.Yellow}{token.Operator}{PrintHelper.Reset}= {PrintHelper.Italic}{(currVal ? PrintHelper.Green : PrintHelper.Red)}{variableValues[token.Operator]}{PrintHelper.Reset}");
                    }
                    else if (token.OperatorType == OPERATOR_TYPE.EXPRESSION_GROUPER)
                    {
                        Console.Write($"{PrintHelper.Blue}{token.Operator}{PrintHelper.Reset}");
                    }
                    else if (token.Operator == "NOT")
                    {
                        Console.Write($"{PrintHelper.Cyan} {token.Operator} ");
                    }
                    else
                    {
                        Console.Write($" {token.Operator} ");
                    }
                }
                var val = root.GetCurrentResult();
                Console.Write($" = {(val ? PrintHelper.BrightGreen : PrintHelper.BrightRed)}{val}{PrintHelper.Reset}");
                Console.WriteLine();
                await Task.Delay(100);
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
