// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDownloadFileViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru
//            Nathanael Smiechowski, Kamil Wojnowski
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
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Services
{
    using CDP4Composition.Views;

    using CDP4Dal;

    /// <summary>
    /// A <see cref="IDownloadFileViewModel"/> defines properties needed by the <see cref="DownloadFileService"/> for its download and upload functionality
    /// </summary>
    public interface IDownloadFileViewModel
    {
        /// <summary>
        /// Gets a value the message text on the <see cref="LoadingControl"/>
        /// </summary>
        string LoadingMessage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the browser is busy
        /// </summary>
        bool IsBusy { get; set; }

        /// <summary>
        /// Gets a value indicating whether the Cancel button is visible on the <see cref="LoadingControl"/>
        /// </summary>
        bool IsCancelButtonVisible { get; set; }

        /// <summary>
        /// Gets the <see cref="ISession"/>
        /// </summary>
        ISession Session { get; }
    }
}
