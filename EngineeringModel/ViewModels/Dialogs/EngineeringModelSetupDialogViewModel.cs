// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EngineeringModelSetupDialogViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
// 
//    This file is part of CDP4-COMET-IME Community Edition.
//    The CDP4-COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
// 
//    The CDP4-COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
// 
//    The CDP4-COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Operations;



    using CDP4Composition.Attributes;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using ReactiveUI;

    using ActiveDomainRowViewModel = CDP4Composition.CommonView.ViewModels.ActiveDomainRowViewModel;

    /// <summary>
    /// The purpose of the <see cref="EngineeringModelSetupDialogViewModel" /> is to allow an
    /// <see cref="EngineeringModelSetup" /> to
    /// be created or updated.
    /// </summary>
    /// <remarks>
    /// The creation of an <see cref="EngineeringModelSetup" /> will result in an <see cref="EngineeringModel" /> being created
    /// by
    /// the connected data-source
    /// </remarks>
    [ThingDialogViewModelExport(ClassKind.EngineeringModelSetup)]
    public class EngineeringModelSetupDialogViewModel : CDP4CommonView.EngineeringModelSetupDialogViewModel, IThingDialogViewModel
    {
        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PossibleSiteReferenceDataLibraries = new List<SiteReferenceDataLibrary>();
            this.PossibleSourceEngineeringModelSetup = new List<EngineeringModelSetup>();
            this.PossibleOrganizations = new List<Organization>();

            this.PossibleActiveDomain = new ReactiveList<ActiveDomainRowViewModel>();
            this.ActiveDomain = new ReactiveList<ActiveDomainRowViewModel>();

            this.ActiveDomain.ItemsAdded.Subscribe(this.SetActiveState);
            this.ActiveDomain.ItemsRemoved.Subscribe(this.SetInactiveState);

            this.SelectedOrganizations = new ReactiveList<Organization>();

            this.WhenAnyValue(vm => vm.ShowDeprecatedDomains).Subscribe(_ => this.ShowHideDeprecatedDomains());
        }

        /// <summary>
        /// Set active domain inactive state
        /// </summary>
        /// <param name="oldRow">
        ///     <see cref="ActiveDomainRowViewModel" />
        /// </param>
        private void SetInactiveState(ActiveDomainRowViewModel oldRow)
        {
            oldRow.IsEnabled = false;
        }

        /// <summary>
        /// Set active domain active state
        /// </summary>
        /// <param name="newRow">
        ///     <see cref="ActiveDomainRowViewModel" />
        /// </param>
        private void SetActiveState(ActiveDomainRowViewModel newRow)
        {
            newRow.IsEnabled = true;
        }

        /// <summary>
        /// Show/hide deprecated domains from the view
        /// </summary>
        private void ShowHideDeprecatedDomains()
        {
            var deprecatedItems = this.PossibleActiveDomain.Where(d => d.DomainOfExpertise.IsDeprecated);

            foreach (var activeDomainRowViewModel in deprecatedItems)
            {
                activeDomainRowViewModel.IsVisible = this.ShowDeprecatedDomains;
            }
        }

        /// <summary>
        /// Populate the <see cref="PossibleActiveDomain" />
        /// </summary>
        protected override void PopulateActiveDomain()
        {
            var sitedir = (SiteDirectory)this.Container;

            var currentActiveDomain = this.Thing.ActiveDomain;
            this.PossibleActiveDomain.Clear();

            foreach (var domainOfExpertise in sitedir.Domain.OrderBy(x => x.Name))
            {
                var activeDomainRowModel = new ActiveDomainRowViewModel(domainOfExpertise, !this.ShowDeprecatedDomains && !domainOfExpertise.IsDeprecated);
                this.PossibleActiveDomain.Add(activeDomainRowModel);
            }

            foreach (var activeDomain in currentActiveDomain)
            {
                var activeDomainRowModel = this.PossibleActiveDomain.FirstOrDefault(d => d.DomainOfExpertise == activeDomain);

                if (activeDomainRowModel != null)
                {
                    this.ActiveDomain.Add(activeDomainRowModel);
                    activeDomainRowModel.IsEnabled = true;
                }
            }
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
        /// Updates the <see cref="Transaction" /> with the changes recorded in the current view-model
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();

            var clone = this.Thing;

            clone.ActiveDomain.Clear();
            clone.ActiveDomain.AddRange(this.ActiveDomain.Select(ad => ad.DomainOfExpertise));

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
                    var participantion = this.Thing.OrganizationalParticipant.FirstOrDefault(p => p.Organization.Equals(organization))?.Clone(false);

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
            this.OkCanExecute = this.OkCanExecute && (this.ActiveDomain.Count >= 0) && this.IsOrganizationalParticipationSetup();
        }

        /// <summary>
        /// Checks whether organizational participation is correctly setup
        /// </summary>
        /// <returns>True if the selection is done correctly.</returns>
        private bool IsOrganizationalParticipationSetup()
        {
            if ((this.SelectedOrganizations == null) || (this.SelectedOrganizations.Count < 1))
            {
                return true;
            }

            if ((this.SelectedDefaultOrganization != null) && this.SelectedOrganizations.Contains(this.SelectedDefaultOrganization))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Updates the default <see cref="Organization" /> selection based on changes in the list of possible organizations.
        /// </summary>
        private void UpdateDefaultOrganization()
        {
            if ((this.SelectedOrganizations == null) || (this.SelectedOrganizations.Count == 0) || !this.SelectedOrganizations.Contains(this.SelectedDefaultOrganization))
            {
                this.SelectedDefaultOrganization = null;
            }

            this.UpdateOkCanExecute();
        }

        /// <summary>
        /// Backing field for the <see cref="SourceEngineeringModelSetup" /> property;
        /// </summary>
        private EngineeringModelSetup sourceEngineeringModelSetup;

        /// <summary>
        /// Backing field for the <see cref="ReferenceDataLibrary" /> property;
        /// </summary>
        private SiteReferenceDataLibrary selectedSiteReferenceDataLibrary;

        /// <summary>
        /// Backing field for the <see cref="isOriginal" /> property.
        /// </summary>
        private bool isOriginal;

        /// <summary>
        /// The backing field for <see cref="ShortName" />
        /// </summary>
        private string shortName;

        /// <summary>
        /// The backing field for <see cref="Name" />
        /// </summary>
        private string name;

        /// <summary>
        /// Backing field for <see cref="SelectedOrganizations" />
        /// </summary>
        private ReactiveList<Organization> selectedOrganizations;

        /// <summary>
        /// Backing field for <see cref="SelectedDefaultOrganization" />
        /// </summary>
        private Organization selectedDefaultOrganization;

        /// <summary>
        /// Initializes a new instance of the <see cref="EngineeringModelSetupDialogViewModel" /> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public EngineeringModelSetupDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EngineeringModelSetupDialogViewModel" /> class.
        /// </summary>
        /// <param name="engineeringModelSetup">
        /// The <see cref="engineeringModelSetup" /> that is the subject of the current view-model. This is the object
        /// that will be either created, or edited.
        /// </param>
        /// <param name="transaction">
        /// The <see cref="ThingTransaction" /> that contains the log of recorded changes.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession" /> in which the current <see cref="Thing" /> is to be added or updated
        /// </param>
        /// <param name="isRoot">
        /// Assert if the <see cref="EngineeringModelSetupDialogViewModel" /> is the root of all
        /// <see cref="IThingDialogViewModel" />
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation this <see cref="EngineeringModelSetupDialogViewModel" /> performs
        /// </param>
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService" /></param>
        /// <param name="container">The container <see cref="Thing" /></param>
        /// <param name="chainOfContainers">
        /// The optional chain of containers that contains the <paramref name="container" />
        /// argument
        /// </param>
        public EngineeringModelSetupDialogViewModel(EngineeringModelSetup engineeringModelSetup, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers = null)
            : base(engineeringModelSetup, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            this.WhenAnyValue(x => x.SourceEngineeringModelSetup).Subscribe(v => this.IsOriginal = (v == null) || (v.Iid == default));

            this.WhenAnyValue(
                    x => x.ActiveDomain, 
                    x => x.SelectedOrganizations,
                    x => x.SelectedDefaultOrganization)
                .Subscribe(_ => this.UpdateOkCanExecute());
            
            this.SelectedOrganizations.Changed.Subscribe(_ => this.UpdateDefaultOrganization());
        }

        /// <summary>
        /// Gets or sets the  the <see cref="EngineeringModelSetup" /> that this <see cref="EngineeringModelSetup" />
        /// has been derived from.
        /// </summary>
        public EngineeringModelSetup SourceEngineeringModelSetup
        {
            get => this.sourceEngineeringModelSetup;
            set => this.RaiseAndSetIfChanged(ref this.sourceEngineeringModelSetup, value);
        }

        /// <summary>
        /// Gets the list of possible source <see cref="EngineeringModelSetup" />s
        /// </summary>
        public List<EngineeringModelSetup> PossibleSourceEngineeringModelSetup { get; private set; }

        /// <summary>
        /// Gets the possible <see cref="SiteReferenceDataLibrary" /> that may be selected.
        /// </summary>
        public List<SiteReferenceDataLibrary> PossibleSiteReferenceDataLibraries { get; private set; }

        /// <summary>
        /// Gets the possible <see cref="Organization" /> that may be selected.
        /// </summary>
        public List<Organization> PossibleOrganizations { get; private set; }

        /// <summary>
        /// Gets or sets the selected <see cref="Organization" />s.
        /// </summary>
        public ReactiveList<Organization> SelectedOrganizations
        {
            get => this.selectedOrganizations;
            set => this.RaiseAndSetIfChanged(ref this.selectedOrganizations, value);
        }

        /// <summary>
        /// Gets or sets the selected default <see cref="Organization" />.
        /// </summary>
        public Organization SelectedDefaultOrganization
        {
            get => this.selectedDefaultOrganization;
            set => this.RaiseAndSetIfChanged(ref this.selectedDefaultOrganization, value);
        }

        /// <summary>
        /// Gets or sets the selected <see cref="SiteReferenceDataLibrary" />
        /// </summary>
        public SiteReferenceDataLibrary SelectedSiteReferenceDataLibrary
        {
            get => this.selectedSiteReferenceDataLibrary;
            set => this.RaiseAndSetIfChanged(ref this.selectedSiteReferenceDataLibrary, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="EngineeringModelSetup" /> is derived.
        /// </summary>
        public bool IsOriginal
        {
            get => this.isOriginal;
            set => this.RaiseAndSetIfChanged(ref this.isOriginal, value);
        }

        /// <summary>
        /// Gets or sets the value of possible active domain.
        /// </summary>
        public new ReactiveList<ActiveDomainRowViewModel> PossibleActiveDomain { get; set; }

        /// <summary>
        /// Gets or sets the value of active domain.
        /// </summary>
        public new ReactiveList<ActiveDomainRowViewModel> ActiveDomain { get; set; }

        /// <summary>
        /// Gets or sets the ShortName
        /// </summary>
        [ValidationOverride(true, "ModelSetupShortName")]
        public override string ShortName
        {
            get => this.shortName;
            set => this.RaiseAndSetIfChanged(ref this.shortName, value);
        }

        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        [ValidationOverride(true, "ModelSetupName")]
        public override string Name
        {
            get => this.name;
            set => this.RaiseAndSetIfChanged(ref this.name, value);
        }

        /// <summary>
        /// The backing field for <see cref="ShowDeprecatedDomains" /> property.
        /// </summary>
        private bool showDeprecatedDomains;

        /// <summary>
        /// Gets or sets a value indicating whether to display deprecated domains or not.
        /// </summary>
        public bool ShowDeprecatedDomains
        {
            get => this.showDeprecatedDomains;
            set => this.RaiseAndSetIfChanged(ref this.showDeprecatedDomains, value);
        }
    }
}
