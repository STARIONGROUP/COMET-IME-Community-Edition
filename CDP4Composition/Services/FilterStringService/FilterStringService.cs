// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FilterStringService.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2019 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru.
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//    Lesser General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Services
{
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Linq;
    using Navigation.Interfaces;
    using NLog;

    /// <summary>
    /// The purpose of the <see cref="FilterStringService"/> is to set the <see cref="DevExpress.Xpf.Grid.DataControlBase.FilterString"/> property
    /// on registered view/view model combinations.
    /// </summary>
    [Export(typeof(IFilterStringService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class FilterStringService : IFilterStringService
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
        //private static readonly object InstanceLock = new object();

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
        /// Gets or sets a value indicating whether <see cref="CDP4Common.CommonData.IDeprecatableThing"/> are visible.
        /// </summary>
        public bool ShowDeprecatedThings { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether only favorited <see cref="Thing"/> are visible.
        /// </summary>
        public bool ShowOnlyFavorites { get; set; } = false;

        /// <summary>
        /// Registers a filterable panel view and viewmodel combo to this service.
        /// </summary>
        /// <param name="view">The view that is to be registered</param>
        /// <param name="viewModel">The viewmodel that is to be registered</param>
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
            if (!this.ShowDeprecatedThings)
            {
                control.FilterString = DeprecatedFilterString;
            }

            // if the control is favoratable
            if (this.OpenFavoriteControls.TryGetValue(view, out var viewModel))
            {
                if (!this.ShowDeprecatedThings && viewModel.ShowOnlyFavorites)
                {
                    control.FilterString = FavoriteAndHideDeprecatedFilterString;
                }
                else if (this.ShowDeprecatedThings && viewModel.ShowOnlyFavorites)
                {
                    control.FilterString = FavoriteFilterString;
                }
                else if (!this.ShowDeprecatedThings && !viewModel.ShowOnlyFavorites)
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
    }
}