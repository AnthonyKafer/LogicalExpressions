using System;
using System.Collections.Generic;
using System.Text;

namespace LogicalExpressions.PrintHelpers
{
    public static class PrintHelper
    {
        public static string Green = "\u001b[32m";
        public static string Red = "\u001b[31m";
        public static string Yellow = "\u001b[33m";
        public static string Blue = "\u001b[34m";
        public static string Reset = "\u001b[0m";
        public static string Underline = "\u001b[4m";
        public static string Bold = "\u001b[1m";
        public static string Italic = "\x1b[3m";
        public static string Cyan = "\u001b[36m";
    }
}
