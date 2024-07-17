// -------------------------------------------------------------------------------------------------
// <copyright file="SortOrder.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2019 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4RelationshipMatrix.Settings
{
    /// <summary>
    /// Determines the order in how information is sorted or arranged.
    /// </summary>
    public enum SortOrder
    {
        /// <summary>
        /// Asserts that the order is ascending; from lowest to highest
        /// </summary>
        /// <example>
        /// 1, 2, 3, 4, 5 and a, b, c, d, e, f
        /// </example>
        Ascending,

        /// <summary>
        /// Asserts that the order is descending; from highest to lowest
        /// </summary>
        /// <example>
        /// 5, 4, 3, 2, 1 and f, e, d, c, b, a
        /// </example>
        Descending
    }
}