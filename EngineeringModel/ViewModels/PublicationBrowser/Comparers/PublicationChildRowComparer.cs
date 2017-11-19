// ------------------------------------------------------------------------------------------------
// <copyright file="PublicationChildRowComparer.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Comparers
{
    using System;
    using System.Collections.Generic;
    using CDP4Common.CommonData;
    using CDP4Common.Comparers;
    using CDP4Common.EngineeringModelData;
    using CDP4Composition.Mvvm;
    using CDP4EngineeringModel.ViewModels;

    /// <summary>
    /// The <see cref="IComparer{T}"/> used to sort the child rows of the <see cref="PublicationDomainOfExpertiseRowViewModel"/>
    /// </summary>
    public class PublicationChildRowComparer : IComparer<IRowViewModelBase<Thing>>
    {
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
            if (x == null || y == null)
            {
                throw new ArgumentNullException();
            }

            if (!(x is PublicationParameterOrOverrideRowViewModel) || !(y is PublicationParameterOrOverrideRowViewModel))
            {
                throw new NotSupportedException("The list contains other types of row than the specified ones.");

            }

            var comparer = new ParameterBaseComparer();
            return comparer.Compare((ParameterOrOverrideBase)x.Thing, (ParameterOrOverrideBase)y.Thing);
        }
    }
}