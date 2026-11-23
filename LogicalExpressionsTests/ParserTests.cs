
using LogicalExpressions.Parser;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using System.Data;
namespace LogicalExpressionsTests
{
    [TestClass]
    public sealed class ParserTests
    {
        public static IEnumerable<object[]> TestCases => new[]
        {
           new object []{
               "", new List<Token>() },
           new object []{
               "", new List<Token>() },
           new object []{
               "", new List<Token>() },
           new object []{
               "", new List<Token>() },
           new object []{
               "", new List<Token>() },
        }

        public void TestTokenParsing()
        {

        }
    }
}
