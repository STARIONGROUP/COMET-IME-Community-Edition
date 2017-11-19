// -------------------------------------------------------------------------------------------------
// <copyright file="FileRevisionRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2017 RHEA System S.A.
// </copyright>
// <summary>
//   This is an auto-generated class. Any manual changes on this file will be overwritten!
// </summary>
// -------------------------------------------------------------------------------------------------

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
        /// Backing field for <see cref="ContentHash"/>
        /// </summary>
        private string contentHash;

        /// <summary>
        /// Backing field for <see cref="Path"/>
        /// </summary>
        private string path;

        /// <summary>
        /// Backing field for <see cref="CreatedOn"/>
        /// </summary>
        private DateTime createdOn;

        /// <summary>
        /// Backing field for <see cref="Name"/>
        /// </summary>
        private string name;

        /// <summary>
        /// Backing field for <see cref="Creator"/>
        /// </summary>
        private Participant creator;

        /// <summary>
        /// Backing field for <see cref="ContainingFolder"/>
        /// </summary>
        private Folder containingFolder;

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
        /// Gets or sets the ContentHash
        /// </summary>
        public string ContentHash
        {
            get { return this.contentHash; }
            set { this.RaiseAndSetIfChanged(ref this.contentHash, value); }
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
        /// Gets or sets the CreatedOn
        /// </summary>
        public DateTime CreatedOn
        {
            get { return this.createdOn; }
            set { this.RaiseAndSetIfChanged(ref this.createdOn, value); }
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
        /// Gets or sets the Creator
        /// </summary>
        public Participant Creator
        {
            get { return this.creator; }
            set { this.RaiseAndSetIfChanged(ref this.creator, value); }
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
            this.ModifiedOn = this.Thing.ModifiedOn;
            this.ContentHash = this.Thing.ContentHash;
            this.Path = this.Thing.Path;
            this.CreatedOn = this.Thing.CreatedOn;
            this.Name = this.Thing.Name;
            this.Creator = this.Thing.Creator;
            this.ContainingFolder = this.Thing.ContainingFolder;
        }
    }
}
