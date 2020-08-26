// -------------------------------------------------------------------------------------------------
// <copyright file="SiteDirectoryDialogViewModel.cs" company="RHEA System S.A.">
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
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;
    
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
	using CDP4Dal.Operations;
    using CDP4Dal.Permission;
    using ReactiveUI;

    /// <summary>
    /// dialog-view-model class representing a <see cref="SiteDirectory"/>
    /// </summary>
    public partial class SiteDirectoryDialogViewModel : TopContainerDialogViewModel<SiteDirectory>
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
        /// Backing field for <see cref="SelectedDefaultParticipantRole"/>
        /// </summary>
        private ParticipantRole selectedDefaultParticipantRole;

        /// <summary>
        /// Backing field for <see cref="SelectedDefaultPersonRole"/>
        /// </summary>
        private PersonRole selectedDefaultPersonRole;

        /// <summary>
        /// Backing field for <see cref="SelectedOrganization"/>
        /// </summary>
        private OrganizationRowViewModel selectedOrganization;

        /// <summary>
        /// Backing field for <see cref="SelectedPerson"/>
        /// </summary>
        private PersonRowViewModel selectedPerson;

        /// <summary>
        /// Backing field for <see cref="SelectedParticipantRole"/>
        /// </summary>
        private ParticipantRoleRowViewModel selectedParticipantRole;

        /// <summary>
        /// Backing field for <see cref="SelectedSiteReferenceDataLibrary"/>
        /// </summary>
        private SiteReferenceDataLibraryRowViewModel selectedSiteReferenceDataLibrary;

        /// <summary>
        /// Backing field for <see cref="SelectedModel"/>
        /// </summary>
        private EngineeringModelSetupRowViewModel selectedModel;

        /// <summary>
        /// Backing field for <see cref="SelectedPersonRole"/>
        /// </summary>
        private PersonRoleRowViewModel selectedPersonRole;

        /// <summary>
        /// Backing field for <see cref="SelectedLogEntry"/>
        /// </summary>
        private SiteLogEntryRowViewModel selectedLogEntry;

        /// <summary>
        /// Backing field for <see cref="SelectedDomainGroup"/>
        /// </summary>
        private DomainOfExpertiseGroupRowViewModel selectedDomainGroup;

        /// <summary>
        /// Backing field for <see cref="SelectedDomain"/>
        /// </summary>
        private DomainOfExpertiseRowViewModel selectedDomain;

        /// <summary>
        /// Backing field for <see cref="SelectedNaturalLanguage"/>
        /// </summary>
        private NaturalLanguageRowViewModel selectedNaturalLanguage;

        /// <summary>
        /// Backing field for <see cref="SelectedAnnotation"/>
        /// </summary>
        private SiteDirectoryDataAnnotationRowViewModel selectedAnnotation;


        /// <summary>
        /// Initializes a new instance of the <see cref="SiteDirectoryDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public SiteDirectoryDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SiteDirectoryDialogViewModel"/> class
        /// </summary>
        /// <param name="siteDirectory">
        /// The <see cref="SiteDirectory"/> that is the subject of the current view-model. This is the object
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
        public SiteDirectoryDialogViewModel(SiteDirectory siteDirectory, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(siteDirectory, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
        }

        /// <summary>
        /// Gets or sets the CreatedOn
        /// </summary>
        public virtual DateTime CreatedOn
        {
            get { return this.createdOn; }
            set { this.RaiseAndSetIfChanged(ref this.createdOn, value); }
        }

        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        public virtual string Name
        {
            get { return this.name; }
            set { this.RaiseAndSetIfChanged(ref this.name, value); }
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
        /// Gets or sets the SelectedDefaultParticipantRole
        /// </summary>
        public virtual ParticipantRole SelectedDefaultParticipantRole
        {
            get { return this.selectedDefaultParticipantRole; }
            set { this.RaiseAndSetIfChanged(ref this.selectedDefaultParticipantRole, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="ParticipantRole"/>s for <see cref="SelectedDefaultParticipantRole"/>
        /// </summary>
        public ReactiveList<ParticipantRole> PossibleDefaultParticipantRole { get; protected set; }

        /// <summary>
        /// Gets or sets the SelectedDefaultPersonRole
        /// </summary>
        public virtual PersonRole SelectedDefaultPersonRole
        {
            get { return this.selectedDefaultPersonRole; }
            set { this.RaiseAndSetIfChanged(ref this.selectedDefaultPersonRole, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="PersonRole"/>s for <see cref="SelectedDefaultPersonRole"/>
        /// </summary>
        public ReactiveList<PersonRole> PossibleDefaultPersonRole { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="OrganizationRowViewModel"/>
        /// </summary>
        public OrganizationRowViewModel SelectedOrganization
        {
            get { return this.selectedOrganization; }
            set { this.RaiseAndSetIfChanged(ref this.selectedOrganization, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="Organization"/>
        /// </summary>
        public ReactiveList<OrganizationRowViewModel> Organization { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="PersonRowViewModel"/>
        /// </summary>
        public PersonRowViewModel SelectedPerson
        {
            get { return this.selectedPerson; }
            set { this.RaiseAndSetIfChanged(ref this.selectedPerson, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="Person"/>
        /// </summary>
        public ReactiveList<PersonRowViewModel> Person { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="ParticipantRoleRowViewModel"/>
        /// </summary>
        public ParticipantRoleRowViewModel SelectedParticipantRole
        {
            get { return this.selectedParticipantRole; }
            set { this.RaiseAndSetIfChanged(ref this.selectedParticipantRole, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="ParticipantRole"/>
        /// </summary>
        public ReactiveList<ParticipantRoleRowViewModel> ParticipantRole { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="SiteReferenceDataLibraryRowViewModel"/>
        /// </summary>
        public SiteReferenceDataLibraryRowViewModel SelectedSiteReferenceDataLibrary
        {
            get { return this.selectedSiteReferenceDataLibrary; }
            set { this.RaiseAndSetIfChanged(ref this.selectedSiteReferenceDataLibrary, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="SiteReferenceDataLibrary"/>
        /// </summary>
        public ReactiveList<SiteReferenceDataLibraryRowViewModel> SiteReferenceDataLibrary { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="EngineeringModelSetupRowViewModel"/>
        /// </summary>
        public EngineeringModelSetupRowViewModel SelectedModel
        {
            get { return this.selectedModel; }
            set { this.RaiseAndSetIfChanged(ref this.selectedModel, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="EngineeringModelSetup"/>
        /// </summary>
        public ReactiveList<EngineeringModelSetupRowViewModel> Model { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="PersonRoleRowViewModel"/>
        /// </summary>
        public PersonRoleRowViewModel SelectedPersonRole
        {
            get { return this.selectedPersonRole; }
            set { this.RaiseAndSetIfChanged(ref this.selectedPersonRole, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="PersonRole"/>
        /// </summary>
        public ReactiveList<PersonRoleRowViewModel> PersonRole { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="SiteLogEntryRowViewModel"/>
        /// </summary>
        public SiteLogEntryRowViewModel SelectedLogEntry
        {
            get { return this.selectedLogEntry; }
            set { this.RaiseAndSetIfChanged(ref this.selectedLogEntry, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="SiteLogEntry"/>
        /// </summary>
        public ReactiveList<SiteLogEntryRowViewModel> LogEntry { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="DomainOfExpertiseGroupRowViewModel"/>
        /// </summary>
        public DomainOfExpertiseGroupRowViewModel SelectedDomainGroup
        {
            get { return this.selectedDomainGroup; }
            set { this.RaiseAndSetIfChanged(ref this.selectedDomainGroup, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="DomainOfExpertiseGroup"/>
        /// </summary>
        public ReactiveList<DomainOfExpertiseGroupRowViewModel> DomainGroup { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="DomainOfExpertiseRowViewModel"/>
        /// </summary>
        public DomainOfExpertiseRowViewModel SelectedDomain
        {
            get { return this.selectedDomain; }
            set { this.RaiseAndSetIfChanged(ref this.selectedDomain, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="DomainOfExpertise"/>
        /// </summary>
        public ReactiveList<DomainOfExpertiseRowViewModel> Domain { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="NaturalLanguageRowViewModel"/>
        /// </summary>
        public NaturalLanguageRowViewModel SelectedNaturalLanguage
        {
            get { return this.selectedNaturalLanguage; }
            set { this.RaiseAndSetIfChanged(ref this.selectedNaturalLanguage, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="NaturalLanguage"/>
        /// </summary>
        public ReactiveList<NaturalLanguageRowViewModel> NaturalLanguage { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="SiteDirectoryDataAnnotationRowViewModel"/>
        /// </summary>
        public SiteDirectoryDataAnnotationRowViewModel SelectedAnnotation
        {
            get { return this.selectedAnnotation; }
            set { this.RaiseAndSetIfChanged(ref this.selectedAnnotation, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="SiteDirectoryDataAnnotation"/>
        /// </summary>
        public ReactiveList<SiteDirectoryDataAnnotationRowViewModel> Annotation { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedDefaultParticipantRole"/>
        /// </summary>
        public ReactiveCommand<object> InspectSelectedDefaultParticipantRoleCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedDefaultPersonRole"/>
        /// </summary>
        public ReactiveCommand<object> InspectSelectedDefaultPersonRoleCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a Organization
        /// </summary>
        public ReactiveCommand<object> CreateOrganizationCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a Organization
        /// </summary>
        public ReactiveCommand<object> DeleteOrganizationCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a Organization
        /// </summary>
        public ReactiveCommand<object> EditOrganizationCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a Organization
        /// </summary>
        public ReactiveCommand<object> InspectOrganizationCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a Person
        /// </summary>
        public ReactiveCommand<object> CreatePersonCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a Person
        /// </summary>
        public ReactiveCommand<object> DeletePersonCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a Person
        /// </summary>
        public ReactiveCommand<object> EditPersonCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a Person
        /// </summary>
        public ReactiveCommand<object> InspectPersonCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a ParticipantRole
        /// </summary>
        public ReactiveCommand<object> CreateParticipantRoleCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a ParticipantRole
        /// </summary>
        public ReactiveCommand<object> DeleteParticipantRoleCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a ParticipantRole
        /// </summary>
        public ReactiveCommand<object> EditParticipantRoleCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a ParticipantRole
        /// </summary>
        public ReactiveCommand<object> InspectParticipantRoleCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a SiteReferenceDataLibrary
        /// </summary>
        public ReactiveCommand<object> CreateSiteReferenceDataLibraryCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a SiteReferenceDataLibrary
        /// </summary>
        public ReactiveCommand<object> DeleteSiteReferenceDataLibraryCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a SiteReferenceDataLibrary
        /// </summary>
        public ReactiveCommand<object> EditSiteReferenceDataLibraryCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a SiteReferenceDataLibrary
        /// </summary>
        public ReactiveCommand<object> InspectSiteReferenceDataLibraryCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a EngineeringModelSetup
        /// </summary>
        public ReactiveCommand<object> CreateModelCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a EngineeringModelSetup
        /// </summary>
        public ReactiveCommand<object> DeleteModelCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a EngineeringModelSetup
        /// </summary>
        public ReactiveCommand<object> EditModelCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a EngineeringModelSetup
        /// </summary>
        public ReactiveCommand<object> InspectModelCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a PersonRole
        /// </summary>
        public ReactiveCommand<object> CreatePersonRoleCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a PersonRole
        /// </summary>
        public ReactiveCommand<object> DeletePersonRoleCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a PersonRole
        /// </summary>
        public ReactiveCommand<object> EditPersonRoleCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a PersonRole
        /// </summary>
        public ReactiveCommand<object> InspectPersonRoleCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a SiteLogEntry
        /// </summary>
        public ReactiveCommand<object> CreateLogEntryCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a SiteLogEntry
        /// </summary>
        public ReactiveCommand<object> DeleteLogEntryCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a SiteLogEntry
        /// </summary>
        public ReactiveCommand<object> EditLogEntryCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a SiteLogEntry
        /// </summary>
        public ReactiveCommand<object> InspectLogEntryCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a DomainOfExpertiseGroup
        /// </summary>
        public ReactiveCommand<object> CreateDomainGroupCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a DomainOfExpertiseGroup
        /// </summary>
        public ReactiveCommand<object> DeleteDomainGroupCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a DomainOfExpertiseGroup
        /// </summary>
        public ReactiveCommand<object> EditDomainGroupCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a DomainOfExpertiseGroup
        /// </summary>
        public ReactiveCommand<object> InspectDomainGroupCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a DomainOfExpertise
        /// </summary>
        public ReactiveCommand<object> CreateDomainCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a DomainOfExpertise
        /// </summary>
        public ReactiveCommand<object> DeleteDomainCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a DomainOfExpertise
        /// </summary>
        public ReactiveCommand<object> EditDomainCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a DomainOfExpertise
        /// </summary>
        public ReactiveCommand<object> InspectDomainCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a NaturalLanguage
        /// </summary>
        public ReactiveCommand<object> CreateNaturalLanguageCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a NaturalLanguage
        /// </summary>
        public ReactiveCommand<object> DeleteNaturalLanguageCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a NaturalLanguage
        /// </summary>
        public ReactiveCommand<object> EditNaturalLanguageCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a NaturalLanguage
        /// </summary>
        public ReactiveCommand<object> InspectNaturalLanguageCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a SiteDirectoryDataAnnotation
        /// </summary>
        public ReactiveCommand<object> CreateAnnotationCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a SiteDirectoryDataAnnotation
        /// </summary>
        public ReactiveCommand<object> DeleteAnnotationCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a SiteDirectoryDataAnnotation
        /// </summary>
        public ReactiveCommand<object> EditAnnotationCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a SiteDirectoryDataAnnotation
        /// </summary>
        public ReactiveCommand<object> InspectAnnotationCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            
            var canExecuteCreateOrganizationCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedOrganizationCommand = this.WhenAny(vm => vm.SelectedOrganization, v => v.Value != null);
            var canExecuteEditSelectedOrganizationCommand = this.WhenAny(vm => vm.SelectedOrganization, v => v.Value != null && !this.IsReadOnly);

            this.CreateOrganizationCommand = ReactiveCommand.Create(canExecuteCreateOrganizationCommand);
            this.CreateOrganizationCommand.Subscribe(_ => this.ExecuteCreateCommand<Organization>(this.PopulateOrganization));

            this.DeleteOrganizationCommand = ReactiveCommand.Create(canExecuteEditSelectedOrganizationCommand);
            this.DeleteOrganizationCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedOrganization.Thing, this.PopulateOrganization));

            this.EditOrganizationCommand = ReactiveCommand.Create(canExecuteEditSelectedOrganizationCommand);
            this.EditOrganizationCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedOrganization.Thing, this.PopulateOrganization));

            this.InspectOrganizationCommand = ReactiveCommand.Create(canExecuteInspectSelectedOrganizationCommand);
            this.InspectOrganizationCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedOrganization.Thing));
            
            var canExecuteCreatePersonCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedPersonCommand = this.WhenAny(vm => vm.SelectedPerson, v => v.Value != null);
            var canExecuteEditSelectedPersonCommand = this.WhenAny(vm => vm.SelectedPerson, v => v.Value != null && !this.IsReadOnly);

            this.CreatePersonCommand = ReactiveCommand.Create(canExecuteCreatePersonCommand);
            this.CreatePersonCommand.Subscribe(_ => this.ExecuteCreateCommand<Person>(this.PopulatePerson));

            this.DeletePersonCommand = ReactiveCommand.Create(canExecuteEditSelectedPersonCommand);
            this.DeletePersonCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedPerson.Thing, this.PopulatePerson));

            this.EditPersonCommand = ReactiveCommand.Create(canExecuteEditSelectedPersonCommand);
            this.EditPersonCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedPerson.Thing, this.PopulatePerson));

            this.InspectPersonCommand = ReactiveCommand.Create(canExecuteInspectSelectedPersonCommand);
            this.InspectPersonCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedPerson.Thing));
            
            var canExecuteCreateParticipantRoleCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedParticipantRoleCommand = this.WhenAny(vm => vm.SelectedParticipantRole, v => v.Value != null);
            var canExecuteEditSelectedParticipantRoleCommand = this.WhenAny(vm => vm.SelectedParticipantRole, v => v.Value != null && !this.IsReadOnly);

            this.CreateParticipantRoleCommand = ReactiveCommand.Create(canExecuteCreateParticipantRoleCommand);
            this.CreateParticipantRoleCommand.Subscribe(_ => this.ExecuteCreateCommand<ParticipantRole>(this.PopulateParticipantRole));

            this.DeleteParticipantRoleCommand = ReactiveCommand.Create(canExecuteEditSelectedParticipantRoleCommand);
            this.DeleteParticipantRoleCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedParticipantRole.Thing, this.PopulateParticipantRole));

            this.EditParticipantRoleCommand = ReactiveCommand.Create(canExecuteEditSelectedParticipantRoleCommand);
            this.EditParticipantRoleCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedParticipantRole.Thing, this.PopulateParticipantRole));

            this.InspectParticipantRoleCommand = ReactiveCommand.Create(canExecuteInspectSelectedParticipantRoleCommand);
            this.InspectParticipantRoleCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedParticipantRole.Thing));
            
            var canExecuteCreateSiteReferenceDataLibraryCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedSiteReferenceDataLibraryCommand = this.WhenAny(vm => vm.SelectedSiteReferenceDataLibrary, v => v.Value != null);
            var canExecuteEditSelectedSiteReferenceDataLibraryCommand = this.WhenAny(vm => vm.SelectedSiteReferenceDataLibrary, v => v.Value != null && !this.IsReadOnly);

            this.CreateSiteReferenceDataLibraryCommand = ReactiveCommand.Create(canExecuteCreateSiteReferenceDataLibraryCommand);
            this.CreateSiteReferenceDataLibraryCommand.Subscribe(_ => this.ExecuteCreateCommand<SiteReferenceDataLibrary>(this.PopulateSiteReferenceDataLibrary));

            this.DeleteSiteReferenceDataLibraryCommand = ReactiveCommand.Create(canExecuteEditSelectedSiteReferenceDataLibraryCommand);
            this.DeleteSiteReferenceDataLibraryCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedSiteReferenceDataLibrary.Thing, this.PopulateSiteReferenceDataLibrary));

            this.EditSiteReferenceDataLibraryCommand = ReactiveCommand.Create(canExecuteEditSelectedSiteReferenceDataLibraryCommand);
            this.EditSiteReferenceDataLibraryCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedSiteReferenceDataLibrary.Thing, this.PopulateSiteReferenceDataLibrary));

            this.InspectSiteReferenceDataLibraryCommand = ReactiveCommand.Create(canExecuteInspectSelectedSiteReferenceDataLibraryCommand);
            this.InspectSiteReferenceDataLibraryCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedSiteReferenceDataLibrary.Thing));
            
            var canExecuteCreateModelCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedModelCommand = this.WhenAny(vm => vm.SelectedModel, v => v.Value != null);
            var canExecuteEditSelectedModelCommand = this.WhenAny(vm => vm.SelectedModel, v => v.Value != null && !this.IsReadOnly);

            this.CreateModelCommand = ReactiveCommand.Create(canExecuteCreateModelCommand);
            this.CreateModelCommand.Subscribe(_ => this.ExecuteCreateCommand<EngineeringModelSetup>(this.PopulateModel));

            this.DeleteModelCommand = ReactiveCommand.Create(canExecuteEditSelectedModelCommand);
            this.DeleteModelCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedModel.Thing, this.PopulateModel));

            this.EditModelCommand = ReactiveCommand.Create(canExecuteEditSelectedModelCommand);
            this.EditModelCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedModel.Thing, this.PopulateModel));

            this.InspectModelCommand = ReactiveCommand.Create(canExecuteInspectSelectedModelCommand);
            this.InspectModelCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedModel.Thing));
            
            var canExecuteCreatePersonRoleCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedPersonRoleCommand = this.WhenAny(vm => vm.SelectedPersonRole, v => v.Value != null);
            var canExecuteEditSelectedPersonRoleCommand = this.WhenAny(vm => vm.SelectedPersonRole, v => v.Value != null && !this.IsReadOnly);

            this.CreatePersonRoleCommand = ReactiveCommand.Create(canExecuteCreatePersonRoleCommand);
            this.CreatePersonRoleCommand.Subscribe(_ => this.ExecuteCreateCommand<PersonRole>(this.PopulatePersonRole));

            this.DeletePersonRoleCommand = ReactiveCommand.Create(canExecuteEditSelectedPersonRoleCommand);
            this.DeletePersonRoleCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedPersonRole.Thing, this.PopulatePersonRole));

            this.EditPersonRoleCommand = ReactiveCommand.Create(canExecuteEditSelectedPersonRoleCommand);
            this.EditPersonRoleCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedPersonRole.Thing, this.PopulatePersonRole));

            this.InspectPersonRoleCommand = ReactiveCommand.Create(canExecuteInspectSelectedPersonRoleCommand);
            this.InspectPersonRoleCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedPersonRole.Thing));
            
            var canExecuteCreateLogEntryCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedLogEntryCommand = this.WhenAny(vm => vm.SelectedLogEntry, v => v.Value != null);
            var canExecuteEditSelectedLogEntryCommand = this.WhenAny(vm => vm.SelectedLogEntry, v => v.Value != null && !this.IsReadOnly);

            this.CreateLogEntryCommand = ReactiveCommand.Create(canExecuteCreateLogEntryCommand);
            this.CreateLogEntryCommand.Subscribe(_ => this.ExecuteCreateCommand<SiteLogEntry>(this.PopulateLogEntry));

            this.DeleteLogEntryCommand = ReactiveCommand.Create(canExecuteEditSelectedLogEntryCommand);
            this.DeleteLogEntryCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedLogEntry.Thing, this.PopulateLogEntry));

            this.EditLogEntryCommand = ReactiveCommand.Create(canExecuteEditSelectedLogEntryCommand);
            this.EditLogEntryCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedLogEntry.Thing, this.PopulateLogEntry));

            this.InspectLogEntryCommand = ReactiveCommand.Create(canExecuteInspectSelectedLogEntryCommand);
            this.InspectLogEntryCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedLogEntry.Thing));
            
            var canExecuteCreateDomainGroupCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedDomainGroupCommand = this.WhenAny(vm => vm.SelectedDomainGroup, v => v.Value != null);
            var canExecuteEditSelectedDomainGroupCommand = this.WhenAny(vm => vm.SelectedDomainGroup, v => v.Value != null && !this.IsReadOnly);

            this.CreateDomainGroupCommand = ReactiveCommand.Create(canExecuteCreateDomainGroupCommand);
            this.CreateDomainGroupCommand.Subscribe(_ => this.ExecuteCreateCommand<DomainOfExpertiseGroup>(this.PopulateDomainGroup));

            this.DeleteDomainGroupCommand = ReactiveCommand.Create(canExecuteEditSelectedDomainGroupCommand);
            this.DeleteDomainGroupCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedDomainGroup.Thing, this.PopulateDomainGroup));

            this.EditDomainGroupCommand = ReactiveCommand.Create(canExecuteEditSelectedDomainGroupCommand);
            this.EditDomainGroupCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedDomainGroup.Thing, this.PopulateDomainGroup));

            this.InspectDomainGroupCommand = ReactiveCommand.Create(canExecuteInspectSelectedDomainGroupCommand);
            this.InspectDomainGroupCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedDomainGroup.Thing));
            
            var canExecuteCreateDomainCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedDomainCommand = this.WhenAny(vm => vm.SelectedDomain, v => v.Value != null);
            var canExecuteEditSelectedDomainCommand = this.WhenAny(vm => vm.SelectedDomain, v => v.Value != null && !this.IsReadOnly);

            this.CreateDomainCommand = ReactiveCommand.Create(canExecuteCreateDomainCommand);
            this.CreateDomainCommand.Subscribe(_ => this.ExecuteCreateCommand<DomainOfExpertise>(this.PopulateDomain));

            this.DeleteDomainCommand = ReactiveCommand.Create(canExecuteEditSelectedDomainCommand);
            this.DeleteDomainCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedDomain.Thing, this.PopulateDomain));

            this.EditDomainCommand = ReactiveCommand.Create(canExecuteEditSelectedDomainCommand);
            this.EditDomainCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedDomain.Thing, this.PopulateDomain));

            this.InspectDomainCommand = ReactiveCommand.Create(canExecuteInspectSelectedDomainCommand);
            this.InspectDomainCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedDomain.Thing));
            
            var canExecuteCreateNaturalLanguageCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedNaturalLanguageCommand = this.WhenAny(vm => vm.SelectedNaturalLanguage, v => v.Value != null);
            var canExecuteEditSelectedNaturalLanguageCommand = this.WhenAny(vm => vm.SelectedNaturalLanguage, v => v.Value != null && !this.IsReadOnly);

            this.CreateNaturalLanguageCommand = ReactiveCommand.Create(canExecuteCreateNaturalLanguageCommand);
            this.CreateNaturalLanguageCommand.Subscribe(_ => this.ExecuteCreateCommand<NaturalLanguage>(this.PopulateNaturalLanguage));

            this.DeleteNaturalLanguageCommand = ReactiveCommand.Create(canExecuteEditSelectedNaturalLanguageCommand);
            this.DeleteNaturalLanguageCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedNaturalLanguage.Thing, this.PopulateNaturalLanguage));

            this.EditNaturalLanguageCommand = ReactiveCommand.Create(canExecuteEditSelectedNaturalLanguageCommand);
            this.EditNaturalLanguageCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedNaturalLanguage.Thing, this.PopulateNaturalLanguage));

            this.InspectNaturalLanguageCommand = ReactiveCommand.Create(canExecuteInspectSelectedNaturalLanguageCommand);
            this.InspectNaturalLanguageCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedNaturalLanguage.Thing));
            
            var canExecuteCreateAnnotationCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedAnnotationCommand = this.WhenAny(vm => vm.SelectedAnnotation, v => v.Value != null);
            var canExecuteEditSelectedAnnotationCommand = this.WhenAny(vm => vm.SelectedAnnotation, v => v.Value != null && !this.IsReadOnly);

            this.CreateAnnotationCommand = ReactiveCommand.Create(canExecuteCreateAnnotationCommand);
            this.CreateAnnotationCommand.Subscribe(_ => this.ExecuteCreateCommand<SiteDirectoryDataAnnotation>(this.PopulateAnnotation));

            this.DeleteAnnotationCommand = ReactiveCommand.Create(canExecuteEditSelectedAnnotationCommand);
            this.DeleteAnnotationCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedAnnotation.Thing, this.PopulateAnnotation));

            this.EditAnnotationCommand = ReactiveCommand.Create(canExecuteEditSelectedAnnotationCommand);
            this.EditAnnotationCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedAnnotation.Thing, this.PopulateAnnotation));

            this.InspectAnnotationCommand = ReactiveCommand.Create(canExecuteInspectSelectedAnnotationCommand);
            this.InspectAnnotationCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedAnnotation.Thing));
            var canExecuteInspectSelectedDefaultParticipantRoleCommand = this.WhenAny(vm => vm.SelectedDefaultParticipantRole, v => v.Value != null);
            this.InspectSelectedDefaultParticipantRoleCommand = ReactiveCommand.Create(canExecuteInspectSelectedDefaultParticipantRoleCommand);
            this.InspectSelectedDefaultParticipantRoleCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedDefaultParticipantRole));
            var canExecuteInspectSelectedDefaultPersonRoleCommand = this.WhenAny(vm => vm.SelectedDefaultPersonRole, v => v.Value != null);
            this.InspectSelectedDefaultPersonRoleCommand = ReactiveCommand.Create(canExecuteInspectSelectedDefaultPersonRoleCommand);
            this.InspectSelectedDefaultPersonRoleCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedDefaultPersonRole));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.CreatedOn = this.CreatedOn;
            clone.Name = this.Name;
            clone.ShortName = this.ShortName;
            clone.DefaultParticipantRole = this.SelectedDefaultParticipantRole;
            clone.DefaultPersonRole = this.SelectedDefaultPersonRole;
        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PossibleDefaultParticipantRole = new ReactiveList<ParticipantRole>();
            this.PossibleDefaultPersonRole = new ReactiveList<PersonRole>();
            this.Organization = new ReactiveList<OrganizationRowViewModel>();
            this.Person = new ReactiveList<PersonRowViewModel>();
            this.ParticipantRole = new ReactiveList<ParticipantRoleRowViewModel>();
            this.SiteReferenceDataLibrary = new ReactiveList<SiteReferenceDataLibraryRowViewModel>();
            this.Model = new ReactiveList<EngineeringModelSetupRowViewModel>();
            this.PersonRole = new ReactiveList<PersonRoleRowViewModel>();
            this.LogEntry = new ReactiveList<SiteLogEntryRowViewModel>();
            this.DomainGroup = new ReactiveList<DomainOfExpertiseGroupRowViewModel>();
            this.Domain = new ReactiveList<DomainOfExpertiseRowViewModel>();
            this.NaturalLanguage = new ReactiveList<NaturalLanguageRowViewModel>();
            this.Annotation = new ReactiveList<SiteDirectoryDataAnnotationRowViewModel>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.CreatedOn = this.Thing.CreatedOn;
            this.Name = this.Thing.Name;
            this.ShortName = this.Thing.ShortName;
            this.SelectedDefaultParticipantRole = this.Thing.DefaultParticipantRole;
            this.PopulatePossibleDefaultParticipantRole();
            this.SelectedDefaultPersonRole = this.Thing.DefaultPersonRole;
            this.PopulatePossibleDefaultPersonRole();
            this.PopulateOrganization();
            this.PopulatePerson();
            this.PopulateParticipantRole();
            this.PopulateSiteReferenceDataLibrary();
            this.PopulateModel();
            this.PopulatePersonRole();
            this.PopulateLogEntry();
            this.PopulateDomainGroup();
            this.PopulateDomain();
            this.PopulateNaturalLanguage();
            this.PopulateAnnotation();
        }

        /// <summary>
        /// Populates the <see cref="Organization"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateOrganization()
        {
            this.Organization.Clear();
            foreach (var thing in this.Thing.Organization.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new OrganizationRowViewModel(thing, this.Session, this);
                this.Organization.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="Person"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulatePerson()
        {
            this.Person.Clear();
            foreach (var thing in this.Thing.Person.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new PersonRowViewModel(thing, this.Session, this);
                this.Person.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="ParticipantRole"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateParticipantRole()
        {
            this.ParticipantRole.Clear();
            foreach (var thing in this.Thing.ParticipantRole.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new ParticipantRoleRowViewModel(thing, this.Session, this);
                this.ParticipantRole.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="SiteReferenceDataLibrary"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateSiteReferenceDataLibrary()
        {
            this.SiteReferenceDataLibrary.Clear();
            foreach (var thing in this.Thing.SiteReferenceDataLibrary.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new SiteReferenceDataLibraryRowViewModel(thing, this.Session, this);
                this.SiteReferenceDataLibrary.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="Model"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateModel()
        {
            this.Model.Clear();
            foreach (var thing in this.Thing.Model.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new EngineeringModelSetupRowViewModel(thing, this.Session, this);
                this.Model.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="PersonRole"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulatePersonRole()
        {
            this.PersonRole.Clear();
            foreach (var thing in this.Thing.PersonRole.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new PersonRoleRowViewModel(thing, this.Session, this);
                this.PersonRole.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="LogEntry"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateLogEntry()
        {
            this.LogEntry.Clear();
            foreach (var thing in this.Thing.LogEntry.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new SiteLogEntryRowViewModel(thing, this.Session, this);
                this.LogEntry.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="DomainGroup"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateDomainGroup()
        {
            this.DomainGroup.Clear();
            foreach (var thing in this.Thing.DomainGroup.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new DomainOfExpertiseGroupRowViewModel(thing, this.Session, this);
                this.DomainGroup.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="Domain"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateDomain()
        {
            this.Domain.Clear();
            foreach (var thing in this.Thing.Domain.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new DomainOfExpertiseRowViewModel(thing, this.Session, this);
                this.Domain.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="NaturalLanguage"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateNaturalLanguage()
        {
            this.NaturalLanguage.Clear();
            foreach (var thing in this.Thing.NaturalLanguage.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new NaturalLanguageRowViewModel(thing, this.Session, this);
                this.NaturalLanguage.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="Annotation"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateAnnotation()
        {
            this.Annotation.Clear();
            foreach (var thing in this.Thing.Annotation.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new SiteDirectoryDataAnnotationRowViewModel(thing, this.Session, this);
                this.Annotation.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="PossibleDefaultParticipantRole"/> property
        /// </summary>
        protected virtual void PopulatePossibleDefaultParticipantRole()
        {
            this.PossibleDefaultParticipantRole.Clear();
        }

        /// <summary>
        /// Populates the <see cref="PossibleDefaultPersonRole"/> property
        /// </summary>
        protected virtual void PopulatePossibleDefaultPersonRole()
        {
            this.PossibleDefaultPersonRole.Clear();
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
            foreach(var organization in this.Organization)
            {
                organization.Dispose();
            }
            foreach(var person in this.Person)
            {
                person.Dispose();
            }
            foreach(var participantRole in this.ParticipantRole)
            {
                participantRole.Dispose();
            }
            foreach(var siteReferenceDataLibrary in this.SiteReferenceDataLibrary)
            {
                siteReferenceDataLibrary.Dispose();
            }
            foreach(var model in this.Model)
            {
                model.Dispose();
            }
            foreach(var personRole in this.PersonRole)
            {
                personRole.Dispose();
            }
            foreach(var logEntry in this.LogEntry)
            {
                logEntry.Dispose();
            }
            foreach(var domainGroup in this.DomainGroup)
            {
                domainGroup.Dispose();
            }
            foreach(var domain in this.Domain)
            {
                domain.Dispose();
            }
            foreach(var naturalLanguage in this.NaturalLanguage)
            {
                naturalLanguage.Dispose();
            }
            foreach(var annotation in this.Annotation)
            {
                annotation.Dispose();
            }
        }
    }
}
