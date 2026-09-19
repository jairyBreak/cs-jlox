namespace CraftingInterpreters.Lox;

using System;
using System.IO;


public class Lox {
    static bool hadError = false;
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

        foreach (Token token in tokens)
        {
            Console.WriteLine(token);
        }
    }

    public static void Error(int line, string message)
    {
        Report(line, "", message);
    }

    private static void Report(int line, string where, string message)
    {
        Console.Error.WriteLine("[line " + line + "] Error" + where + ": " + message);
        hadError = true;
    }


}

