// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IUpdatableThingRowViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2022 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMET.ViewModels
{
    using System.Threading.Tasks;

    using CDP4UpdateServerDal;

    using COMET.Services;

    /// <summary>
    /// Definitions of methods <see cref="PluginRowViewModel"/> and <see cref="ImeRowViewModel"/> have to implement
    /// </summary>
    public interface IUpdatableThingRowViewModel
    {
        /// <summary>
        /// Gets or sets the assert whether the represented thing will be installed or downloaded
        /// </summary>
        /// <returns>An assert whether the representent updatatable thing is selected</returns>
        bool IsSelected { get; set; }

        /// <summary>
        /// Gets the <see cref="IUpdateFileSystemService"/> to operate on
        /// </summary>
        /// <returns>An <see cref="IUpdateFileSystemService"/></returns>
        IUpdateFileSystemService FileSystem { get; set; }

        /// <summary>
        /// Downloads this represented plugin
        /// </summary>
        /// <param name="client">the Update Server Client to perform request</param>
        /// <returns>A <see cref="Task"/></returns>
        Task Download(IUpdateServerClient client);

        /// <summary>
        /// Handles the cancelation of the download process
        /// </summary>
        /// <returns>A <see cref="Task"/></returns>
        Task HandlingCancelationOfDownload();
    }
}
