// -------------------------------------------------------------------------------------------------
// <copyright file="SiteDirectoryRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2017 RHEA System S.A.
// </copyright>
// <summary>
//   This is an auto-generated class. Any manual changes on this file will be overwritten!
// </summary>
// -------------------------------------------------------------------------------------------------

namespace CDP4ObjectBrowser
{
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.DiagramData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Common.ReportingData;
    using System;
    using System.Reactive.Linq;

    /// <summary>
    /// Row class representing a <see cref="SiteDirectory"/>
    /// </summary>
    public partial class SiteDirectoryRowViewModel : TopContainerRowViewModel<SiteDirectory>
    {
        /// <summary>
        /// Intermediate folder containing <see cref="OrganizationRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel organizationFolder;

        /// <summary>
        /// Intermediate folder containing <see cref="PersonRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel personFolder;

        /// <summary>
        /// Intermediate folder containing <see cref="ParticipantRoleRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel participantRoleFolder;

        /// <summary>
        /// Intermediate folder containing <see cref="SiteReferenceDataLibraryRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel siteReferenceDataLibraryFolder;

        /// <summary>
        /// Intermediate folder containing <see cref="ModelRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel modelFolder;

        /// <summary>
        /// Intermediate folder containing <see cref="PersonRoleRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel personRoleFolder;

        /// <summary>
        /// Intermediate folder containing <see cref="LogEntryRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel logEntryFolder;

        /// <summary>
        /// Intermediate folder containing <see cref="DomainGroupRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel domainGroupFolder;

        /// <summary>
        /// Intermediate folder containing <see cref="DomainRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel domainFolder;

        /// <summary>
        /// Intermediate folder containing <see cref="NaturalLanguageRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel naturalLanguageFolder;

        /// <summary>
        /// Intermediate folder containing <see cref="AnnotationRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel annotationFolder;

