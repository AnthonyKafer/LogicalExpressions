
using LogicalExpressions.AST;
using LogicalExpressions.Evaluator;
using LogicalExpressions.Parser;

public static class MainClass
{
    public static void Main(string[] args)
    {
        var parser = new Parser();
        var treeMaker = new AstEvaluator();

        while (true)
        {
            Console.WriteLine("Digite uma expressão:");
            var expression = Console.ReadLine();
            Console.Clear();
            try
            {
                var tokens = parser.Parse(expression);
                var tree = treeMaker.MountAstTree(tokens);
                tree.PrintTree();
                tree.EvaluateAstTree();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}