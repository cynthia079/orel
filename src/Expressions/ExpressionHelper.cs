using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.CSharp.RuntimeBinder;

namespace Orel.Expressions
{
    public static class ExpressionHelper
    {
        /// <summary>
        /// 创建成员访问表达式
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="memberName"></param>
        /// <param name="contextType"></param>
        /// <param name="itemType"></param>
        /// <returns></returns>
        public static Expression MakeMemberAccessExpression(Expression expression, string memberName, Type contextType, Type itemType, bool isList = false)
        {
            var runtimeType = GetRepresentType(itemType);
            CallSiteBinder binder = CreateBinder(memberName, contextType);
            Expression instance = expression;

            var callSiteType = typeof(Func<CallSite, object, object>);
            DynamicExpression memberExpression = Expression.MakeDynamic(callSiteType, binder, instance);

            var guardExpression = GetMethodCallExpression(typeof(IntrinsicFunctions), nameof(IntrinsicFunctions.IsNull), instance);

            var returnType = isList ? typeof(List<>).MakeGenericType(runtimeType) : runtimeType;
            //var convertMethodName = isList ? nameof(IntrinsicFunctions.ConvertListType) : nameof(IntrinsicFunctions.ConvertClassType);

            LabelTarget target = Expression.Label(returnType);
            LabelExpression label = Expression.Label(target, Expression.Default(returnType));

            Expression returnExpression = isList
                ? GetMethodCallExpression(typeof(IntrinsicFunctions), nameof(IntrinsicFunctions.ConvertListType), new[] { runtimeType }, memberExpression.AsList())
                : GetMethodCallExpression(typeof(IntrinsicFunctions), nameof(IntrinsicFunctions.ConvertClassType), new[] { runtimeType }, memberExpression);
            //var returnExpression = GetMethodCallExpression(typeof(IntrinsicFunctions), methodName, new[] { runtimeType }, memberExpression);

            var block = Expression.Block(
                Expression.IfThenElse(guardExpression, Expression.Return(target, Expression.Default(returnType)),
                Expression.TryCatch(
                    Expression.Return(target, returnExpression),
                    Expression.Catch(typeof(Exception), Expression.Return(target, Expression.Default(returnType))))
                ), label);

            return block;
        }

        private static Type GetRepresentType(Type type)
        {
            if (type == null)
                return typeof(object);
            if (type.IsPrimitive)
                return typeof(Nullable<>).MakeGenericType(type);
            return type;
        }

        internal static CallSiteBinder CreateBinder(string memberName, Type type = null)
        {
            CallSiteBinder binder = Microsoft.CSharp.RuntimeBinder.Binder.GetMember(CSharpBinderFlags.None, memberName, type ?? typeof(object),
               new CSharpArgumentInfo[]
               {
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
               });
            return binder;
        }

        public static MethodCallExpression GetMethodCallExpression(Type classType, string methodName, params Expression[] arguments)
        {
            MethodInfo method = classType.GetMethod(methodName, arguments.Select(arg => arg.Type).ToArray());
            if (method == null)
            {
                throw new ArgumentException(methodName);
            }
            var args = AdjustArgumentsByTypes(method, arguments);
            return Expression.Call(method, args);
        }

        public static MethodCallExpression GetMethodCallExpression(Type classType, string methodName, Type[] typeArguments, params Expression[] arguments)
        {
            MethodInfo method = classType.GetMethod(methodName, arguments.Select(arg => arg.Type).ToArray());
            if (method == null)
            {
                throw new ArgumentException(methodName);
            }
            var args = AdjustArgumentsByTypes(method, arguments).ToArray();
            return Expression.Call(classType, method.Name, typeArguments, args);
        }

        private static IEnumerable<Expression> AdjustArgumentsByTypes(MethodInfo method, Expression[] arguments)
        {
            var parameters = method.GetParameters();
            for (int i = 0; i < parameters.Length; i++)
            {
                var param = parameters[i];
                var arg = arguments[i];
                if (arguments[i].Type != param.ParameterType)
                {
                    if (param.ParameterType == typeof(object))
                    {
                        yield return arg.AsObject();
                    }
                }
                else
                    yield return arg;
            }
        }


