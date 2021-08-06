// -------------------------------------------------------------------------------------------------
// <copyright file="ElementDefinitionDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4CommonView.Views;

    using CDP4Composition.Attributes;
    using CDP4Composition.CommonView.HandCodedRows;
    using CDP4Composition.CommonView.ViewModels;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.Operations;

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
        /// Backing field for <see cref="SelectedBehavior"/>
        /// </summary>
        private BehaviorRowViewModel selectedBehavior;

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
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            var canExecuteCreateBehaviorCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            this.CreateBehaviorCommand = ReactiveCommand.Create(canExecuteCreateBehaviorCommand);
            this.CreateBehaviorCommand.Subscribe(_ => this.ExecuteCreateCommand<Behavior>(this.PopulateBehaviors));

            var canExecuteEditSelectedBehaviorCommand = this.WhenAny(vm => vm.SelectedBehavior, v => v.Value != null && !this.IsReadOnly);
            this.EditBehaviorCommand = ReactiveCommand.Create(canExecuteEditSelectedBehaviorCommand);
            this.EditBehaviorCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedBehavior.Thing, this.PopulateBehaviors));

            this.DeleteBehaviorCommand = ReactiveCommand.Create(canExecuteEditSelectedBehaviorCommand);
            this.DeleteBehaviorCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedBehavior.Thing, this.PopulateBehaviors));

            var canExecuteInspectSelectedBehaviorCommand = this.WhenAny(vm => vm.SelectedBehavior, v => v.Value != null);
            this.InspectBehaviorCommand = ReactiveCommand.Create(canExecuteInspectSelectedBehaviorCommand);
            this.InspectBehaviorCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedBehavior.Thing));
        }

        /// <summary>
        /// Populates the <see cref="Behavior"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        private void PopulateBehaviors()
        {
            this.Behavior.Clear();
            foreach (var thing in this.Thing.Behavior.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new BehaviorRowViewModel(thing, this.Session, this);
                this.Behavior.Add(row);
            }
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
        /// Gets the list of <see cref="Behavior"/>s
        /// </summary>
        public ReactiveList<BehaviorRowViewModel> Behavior { get; private set; }

        /// <summary>
        /// Gets or sets the selected <see cref="BehaviorRowViewModel"/>
        /// </summary>
        public BehaviorRowViewModel SelectedBehavior
        {
            get { return this.selectedBehavior; }
            set { this.RaiseAndSetIfChanged(ref this.selectedBehavior, value); }
        }

        /// <summary>
        /// Gets the delete <see cref="ReactiveCommand"/> to delete a <see cref="Behavior"/>
        /// </summary>
        public ReactiveCommand<object> DeleteBehaviorCommand { get; private set; }

        /// <summary>
        /// Gets the inspect <see cref="ReactiveCommand"/> to inspect a <see cref="Behavior"/>
        /// </summary>
        public ReactiveCommand<object> InspectBehaviorCommand { get; private set; }

        /// <summary>
        /// Gets the create <see cref="ReactiveCommand"/> to create a <see cref="Behavior"/>
        /// </summary>
        public ReactiveCommand<object> CreateBehaviorCommand { get; private set; }

        /// <summary>
        /// Gets the edit <see cref="ReactiveCommand"/> to edit a <see cref="Behavior"/>
        /// </summary>
        public ReactiveCommand<object> EditBehaviorCommand { get; private set; }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            this.PossibleOrganizations = new List<Organization>();
            this.SelectedOrganizations = new ReactiveList<Organization>();
            this.SelectedOrganizations.ChangeTrackingEnabled = true;

            this.PopulatePossibleCategories();
            this.PopulatePossibleOrganizations();

            this.Behavior = new ReactiveList<BehaviorRowViewModel>();
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

            this.PopulateBehaviors();
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
