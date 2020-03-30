// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileStoreFileAndFolderHandler.cs" company="RHEA System S.A.">
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

namespace CDP4EngineeringModel.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Mvvm;

    using CDP4EngineeringModel.ViewModels.FileStoreBrowsers;

    /// <summary>
    /// Generic handler for <see cref="Things"/> that contain <see cref="Folder"/> and <see cref="File"/> objects
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FileStoreFileAndFolderHandler<T> : IFileStoreFileAndFolderHandler where T : FileStore
    {
        /// <summary>
        /// The ViewModel that implements both <see cref="IViewModelBase<FileStore>"/> and <see cref="IFileStoreRow"/>
        /// </summary>
        private readonly IFileStoreRow<T> fileStoreRow;

        /// <summary>
        /// The <see cref="IEnumerable{Folder}"/>
        /// </summary>
        private IEnumerable<Folder> Folders => this.fileStoreRow.Thing.Folder;

        /// <summary>
        /// The <see cref="IEnumerable{File}"/>
        /// </summary>
        private IEnumerable<File> Files => this.fileStoreRow.Thing.File;

        /// <summary>
        /// Creates a new instance of <see cref="FileStoreFileAndFolderHandler"/>
        /// </summary>
        /// <param name="fileStoreRow">
        /// The ViewModel that implements both <see cref="IViewModelBase<FileStore>"/> and <see cref="IFileStoreRow"/>.
        /// Typically a <see cref="CommonFileStore"/> or <see cref="DomainFileStore"/>.
        /// </param>
        public FileStoreFileAndFolderHandler(IFileStoreRow<T> fileStoreRow) 
        {
            this.fileStoreRow = fileStoreRow;
        }

        /// <summary>
        /// Update the <see cref="Folder"/> rows
        /// </summary>
        public void UpdateFolderRows()
        {
            var currentFolders = this.fileStoreRow.FolderCache.Keys;

            var addedFolders = this.Folders.Except(currentFolders).ToList();
            var removedFolders = currentFolders.Except(this.Folders).ToList();

            foreach (var removedFolder in removedFolders)
            {
                if (this.fileStoreRow.FolderCache.TryGetValue(removedFolder, out var row))
                {
                    this.fileStoreRow.FolderCache.Remove(removedFolder);
                    ((IHaveContainedRows)row.ContainerViewModel).ContainedRows.RemoveAndDispose(row);
                }
            }

            foreach (var addedFolder in addedFolders)
            {
                var row = new FolderRowViewModel(addedFolder, this.fileStoreRow.Session, this.fileStoreRow, this);
                this.fileStoreRow.FolderCache.Add(addedFolder, row);
            }

            foreach (var addedFolder in addedFolders)
            {
                if (addedFolder.ContainingFolder == null)
                {
                    this.fileStoreRow.ContainedRows.Add(this.fileStoreRow.FolderCache[addedFolder]);
                }
                else
                {
                    var row = this.fileStoreRow.FolderCache[addedFolder];
                    var containerViewModel = this.fileStoreRow.FolderCache[addedFolder.ContainingFolder];
                    containerViewModel.ContainedRows.Add(row);
                    row.UpdateContainerViewModel(containerViewModel);
                }
            }
        }

        /// <summary>
        /// Update the <see cref="File"/> rows
        /// </summary>
        public void UpdateFileRows()
        {
            this.UpdateFolderRows();

            var currentFiles = this.fileStoreRow.FileCache.Keys;

            var addedFiles = this.Files.Except(currentFiles).ToList();
            var removedFiles = currentFiles.Except(this.Files).ToList();

            foreach (var removedFile in removedFiles)
            {
                if (this.fileStoreRow.FileCache.TryGetValue(removedFile, out var row))
                {
                    this.fileStoreRow.FileCache.Remove(removedFile);
                    ((IHaveContainedRows)row.ContainerViewModel).ContainedRows.RemoveAndDispose(row);
                }
            }

            foreach (var addedFile in addedFiles)
            {
                var row = new FileRowViewModel(addedFile, this.fileStoreRow.Session, this.fileStoreRow, this);
                this.fileStoreRow.FileCache.Add(addedFile, row);

                var lastRevision = addedFile.FileRevision.OrderByDescending(x => x.CreatedOn).First();

                if (lastRevision.ContainingFolder == null)
                {
                    this.fileStoreRow.ContainedRows.Add(row);
                }
                else
                {
                    var containerViewModel = this.fileStoreRow.FolderCache[lastRevision.ContainingFolder];
                    containerViewModel.ContainedRows.Add(row);
                    row.UpdateContainerViewModel(containerViewModel);
                }
            }
        }

        /// <summary>
        /// Update the position of a <see cref="Folder"/>
        /// </summary>
        /// <param name="updatedFolder">The updated <see cref="Folder"/></param>
        public void UpdateFolderRowPosition(Folder updatedFolder)
        {
            this.UpdateFolderRows();

            var row = this.fileStoreRow.FolderCache[updatedFolder];

            if (updatedFolder.ContainingFolder == null)
            {
                if (!row.ContainerViewModel.Equals(this.fileStoreRow))
                {
                    ((FolderRowViewModel)row.ContainerViewModel).ContainedRows.RemoveWithoutDispose(row);
                    this.fileStoreRow.ContainedRows.Add(row);

                    if (this.fileStoreRow is IViewModelBase<Thing> viewModelBase)
                    {
                        row.UpdateContainerViewModel(viewModelBase);
                    }
                }
            }
            else if (updatedFolder.ContainingFolder != row.ContainerViewModel.Thing)
            {
                ((IHaveContainedRows)row.ContainerViewModel).ContainedRows.RemoveWithoutDispose(row);
                var containerViewModel = this.fileStoreRow.FolderCache[updatedFolder.ContainingFolder];
                containerViewModel.ContainedRows.Add(row);
                row.UpdateContainerViewModel(containerViewModel);
            }
        }

        /// <summary>
        /// Update the <see cref="File"/> row position
        /// </summary>
        /// <param name="file">The <see cref="File"/></param>
        /// <param name="fileRevision">The latest <see cref="FileRevision"/></param>
        public void UpdateFileRowPosition(File file, FileRevision fileRevision)
        {
            this.UpdateFileRows();

            var row = this.fileStoreRow.FileCache[file];

            if (fileRevision.ContainingFolder == null)
            {
                if (!row.ContainerViewModel.Equals(this.fileStoreRow))
                {
                    ((FolderRowViewModel)row.ContainerViewModel).ContainedRows.RemoveWithoutDispose(row);
                    this.fileStoreRow.ContainedRows.Add(row);
                    row.UpdateContainerViewModel(this.fileStoreRow);
                }
            }
            else if (fileRevision.ContainingFolder != row.ContainerViewModel.Thing)
            {
                ((IHaveContainedRows)row.ContainerViewModel).ContainedRows.RemoveWithoutDispose(row);
                var containerViewModel = this.fileStoreRow.FolderCache[fileRevision.ContainingFolder];
                containerViewModel.ContainedRows.Add(row);
                row.UpdateContainerViewModel(containerViewModel);
            }
        }
    }
}
