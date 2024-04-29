// -------------------------------------------------------------------------------------------------
// <copyright file="SiteDirectoryRowViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2018 Starion Group S.A.
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
    /// Row class representing a <see cref="SiteDirectory"/>
    /// </summary>
    public partial class SiteDirectoryRowViewModel : TopContainerRowViewModel<SiteDirectory>
    {

        /// <summary>
        /// Backing field for <see cref="CreatedOn"/>
        /// </summary>
        private DateTime createdOn;

        /// <summary>
        /// Backing field for <see cref="Name"/>
        /// </summary>
        private string name;

        /// <summary>
        /// Backing field for <see cref="ShortName"/>
        /// </summary>
        private string shortName;

        /// <summary>
        /// Backing field for <see cref="DefaultParticipantRole"/>
        /// </summary>
        private ParticipantRole defaultParticipantRole;

        /// <summary>
        /// Backing field for <see cref="DefaultParticipantRoleShortName"/>
        /// </summary>
        private string defaultParticipantRoleShortName;

        /// <summary>
        /// Backing field for <see cref="DefaultParticipantRoleName"/>
        /// </summary>
        private string defaultParticipantRoleName;

        /// <summary>
        /// Backing field for <see cref="DefaultPersonRole"/>
        /// </summary>
        private PersonRole defaultPersonRole;

        /// <summary>
        /// Backing field for <see cref="DefaultPersonRoleShortName"/>
        /// </summary>
        private string defaultPersonRoleShortName;

        /// <summary>
        /// Backing field for <see cref="DefaultPersonRoleName"/>
        /// </summary>
        private string defaultPersonRoleName;

        /// <summary>
        /// Initializes a new instance of the <see cref="SiteDirectoryRowViewModel"/> class
        /// </summary>
        /// <param name="siteDirectory">The <see cref="SiteDirectory"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public SiteDirectoryRowViewModel(SiteDirectory siteDirectory, ISession session, IViewModelBase<Thing> containerViewModel) : base(siteDirectory, session, containerViewModel)
        {
            this.UpdateProperties();
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
        /// Gets or sets the ShortName
        /// </summary>
        public string ShortName
        {
            get { return this.shortName; }
            set { this.RaiseAndSetIfChanged(ref this.shortName, value); }
        }

        /// <summary>
        /// Gets or sets the DefaultParticipantRole
        /// </summary>
        public ParticipantRole DefaultParticipantRole
        {
            get { return this.defaultParticipantRole; }
            set { this.RaiseAndSetIfChanged(ref this.defaultParticipantRole, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="DefaultParticipantRole"/>
        /// </summary>
        public string DefaultParticipantRoleShortName
        {
            get { return this.defaultParticipantRoleShortName; }
            set { this.RaiseAndSetIfChanged(ref this.defaultParticipantRoleShortName, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="DefaultParticipantRole"/>
        /// </summary>
        public string DefaultParticipantRoleName
        {
            get { return this.defaultParticipantRoleName; }
            set { this.RaiseAndSetIfChanged(ref this.defaultParticipantRoleName, value); }
        }

        /// <summary>
        /// Gets or sets the DefaultPersonRole
        /// </summary>
        public PersonRole DefaultPersonRole
        {
            get { return this.defaultPersonRole; }
            set { this.RaiseAndSetIfChanged(ref this.defaultPersonRole, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="DefaultPersonRole"/>
        /// </summary>
        public string DefaultPersonRoleShortName
        {
            get { return this.defaultPersonRoleShortName; }
            set { this.RaiseAndSetIfChanged(ref this.defaultPersonRoleShortName, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="DefaultPersonRole"/>
        /// </summary>
        public string DefaultPersonRoleName
        {
            get { return this.defaultPersonRoleName; }
            set { this.RaiseAndSetIfChanged(ref this.defaultPersonRoleName, value); }
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
            this.CreatedOn = this.Thing.CreatedOn;
            this.Name = this.Thing.Name;
            this.ShortName = this.Thing.ShortName;
			if (this.Thing.DefaultParticipantRole != null)
			{
				this.DefaultParticipantRoleShortName = this.Thing.DefaultParticipantRole.ShortName;
				this.DefaultParticipantRoleName = this.Thing.DefaultParticipantRole.Name;
			}			
            this.DefaultParticipantRole = this.Thing.DefaultParticipantRole;
			if (this.Thing.DefaultPersonRole != null)
			{
				this.DefaultPersonRoleShortName = this.Thing.DefaultPersonRole.ShortName;
				this.DefaultPersonRoleName = this.Thing.DefaultPersonRole.Name;
			}			
            this.DefaultPersonRole = this.Thing.DefaultPersonRole;
        }
    }
}
