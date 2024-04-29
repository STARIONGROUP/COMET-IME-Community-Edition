// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDownloadFileService.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2020 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru
//            Nathanael Smiechowski, Kamil Wojnowski
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
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Services
{
    using System.Threading.Tasks;

    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// The purpose of the <see cref="IDownloadFileService"/> is to download a file, for example from a <see cref="FileStore"/>
    /// </summary>
    public interface IDownloadFileService
    {
        /// <summary>
        /// Executes a file download for a <see cref="File"/>
        /// </summary>
        /// <param name="downloadFileViewModel">The <see cref="IDownloadFileViewModel"/></param>
        /// <param name="file">The <see cref="File"/></param>
        /// <returns>An awaitable <see cref="Task"/></returns>
        Task ExecuteDownloadFile(IDownloadFileViewModel downloadFileViewModel, File file);

        /// <summary>
        /// Executes a file download for a <see cref="FileRevision"/>
        /// </summary>
        /// <param name="downloadFileViewModel">The <see cref="IDownloadFileViewModel"/></param>
        /// <param name="fileRevision">The <see cref="FileRevision"/></param>
        /// <returns>An awaitable <see cref="Task"/></returns>
        Task ExecuteDownloadFile(IDownloadFileViewModel downloadFileViewModel, FileRevision fileRevision);

        /// <summary>
        /// Cancels a file download
        /// </summary>
        /// <param name="downloadFileViewModel">The <see cref="IDownloadFileViewModel"/></param>
        void CancelDownloadFile(IDownloadFileViewModel downloadFileViewModel);
    }
}
