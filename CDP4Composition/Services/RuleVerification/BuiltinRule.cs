// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BuiltInRule.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Services
{
    using System.Collections.Generic;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// Abstract super class from which all <see cref="BuiltInRule"/> derive
    /// </summary>
    public abstract class BuiltInRule : IBuiltInRule
    {
        /// <summary>
        /// Verify an <see cref="Iteration"/> with respect to a <see cref="Rule"/> 
        /// </summary>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> that is to be verified.
        /// </param>
        /// <returns>
        /// an <see cref="IEnumerable{RuleViolation}"/>, this may be empty of no <see cref="RuleViolation"/>s have been found.
        /// </returns>
        public abstract IEnumerable<RuleViolation> Verify(Iteration iteration);
    }
}
