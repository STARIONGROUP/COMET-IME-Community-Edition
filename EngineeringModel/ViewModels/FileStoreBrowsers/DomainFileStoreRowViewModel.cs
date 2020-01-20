// -------------------------------------------------------------------------------------------------
// <copyright file="DomainFileStoreRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2020 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using CDP4EngineeringModel.ViewModels.FileStoreBrowsers;
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Composition.Mvvm;
    using CDP4Composition;
    using CDP4Dal;
    using CDP4Dal.Events;

    /// <summary>
    /// The <see cref="DomainFileStore"/> row-view-model
    /// </summary>
    public class DomainFileStoreRowViewModel : CDP4CommonView.DomainFileStoreRowViewModel, IFileStoreRow
    {
        /// <summary>
        /// The <see cref="Folder"/> cache
        /// </summary>
        private Dictionary<Folder, FolderRowViewModel> folderCache;

        /// <summary>
        /// The <see cref="File"/> cache
        /// </summary>
        private Dictionary<File, FileRowViewModel> fileCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainFileStoreRowViewModel"/> class
        /// </summary>
        /// <param name="store">The associated <see cref="DomainFileStore"/></param>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <param name="containerViewModel">The container view-model</param>
        public DomainFileStoreRowViewModel(DomainFileStore store, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(store, session, containerViewModel)
        {
            this.folderCache = new Dictionary<Folder, FolderRowViewModel>();
            this.fileCache = new Dictionary<File, FileRowViewModel>();
            this.UpdateProperties();
        }

        /// <summary>
        /// The event-handler that is invoked by the subscription that listens for updates
        /// on the <see cref="Thing"/> that is being represented by the view-model
        /// </summary>
        /// <param name="objectChange">
        /// The payload of the event that is being handled
        /// </param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateProperties();
        }

        /// <summary>
        /// Update the properties of this row
        /// </summary>
        private void UpdateProperties()
        {
            this.UpdateFolderRows();
            this.UpdateFileRows();
        }

        /// <summary>
        /// Update the <see cref="Folder"/> rows
        /// </summary>
        private void UpdateFolderRows()
        {
            var currentFolders = this.folderCache.Keys;

            var addedFolders = this.Thing.Folder.Except(currentFolders).ToList();
            var removedFolders = currentFolders.Except(this.Thing.Folder);

            foreach (var removedFolder in removedFolders)
            {
                if (this.folderCache.TryGetValue(removedFolder, out var row))
                {
                    this.folderCache.Remove(removedFolder);
                    ((IRowViewModelBase<Thing>)row.ContainerViewModel).ContainedRows.RemoveAndDispose(row);
                }
            }

            foreach (var addedFolder in addedFolders)
            {
                var row = new FolderRowViewModel(addedFolder, this.Session, this);
                this.folderCache.Add(addedFolder, row);
            }

            foreach (var addedFolder in addedFolders)
            {
                if (addedFolder.ContainingFolder == null)
                {
                    this.ContainedRows.Add(this.folderCache[addedFolder]);
                }
                else
                {
                    var row = this.folderCache[addedFolder];
                    var containerViewModel = this.folderCache[addedFolder.ContainingFolder];
                    containerViewModel.ContainedRows.Add(row);
                    row.UpdateContainerViewModel(containerViewModel);
                }
            }
        }

        /// <summary>
        /// Update the <see cref="File"/> rows
        /// </summary>
        private void UpdateFileRows()
        {
            var currentFiles = this.fileCache.Keys;

            var addedFiles = this.Thing.File.Except(currentFiles).ToList();
            var removedFiles = currentFiles.Except(this.Thing.File);

            foreach (var removedFile in removedFiles)
            {
                if (this.fileCache.TryGetValue(removedFile, out var row))
                {
                    this.fileCache.Remove(removedFile);
                    ((IRowViewModelBase<Thing>)row.ContainerViewModel).ContainedRows.RemoveAndDispose(row);
                }
            }

            foreach (var addedFile in addedFiles)
            {
                var row = new FileRowViewModel(addedFile, this.Session, this);
                this.fileCache.Add(addedFile, row);

                var lastCreatedDate = addedFile.FileRevision.Select(x => x.CreatedOn).Max();
                var lastRevision = addedFile.FileRevision.First(x => x.CreatedOn == lastCreatedDate);

                if (lastRevision.ContainingFolder == null)
                {
                    this.ContainedRows.Add(row);
                }
                else
                {
                    var containerViewModel = this.folderCache[lastRevision.ContainingFolder];
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
            var row = this.folderCache[updatedFolder];
            if (updatedFolder.ContainingFolder == null)
            {
                if (row.ContainerViewModel != this)
                {
                    ((FolderRowViewModel)row.ContainerViewModel).ContainedRows.RemoveWithoutDispose(row);
                    this.ContainedRows.Add(row);
                    row.UpdateContainerViewModel(this);
                }
            }
            else if (updatedFolder.ContainingFolder != row.ContainerViewModel.Thing)
            {
                ((IRowViewModelBase<Thing>)row.ContainerViewModel).ContainedRows.RemoveWithoutDispose(row);
                var containerViewModel = this.folderCache[updatedFolder.ContainingFolder];
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
            // make sure that the folders are all updated
            this.UpdateFolderRows();

            var row = this.fileCache[file];
            if (fileRevision.ContainingFolder == null)
            {
                if (row.ContainerViewModel != this)
                {
                    ((FolderRowViewModel)row.ContainerViewModel).ContainedRows.RemoveWithoutDispose(row);
                    this.ContainedRows.Add(row);
                    row.UpdateContainerViewModel(this);
                }
            }
            else if (fileRevision.ContainingFolder != row.ContainerViewModel.Thing)
            {
                ((IRowViewModelBase<Thing>)row.ContainerViewModel).ContainedRows.RemoveWithoutDispose(row);
                var containerViewModel = this.folderCache[fileRevision.ContainingFolder];
                containerViewModel.ContainedRows.Add(row);
                row.UpdateContainerViewModel(containerViewModel);
            }
        }
    }
}
