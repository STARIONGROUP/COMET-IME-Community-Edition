// -------------------------------------------------------------------------------------------------
// <copyright file="CompoundParameterTypeDialogViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2017 Starion Group S.A.
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
    using CDP4Common.Types;
    
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
	using CDP4Dal.Operations;
    using CDP4Dal.Permission;
    using ReactiveUI;
    using System.Reactive;

    /// <summary>
    /// dialog-view-model class representing a <see cref="CompoundParameterType"/>
    /// </summary>
    public partial class CompoundParameterTypeDialogViewModel : ParameterTypeDialogViewModel<CompoundParameterType>
    {
        /// <summary>
        /// Backing field for <see cref="IsFinalized"/>
        /// </summary>
        private bool isFinalized;

        /// <summary>
        /// Backing field for <see cref="SelectedComponent"/>
        /// </summary>
        private ParameterTypeComponentRowViewModel selectedComponent;


        /// <summary>
        /// Initializes a new instance of the <see cref="CompoundParameterTypeDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public CompoundParameterTypeDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompoundParameterTypeDialogViewModel"/> class
        /// </summary>
        /// <param name="compoundParameterType">
        /// The <see cref="CompoundParameterType"/> that is the subject of the current view-model. This is the object
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
        public CompoundParameterTypeDialogViewModel(CompoundParameterType compoundParameterType, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(compoundParameterType, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            if(container != null)
            {
                var containerThing = container as ReferenceDataLibrary;
                if(containerThing == null)
                {
                    var errorMessage =
                        string.Format(
                            "The container parameter is of type {0}, it shall be of type ReferenceDataLibrary",
                            container.GetType());
                    throw new ArgumentException(errorMessage);
                }
            }
        }

        /// <summary>
        /// Gets or sets the IsFinalized
        /// </summary>
        public virtual bool IsFinalized
        {
            get { return this.isFinalized; }
            set { this.RaiseAndSetIfChanged(ref this.isFinalized, value); }
        }
        
        /// <summary>
        /// Gets or sets the selected <see cref="ParameterTypeComponentRowViewModel"/>
        /// </summary>
        public ParameterTypeComponentRowViewModel SelectedComponent
        {
            get { return this.selectedComponent; }
            set { this.RaiseAndSetIfChanged(ref this.selectedComponent, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="ParameterTypeComponent"/>
        /// </summary>
        public ReactiveList<ParameterTypeComponentRowViewModel> Component { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a ParameterTypeComponent
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateComponentCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a ParameterTypeComponent
        /// </summary>
        public ReactiveCommand<Unit, Unit> DeleteComponentCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a ParameterTypeComponent
        /// </summary>
        public ReactiveCommand<Unit, Unit> EditComponentCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a ParameterTypeComponent
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectComponentCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Move-Up <see cref="ICommand"/> to move up the order of a ParameterTypeComponent 
        /// </summary>
        public ReactiveCommand<Unit, Unit> MoveUpComponentCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Move-Down <see cref="ICommand"/> to move down the order of a ParameterTypeComponent
        /// </summary>
        public ReactiveCommand<Unit, Unit> MoveDownComponentCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            
            var canExecuteCreateComponentCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedComponentCommand = this.WhenAny(vm => vm.SelectedComponent, v => v.Value != null);
            var canExecuteEditSelectedComponentCommand = this.WhenAny(vm => vm.SelectedComponent, v => v.Value != null && !this.IsReadOnly);

            this.CreateComponentCommand = ReactiveCommandCreator.Create(canExecuteCreateComponentCommand);
            this.CreateComponentCommand.Subscribe(_ => this.ExecuteCreateCommand<ParameterTypeComponent>(this.PopulateComponent));

            this.DeleteComponentCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedComponentCommand);
            this.DeleteComponentCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedComponent.Thing, this.PopulateComponent));

            this.EditComponentCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedComponentCommand);
            this.EditComponentCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedComponent.Thing, this.PopulateComponent));

            this.InspectComponentCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedComponentCommand);
            this.InspectComponentCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedComponent.Thing));

            this.MoveUpComponentCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedComponentCommand);
            this.MoveUpComponentCommand.Subscribe(_ => this.ExecuteMoveUpCommand(this.Component, this.SelectedComponent));

            this.MoveDownComponentCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedComponentCommand);
            this.MoveDownComponentCommand.Subscribe(_ => this.ExecuteMoveDownCommand(this.Component, this.SelectedComponent));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.IsFinalized = this.IsFinalized;

            if (!clone.Component.SortedItems.Values.SequenceEqual(this.Component.Select(x => x.Thing)))
            {
                var itemCount = this.Component.Count;
                for (var i = 0; i < itemCount; i++)
                {
                    var item = this.Component[i].Thing;
                    var currentIndex = clone.Component.IndexOf(item);

                    if (currentIndex != i)
                    {
                        clone.Component.Move(currentIndex, i);
                    }
                }
            }
        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.Component = new ReactiveList<ParameterTypeComponentRowViewModel>();
            this.PopulatePossibleContainer();
        }

        /// <summary>
        /// Populate the possible containers for a <see cref="CompoundParameterType"/>
        /// </summary>
        private void PopulatePossibleContainer()
        {
            this.PossibleContainer.Clear();
            // When creating a new Rule, it can be contained by any ReferenceDataLibrary that is currently loaded
            if (this.dialogKind == ThingDialogKind.Create)
            {
                this.PossibleContainer.AddRange(this.Session.OpenReferenceDataLibraries.Where(x => this.PermissionService.CanWrite(x)).Select(x => x.Clone(false)));
                this.Container = this.PossibleContainer.FirstOrDefault();
                return;
            }

            // When inspecting an existing Rule, only it's container needs to be added to the PossibleContainer property (it cannot be changed)
            if (this.dialogKind == ThingDialogKind.Inspect)
            {
                this.PossibleContainer.Add(this.Thing.Container);
                this.Container = this.Thing.Container;
                return;
            }

            // When updating a Rule, the possible ReferenceDataLibrary can only be the ReferenceDataLibrary in the current chain of ReferenceDataLibrary of the Rule
            if (this.dialogKind == ThingDialogKind.Update)
            {
                var containerRdl = (ReferenceDataLibrary)this.Container;
                this.PossibleContainer.Add(containerRdl);
                var chainOfRdls = containerRdl.GetRequiredRdls();
                this.PossibleContainer.AddRange(chainOfRdls.Where(x => this.PermissionService.CanWrite(x)).Select(x => x.Clone(false)));
            }
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.IsFinalized = this.Thing.IsFinalized;
            this.PopulateComponent();
        }

        /// <summary>
        /// Populates the <see cref="Component"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateComponent()
        {
            this.Component.Clear();
            foreach (ParameterTypeComponent thing in this.Thing.Component.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new ParameterTypeComponentRowViewModel(thing, this.Session, this);
                this.Component.Add(row);
                row.Index = this.Thing.Component.IndexOf(thing);
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
            foreach(var component in this.Component)
            {
                component.Dispose();
            }
        }
    }
}
