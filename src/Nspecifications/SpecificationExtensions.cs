﻿using System.Collections.Generic;
using System.Linq;

namespace NSpecifications
{
    /// <summary>
    /// Extensions that facilitates the usage of ISpecifications.
    /// </summary>
    public static class SpecificationExtensions
    {
        /// <summary>
        /// Checks if a certain candidate meets a given specification.
        /// </summary>
        /// <param name="candidate"></param>
        /// <param name="spec"> Спецификация. </param>
        /// <returns>New specification</returns>
        public static bool Is<T>(this T candidate, ISpecification<T> spec)
        {
            return spec.IsSatisfiedBy(candidate);
        }

        /// <summary>
        /// Checks if a certain collection of candidates meets a given specification.
        /// </summary>
        /// <param name="candidates">Candidates</param>
        /// <param name="spec"> Спецификация. </param>
        /// <returns>New specification</returns>
        public static  bool Are<T>(this IEnumerable<T> candidates, ISpecification<T> spec)
        {
            return candidates.All(spec.IsSatisfiedBy);
        }

        /// <summary>
        /// Composes two ISpecifications using an And operator.
        /// </summary>
        /// <typeparam name="T">Candidate type</typeparam>
        /// <param name="spec1"></param>
        /// <param name="spec2"></param>
        /// <returns></returns>
        public static ISpecification<T> And<T>(this ISpecification<T> spec1, ISpecification<T> spec2)
        {
            return new Spec<T>(spec1.Expression.And(spec2.Expression));
        }

        /// <summary>
        /// Composes two ISpecifications using an Or operator.
        /// </summary>
        /// <typeparam name="T">Candidate type</typeparam>
        /// <param name="spec1"></param>
        /// <param name="spec2"></param>
        /// <returns></returns>
        public static ISpecification<T> Or<T>(this ISpecification<T> spec1, ISpecification<T> spec2)
        {
            return new Spec<T>(spec1.Expression.Or(spec2.Expression));
        }

        /// <summary>
        /// Negates an ISpecification.
        /// </summary>
        /// <typeparam name="T">Candidate type</typeparam>
        /// <param name="inner">Inner specification</param>
        /// <returns></returns>
        public static ISpecification<T> Not<T>(this ISpecification<T> inner)
        {
            return new Spec<T>(inner.Expression.Not());
        }
    }
}
