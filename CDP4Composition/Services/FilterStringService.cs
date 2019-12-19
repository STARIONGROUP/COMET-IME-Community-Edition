// -------------------------------------------------------------------------------------------------
// <copyright file="FilterStringService.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA-2019 System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using Navigation.Interfaces;
    using NLog;

    /// <summary>
    /// This behavior Shows or hides the deprecated rows in all browsers
    /// </summary>
    public class FilterStringService
    {
        /// <summary>
        /// The string to filter rows of things that are deprecated.
        /// </summary>
        private static readonly string DeprecatedFilterString = "[IsDeprecated]=False";

        /// <summary>
        /// The string to filter rows of things that not favorite.
        /// </summary>
        private static readonly string FavoriteFilterString = "[IsFavorite]=True";

        /// <summary>
        /// The string to filter rows of things that not favorite and deprected.
        /// </summary>
        private static readonly string FavoriteAndHideDeprecatedFilterString =
            $"{FavoriteFilterString} AND {DeprecatedFilterString}";

        /// <summary>
        /// Object used to make this singleton threadsafe
        /// </summary>
        private static readonly object InstanceLock = new object();

        /// <summary>
        /// The NLog logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The view and viewmodel of deprecatable browsers.
        /// </summary>
        public readonly Dictionary<IPanelFilterableDataGridView, IDeprecatableBrowserViewModel> OpenDeprecatedControls =
            new Dictionary<IPanelFilterableDataGridView, IDeprecatableBrowserViewModel>();

        /// <summary>
        /// The view and viewmodel of favoritable browsers.
        /// </summary>
        public readonly Dictionary<IPanelFilterableDataGridView, IFavoritesBrowserViewModel> OpenFavoriteControls =
            new Dictionary<IPanelFilterableDataGridView, IFavoritesBrowserViewModel>();

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
        /// Gets the grid filter string service.
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
        /// Gets the viewmodel in charge of toggling the state of deprecatable visibility.
        /// </summary>
        public IDeprecatableToggleViewModel DeprecatableToggleViewModel { get; private set; }

        /// <summary>
        /// Registers a filterable panel view and viewmodel combo to this service.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="viewModel">The viewmodel.</param>
        public void RegisterForService(IPanelView view, IPanelViewModel viewModel)
        {
            if (!(view is IPanelFilterableDataGridView filterableView))
            {
                // if not filterable view, do not bother registration
                return;
            }

            if (viewModel is IDeprecatableBrowserViewModel deprecatableViewModel)
            {
                // deprecatable viewmodel
                this.AddDeprecatedControl(filterableView, deprecatableViewModel);
            }

            if (viewModel is IFavoritesBrowserViewModel favoritableViewModel)
            {
                // favoritable viewmodel
                this.AddFavoritesControl(filterableView, favoritableViewModel);
            }
        }

        /// <summary>
        /// Unregisters a panel view from all relevant collections.
        /// </summary>
        /// <param name="view">The view to unregister.</param>
        public void UnregisterFromService(IPanelView view)
        {
            if (!(view is IPanelFilterableDataGridView filterableView))
            {
                return;
            }

            this.RemoveView(filterableView);
        }

        /// <summary>
        /// Add the deprecatable browser view and viewmodel to the list of open controls <see cref="OpenDeprecatedControls"/>.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="viewModel">The view model.</param>
        private void AddDeprecatedControl(IPanelFilterableDataGridView view, IDeprecatableBrowserViewModel viewModel)
        {
            if (view == null || viewModel == null)
            {
                return;
            }

            this.OpenDeprecatedControls.Add(view, viewModel);
            this.RefreshControl(view);

            logger.Debug("{0} Added deprecatable to the FilterStringService", view.FilterableControl.Name);
        }

        /// <summary>
        /// Add the favoratable browser view and viewmodel to the list of open controls <see cref="OpenFavoriteControls"/>.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="viewModel">The view model.</param>
        private void AddFavoritesControl(IPanelFilterableDataGridView view, IFavoritesBrowserViewModel viewModel)
        {
            if (view == null || viewModel == null)
            {
                return;
            }

            this.OpenFavoriteControls.Add(view, viewModel);
            this.RefreshControl(view);

            logger.Debug("{0} Added to the Favorites FilterStringService", view.FilterableControl.Name);
        }

        /// <summary>
        /// Refresh all <see cref="OpenDeprecatedControls"/>.
        /// </summary>
        public void RefreshDeprecatableFilterAll()
        {
            foreach (var grid in this.OpenDeprecatedControls.Keys)
            {
                this.RefreshControl(grid);
            }
        }

        /// <summary>
        /// Refresh a favoratable grid or tree control.
        /// </summary>
        /// <param name="vm">The ViewModel of type <see cref="IFavoritesBrowserViewModel"/> to refresh</param>
        public void RefreshFavoriteBrowser(IFavoritesBrowserViewModel vm)
        {
            var registeredView = this.OpenFavoriteControls
                .FirstOrDefault(c => c.Value == vm).Key;

            if (registeredView != null)
            {
                this.RefreshControl(registeredView);
            }
        }

        /// <summary>
        /// Remove the view from all registered disctionaries.
        /// </summary>
        /// <param name="view">The view to remove.</param>
        private void RemoveView(IPanelFilterableDataGridView view)
        {
            this.OpenDeprecatedControls.Remove(view);
            this.OpenFavoriteControls.Remove(view);
        }

        /// <summary>
        /// Handles the refreshing of the control when needed
        /// </summary>
        /// <param name="view">The view to refresh.</param>
        private void RefreshControl(IPanelFilterableDataGridView view)
        {
            var control = view.FilterableControl;
            control.FilterString = string.Empty;

            // filters are always reenabled in case they were manually turned off.
            control.IsFilterEnabled = true;

            // deprecation state can be told no matter if the browser has the favorites vm assigned or not
            if (!this.DeprecatableToggleViewModel.ShowDeprecatedThings)
            {
                control.FilterString = DeprecatedFilterString;
            }

            // if the control is favoratable
            if (this.OpenFavoriteControls.TryGetValue(view, out var viewModel))
            {
                if (!this.DeprecatableToggleViewModel.ShowDeprecatedThings && viewModel.ShowOnlyFavorites)
                {
                    control.FilterString = FavoriteAndHideDeprecatedFilterString;
                }
                else if (this.DeprecatableToggleViewModel.ShowDeprecatedThings && viewModel.ShowOnlyFavorites)
                {
                    control.FilterString = FavoriteFilterString;
                }
                else if (!this.DeprecatableToggleViewModel.ShowDeprecatedThings && !viewModel.ShowOnlyFavorites)
                {
                    control.FilterString = DeprecatedFilterString;
                }
                else
                {
                    control.FilterString = string.Empty;
                }

                return;
            }

            control.RefreshData();
        }

        /// <summary>
        /// Register the viewmodel rsponsible for holding the state of the visibility of deprecatable things.
        /// </summary>
        /// <param name="showDeprecatedBrowserRibbonViewModel">The <see cref="IDeprecatableToggleViewModel"/> representing the vm.</param>
        public void RegisterDeprecatableToggleViewModel(
            IDeprecatableToggleViewModel showDeprecatedBrowserRibbonViewModel)
        {
            this.DeprecatableToggleViewModel = showDeprecatedBrowserRibbonViewModel;
        }
    }
}