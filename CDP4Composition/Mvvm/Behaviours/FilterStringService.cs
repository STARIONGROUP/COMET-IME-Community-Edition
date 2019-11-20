// -------------------------------------------------------------------------------------------------
// <copyright file="FilterStringService.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA-2019 System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition
{
    using System.Collections.Generic;
    using DevExpress.Xpf.Grid;
    
    /// <summary>
    /// This behavior Shows or hides the deprecated rows in all browsers
    /// </summary>
    public class FilterStringService
    {
        /// <summary>
        /// The string to filter rows of things that are deprecated.
        /// </summary>
        private const string DeprecatedFilterString = "[IsDeprecated]=False";

        /// <summary>
        /// The DataControlBase controls of the open <see cref="DataControlBase"/>s
        /// </summary>
        public readonly Dictionary<string, DataControlBase> OpenControls = new Dictionary<string, DataControlBase>();

        /// <summary>
        /// Gets the deprecated filter.
        /// </summary>
        public static FilterStringService FilterString { get; } = new FilterStringService();

        /// <summary>
        /// Gets or sets a value indicating whether the deprecated filter is enabled.
        /// </summary>
        public bool IsFilterActive { get; set; }

        /// <summary>
        /// The refresh all <see cref="OpenControls"/> and the <see cref="OpenTreeListControls"/>.
        /// </summary>
        public void RefreshAll()
        {
            foreach (var grid in this.OpenControls.Values)
            {
                this.RefreshElement(grid);
            }
        }

        private void RefreshElement(DataControlBase control)
        {
            control.IsFilterEnabled = this.IsFilterActive;
            control.RefreshData();
        }

        /// <summary>
        /// The add grid control.
        /// </summary>
        /// <param name="gridControl">
        /// The grid Control.
        /// </param>
        public void AddGridControl(GridControl gridControl)
        {
            gridControl.FilterString = DeprecatedFilterString;
            this.OpenControls[gridControl.Name] = gridControl;
            this.RefreshElement(gridControl);
        }

        /// <summary>
        /// The add tree list control.
        /// </summary>
        /// <param name="treeListControl">
        /// The tree list control.
        /// </param>
        public void AddTreeListControl(TreeListControl treeListControl)
        {
            treeListControl.FilterString = DeprecatedFilterString;
            this.OpenControls[treeListControl.Name] = treeListControl;
            this.RefreshElement(treeListControl);
        }
    }
}