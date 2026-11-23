using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.Swift;
using System.Text;

namespace LogicalExpressions.Parser
{

    public class Parser
    {

        public List<Token> Parse(string expression)
        {
            var result = new List<Token>();
            var groupStack = new Stack<Token>();
            var operatorStack = new Stack<Token>();

            for (int i = 0; i < expression.Length; i++)
            {
                var digit = expression[i];
                if (digit == '(')
                {
                    var newToken = new Token(type: OPERATOR_TYPE.EXPRESSION_GROUPER, operatorName: digit, result.Count);
                    result.Add(newToken);
                    groupStack.Push(newToken);
                }
                else if (digit == ')')
                {
                    result.Add(new(type: OPERATOR_TYPE.EXPRESSION_GROUPER, operatorName: digit, result.Count));
                    groupStack.Pop();
                }

                else if (char.IsLetter(digit))
                {
                    i = PeekNext(expression, i, result, groupStack, operatorStack);
                }
                else if (digit == ' ')
                    continue;
                else
                    throw new InvalidDataException($"A expressão é inválida - {digit} não é uma letra ou parêntese");
            }
            if (groupStack.Count > 0)
                throw new InvalidDataException("A expressão é inválida - há grupos que não fecham os parênteses");
            if (operatorStack.Count > 0)
                throw new InvalidDataException("A expressão é inválida - há operadores que não relacionam variáveis");

            if (!result.Any(x => x.OperatorType == OPERATOR_TYPE.OPERADOR))
                throw new InvalidDataException("A expressão é pois não possui operadores");
            if (!result.Any(x => x.OperatorType == OPERATOR_TYPE.VARIABLE))
                throw new InvalidDataException("A expressão é pois não possui variáveis");

            return result;
        }

        private int PeekNext(string expression, int index, List<Token> result, Stack<Token> groupStack, Stack<Token> operatorStack)
        {
            int start = index;
            while (index < expression.Length && index + 1 < expression.Length && char.IsLetter(expression[index + 1]))
            {
                index++;
            }

            var currentToken = expression.Substring(start, index - start + 1).Trim().ToUpper();

            if (currentToken == "OR" || currentToken == "AND" || currentToken == "NOT")
            {
                var newToken = new Token(type: OPERATOR_TYPE.OPERADOR, operatorName: currentToken, result.Count, groupStack.Count);
                result.Add(newToken);
                operatorStack.Push(newToken);

                if (result.Count == 1 && newToken.Operator != "NOT")
                {
                    throw new InvalidDataException($"Um operador lógico não pode iniciar uma expressão, ({currentToken}, em {newToken.Index})");
                }
                if (result.Count >= 2 && currentToken != "NOT")
                {
                    var predecessor = result[result.Count - 2];
                    if (predecessor.OperatorType == OPERATOR_TYPE.OPERADOR)
                        throw new InvalidDataException($"A expressão é inválida - o operador {currentToken}, {index},não é precedido uma variável.");
                }
            }
            else if (currentToken.Length == 1)
            {
                var newToken = new Token(type: OPERATOR_TYPE.VARIABLE, operatorName: currentToken, result.Count);
                result.Add(newToken);

                if (result.Count >= 2)
                {
                    var predecessor = result[result.Count - 2];
                    if (predecessor.OperatorType == OPERATOR_TYPE.VARIABLE)
                        throw new InvalidDataException($"A expressão é inválida - a variável {newToken.Operator} é precedido por outra variável ( {predecessor.Operator} )");
                    else if (operatorStack.Count == 0 && groupStack.Count == 0)
                        throw new InvalidDataException($"A expressão é inválida, a variável {newToken.Operator} não está em um grupo ou operação");
                    else if (operatorStack.Count > 0)
                        operatorStack.Pop();
                }
            }
            else
                throw new InvalidDataException($"A expressão é inválida - {currentToken} não é um operador lógico válido");

            return index;
        }
    }
    public enum OPERATOR_TYPE
    {
        OPERADOR,
        EXPRESSION_GROUPER,
        VARIABLE,
    }

    public class Token
    {
        private bool Value { get; set; }
        public int InGroupDepth { get; set; }

        public Token(OPERATOR_TYPE type, string operatorName, int index)
        {
            OperatorType = type;
            Operator = operatorName;
            Index = index;
        }
        public Token(OPERATOR_TYPE type, string operatorName, int index, int inGroupDepth)
        {
            OperatorType = type;
            Operator = operatorName;
            Index = index;
            InGroupDepth = inGroupDepth;
        }
        public Token(OPERATOR_TYPE type, char operatorName, int index)
        {
            OperatorType = type;
            Operator = operatorName.ToString();
            Index = index;
        }
        public OPERATOR_TYPE OperatorType { get; set; }
        public string Operator { get; set; } = string.Empty;
        public int Index { get; set; }

        public void SetValue(bool val)
        {
            Value = val;
        }

        public bool GetValue()
        {
            return Value;
        }
    }
}
