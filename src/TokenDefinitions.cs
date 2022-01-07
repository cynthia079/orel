using System;
using System.Collections.Generic;
using System.Linq;

namespace Orel
{
    internal static class TokenDefinitions
    {
        public const char LineBreaker = '\n';
        public const char SingleQuote = '\'';
        public const char DoubleQuote = '\"';
        public const char MemberQuote = '`';
        public const char OpenParen = '(';
        public const char CloseParen = ')';
        public const char OpenBracket = '[';
        public const char CloseBracket = ']';
        public const char LessThan = '<';
        public const char Equal = '=';
        public const char GreaterThan = '>';
        public const char Not = '!';
        public const char Comma = ',';
        public const char Add = '+';
        public const char Substract = '-';
        public const char Multiply = '*';
        public const char Divide = '/';
        public const char Dot = '.';
        public const char OpenBrace = '{';
        public const char CloseBrace = '}';
        public const char Colon = ':';
        public const char Compose = '|';
        public const char Semicolon = ';';
        public const char Comment = '#';

        public const string DefaultArgument = "_";
        public const string LessThanOrEqual = "<=";
        public const string GreaterThanOrEqual = ">=";
        public const string NotEqual = "!=";
        public const string And = "and";
        public const string Or = "or";
        public const string Between = "between";
        public const string Like = "like";
        public const string Range = "..";
        public const string Yield = "=>";
        public const string Reduce = "->";
        public const string Export = "<-";

        public static readonly char[] StopChars = new char[]
        {
            SingleQuote,
            DoubleQuote,
            MemberQuote,
            OpenParen,
            CloseParen,
            OpenBracket,
            CloseBracket,
            LessThan,
            Equal,
            GreaterThan,
            Add,
            Substract,
            Multiply,
            Divide,
            Comma,
            Dot,
            Not,
            OpenBrace,
            CloseBrace,
            Colon,
            Compose,
            Substract,
            Semicolon,
            Comment
        };

        public static readonly TokenType[] Comparers = new TokenType[] { TokenType.Equal, TokenType.GreaterThan, TokenType.GreaterThanOrEqual, TokenType.LessThan, TokenType.LessThanOrEqual, TokenType.NotEqual, TokenType.Like };

        public static readonly TokenType[] Operators = new TokenType[] { TokenType.Add, TokenType.Substract, TokenType.Multiply, TokenType.Divide, TokenType.Compose };

        public static readonly TokenType[] Calls = new TokenType[] { TokenType.Between };

        public static readonly TokenType[] Logics = new TokenType[] { TokenType.And, TokenType.Or };

        /// <summary>
        /// 判断是否是双字符符号
        /// </summary>
        /// <param name="ch"></param>
        /// <param name="anothor"></param>
        /// <returns></returns>
        public static bool TryGetDoubleCharSymbol(char ch, char anothor)
        {
            if ((ch == LessThan || ch == GreaterThan || ch == Not) && anothor == Equal)  //<= ; >= ; !=
            {
                return true;
            }
            else if (ch == Dot && anothor == Dot)  //..
            {
                return true;
            }
            else if (ch == Equal && anothor == GreaterThan)  //=>
            {
                return true;
            }
            else if (ch == Substract && anothor == GreaterThan)  //->
            {
                return true;
            }
            else if (ch == LessThan && anothor == Substract) //<-
            {
                return true;
            }
            return false;
        }

