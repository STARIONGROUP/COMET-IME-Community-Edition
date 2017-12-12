// -------------------------------------------------------------------------------------------------
// <copyright file="DiagramElementContainerDialogViewModel.cs" company="RHEA System S.A.">
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
    using CDP4Common.DiagramData;
    
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
	using CDP4Dal.Operations;
    using CDP4Dal.Permission;
    using ReactiveUI;

    /// <summary>
    /// dialog-view-model class representing a <see cref="DiagramElementContainer"/>
    /// </summary>
    public abstract partial class DiagramElementContainerDialogViewModel<T> : DiagramThingBaseDialogViewModel<T> where T : DiagramElementContainer
    {
        /// <summary>
        /// Backing field for <see cref="SelectedDiagramElement"/>
        /// </summary>
        private IRowViewModelBase<DiagramElementThing> selectedDiagramElement;

        /// <summary>
        /// Backing field for <see cref="SelectedBounds"/>
        /// </summary>
        private BoundsRowViewModel selectedBounds;


        /// <summary>
        /// Backing field for <see cref="SelectedDiagramElementThing"/>Kind
        /// </summary>
        private ClassKind selectedDiagramElementThingKind;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagramElementContainerDialogViewModel{T}"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        protected DiagramElementContainerDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagramElementContainerDialogViewModel{T}"/> class
        /// </summary>
        /// <param name="diagramElementContainer">
        /// The <see cref="DiagramElementContainer"/> that is the subject of the current view-model. This is the object
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
        protected DiagramElementContainerDialogViewModel(T diagramElementContainer, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(diagramElementContainer, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
        }
        
        /// <summary>
        /// Gets the concrete DiagramElementThing to create
        /// </summary>
        public ClassKind SelectedDiagramElementThingKind
        {
            get { return this.selectedDiagramElementThingKind; }
            set { this.RaiseAndSetIfChanged(ref this.selectedDiagramElementThingKind, value); } 
        }

        /// <summary>
        /// Gets the list of possible <see cref="ClassKind"/> for <see cref="Uml.StructuredClassifiers.Class"/>
        /// </summary>
        public readonly ReactiveList<ClassKind> PossibleDiagramElementThingKind = new ReactiveList<ClassKind>() 
        { 
            ClassKind.DiagramEdge,
            ClassKind.DiagramObject 
        };
        
        /// <summary>
        /// Gets or sets the selected <see cref="IRowViewModelBase{T}"/>
        /// </summary>
        public IRowViewModelBase<DiagramElementThing> SelectedDiagramElement
        {
            get { return this.selectedDiagramElement; }
            set { this.RaiseAndSetIfChanged(ref this.selectedDiagramElement, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="DiagramElementThing"/>
        /// </summary>
        public ReactiveList<IRowViewModelBase<DiagramElementThing>> DiagramElement { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="BoundsRowViewModel"/>
        /// </summary>
        public BoundsRowViewModel SelectedBounds
        {
            get { return this.selectedBounds; }
            set { this.RaiseAndSetIfChanged(ref this.selectedBounds, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="Bounds"/>
        /// </summary>
        public ReactiveList<BoundsRowViewModel> Bounds { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a DiagramElementThing
        /// </summary>
        public ReactiveCommand<object> CreateDiagramElementCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a DiagramElementThing
        /// </summary>
        public ReactiveCommand<object> DeleteDiagramElementCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a DiagramElementThing
        /// </summary>
        public ReactiveCommand<object> EditDiagramElementCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a DiagramElementThing
        /// </summary>
        public ReactiveCommand<object> InspectDiagramElementCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a Bounds
        /// </summary>
        public ReactiveCommand<object> CreateBoundsCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a Bounds
        /// </summary>
        public ReactiveCommand<object> DeleteBoundsCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a Bounds
        /// </summary>
        public ReactiveCommand<object> EditBoundsCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a Bounds
        /// </summary>
        public ReactiveCommand<object> InspectBoundsCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            
            var canExecuteCreateDiagramElementCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedDiagramElementCommand = this.WhenAny(vm => vm.SelectedDiagramElement, v => v.Value != null);
            var canExecuteEditSelectedDiagramElementCommand = this.WhenAny(vm => vm.SelectedDiagramElement, v => v.Value != null && !this.IsReadOnly);

            this.CreateDiagramElementCommand = ReactiveCommand.Create(canExecuteCreateDiagramElementCommand);
            this.CreateDiagramElementCommand.Subscribe(_ => this.ExecuteCreateDiagramElementCommand());

            this.DeleteDiagramElementCommand = ReactiveCommand.Create(canExecuteEditSelectedDiagramElementCommand);
            this.DeleteDiagramElementCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedDiagramElement.Thing, this.PopulateDiagramElement));

            this.EditDiagramElementCommand = ReactiveCommand.Create(canExecuteEditSelectedDiagramElementCommand);
            this.EditDiagramElementCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedDiagramElement.Thing, this.PopulateDiagramElement));

            this.InspectDiagramElementCommand = ReactiveCommand.Create(canExecuteInspectSelectedDiagramElementCommand);
            this.InspectDiagramElementCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedDiagramElement.Thing));
            
            var canExecuteCreateBoundsCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedBoundsCommand = this.WhenAny(vm => vm.SelectedBounds, v => v.Value != null);
            var canExecuteEditSelectedBoundsCommand = this.WhenAny(vm => vm.SelectedBounds, v => v.Value != null && !this.IsReadOnly);

            this.CreateBoundsCommand = ReactiveCommand.Create(canExecuteCreateBoundsCommand);
            this.CreateBoundsCommand.Subscribe(_ => this.ExecuteCreateCommand<Bounds>(this.PopulateBounds));

            this.DeleteBoundsCommand = ReactiveCommand.Create(canExecuteEditSelectedBoundsCommand);
            this.DeleteBoundsCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedBounds.Thing, this.PopulateBounds));

            this.EditBoundsCommand = ReactiveCommand.Create(canExecuteEditSelectedBoundsCommand);
            this.EditBoundsCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedBounds.Thing, this.PopulateBounds));

            this.InspectBoundsCommand = ReactiveCommand.Create(canExecuteInspectSelectedBoundsCommand);
            this.InspectBoundsCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedBounds.Thing));
        }

        /// <summary>
        /// Executes the Create <see cref="ICommand"/> for the abstract <see cref="DiagramElementThing"/>
        /// </summary>
        protected void ExecuteCreateDiagramElementCommand()
        {
            switch (this.SelectedDiagramElementThingKind)
            {
                case ClassKind.DiagramEdge:
                    this.ExecuteCreateCommand<DiagramEdge>(this.PopulateDiagramElement);
                    break;
                case ClassKind.DiagramObject:
                    this.ExecuteCreateCommand<DiagramObject>(this.PopulateDiagramElement);
                    break;
                default:
                    break;

            }
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.DiagramElement = new ReactiveList<IRowViewModelBase<DiagramElementThing>>();
            this.Bounds = new ReactiveList<BoundsRowViewModel>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.PopulateDiagramElement();
            this.PopulateBounds();
        }

        /// <summary>
        /// Populates the <see cref="DiagramElement"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateDiagramElement()
        {
            // this method needs to be overriden.
        }

        /// <summary>
        /// Populates the <see cref="Bounds"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateBounds()
        {
            this.Bounds.Clear();
            foreach (var thing in this.Thing.Bounds.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new BoundsRowViewModel(thing, this.Session, this);
                this.Bounds.Add(row);
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
            foreach(var diagramElement in this.DiagramElement)
            {
                diagramElement.Dispose();
            }
            foreach(var bounds in this.Bounds)
            {
                bounds.Dispose();
            }
        }
    }
}
