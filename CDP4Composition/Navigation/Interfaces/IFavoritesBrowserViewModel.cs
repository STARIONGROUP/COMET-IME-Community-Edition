// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IFavoritesBrowserViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2019 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition
{
    using System;

    /// <summary>
    /// Interface definition for view models that are intended to house favoritable items
    /// </summary>
    public interface IFavoritesBrowserViewModel
    {
        /// <summary>
        /// Gets the a value indicating whether to display favorites.
        /// </summary>
        bool ShowOnlyFavorites { get; set; }
    }
}
