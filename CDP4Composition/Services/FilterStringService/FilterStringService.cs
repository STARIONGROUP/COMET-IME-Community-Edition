// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FilterStringService.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2019 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru.
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
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
    /// on registered view model.
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
        /// The viewmodel of deprecatable browsers.
        /// </summary>
        public readonly List<IPanelFilterableDataGridViewModel> OpenDeprecatedViewModels =
            new List<IPanelFilterableDataGridViewModel>();

        /// <summary>
        /// The viewmodel of favoritable browsers.
        /// </summary>
        public readonly List<IPanelFilterableDataGridViewModel> OpenFavoriteViewModels =
            new List<IPanelFilterableDataGridViewModel>();

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
        public void RegisterForService(IPanelViewModel viewModel)
        {
            if (viewModel is not IPanelFilterableDataGridViewModel panelFilterableDataGridViewModel)
            {
                // if not filterable view, do not bother registration
                return;
            }

            if (viewModel is IDeprecatableBrowserViewModel)
            {
                // deprecatable viewmodel
                this.AddDeprecatedControl(panelFilterableDataGridViewModel);
            }

            if (viewModel is IFavoritesBrowserViewModel)
            {
                // favoritable viewmodel
                this.AddFavoritesControl(panelFilterableDataGridViewModel);
            }
        }

        /// <summary>
        /// Unregisters a panel view from all relevant collections.
        /// </summary>
        /// <param name="view">The view to unregister.</param>
        public void UnregisterFromService(IPanelViewModel viewModel)
        {
            if (viewModel is not IPanelFilterableDataGridViewModel panelFilterableDataGridViewModel)
            {
                return;
            }

            this.RemoveView(panelFilterableDataGridViewModel);
        }

        /// <summary>
        /// Add the deprecatable browser view and viewmodel to the list of open controls <see cref="OpenDeprecatedViewModels"/>.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="viewModel">The view model.</param>
        private void AddDeprecatedControl(IPanelFilterableDataGridViewModel viewModel)
        {
            if (viewModel is null)
            {
                return;
            }

            this.OpenDeprecatedViewModels.Add(viewModel);
            this.Refresh(viewModel);

            logger.Debug($"{viewModel} Added deprecatable to the FilterStringService");
        }

        /// <summary>
        /// Add the favoratable browser view and viewmodel to the list of open controls <see cref="OpenFavoriteViewModels"/>.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="viewModel">The view model.</param>
        private void AddFavoritesControl(IPanelFilterableDataGridViewModel viewModel)
        {
            if (viewModel is null)
            {
                return;
            }

            this.OpenFavoriteViewModels.Add(viewModel);
            this.Refresh(viewModel);

            logger.Debug($"{viewModel} Added to the Favorites FilterStringService");
        }

        /// <summary>
        /// Refresh all <see cref="OpenDeprecatedViewModels"/>.
        /// </summary>
        public void RefreshDeprecatableFilterAll()
        {
            foreach (var grid in this.OpenDeprecatedViewModels)
            {
                this.Refresh(grid);
            }
        }

        /// <summary>
        /// Refresh a favoratable grid or tree control.
        /// </summary>
        /// <param name="vm">The ViewModel of type <see cref="IFavoritesBrowserViewModel"/> to refresh</param>
        public void RefreshFavoriteBrowser(IFavoritesBrowserViewModel vm)
        {
            var registeredViewModel = this.OpenFavoriteViewModels
                .FirstOrDefault(c => c == vm);

            if (registeredViewModel is not null)
            {
                this.Refresh(registeredViewModel);
            }
        }

        /// <summary>
        /// Remove the view from all registered disctionaries.
        /// </summary>
        /// <param name="view">The view to remove.</param>
        private void RemoveView(IPanelFilterableDataGridViewModel viewModel)
        {
            this.OpenDeprecatedViewModels.Remove(viewModel);
            this.OpenFavoriteViewModels.Remove(viewModel);
        }

        /// <summary>
        /// Handles the refreshing of the control when needed
        /// </summary>
        /// <param name="viewModel">The view to refresh.</param>
        private void Refresh(IPanelFilterableDataGridViewModel viewModel)
        {
            viewModel.FilterString = string.Empty;

            // filters are always reenabled in case they were manually turned off.
            viewModel.IsFilterEnabled = true;

            // deprecation state can be told no matter if the browser has the favorites vm assigned or not
            if (!this.ShowDeprecatedThings)
            {
                viewModel.FilterString = DeprecatedFilterString;
            }

            // if the control is favoratable
            if (this.OpenFavoriteViewModels.Contains(viewModel))
            {
                var favoritesBrowserViewModel = (IFavoritesBrowserViewModel)viewModel;

                if (!this.ShowDeprecatedThings && favoritesBrowserViewModel.ShowOnlyFavorites)
                {
                    viewModel.FilterString = FavoriteAndHideDeprecatedFilterString;
                }
                else if (this.ShowDeprecatedThings && favoritesBrowserViewModel.ShowOnlyFavorites)
                {
                    viewModel.FilterString = FavoriteFilterString;
                }
                else if (!this.ShowDeprecatedThings && !favoritesBrowserViewModel.ShowOnlyFavorites)
                {
                    viewModel.FilterString = DeprecatedFilterString;
                }
                else
                {
                    viewModel.FilterString = string.Empty;
                }
            }
        }
    }
}
