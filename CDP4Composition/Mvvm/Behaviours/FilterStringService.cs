// -------------------------------------------------------------------------------------------------
// <copyright file="FilterStringService.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA-2018 System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition
{
    using System.Collections.Generic;
    using System.Windows;
    using DevExpress.Mvvm.UI.Interactivity;
    using DevExpress.Xpf.Grid;
    
    /// <summary>
    /// This behavior Shows or hides the deprecated rows in all browsers
    /// </summary>
    public class FilterStringService : Behavior<FrameworkElement>
    {
        /// <summary>
        /// The string to filter rows of things that are deprecated.
        /// </summary>
        private const string DeprecatedFilterString = "[IsDeprecated]=False";

        /// <summary>
        /// The deprecated filter.
        /// </summary>
        private static readonly FilterStringService filterString = new FilterStringService();

        /// <summary>
        /// The GridControls of the open <see cref="GridControl"/>s
        /// </summary>
        public readonly Dictionary<string, GridControl> OpenGridControls = new Dictionary<string, GridControl>();

        /// <summary>
        /// The GridControls of the open  open <see cref="TreeListControl"/>s
        /// </summary>
        public readonly Dictionary<string, TreeListControl> OpenTreeListControls = new Dictionary<string, TreeListControl>();

        /// <summary>
        /// Gets the deprecated filter.
        /// </summary>
        public static FilterStringService FilterString
        {
            get { return filterString; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the deprecated filter is enabled.
        /// </summary>
        public bool IsFilterActive { get; set; }

        /// <summary>
        /// The refresh all <see cref="OpenGridControls"/> and the <see cref="OpenTreeListControls"/>.
        /// </summary>
        public void RefreshAll()
        {
            foreach (var grid in this.OpenGridControls.Values)
            {
                grid.IsFilterEnabled = this.IsFilterActive;
                grid.RefreshData();
            }

            foreach (var tree in this.OpenTreeListControls.Values)
            {
                tree.IsFilterEnabled = this.IsFilterActive;
                tree.RefreshData();
            }
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
            this.OpenGridControls[gridControl.Name] = gridControl;
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
            this.OpenTreeListControls[treeListControl.Name] = treeListControl;
        }
    }
}