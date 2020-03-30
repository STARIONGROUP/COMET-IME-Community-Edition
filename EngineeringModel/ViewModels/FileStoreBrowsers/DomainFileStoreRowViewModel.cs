// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DomainFileStoreRowViewModel.cs" company="RHEA System S.A.">
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

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Mvvm;

    using CDP4Dal;
    using CDP4Dal.Events;

    using CDP4EngineeringModel.ViewModels.FileStoreBrowsers;

    using ReactiveUI;

    /// <summary>
    /// The <see cref="DomainFileStore"/> row-view-model
    /// </summary>
    public class DomainFileStoreRowViewModel : CDP4CommonView.DomainFileStoreRowViewModel, IFileStoreRow<DomainFileStore>
    {
        /// <summary>
        /// Backing field for <see cref="FolderCache"/>
        /// </summary>
        private Dictionary<Folder, FolderRowViewModel> folderCache;

        /// <summary>
        /// Backingfield for <see cref="FileCache"/>
        /// </summary>
        private Dictionary<File, FileRowViewModel> fileCache;

        /// <summary>
        /// The <see cref="IFileStoreFileAndFolderHandler"/>
        /// </summary>
        private readonly IFileStoreFileAndFolderHandler fileStoreFileAndFolderHandler;

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
            this.fileStoreFileAndFolderHandler = new FileStoreFileAndFolderHandler<DomainFileStore>(this);
            this.UpdateProperties();
        }

        /// <summary>
        /// Gets or sets the <see cref="Folder"/> cache
        /// </summary>
        public Dictionary<Folder, FolderRowViewModel> FolderCache
        {
            get => this.folderCache;
            private set => this.RaiseAndSetIfChanged(ref this.folderCache, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="File"/> cache
        /// </summary>
        public Dictionary<File, FileRowViewModel> FileCache
        {
            get => this.fileCache;
            private set => this.RaiseAndSetIfChanged(ref this.fileCache, value);
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
            this.fileStoreFileAndFolderHandler.UpdateFolderRows();
            this.fileStoreFileAndFolderHandler.UpdateFileRows();
        }

        public FileStore Thing { get; }
    }
}