        public static bool Expects(this Token token, Token next)
        {
            if (token == null)
            {
                return next.MayOneOf(TokenType.ConstantString, TokenType.Variable, TokenType.OpenParen, TokenType.Substract, TokenType.Not);
            }
            else if (token.Type == TokenType.OpenParen)
            {
                return next.MayOneOf(TokenType.ConstantString, TokenType.Variable, TokenType.OpenParen, TokenType.Substract, TokenType.Not, TokenType.CloseParen);
            }
            else if (token.Type == TokenType.Between)
            {
                return next.MayOneOf(TokenType.OpenParen, TokenType.OpenBracket);
            }
            else if (token.Type == TokenType.Like)
            {
                return next.MayOneOf(TokenType.OpenParen, TokenType.ConstantString);
            }
            else if (token.Type == TokenType.Variable)
            {
                return next.MayOneOf(Comparers)
                    || next.MayOneOf(Operators)
                    || next.MayOneOf(Calls)
                    || next.MayOneOf(Logics)
                    || next.MayOneOf(TokenType.OpenParen, TokenType.OpenBracket, TokenType.CloseParen, TokenType.CloseBracket, TokenType.Comma);
            }
            else if (token.Type == TokenType.CloseBracket || token.Type == TokenType.CloseParen)
            {
                return next.MayOneOf(Comparers)
                    || next.MayOneOf(Operators)
                    || next.MayOneOf(Calls)
                    || next.MayOneOf(Logics)
                    || next.MayOneOf(TokenType.Comma, TokenType.CloseParen, TokenType.CloseBracket);
            }
            else if (Comparers.Contains(token.Type) || Operators.Contains(token.Type))
            {
                return next.MayOneOf(TokenType.Variable, TokenType.OpenParen, TokenType.ConstantString, TokenType.Substract);
            }
            else if (Logics.Contains(token.Type))
            {
                return next.MayOneOf(TokenType.Variable, TokenType.OpenParen, TokenType.ConstantString);
            }
            else if (token.Type == TokenType.OpenBracket)
            {
                return next.MayOneOf(TokenType.ConstantString, TokenType.Variable, TokenType.OpenParen, TokenType.Substract, TokenType.Not);
            }
            else if (token.Type == TokenType.ConstantString)
            {
                return next.MayOneOf(Logics) || next.MayOneOf(Operators) || next.MayOneOf(Comparers) || next.MayOneOf(TokenType.Comma, TokenType.CloseBracket, TokenType.CloseParen);
            }
            else if (token.Type == TokenType.Comma)
            {
                return next.MayOneOf(TokenType.Variable, TokenType.ConstantString, TokenType.Substract, TokenType.OpenParen);
            }
            else if (Operators.Contains(token.Type))
            {

            }
            return false;
        }

        private static bool MayOneOf(this Token token, params TokenType[] types)
        {
            return types.Any(t => t == token.Type);
        }

        public static TokenType GetTokenType(char ch)
        {
            switch (ch)
            {
                case OpenParen:
                    return TokenType.OpenParen;
                case CloseParen:
                    return TokenType.CloseParen;
                case OpenBracket:
                    return TokenType.OpenBracket;
                case CloseBracket:
                    return TokenType.CloseBracket;
                case LessThan:
                    return TokenType.LessThan;
                case Equal:
                    return TokenType.Equal;
                case GreaterThan:
                    return TokenType.GreaterThan;
                case Add:
                    return TokenType.Add;
                case Substract:
                    return TokenType.Substract;
                case Multiply:
                    return TokenType.Multiply;
                case Divide:
                    return TokenType.Divide;
                case Not:
                    return TokenType.Not;
                case Comma:
                    return TokenType.Comma;
                case Dot:
                    return TokenType.Dot;
                case OpenBrace:
                    return TokenType.OpenBrace;
                case CloseBrace:
                    return TokenType.CloseBrace;
                case Colon:
                    return TokenType.Colon;
                case Compose:
                    return TokenType.Compose;
                case Semicolon:
                    return TokenType.Delimiter;
                case Comment:
                    return TokenType.Comment;
                default:
                    //throw ThrowHelper.InvalidToken(ch.ToString());
                    return TokenType.Variable;
            }
        }

        public static TokenType GetTokenType(string text)
        {
            switch (text.ToLowerInvariant())
            {
                case GreaterThanOrEqual:
                    return TokenType.GreaterThanOrEqual;
                case LessThanOrEqual:
                    return TokenType.LessThanOrEqual;
                case NotEqual:
                    return TokenType.NotEqual;
                case And:
                    return TokenType.And;
                case Or:
                    return TokenType.Or;
                case Between:
                    return TokenType.Between;
                case Like:
                    return TokenType.Like;
                case Range:
                    return TokenType.Range;
                case Yield:
                    return TokenType.Yield;
                case Reduce:
                    return TokenType.Reduce;
                case Export:
                    return TokenType.Export;
                default:
                    return TokenType.Variable;
            }
        }
    }

    public enum TokenType
    {
        ConstantString,
        ConstantNumber,
        ConstantBoolean,
        Variable,
        OpenParen,
        CloseParen,
        OpenBracket,
        CloseBracket,
        GreaterThan,
        LessThan,
        GreaterThanOrEqual,
        LessThanOrEqual,
        Equal,
        NotEqual,
        Add,
        Substract,
        Multiply,
        Divide,
        And,
        Or,
        Not,
        Between,
        Like,
        Comma,
        MemberAccess,
        MethodCall,
        ArrayAccess,
        Empty,
        Dot,
        Range,
        ArrayIndexStart,
        ArrayIndexEnd,
        MethodStart,
        MethodEnd,
        BetweenStart,
        BetweenEnd,
        BlockStart,
        BlockEnd,
        ConstantNull,
        Yield,
        DefaultArgument,
        OpenBrace,
        CloseBrace,
        Colon,
        BuildObject,
        Compose,
        Parameter,
        Reduce,
        Delimiter,
        Export,
        Comment
    }

