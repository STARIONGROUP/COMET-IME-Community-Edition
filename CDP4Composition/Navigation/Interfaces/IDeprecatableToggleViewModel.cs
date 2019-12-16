// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDeprecatableToggleViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2019 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition
{
    using System;

    /// <summary>
    /// Interface definition for view models that are intended to have control over deprecatable thing visibility
    /// </summary>
    public interface IDeprecatableToggleViewModel
    {
        /// <summary>
        /// Gets the a value indicating whether to display deprecatable things.
        /// </summary>
        bool ShowDeprecatedThings { get; set; }
    }
}
