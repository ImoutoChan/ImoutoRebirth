using System.Linq.Expressions;
using System.Reflection;

namespace ImoutoRebirth.Lilin.Infrastructure.ExpressionHelpers;

public static class PredicateBuilder
{
    public static Expression<Func<T, bool>>? Get<T>() => null;

    public static Expression<Func<T, bool>> Get<T>(this Expression<Func<T, bool>> predicate) => predicate;

    public static Expression<Func<T, bool>> Or<T>(
        this Expression<Func<T, bool>>? expr,
        Expression<Func<T, bool>> or)
    {
        if (expr == null)
            return or;

        PredicateBuilder.Replace(
            (object)or,
            (object)or.Parameters[0],
            (object)expr.Parameters[0]);

        return Expression.Lambda<Func<T, bool>>(
            (Expression)Expression.Or(expr.Body, or.Body),
            (IEnumerable<ParameterExpression>)expr.Parameters);
    }

    public static Expression<Func<T, bool>> And<T>(
        this Expression<Func<T, bool>>? expr,
        Expression<Func<T, bool>> and)
    {
        if (expr == null)
            return and;

        Replace(
            (object)and,
            (object)and.Parameters[0],
            (object)expr.Parameters[0]);

        return Expression.Lambda<Func<T, bool>>(
            (Expression)Expression.And(expr.Body, and.Body),
            (IEnumerable<ParameterExpression>)expr.Parameters);
    }

    private static void Replace(object instance, object old, object replacement)
    {
        for (var type = instance.GetType(); type != null; type = type.BaseType)
        {
            foreach (var field in type.GetFields(
                         BindingFlags.Instance
                         | BindingFlags.Public
                         | BindingFlags.NonPublic))
            {
                var instance1 = field.GetValue(instance);

                if (instance1 == null || instance1.GetType().Assembly != typeof(Expression).Assembly) 
                    continue;
                
                if (instance1 == old)
                {
                    field.SetValue(instance, replacement);
                }
                else
                {
                    Replace(instance1, old, replacement);
                }
            }
        }
    }
}
