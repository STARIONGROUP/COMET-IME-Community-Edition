// -------------------------------------------------------------------------------------------------
// <copyright file="FileRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using CDP4EngineeringModel.ViewModels.FileStoreBrowsers;
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
    public class FileRowViewModel : CDP4CommonView.FileRowViewModel
    {
        /// <summary>
        /// The <see cref="IFileStoreRow"/>
        /// </summary>
        private readonly IFileStoreRow fileStoreRow;

        /// <summary>
        /// The current <see cref="FileRevision"/>
        /// </summary>
        private FileRevision fileRevision;

        /// <summary>
        /// Backing field for <see cref="Name"/>
        /// </summary>
        private string name;

        /// <summary>
        /// Backing field for <see cref="CreatedOn"/>
        /// </summary>
        private string createdOn;

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
        public FileRowViewModel(File file, ISession session, IFileStoreRow containerViewModel)
            : base(file, session, (IViewModelBase<Thing>)containerViewModel)
        {
            if (containerViewModel == null)
            {
                throw new ArgumentNullException("containerViewModel", "The containerViewModel may not be null");
            }

            this.fileStoreRow = (IFileStoreRow)containerViewModel;
            this.UpdateProperties();
        }

        /// <summary>
        /// Gets the name
        /// </summary>
        public string Name
        {
            get { return this.name; }
            private set { this.RaiseAndSetIfChanged(ref this.name, value); }
        }

        /// <summary>
        /// Gets the date the current <see cref="FileRevision"/> was created
        /// </summary>
        public string CreatedOn
        {
            get { return this.createdOn; }
            private set { this.RaiseAndSetIfChanged(ref this.createdOn, value); }
        }

        /// <summary>
        /// Gets the creator of the <see cref="FileRevision"/>
        /// </summary>
        public string CreatorValue
        {
            get { return this.creatorValue; }
            private set { this.RaiseAndSetIfChanged(ref this.creatorValue, value); }
        }

        /// <summary>
        /// Gets a value indicating whether the current <see cref="File"/> is locked
        /// </summary>
        public bool IsLocked
        {
            get { return this.isLocked; }
            private set { this.RaiseAndSetIfChanged(ref this.isLocked, value); }
        }

        /// <summary>
        /// Gets the name of the person that locked the current <see cref="File"/>
        /// </summary>
        public string Locker
        {
            get { return this.locker; }
            private set { this.RaiseAndSetIfChanged(ref this.locker, value); }
        }

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
            // check if there is a new file revision
            var lastCreatedDate = this.Thing.FileRevision.Select(x => x.CreatedOn).Max();
            if (this.fileRevision == null)
            {
                this.fileRevision = this.Thing.FileRevision.First(x => x.CreatedOn == lastCreatedDate);
                this.UpdateFileRevisionProperties();
            }

            if (this.fileRevision.CreatedOn != lastCreatedDate)
            {
                this.fileRevision = this.Thing.FileRevision.First(x => x.CreatedOn == lastCreatedDate);
                ((IFileStoreRow)this.fileStoreRow).UpdateFileRowPosition(this.Thing, this.fileRevision);
                this.UpdateFileRevisionProperties();
            }

            this.IsLocked = this.Thing.LockedBy != null;
            if (this.IsLocked)
            {
                this.Locker = this.Thing.LockedBy.Name;
            }
            else
            {
                this.locker = string.Empty;
            }
        }

        /// <summary>
        /// Update the properties related to the <see cref="FileRevision"/> information
        /// </summary>
        private void UpdateFileRevisionProperties()
        {
            this.Name = this.fileRevision.Name;
            this.CreatedOn = this.fileRevision.CreatedOn.ToString(CultureInfo.InvariantCulture);
            this.CreatorValue = this.fileRevision.Creator.Person.Name;
        }
    }
}
