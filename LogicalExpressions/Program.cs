
using LogicalExpressions.AST;
using LogicalExpressions.Evaluator;
using LogicalExpressions.Parser;
using LogicalExpressions.PrintHelpers;

public static class MainClass
{
    public static void Main(string[] args)
    {
        MainAsync(args).Wait();
    }

    public static async Task MainAsync(string[] args)
    {
        var parser = new Parser();
        var treeMaker = new AstEvaluator();

        while (true)
        {
            Console.WriteLine("Digite uma expressão:");
            var command = Console.ReadLine();
            if (command == "cls" || command == "clear")
            {
                Console.Clear();
                continue;
            }
            Console.Clear();
            try
            {
                var tokens = parser.Parse(command);
                var tree = treeMaker.MountAstTree(tokens);
                tree.PrintTree();
                Console.WriteLine();
                Console.WriteLine($"{PrintHelper.Green}{new String('-', 20)}{PrintHelper.Reset}");
                Console.WriteLine();
                await tree.EvaluateAstTree(tokens);
                Console.WriteLine($"{PrintHelper.Green}{new String('-', 20)}{PrintHelper.Reset}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}