        /// <summary>
        /// Initializes a new instance of the <see cref="SiteDirectoryRowViewModel"/> class
        /// </summary>
        /// <param name="siteDirectory">The <see cref="SiteDirectory"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase"/> that is the container of this <see cref="IRowViewModelBase"/></param>
        public SiteDirectoryRowViewModel(SiteDirectory siteDirectory, ISession session, IViewModelBase<Thing> containerViewModel) : base(siteDirectory, session, containerViewModel)
        {
            this.organizationFolder = new CDP4Composition.FolderRowViewModel("Organization", "Organization", this.Session, this);
            this.ContainedRows.Add(this.organizationFolder);
            this.personFolder = new CDP4Composition.FolderRowViewModel("Person", "Person", this.Session, this);
            this.ContainedRows.Add(this.personFolder);
            this.participantRoleFolder = new CDP4Composition.FolderRowViewModel("Participant Role", "Participant Role", this.Session, this);
            this.ContainedRows.Add(this.participantRoleFolder);
            this.siteReferenceDataLibraryFolder = new CDP4Composition.FolderRowViewModel("Site Reference Data Library", "Site Reference Data Library", this.Session, this);
            this.ContainedRows.Add(this.siteReferenceDataLibraryFolder);
            this.modelFolder = new CDP4Composition.FolderRowViewModel("Model", "Model", this.Session, this);
            this.ContainedRows.Add(this.modelFolder);
            this.personRoleFolder = new CDP4Composition.FolderRowViewModel("Person Role", "Person Role", this.Session, this);
            this.ContainedRows.Add(this.personRoleFolder);
            this.logEntryFolder = new CDP4Composition.FolderRowViewModel("Log Entry", "Log Entry", this.Session, this);
            this.ContainedRows.Add(this.logEntryFolder);
            this.domainGroupFolder = new CDP4Composition.FolderRowViewModel("Domain Group", "Domain Group", this.Session, this);
            this.ContainedRows.Add(this.domainGroupFolder);
            this.domainFolder = new CDP4Composition.FolderRowViewModel("Domain", "Domain", this.Session, this);
            this.ContainedRows.Add(this.domainFolder);
            this.naturalLanguageFolder = new CDP4Composition.FolderRowViewModel("Natural Language", "Natural Language", this.Session, this);
            this.ContainedRows.Add(this.naturalLanguageFolder);
            this.annotationFolder = new CDP4Composition.FolderRowViewModel("Annotation", "Annotation", this.Session, this);
            this.ContainedRows.Add(this.annotationFolder);
            this.UpdateProperties();
            this.UpdateColumnValues();
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
        /// Updates all the properties rows
        /// /// </summary>
        private void UpdateProperties()
        {
            this.ComputeRows(this.Thing.Organization, this.organizationFolder, this.AddOrganizationRowViewModel);
            this.ComputeRows(this.Thing.Person, this.personFolder, this.AddPersonRowViewModel);
            this.ComputeRows(this.Thing.ParticipantRole, this.participantRoleFolder, this.AddParticipantRoleRowViewModel);
            this.ComputeRows(this.Thing.SiteReferenceDataLibrary, this.siteReferenceDataLibraryFolder, this.AddSiteReferenceDataLibraryRowViewModel);
            this.ComputeRows(this.Thing.Model, this.modelFolder, this.AddModelRowViewModel);
            this.ComputeRows(this.Thing.PersonRole, this.personRoleFolder, this.AddPersonRoleRowViewModel);
            this.ComputeRows(this.Thing.LogEntry, this.logEntryFolder, this.AddLogEntryRowViewModel);
            this.ComputeRows(this.Thing.DomainGroup, this.domainGroupFolder, this.AddDomainGroupRowViewModel);
            this.ComputeRows(this.Thing.Domain, this.domainFolder, this.AddDomainRowViewModel);
            this.ComputeRows(this.Thing.NaturalLanguage, this.naturalLanguageFolder, this.AddNaturalLanguageRowViewModel);
            this.ComputeRows(this.Thing.Annotation, this.annotationFolder, this.AddAnnotationRowViewModel);
        }
        /// <summary>
        /// Add an Organization row view model to the list of <see cref="Organization"/>
        /// </summary>
        /// <param name="organization">
        /// The <see cref="Organization"/> that is to be added
        /// </param>
        private OrganizationRowViewModel AddOrganizationRowViewModel(Organization organization)
        {
            return new OrganizationRowViewModel(organization, this.Session, this);
        }
        /// <summary>
        /// Add an Person row view model to the list of <see cref="Person"/>
        /// </summary>
        /// <param name="person">
        /// The <see cref="Person"/> that is to be added
        /// </param>
        private PersonRowViewModel AddPersonRowViewModel(Person person)
        {
            return new PersonRowViewModel(person, this.Session, this);
        }
        /// <summary>
        /// Add an Participant Role row view model to the list of <see cref="ParticipantRole"/>
        /// </summary>
        /// <param name="participantRole">
        /// The <see cref="ParticipantRole"/> that is to be added
        /// </param>
        private ParticipantRoleRowViewModel AddParticipantRoleRowViewModel(ParticipantRole participantRole)
        {
            return new ParticipantRoleRowViewModel(participantRole, this.Session, this);
        }
        /// <summary>
        /// Add an Site Reference Data Library row view model to the list of <see cref="SiteReferenceDataLibrary"/>
        /// </summary>
        /// <param name="siteReferenceDataLibrary">
        /// The <see cref="SiteReferenceDataLibrary"/> that is to be added
        /// </param>
        private SiteReferenceDataLibraryRowViewModel AddSiteReferenceDataLibraryRowViewModel(SiteReferenceDataLibrary siteReferenceDataLibrary)
        {
            return new SiteReferenceDataLibraryRowViewModel(siteReferenceDataLibrary, this.Session, this);
        }
        /// <summary>
        /// Add an Model row view model to the list of <see cref="Model"/>
        /// </summary>
        /// <param name="model">
        /// The <see cref="Model"/> that is to be added
        /// </param>
        private EngineeringModelSetupRowViewModel AddModelRowViewModel(EngineeringModelSetup model)
        {
            return new EngineeringModelSetupRowViewModel(model, this.Session, this);
        }
        /// <summary>
        /// Add an Person Role row view model to the list of <see cref="PersonRole"/>
        /// </summary>
        /// <param name="personRole">
        /// The <see cref="PersonRole"/> that is to be added
        /// </param>
        private PersonRoleRowViewModel AddPersonRoleRowViewModel(PersonRole personRole)
        {
            return new PersonRoleRowViewModel(personRole, this.Session, this);
        }
        /// <summary>
        /// Add an Log Entry row view model to the list of <see cref="LogEntry"/>
        /// </summary>
        /// <param name="logEntry">
        /// The <see cref="LogEntry"/> that is to be added
        /// </param>
        private SiteLogEntryRowViewModel AddLogEntryRowViewModel(SiteLogEntry logEntry)
        {
            return new SiteLogEntryRowViewModel(logEntry, this.Session, this);
        }
        /// <summary>
        /// Add an Domain Group row view model to the list of <see cref="DomainGroup"/>
        /// </summary>
        /// <param name="domainGroup">
        /// The <see cref="DomainGroup"/> that is to be added
        /// </param>
        private DomainOfExpertiseGroupRowViewModel AddDomainGroupRowViewModel(DomainOfExpertiseGroup domainGroup)
        {
            return new DomainOfExpertiseGroupRowViewModel(domainGroup, this.Session, this);
        }
        /// <summary>
        /// Add an Domain row view model to the list of <see cref="Domain"/>
        /// </summary>
        /// <param name="domain">
        /// The <see cref="Domain"/> that is to be added
        /// </param>
        private DomainOfExpertiseRowViewModel AddDomainRowViewModel(DomainOfExpertise domain)
        {
            return new DomainOfExpertiseRowViewModel(domain, this.Session, this);
        }
        /// <summary>
        /// Add an Natural Language row view model to the list of <see cref="NaturalLanguage"/>
        /// </summary>
        /// <param name="naturalLanguage">
        /// The <see cref="NaturalLanguage"/> that is to be added
        /// </param>
        private NaturalLanguageRowViewModel AddNaturalLanguageRowViewModel(NaturalLanguage naturalLanguage)
        {
            return new NaturalLanguageRowViewModel(naturalLanguage, this.Session, this);
        }
        /// <summary>
        /// Add an Annotation row view model to the list of <see cref="Annotation"/>
        /// </summary>
        /// <param name="annotation">
        /// The <see cref="Annotation"/> that is to be added
        /// </param>
        private SiteDirectoryDataAnnotationRowViewModel AddAnnotationRowViewModel(SiteDirectoryDataAnnotation annotation)
        {
            return new SiteDirectoryDataAnnotationRowViewModel(annotation, this.Session, this);
        }
    }
}
