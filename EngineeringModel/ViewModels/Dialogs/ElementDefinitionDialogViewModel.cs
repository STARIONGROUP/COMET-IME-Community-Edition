// -------------------------------------------------------------------------------------------------
// <copyright file="ElementDefinitionDialogViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2026 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary,
//              Rowan de Voogt
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
    using System.Reactive;
    using System.Reactive.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal.Operations;

    using CDP4Composition.Attributes;
    using CDP4Composition.Extensions;
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
        /// Backing field for <see cref="SelectedParameterTreeRow"/>
        /// </summary>
        private IRowViewModelBase<Thing> selectedParameterTreeRow;

        /// <summary>
        /// Backing field for <see cref="SelectedParameterTypeToAdd"/>
        /// </summary>
        private ParameterType selectedParameterTypeToAdd;

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
        /// Gets the <see cref="ParameterGroup"/> and <see cref="Parameter"/> rows shown on the Parameters tab.
        /// The rows are reused from the Element Definition browser so that the same hierarchy and properties are displayed.
        /// </summary>
        public ReactiveList<IRowViewModelBase<Thing>> ParameterRows { get; private set; }

        /// <summary>
        /// Gets or sets the row that is selected in the Parameters tree.
        /// </summary>
        public IRowViewModelBase<Thing> SelectedParameterTreeRow
        {
            get { return this.selectedParameterTreeRow; }
            set { this.RaiseAndSetIfChanged(ref this.selectedParameterTreeRow, value); }
        }

        /// <summary>
        /// Gets the <see cref="ParameterType"/>s from which a new <see cref="Parameter"/> can be created.
        /// <see cref="ParameterType"/>s that are already used by a <see cref="Parameter"/> of this <see cref="ElementDefinition"/>
        /// are excluded, as the same <see cref="ParameterType"/> may not be applied twice.
        /// </summary>
        public ReactiveList<ParameterType> PossibleParameterTypesToAdd { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="ParameterType"/> selected for the creation of a new <see cref="Parameter"/>.
        /// </summary>
        public ParameterType SelectedParameterTypeToAdd
        {
            get { return this.selectedParameterTypeToAdd; }
            set { this.RaiseAndSetIfChanged(ref this.selectedParameterTypeToAdd, value); }
        }

        /// <summary>
        /// Gets the command to create a <see cref="Parameter"/> of the <see cref="SelectedParameterTypeToAdd"/> as part of this dialog's transaction.
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateParameterTreeCommand { get; private set; }

        /// <summary>
        /// Gets the command to create a <see cref="ParameterGroup"/> as part of this dialog's transaction.
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateParameterGroupTreeCommand { get; private set; }

        /// <summary>
        /// Gets the command to edit the selected parameter or parameter group as part of this dialog's transaction.
        /// </summary>
        public ReactiveCommand<Unit, Unit> EditParameterTreeCommand { get; private set; }

        /// <summary>
        /// Gets the command to delete the selected parameter or parameter group as part of this dialog's transaction.
        /// </summary>
        public ReactiveCommand<Unit, Unit> DeleteParameterTreeCommand { get; private set; }

        /// <summary>
        /// Gets the command to inspect the selected parameter or parameter group.
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectParameterTreeCommand { get; private set; }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            this.PossibleOrganizations = new List<Organization>();
            this.SelectedOrganizations = new ReactiveList<Organization>();
            this.ParameterRows = new ReactiveList<IRowViewModelBase<Thing>>();
            this.PossibleParameterTypesToAdd = new ReactiveList<ParameterType>();

            this.PopulatePossibleCategories();
            this.PopulatePossibleOrganizations();
            this.PopulateParameterRows();
            this.PopulatePossibleParameterTypesToAdd();
        }

        /// <summary>
        /// Initializes the <see cref="ReactiveCommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            var canCreateParameter = this.WhenAnyValue(vm => vm.SelectedParameterTypeToAdd, vm => vm.IsReadOnly, (parameterType, readOnly) => parameterType != null && !readOnly);
            var canCreateGroup = this.WhenAnyValue(vm => vm.IsReadOnly, readOnly => !readOnly);
            var canModifySelected = this.WhenAnyValue(vm => vm.SelectedParameterTreeRow, vm => vm.IsReadOnly, (row, readOnly) => row != null && !readOnly);
            var canInspectSelected = this.WhenAnyValue(vm => vm.SelectedParameterTreeRow).Select(row => row != null);

            this.CreateParameterTreeCommand = ReactiveCommandCreator.Create(this.CreateParameter, canCreateParameter);
            this.CreateParameterGroupTreeCommand = ReactiveCommandCreator.Create(() => this.ExecuteCreateCommand<ParameterGroup>(this.RefreshParameterTree), canCreateGroup);
            this.EditParameterTreeCommand = ReactiveCommandCreator.Create(() => this.ExecuteEditCommand(this.SelectedParameterTreeRow.Thing, this.PopulateParameterRows), canModifySelected);
            this.DeleteParameterTreeCommand = ReactiveCommandCreator.Create(this.DeleteSelectedParameterTreeRow, canModifySelected);
            this.InspectParameterTreeCommand = ReactiveCommandCreator.Create(() => this.ExecuteInspectCommand(this.SelectedParameterTreeRow.Thing), canInspectSelected);
        }

        /// <summary>
        /// Creates a new <see cref="Parameter"/> of the <see cref="SelectedParameterTypeToAdd"/> as part of this dialog's
        /// transaction. A blank Parameter cannot be created through the Parameter dialog (it requires a ParameterType),
        /// so the Parameter is staged directly on the transaction, mirroring how the browser creates one.
        /// </summary>
        private void CreateParameter()
        {
            var parameter = new Parameter(Guid.NewGuid(), this.Thing.Cache, this.Thing.IDalUri)
            {
                Owner = this.SelectedOwner,
                ParameterType = this.SelectedParameterTypeToAdd,
                Scale = (this.SelectedParameterTypeToAdd as QuantityKind)?.DefaultScale
            };

            this.Thing.Parameter.Add(parameter);
            this.transaction.Create(parameter);

            this.RefreshParameterTree();
        }

        /// <summary>
        /// Rebuilds the parameter tree and the list of <see cref="ParameterType"/>s that are still available for creation.
        /// </summary>
        private void RefreshParameterTree()
        {
            this.PopulateParameterRows();
            this.PopulatePossibleParameterTypesToAdd();
        }

        /// <summary>
        /// Deletes the selected <see cref="Parameter"/> or <see cref="ParameterGroup"/> as part of this dialog's
        /// transaction and refreshes the tree.
        /// </summary>
        private void DeleteSelectedParameterTreeRow()
        {
            var thing = this.SelectedParameterTreeRow.Thing;
            this.transaction.Delete(thing.Clone(false), this.Thing);

            switch (thing)
            {
                case Parameter parameter:
                    this.Thing.Parameter.Remove(parameter);
                    break;
                case ParameterGroup parameterGroup:
                    this.Thing.ParameterGroup.Remove(parameterGroup);
                    break;
            }

            this.RefreshParameterTree();
        }

        /// <summary>
        /// Populates the <see cref="PossibleParameterTypesToAdd"/> from the model reference data libraries, excluding the
        /// <see cref="ParameterType"/>s that are already used by a <see cref="Parameter"/> of this <see cref="ElementDefinition"/>.
        /// </summary>
        private void PopulatePossibleParameterTypesToAdd()
        {
            this.PossibleParameterTypesToAdd.Clear();

            var model = (EngineeringModel)this.Container.Container;
            var mrdl = model.EngineeringModelSetup.RequiredRdl.Single();

            var parameterTypes = new List<ParameterType>(mrdl.ParameterType);
            parameterTypes.AddRange(mrdl.GetRequiredRdls().SelectMany(rdl => rdl.ParameterType).Except(parameterTypes));

            var usedParameterTypeIids = this.Thing.Parameter.Select(p => p.ParameterType.Iid).ToList();

            this.PossibleParameterTypesToAdd.AddRange(parameterTypes
                .Where(p => !usedParameterTypeIids.Contains(p.Iid))
                .OrderBy(p => p.ShortName));

            this.SelectedParameterTypeToAdd = this.PossibleParameterTypesToAdd.FirstOrDefault();
        }

        /// <summary>
        /// Builds the parameter / parameter-group tree shown on the Parameters tab. The rows are reused from the Element
        /// Definition browser (so the same properties are shown), but the nesting is built here, keyed by <see cref="Thing.Iid"/>
        /// so that it survives the cloning performed by the transaction, and it reflects the parameters and groups that are
        /// currently staged on this dialog's <see cref="Thing"/>.
        /// </summary>
        private void PopulateParameterRows()
        {
            foreach (var row in this.ParameterRows)
            {
                row.Dispose();
            }

            this.ParameterRows.Clear();

            var currentDomain = this.Session.QuerySelectedDomainOfExpertise((Iteration)this.Container);

            var groupRowByIid = new Dictionary<Guid, ParameterGroupRowViewModel>();
            var groups = this.Thing.ParameterGroup.ToList();

            foreach (var group in groups)
            {
                groupRowByIid[group.Iid] = new ParameterGroupRowViewModel(group, currentDomain, this.Session, this);
            }

            foreach (var group in groups)
            {
                var groupRow = groupRowByIid[group.Iid];

                if (group.ContainingGroup != null && groupRowByIid.TryGetValue(group.ContainingGroup.Iid, out var parentRow))
                {
                    parentRow.ContainedRows.Add(groupRow);
                }
                else
                {
                    this.ParameterRows.Add(groupRow);
                }
            }

            foreach (var parameter in this.Thing.Parameter)
            {
                var parameterRow = new ParameterRowViewModel(parameter, this.Session, this, true);

                if (parameter.Group != null && groupRowByIid.TryGetValue(parameter.Group.Iid, out var groupRow))
                {
                    groupRow.ContainedRows.Add(parameterRow);
                }
                else
                {
                    this.ParameterRows.Add(parameterRow);
                }
            }
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

            var iteration = (Iteration)this.Container;
            this.PossibleOwner.AddRange(this.Session.QueryAllowedOwners(iteration, this.Thing.Owner));
            this.CurrentDomainOfExpertise = this.Session.QuerySelectedDomainOfExpertise(iteration);

            if (this.SelectedOwner == null && this.dialogKind == ThingDialogKind.Create)
            {
                this.SelectedOwner = this.CurrentDomainOfExpertise ?? this.PossibleOwner.FirstOrDefault();
            }
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

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// a value indicating whether the class is being disposed of
        /// </param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                foreach (var row in this.ParameterRows)
                {
                    row.Dispose();
                }
            }
        }
    }
}
