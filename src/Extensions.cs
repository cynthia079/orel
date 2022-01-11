using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Newtonsoft.Json.Linq;
using Orel.Expressions;
using Orel.Schema;

namespace Orel
{
    public static class Extensions
    {
        public static string AsString(this ReadOnlySpan<char> src, int start, int end)
        {
            if (end > src.Length)
            {
                end = src.Length;
            }

            ReadOnlySpan<char> span = src.Slice(start, end - start + 1);
            return new string(span);
        }

        public static bool IsNumeric(this Type type)
        {
            return type == typeof(decimal) || type == typeof(decimal?)
               || type == typeof(int) || type == typeof(int?)
               || type == typeof(long) || type == typeof(long?)
               || type == typeof(double) || type == typeof(double?)
               || type == typeof(short) || type == typeof(short?)
               || type == typeof(uint) || type == typeof(uint?)
               || type == typeof(ushort) || type == typeof(ushort?)
               || type == typeof(ulong) || type == typeof(ulong?)
               || type == typeof(byte) || type == typeof(byte?)
               || type == typeof(float) || type == typeof(float?)
               || type == typeof(sbyte) || type == typeof(sbyte?)
               ;
        }

        public static bool IsInterger(this Type type)
        {
            return type == typeof(int) || type == typeof(int?)
               || type == typeof(long) || type == typeof(long?)
               || type == typeof(short) || type == typeof(short?)
               || type == typeof(uint) || type == typeof(uint?)
               || type == typeof(ushort) || type == typeof(ushort?)
               || type == typeof(ulong) || type == typeof(ulong?)
               || type == typeof(byte) || type == typeof(byte?)
               || type == typeof(sbyte) || type == typeof(sbyte?)
               ;
        }

        public static bool IsFloat(this Type type)
        {
            return type == typeof(decimal) || type == typeof(decimal?)
                || type == typeof(double) || type == typeof(double?)
                || type == typeof(float) || type == typeof(float?);
        }

        public static bool IsDateTime(this Type type)
        {
            return type == typeof(DateTime) || type == typeof(DateTime?)
             || type == typeof(DateTimeOffset) || type == typeof(DateTimeOffset?);
        }

        public static bool IsString(this Type type)
        {
            return type == typeof(string);
        }

        public static bool IsList(this Type type)
        {
            if (type == typeof(JObject))
            {
                return false;
            }
            return typeof(IList).IsAssignableFrom(type)
                || (type.IsGenericType && typeof(IList<>).IsAssignableFrom(type.GetGenericTypeDefinition()));
        }

        public static bool IsBoolean(this Type type)
        {
            return type == typeof(bool) || type == typeof(bool?);
        }

        public static bool IsDynamic(this Type type)
        {
            return typeof(IDynamicMetaObjectProvider).IsAssignableFrom(type);
        }

        //public static DefaultMemberDescriptor SetToDescriptor(this IEnumerable<MemberDefinition> members, string defaultScope = "Data")
        //{
        //    return new DefaultMemberDescriptor(members, defaultScope);
        //}

        public static ExpressionWrapper Wrap(this Expression expression, Token token = null)
        {
            return new ExpressionWrapper(expression, null, token);
        }

        public static ExpressionWrapper Wrap(this Expression expression, Type explicitType, Token token = null)
        {
            return new ExpressionWrapper(expression, null, token) { ExplicitType = explicitType };
        }

        public static ExpressionWrapper Wrap(this Expression expression, IMemberDefinition member, Token token = null)
        {
            return new ExpressionWrapper(expression, member, token);
        }

        public static ExpressionWrapper Wrap(this Expression expression, DataType ExpectType, Token token = null)
        {
            return new ExpressionWrapper(expression, ExpectType, token);
        }

        internal static string CapsulateAsMemberName(this string memberName)
        {
            return $"`{memberName}`";
        }

        public static IEnumerable<T> HeadOf<T>(this T first, IEnumerable<T> enumerable)
        {
            yield return first;
            foreach (var item in enumerable)
            {
                yield return item;
            }
        }

        public static IEnumerable<T> TailOf<T>(this T last, IEnumerable<T> enumerable)
        {
            foreach (var item in enumerable)
            {
                yield return item;
            }
            yield return last;
        }

        public static void AddRange<T>(this HashSet<T> hashSet, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                hashSet.Add(item);
            }
        }

        public static bool TryCast<TOut>(this object src, out TOut target) where TOut : class
        {
            if (src is TOut)
            {
                target = src as TOut;
                return true;
            }
            else
            {
                target = default;
                return false;
            }
        }

        public static string GetMethodSign(this MethodInfo method)
        {
            var paramSign = string.Join(';', method.GetParameters().Select(s => s.ParameterType.Name));
            return $"{method.Name}::{paramSign}";
        }

        public static string GetParameterSign(this LambdaExpression lambdaExpression)
        {
            var paramSign = string.Join(';', lambdaExpression.Parameters.Select(s => s.Type.Name));
            return paramSign;
        }

        internal static Type GetElementType(this Type type)
        {
            if (type.IsArray)
            {
                return type.GetElementType();
            }
            else
            {
                return type.GenericTypeArguments.FirstOrDefault();
            }
        }
    }
}
