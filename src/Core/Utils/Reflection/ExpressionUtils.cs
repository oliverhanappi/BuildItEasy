using System;
using System.Linq.Expressions;

namespace BuildItEasy.Utils.Reflection
{
    public static class ExpressionUtils
    {
        public static string GetMemberName<TSource, TMember>(Expression<Func<TSource, TMember>> expression)
        {
            return GetMemberName((Expression) expression);
        }

        public static string GetMemberName(Expression expression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            switch (expression)
            {
                case MemberExpression memberExpression:
                    return memberExpression.Member.Name;

                case LambdaExpression lambdaExpression:
                    return GetMemberName(lambdaExpression.Body);
                
                case UnaryExpression unaryExpression:
                    return GetMemberName(unaryExpression.Operand);
                
                default:
                    throw new ArgumentException($"Unsupported expression: {expression}", nameof(expression));
            }
        }
    }
}
