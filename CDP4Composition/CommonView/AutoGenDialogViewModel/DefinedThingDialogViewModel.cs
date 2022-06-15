// -------------------------------------------------------------------------------------------------
// <copyright file="DefinedThingDialogViewModel.cs" company="RHEA System S.A.">
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
    
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
	using CDP4Dal.Operations;
    using CDP4Dal.Permission;
    using ReactiveUI;
    using System.Reactive;

    /// <summary>
    /// dialog-view-model class representing a <see cref="DefinedThing"/>
    /// </summary>
    public abstract partial class DefinedThingDialogViewModel<T> : DialogViewModelBase<T> where T : DefinedThing
    {
        /// <summary>
        /// Backing field for <see cref="Name"/>
        /// </summary>
        private string name;

        /// <summary>
        /// Backing field for <see cref="ShortName"/>
        /// </summary>
        private string shortName;

        /// <summary>
        /// Backing field for <see cref="SelectedAlias"/>
        /// </summary>
        private AliasRowViewModel selectedAlias;

        /// <summary>
        /// Backing field for <see cref="SelectedDefinition"/>
        /// </summary>
        private DefinitionRowViewModel selectedDefinition;

        /// <summary>
        /// Backing field for <see cref="SelectedHyperLink"/>
        /// </summary>
        private HyperLinkRowViewModel selectedHyperLink;


        /// <summary>
        /// Initializes a new instance of the <see cref="DefinedThingDialogViewModel{T}"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        protected DefinedThingDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefinedThingDialogViewModel{T}"/> class
        /// </summary>
        /// <param name="definedThing">
        /// The <see cref="DefinedThing"/> that is the subject of the current view-model. This is the object
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
        protected DefinedThingDialogViewModel(T definedThing, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(definedThing, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
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
        /// Gets or sets the selected <see cref="AliasRowViewModel"/>
        /// </summary>
        public AliasRowViewModel SelectedAlias
        {
            get { return this.selectedAlias; }
            set { this.RaiseAndSetIfChanged(ref this.selectedAlias, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="Alias"/>
        /// </summary>
        public ReactiveList<AliasRowViewModel> Alias { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="DefinitionRowViewModel"/>
        /// </summary>
        public DefinitionRowViewModel SelectedDefinition
        {
            get { return this.selectedDefinition; }
            set { this.RaiseAndSetIfChanged(ref this.selectedDefinition, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="Definition"/>
        /// </summary>
        public ReactiveList<DefinitionRowViewModel> Definition { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="HyperLinkRowViewModel"/>
        /// </summary>
        public HyperLinkRowViewModel SelectedHyperLink
        {
            get { return this.selectedHyperLink; }
            set { this.RaiseAndSetIfChanged(ref this.selectedHyperLink, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="HyperLink"/>
        /// </summary>
        public ReactiveList<HyperLinkRowViewModel> HyperLink { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a Alias
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateAliasCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a Alias
        /// </summary>
        public ReactiveCommand<Unit, Unit> DeleteAliasCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a Alias
        /// </summary>
        public ReactiveCommand<Unit, Unit> EditAliasCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a Alias
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectAliasCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a Definition
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateDefinitionCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a Definition
        /// </summary>
        public ReactiveCommand<Unit, Unit> DeleteDefinitionCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a Definition
        /// </summary>
        public ReactiveCommand<Unit, Unit> EditDefinitionCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a Definition
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectDefinitionCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a HyperLink
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateHyperLinkCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a HyperLink
        /// </summary>
        public ReactiveCommand<Unit, Unit> DeleteHyperLinkCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a HyperLink
        /// </summary>
        public ReactiveCommand<Unit, Unit> EditHyperLinkCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a HyperLink
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectHyperLinkCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            
            var canExecuteCreateAliasCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedAliasCommand = this.WhenAny(vm => vm.SelectedAlias, v => v.Value != null);
            var canExecuteEditSelectedAliasCommand = this.WhenAny(vm => vm.SelectedAlias, v => v.Value != null && !this.IsReadOnly);

            this.CreateAliasCommand = ReactiveCommandCreator.Create(canExecuteCreateAliasCommand);
            this.CreateAliasCommand.Subscribe(_ => this.ExecuteCreateCommand<Alias>(this.PopulateAlias));

            this.DeleteAliasCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedAliasCommand);
            this.DeleteAliasCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedAlias.Thing, this.PopulateAlias));

            this.EditAliasCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedAliasCommand);
            this.EditAliasCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedAlias.Thing, this.PopulateAlias));

            this.InspectAliasCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedAliasCommand);
            this.InspectAliasCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedAlias.Thing));
            
            var canExecuteCreateDefinitionCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedDefinitionCommand = this.WhenAny(vm => vm.SelectedDefinition, v => v.Value != null);
            var canExecuteEditSelectedDefinitionCommand = this.WhenAny(vm => vm.SelectedDefinition, v => v.Value != null && !this.IsReadOnly);

            this.CreateDefinitionCommand = ReactiveCommandCreator.Create(canExecuteCreateDefinitionCommand);
            this.CreateDefinitionCommand.Subscribe(_ => this.ExecuteCreateCommand<Definition>(this.PopulateDefinition));

            this.DeleteDefinitionCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedDefinitionCommand);
            this.DeleteDefinitionCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedDefinition.Thing, this.PopulateDefinition));

            this.EditDefinitionCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedDefinitionCommand);
            this.EditDefinitionCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedDefinition.Thing, this.PopulateDefinition));

            this.InspectDefinitionCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedDefinitionCommand);
            this.InspectDefinitionCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedDefinition.Thing));
            
            var canExecuteCreateHyperLinkCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedHyperLinkCommand = this.WhenAny(vm => vm.SelectedHyperLink, v => v.Value != null);
            var canExecuteEditSelectedHyperLinkCommand = this.WhenAny(vm => vm.SelectedHyperLink, v => v.Value != null && !this.IsReadOnly);

            this.CreateHyperLinkCommand = ReactiveCommandCreator.Create(canExecuteCreateHyperLinkCommand);
            this.CreateHyperLinkCommand.Subscribe(_ => this.ExecuteCreateCommand<HyperLink>(this.PopulateHyperLink));

            this.DeleteHyperLinkCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedHyperLinkCommand);
            this.DeleteHyperLinkCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedHyperLink.Thing, this.PopulateHyperLink));

            this.EditHyperLinkCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedHyperLinkCommand);
            this.EditHyperLinkCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedHyperLink.Thing, this.PopulateHyperLink));

            this.InspectHyperLinkCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedHyperLinkCommand);
            this.InspectHyperLinkCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedHyperLink.Thing));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.Name = this.Name;
            clone.ShortName = this.ShortName;
        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.Alias = new ReactiveList<AliasRowViewModel>();
            this.Definition = new ReactiveList<DefinitionRowViewModel>();
            this.HyperLink = new ReactiveList<HyperLinkRowViewModel>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.Name = this.Thing.Name;
            this.ShortName = this.Thing.ShortName;
            this.PopulateAlias();
            this.PopulateDefinition();
            this.PopulateHyperLink();
        }

        /// <summary>
        /// Populates the <see cref="Alias"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateAlias()
        {
            this.Alias.Clear();
            foreach (var thing in this.Thing.Alias.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new AliasRowViewModel(thing, this.Session, this);
                this.Alias.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="Definition"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateDefinition()
        {
            this.Definition.Clear();
            foreach (var thing in this.Thing.Definition.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new DefinitionRowViewModel(thing, this.Session, this);
                this.Definition.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="HyperLink"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateHyperLink()
        {
            this.HyperLink.Clear();
            foreach (var thing in this.Thing.HyperLink.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new HyperLinkRowViewModel(thing, this.Session, this);
                this.HyperLink.Add(row);
            }
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
            foreach(var alias in this.Alias)
            {
                alias.Dispose();
            }
            foreach(var definition in this.Definition)
            {
                definition.Dispose();
            }
            foreach(var hyperLink in this.HyperLink)
            {
                hyperLink.Dispose();
            }
        }
    }
}
