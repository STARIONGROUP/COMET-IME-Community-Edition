// -------------------------------------------------------------------------------------------------
// <copyright file="FileStoreRowViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2017 Starion Group S.A.
// </copyright>
// <summary>
//   This is an auto-generated class. Any manual changes on this file will be overwritten!
// </summary>
// -------------------------------------------------------------------------------------------------

namespace CDP4ObjectBrowser
{
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.DiagramData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Common.ReportingData;
    using System;
    using System.Reactive.Linq;

    /// <summary>
    /// Row class representing a <see cref="FileStore"/>
    /// </summary>
    public abstract partial class FileStoreRowViewModel<T> : ObjectBrowserRowViewModel<T>, IFileStoreRowViewModel<T> where T :FileStore
    {
        /// <summary>
        /// Intermediate folder containing <see cref="FolderRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel folderFolder;

        /// <summary>
        /// Intermediate folder containing <see cref="FileRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel fileFolder;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileStoreRowViewModel{T}"/> class
        /// </summary>
        /// <param name="fileStore">The <see cref="FileStore"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase"/> that is the container of this <see cref="IRowViewModelBase"/></param>
        protected FileStoreRowViewModel(T fileStore, ISession session, IViewModelBase<Thing> containerViewModel) : base(fileStore, session, containerViewModel)
        {
            this.folderFolder = new CDP4Composition.FolderRowViewModel("Folder", "Folder", this.Session, this);
            this.ContainedRows.Add(this.folderFolder);
            this.fileFolder = new CDP4Composition.FolderRowViewModel("File", "File", this.Session, this);
            this.ContainedRows.Add(this.fileFolder);
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
        /// Updates all the properties rows
        /// /// </summary>
        private void UpdateProperties()
        {
            this.ComputeRows(this.Thing.Folder, this.folderFolder, this.AddFolderRowViewModel);
            this.ComputeRows(this.Thing.File, this.fileFolder, this.AddFileRowViewModel);
        }
        /// <summary>
        /// Add an Folder row view model to the list of <see cref="Folder"/>
        /// </summary>
        /// <param name="folder">
        /// The <see cref="Folder"/> that is to be added
        /// </param>
        private FolderRowViewModel AddFolderRowViewModel(Folder folder)
        {
            return new FolderRowViewModel(folder, this.Session, this);
        }
        /// <summary>
        /// Add an File row view model to the list of <see cref="File"/>
        /// </summary>
        /// <param name="file">
        /// The <see cref="File"/> that is to be added
        /// </param>
        private FileRowViewModel AddFileRowViewModel(File file)
        {
            return new FileRowViewModel(file, this.Session, this);
        }
    }
}
