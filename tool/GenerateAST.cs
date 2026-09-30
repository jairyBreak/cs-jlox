namespace CraftingInterpreters.tool;

public class GenerateAST
{
    
    public static void Main(string[] args){
        if(args.Length != 1){
            Console.Error.WriteLine("Usage : <output directory> ");
            Environment.Exit(64);
        }
        string OutputDir = args[0];
        DefineAST(OutputDir, "Expr", new List<string>
        { 
            "Binary   : Expr left, Token op, Expr right",
            "Grouping : Expr expression",
            "Literal  : object? value",
            "Unary    : Token op, Expr right"
        });
    }
    
    private static void DefineAST(string outputDir, string baseName, List<string> types)
    {
        string path = outputDir + "/" + baseName + ".cs";

        using var writer = new StreamWriter(path);
        writer.WriteLine("namespace CraftingInterpreters.Lox;");
        writer.WriteLine("");
        writer.WriteLine("public abstract class " + baseName + "{");

        DefineVisitor(writer, baseName, types);

        foreach(string type in types)
        {
            string className = type.Split(':')[0].Trim();
            string field = type.Split(':')[1].Trim();

            DefineType(writer, baseName, className, field); 
        }

        writer.WriteLine("");
        writer.WriteLine("  public abstract R Accept<R>(IVisitor<R> visitor);");
        writer.WriteLine("}");
        writer.Close();
    }

    private static void DefineVisitor(StreamWriter writer, string baseName, List<string> types)
    {
        writer.WriteLine("    public interface IVisitor<R>");
        writer.WriteLine("    {");
        foreach (string type in types)
        {
            string typeName = type.Split(':')[0].Trim();
            writer.WriteLine($"        R Visit{typeName}{baseName}({typeName} {baseName.ToLower()});");
        }

        writer.WriteLine("    }");
    }
    private static void DefineType(StreamWriter writer, string baseName, string className, string fieldList)
    {
        writer.WriteLine("  public class " + className + " : " + baseName + " {");
        writer.WriteLine("    public " + className + "(" + fieldList + ") {");
        string[] fields = fieldList.Split(", ");
        foreach(string field in fields)
        {
            string name = field.Split(" ")[1];
            writer.WriteLine("      this." + name + " = " + name + ";");
        }

        writer.WriteLine("    }");

        writer.WriteLine("");
        writer.WriteLine("  public override R Accept<R>(IVisitor<R> visitor){");
        writer.WriteLine($"      return visitor.Visit{className}{baseName}(this);");
        writer.WriteLine("  }");

        writer.WriteLine("");
        foreach(string field in fields)
        {
            writer.WriteLine("  public readonly " + field + ";");
        }

        writer.WriteLine("}");
    }
}