// -------------------------------------------------------------------------------------------------
// <copyright file="FilterStringService.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA-2019 System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Services
{
    using System.Collections.Generic;

    using DevExpress.Xpf.Grid;

    using NLog;

    /// <summary>
    /// This behavior Shows or hides the deprecated rows in all browsers
    /// </summary>
    public class FilterStringService
    {
        /// <summary>
        /// The string to filter rows of things that are deprecated.
        /// </summary>
        private const string DeprecatedFilterString = "[IsHidden]=False";

        /// <summary>
        /// Object used to make this singleton threadsafe
        /// </summary>
        private static readonly object InstanceLock = new object();

        /// <summary>
        /// The NLog logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The DataControlBase controls of the open <see cref="DataControlBase"/>s
        /// </summary>
        public readonly Dictionary<string, DataControlBase> OpenControls = new Dictionary<string, DataControlBase>();

        /// <summary>
        /// Field backing the <see cref="FilterString"/> property
        /// </summary>
        private static FilterStringService filterString;

        /// <summary>
        /// Make the constructor private so the class can't be instantiated in a wrong way.
        /// </summary>
        private FilterStringService()
        {
        }

        /// <summary>
        /// Gets the deprecated filter.
        /// </summary>
        public static FilterStringService FilterString
        {
            get
            {
                if (filterString == null)
                {
                    lock (InstanceLock)
                    {
                        filterString = filterString ?? new FilterStringService();
                    }
                }

                return filterString;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the deprecated filter is enabled.
        /// </summary>
        public bool IsFilterActive { get; private set; } = true;

        /// <summary>
        /// Toggles the <see cref="IsFilterActive"/> property and refreshes all linked controls in the <see cref="OpenControls"/> property.
        /// </summary>
        public void ToggleIsFilterActive()
        {
            this.IsFilterActive = !this.IsFilterActive;
            this.RefreshAll();
        }

        /// <summary>
        /// Typesafely, add a grid control to the list of open controls <see cref="OpenControls"/>.
        /// </summary>
        /// <param name="gridControl">
        /// The grid Control.
        /// </param>
        public void AddGridControl(GridControl gridControl) => this.AddControl(gridControl);

        /// <summary>
        /// Typesafely, add a tree list control to the list of open controls <see cref="OpenControls"/>.
        /// </summary>
        /// <param name="treeListControl">
        /// The grid control.
        /// </param>
        public void AddTreeListControl(TreeListControl treeListControl) => this.AddControl(treeListControl);

        /// <summary>
        /// The add a control derived from DataControlBase to the list of open controls <see cref="OpenControls"/>.
        /// </summary>
        /// <param name="control">
        /// The tree list control.
        /// </param>
        private void AddControl(DataControlBase control)
        {
            if (control == null)
            {
                return;
            }

            control.FilterString = DeprecatedFilterString;
            this.OpenControls[control.Name] = control;
            this.RefreshControl(control);

            logger.Debug("{0} Added to the FilterStringService", control.Name);
        }

        /// <summary>
        /// The refresh all <see cref="OpenControls"/>.
        /// </summary>
        private void RefreshAll()
        {
            foreach (var grid in this.OpenControls.Values)
            {
                this.RefreshControl(grid);
            }
        }

        /// <summary>
        /// Handles the refreshing of the control when needed (initially and 
        /// </summary>
        /// <param name="control"></param>
        private void RefreshControl(DataControlBase control)
        {
            control.IsFilterEnabled = this.IsFilterActive;
            control.RefreshData();
        }
    }
}
