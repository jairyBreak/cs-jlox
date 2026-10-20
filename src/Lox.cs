namespace CraftingInterpreters.Lox;

using System;
using System.IO;


public class Lox {
    private static readonly Interpreter interpreter = new Interpreter();
    static bool hadError = false;
    static bool hadRuntimeError = true;
    public static void Main(string[] args)
    {
        if (args.Length > 1)
        {
            Console.WriteLine("wtf: 1 arg");
            Environment.Exit(64);
        }
        else if (args.Length == 1)
        {
            RunFile(args[0]);
        }
        else
        {
            RunPrompt();
        }
    }

    private static void RunFile(string filename)
    {
        string source = File.ReadAllText(filename);
        Run(source);

        if(hadError){Environment.Exit(65);}
        if(hadRuntimeError){Environment.Exit(70);}
    }

    private static void RunPrompt()
    {
        while (true)
        {
            Console.Write("> ");
            string? line = Console.ReadLine();
            if (line == null){ break;}
            Run(line);
            hadError = false;
        }
    }

    private static void Run(string source)
    {
        var scanner = new Scanner(source);
        List<Token> tokens = scanner.ScanTokens();

        Parser parser = new Parser(tokens);
        List<Stmt> statements = parser.Parse();

        if (hadError) return;

        interpreter.Interpret(statements);
    }

    public static void Error(int line, string message)
    {
        Report(line, "", message);
    }

    public static void RuntimeError(RuntimeError error)
    {
        Console.WriteLine($"{error.Message}\n[line {error.token.line}]");
        hadRuntimeError = true;
    }

    private static void Report(int line, string where, string message)
    {
        Console.Error.WriteLine("[line " + line + "] Error" + where + ": " + message);
        hadError = true;
    }

    public static void Error(Token token, string message)
    {
        if(token.type == TokenType.EOF){
            Report(token.line, "at end", message);
        }
        else
        {
            Report(token.line," at '" + token.lexeme + "'", message);
        }
    }
}

