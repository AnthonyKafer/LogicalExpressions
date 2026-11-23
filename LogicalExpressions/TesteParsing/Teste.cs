using LogicalExpressions.Parser;
using System;
using System.Collections.Generic;
using System.Text;

namespace LogicalExpressions.TesteParsing
{
    internal class Teste
    {
        private Parser.Parser _parser = new();

        public void RunTeste()
        {
            PrintTitle("Rodando testes de parse correto...");
            TesteParsing();
            PrintTitle("Todos os testes de testes de parse passaram.");

            Console.WriteLine();
            Console.WriteLine();

            PrintTitle("Rodando testes de detecção de expressões inválidas...");
            TestParsing_CheckCatchInvalidExpression();
            PrintTitle("Todos os testes de detecção de expressões inválidas passaram.");
        }
        public void TesteParsing()
        {
            foreach (var (testIndex, expression, expectedTokens) in TestCases_Success)
            {
                var generatedTokens = _parser.Parse(expression);

                if (generatedTokens.Count != expectedTokens.Count)
                {
                    PrintFail($"Teste: {testIndex} Falhou, a quantidade de tokens não coincide");
                    return;
                }

                for (int i = 0; i < generatedTokens.Count - 1; i++)
                {
                    Token currGenerated = generatedTokens[i], currExpected = expectedTokens[i];
                    if (currGenerated.Index != currExpected.Index)
                    {
                        PrintFail($"Teste: {testIndex} falhou - Índice esperado: {currExpected.Index}. Índice Gerado {currGenerated.Index}");
                        return;
                    }
                    if (currGenerated.Operator != currExpected.Operator)
                    {
                        PrintFail($"Teste: {testIndex} falhou - Operador esperado: {currExpected.Operator}. Operador Gerado {currGenerated.Operator}");
                        return;
                    }
                    if (currGenerated.OperatorType != currExpected.OperatorType)
                    {
                        PrintFail($"Teste: {testIndex} falhou - OperatorType esperado: {currExpected.OperatorType}. OperatorType Gerado {currGenerated.OperatorType}");
                        return;
                    }
                }

                PrintSuccess($"Teste: {testIndex} passou");
            }
        }

        public void TestParsing_CheckCatchInvalidExpression()
        {

            foreach (var (testIndex, expression, expectedError) in TestCases_ExpectFails)
            {
                try
                {
                    _parser.Parse(expression);
                    PrintFail($"Teste: {testIndex} falhou - nenhuma exceção lançada");
                }
                catch (Exception ex)
                {
                    if (ex.Message == expectedError)
                    {
                        PrintSuccess($"Teste: {testIndex} passou");
                    }
                }
            }
        }

        public void PrintTitle(string message)
        {
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"\t{message}");
            Console.ResetColor();
        }

        public void PrintFail(string message)
        {
            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"\t\t{message}");
            Console.ResetColor();
        }
        public void PrintSuccess(string message)
        {
            Console.BackgroundColor = ConsoleColor.DarkGreen;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\t\t{message}");
            Console.ResetColor();
        }

        /// <summary>
        /// Teste case de parse de expressões
        /// </summary>
        private List<(string testName, string expression, string expectedError)> TestCases_ExpectFails => new()
        {
            ("1","a b OR c AND z", "A expressão é inválida - a variável B é precedido por outra variável ( A )"),
            ("2","((a AND b)", "A expressão é inválida - há grupos que não fecham os parênteses"),
            ("3","(a OR b) AND AND", "A expressão é inválida - o operador AND, 15,não é precedido uma variável."),
            ("4","a AND ((a OR b) Z", "A expressão é inválida - há grupos que não fecham os parênteses"),
            ("5","AND ((a OR b) Z", "Um operador lógico não pode iniciar uma expressão, (AND, em 0)"),
            ("6","((AND (a OR b) OR Z", "A expressão é inválida - há grupos que não fecham os parênteses"),
            ("7","(a OR b) Z", "A expressão é inválida, a variável Z não está em um grupo ou operação"),
        };

        private List<(string testName, string expression, List<Token> tokens)> TestCases_Success => new()
        {
            ("1","((a AND b) OR c) AND z", new(){
                new Token(OPERATOR_TYPE.EXPRESSION_GROUPER, '(', 0),
                new Token(OPERATOR_TYPE.EXPRESSION_GROUPER, '(', 1),
                new Token(OPERATOR_TYPE.VARIABLE, 'A', 2),
                new Token(OPERATOR_TYPE.OPERADOR, "AND", 3),
                new Token(OPERATOR_TYPE.VARIABLE, 'B', 4),
                new Token(OPERATOR_TYPE.EXPRESSION_GROUPER, ')',5),
                new Token(OPERATOR_TYPE.OPERADOR, "OR", 6),
                new Token(OPERATOR_TYPE.VARIABLE, 'C', 7),
                new Token(OPERATOR_TYPE.EXPRESSION_GROUPER, ')', 8),
                new Token(OPERATOR_TYPE.OPERADOR, "AND", 9),
                new Token(OPERATOR_TYPE.VARIABLE, 'Z', 10),
            }),
            ("2","a AND b", new(){
                new Token(OPERATOR_TYPE.VARIABLE, 'A', 0),
                new Token(OPERATOR_TYPE.OPERADOR, "AND", 1),
                new Token(OPERATOR_TYPE.VARIABLE, 'B', 2),
            }),
            ("3","a OR b", new(){
                new Token(OPERATOR_TYPE.VARIABLE, 'A', 0),
                new Token(OPERATOR_TYPE.OPERADOR, "OR", 1),
                new Token(OPERATOR_TYPE.VARIABLE, 'B', 2),
            }),
            ("4","(a AND (b OR c)) AND (z OR x)", new(){
                new Token(OPERATOR_TYPE.EXPRESSION_GROUPER, '(', 0),
                new Token(OPERATOR_TYPE.VARIABLE, 'A', 1),
                new Token(OPERATOR_TYPE.OPERADOR, "AND", 2),
                new Token(OPERATOR_TYPE.EXPRESSION_GROUPER, '(', 3),
                new Token(OPERATOR_TYPE.VARIABLE, 'B', 4),
                new Token(OPERATOR_TYPE.OPERADOR, "OR",5),
                new Token(OPERATOR_TYPE.VARIABLE, 'C', 6),
                new Token(OPERATOR_TYPE.EXPRESSION_GROUPER, ')', 7),
                new Token(OPERATOR_TYPE.EXPRESSION_GROUPER, ')', 8),
                new Token(OPERATOR_TYPE.OPERADOR, "AND", 9),
                new Token(OPERATOR_TYPE.EXPRESSION_GROUPER, '(', 10),
                new Token(OPERATOR_TYPE.VARIABLE, 'Z', 11),
                new Token(OPERATOR_TYPE.OPERADOR, "OR", 12),
                new Token(OPERATOR_TYPE.VARIABLE, 'X', 13),
                new Token(OPERATOR_TYPE.EXPRESSION_GROUPER, ')', 14),
            }),
        };
    }
}
