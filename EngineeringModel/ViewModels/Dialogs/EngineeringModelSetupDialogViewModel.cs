// -------------------------------------------------------------------------------------------------
// <copyright file="EngineeringModelSetupDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using CDP4Common.CommonData;
    using CDP4Dal.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="EngineeringModelSetupDialogViewModel"/> is to allow an <see cref="EngineeringModelSetup"/> to
    /// be created or updated.
    /// </summary>
    /// <remarks>
    /// The creation of an <see cref="EngineeringModelSetup"/> will result in an <see cref="EngineeringModel"/> being created by 
    /// the connected data-source
    /// </remarks>
    [ThingDialogViewModelExport(ClassKind.EngineeringModelSetup)]
    public class EngineeringModelSetupDialogViewModel : CDP4CommonView.EngineeringModelSetupDialogViewModel, IThingDialogViewModel
    {
        #region field
        /// <summary>
        /// Backing field for the <see cref="SourceEngineeringModelSetup"/> property;
        /// </summary>
        private EngineeringModelSetup sourceEngineeringModelSetup;

        /// <summary>
        /// Backing field for the <see cref="ReferenceDataLibrary"/> property;
        /// </summary>
        private SiteReferenceDataLibrary selectedSiteReferenceDataLibrary;

        /// <summary>
        /// Backing field for the <see cref="isOriginal"/> property.
        /// </summary>
        private bool isOriginal;

        /// <summary>
        /// The backing field for <see cref="ShortName"/>
        /// </summary>
        private string shortName;

        /// <summary>
        /// The backing field for <see cref="Name"/>
        /// </summary>
        private string name;

        /// <summary>
        /// Backing field for <see cref="SelectedOrganizations"/>
        /// </summary>
        private ReactiveList<Organization> selectedOrganizations;

        /// <summary>
        /// Backing field for <see cref="SelectedDefaultOrganization"/>
        /// </summary>
        private Organization selectedDefaultOrganization;

        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="EngineeringModelSetupDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public EngineeringModelSetupDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EngineeringModelSetupDialogViewModel"/> class.
        /// </summary>
        /// <param name="engineeringModelSetup">
        /// The <see cref="engineeringModelSetup"/> that is the subject of the current view-model. This is the object
        /// that will be either created, or edited.
        /// </param>
        /// <param name="transaction">
        /// The <see cref="ThingTransaction"/> that contains the log of recorded changes.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the current <see cref="Thing"/> is to be added or updated
        /// </param>
        /// <param name="isRoot">
        /// Assert if the <see cref="EngineeringModelSetupDialogViewModel"/> is the root of all <see cref="IThingDialogViewModel"/>
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation this <see cref="EngineeringModelSetupDialogViewModel"/> performs
        /// </param>
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService"/></param>
        /// <param name="container">The container <see cref="Thing"/></param>
        /// <param name="chainOfContainers">The optional chain of containers that contains the <paramref name="container"/> argument</param>
        public EngineeringModelSetupDialogViewModel(EngineeringModelSetup engineeringModelSetup, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers = null)
            : base(engineeringModelSetup, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            this.ActiveDomain.ChangeTrackingEnabled = true;

            this.WhenAnyValue(x => x.SourceEngineeringModelSetup).Subscribe(v => this.IsOriginal = v == null || v.Iid == default(Guid));
            this.WhenAnyValue(x => x.ActiveDomain).Subscribe(_ => this.UpdateOkCanExecute());
            this.WhenAnyValue(x => x.SelectedOrganizations).Subscribe(_ => this.UpdateOkCanExecute());
            this.SelectedOrganizations.Changed.Subscribe(_ => this.UpdateDefaultOrganization());
            this.WhenAnyValue(x => x.SelectedDefaultOrganization).Subscribe(_ => this.UpdateOkCanExecute());
        }

        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the  the <see cref="EngineeringModelSetup"/> that this <see cref="EngineeringModelSetup"/>
        /// has been derived from.
        /// </summary>
        public EngineeringModelSetup SourceEngineeringModelSetup
        {
            get { return this.sourceEngineeringModelSetup; }
            set { this.RaiseAndSetIfChanged(ref this.sourceEngineeringModelSetup, value); }
        }

        /// <summary>
        /// Gets the list of possible source <see cref="EngineeringModelSetup"/>s
        /// </summary>
        public List<EngineeringModelSetup> PossibleSourceEngineeringModelSetup { get; private set; }

        /// <summary>
        /// Gets the possible <see cref="SiteReferenceDataLibrary"/> that may be selected.
        /// </summary>
        public List<SiteReferenceDataLibrary> PossibleSiteReferenceDataLibraries { get; private set; }

        /// <summary>
        /// Gets the possible <see cref="Organization"/> that may be selected.
        /// </summary>
        public List<Organization> PossibleOrganizations { get; private set; }

        /// <summary>
        /// Gets or sets the selected <see cref="Organization"/>s.
        /// </summary>
        public ReactiveList<Organization> SelectedOrganizations
        {
            get { return this.selectedOrganizations; }
            set { this.RaiseAndSetIfChanged(ref this.selectedOrganizations, value); }
        }

        /// <summary>
        /// Gets or sets the selected default <see cref="Organization"/>.
        /// </summary>
        public Organization SelectedDefaultOrganization
        {
            get { return this.selectedDefaultOrganization; }
            set { this.RaiseAndSetIfChanged(ref this.selectedDefaultOrganization, value); }
        }

        /// <summary>
        /// Gets or sets the selected <see cref="SiteReferenceDataLibrary"/>
        /// </summary>
        public SiteReferenceDataLibrary SelectedSiteReferenceDataLibrary
        {
            get { return this.selectedSiteReferenceDataLibrary; }
            set { this.RaiseAndSetIfChanged(ref this.selectedSiteReferenceDataLibrary, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="EngineeringModelSetup"/> is derived.
        /// </summary>
        public bool IsOriginal
        {
            get { return this.isOriginal; }
            set { this.RaiseAndSetIfChanged(ref this.isOriginal, value); }
        }

        /// <summary>
        /// Gets a value indicating whether the Active Domain of Expertiese selection is possible
        /// </summary>
        /// <remarks>
        /// TODO: In the future this should be refactored to allow CDP4 server creating active domains on Engineering model setup creation.
        /// </remarks>
        public bool IsActiveDomainSelectionReadOnly
        {
            get { return (this.dialogKind != ThingDialogKind.Update) || this.IsReadOnly; }
        }

        /// <summary>
        /// Gets or sets the ShortName
        /// </summary>
        [ValidationOverride(true, "ModelSetupShortName")]
        public override string ShortName
        {
            get { return this.shortName; }
            set { this.RaiseAndSetIfChanged(ref this.shortName, value); }
        }

        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        [ValidationOverride(true, "ModelSetupName")]
        public override string Name
        {
            get { return this.name; }
            set { this.RaiseAndSetIfChanged(ref this.name, value); }
        }
        #endregion

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PossibleSiteReferenceDataLibraries = new List<SiteReferenceDataLibrary>();
            this.PossibleSourceEngineeringModelSetup = new List<EngineeringModelSetup>();
            this.PossibleOrganizations = new List<Organization>();

            this.SelectedOrganizations = new ReactiveList<Organization>();
            this.SelectedOrganizations.ChangeTrackingEnabled = true;
        }

        /// <summary>
        /// Populate the <see cref="PossibleActiveDomain"/>
        /// </summary>
        protected override void PopulateActiveDomain()
        {
            var sitedir = (SiteDirectory)this.Container;

            var currentActiveDomain = this.Thing.ActiveDomain;
            this.PossibleActiveDomain.Clear();
            this.PossibleActiveDomain.AddRange(sitedir.Domain.OrderBy(x => x.Name));

            this.ActiveDomain = new ReactiveList<DomainOfExpertise>(currentActiveDomain);
        }

        /// <summary>
        /// Update the properties of the current view-model
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            var siteDirectory = this.Session.RetrieveSiteDirectory();

            this.PossibleSourceEngineeringModelSetup.AddRange(siteDirectory.Model);
            this.PossibleOrganizations.AddRange(siteDirectory.Organization);

            this.SourceEngineeringModelSetup = siteDirectory.Model.SingleOrDefault(x => x.Iid == this.Thing.SourceEngineeringModelSetupIid);
            this.SourceEngineeringModelSetup = this.PossibleSourceEngineeringModelSetup.SingleOrDefault(x => x.Iid.Equals(this.Thing.SourceEngineeringModelSetupIid));

            // populates the possibleSiteReferenceDataLibrary
            this.PossibleSiteReferenceDataLibraries.AddRange(siteDirectory.SiteReferenceDataLibrary);

            if (this.PossibleSiteReferenceDataLibraries.Any())
            {
                this.SelectedSiteReferenceDataLibrary = this.PossibleSiteReferenceDataLibraries.First();
            }

            // set the site reference data library if this model already has a mrdl assigned
            var modelRdl = this.Thing.RequiredRdl.FirstOrDefault();
            if (modelRdl != null)
            {
                this.SelectedSiteReferenceDataLibrary = modelRdl.RequiredRdl;
            }

            // get organizational participation data
            var organizationalParticipationOrganizations = this.Thing.OrganizationalParticipant?.Select(op => op.Organization);
            this.SelectedOrganizations.Clear();
            this.SelectedOrganizations.AddRange(organizationalParticipationOrganizations);

            var thingDefaultOrganizationalParticipant = this.Thing.DefaultOrganizationalParticipant;

            if (thingDefaultOrganizationalParticipant != null)
            {
                this.SelectedDefaultOrganization = this.SelectedOrganizations.Contains(thingDefaultOrganizationalParticipant.Organization) ? thingDefaultOrganizationalParticipant.Organization : null;
            }
        }

        /// <summary>
        /// Updates the <see cref="Transaction"/> with the changes recorded in the current view-model
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();

            // organizational prticipation
            if (this.SelectedOrganizations.Any() || this.Thing.OrganizationalParticipant.Any())
            {
                var existingOrganizations = this.Thing.OrganizationalParticipant.Select(op => op.Organization).ToList();

                var newOrgs = this.SelectedOrganizations.Except(existingOrganizations);
                var deletedOrgs = existingOrganizations.Except(this.SelectedOrganizations);

                foreach (var organization in newOrgs)
                {
                    var orgParticipation = new OrganizationalParticipant
                    {
                        Organization = organization
                    };

                    this.Thing.OrganizationalParticipant.Add(orgParticipation);
                    this.transaction.Create(orgParticipation);
                }

                foreach (var organization in deletedOrgs)
                {
                    var participantion = this.Thing.OrganizationalParticipant.FirstOrDefault(p => p.Organization.Equals(organization));

                    if (participantion != null)
                    {
                        this.transaction.Delete(participantion, this.Thing);
                    }
                }
            }

            this.Thing.DefaultOrganizationalParticipant = this.Thing.OrganizationalParticipant.FirstOrDefault(p => p.Organization.Equals(this.SelectedDefaultOrganization));

            if (this.dialogKind.Equals(ThingDialogKind.Update))
            {
                return;
            }

            // set the source SourceEngineeringModelSetup if the model is derived, otherwise set the Required RDL
            if (this.SourceEngineeringModelSetup == null)
            {
                this.Thing.SourceEngineeringModelSetupIid = null;
                this.Thing.RequiredRdl.Clear();

                // this is a non-derived EMS thus a new engineering model rdl has to be passed in as well
                var mrdl = new ModelReferenceDataLibrary();
                mrdl.Name = string.Concat(this.Name, " Model RDL");
                mrdl.ShortName = string.Concat(this.ShortName, "MRDL");
                mrdl.RequiredRdl = this.SelectedSiteReferenceDataLibrary;
                this.Thing.RequiredRdl.Add(mrdl);
                this.transaction.Create(mrdl);
            }
            else
            {
                if (this.SourceEngineeringModelSetup != null)
                {
                    this.Thing.SourceEngineeringModelSetupIid = this.SourceEngineeringModelSetup.Iid;
                }
            }

            this.Thing.EngineeringModelIid = Guid.NewGuid();
        }

        /// <summary>
        /// Update the value that enables the Ok-button
        /// </summary>
        protected override void UpdateOkCanExecute()
        {
            base.UpdateOkCanExecute();
            this.OkCanExecute = this.OkCanExecute && this.ActiveDomain.Count >= 0 && this.IsOrganizationalParticipationSetup();
        }

        /// <summary>
        /// Checks whether organizational participation is correctly setup
        /// </summary>
        /// <returns>True if the selection is done correctly.</returns>
        private bool IsOrganizationalParticipationSetup()
        {
            if (this.SelectedOrganizations == null || this.SelectedOrganizations.Count < 1)
            {
                return true;
            }

            if (this.SelectedDefaultOrganization != null && this.SelectedOrganizations.Contains(this.SelectedDefaultOrganization))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Updates the default <see cref="Organization"/> selection based on changes in the list of possible organizations.
        /// </summary>
        private void UpdateDefaultOrganization()
        {
            if (this.SelectedOrganizations == null || this.SelectedOrganizations.Count == 0 || !this.SelectedOrganizations.Contains(this.SelectedDefaultOrganization))
            {
                this.SelectedDefaultOrganization = null;
            }

            this.UpdateOkCanExecute();
        }
    }
}
