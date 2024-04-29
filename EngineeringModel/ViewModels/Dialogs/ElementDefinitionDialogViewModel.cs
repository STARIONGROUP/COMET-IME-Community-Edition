// -------------------------------------------------------------------------------------------------
// <copyright file="ElementDefinitionDialogViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2022 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    
    using CDP4Dal.Operations;
    
    using CDP4Composition.Attributes;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    
    using ReactiveUI;

    /// <summary>
    /// The dialog-view model to create, edit or inspect a <see cref="Option"/>
    /// </summary>
    [ThingDialogViewModelExport(ClassKind.ElementDefinition)]
    public class ElementDefinitionDialogViewModel : CDP4CommonView.ElementDefinitionDialogViewModel, IThingDialogViewModel
    {
        /// <summary>
        /// Backing field for the <see cref="IsTopElement"/> property.
        /// </summary>
        private bool isTopElement;

        /// <summary>
        /// Backing field for the <see cref="ModelCode"/> property.
        /// </summary>
        private string modelCode;

        /// <summary>
        /// Backing field for <see cref="SelectedOrganizations"/>
        /// </summary>
        private ReactiveList<Organization> selectedOrganizations;

        /// <summary>
        /// Backing field for see <see cref="AreOrganizationsVisible"/>
        /// </summary>
        private bool areOrganizationsVisible;

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementDefinitionDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public ElementDefinitionDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementDefinitionDialogViewModel"/> class
        /// </summary>
        /// <param name="elementDefinition">
        /// The <see cref="ElementDefinition"/> that is the subject of the current view-model. This is the object
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
        /// <param name="container">The Container <see cref="Thing"/> of the created <see cref="MultiRelationshipRule"/></param>
        /// <param name="chainOfContainers">
        /// The optional chain of containers that contains the <paramref name="container"/> argument
        /// </param>
        public ElementDefinitionDialogViewModel(ElementDefinition elementDefinition, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
            : base(elementDefinition, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            this.WhenAnyValue(x => x.SelectedOwner).Subscribe(x => this.UpdateOkCanExecute());
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ElementDefinition"/> is the top <see cref="ElementDefinition"/>
        /// of the container <see cref="Iteration"/>
        /// </summary>
        public bool IsTopElement
        {
            get { return this.isTopElement; }
            set { this.RaiseAndSetIfChanged(ref this.isTopElement, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ElementDefinition"/> can set <see cref="OrganizationalParticipant"/>
        /// </summary>
        public bool AreOrganizationsVisible
        {
            get { return this.areOrganizationsVisible; }
            set { this.RaiseAndSetIfChanged(ref this.areOrganizationsVisible, value); }
        }

        /// <summary>
        /// Gets or sets a value that represents the ModelCode of the current <see cref="ElementDefinition"/>
        /// </summary>
        public string ModelCode
        {
            get { return this.modelCode; }
            set { this.RaiseAndSetIfChanged(ref this.modelCode, value); }
        }

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
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            this.PossibleOrganizations = new List<Organization>();
            this.SelectedOrganizations = new ReactiveList<Organization>();

            this.PopulatePossibleCategories();
            this.PopulatePossibleOrganizations();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();

            if (((Iteration)this.Container).TopElement != null)
            {
                this.IsTopElement = ((Iteration)this.Container).TopElement.Iid == this.Thing.Iid; 
            }

            this.ModelCode = this.Thing.ModelCode();

            if (this.SelectedOwner == null)
            {
                this.SelectedOwner = this.Session.QuerySelectedDomainOfExpertise((Iteration)this.Container);
            }

            this.SelectedOrganizations.Clear();
            this.SelectedOrganizations.AddRange(this.Thing.OrganizationalParticipant.Select(op => op.Organization));
        }

        /// <summary>
        /// Populates the <see cref="DomainOfExpertise"/> that may be owner.
        /// </summary>
        protected override void PopulatePossibleOwner()
        {
            base.PopulatePossibleOwner();

            var engineeringModel = (EngineeringModel)this.Container.Container;
            var domains = engineeringModel.EngineeringModelSetup.ActiveDomain.OrderBy(x => x.Name);
            this.PossibleOwner.AddRange(domains);
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();

            var iteration = this.Container as Iteration;

            if (this.IsTopElement)
            {
                iteration.TopElement = this.Thing;
            }
            else
            {
                if (iteration.TopElement != null && iteration.TopElement.Iid == this.Thing.Iid)
                {
                    iteration.TopElement = null;
                }
            }

            var model = (EngineeringModel)this.Container.Container;
            var organizationalParticipations = model.EngineeringModelSetup.OrganizationalParticipant;

            var selectedOrganizationalParticipants = new List<OrganizationalParticipant>();

            foreach (var selectedOrganization in this.SelectedOrganizations)
            {
                var participant = organizationalParticipations.FirstOrDefault(op => op.Organization.Equals(selectedOrganization));

                if (participant != null)
                {
                    selectedOrganizationalParticipants.Add(participant);
                }
            }

            this.Thing.OrganizationalParticipant = selectedOrganizationalParticipants;
        }

        /// <summary>
        /// Populate the possible <see cref="Category"/> for this <see cref="ElementDefinition"/>
        /// </summary>
        private void PopulatePossibleCategories()
        {
            this.PossibleCategory.Clear();
            var model = (EngineeringModel)this.Container.Container;
            var mrdl = model.EngineeringModelSetup.RequiredRdl.Single();

            var allowedCategories = new List<Category>(mrdl.DefinedCategory.Where(c => c.PermissibleClass.Contains(this.Thing.ClassKind)));
            allowedCategories.AddRange(mrdl.GetRequiredRdls().SelectMany(rdl => rdl.DefinedCategory)
                        .Where(c => c.PermissibleClass.Contains(this.Thing.ClassKind)));

            this.PossibleCategory.AddRange(allowedCategories.OrderBy(c => c.ShortName));
        }

        /// <summary>
        /// Populate the possible <see cref="Organization"/> for this <see cref="ElementDefinition"/>
        /// </summary>
        private void PopulatePossibleOrganizations()
        {
            this.PossibleOrganizations.Clear();

            var model = (EngineeringModel)this.Container.Container;
            var organizationalParticipations = model.EngineeringModelSetup.OrganizationalParticipant;

            if (organizationalParticipations == null || !organizationalParticipations.Any())
            {
                this.AreOrganizationsVisible = false;
                return;
            }
           
            this.AreOrganizationsVisible = true;
            

            var organizations = organizationalParticipations.Select(op => op.Organization).Except(new List<Organization> { model.EngineeringModelSetup.DefaultOrganizationalParticipant?.Organization });

            this.PossibleOrganizations.AddRange(organizations.OrderBy(c => c.Name));
        }

        /// <summary>
        /// Updates the <see cref="OkCanExecute"/> property
        /// </summary>
        protected override void UpdateOkCanExecute()
        {
            base.UpdateOkCanExecute();
            this.OkCanExecute = this.OkCanExecute && this.SelectedOwner != null;
        }
    }
}
