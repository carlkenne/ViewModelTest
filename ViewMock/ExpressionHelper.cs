using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ViewMockBase
{
    /// <summary>
    /// Helper class to simplify common expression and lamda tasks.
    /// </summary>
    public static class ExpressionHelper
    {
        public static T GetPropertyValue<T>(PropertyInfo pi, object input)
        {
            return (T) pi.GetValue(input, null);
        }

        public static PropertyInfo GetProperty<TModel, T>(Expression<Func<TModel, T>> expression)
        {
            var isExpressionOfDynamicComponent = expression.ToString().Contains("get_Item");

            if (isExpressionOfDynamicComponent)
                return GetDynamicComponentProperty(expression);

            MemberExpression memberExpression = GetMemberExpressionGeneric(expression);

            return (PropertyInfo)memberExpression.Member;
        }

        private static PropertyInfo GetDynamicComponentProperty<TModel, T>(Expression<Func<TModel, T>> expression)
        {
            var nextOperand = expression.Body;

            while (nextOperand != null)
            {
                if (nextOperand.NodeType == ExpressionType.Call)
                {
                    break;
                }

                if (nextOperand.NodeType != ExpressionType.Convert)
                    throw new ArgumentException("Expression not supported", "expression");

                var unaryExpression = (UnaryExpression)nextOperand;
                nextOperand = unaryExpression.Operand;
            }

            return null;
        }
        
        public static MemberExpression GetMemberExpressionGeneric<TModel, T>(Expression<Func<TModel, T>> expression)
        {
            return GetMemberExpression(expression, true);
        }

        public static MemberExpression GetMemberExpression(LambdaExpression expression, bool enforceCheck)
        {
            MemberExpression memberExpression = null;
            if (expression.Body.NodeType == ExpressionType.Convert)
            {
                var body = (UnaryExpression)expression.Body;
                memberExpression = body.Operand as MemberExpression;
            }
            else if (expression.Body.NodeType == ExpressionType.MemberAccess)
            {
                memberExpression = expression.Body as MemberExpression;
            }

            if (enforceCheck && memberExpression == null)
            {
                throw new ArgumentException("Not a member access", "expression");
            }

            return memberExpression;
        }

        public static string GetPropertyNameFrom<T,Y>(Expression<Func<T, Y>> property)
        {
            var member = GetMemberExpressionGeneric(property);
            return member.Member.Name;
        }

        public static MethodInfo GetMethodInfoFor<T>(Expression<Func<T, object>> expression)
        {
            if (expression.Body.NodeType == ExpressionType.Call)
            {
                var memberExpression = expression.Body as MethodCallExpression;
                return memberExpression.Method;
            }
            throw new ArgumentException("expression does not invoke method");
        }
    }
}
