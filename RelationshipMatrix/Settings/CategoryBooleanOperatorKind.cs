// ------------------------------------------------------------------------------------------------
// <copyright file="CategoryBooleanOperatorKind.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2019 Starion Group S.A.
// </copyright>
// -----------------------------------------------------------------------------------------------

namespace CDP4RelationshipMatrix.Settings
{
    /// <summary>
    /// Boolean operators intended to define combinations of selected categories.
    /// </summary>
    public enum CategoryBooleanOperatorKind
    {
        /// <summary>
        /// Requires both categories to be in each item returned. If one category is contained in the
        /// Thing and the other is not, the item is not included in the resulting list. 
        /// </summary>
        AND,

        /// <summary>
        /// Either category (or both) will be in the returned Thing.
        /// </summary>
        OR
    }
}
