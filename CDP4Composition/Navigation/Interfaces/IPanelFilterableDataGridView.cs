// -------------------------------------------------------------------------------------------------
// <copyright file="IPanelFilterableDataGridView.cs" company="RHEA System S.A.">
//   Copyright (c) 2019 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Navigation.Interfaces
{
    using DevExpress.Xpf.Grid;

    /// <summary>
    /// An inteface for a panel with filterable data grid.
    /// </summary>
    public interface IPanelFilterableDataGridView
    {
        /// <summary>
        /// Gets the <see cref="DataControlBase"/> that is to be set up for filtering service.
        /// </summary>
        DataControlBase FilterableControl { get; }
    }
}
