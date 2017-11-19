// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPanelViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition
{
    using System;

    /// <summary>
    /// Interface definition for view models that are intended to be docking panels
    /// </summary>
    public interface IPanelViewModel : IDisposable
    {
        /// <summary>
        /// Gets the caption displayed by the docking panel
        /// </summary>
        string Caption { get; }

        /// <summary>
        /// Gets the unique identifier of the view-model
        /// </summary>
        Guid Identifier { get;  }

        /// <summary>
        /// Gets the ToolTip displayed by the docking panel
        /// </summary>
        string ToolTip { get; }

        /// <summary>
        /// Gets the Data-Source
        /// </summary>
        string DataSource { get; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="IPanelViewModel"/> is dirty
        /// </summary>
        bool IsDirty { get; }
    }
}
