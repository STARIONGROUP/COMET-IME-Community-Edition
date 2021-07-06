// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileRevisionRowViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski, Ahmed Abulwafa Ahmed
//
//    This file is part of CDP4-IME Community Edition.
//    This is an auto-generated class. Any manual changes to this file will be overwritten!
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Lesser General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// <summary>
//   This is an auto-generated class. Any manual changes on this file will be overwritten!
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4CommonView
{
    using System;
    using System.Reactive.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.DiagramData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using ReactiveUI;

    /// <summary>
    /// Row class representing a <see cref="FileRevision"/>
    /// </summary>
    public partial class FileRevisionRowViewModel : RowViewModelBase<FileRevision>
    {
        /// <summary>
        /// Backing field for <see cref="ContainingFolder"/> property
        /// </summary>
        private Folder containingFolder;

        /// <summary>
        /// Backing field for <see cref="ContainingFolderName"/> property
        /// </summary>
        private string containingFolderName;

        /// <summary>
        /// Backing field for <see cref="ContentHash"/> property
        /// </summary>
        private string contentHash;

        /// <summary>
        /// Backing field for <see cref="CreatedOn"/> property
        /// </summary>
        private DateTime createdOn;

        /// <summary>
        /// Backing field for <see cref="Creator"/> property
        /// </summary>
        private Participant creator;

        /// <summary>
        /// Backing field for <see cref="Name"/> property
        /// </summary>
        private string name;

        /// <summary>
        /// Backing field for <see cref="Path"/> property
        /// </summary>
        private string path;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileRevisionRowViewModel"/> class
        /// </summary>
        /// <param name="fileRevision">The <see cref="FileRevision"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public FileRevisionRowViewModel(FileRevision fileRevision, ISession session, IViewModelBase<Thing> containerViewModel) : base(fileRevision, session, containerViewModel)
        {
            this.UpdateProperties();
        }

        /// <summary>
        /// Gets or sets the ContainingFolder
        /// </summary>
        public Folder ContainingFolder
        {
            get { return this.containingFolder; }
            set { this.RaiseAndSetIfChanged(ref this.containingFolder, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="ContainingFolder"/>
        /// </summary>
        public string ContainingFolderName
        {
            get { return this.containingFolderName; }
            set { this.RaiseAndSetIfChanged(ref this.containingFolderName, value); }
        }

        /// <summary>
        /// Gets or sets the ContentHash
        /// </summary>
        public string ContentHash
        {
            get { return this.contentHash; }
            set { this.RaiseAndSetIfChanged(ref this.contentHash, value); }
        }

        /// <summary>
        /// Gets or sets the CreatedOn
        /// </summary>
        public DateTime CreatedOn
        {
            get { return this.createdOn; }
            set { this.RaiseAndSetIfChanged(ref this.createdOn, value); }
        }

        /// <summary>
        /// Gets or sets the Creator
        /// </summary>
        public Participant Creator
        {
            get { return this.creator; }
            set { this.RaiseAndSetIfChanged(ref this.creator, value); }
        }

        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        public string Name
        {
            get { return this.name; }
            set { this.RaiseAndSetIfChanged(ref this.name, value); }
        }

        /// <summary>
        /// Gets or sets the Path
        /// </summary>
        public string Path
        {
            get { return this.path; }
            set { this.RaiseAndSetIfChanged(ref this.path, value); }
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
        /// Updates the properties of this row
        /// </summary>
        private void UpdateProperties()
        {
            this.ContainingFolder = this.Thing.ContainingFolder;
            if (this.Thing.ContainingFolder != null)
            {
                this.ContainingFolderName = this.Thing.ContainingFolder.Name;
            }
            else
            {
                this.ContainingFolderName = string.Empty;
            }
            this.ContentHash = this.Thing.ContentHash;
            this.CreatedOn = this.Thing.CreatedOn;
            this.Creator = this.Thing.Creator;
            this.Name = this.Thing.Name;
            this.Path = this.Thing.Path;
        }
    }
}