        internal static IEnumerable<MethodInfo> GetMethods(Type classType, string methodName)
        {
            IEnumerable<MethodInfo> methods = classType.GetMethods().Where(m => m.Name.Equals(methodName, StringComparison.OrdinalIgnoreCase)
        || m.GetCustomAttributes<MethodNameAttribute>().Any(attr => attr.Name.Equals(methodName, StringComparison.OrdinalIgnoreCase)));

            return methods;
        }

        private static IEnumerable<MethodInfo> GetCandidateMethodInfos(Type classType, string methodName, params Expression[] arguments)
        {
            IEnumerable<MethodInfo> methods = classType.GetMethods().Where(m => m.Name.Equals(methodName, StringComparison.OrdinalIgnoreCase)
          || m.GetCustomAttributes<MethodNameAttribute>().Any(attr => attr.Name.Equals(methodName, StringComparison.OrdinalIgnoreCase)));

            foreach (MethodInfo m in methods)
            {
                ParameterInfo[] paramList = m.GetParameters();
                if (paramList.Length > 0 && paramList.Last().CustomAttributes.Any(c => c.AttributeType == typeof(ParamArrayAttribute)))
                {
                    var skip = paramList.Length - 1;
                    var paramArrayArgs = arguments.Skip(skip);
                    bool equal = true;
                    var elementType = paramList.Last().ParameterType.GetElementType();
                    foreach (var arg in paramArrayArgs)
                    {
                        if (!CompatibleTypes(elementType, arg.Type))
                            equal = false;
                    }
                    if (equal)
                        yield return m;
                }
                else if (paramList.Length == arguments.Length)
                {
                    bool equal = true;
                    for (int i = 0; i < paramList.Length; i++)
                    {
                        var arg = arguments[i];
                        var param = paramList[i];
                        var acceptAnyType = param.GetCustomAttribute<FallbackAttribute>();
                        if (acceptAnyType == null && arg.Type != typeof(object) && !CompatibleTypes(param.ParameterType, arg.Type))
                        {
                            equal = false;
                            break;
                        }
                    }
                    if (equal)
                    {
                        yield return m;
                    }
                }
            }
        }

        private static MethodInfo GetAppropriateMethodInfo(IList<MethodInfo> candidates, params ExpressionWrapper[] arguments)
        {
            if (!candidates.Any())
                return null;
            var fallbacks = new List<(MethodInfo, List<(ExpressionWrapper, DataType)>)>();
            //match flag: match:1,  miss:0, fallback:2
            const int MATCH = 1;
            const int MISS = 0;
            const int FALLBACK = 2;
            foreach (var m in candidates)
            {
                var paramList = m.GetParameters();
                var flag = MATCH;
                var alignArgs = new List<(ExpressionWrapper, DataType)>();
                Type parameterType = null;
                bool fallback = false;
                for (int i = 0; i < arguments.Length; i++)
                {
                    if (i == paramList.Length - 1 && paramList[i].CustomAttributes.Any(p => p.AttributeType == typeof(ParamArrayAttribute)))
                    {
                        parameterType = paramList[i].ParameterType.GetElementType();
                    }
                    else if (i < paramList.Length)
                    {
                        parameterType = paramList[i].ParameterType;
                        fallback = paramList[i].GetCustomAttribute<FallbackAttribute>() != null;
                    }

                    var arg = arguments[i];

                    if (arg.MemberDefinition != null)
                    {
                        var runtimeType = arg.MemberDefinition.DataType.GetRuntimeType();
                        if (!CompatibleTypes(parameterType, runtimeType))
                        {
                            if (parameterType == typeof(string))
                            {
                                flag = FALLBACK;
                                alignArgs.Add((arg, DataType.Text));
                            }
                            else if (fallback)
                            {
                                flag = FALLBACK;
                                alignArgs.Add((arg, parameterType.GetORELDataType()));
                            }
                            else
                            {
                                flag = MISS;
                            }
                        }
                        else
                        {
                            alignArgs.Add((arg, arg.MemberDefinition.DataType));
                        }
                    }
                    else if (!CompatibleTypes(parameterType, arg.Type))
                    {
                        if (fallback)
                        {
                            flag = FALLBACK;
                            alignArgs.Add((arg, parameterType.GetORELDataType()));
                        }
                        else
                        {
                            flag = MISS;
                        }
                    }
                    else
                    {
                        alignArgs.Add((arg, parameterType.GetORELDataType()));
                    }
                    if (flag == MISS)
                    {
                        break;
                    }
                }
                if (flag == MATCH)
                {
                    alignArgs.ForEach(ar => ar.Item1.AlignExpressionType(ar.Item2));
                    return m;
                }
                if (flag == FALLBACK)
                {
                    fallbacks.Add((m, alignArgs));
                }
            }
            if (fallbacks.Any())
            {
                //如果有fallback方案，则返回第一个fallback方法
                var f = fallbacks.First();
                f.Item2.ForEach(ar => ar.Item1.AlignExpressionType(ar.Item2));
                return f.Item1;
            }
            return null;
        }

