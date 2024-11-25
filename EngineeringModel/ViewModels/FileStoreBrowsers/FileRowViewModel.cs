﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileRowViewModel.cs" company="Starion Group S.A.">
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
    using System;
    using System.Globalization;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Mvvm;

    using CDP4Dal;
    using CDP4Dal.Events;

    using ReactiveUI;

    /// <summary>
    /// The folder row view model.
    /// </summary>
    public class FileRowViewModel : CDP4CommonView.FileRowViewModel, IOwnedThingViewModel
    {
        /// <summary>
        /// The <see cref="IFileStoreFileAndFolderHandler"/>
        /// </summary>
        private readonly IFileStoreFileAndFolderHandler parentFileStoreFileAndFolderHandler;

        /// <summary>
        /// Backing field for <see cref="Name"/>
        /// </summary>
        private string name;

        /// <summary>
        /// Backing field for the <see cref="CreationDate"/> property
        /// </summary>
        private string creationDate;

        /// <summary>
        /// Backing field for <see cref="CreatorValue"/>
        /// </summary>
        private string creatorValue;

        /// <summary>
        /// Backing field for <see cref="IsLocked"/>
        /// </summary>
        private bool isLocked;

        /// <summary>
        /// Backing field for <see cref="Locker"/>
        /// </summary>
        private string locker;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileRowViewModel"/> class. 
        /// </summary>
        /// <param name="file">
        /// The <see cref="File"/> associated with this row
        /// </param>
        /// <param name="session">
        /// The session
        /// </param>
        /// <param name="containerViewModel">
        /// The <see cref="IViewModelBase<Thing>"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/>
        /// </param>
        public FileRowViewModel(File file, ISession session, IViewModelBase<Thing> containerViewModel, IFileStoreFileAndFolderHandler fileStoreFileAndFolderHandler)
            : base(file, session, containerViewModel)
        {
            if (containerViewModel == null)
            {
                throw new ArgumentNullException(nameof(containerViewModel), $"The {nameof(containerViewModel)} may not be null");
            }

            this.parentFileStoreFileAndFolderHandler = fileStoreFileAndFolderHandler;
            this.UpdateProperties();
        }

        /// <summary>
        /// Gets the name
        /// </summary>
        public string Name
        {
            get => this.name;
            private set => this.RaiseAndSetIfChanged(ref this.name, value);
        }

        /// <summary>
        /// Gets or sets the date of creation of the <see cref="Folder"/>
        /// </summary>
        public string CreationDate
        {
            get => this.creationDate;
            private set => this.RaiseAndSetIfChanged(ref this.creationDate, value);
        }

        /// <summary>
        /// Gets the creator of the <see cref="CDP4Common.EngineeringModelData.FileRevision"/>
        /// </summary>
        public string CreatorValue
        {
            get => this.creatorValue;
            private set => this.RaiseAndSetIfChanged(ref this.creatorValue, value);
        }

        /// <summary>
        /// Gets a value indicating whether the current <see cref="File"/> is locked
        /// </summary>
        public bool IsLocked
        {
            get => this.isLocked;
            private set => this.RaiseAndSetIfChanged(ref this.isLocked, value);
        }

        /// <summary>
        /// Gets the name of the person that locked the current <see cref="File"/>
        /// </summary>
        public string Locker
        {
            get => this.locker;
            private set => this.RaiseAndSetIfChanged(ref this.locker, value);
        }

        /// <summary>
        /// The current <see cref="CDP4Common.EngineeringModelData.FileRevision"/>
        /// </summary>
        public FileRevision FileRevision { get; set; }
        
        /// <summary>
        /// Update the <see cref="ContainerViewModel"/>
        /// </summary>
        /// <param name="containerViewModel">The new <see cref="ContainerViewModel"/></param>
        public void UpdateContainerViewModel(IViewModelBase<Thing> containerViewModel)
        {
            this.ContainerViewModel = containerViewModel;
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
            if (!this.Thing.FileRevision.Any())
            {
                return;
            }

            // check if there is a new file revision
            var lastCreatedDate = this.Thing.FileRevision.Select(x => x.CreatedOn).Max();

            if (this.FileRevision == null)
            {
                this.FileRevision = this.Thing.FileRevision.First(x => x.CreatedOn == lastCreatedDate);
                this.UpdateFileRevisionProperties();
            }

            if (this.FileRevision.CreatedOn != lastCreatedDate)
            {
                this.FileRevision = this.Thing.FileRevision.First(x => x.CreatedOn == lastCreatedDate);
                this.parentFileStoreFileAndFolderHandler?.UpdateFileRowPosition(this.Thing, this.FileRevision);
                this.UpdateFileRevisionProperties();
            }

            this.IsLocked = this.Thing.LockedBy != null;

            if (this.IsLocked)
            {
                this.Locker = this.Thing.LockedBy?.Name;
            }
            else
            {
                this.locker = string.Empty;
            }

            this.UpdateThingStatus();
        }

        /// <summary>
        /// Update the <see cref="ThingStatus"/> property
        /// </summary>
        protected override void UpdateThingStatus()
        {
            base.UpdateThingStatus();
            this.ThingStatus = new ThingStatus(this.Thing) { IsLocked = this.Thing.LockedBy != null };
        }

        /// <summary>
        /// Update the properties related to the <see cref="CDP4Common.EngineeringModelData.FileRevision"/> information
        /// </summary>
        private void UpdateFileRevisionProperties()
        {
            this.Name = this.FileRevision.Name;
            this.CreationDate = this.FileRevision.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss", CultureInfo.InvariantCulture);
            this.CreatorValue = this.FileRevision.Creator.Person.Name;
        }
    }
}
