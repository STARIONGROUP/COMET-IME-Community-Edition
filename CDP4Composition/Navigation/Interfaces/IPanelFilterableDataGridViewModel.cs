// -------------------------------------------------------------------------------------------------
// <copyright file="IPanelFilterableDataGridView.cs" company="RHEA System S.A.">
//   Copyright (c) 2019 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Navigation.Interfaces
{
    /// <summary>
    /// An interface for a panel view model with filterable data grid.
    /// </summary>
    public interface IPanelFilterableDataGridViewModel
    {
        /// <summary>
        /// The filter string which the view is bound to.
        /// </summary>
        string FilterString { get; set; }

        /// <summary>
        /// The enabled value of the filter which the view is bound to.
        /// </summary>
        bool IsFilterEnabled { get; set; }
    }
}
