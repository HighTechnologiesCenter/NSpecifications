using System;
using System.Linq;
using System.Linq.Expressions;

namespace NSpecifications
{
    /// <summary>
    /// Enables the efficient, dynamic composition of query predicates.
    /// </summary>
    public static class PredicateBuilder
    {
        /// <summary>
        /// Creates a predicate that evaluates to true.
        /// </summary>
        public static Expression<Func<T, bool>> True<T>()
        {
            return param => true;
        }

        /// <summary>
        /// Creates a predicate that evaluates to false.
        /// </summary>
        public static Expression<Func<T, bool>> False<T>()
        {
            return param => false;
        }

        /// <summary>
        /// Creates a predicate expression from the specified lambda expression.
        /// </summary>
        public static Expression<Func<T, bool>> Create<T>(Expression<Func<T, bool>> predicate)
        {
            return predicate;
        }

        /// <summary>
        /// Combines the first predicate with the second using the logical "and".
        /// </summary>
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> first,
                                                       Expression<Func<T, bool>> second)
        {
            return (Expression<Func<T, bool>>)first.And((LambdaExpression)second);
        }

        /// <summary>
        /// Combines the first predicate with the second using the logical "or".
        /// </summary>
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> first,
                                                      Expression<Func<T, bool>> second)
        {
            return (Expression<Func<T, bool>>)first.Or((LambdaExpression)second);
        }

        /// <summary>
        /// Negates the predicate.
        /// </summary>
        public static Expression<Func<T, bool>> Not<T>(this Expression<Func<T, bool>> expression)
        {
            UnaryExpression negated = Expression.Not(expression.Body);
            return Expression.Lambda<Func<T, bool>>(negated, expression.Parameters);
        }

        /// <summary>
        ///     Логическая операция И.
        /// </summary>
        /// <param name="leftPredicate"> Левый предикат. </param>
        /// <param name="rightPredicate"> Правый предикат. </param>
        /// <returns> Итоговый предикат. </returns>
        private static LambdaExpression And(this LambdaExpression leftPredicate, LambdaExpression rightPredicate)
        {
            return CreateLogicalExpression(LogicalOperation.And, leftPredicate, rightPredicate);
        }

        /// <summary>
        ///     Логическая операция ИЛИ.
        /// </summary>
        /// <param name="leftPredicate"> Левый предикат. </param>
        /// <param name="rightPredicate"> Правый предикат. </param>
        /// <returns> Итоговый предикат. </returns>
        private static LambdaExpression Or(this LambdaExpression leftPredicate, LambdaExpression rightPredicate)
        {
            return CreateLogicalExpression(LogicalOperation.Or, leftPredicate, rightPredicate);
        }

        /// <summary>
        ///     Создание предиката на основе логической операции.
        /// </summary>
        /// <param name="logicalOperation">Тип логической операции. </param>
        /// <param name="leftPredicate">Левый предикат. </param>
        /// <param name="rightPredicate">правый предикат. </param>
        /// <returns>итоговый предикат. </returns>
        private static LambdaExpression CreateLogicalExpression(
            LogicalOperation logicalOperation,
            LambdaExpression leftPredicate,
            LambdaExpression rightPredicate
        )
        {
            if (!leftPredicate.Parameters.Select(x => x.Type)
                    .SequenceEqual(rightPredicate.Parameters.Select(x => x.Type)))
                throw new InvalidOperationException(
                    "Параметры левого предиката должны совпадать с параметрами правого");

            ExpressionType expressionType;

            switch (logicalOperation)
            {
                case LogicalOperation.And:
                    expressionType = ExpressionType.AndAlso;
                    break;
                case LogicalOperation.Or:
                    expressionType = ExpressionType.OrElse;
                    break;
                default:
                    throw new NotSupportedException();
            }

            var newParameters = leftPredicate.Parameters.Select(x => Expression.Parameter(x.Type, x.Name)).ToArray();

            var res = leftPredicate.Parameters.Zip(rightPredicate.Parameters.Zip(newParameters, (r, n) => (r, n)),
                (l, rn) => (new ExpressionReplaceVisitor(l, rn.n), new ExpressionReplaceVisitor(rn.r, rn.n)));


            var leftPredicateBody = leftPredicate.Body;
            var rightPredicateBody = rightPredicate.Body;

            foreach (var (leftPredicateReplacer, rightPredicateReplacer) in res)
            {
                leftPredicateBody = leftPredicateReplacer.Visit(leftPredicateBody) ??
                                    throw new InvalidOperationException();
                rightPredicateBody = rightPredicateReplacer.Visit(rightPredicateBody) ??
                                     throw new InvalidOperationException();
            }

            var binary = Expression.MakeBinary(expressionType, leftPredicateBody, rightPredicateBody);

            return Expression.Lambda(binary, newParameters);
        }
    }
}
