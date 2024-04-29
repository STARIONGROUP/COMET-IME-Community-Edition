// ------------------------------------------------------------------------------------------------
// <copyright file="IPanelNavigationService.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2020 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Ahmed Abulwafa Ahmed
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
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// ------------------------------------------------------------------------------------------------
namespace CDP4Composition.Navigation
{
    using System;

    using CDP4Dal;
    using CDP4Dal.Composition;

    using Interfaces;
    
    /// <summary>
    /// The Interface for Panel Navigation Service
    /// </summary>
    public interface IPanelNavigationService
    {
        /// <summary>
        /// Opens the view associated to the provided view-model in the dock
        /// </summary>
        /// <param name="viewModel">
        /// The <see cref="IPanelViewModel"/> for which the associated view needs to be opened
        /// </param>
        /// <remarks>
        /// The data context of the view is the <see cref="IPanelViewModel"/>
        /// </remarks>
        void OpenInDock(IPanelViewModel viewModel);

        /// <summary>
        /// Opens the view associated to a view-model. The view-model is identified by its <see cref="INameMetaData.Name"/>.
        /// </summary>
        /// <param name="viewModelName">The name we want to compare to the <see cref="INameMetaData.Name"/> of the view-models.</param>
        /// <param name="session">The <see cref="ISession"/> associated.</param>
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService"/>.</param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/>.</param>
        void OpenInDock(string viewModelName, ISession session, IThingDialogNavigationService thingDialogNavigationService, IDialogNavigationService dialogNavigationService);

        /// <summary>
        /// Opens the view associated with the <see cref="IPanelViewModel"/> in the AddIn
        /// </summary>
        /// <param name="viewModel">The <see cref="IPanelViewModel"/> to open</param>
        void OpenInAddIn(IPanelViewModel viewModel);

        /// <summary>
        /// Re-opens an exisiting View associated to the provided view-model, or opens a new View
        /// Re-opening is done by sending a <see cref="CDPMessageBus"/> event.
        /// This event can be handled by more specific code,  for example in the addin, where some
        /// ViewModels should not close at all. For those viewmodels visibility is toggled on every
        /// <see cref="NavigationPanelEvent"/> event that has <see cref="PanelStatus.Open"/> set.
        /// </summary>
        /// <param name="viewModel">
        /// The <see cref="IPanelViewModel"/> for which the associated view needs to be opened, or closed
        /// </param>
        void OpenExistingOrOpenInAddIn(IPanelViewModel viewModel);

        /// <summary>
        /// Closes the <see cref="IPanelView"/> associated to the <see cref="IPanelViewModel"/>
        /// </summary>
        /// <param name="viewModel">
        /// The view-model that is to be closed.
        /// </param>
        void CloseInDock(IPanelViewModel viewModel);

        /// <summary>
        /// Closes all the <see cref="IPanelView"/> which associated <see cref="IPanelViewModel"/> is of a certain Type
        /// </summary>
        /// <param name="viewModelType">The <see cref="Type"/> of the <see cref="IPanelViewModel"/> to close</param>
        void CloseInDock(Type viewModelType);

        /// <summary>
        /// Closes all the <see cref="IPanelView"/> associated to a data-source
        /// </summary>
        /// <param name="datasourceUri">The string representation of the data-source's uri</param>
        void CloseInDock(string datasourceUri);

        /// <summary>
        /// Closes the view associated with the <see cref="IPanelViewModel"/> in the AddIn
        /// </summary>
        /// <param name="viewModel">The <see cref="IPanelViewModel"/> to close</param>
        void CloseInAddIn(IPanelViewModel viewModel);
    }
}
