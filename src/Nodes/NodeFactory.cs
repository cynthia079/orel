using System;
using System.Collections.Generic;
using System.Text;

namespace Orel.Nodes
{
    internal static class NodeFactory
    {
        private static Dictionary<TokenType, Func<Token, Node>> _construnctors = new Dictionary<TokenType, Func<Token, Node>>();
        static NodeFactory()
        {
            _construnctors.Add(TokenType.ConstantNumber, token => new ConstantNode(token));
            _construnctors.Add(TokenType.ConstantString, token => new ConstantNode(token));
            _construnctors.Add(TokenType.ConstantNull, token => new ConstantNode(token));
            _construnctors.Add(TokenType.ConstantBoolean, token => new ConstantBooleanNode(token));
            _construnctors.Add(TokenType.DefaultArgument, token => new DefaultArgumentNode(token));
            _construnctors.Add(TokenType.MemberAccess, token => new MemberAccessNode(token));
            _construnctors.Add(TokenType.Dot, token => new DotNode(token));
            _construnctors.Add(TokenType.And, token => new BinaryLogicNode(token));
            _construnctors.Add(TokenType.Or, token => new BinaryLogicNode(token));
            _construnctors.Add(TokenType.Not, token => new NotNode(token));
            _construnctors.Add(TokenType.Add, token => new AddNode(token));
            _construnctors.Add(TokenType.Substract, token => new SubstractNode(token));
            _construnctors.Add(TokenType.Multiply, token => new MultDivNode(token));
            _construnctors.Add(TokenType.Divide, token => new MultDivNode(token));
            _construnctors.Add(TokenType.Equal, token => new ComparerNode(token));
            _construnctors.Add(TokenType.NotEqual, token => new ComparerNode(token));
            _construnctors.Add(TokenType.GreaterThan, token => new ComparerNode(token));
            _construnctors.Add(TokenType.GreaterThanOrEqual, token => new ComparerNode(token));
            _construnctors.Add(TokenType.LessThan, token => new ComparerNode(token));
            _construnctors.Add(TokenType.LessThanOrEqual, token => new ComparerNode(token));
            _construnctors.Add(TokenType.Like, token => new LikeNode(token));
            _construnctors.Add(TokenType.MethodStart, token => new MethodNode(token));
            _construnctors.Add(TokenType.Between, token => new BetweenNode(token));
            _construnctors.Add(TokenType.BetweenStart, token => new ConstantBooleanNode(token));
            _construnctors.Add(TokenType.BetweenEnd, token => new ConstantBooleanNode(token));
            _construnctors.Add(TokenType.ArrayIndexStart, token => new ArrayNode(token));
            _construnctors.Add(TokenType.Range, token => new RangeNode(token));
            _construnctors.Add(TokenType.Yield, token => new YieldNode(token));
            _construnctors.Add(TokenType.OpenBrace, token => new BuildObjectNode(token));
            _construnctors.Add(TokenType.Colon, token => new ColonNode(token));
            _construnctors.Add(TokenType.Compose, token => new ComposeNode(token));
            _construnctors.Add(TokenType.Parameter, token => new ParameterNode(token));
            _construnctors.Add(TokenType.Empty, token => Node.Empty);
            _construnctors.Add(TokenType.Reduce, token => new ReduceNode(token));
            _construnctors.Add(TokenType.Export, token => new ExportNode(token));
        }

        public static Node CreateFrom(Token token)
        {
            if (_construnctors.TryGetValue(token.Type, out var func))
            {
                return func(token);
            }
            else
            {
                return new Node(token);
            }
        }
    }
}