    public enum TokenGroupType
    {
        Default,
        Operator,
        Comparer,
        LogicConnect
    }

    public struct SymbolPosition
    {
        public int AbsolutePosition { get; }
        public int LinePosition { get; }
        public int LineNo { get; }

        public SymbolPosition(int absolutePosition, int lineNo, int linePosition)
        {
            AbsolutePosition = absolutePosition;
            LinePosition = linePosition;
            LineNo = lineNo;
        }
    }

    public class Token
    {
        public static Token Empty = new Token(TokenType.Empty, string.Empty, -1);
        /// <summary>
        /// Token类型
        /// </summary>
        public TokenType Type { get; set; }
        /// <summary>
        /// Token对应的原始文本
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// 如果是常量数据，表示转换后的值
        /// </summary>
        public object Value { get; set; }
        /// <summary>
        /// 在Query文本中的位置
        /// </summary>
        public int Position { get; set; }
        /// <summary>
        /// 在整个Token序列的位置
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// 如果是括号的情况，表示配对的括号的Token
        /// </summary>
        public Token Pair { get; set; }
        public SymbolPosition? SymbolPosition { get; }

        public Token(TokenType type, string text, int position, SymbolPosition? symbolPostion = null)
        {
            Type = type;
            Text = text;
            SymbolPosition = symbolPostion;
        }

        public Token(TokenType type, string text, SymbolPosition? symbolPostion = null)
        {
            Type = type;
            Text = text;
            SymbolPosition = symbolPostion;
        }

        /// <summary>
        /// 设定Token在表达式树中的优先级(数值越小，优先级越高)
        /// </summary>
        public int Priority
        {
            get
            {
                switch (Type)
                {
                    case TokenType.Dot:
                        return -2;
                    case TokenType.Compose:
                        return 2;
                    case TokenType.Yield:
                        return 3;
                    case TokenType.Reduce:
                        return 4;
                    case TokenType.Colon:
                        return 5;
                    case TokenType.Comma:
                        return 6;
                    case TokenType.Export:
                        return 7;
                    default:
                        if (IsComparer)
                            return 0;
                        else if (IsLogic)
                            return 1;
                        else
                            return -1;
                }
                //if (Type == TokenType.Dot)
                //{
                //    return -2;
                //}
                //if (IsComparer)
                //{
                //    return 0;
                //}
                //if (IsLogic)
                //{
                //    return 1;
                //}
                //if (Type == TokenType.Compose)
                //{
                //    return 2;
                //}
                //if (Type == TokenType.Yield)
                //{
                //    return 3;
                //}
                //if (Type == TokenType.Reduce)
                //{
                //    return 4;
                //}
                //if (Type == TokenType.Colon)
                //{
                //    return 5;
                //}
                //if (Type == TokenType.Comma)
                //{
                //    return 6;
                //}
                //if(Type == TokenType.Export)
                //{
                //    return 7;
                //}
                //return -1;
            }
        }

        public bool IsOperator => TokenDefinitions.Operators.Contains(Type);

        public bool IsAdd => Type == TokenType.Add;

        public bool IsAddOrSub => Type == TokenType.Add || Type == TokenType.Substract;

        public bool IsMultOrDivide => Type == TokenType.Multiply || Type == TokenType.Divide;

        public bool IsOperand => Type == TokenType.ConstantNumber || Type == TokenType.ConstantString
            || Type == TokenType.ConstantNull || Type == TokenType.ConstantBoolean;

        public bool IsComparer => TokenDefinitions.Comparers.Contains(Type);

        public bool IsLogic => Type == TokenType.And || Type == TokenType.Or || Type == TokenType.Not;

        public override string ToString()
        {
            return Text;
        }

        public string DebugInfo
        {
            get { return $"'{Text}' at Line:{SymbolPosition?.LineNo}, Position:{SymbolPosition?.LinePosition}"; }
        }

        public static Token Create(char ch, SymbolPosition position)
        {
            return new Token(TokenDefinitions.GetTokenType(ch), ch.ToString(), position);
        }

        public static Token Create(string text, SymbolPosition position)
        {
            return new Token(TokenDefinitions.GetTokenType(text), text, position);
        }

        //public static Token Create(ReadOnlySpan<char> chars, int start, int end, int lineNo, int linePostion)
        //{
        //    string text = chars.AsString(start, end);
        //    return new Token(TokenDefinitions.GetTokenType(text), text,);
        //}

        public static Token CreateQuotedToken(string text, char quoteChar, SymbolPosition position)
        {
            if (quoteChar == TokenDefinitions.MemberQuote)
            {
                return new Token(TokenType.MemberAccess, text, position);
            }
            else
            {
                return new Token(TokenType.ConstantString, text, position);
            }
        }
    }
}
