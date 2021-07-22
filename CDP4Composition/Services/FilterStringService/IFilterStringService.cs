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
    /// <summary>
    /// The purpose of the <see cref="FilterStringService"/> is to set the <see cref="DevExpress.Xpf.Grid.DataControlBase.FilterString"/> property
    /// on registered view/view model combinations.
    /// </summary>
    public interface IFilterStringService
    {
        /// <summary>
        /// Gets or sets a value indicating whether <see cref="CDP4Common.CommonData.IDeprecatableThing"/> are visible.
        /// </summary>
        bool ShowDeprecatedThings { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether only favorited <see cref="Thing"/> are visible.
        /// </summary>
        bool ShowOnlyFavorites { get; set; }

        /// <summary>
        /// Registers a filterable panel view and viewmodel combo to this service.
        /// </summary>
        /// <param name="view">The view that is to be registered</param>
        /// <param name="viewModel">The viewmodel that is to be registered</param>
        void RegisterForService(IPanelViewModel viewModel);

        /// <summary>
        /// Unregisters a panel view from all relevant collections.
        /// </summary>
        /// <param name="view">The view to unregister.</param>
        void UnregisterFromService(IPanelViewModel viewModel);

        /// <summary>
        /// Refresh all <see cref="OpenDeprecatedControls"/>.
        /// </summary>
        void RefreshDeprecatableFilterAll();

        /// <summary>
        /// Refresh a favoratable grid or tree control.
        /// </summary>
        /// <param name="vm">The ViewModel of type <see cref="IFavoritesBrowserViewModel"/> to refresh</param>
        void RefreshFavoriteBrowser(IFavoritesBrowserViewModel vm);
    }
}