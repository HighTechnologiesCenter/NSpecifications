using System;
using System.Linq.Expressions;

namespace NSpecifications
{
    public interface ISpecification
    {
        LambdaExpression Expression { get; }
    }

    public interface ISpecification<T> : ISpecification
    {
        bool IsSatisfiedBy(T candidate);

        new Expression<Func<T, bool>> Expression { get; }

        /// <summary>
        /// Composes two specifications with an And operator.
        /// </summary>
        /// <param name="spec1">Specification</param>
        /// <param name="spec2">Specification</param>
        /// <returns>New specification</returns>
        public static ISpecification<T> operator &(ISpecification<T> spec1, ISpecification<T> spec2)
        {
            return new Spec<T>(spec1.Expression.And(spec2.Expression));
        }

        /// <summary>
        /// Composes two specifications with an Or operator.
        /// </summary>
        /// <param name="spec1">Specification</param>
        /// <param name="spec2">Specification</param>
        /// <returns>New specification</returns>
        public static ISpecification<T> operator |(ISpecification<T> spec1, ISpecification<T> spec2)
        {
            return new Spec<T>(spec1.Expression.Or(spec2.Expression));
        }

        /// <summary>
        /// Creates a new specification that negates a given specification.
        /// </summary>
        /// <param name="spec">Specification</param>
        /// <returns>New specification</returns>
        public static ISpecification<T> operator !(ISpecification<T> spec)
        {
            return new Spec<T>(spec.Expression.Not());
        }
    }
}