        /// <summary>
        /// 获取合适的方法，并对齐参数，目前尚不支持日期类型参数对于字符串类型实参的默认转换
        /// </summary>
        /// <param name="classType"></param>
        /// <param name="methodName"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public static MethodInfo GetAppropriateMethodInfo(Type classType, string methodName, params ExpressionWrapper[] arguments)
        {
            var candidates = GetCandidateMethodInfos(classType, methodName, arguments.Select(ar => ar.Expression).ToArray()).ToList();
            return GetAppropriateMethodInfo(candidates, arguments);
        }

        /// <summary>
        /// 获取合适的外部委托
        /// </summary>
        /// <param name="functions"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public static Delegate GetAppropriateDelegate(IList<Delegate> functions, params ExpressionWrapper[] arguments)
        {
            if (!functions.Any())
                return null;
            const int MATCH = 1; const int MISS = 0; const int FALLBACK = 2;
            var fallbacks = new List<(Delegate, List<(ExpressionWrapper, Type)>)>();
            foreach (var func in functions)
            {
                var parameters = func.Method.GetParameters();
                if (arguments.Length != parameters.Length)
                    continue;

                var flag = MATCH;
                var alignArgs = new List<(ExpressionWrapper, Type)>();
                for (int i = 0; i < arguments.Length; i++)
                {
                    var arg = arguments[i];
                    var param = parameters[i];
                    if (arg.MemberDefinition != null)
                    {
                        var runtimeType = arg.MemberDefinition.DataType.GetRuntimeType();
                        if (!CompatibleTypes(param.ParameterType, runtimeType))
                        {
                            if (param.ParameterType == typeof(string))
                            {
                                flag = FALLBACK;
                                alignArgs.Add((arg, param.ParameterType));
                            }
                            else
                            {
                                flag = MISS;
                            }
                        }
                        else
                        {
                            alignArgs.Add((arg, param.ParameterType));
                        }
                    }
                    else if (!CompatibleTypes(param.ParameterType, arg.Type))
                    {
                        flag = MISS;
                    }
                    else
                    {
                        alignArgs.Add((arg, param.ParameterType));
                    }
                    if (flag == MISS)
                    {
                        break;
                    }
                }
                if (flag == MATCH)
                {
                    alignArgs.ForEach(ar => ar.Item1.AlignToType(ar.Item2));
                    return func;
                }
                if (flag == FALLBACK)
                {
                    fallbacks.Add((func, alignArgs));
                }
            }
            if (fallbacks.Any())
            {
                //如果有fallback方案，则返回第一个fallback方法
                var f = fallbacks.First();
                f.Item2.ForEach(ar => ar.Item1.AlignToType(ar.Item2));
                return f.Item1;
            }
            return null;
        }

        private static bool AlignableTypes(Type type1, Type type2)
        {
            if (type1.IsBoolean() && type2.IsBoolean()) return true;
            if (type1.IsNumeric() && type2.IsNumeric()) return true;
            if (type1.IsDateTime() && type2.IsDateTime()) return true;
            return false;
        }

        /// <summary>
        /// 创建方法调用表达式，在有数据成员作为参数的情况下，与方法参数类型不同时，先尝试转换为对应的数据类型去匹配，再尝试转化为String类型去匹配
        /// </summary>
        /// <param name="classType">方法所属类型</param>
        /// <param name="methodName">方法的MethodNameAttribute标注的名称</param>
        /// <param name="arguments">参数表达式</param>
        /// <returns></returns>
        public static Expression CreateMethodCallExpression(Type classType, string methodName, params ExpressionWrapper[] arguments)
        {
            var methodInfo = GetAppropriateMethodInfo(classType, methodName, arguments);
            if (methodInfo == null)
            {
                return null;
            }
            //最后一个参数是可变个数参数
            var @params = methodInfo.GetParameters();
            if (@params.Length < arguments.Length)
            {
                var paramArgs = arguments.Skip(@params.Length - 1).Select(e => e.Expression);
                var paramArgExpression = Expression.NewArrayInit(@params.Last().ParameterType.GetElementType(), paramArgs);
                return Expression.Call(methodInfo, paramArgExpression.TailOf(arguments.Take(@params.Length - 1).Select(c => c.Expression)));
            }
            else
            {
                return Expression.Call(methodInfo, arguments.Select(c => c.Expression));
            }
        }

