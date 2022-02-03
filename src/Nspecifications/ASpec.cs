using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace NSpecifications
{
    /// <summary>
    /// Abstract Specification defined by an Expressions that can be used on IQueryables.
    /// </summary>
    /// <typeparam name="T">The type of the candidate.</typeparam>
    public abstract class ASpec<T> : ISpecification<T>
    {
        /// <summary>
        /// Holds the compiled expression so that it doesn't need to compile it everytime.
        /// </summary>
        Func<T, bool>? _compiledFunc;

        /// <summary>
        /// Checks if a certain candidate meets a given specification.
        /// </summary>
        /// <param name="candidate"></param>
        /// <returns>New specification</returns>
        public virtual bool IsSatisfiedBy(T candidate)
        {
            _compiledFunc ??= Expression.Compile();
            return _compiledFunc(candidate);
        }

        /// <summary>
        /// Expression that defines the specification.
        /// </summary>
        public abstract Expression<Func<T, bool>> Expression { get; }

        /// <summary>
        /// Composes two specifications with an And operator.
        /// </summary>
        /// <param name="spec1">Specification</param>
        /// <param name="spec2">Specification</param>
        /// <returns>New specification</returns>
        public static ASpec<T> operator &(ASpec<T> spec1, ASpec<T> spec2)
        {
            return new Spec<T>(spec1.Expression.And(spec2.Expression));
        }

        /// <summary>
        /// Composes two specifications with an Or operator.
        /// </summary>
        /// <param name="spec1">Specification</param>
        /// <param name="spec2">Specification</param>
        /// <returns>New specification</returns>
        public static ASpec<T> operator |(ASpec<T> spec1, ASpec<T> spec2)
        {
            return new Spec<T>(spec1.Expression.Or(spec2.Expression));
        }

        /// <summary>
        /// Combines a specification with a boolean value. 
        /// The candidate meets the criteria only when the boolean is true.
        /// </summary>
        /// <param name="value">Boolean value</param>
        /// <param name="spec">Specification</param>
        /// <returns>New specification</returns>
        public static ASpec<T> operator ==(bool value, ASpec<T> spec)
        {
            return value ? spec : !spec;
        }

        /// <summary>
        /// Combines a specification with a boolean value. 
        /// The candidate meets the criteria only when the boolean is true.
        /// </summary>
        /// <param name="value">Boolean value</param>
        /// <param name="spec">Specification</param>
        /// <returns>New specification</returns>
        public static ASpec<T> operator ==(ASpec<T> spec, bool value)
        {
            return value ? spec : !spec;
        }

        /// <summary>
        /// Combines a specification with a boolean value. 
        /// The candidate meets the criteria only when the boolean is false.
        /// </summary>
        /// <param name="value">Boolean value</param>
        /// <param name="spec">Specification</param>
        /// <returns>New specification</returns>
        public static ASpec<T> operator !=(bool value, ASpec<T> spec)
        {
            return value ? !spec : spec;
        }

        /// <summary>
        /// Combines a specification with a boolean value. 
        /// The candidate meets the criteria only when the boolean is false.
        /// </summary>
        /// <param name="value">Boolean value</param>
        /// <param name="spec">Specification</param>
        /// <returns>New specification</returns>
        public static ASpec<T> operator !=(ASpec<T> spec, bool value)
        {
            return value ? !spec : spec;
        }

        /// <summary>
        /// Creates a new specification that negates a given specification.
        /// </summary>
        /// <param name="spec">Specification</param>
        /// <returns>New specification</returns>
        public static ASpec<T> operator !(ASpec<T> spec)
        {
            return new Spec<T>(spec.Expression.Not());
        }

        /// <summary>
        /// Allows using ASpec[T] in place of a lambda expression.
        /// </summary>
        /// <param name="spec"></param>
        public static implicit operator Expression<Func<T, bool>>(ASpec<T> spec)
        {
            return spec.Expression;
        }

        /// <summary>
        /// Allows using ASpec[T] in place of Func[T, bool].
        /// </summary>
        /// <param name="spec"></param>
        public static implicit operator Func<T, bool>(ASpec<T> spec)
        {
            return spec.IsSatisfiedBy;
        }

        /// <summary>
        /// Converts the expression into a string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Expression.ToString();
        }

        public override bool Equals(object? obj)
        {
            return obj is ASpec<T> spec &&
                   EqualityComparer<Expression<Func<T, bool>>>.Default.Equals(Expression, spec.Expression);
        }

        public override int GetHashCode()
        {
            return -1489834557 + EqualityComparer<Expression<Func<T, bool>>>.Default.GetHashCode(Expression);
        }

        LambdaExpression ISpecification.Expression => Expression;
    }
}
