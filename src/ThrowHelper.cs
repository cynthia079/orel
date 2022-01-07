using System;
using System.Collections.Generic;
using System.Linq;

namespace Orel
{
    internal static class ThrowHelper
    {
        internal static UnExpectedEndException UnExpectedEnd()
        {
            return new UnExpectedEndException();
        }

        internal static UnExpectedEndException UnExpectedEnd(Token token)
        {
            return new UnExpectedEndException();
        }

        internal static UnClosedQuoteException UnClosedQuote()
        {
            return new UnClosedQuoteException();
        }

        internal static UnSupportedSyntaxException UnSupportedSyntax(Token token)
        {
            return new UnSupportedSyntaxException(token);
        }

        internal static UnSupportedSyntaxException UnSupportedSyntax(Token token, string message)
        {
            return new UnSupportedSyntaxException(token, message);
        }

        internal static InvalidTokenException InvalidToken(string value)
        {
            return new InvalidTokenException(value);
        }

        internal static InvalidOperatorException InvalidOperator(Token token)
        {
            return new InvalidOperatorException(token);
        }

        internal static InvalidOperandException InvalidOperand(Token op, params Token[] operands)
        {
            string oprandsDesc = string.Join(",", operands.Select(o => o.Text));
            string msg = $"参数：'{oprandsDesc}' 无法适用于方法或操作符：{op.DebugInfo}";
            return new InvalidOperandException(msg);
        }

        internal static InvalidOperandException InvalidOperand(Token op, IEnumerable<string> operands, string message = null)
        {
            string oprandsDesc = string.Join(",", operands);
            string msg = $"参数：'{oprandsDesc}' 无法适用于方法或操作符：{op.DebugInfo}";
            if (message != null)
            {
                msg += $",{message}";
            }
            return new InvalidOperandException(msg);
        }

        internal static InvalidMemberNameException InvalidMemberName(string memberName)
        {
            string message = $"指定的字段名称不存在：{memberName}";
            return new InvalidMemberNameException(message);
        }

        internal static InvalidMemberUsageException InvalidMemberUsage(string memberName)
        {
            string message = $"字段类型不支持该使用方式：{memberName}";
            return new InvalidMemberUsageException(message);
        }

        internal static InvalidPrecompileMethodArgumentsException InvalidPrecompileMethodArguments(Token token)
        {
            return new InvalidPrecompileMethodArgumentsException(token);
        }

        internal static InvalidMethodCallException InvalidMethodCall(string methodName, IEnumerable<string> arguments)
        {
            return new InvalidMethodCallException(methodName, arguments);
        }

        internal static ConflictParameterTypeException ConflictParameterType(Token token, Type type, Type curType)
        {
            return new ConflictParameterTypeException(token, type, curType);
        }

        internal static InvalidParameterOperationException InvalidParameterOperation(Token @operator, params Token[] @params)
        {
            return new InvalidParameterOperationException(@operator);
        }
    }

    public class BaseException : Exception
    {
        internal BaseException(string message) : base(message) { }
        internal BaseException(Token token, string message) : base($"{token.DebugInfo} {message}") { }
    }

    public class UnExpectedEndException : BaseException
    {
        internal UnExpectedEndException() : base("不正确的结束匹配") { }
        internal UnExpectedEndException(Token token) : base(token, "无正确的结束匹配") { }
    }

    public class UnClosedQuoteException : BaseException
    {
        internal UnClosedQuoteException() : base("无法正确结束对内容的引用") { }
        internal UnClosedQuoteException(Token token) : base(token, "无法正确结束对内容的引用") { }
    }

    public class UnSupportedSyntaxException : BaseException
    {
        internal UnSupportedSyntaxException(Token token) : base(token, "无效") { }
        internal UnSupportedSyntaxException(Token token, string message) : base(token, message) { }
    }

    public class InvalidTokenException : BaseException
    {
        internal InvalidTokenException(string value) : base($"{value} 是无效的符号") { }
    }

    public class InvalidOperatorException : BaseException
    {
        internal InvalidOperatorException(Token token) : base(token, "无法构成有效的运算表达式") { }
    }

    public class InvalidOperandException : BaseException
    {
        internal InvalidOperandException(string message) : base(message) { }
    }

    public class InvalidMemberNameException : BaseException
    {
        internal InvalidMemberNameException(string message) : base(message) { }
    }

    public class InvalidMemberUsageException : BaseException
    {
        internal InvalidMemberUsageException(string message) : base(message) { }
    }

    public class InvalidPrecompileMethodArgumentsException : BaseException
    {
        internal InvalidPrecompileMethodArgumentsException(Token token) : base(token, "预编译方法的参数无效，必须是常量") { }
    }

    public class InvalidMethodCallException : BaseException
    {
        internal InvalidMethodCallException(string methodName, IEnumerable<string> arguments)
            : base($"无效的方法调用，方法不存在或参数错误{methodName}({string.Join(',', arguments)})") { }
    }

    public class ConflictParameterTypeException : BaseException
    {
        internal ConflictParameterTypeException(Token token, Type type, Type curType)
            : base($"参数{token.DebugInfo}的期望类型为{type}, 与已定义的期望类型{curType}不一致")
        { }
    }

    public class InvalidParameterOperationException : BaseException
    {
        internal InvalidParameterOperationException(Token @operator, params Token[] @params)
            : base($"参数：{String.Join(",", @params.Select(p => p.DebugInfo))} 无法参与运算:{@operator.DebugInfo}，缺少类型信息，或类型推断无效") { }
    }
}
