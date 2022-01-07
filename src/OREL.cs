using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Orel.Schema;

namespace Orel
{
    public static class OREL
    {
        /// <summary>
        /// 使用OREL语法编译语句，生成可执行委托, 使用Data字段作为默认Scope，每一层级的字段附带上一层级（非根节点）的名称+"_"作为前缀
        /// </summary>
        /// <param name="statement"></param>
        /// <param name="members"></param>
        /// <returns></returns>
        public static ORELExecutable Compile(string statement, IEnumerable<MemberDefinition> members, IEnumerable<ParameterDefinition> parameters = null, string defaultScope = "Data")
        {
            var memberDescriptor = new DefaultMemberDescriptor(members, defaultScope);
            return Compile(statement, memberDescriptor, parameters);
        }

        /// <summary>
        /// 使用OREL语法编译语句，生成可执行委托
        /// </summary>
        /// <param name="statement"></param>
        /// <param name="memberDescriptor"></param>
        /// <returns></returns>
        public static ORELExecutable Compile(string statement, IMemberDescriptor memberDescriptor, IEnumerable<ParameterDefinition> parameters = null)
        {
            List<Token> tokens = Tokenizer.Scan(statement).First();
            TreeBuilder tb = new TreeBuilder();
            tb.AppendRange(tokens);

            ParameterExpression paramExp = memberDescriptor.RootType == RootType.Object ? Expression.Parameter(typeof(object), "x")
                : Expression.Parameter(typeof(IList), "x");

            var context = BuildContext.Create(paramExp, memberDescriptor, parameters);
            var lamdaParameters = paramExp.HeadOf(context.ParameterManager.Parameters.Select(p => p.Value.Expression));
            Expression exp = tb.GernerateTree(context);
            LambdaExpression lambda = Expression.Lambda(exp, lamdaParameters);
            Delegate del = lambda.Compile();
            return new ORELExecutable()
            {
                Lambda = lambda,
                Delegate = del,
                MemberDescriptor = memberDescriptor,
                Parameters = context.ParameterManager.Parameters
            };
        }

        /// <summary>
        /// 使用OREL语法编译语句，生成可执行委托（无Schema）
        /// </summary>
        /// <param name="statement"></param>
        /// <returns></returns>
        public static ORELExecutable Compile(string statement, IEnumerable<ParameterDefinition> parameters = null)
        {
            List<Token> tokens = Tokenizer.Scan(statement).First();
            TreeBuilder tb = new TreeBuilder();
            tb.AppendRange(tokens);
            var context = BuildContext.Create(parameters);
            var lamdaParameters = context.ParameterManager.Parameters.Select(p => p.Value.Expression);
            Expression exp = tb.GernerateTree(context);
            LambdaExpression lambda = Expression.Lambda(exp, lamdaParameters);
            Delegate del = lambda.Compile();
            return new ORELExecutable()
            {
                Lambda = lambda,
                Delegate = del,
                Parameters = context.ParameterManager.Parameters
            };
        }

        /// <summary>
        /// 预编译语句，尝试将语句中的预编译方法固化，返回固化后的语句
        /// </summary>
        /// <param name="statement"></param>
        /// <returns></returns>
        public static string Precompile(string statement)
        {
            List<Token> tokens = Tokenizer.Scan(statement).First();
            TreeBuilder tb = new TreeBuilder();
            tb.AppendRange(tokens);
            return tb.Precompile();
        }

        /// <summary>
        /// 从一个object动态生成schema，并编译语句
        /// </summary>
        /// <param name="schemaObject"></param>
        /// <param name="statement"></param>
        /// <returns></returns>
        public static ORELExecutable DynamicCompile(object schemaObject, string statement)
        {
            if (schemaObject == null)
                throw new ArgumentNullException(nameof(schemaObject));
            var schema = SchemaProvider.FromObject(schemaObject);
            return Compile(statement, schema);
        }

        /// <summary>
        /// 从一个object动态生成schema，编译并执行语句，返回执行后的结果
        /// </summary>
        /// <param name="schemaObject"></param>
        /// <param name="statement"></param>
        /// <returns></returns>
        public static object DynamicExecute(object schemaObject, string statement)
        {
            var exe = DynamicCompile(schemaObject, statement);
            return exe.Execute(schemaObject);
        }

        public static ORELExecutable<T> Compile<T>(string statement, IMemberDescriptor memberDescriptor, IEnumerable<ParameterDefinition> parameters = null)
        {
            var exe = Compile(statement, memberDescriptor, parameters);
            return new ORELExecutable<T>(exe);
        }
    }
}
