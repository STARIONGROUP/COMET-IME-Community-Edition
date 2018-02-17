// -------------------------------------------------------------------------------------------------
// <copyright file="ExternalIdentifierMapRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
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
    /// Row class representing a <see cref="ExternalIdentifierMap"/>
    /// </summary>
    public partial class ExternalIdentifierMapRowViewModel : RowViewModelBase<ExternalIdentifierMap>
    {

        /// <summary>
        /// Backing field for <see cref="ExternalModelName"/>
        /// </summary>
        private string externalModelName;

        /// <summary>
        /// Backing field for <see cref="ExternalToolName"/>
        /// </summary>
        private string externalToolName;

        /// <summary>
        /// Backing field for <see cref="ExternalToolVersion"/>
        /// </summary>
        private string externalToolVersion;

        /// <summary>
        /// Backing field for <see cref="Name"/>
        /// </summary>
        private string name;

        /// <summary>
        /// Backing field for <see cref="ExternalFormat"/>
        /// </summary>
        private ReferenceSource externalFormat;

        /// <summary>
        /// Backing field for <see cref="ExternalFormatShortName"/>
        /// </summary>
        private string externalFormatShortName;

        /// <summary>
        /// Backing field for <see cref="ExternalFormatName"/>
        /// </summary>
        private string externalFormatName;

        /// <summary>
        /// Backing field for <see cref="Owner"/>
        /// </summary>
        private DomainOfExpertise owner;

        /// <summary>
        /// Backing field for <see cref="OwnerShortName"/>
        /// </summary>
        private string ownerShortName;

        /// <summary>
        /// Backing field for <see cref="OwnerName"/>
        /// </summary>
        private string ownerName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalIdentifierMapRowViewModel"/> class
        /// </summary>
        /// <param name="externalIdentifierMap">The <see cref="ExternalIdentifierMap"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public ExternalIdentifierMapRowViewModel(ExternalIdentifierMap externalIdentifierMap, ISession session, IViewModelBase<Thing> containerViewModel) : base(externalIdentifierMap, session, containerViewModel)
        {
            this.UpdateProperties();
        }


        /// <summary>
        /// Gets or sets the ExternalModelName
        /// </summary>
        public string ExternalModelName
        {
            get { return this.externalModelName; }
            set { this.RaiseAndSetIfChanged(ref this.externalModelName, value); }
        }

        /// <summary>
        /// Gets or sets the ExternalToolName
        /// </summary>
        public string ExternalToolName
        {
            get { return this.externalToolName; }
            set { this.RaiseAndSetIfChanged(ref this.externalToolName, value); }
        }

        /// <summary>
        /// Gets or sets the ExternalToolVersion
        /// </summary>
        public string ExternalToolVersion
        {
            get { return this.externalToolVersion; }
            set { this.RaiseAndSetIfChanged(ref this.externalToolVersion, value); }
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
        /// Gets or sets the ExternalFormat
        /// </summary>
        public ReferenceSource ExternalFormat
        {
            get { return this.externalFormat; }
            set { this.RaiseAndSetIfChanged(ref this.externalFormat, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="ExternalFormat"/>
        /// </summary>
        public string ExternalFormatShortName
        {
            get { return this.externalFormatShortName; }
            set { this.RaiseAndSetIfChanged(ref this.externalFormatShortName, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="ExternalFormat"/>
        /// </summary>
        public string ExternalFormatName
        {
            get { return this.externalFormatName; }
            set { this.RaiseAndSetIfChanged(ref this.externalFormatName, value); }
        }

        /// <summary>
        /// Gets or sets the Owner
        /// </summary>
        public DomainOfExpertise Owner
        {
            get { return this.owner; }
            set { this.RaiseAndSetIfChanged(ref this.owner, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="Owner"/>
        /// </summary>
        public string OwnerShortName
        {
            get { return this.ownerShortName; }
            set { this.RaiseAndSetIfChanged(ref this.ownerShortName, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="Owner"/>
        /// </summary>
        public string OwnerName
        {
            get { return this.ownerName; }
            set { this.RaiseAndSetIfChanged(ref this.ownerName, value); }
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
            this.ExternalModelName = this.Thing.ExternalModelName;
            this.ExternalToolName = this.Thing.ExternalToolName;
            this.ExternalToolVersion = this.Thing.ExternalToolVersion;
            this.Name = this.Thing.Name;
			if (this.Thing.ExternalFormat != null)
			{
				this.ExternalFormatShortName = this.Thing.ExternalFormat.ShortName;
				this.ExternalFormatName = this.Thing.ExternalFormat.Name;
			}			
            this.ExternalFormat = this.Thing.ExternalFormat;
			if (this.Thing.Owner != null)
			{
				this.OwnerShortName = this.Thing.Owner.ShortName;
				this.OwnerName = this.Thing.Owner.Name;
			}			
            this.Owner = this.Thing.Owner;
        }
    }
}
