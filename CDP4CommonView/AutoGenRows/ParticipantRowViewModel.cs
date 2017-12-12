// -------------------------------------------------------------------------------------------------
// <copyright file="ParticipantRowViewModel.cs" company="RHEA S.A.">
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
    /// Row class representing a <see cref="Participant"/>
    /// </summary>
    public partial class ParticipantRowViewModel : RowViewModelBase<Participant>
    {

        /// <summary>
        /// Backing field for <see cref="IsActive"/>
        /// </summary>
        private bool isActive;

        /// <summary>
        /// Backing field for <see cref="Person"/>
        /// </summary>
        private Person person;

        /// <summary>
        /// Backing field for <see cref="Role"/>
        /// </summary>
        private ParticipantRole role;

        /// <summary>
        /// Backing field for <see cref="RoleShortName"/>
        /// </summary>
        private string roleShortName;

        /// <summary>
        /// Backing field for <see cref="RoleName"/>
        /// </summary>
        private string roleName;

        /// <summary>
        /// Backing field for <see cref="SelectedDomain"/>
        /// </summary>
        private DomainOfExpertise selectedDomain;

        /// <summary>
        /// Backing field for <see cref="SelectedDomainShortName"/>
        /// </summary>
        private string selectedDomainShortName;

        /// <summary>
        /// Backing field for <see cref="SelectedDomainName"/>
        /// </summary>
        private string selectedDomainName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParticipantRowViewModel"/> class
        /// </summary>
        /// <param name="participant">The <see cref="Participant"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public ParticipantRowViewModel(Participant participant, ISession session, IViewModelBase<Thing> containerViewModel) : base(participant, session, containerViewModel)
        {
            this.UpdateProperties();
        }


        /// <summary>
        /// Gets or sets the IsActive
        /// </summary>
        public bool IsActive
        {
            get { return this.isActive; }
            set { this.RaiseAndSetIfChanged(ref this.isActive, value); }
        }

        /// <summary>
        /// Gets or sets the Person
        /// </summary>
        public Person Person
        {
            get { return this.person; }
            set { this.RaiseAndSetIfChanged(ref this.person, value); }
        }

        /// <summary>
        /// Gets or sets the Role
        /// </summary>
        public ParticipantRole Role
        {
            get { return this.role; }
            set { this.RaiseAndSetIfChanged(ref this.role, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="Role"/>
        /// </summary>
        public string RoleShortName
        {
            get { return this.roleShortName; }
            set { this.RaiseAndSetIfChanged(ref this.roleShortName, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="Role"/>
        /// </summary>
        public string RoleName
        {
            get { return this.roleName; }
            set { this.RaiseAndSetIfChanged(ref this.roleName, value); }
        }

        /// <summary>
        /// Gets or sets the SelectedDomain
        /// </summary>
        public DomainOfExpertise SelectedDomain
        {
            get { return this.selectedDomain; }
            set { this.RaiseAndSetIfChanged(ref this.selectedDomain, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="SelectedDomain"/>
        /// </summary>
        public string SelectedDomainShortName
        {
            get { return this.selectedDomainShortName; }
            set { this.RaiseAndSetIfChanged(ref this.selectedDomainShortName, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="SelectedDomain"/>
        /// </summary>
        public string SelectedDomainName
        {
            get { return this.selectedDomainName; }
            set { this.RaiseAndSetIfChanged(ref this.selectedDomainName, value); }
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
            this.IsActive = this.Thing.IsActive;
            this.Person = this.Thing.Person;
            this.Role = this.Thing.Role;
            this.SelectedDomain = this.Thing.SelectedDomain;
        }
    }
}