        /// <summary>
        /// 创建方法调用表达式
        /// </summary>
        /// <param name="methodToken"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public static MethodCallExpression GetMethodCallExpression(Token methodToken, params Expression[] arguments)
        {
            Type cla = typeof(IntrinsicFunctions);
            Type[] types = arguments.Select(a => a.Type).ToArray();
            MethodNameAttribute attrs = cla.GetCustomAttribute<MethodNameAttribute>();

            MethodInfo method = cla.GetMethods().Where(m => m.GetCustomAttributes<MethodNameAttribute>().Any(c => c.Name.Equals(methodToken.Text, StringComparison.OrdinalIgnoreCase))
                                                    && CheckMethodArguments(m, types)).FirstOrDefault();
            if (method == null)
            {
                throw ThrowHelper.UnSupportedSyntax(methodToken, "无效的方法名或参数");
            }
            return Expression.Call(method, arguments);
        }

        private static bool CheckMethodArguments(MethodInfo method, Type[] types)
        {
            ParameterInfo[] param = method.GetParameters();
            if (param.Length != types.Length)
            {
                return false;
            }
            for (int i = 0; i < param.Length; i++)
            {
                if ((i > types.Length - 1 || !CompatibleTypes(param[i].ParameterType, types[i])))
                {
                    return false;
                }
            }
            return true;
        }

        internal static bool CompatibleTypes(Type methodType, Type argType)
        {
            if (AlignableTypes(methodType, argType))
            {
                return true;
            }
            else if (methodType.IsInterface)
            {
                return methodType.IsAssignableFrom(argType);
            }
            return methodType.Equals(argType);
        }

        /// <summary>
        /// 构建Foreach的表达式
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="loopVar"></param>
        /// <param name="loopContent"></param>
        /// <returns></returns>
        private static Expression ForEach(Expression collection, ParameterExpression loopVar, Expression loopContent)
        {
            ParameterExpression enumeratorVar = Expression.Variable(typeof(IEnumerator), "enumerator");
            MethodCallExpression getEnumeratorCall = Expression.Call(collection, typeof(IEnumerable).GetMethod("GetEnumerator"));
            BinaryExpression enumeratorAssign = Expression.Assign(enumeratorVar, getEnumeratorCall);

            // The MoveNext method's actually on IEnumerator, not IEnumerator<T>
            MethodCallExpression moveNextCall = Expression.Call(enumeratorVar, typeof(IEnumerator).GetMethod("MoveNext"));

            var getItemExpression = GetMethodCallExpression(typeof(IntrinsicFunctions), nameof(IntrinsicFunctions.ConvertClassType), new[] { loopVar.Type },
                Expression.Property(enumeratorVar, "Current"));

            LabelTarget breakLabel = Expression.Label("LoopBreak");

            BlockExpression loop = Expression.Block(new[] { enumeratorVar },
                 enumeratorAssign,
                 Expression.Loop(
                     Expression.IfThenElse(
                         Expression.Equal(moveNextCall, Expression.Constant(true)),
                         Expression.Block(new[] { loopVar },
                             Expression.Assign(loopVar, getItemExpression),
                             loopContent
                         ),
                         Expression.Break(breakLabel)
                     ),
                 breakLabel)
                );

            return loop;
        }

        public static Expression Where(Expression listExpression, Expression condition, ParameterExpression parameter)
        {
            var type = parameter.Type;
            var listType = typeof(List<>).MakeGenericType(type);
            ParameterExpression loopVar = Expression.Parameter(type, "loopVar");
            ParameterExpression result = Expression.Parameter(listType, "result");
            LambdaExpression lambda = Expression.Lambda(condition, parameter);

            NewExpression init = Expression.New(listType);
            BinaryExpression assign = Expression.Assign(result, init);
            Expression loop = ForEach(listExpression, loopVar,
                Expression.IfThen(Expression.Invoke(lambda, loopVar),
                Expression.Call(result, listType.GetMethod("Add"), loopVar)));

            LabelTarget targetLable = Expression.Label(listType);
            LabelExpression lable = Expression.Label(targetLable, Expression.Default(listType));

            var guardExpression = Expression.Equal(listExpression, Expression.Default(listExpression.Type));
            BlockExpression block = Expression.Block(new[] { result },
                Expression.IfThen(guardExpression, Expression.Return(targetLable, Expression.Default(listType))),
                /*init,*/ assign, loop,
                Expression.Return(targetLable, result),
                lable
                );

            return block;
        }

