//  --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IPanelNavigationService.cs" company="RHEA System S.A.">
//     Copyright (c) 2015-2020 RHEA System S.A.
// 
//     Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft,
//             Nathanael Smiechowski, Kamil Wojnowski
// 
//     This file is part of CDP4-IME Community Edition.
//     The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//     compliant with ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The CDP4-IME Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or any later version.
// 
//     The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//     GNU Affero General Public License for more details.
// 
//     You should have received a copy of the GNU Affero General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Navigation
{
    using System;

    using CDP4Common.CommonData;

    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.Composition;

    /// <summary>
    /// The Interface for Panel Navigation Service
    /// </summary>
    public interface IPanelNavigationService
    {
        /// <summary>
        /// Opens the view associated to the provided view-model
        /// </summary>
        /// <param name="viewModel">
        /// The <see cref="IPanelViewModel"/> for which the associated view needs to be opened
        /// </param>
        /// <remarks>
        /// The data context of the view is the <see cref="IPanelViewModel"/>
        /// </remarks>
        void Open(IPanelViewModel viewModel);

        /// <summary>
        /// Opens the properties of a <see cref="Thing"/> in a docking panel
        /// </summary>
        /// <param name="thing">The <see cref="Thing"/> which properties are displayed</param>
        /// <param name="session">The <see cref="ISession"/> associated</param>
        void Open(Thing thing, ISession session);

        /// <summary>
        /// Opens the view associated to a view-model. The view-model is identified by its <see cref="INameMetaData.Name"/>.
        /// </summary>
        /// <param name="viewModelName">The name we want to compare to the <see cref="INameMetaData.Name"/> of the view-models.</param>
        /// <param name="session">The <see cref="ISession"/> associated.</param>
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService"/>.</param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/>.</param>
        void Open(string viewModelName, ISession session, IThingDialogNavigationService thingDialogNavigationService, IDialogNavigationService dialogNavigationService);

        /// <summary>
        /// Closes the <see cref="IPanelView"/> associated to the <see cref="IPanelViewModel"/>
        /// </summary>
        /// <param name="viewModel">
        /// The view-model that is to be closed.
        /// </param>
        void Close(IPanelViewModel viewModel);

        /// <summary>
        /// Closes all the <see cref="IPanelView"/> which associated <see cref="IPanelViewModel"/> is of a certain Type
        /// </summary>
        /// <param name="viewModelType">The <see cref="Type"/> of the <see cref="IPanelViewModel"/> to close</param>
        void Close(Type viewModelType);

        /// <summary>
        /// Closes all the <see cref="IPanelView"/> associated to a data-source
        /// </summary>
        /// <param name="datasourceUri">The string representation of the data-source's uri</param>
        void Close(string datasourceUri);
    }
}
