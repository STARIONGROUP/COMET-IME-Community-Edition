// -------------------------------------------------------------------------------------------------
// <copyright file="RuleNavBarItemComparerAscending.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4RelationshipEditor.Helpers
{
    using System.Collections.Generic;
    using ViewModels;

    /// <summary>
    /// Compares two <see cref="RuleNavBarItemViewModel"/> to be able to sort them by name.
    /// </summary>
    public class RuleNavBarItemComparerAscending : IComparer<RuleNavBarItemViewModel>
    {
        /// <summary>
        /// Compares two <see cref="RuleNavBarItemViewModel"/> by their <see cref="RuleNavBarItemViewModel.Name"/>
        /// </summary>
        /// <param name="x">The first item.</param>
        /// <param name="y">The second item.</param>
        /// <returns>An integer indicating the order.</returns>
        public int Compare(RuleNavBarItemViewModel x, RuleNavBarItemViewModel y)
        {
            return string.CompareOrdinal(x.Name, y.Name);
        }
    }
}