        public static Expression Select(Expression listExpression, string memberName, Type contextType, Type itemType)
        {
            var type = itemType ?? typeof(object);
            var listType = typeof(List<>).MakeGenericType(type);
            ParameterExpression loopVar = Expression.Parameter(contextType ?? typeof(object), "loopVar");
            ParameterExpression result = Expression.Parameter(listType, "result");
            Expression memberAccess = MakeMemberAccessExpression(loopVar, memberName, contextType, type);


            NewExpression init = Expression.New(listType);
            BinaryExpression assign = Expression.Assign(result, init);
            Expression loop = ForEach(listExpression, loopVar, Expression.Call(result, listType.GetMethod("Add"), memberAccess));
            LabelTarget targetLable = Expression.Label(listType);
            LabelExpression lable = Expression.Label(targetLable, Expression.Default(listType));

            var guardExpression = Expression.Equal(listExpression, Expression.Default(listExpression.Type));
            BlockExpression block = Expression.Block(new[] { result },
                Expression.IfThen(guardExpression, Expression.Return(targetLable, Expression.Default(listType))),
                /*init,*/ assign, loop,
                Expression.Return(targetLable, result),
                lable
                );

            return block;
        }

        public static Expression SelectMany(Expression listExpression, string memberName, Type contextType, Type itemType)
        {
            var type = itemType ?? typeof(object);
            var listType = typeof(List<>).MakeGenericType(type);
            ParameterExpression loopVar = Expression.Parameter(contextType ?? typeof(object), "loopVar");
            ParameterExpression innerLoopVar = Expression.Parameter(type, "innerLoopVar");
            ParameterExpression listVar = Expression.Parameter(typeof(IList), "listVar");
            ParameterExpression result = Expression.Parameter(listType, "result");
            Expression memberAccess = MakeMemberAccessExpression(loopVar, memberName, contextType, itemType, type != typeof(object)).AsList();

            NewExpression init = Expression.New(listType);
            BinaryExpression assign = Expression.Assign(result, init);
            Expression loop = ForEach(listExpression, loopVar,
                Expression.Block(new[] { listVar },
                Expression.Assign(listVar, memberAccess),
                Expression.IfThen(Expression.NotEqual(listVar, Expression.Constant(null)),
                ForEach(listVar, innerLoopVar, Expression.Call(result, listType.GetMethod("Add"), innerLoopVar))
                )));
            LabelTarget targetLable = Expression.Label(listType);
            LabelExpression lable = Expression.Label(targetLable, Expression.Default(listType));

            var guardExpression = Expression.Equal(listExpression, Expression.Default(listExpression.Type));
            BlockExpression block = Expression.Block(new[] { result },
                Expression.IfThen(guardExpression, Expression.Return(targetLable, Expression.Default(listType))),
                /*init,*/ assign, loop,
                Expression.Return(targetLable, result),
                lable
                );

            return block;
        }

        public static Expression TransformTo(Expression listExpression, Expression transform, ParameterExpression parameter, ParameterExpression indexVar)
        {
            var listType = typeof(List<>).MakeGenericType(transform.Type);
            ParameterExpression loopVar = Expression.Parameter(parameter.Type, "loopVar");
            ParameterExpression result = Expression.Parameter(listType, "result");

            NewExpression init = Expression.New(listType);
            BinaryExpression assign = Expression.Assign(result, init);

            if (indexVar == null)
            {
                indexVar = Expression.Parameter(typeof(decimal?), "index");
            }
            BinaryExpression indexAssign = Expression.Assign(indexVar, Expression.Constant(0m, typeof(decimal?)));

            var lambda = Expression.Lambda(transform, parameter, indexVar);
            Expression loop = ForEach(listExpression, loopVar,
               Expression.Block(
               Expression.Assign(indexVar, Expression.Increment(indexVar)),
               Expression.Call(result, listType.GetMethod("Add"), Expression.Invoke(lambda, loopVar/*.AsType(parameter.Type)*/, indexVar))));

            LabelTarget targetLable = Expression.Label(listType);
            LabelExpression lable = Expression.Label(targetLable, Expression.Default(listType));

            var guardExpression = Expression.Equal(listExpression, Expression.Default(listExpression.Type));
            BlockExpression block = Expression.Block(new[] { result, indexVar },
                Expression.IfThen(guardExpression, Expression.Return(targetLable, Expression.Default(listType))),
                /*init,*/ assign, indexAssign, loop,
                Expression.Return(targetLable, result),
                lable
                );

            return block;
        }

