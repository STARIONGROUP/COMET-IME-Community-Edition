// ------------------------------------------------------------------------------------------------
// <copyright file="DomainRowComparer.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.ViewModels
{
    using System;
    using System.Collections.Generic;

    using CDP4Common.CommonData;
    using CDP4Common.Comparers;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Mvvm;

    /// <summary>
    /// The <see cref="IComparer{T}"/> for domain-rows
    /// </summary>
    public class DomainRowComparer : IComparer<IRowViewModelBase<Thing>>
    {
        /// <summary>
        /// The comparer
        /// </summary>
        private static readonly DefinedThingComparer comparer = new DefinedThingComparer();

        /// <summary>
        /// Compares two <see cref="IRowViewModelBase{Thing}"/>
        /// </summary>
        /// <param name="x">The first <see cref="IRowViewModelBase{Thing}"/> to compare</param>
        /// <param name="y">The second <see cref="IRowViewModelBase{Thing}"/> to compare</param>
        /// <returns>
        /// Less than zero : x is "lower" than y 
        /// Zero: x "equals" y. 
        /// Greater than zero: x is "greater" than y.
        /// </returns>
        public int Compare(IRowViewModelBase<Thing> x, IRowViewModelBase<Thing> y)
        {
            if (!(x.Thing is DomainOfExpertise) || !(y.Thing is DomainOfExpertise))
            {
                throw new InvalidOperationException("one or both of the parameters is not an Element Definition row.");
            }

            return comparer.Compare((DomainOfExpertise)x.Thing, (DomainOfExpertise)y.Thing);
        }
    }
}