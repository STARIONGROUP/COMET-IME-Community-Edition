// ------------------------------------------------------------------------------------------------
// <copyright file="BreadCrumbComparer.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Utils
{
    using System.Collections.Generic;

    /// <summary>
    /// The purpose of the <see cref="BreadCrumbComparer"/> is to compare 2 instances of <see cref="IBreadCrumb"/>
    /// </summary>
    public class BreadCrumbComparer : IComparer<IBreadCrumb>
    {
        /// <summary>Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.</summary>
        /// <returns>A signed integer that indicates the relative values of <paramref name="x" /> and <paramref name="y" />, as shown in the following table.Value Meaning Less than zero<paramref name="x" /> is less than <paramref name="y" />.Zero<paramref name="x" /> equals <paramref name="y" />.Greater than zero<paramref name="x" /> is greater than <paramref name="y" />.</returns>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        public int Compare(IBreadCrumb x, IBreadCrumb y)
        {
            return System.String.Compare(x.BreadCrumb, y.BreadCrumb, System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
