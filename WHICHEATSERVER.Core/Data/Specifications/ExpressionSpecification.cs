namespace WHICHEATSERVER.Core.Data.Specifications
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq.Expressions;
    using System.Reflection;

    sealed class ExpressionSpecification
    {
        public static string SatisfiedBy<T>(Expression<Func<T, bool>> express) // 规约
        {
            return ResolveExpression(express.Body, express.Parameters);
        }

        public static string SatisfiedBy<T, E>(Expression<Func<T, E, bool>> express)
        {
            return ResolveExpression(express.Body, express.Parameters);
        }

        public static string SatisfiedBy<T>(Expression<Func<T, object>> express)
        {
            return ResolveExpression(express.Body, express.Parameters);
        }

        private static string ResolveBinaryExpression(Expression express, string symbol, ReadOnlyCollection<ParameterExpression> key)
        {
            if (express is BinaryExpression)
            {
                BinaryExpression e = (BinaryExpression)express;
                // 
                string left = ResolveExpression(e.Left, key);
                string right = ResolveExpression(e.Right, key);
                //
                return string.Format("{0} {1} {2}", left, symbol, right);
            }
            return null;
        }

        private static string ResolveConvertExpression(Expression express, ReadOnlyCollection<ParameterExpression> key)
        {
            if (express is UnaryExpression)
            {
                UnaryExpression e = (UnaryExpression)express;
                //
                return ResolveExpression(e.Operand, key);
            }
            return null;
        }

        private static string ResolveNotAndEqualsExpress(Expression express, ReadOnlyCollection<ParameterExpression> key)
        {
            if (express is BinaryExpression)
            {
                BinaryExpression e = (BinaryExpression)express;
                //
                string left = ResolveExpression(e.Left, key);
                string right = ResolveExpression(e.Right, key);
                //
                Type clazz = e.Type;
                //
                if (left != null && right != null)
                {
                    if (e.NodeType == ExpressionType.NotEqual)
                    {
                        return string.Format("{0} != {1}", left, right);
                    }
                    return string.Format("{0} = {1}", left, right);
                }
                else
                {
                    string value = left != null ? left : right;
                    if (e.NodeType == ExpressionType.NotEqual)
                    {
                        return string.Format("{0} IS NOT NULL", value);
                    }
                    return string.Format("{0} IS NULL", value);
                }
            }
            return null;
        }

        private static object ResolveMemberResource(object o, MemberInfo mi)
        {
            object value = null;
            switch (mi.MemberType)
            {
                case MemberTypes.Property:
                    value = ((PropertyInfo)mi).GetValue(value, null);
                    break;
                case MemberTypes.Field:
                    value = ((FieldInfo)mi).GetValue(value);
                    break;
            }
            return value;
        }

        private static string ResolveMemberExpression(Expression express, ReadOnlyCollection<ParameterExpression> key)
        {
            if (express is MemberExpression)
            {
                MemberExpression e = (MemberExpression)express;
                MemberInfo mi = e.Member;
                // 
                if (!key.Contains(e.Expression as ParameterExpression))
                {
                    object value = ResolveResourceExpress(e.Expression);
                    if (value == null)
                    {
                        value = ResolveMemberResource(value, mi);
                    }
                    Type clazz = e.Type;
                    //
                    if (clazz == typeof(string) || clazz == typeof(DateTime) ||
                        (clazz.IsGenericType && clazz.GetGenericTypeDefinition() == typeof(Nullable<>)))
                    {
                        return string.Format("'{0}'", value);
                    }
                    else
                    {
                        return string.Format("{0}", value);
                    }
                }
                ParameterExpression pi = (ParameterExpression)e.Expression;
                return string.Format("[{0}].[{1}]", pi.Name, mi.Name);
            }
            return null;
        }

        private static string ResolveConstantExpression(Expression express, ReadOnlyCollection<ParameterExpression> key)
        {
            if (express is ConstantExpression)
            {
                ConstantExpression e = (ConstantExpression)express;
                //
                Type clazz = e.Type;
                //
                if (clazz == typeof(string) || clazz == typeof(DateTime) ||
                    (clazz.IsGenericType && clazz.GetGenericTypeDefinition() == typeof(Nullable<>)))
                {
                    if (e.Value == null)
                    {
                        return null;
                    }
                    return string.Format("'{0}'", e.Value);
                }
                else
                {
                    return string.Format("{0}", e.Value);
                }
            }
            return null;
        }

        private static string ResolveExpression(Expression e, ReadOnlyCollection<ParameterExpression> key)
        {
            string express = null;
            object value = null;
            //
            if (e.NodeType == ExpressionType.And || e.NodeType == ExpressionType.AndAlso)
            {
                value = ResolveBinaryExpression(e, "AND", key);
            }
            else if (e.NodeType == ExpressionType.NotEqual)
            {
                value = ResolveNotAndEqualsExpress(e, key);
            }
            else if (e.NodeType == ExpressionType.Equal)
            {
                value = ResolveNotAndEqualsExpress(e, key);
            }
            else if (e.NodeType == ExpressionType.GreaterThan)
            {
                value = ResolveBinaryExpression(e, ">", key);
            }
            else if (e.NodeType == ExpressionType.Or || e.NodeType == ExpressionType.OrElse)
            {
                value = ResolveBinaryExpression(e, "OR", key);
            }
            else if (e.NodeType == ExpressionType.LessThan)
            {
                value = ResolveBinaryExpression(e, "<", key);
            }
            else if (e.NodeType == ExpressionType.Convert)
            {
                value = ResolveConvertExpression(e, key);
            }
            else if (e.NodeType == ExpressionType.GreaterThanOrEqual)
            {
                value = ResolveBinaryExpression(e, ">=", key);
            }
            else if (e.NodeType == ExpressionType.LessThanOrEqual)
            {
                value = ResolveBinaryExpression(e, "<=", key);
            }
            else if (e.NodeType == ExpressionType.Constant)
            {
                value = ResolveConstantExpression(e, key);
            }
            else if (e.NodeType == ExpressionType.MemberAccess)
            {
                value = ResolveMemberExpression(e, key);
            }
            else if (e.NodeType == ExpressionType.Call)
            {
                value = ResolveCallExpression(e);
            }
            if (value != null)
            {
                express += value;
            }
            return express;
        }

        private static bool IsAnonymousType(Type clazz)
        {
            string name = clazz.Name;
            return name.Contains("<>");
        }

        private static object ResolveResourceExpress(Expression express)
        {
            if (express != null)
            {
                if (express.NodeType == ExpressionType.Constant)
                {
                    ConstantExpression cs = (ConstantExpression)express;
                    //
                    Type clazz = cs.Type;
                    object value = cs.Value;
                    //
                    if (IsAnonymousType(clazz))
                    {
                        FieldInfo[] fs = clazz.GetFields();
                        if (fs.Length > 0)
                        {
                            FieldInfo f = fs[0];
                            value = f.GetValue(value);
                        }
                    }
                    return value;
                }
                else if (express.NodeType == ExpressionType.MemberAccess)
                {
                    return ResolveResourceExpress(((MemberExpression)express).Expression);
                }
                else if (express.NodeType == ExpressionType.Call)
                {
                    MethodCallExpression call = (MethodCallExpression)express;
                    //
                    MethodInfo met = call.Method;
                    var args = call.Arguments;
                    //
                    object[] values = new object[args.Count];
                    //
                    for (int i = 0; i < args.Count; i++)
                    {
                        values[i] = ResolveResourceExpress(args[i]);
                    }
                    return met.Invoke(call.Object, values);
                }
            }
            return null;
        }

        private static string ResolveCallExpression(Expression express)
        {
            if (express is MethodCallExpression)
            {
                MethodCallExpression e = (MethodCallExpression)express;
                //
                return Convert.ToString(ResolveResourceExpress(e));
            }
            return null;
        }

    }
}
