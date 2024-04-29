// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IFileStoreFileAndFolderHandler.cs" company="Starion Group S.A.">
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

namespace CDP4EngineeringModel.ViewModels
{
    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// Interface that a <see cref="FileStoreFileAndFolderHandler{T}"/> should implement.
    /// </summary>
    public interface IFileStoreFileAndFolderHandler
    {
        /// <summary>
        /// Update the <see cref="Folder"/> rows
        /// </summary>
        void UpdateFolderRows();

        /// <summary>
        /// Update the <see cref="File"/> rows
        /// </summary>
        void UpdateFileRows();

        /// <summary>
        /// Update the position of a <see cref="Folder"/>
        /// </summary>
        /// <param name="updatedFolder">The updated <see cref="Folder"/></param>
        void UpdateFolderRowPosition(Folder updatedFolder);

        /// <summary>
        /// Update the <see cref="File"/> row position
        /// </summary>
        /// <param name="file">The <see cref="File"/></param>
        /// <param name="fileRevision">The latest <see cref="FileRevision"/></param>
        void UpdateFileRowPosition(File file, FileRevision fileRevision);
    }
}
