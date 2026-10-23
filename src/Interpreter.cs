using Microsoft.Win32.SafeHandles;

namespace CraftingInterpreters.Lox;

public class Interpreter : Expr.IVisitor<object>, Stmt.IVisitor<object?>
{
    private Environ environment = new Environ();
    public void Interpret(List<Stmt> statements)
    {
        try
        {
            foreach (Stmt statement in statements)
            {
                Execute(statement);
            }
        }
        catch (RuntimeError error)
        {
            Lox.RuntimeError(error);
        }
    }

    public object VisitLiteralExpr(Expr.Literal expr)
    {
        return expr.value;
    }

    public object VisitGroupingExpr(Expr.Grouping expr)
    {
        return Evaluate(expr.expression);
    }

    private object Evaluate(Expr expr)
    {
        return expr.Accept(this);
    }

    private void Execute(Stmt stmt)
    {
        stmt.Accept(this);
    }

    void ExecuteBlock(List<Stmt> statements, Environ environment)
    {
        Environ previous = this.environment;
        try
        {
            this.environment = environment;
            foreach (Stmt statement in statements)
            {
                Execute(statement);
            }
        } 
        finally
        {
            this.environment = previous;    
        }
    }

    public object? VisitBlockStmt(Stmt.Block stmt)
    {
        ExecuteBlock(stmt.statements, new Environ(environment));
        return null;
    }



    public object? VisitExpressionStmt(Stmt.Expression stmt)
    {
        Evaluate(stmt.expression);
        return null;
    }

    public object? VisitPrintStmt(Stmt.Print stmt)
    {
        object value = Evaluate(stmt.expression);
        Console.WriteLine(Stringify(value));
        return null;
    }

    public object? VisitVarStmt(Stmt.Var stmt)
    {
        object? value = null;
        if(stmt.initializer != null)
        {
            value = Evaluate(stmt.initializer);
        }

        environment.Define(stmt.name.lexeme ,value);
        return null;
    }
    
    public object VisitAssignExpr(Expr.Assign expr)
    {
        object value = Evaluate(expr.value);
        environment.Assign(expr.name, value);
        return value;
    }

    public object VisitUnaryExpr(Expr.Unary expr)
    {
        object right = Evaluate(expr.right);

        switch (expr.op.type)
        {
            case TokenType.BANG:
                return !IsTruthy(right);
            case TokenType.MINUS: 
                CheckNumberOperand(expr.op, right);
                return -(double)right;
        }

        return null;
    }

    public object VisitVariableExpr(Expr.Variable expr)
    {
        return environment.Get(expr.name);
    }

    private void CheckNumberOperand(Token op, object operand)
    {
        if (operand is double) return;
        throw new RuntimeError(op, "Operand must be a number!");
    }

    private void CheckNumberOperands(Token op, object left, object right)
    {
        if(left is double && right is double) return;
        throw new RuntimeError(op, "Operand must be numbers!");
    }

    private bool IsTruthy(object? Object)
    {
        if (Object == null) return false;
        if (Object is bool) return (bool)Object;
        return true;
    }

    private bool IsEqual(object a, object b)
    {
        if(a == null && b == null) return true;
        if(a == null) return false;
        return a.Equals(b);
    }

    private string Stringify(object Object)
    {
        if (Object == null) return "nil";

        if (Object is double)
        {
            string text = Object.ToString();
            if (text.EndsWith(".0"))
            {
                text = text.Substring(0,text.Length - 2);
            }
            return text;
        }

        return Object.ToString();
    }

    public object VisitBinaryExpr(Expr.Binary expr)
    {
        object left = Evaluate(expr.left);
        object right = Evaluate(expr.right);

        switch (expr.op.type)
        {
            case TokenType.BANG_EQUAL: 
                return !IsEqual(left, right);
            case TokenType.EQUAL_EQUAL: 
                return IsEqual(left, right);
            case TokenType.GREATER:
                CheckNumberOperands(expr.op, left, right);
                return (double)left > (double)right;
            case TokenType.GREATER_EQUAL:
                CheckNumberOperands(expr.op, left, right); 
                return (double)left >= (double)right;
            case TokenType.LESS:
                CheckNumberOperands(expr.op, left, right);
                return (double)left < (double)right;
            case TokenType.LESS_EQUAL:
                CheckNumberOperands(expr.op, left, right);
                return (double)left <= (double)right;
            case TokenType.MINUS:
                CheckNumberOperands(expr.op, left, right);
                return (double)left - (double)right;
            case TokenType.PLUS:
                if(left is double && right is double)
                {
                    return (double)left + (double)right;
                }
                if(left is string && right is string)
                {
                    return (String)left + (String)right;
                }
                throw new RuntimeError(expr.op, "operands must be two numbers or two strings.");
            case TokenType.SLASH:
                CheckNumberOperands(expr.op, left, right);
                return (double)left / (double)right;
            case TokenType.STAR:
                CheckNumberOperands(expr.op, left, right);
                return (double)left * (double)right;
        }

        return null;
    }
}