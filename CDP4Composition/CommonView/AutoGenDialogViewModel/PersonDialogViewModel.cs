// -------------------------------------------------------------------------------------------------
// <copyright file="PersonDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2017 RHEA S.A.
// </copyright>
// <summary>
//   This is an auto-generated class. Any manual changes on this file will be overwritten!
// </summary>
// -------------------------------------------------------------------------------------------------

namespace CDP4CommonView
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using CDP4Common;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;
    
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
	using CDP4Dal.Operations;
    using CDP4Dal.Permission;
    using ReactiveUI;
    using System.Reactive;

    /// <summary>
    /// dialog-view-model class representing a <see cref="Person"/>
    /// </summary>
    public partial class PersonDialogViewModel : DialogViewModelBase<Person>
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
        /// Backing field for <see cref="IsDeprecated"/>
        /// </summary>
        private bool isDeprecated;

        /// <summary>
        /// Backing field for <see cref="SelectedOrganization"/>
        /// </summary>
        private Organization selectedOrganization;

        /// <summary>
        /// Backing field for <see cref="SelectedDefaultDomain"/>
        /// </summary>
        private DomainOfExpertise selectedDefaultDomain;

        /// <summary>
        /// Backing field for <see cref="SelectedRole"/>
        /// </summary>
        private PersonRole selectedRole;

        /// <summary>
        /// Backing field for <see cref="SelectedDefaultEmailAddress"/>
        /// </summary>
        private EmailAddress selectedDefaultEmailAddress;

        /// <summary>
        /// Backing field for <see cref="SelectedDefaultTelephoneNumber"/>
        /// </summary>
        private TelephoneNumber selectedDefaultTelephoneNumber;

        /// <summary>
        /// Backing field for <see cref="SelectedEmailAddress"/>
        /// </summary>
        private EmailAddressRowViewModel selectedEmailAddress;

        /// <summary>
        /// Backing field for <see cref="SelectedTelephoneNumber"/>
        /// </summary>
        private TelephoneNumberRowViewModel selectedTelephoneNumber;

        /// <summary>
        /// Backing field for <see cref="SelectedUserPreference"/>
        /// </summary>
        private UserPreferenceRowViewModel selectedUserPreference;


        /// <summary>
        /// Initializes a new instance of the <see cref="PersonDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public PersonDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonDialogViewModel"/> class
        /// </summary>
        /// <param name="person">
        /// The <see cref="Person"/> that is the subject of the current view-model. This is the object
        /// that will be either created, or edited.
        /// </param>
        /// <param name="transaction">
        /// The <see cref="ThingTransaction"/> that contains the log of recorded changes.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the current <see cref="Thing"/> is to be added or updated
        /// </param>
        /// <param name="isRoot">
        /// Assert if this <see cref="DialogViewModelBase{T}"/> is the root of all <see cref="DialogViewModelBase{T}"/>
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation this <see cref="DialogViewModelBase{T}"/> performs
        /// </param>
        /// <param name="thingDialogNavigationService">
        /// The <see cref="IThingDialogNavigationService"/> that is used to navigate to a dialog of a specific <see cref="Thing"/>.
        /// </param>
        /// <param name="container">
        /// The <see cref="Thing"/> that contains the created <see cref="Thing"/> in this Dialog
        /// </param>
        /// <param name="chainOfContainers">
        /// The optional chain of containers that contains the <paramref name="container"/> argument
        /// </param>
        public PersonDialogViewModel(Person person, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(person, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            if(container != null)
            {
                var containerThing = container as SiteDirectory;
                if(containerThing == null)
                {
                    var errorMessage =
                        string.Format(
                            "The container parameter is of type {0}, it shall be of type SiteDirectory",
                            container.GetType());
                    throw new ArgumentException(errorMessage);
                }
            }
        }

        /// <summary>
        /// Gets or sets the GivenName
        /// </summary>
        public virtual string GivenName
        {
            get { return this.givenName; }
            set { this.RaiseAndSetIfChanged(ref this.givenName, value); }
        }

        /// <summary>
        /// Gets or sets the Surname
        /// </summary>
        public virtual string Surname
        {
            get { return this.surname; }
            set { this.RaiseAndSetIfChanged(ref this.surname, value); }
        }

        /// <summary>
        /// Gets or sets the OrganizationalUnit
        /// </summary>
        public virtual string OrganizationalUnit
        {
            get { return this.organizationalUnit; }
            set { this.RaiseAndSetIfChanged(ref this.organizationalUnit, value); }
        }

        /// <summary>
        /// Gets or sets the IsActive
        /// </summary>
        public virtual bool IsActive
        {
            get { return this.isActive; }
            set { this.RaiseAndSetIfChanged(ref this.isActive, value); }
        }

        /// <summary>
        /// Gets or sets the Password
        /// </summary>
        public virtual string Password
        {
            get { return this.password; }
            set { this.RaiseAndSetIfChanged(ref this.password, value); }
        }

        /// <summary>
        /// Gets or sets the ShortName
        /// </summary>
        public virtual string ShortName
        {
            get { return this.shortName; }
            set { this.RaiseAndSetIfChanged(ref this.shortName, value); }
        }

        /// <summary>
        /// Gets or sets the IsDeprecated
        /// </summary>
        public virtual bool IsDeprecated
        {
            get { return this.isDeprecated; }
            set { this.RaiseAndSetIfChanged(ref this.isDeprecated, value); }
        }

        /// <summary>
        /// Gets or sets the SelectedOrganization
        /// </summary>
        public virtual Organization SelectedOrganization
        {
            get { return this.selectedOrganization; }
            set { this.RaiseAndSetIfChanged(ref this.selectedOrganization, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="Organization"/>s for <see cref="SelectedOrganization"/>
        /// </summary>
        public ReactiveList<Organization> PossibleOrganization { get; protected set; }

        /// <summary>
        /// Gets or sets the SelectedDefaultDomain
        /// </summary>
        public virtual DomainOfExpertise SelectedDefaultDomain
        {
            get { return this.selectedDefaultDomain; }
            set { this.RaiseAndSetIfChanged(ref this.selectedDefaultDomain, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="DomainOfExpertise"/>s for <see cref="SelectedDefaultDomain"/>
        /// </summary>
        public ReactiveList<DomainOfExpertise> PossibleDefaultDomain { get; protected set; }

        /// <summary>
        /// Gets or sets the SelectedRole
        /// </summary>
        public virtual PersonRole SelectedRole
        {
            get { return this.selectedRole; }
            set { this.RaiseAndSetIfChanged(ref this.selectedRole, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="PersonRole"/>s for <see cref="SelectedRole"/>
        /// </summary>
        public ReactiveList<PersonRole> PossibleRole { get; protected set; }

        /// <summary>
        /// Gets or sets the SelectedDefaultEmailAddress
        /// </summary>
        public virtual EmailAddress SelectedDefaultEmailAddress
        {
            get { return this.selectedDefaultEmailAddress; }
            set { this.RaiseAndSetIfChanged(ref this.selectedDefaultEmailAddress, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="EmailAddress"/>s for <see cref="SelectedDefaultEmailAddress"/>
        /// </summary>
        public ReactiveList<EmailAddress> PossibleDefaultEmailAddress { get; protected set; }

        /// <summary>
        /// Gets or sets the SelectedDefaultTelephoneNumber
        /// </summary>
        public virtual TelephoneNumber SelectedDefaultTelephoneNumber
        {
            get { return this.selectedDefaultTelephoneNumber; }
            set { this.RaiseAndSetIfChanged(ref this.selectedDefaultTelephoneNumber, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="TelephoneNumber"/>s for <see cref="SelectedDefaultTelephoneNumber"/>
        /// </summary>
        public ReactiveList<TelephoneNumber> PossibleDefaultTelephoneNumber { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="EmailAddressRowViewModel"/>
        /// </summary>
        public EmailAddressRowViewModel SelectedEmailAddress
        {
            get { return this.selectedEmailAddress; }
            set { this.RaiseAndSetIfChanged(ref this.selectedEmailAddress, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="EmailAddress"/>
        /// </summary>
        public ReactiveList<EmailAddressRowViewModel> EmailAddress { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="TelephoneNumberRowViewModel"/>
        /// </summary>
        public TelephoneNumberRowViewModel SelectedTelephoneNumber
        {
            get { return this.selectedTelephoneNumber; }
            set { this.RaiseAndSetIfChanged(ref this.selectedTelephoneNumber, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="TelephoneNumber"/>
        /// </summary>
        public ReactiveList<TelephoneNumberRowViewModel> TelephoneNumber { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="UserPreferenceRowViewModel"/>
        /// </summary>
        public UserPreferenceRowViewModel SelectedUserPreference
        {
            get { return this.selectedUserPreference; }
            set { this.RaiseAndSetIfChanged(ref this.selectedUserPreference, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="UserPreference"/>
        /// </summary>
        public ReactiveList<UserPreferenceRowViewModel> UserPreference { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedOrganization"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectSelectedOrganizationCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedDefaultDomain"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectSelectedDefaultDomainCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedRole"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectSelectedRoleCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedDefaultEmailAddress"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectSelectedDefaultEmailAddressCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedDefaultTelephoneNumber"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectSelectedDefaultTelephoneNumberCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a EmailAddress
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateEmailAddressCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a EmailAddress
        /// </summary>
        public ReactiveCommand<Unit, Unit> DeleteEmailAddressCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a EmailAddress
        /// </summary>
        public ReactiveCommand<Unit, Unit> EditEmailAddressCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a EmailAddress
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectEmailAddressCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a TelephoneNumber
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateTelephoneNumberCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a TelephoneNumber
        /// </summary>
        public ReactiveCommand<Unit, Unit> DeleteTelephoneNumberCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a TelephoneNumber
        /// </summary>
        public ReactiveCommand<Unit, Unit> EditTelephoneNumberCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a TelephoneNumber
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectTelephoneNumberCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a UserPreference
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateUserPreferenceCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a UserPreference
        /// </summary>
        public ReactiveCommand<Unit, Unit> DeleteUserPreferenceCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a UserPreference
        /// </summary>
        public ReactiveCommand<Unit, Unit> EditUserPreferenceCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a UserPreference
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectUserPreferenceCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            
            var canExecuteCreateEmailAddressCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedEmailAddressCommand = this.WhenAny(vm => vm.SelectedEmailAddress, v => v.Value != null);
            var canExecuteEditSelectedEmailAddressCommand = this.WhenAny(vm => vm.SelectedEmailAddress, v => v.Value != null && !this.IsReadOnly);

            this.CreateEmailAddressCommand = ReactiveCommandCreator.Create(canExecuteCreateEmailAddressCommand);
            this.CreateEmailAddressCommand.Subscribe(_ => this.ExecuteCreateCommand<EmailAddress>(this.PopulateEmailAddress));

            this.DeleteEmailAddressCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedEmailAddressCommand);
            this.DeleteEmailAddressCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedEmailAddress.Thing, this.PopulateEmailAddress));

            this.EditEmailAddressCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedEmailAddressCommand);
            this.EditEmailAddressCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedEmailAddress.Thing, this.PopulateEmailAddress));

            this.InspectEmailAddressCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedEmailAddressCommand);
            this.InspectEmailAddressCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedEmailAddress.Thing));
            
            var canExecuteCreateTelephoneNumberCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedTelephoneNumberCommand = this.WhenAny(vm => vm.SelectedTelephoneNumber, v => v.Value != null);
            var canExecuteEditSelectedTelephoneNumberCommand = this.WhenAny(vm => vm.SelectedTelephoneNumber, v => v.Value != null && !this.IsReadOnly);

            this.CreateTelephoneNumberCommand = ReactiveCommandCreator.Create(canExecuteCreateTelephoneNumberCommand);
            this.CreateTelephoneNumberCommand.Subscribe(_ => this.ExecuteCreateCommand<TelephoneNumber>(this.PopulateTelephoneNumber));

            this.DeleteTelephoneNumberCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedTelephoneNumberCommand);
            this.DeleteTelephoneNumberCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedTelephoneNumber.Thing, this.PopulateTelephoneNumber));

            this.EditTelephoneNumberCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedTelephoneNumberCommand);
            this.EditTelephoneNumberCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedTelephoneNumber.Thing, this.PopulateTelephoneNumber));

            this.InspectTelephoneNumberCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedTelephoneNumberCommand);
            this.InspectTelephoneNumberCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedTelephoneNumber.Thing));
            
            var canExecuteCreateUserPreferenceCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedUserPreferenceCommand = this.WhenAny(vm => vm.SelectedUserPreference, v => v.Value != null);
            var canExecuteEditSelectedUserPreferenceCommand = this.WhenAny(vm => vm.SelectedUserPreference, v => v.Value != null && !this.IsReadOnly);

            this.CreateUserPreferenceCommand = ReactiveCommandCreator.Create(canExecuteCreateUserPreferenceCommand);
            this.CreateUserPreferenceCommand.Subscribe(_ => this.ExecuteCreateCommand<UserPreference>(this.PopulateUserPreference));

            this.DeleteUserPreferenceCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedUserPreferenceCommand);
            this.DeleteUserPreferenceCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedUserPreference.Thing, this.PopulateUserPreference));

            this.EditUserPreferenceCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedUserPreferenceCommand);
            this.EditUserPreferenceCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedUserPreference.Thing, this.PopulateUserPreference));

            this.InspectUserPreferenceCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedUserPreferenceCommand);
            this.InspectUserPreferenceCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedUserPreference.Thing));
            var canExecuteInspectSelectedOrganizationCommand = this.WhenAny(vm => vm.SelectedOrganization, v => v.Value != null);
            this.InspectSelectedOrganizationCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedOrganizationCommand);
            this.InspectSelectedOrganizationCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedOrganization));
            var canExecuteInspectSelectedDefaultDomainCommand = this.WhenAny(vm => vm.SelectedDefaultDomain, v => v.Value != null);
            this.InspectSelectedDefaultDomainCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedDefaultDomainCommand);
            this.InspectSelectedDefaultDomainCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedDefaultDomain));
            var canExecuteInspectSelectedRoleCommand = this.WhenAny(vm => vm.SelectedRole, v => v.Value != null);
            this.InspectSelectedRoleCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedRoleCommand);
            this.InspectSelectedRoleCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedRole));
            var canExecuteInspectSelectedDefaultEmailAddressCommand = this.WhenAny(vm => vm.SelectedDefaultEmailAddress, v => v.Value != null);
            this.InspectSelectedDefaultEmailAddressCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedDefaultEmailAddressCommand);
            this.InspectSelectedDefaultEmailAddressCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedDefaultEmailAddress));
            var canExecuteInspectSelectedDefaultTelephoneNumberCommand = this.WhenAny(vm => vm.SelectedDefaultTelephoneNumber, v => v.Value != null);
            this.InspectSelectedDefaultTelephoneNumberCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedDefaultTelephoneNumberCommand);
            this.InspectSelectedDefaultTelephoneNumberCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedDefaultTelephoneNumber));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.GivenName = this.GivenName;
            clone.Surname = this.Surname;
            clone.OrganizationalUnit = this.OrganizationalUnit;
            clone.IsActive = this.IsActive;
            clone.ShortName = this.ShortName;
            clone.IsDeprecated = this.IsDeprecated;
            clone.Organization = this.SelectedOrganization;
            clone.DefaultDomain = this.SelectedDefaultDomain;
            clone.Role = this.SelectedRole;
            clone.DefaultEmailAddress = this.SelectedDefaultEmailAddress;
            clone.DefaultTelephoneNumber = this.SelectedDefaultTelephoneNumber;
        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PossibleOrganization = new ReactiveList<Organization>();
            this.PossibleDefaultDomain = new ReactiveList<DomainOfExpertise>();
            this.PossibleRole = new ReactiveList<PersonRole>();
            this.PossibleDefaultEmailAddress = new ReactiveList<EmailAddress>();
            this.PossibleDefaultTelephoneNumber = new ReactiveList<TelephoneNumber>();
            this.EmailAddress = new ReactiveList<EmailAddressRowViewModel>();
            this.TelephoneNumber = new ReactiveList<TelephoneNumberRowViewModel>();
            this.UserPreference = new ReactiveList<UserPreferenceRowViewModel>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.GivenName = this.Thing.GivenName;
            this.Surname = this.Thing.Surname;
            this.OrganizationalUnit = this.Thing.OrganizationalUnit;
            this.IsActive = this.Thing.IsActive;
            this.Password = this.Thing.Password;
            this.ShortName = this.Thing.ShortName;
            this.IsDeprecated = this.Thing.IsDeprecated;
            this.SelectedOrganization = this.Thing.Organization;
            this.PopulatePossibleOrganization();
            this.SelectedDefaultDomain = this.Thing.DefaultDomain;
            this.PopulatePossibleDefaultDomain();
            this.SelectedRole = this.Thing.Role;
            this.PopulatePossibleRole();
            this.SelectedDefaultEmailAddress = this.Thing.DefaultEmailAddress;
            this.PopulatePossibleDefaultEmailAddress();
            this.SelectedDefaultTelephoneNumber = this.Thing.DefaultTelephoneNumber;
            this.PopulatePossibleDefaultTelephoneNumber();
            this.PopulateEmailAddress();
            this.PopulateTelephoneNumber();
            this.PopulateUserPreference();
        }

        /// <summary>
        /// Populates the <see cref="EmailAddress"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateEmailAddress()
        {
            this.EmailAddress.Clear();
            foreach (var thing in this.Thing.EmailAddress.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new EmailAddressRowViewModel(thing, this.Session, this);
                this.EmailAddress.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="TelephoneNumber"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateTelephoneNumber()
        {
            this.TelephoneNumber.Clear();
            foreach (var thing in this.Thing.TelephoneNumber.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new TelephoneNumberRowViewModel(thing, this.Session, this);
                this.TelephoneNumber.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="UserPreference"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateUserPreference()
        {
            this.UserPreference.Clear();
            foreach (var thing in this.Thing.UserPreference.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new UserPreferenceRowViewModel(thing, this.Session, this);
                this.UserPreference.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="PossibleOrganization"/> property
        /// </summary>
        protected virtual void PopulatePossibleOrganization()
        {
            this.PossibleOrganization.Clear();
        }

        /// <summary>
        /// Populates the <see cref="PossibleDefaultDomain"/> property
        /// </summary>
        protected virtual void PopulatePossibleDefaultDomain()
        {
            this.PossibleDefaultDomain.Clear();
        }

        /// <summary>
        /// Populates the <see cref="PossibleRole"/> property
        /// </summary>
        protected virtual void PopulatePossibleRole()
        {
            this.PossibleRole.Clear();
        }

        /// <summary>
        /// Populates the <see cref="PossibleDefaultEmailAddress"/> property
        /// </summary>
        protected virtual void PopulatePossibleDefaultEmailAddress()
        {
            this.PossibleDefaultEmailAddress.Clear();
        }

        /// <summary>
        /// Populates the <see cref="PossibleDefaultTelephoneNumber"/> property
        /// </summary>
        protected virtual void PopulatePossibleDefaultTelephoneNumber()
        {
            this.PossibleDefaultTelephoneNumber.Clear();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// a value indicating whether the class is being disposed of
        /// </param>
        /// <remarks>
        /// This method is called by the <see cref="ThingDialogNavigationService"/> when the Dialog is closed
        /// </remarks>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            foreach(var emailAddress in this.EmailAddress)
            {
                emailAddress.Dispose();
            }
            foreach(var telephoneNumber in this.TelephoneNumber)
            {
                telephoneNumber.Dispose();
            }
            foreach(var userPreference in this.UserPreference)
            {
                userPreference.Dispose();
            }
        }
    }
}
