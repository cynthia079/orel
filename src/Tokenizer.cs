using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Orel
{
    public class Tokenizer
    {
        public static IList<List<Token>> Scan(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                throw new ArgumentException("The content of source must not be empty");
            }
            var list = new List<List<Token>>();
            var scanner = new Scanner(source.AsSpan());
            while (!scanner.IsEnd)
            {
                var tokens = Scan(ref scanner);
                if (tokens.Count > 0)
                {
                    list.Add(tokens);
                }
            }
            return list;
        }

        /// <summary>
        /// Extract tokens from a quert text
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private static List<Token> Scan(ref Scanner scanner)
        {
            List<Token> list = new List<Token>();
            Stack<Token> blockStack = new Stack<Token>();
            try
            {
                while (!scanner.IsEnd)
                {
                    Token token = ScanForToken(ref scanner);
                    if (token == null)
                    {
                        break;
                    }
                    if (token.Type == TokenType.Delimiter)
                    {
                        break;
                    }
                    if (token.Type == TokenType.Comment)
                    {
                        continue;
                    }
                    Token previous = list.Count > 0 ? list[list.Count - 1] : null;
                    if (token.Type == TokenType.OpenBracket)
                    {
                        if (previous?.Type != TokenType.Between)
                        {
                            if (previous != null)
                            {
                                ResetVariableType(previous, TokenType.MemberAccess);
                            }
                            token.Type = TokenType.ArrayIndexStart;
                        }
                        else
                        {
                            token.Type = TokenType.BetweenStart;
                            token.Value = true;
                        }
                        blockStack.Push(token);
                    }
                    else if (token.Type == TokenType.OpenParen)
                    {
                        if (previous?.Type == TokenType.Between)
                        {
                            token.Type = TokenType.BetweenStart;
                            token.Value = false;
                        }
                        else
                        {
                            if (previous?.Type == TokenType.Variable)
                            {
                                ResetVariableType(previous, TokenType.MethodCall);
                                token.Type = TokenType.MethodStart;
                            }
                            else
                            {
                                token.Type = TokenType.BlockStart;
                            }
                        }
                        blockStack.Push(token);
                    }
                    else if (list.Count >= 2 && previous.Type == TokenType.Dot && list[list.Count - 2].Type == TokenType.ConstantNumber)
                    {
                        //判别是否是小数
                        if (decimal.TryParse(token.Text, out decimal floatValue))
                        {
                            string numberText = $"{list[list.Count - 2].Text}.{token.Text}";
                            list.Remove(list[list.Count - 1]);
                            list[list.Count - 1].Text = numberText;
                            if (decimal.TryParse(numberText, out floatValue))
                            {
                                list[list.Count - 1].Value = floatValue;
                            }
                            else
                            {
                                throw ThrowHelper.InvalidToken(numberText);
                            }
                            continue;
                        }
                        else
                        {
                            throw ThrowHelper.InvalidToken(token.Text);
                        }
                    }
                    else
                    {
                        if (previous?.Type == TokenType.Variable)
                        {
                            ResetVariableType(previous);
                        }

                        if (token.Type == TokenType.CloseBracket)
                        {
                            Token block = blockStack.Pop();
                            if (block.Type == TokenType.BetweenStart)
                            {
                                token.Type = TokenType.BetweenEnd;
                                token.Value = true;
                            }
                            else if (block.Type == TokenType.ArrayIndexStart)
                            {
                                token.Type = TokenType.ArrayIndexEnd;
                            }
                            else if (block.Type == TokenType.BlockStart)
                            {
                                token.Type = TokenType.BlockEnd;
                            }
                            else
                            {
                                throw ThrowHelper.UnSupportedSyntax(token);
                            }
                        }
                        else if (token.Type == TokenType.CloseParen)
                        {
                            Token block = blockStack.Pop();
                            if (block.Type == TokenType.BetweenStart)
                            {
                                token.Type = TokenType.BetweenEnd;
                                token.Value = false;
                                //blockCheck.Between = false;
                            }
                            else if (block.Type == TokenType.MethodStart)
                            {
                                token.Type = TokenType.MethodEnd;
                            }
                            else if (block.Type == TokenType.BlockStart)
                            {
                                token.Type = TokenType.BlockEnd;
                            }
                            else
                            {
                                throw ThrowHelper.UnSupportedSyntax(token);
                            }
                        }
                    }
                    //token.Index = i;
                    list.Add(token);
                }
                if (blockStack.Count > 0)
                {
                    throw ThrowHelper.UnExpectedEnd(blockStack.Peek());
                }
                if (list.Count > 0 && list.Last().Type == TokenType.Variable)
                {
                    ResetVariableType(list.Last());
                }
                return list;
            }
            catch (IndexOutOfRangeException)
            {
                throw ThrowHelper.UnExpectedEnd();
            }
        }

        private static void ResetVariableType(Token token, TokenType tokenType = TokenType.MemberAccess)
        {
            if (token.Type != TokenType.Variable)
            {
                return;
            }
            if (decimal.TryParse(token.Text, out decimal dv))
            {
                token.Value = dv;
                token.Type = TokenType.ConstantNumber;
            }
            else if (token.Text == "null" || token.Text == "NULL")
            {
                token.Type = TokenType.ConstantNull;
            }
            else if (Boolean.TryParse(token.Text, out bool bValue))
            {
                token.Type = TokenType.ConstantBoolean;
                token.Value = bValue;
            }
            else if (token.Text == TokenDefinitions.DefaultArgument)
            {
                token.Type = TokenType.DefaultArgument;
            }
            else
            {
                if (tokenType == TokenType.MemberAccess && token.Text.StartsWith("@"))
                {
                    token.Type = TokenType.Parameter;
                    token.Value = token.Text.Substring(1);
                }
                else
                {
                    token.Type = tokenType;
                }
            }
        }

        private static Token ScanForToken(ref Scanner scanner)
        {
            Token token = null;
            const char escape = '\\';

            while (char.IsWhiteSpace(scanner.Current) && scanner.Forward()) ;
            if (scanner.IsEnd)
            {
                return null;
            }

            int start = scanner.CurrentPosition;
            var symbolStart = scanner.DumpPosition();
            while (!IsTokenEnd(scanner.Current) && scanner.Forward()) ;

            if (scanner.CurrentPosition > start)
            {
                var end = scanner.IsEnd ? scanner.CurrentPosition : scanner.CurrentPosition - 1;
                var text = scanner.Retrieve(start, end);
                token = Token.Create(text, symbolStart);
            }
            else if (IsQuoteSymbol(scanner.Current))
            {
                StringBuilder stringBuffer = new StringBuilder();
                char startQuote = scanner.Current;
                scanner.Forward();
                start = scanner.CurrentPosition;
                if (scanner.IsEnd)
                {
                    ThrowHelper.UnExpectedEnd();
                }
                char previous = startQuote;
                bool closeQuoteFound = false;
                do
                {
                    if (IsQuoteSymbol(scanner.Current) && scanner.Current == startQuote)
                    {
                        if (escape.Equals(previous))
                        {
                            //'abc\'aaaa' = abc'aaaa 在引号中使用\表达对该引号的转义
                            stringBuffer.Append(scanner.Retrieve(start, scanner.CurrentPosition - 2));
                            start = scanner.CurrentPosition; //position;
                        }
                        else
                        {
                            //end of string
                            stringBuffer.Append(scanner.Retrieve(start, scanner.CurrentPosition - 1));
                            closeQuoteFound = true;
                            break;
                        }
                    }
                    previous = scanner.Current;
                } while (scanner.Forward());
                if (!closeQuoteFound)
                {
                    throw ThrowHelper.UnClosedQuote();
                }
                token = Token.CreateQuotedToken(stringBuffer.ToString(), startQuote, symbolStart);
                scanner.Forward();
            }
            else if (scanner.Current == TokenDefinitions.Comment)
            {
                while (scanner.Forward() && scanner.Current != TokenDefinitions.LineBreaker) ;
                var text = scanner.Retrieve(start, scanner.CurrentPosition);
                token = Token.Create(text, symbolStart);
                token.Type = TokenType.Comment;
                scanner.Forward();
            }
            else
            {
                if (scanner.LeftCount >= 1 && TokenDefinitions.TryGetDoubleCharSymbol(scanner.Current, scanner.Next))
                {
                    var text = scanner.Retrieve(start, scanner.CurrentPosition + 1);
                    token = Token.Create(text, symbolStart);
                    scanner.Forward();
                    scanner.Forward();
                }
                else
                {
                    token = Token.Create(scanner.Current, symbolStart);
                    scanner.Forward();
                }
            }
            return token;
        }

        private static bool IsQuoteSymbol(char ch, char alignSymbol, char previousSymbol)
        {
            if (IsQuoteSymbol(ch))
            {
                return !alignSymbol.Equals(ch) || !'\\'.Equals(previousSymbol);
            }
            return false;
        }

        private static bool IsQuoteSymbol(char ch)
        {
            return (ch == TokenDefinitions.SingleQuote || ch == TokenDefinitions.DoubleQuote || ch == TokenDefinitions.MemberQuote);
        }

        private static bool IsTokenEnd(char ch)
        {
            return char.IsWhiteSpace(ch)
                || (TokenDefinitions.StopChars.Any(s => s == ch));
        }
    }

    internal ref struct Scanner
    {
        private ReadOnlySpan<char> _chars;
        public bool IsEnd { get; private set; }
        public int CurrentPosition { get; private set; }
        public int LineNo { get; private set; }
        public int LinePostion { get; private set; }

        public Scanner(ReadOnlySpan<char> chars, int start = 0)
        {
            _chars = chars;
            CurrentPosition = start;
            LineNo = 1;
            LinePostion = 0;
            IsEnd = false;
        }

        public bool Forward()
        {
            if (CurrentPosition >= _chars.Length - 1)
            {
                IsEnd = true;
                return false;
            }
            CurrentPosition++;
            LinePostion++;
            if (Current == TokenDefinitions.LineBreaker)
            {
                LineNo++;
                LinePostion = 0;
            }
            return true;
        }

        public char Current
        {
            get { return _chars[CurrentPosition]; }
        }

        public char Next
        {
            get { return _chars[CurrentPosition + 1]; }
        }

        public string Retrieve(int start, int end)
        {
            return _chars.AsString(start, end);
        }

        public SymbolPosition DumpPosition() => new SymbolPosition(CurrentPosition, LineNo, LinePostion);

        public int LeftCount => _chars.Length - CurrentPosition - 1;
    }
}
