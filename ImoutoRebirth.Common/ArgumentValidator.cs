using System;
using System.Linq.Expressions;

namespace ImoutoRebirth.Common
{
    public static class ArgumentValidator
    {
        public static void NotNull(params Expression<Func<object>>[] callerArgs)
        {
            foreach (var expression in callerArgs)
            {
                if (GetParameterValue(expression) == null)
                    throw new ArgumentNullException(GetParameterName(expression));
            }
        }

        public static void Requires(Func<bool> predicate, string name)
        {
            if (!predicate.Invoke())
                throw new ArgumentException(name);
        }

        private static object GetParameterValue(Expression<Func<object>> expr) => expr.Compile().Invoke();

        private static string GetParameterName(Expression<Func<object>> expr)
        {
            //First try to get a member expression directly.
            //If it fails we know there is a type conversion: parameter is not an object 
            //(or we have an invalid lambda that will make us crash)
            //Get the "to object" conversion unary expression operand and then 
            //get the member expression of that and we're set.
            var m = expr.Body as MemberExpression 
                    ?? (expr.Body as UnaryExpression)?.Operand as MemberExpression;

            if (m == null)
                throw new InvalidOperationException("Incorrect validation argument");

            return m.Member.Name;
        }
    }
}