        public static Expression ReduceTo(Expression listExpression, Expression reduce, ParameterExpression parameter)
        {
            var lambda = Expression.Lambda(reduce, parameter);
            var invoke = Expression.Invoke(lambda, listExpression);
            return invoke;
        }

        public static Expression MakeListInitiation(params ExpressionWrapper[] expressions)
        {
            var listType = typeof(List<object>);
            NewExpression init = Expression.New(listType);
            var exp = Expression.ListInit(init, expressions.Select(e => e.Expression.AsObject()).ToArray());
            return exp;
        }

        public static Expression MakeListConcat(ExpressionWrapper expression1, ExpressionWrapper expression2)
        {
            var listType = typeof(List<object>);
            ParameterExpression result = Expression.Parameter(listType, "result");
            ParameterExpression loopVar = Expression.Parameter(typeof(object), "loopVar");
            Expression assign;
            var method = listType.GetMethod("Add");
            IEnumerable<Expression> adds;

            if (expression1.Type == listType)
            {
                assign = Expression.Assign(result, expression1);
                adds = new[] { ForEach(expression2, loopVar, Expression.Call(result, method, loopVar.AsObject())) };
            }
            else
            {
                NewExpression init = Expression.New(listType);
                assign = Expression.Assign(result, init);
                adds = new[] {
                    ForEach(expression1, loopVar, Expression.Call(result, method, loopVar.AsObject())),
                    ForEach(expression2, loopVar, Expression.Call(result, method, loopVar.AsObject()))
                };
            }
            LabelTarget targetLable = Expression.Label(listType);
            LabelExpression lable = Expression.Label(targetLable, Expression.Default(listType));
            var block = Expression.Block(new[] { result }, assign.HeadOf(adds).Concat(new Expression[] { Expression.Return(targetLable, result), lable }));
            return block;
        }

        public static Expression MakeORELObjectExpression(IEnumerable<(string, Expression)> members)
        {
            var resultType = typeof(ORELObject);
            var result = Expression.Variable(resultType);
            var init = Expression.New(resultType);
            var assign = Expression.Assign(result, init);
            var methodInfo = resultType.GetMethod("SetMember");
            var setMemberActions = Expression.Block(members.Select(m => Expression.Call(result, methodInfo, Expression.Constant(m.Item1, typeof(string)), m.Item2.AsObject())));
            LabelTarget target = Expression.Label(resultType);
            LabelExpression label = Expression.Label(target, Expression.Default(resultType));
            return Expression.Block(new[] { result }, assign, setMemberActions, Expression.Return(target, result), label);
        }

        public static Expression MakeIfElseExpression(Expression condition, Expression valueIfTrue, Expression valueIfFalse)
        {
            LabelTarget target = Expression.Label(typeof(object));
            LabelExpression label = Expression.Label(target, Expression.Default(typeof(object)));
            if (condition.Type == typeof(bool?))
            {
                condition = Expression.Call(condition, nameof(Nullable<bool>.GetValueOrDefault), null);
            }
            return Expression.Block(
                Expression.IfThenElse(condition, Expression.Return(target, valueIfTrue.AsObject()), Expression.Return(target, valueIfFalse.AsObject()))
                , label);
        }

        public static Expression AsList(this Expression expression)
        {
            return Expression.TypeAs(expression, typeof(IList));
        }

        public static Expression AsObject(this Expression expression)
        {
            return Expression.TypeAs(expression, typeof(object));
        }

        public static Expression AsType(this Expression expression, Type type)
        {
            return Expression.TypeAs(expression, type);
        }

        public static Expression AsType<T>(this Expression expression)
        {
            return Expression.TypeAs(expression, typeof(T));
        }

        public static Expression CastType<T>(this Expression expression)
        {
            return Expression.ConvertChecked(expression, typeof(T));
        }
    }
}
