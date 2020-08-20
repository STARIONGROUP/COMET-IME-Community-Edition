// -------------------------------------------------------------------------------------------------
// <copyright file="PersonRowViewModel.cs" company="RHEA System S.A.">
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
    /// Row class representing a <see cref="Person"/>
    /// </summary>
    public partial class PersonRowViewModel : RowViewModelBase<Person>
    {

        /// <summary>
        /// Backing field for <see cref="GivenName"/>
        /// </summary>
        private string givenName;

        /// <summary>
        /// Backing field for <see cref="Surname"/>
        /// </summary>
        private string surname;

        /// <summary>
        /// Backing field for <see cref="OrganizationalUnit"/>
        /// </summary>
        private string organizationalUnit;

        /// <summary>
        /// Backing field for <see cref="IsActive"/>
        /// </summary>
        private bool isActive;

        /// <summary>
        /// Backing field for <see cref="Password"/>
        /// </summary>
        private string password;

        /// <summary>
        /// Backing field for <see cref="ShortName"/>
        /// </summary>
        private string shortName;

        /// <summary>
        /// Backing field for <see cref="Name"/>
        /// </summary>
        private string name;

        /// <summary>
        /// Backing field for <see cref="IsDeprecated"/>
        /// </summary>
        private bool isDeprecated;

        /// <summary>
        /// Backing field for <see cref="Organization"/>
        /// </summary>
        private Organization organization;

        /// <summary>
        /// Backing field for <see cref="DefaultDomain"/>
        /// </summary>
        private DomainOfExpertise defaultDomain;

        /// <summary>
        /// Backing field for <see cref="DefaultDomainShortName"/>
        /// </summary>
        private string defaultDomainShortName;

        /// <summary>
        /// Backing field for <see cref="DefaultDomainName"/>
        /// </summary>
        private string defaultDomainName;

        /// <summary>
        /// Backing field for <see cref="Role"/>
        /// </summary>
        private PersonRole role;

        /// <summary>
        /// Backing field for <see cref="RoleShortName"/>
        /// </summary>
        private string roleShortName;

        /// <summary>
        /// Backing field for <see cref="RoleName"/>
        /// </summary>
        private string roleName;

        /// <summary>
        /// Backing field for <see cref="DefaultEmailAddress"/>
        /// </summary>
        private EmailAddress defaultEmailAddress;

        /// <summary>
        /// Backing field for <see cref="DefaultTelephoneNumber"/>
        /// </summary>
        private TelephoneNumber defaultTelephoneNumber;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonRowViewModel"/> class
        /// </summary>
        /// <param name="person">The <see cref="Person"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public PersonRowViewModel(Person person, ISession session, IViewModelBase<Thing> containerViewModel) : base(person, session, containerViewModel)
        {
            this.UpdateProperties();
        }


        /// <summary>
        /// Gets or sets the GivenName
        /// </summary>
        public string GivenName
        {
            get { return this.givenName; }
            set { this.RaiseAndSetIfChanged(ref this.givenName, value); }
        }

        /// <summary>
        /// Gets or sets the Surname
        /// </summary>
        public string Surname
        {
            get { return this.surname; }
            set { this.RaiseAndSetIfChanged(ref this.surname, value); }
        }

        /// <summary>
        /// Gets or sets the OrganizationalUnit
        /// </summary>
        public string OrganizationalUnit
        {
            get { return this.organizationalUnit; }
            set { this.RaiseAndSetIfChanged(ref this.organizationalUnit, value); }
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
        /// Gets or sets the Password
        /// </summary>
        public string Password
        {
            get { return this.password; }
            set { this.RaiseAndSetIfChanged(ref this.password, value); }
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
        /// Gets or sets the Name
        /// </summary>
        public string Name
        {
            get { return this.name; }
            set { this.RaiseAndSetIfChanged(ref this.name, value); }
        }

        /// <summary>
        /// Gets or sets the IsDeprecated
        /// </summary>
        public bool IsDeprecated
        {
            get { return this.isDeprecated; }
            set { this.RaiseAndSetIfChanged(ref this.isDeprecated, value); }
        }

        /// <summary>
        /// Gets or sets the Organization
        /// </summary>
        public Organization Organization
        {
            get { return this.organization; }
            set { this.RaiseAndSetIfChanged(ref this.organization, value); }
        }

        /// <summary>
        /// Gets or sets the DefaultDomain
        /// </summary>
        public DomainOfExpertise DefaultDomain
        {
            get { return this.defaultDomain; }
            set { this.RaiseAndSetIfChanged(ref this.defaultDomain, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="DefaultDomain"/>
        /// </summary>
        public string DefaultDomainShortName
        {
            get { return this.defaultDomainShortName; }
            set { this.RaiseAndSetIfChanged(ref this.defaultDomainShortName, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="DefaultDomain"/>
        /// </summary>
        public string DefaultDomainName
        {
            get { return this.defaultDomainName; }
            set { this.RaiseAndSetIfChanged(ref this.defaultDomainName, value); }
        }

        /// <summary>
        /// Gets or sets the Role
        /// </summary>
        public PersonRole Role
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
        /// Gets or sets the DefaultEmailAddress
        /// </summary>
        public EmailAddress DefaultEmailAddress
        {
            get { return this.defaultEmailAddress; }
            set { this.RaiseAndSetIfChanged(ref this.defaultEmailAddress, value); }
        }

        /// <summary>
        /// Gets or sets the DefaultTelephoneNumber
        /// </summary>
        public TelephoneNumber DefaultTelephoneNumber
        {
            get { return this.defaultTelephoneNumber; }
            set { this.RaiseAndSetIfChanged(ref this.defaultTelephoneNumber, value); }
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
            this.GivenName = this.Thing.GivenName;
            this.Surname = this.Thing.Surname;
            this.OrganizationalUnit = this.Thing.OrganizationalUnit;
            this.IsActive = this.Thing.IsActive;
            this.Password = this.Thing.Password;
            this.ShortName = this.Thing.ShortName;
            this.Name = this.Thing.Name;
            this.IsDeprecated = this.Thing.IsDeprecated;
            this.Organization = this.Thing.Organization;
			if (this.Thing.DefaultDomain != null)
			{
				this.DefaultDomainShortName = this.Thing.DefaultDomain.ShortName;
				this.DefaultDomainName = this.Thing.DefaultDomain.Name;
			}			
            this.DefaultDomain = this.Thing.DefaultDomain;
			if (this.Thing.Role != null)
			{
				this.RoleShortName = this.Thing.Role.ShortName;
				this.RoleName = this.Thing.Role.Name;
			}			
            this.Role = this.Thing.Role;
            this.DefaultEmailAddress = this.Thing.DefaultEmailAddress;
            this.DefaultTelephoneNumber = this.Thing.DefaultTelephoneNumber;
        }
    }
}
