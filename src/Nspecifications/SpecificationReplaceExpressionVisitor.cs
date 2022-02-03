using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NSpecifications
{
    public class SpecificationReplaceExpressionVisitor : ExpressionVisitor
    {
        private static MethodInfo _isMethod =
            typeof(SpecificationExtensions).GetMethod(nameof(SpecificationExtensions.Is),
                BindingFlags.Public | BindingFlags.Static)!;

        private static MethodInfo _areMethod =
            typeof(SpecificationExtensions).GetMethod(nameof(SpecificationExtensions.Are),
                BindingFlags.Public | BindingFlags.Static)!;

        /// <inheritdoc />
        public override Expression? Visit(Expression? node)
        {
            if (node is null)
                return null;

            if (node is MethodCallExpression methodCall)
            {
                if (EqualMethods(methodCall.Method, _isMethod))
                    return VisitIs(methodCall);

                if (EqualMethods(methodCall.Method, _areMethod))
                    return VisitAre(methodCall);
            }
            return base.Visit(node);
        }


        private bool EqualMethods(MethodInfo src, MethodInfo dst)
        {
            if (!src.IsGenericMethod && !dst.IsGenericMethod)
                return src == dst;

            if (src.IsGenericMethod != dst.IsGenericMethod)
                return false;

            if (src.GetGenericArguments().Length != dst.GetGenericArguments().Length)
                return false;

            return src == dst.MakeGenericMethod(src.GetGenericArguments());
        }

        private Expression VisitIs(MethodCallExpression node)
        {
            var objectNode = node.Arguments[0];
            var specNode = node.Arguments[1];

            var potentialSpecExpression = ExtractSpecificationExpression(specNode);

            if (potentialSpecExpression is null)
                return node;

            if (potentialSpecExpression is not LambdaExpression specExpression)
                return node;

            var replaceVisitor = new ExpressionReplaceVisitor(specExpression.Parameters[0], objectNode);

            var newBody = replaceVisitor.Visit(specExpression.Body);

            return newBody ?? node;
        }

        // ReSharper disable once UnusedParameter.Local
        private Expression VisitAre(MethodCallExpression node)
        {
            throw new NotImplementedException();
        }

        private Expression? ExtractSpecificationExpression(Expression? node)
        {
            if (node is null)
                return null;

            if (!node.Type.IsAssignableTo(typeof(ISpecification)))
                return node;

            if (!TryGetValue(node, out var value))
                return node;

            if (value is not ISpecification spec)
                return node;

            var resultExpression = spec.Expression;

            return resultExpression;
        }

        private bool TryGetValue(Expression expression, out object? value)
        {
            switch (expression)
            {
                case MemberExpression memberExpression:
                    if (memberExpression.Expression is null)
                    {
                        value = null;
                        return false;
                    }
                    if (!TryGetValue(memberExpression.Expression, out var instanceValue))
                    {
                        value = null;
                        return false;
                    }
                    try
                    {
                        switch (memberExpression.Member)
                        {
                            case FieldInfo fieldInfo:
                                value = fieldInfo.GetValue(instanceValue);
                                return true;

                            case PropertyInfo propertyInfo:
                                value = propertyInfo.GetValue(instanceValue);
                                return true;
                        }
                    }
                    catch
                    {
                        // Try again when we compile the delegate
                    }

                    break;

                case ConstantExpression constantExpression:
                    value = constantExpression.Value;
                    return true;

                case UnaryExpression unaryExpression
                    when (unaryExpression.NodeType == ExpressionType.Convert
                        || unaryExpression.NodeType == ExpressionType.ConvertChecked)
                    && unaryExpression.Type == unaryExpression.Operand.Type 
                    || Nullable.GetUnderlyingType(unaryExpression.Type) == unaryExpression.Operand.Type:
                    if (TryGetValue(unaryExpression.Operand, out var unaryValue))
                    {
                        value = unaryValue;
                        return true;
                    }

                    break;

                case NewExpression newExpression:
                    if (newExpression.Constructor is null)
                    {
                        value = null;
                        return false;
                    }
                    
                    var replacingValues = new Dictionary<Expression, object?>();

                    foreach (var arg in newExpression.Arguments)
                    {
                        if (!TryGetValue(arg, out var ctorArgValue))
                        {
                            value = null;
                            return false;
                        }
                        
                        replacingValues.Add(arg, ctorArgValue);
                    }

                    try
                    {
                        value = newExpression.Constructor.Invoke(replacingValues.Values.ToArray());
                        return true;
                    }
                    catch
                    {
                        // Try again when we compile the delegate
                    }
                    break;
            }

            try
            {
                value = Expression.Lambda<Func<object>>(
                        Expression.Convert(expression, typeof(object)))
                    .Compile()
                    .Invoke();

                return true;
            }
            catch
            {
                value = null;
                return false;
            }
        }
    }
}