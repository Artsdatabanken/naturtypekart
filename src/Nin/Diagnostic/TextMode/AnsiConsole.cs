using System;

namespace Nin.Common.Diagnostic.TextMode
{
    public static class AnsiConsole
    {
        public static void WriteLine(AnsiColor color, string message)
        {
            System.Console.WriteLine($"{BoldOn}{color.Code}{message}{BoldOff}{AnsiColor.Black.Code}");
        }

        public static string WriteLine2(AnsiColor color, string message)
        {
            return $"{BoldOn}{color.Code}{message}{BoldOff}{AnsiColor.Black.Code}";
        }

        public static void WriteLineGreen(string message)
        {
            WriteColor(message, FgGreen);
        }

        public static void WriteLineRed(string message)
        {
            WriteColor(message, FgRed);
        }

        private static void WriteColor(string message, string color)
        {
            System.Console.WriteLine($"{BoldOn}{color}{message}{BoldOff}{FgBlack}");
        }

        const string BoldOn = "\x1b[1m";
        const string BoldOff = "\x1b[22m";
        const string FgRed = "\x1b[31m";
        const string FgGreen = "\x1b[32m";
        const string FgBlack = "\x1b[30m";
    }

    public class AnsiColor
    {
        public static readonly AnsiColor Red = new AnsiColor("\x1b[31m");
        public static readonly AnsiColor Yellow = new AnsiColor("\x1b[33m");
        public static readonly AnsiColor Green = new AnsiColor("\x1b[32m");
        public static readonly AnsiColor Black = new AnsiColor("\x1b[30m");
        public static readonly AnsiColor Bold = new AnsiColor("\x1b[1m");
        public static readonly AnsiColor Regular = new AnsiColor("\x1b[22m");

        public readonly string Code;

        private AnsiColor(string color)
        {
            Code = color;
        }
    }
}