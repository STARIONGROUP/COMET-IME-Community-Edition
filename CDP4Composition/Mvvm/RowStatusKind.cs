// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RowStatusKind.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Mvvm
{
    using CDP4Common.CommonData;

    /// <summary>
    /// An enumeration of the kind of row status.
    /// </summary>
    public enum RowStatusKind
    {
        /// <summary>
        /// The row represent an active <see cref="Thing"/>
        /// </summary>
        Active = 0,

        /// <summary>
        /// The row represent an inactive <see cref="Thing"/>
        /// </summary>
        Inactive = 1
    }
}