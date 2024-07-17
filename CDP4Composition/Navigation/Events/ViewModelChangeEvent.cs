﻿// ------------------------------------------------------------------------------------------------
// <copyright file="ViewModelChangeEvent.cs" company="Starion Group S.A.">
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

namespace CDP4Composition.Navigation.Events
{
    /// <summary>
    /// The event carrying information for a viewmodel change.
    /// </summary>
    public class ViewModelChangeEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelChangeEvent"/> class
        /// </summary>
        /// <param name="viewModel">
        /// The event's <see cref="IPanelViewModel"/>
        /// </param>
        public ViewModelChangeEvent(IPanelViewModel viewModel)
        {
            this.ViewModel = viewModel;
        }

        /// <summary>
        /// Gets the <see cref="IPanelViewModel"/>
        /// </summary>
        public IPanelViewModel ViewModel { get; private set; }
    }
}
