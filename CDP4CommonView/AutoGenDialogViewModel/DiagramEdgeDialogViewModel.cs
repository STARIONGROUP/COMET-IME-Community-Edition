// -------------------------------------------------------------------------------------------------
// <copyright file="DiagramEdgeDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2017 RHEA System S.A.
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
    using CDP4Common.Types;
    using CDP4Common.Operations;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Permission;
    using ReactiveUI;

    /// <summary>
    /// dialog-view-model class representing a <see cref="DiagramEdge"/>
    /// </summary>
    public partial class DiagramEdgeDialogViewModel : DiagramElementThingDialogViewModel<DiagramEdge>
    {
        /// <summary>
        /// Backing field for <see cref="SelectedSource"/>
        /// </summary>
        private DiagramElementThing selectedSource;

        /// <summary>
        /// Backing field for <see cref="SelectedTarget"/>
        /// </summary>
        private DiagramElementThing selectedTarget;

        /// <summary>
        /// Backing field for <see cref="SelectedPoint"/>
        /// </summary>
        private PointRowViewModel selectedPoint;


        /// <summary>
        /// Initializes a new instance of the <see cref="DiagramEdgeDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public DiagramEdgeDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagramEdgeDialogViewModel"/> class
        /// </summary>
        /// <param name="diagramEdge">
        /// The <see cref="DiagramEdge"/> that is the subject of the current view-model. This is the object
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
        public DiagramEdgeDialogViewModel(DiagramEdge diagramEdge, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(diagramEdge, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            if(container != null)
            {
                var containerThing = container as DiagramElementContainer;
                if(containerThing == null)
                {
                    var errorMessage =
                        string.Format(
                            "The container parameter is of type {0}, it shall be of type DiagramElementContainer",
                            container.GetType());
                    throw new ArgumentException(errorMessage);
                }
            }
        }

        /// <summary>
        /// Gets or sets the SelectedSource
        /// </summary>
        public virtual DiagramElementThing SelectedSource
        {
            get { return this.selectedSource; }
            set { this.RaiseAndSetIfChanged(ref this.selectedSource, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="DiagramElementThing"/>s for <see cref="SelectedSource"/>
        /// </summary>
        public ReactiveList<DiagramElementThing> PossibleSource { get; protected set; }

        /// <summary>
        /// Gets or sets the SelectedTarget
        /// </summary>
        public virtual DiagramElementThing SelectedTarget
        {
            get { return this.selectedTarget; }
            set { this.RaiseAndSetIfChanged(ref this.selectedTarget, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="DiagramElementThing"/>s for <see cref="SelectedTarget"/>
        /// </summary>
        public ReactiveList<DiagramElementThing> PossibleTarget { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="PointRowViewModel"/>
        /// </summary>
        public PointRowViewModel SelectedPoint
        {
            get { return this.selectedPoint; }
            set { this.RaiseAndSetIfChanged(ref this.selectedPoint, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="Point"/>
        /// </summary>
        public ReactiveList<PointRowViewModel> Point { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedSource"/>
        /// </summary>
        public ReactiveCommand<object> InspectSelectedSourceCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedTarget"/>
        /// </summary>
        public ReactiveCommand<object> InspectSelectedTargetCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a Point
        /// </summary>
        public ReactiveCommand<object> CreatePointCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a Point
        /// </summary>
        public ReactiveCommand<object> DeletePointCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a Point
        /// </summary>
        public ReactiveCommand<object> EditPointCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a Point
        /// </summary>
        public ReactiveCommand<object> InspectPointCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Move-Up <see cref="ICommand"/> to move up the order of a Point 
        /// </summary>
        public ReactiveCommand<object> MoveUpPointCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Move-Down <see cref="ICommand"/> to move down the order of a Point
        /// </summary>
        public ReactiveCommand<object> MoveDownPointCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            
            var canExecuteCreatePointCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedPointCommand = this.WhenAny(vm => vm.SelectedPoint, v => v.Value != null);
            var canExecuteEditSelectedPointCommand = this.WhenAny(vm => vm.SelectedPoint, v => v.Value != null && !this.IsReadOnly);

            this.CreatePointCommand = ReactiveCommand.Create(canExecuteCreatePointCommand);
            this.CreatePointCommand.Subscribe(_ => this.ExecuteCreateCommand<Point>(this.PopulatePoint));

            this.DeletePointCommand = ReactiveCommand.Create(canExecuteEditSelectedPointCommand);
            this.DeletePointCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedPoint.Thing, this.PopulatePoint));

            this.EditPointCommand = ReactiveCommand.Create(canExecuteEditSelectedPointCommand);
            this.EditPointCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedPoint.Thing, this.PopulatePoint));

            this.InspectPointCommand = ReactiveCommand.Create(canExecuteInspectSelectedPointCommand);
            this.InspectPointCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedPoint.Thing));

            this.MoveUpPointCommand = ReactiveCommand.Create(canExecuteEditSelectedPointCommand);
            this.MoveUpPointCommand.Subscribe(_ => this.ExecuteMoveUpCommand(this.Point, this.SelectedPoint));

            this.MoveDownPointCommand = ReactiveCommand.Create(canExecuteEditSelectedPointCommand);
            this.MoveDownPointCommand.Subscribe(_ => this.ExecuteMoveDownCommand(this.Point, this.SelectedPoint));
            var canExecuteInspectSelectedSourceCommand = this.WhenAny(vm => vm.SelectedSource, v => v.Value != null);
            this.InspectSelectedSourceCommand = ReactiveCommand.Create(canExecuteInspectSelectedSourceCommand);
            this.InspectSelectedSourceCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedSource));
            var canExecuteInspectSelectedTargetCommand = this.WhenAny(vm => vm.SelectedTarget, v => v.Value != null);
            this.InspectSelectedTargetCommand = ReactiveCommand.Create(canExecuteInspectSelectedTargetCommand);
            this.InspectSelectedTargetCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedTarget));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.Source = this.SelectedSource;
            clone.Target = this.SelectedTarget;

            if (!clone.Point.SortedItems.Values.SequenceEqual(this.Point.Select(x => x.Thing)))
            {
                var itemCount = this.Point.Count;
                for (var i = 0; i < itemCount; i++)
                {
                    var item = this.Point[i].Thing;
                    var currentIndex = clone.Point.IndexOf(item);

                    if (currentIndex != i)
                    {
                        clone.Point.Move(currentIndex, i);
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
            this.PossibleSource = new ReactiveList<DiagramElementThing>();
            this.PossibleTarget = new ReactiveList<DiagramElementThing>();
            this.Point = new ReactiveList<PointRowViewModel>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.SelectedSource = this.Thing.Source;
            this.PopulatePossibleSource();
            this.SelectedTarget = this.Thing.Target;
            this.PopulatePossibleTarget();
            this.PopulatePoint();
        }

        /// <summary>
        /// Populates the <see cref="Point"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulatePoint()
        {
            this.Point.Clear();
            foreach (Point thing in this.Thing.Point.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new PointRowViewModel(thing, this.Session, this);
                this.Point.Add(row);
                row.Index = this.Thing.Point.IndexOf(thing);
            }
        }

        /// <summary>
        /// Populates the <see cref="PossibleSource"/> property
        /// </summary>
        protected virtual void PopulatePossibleSource()
        {
            this.PossibleSource.Clear();
        }

        /// <summary>
        /// Populates the <see cref="PossibleTarget"/> property
        /// </summary>
        protected virtual void PopulatePossibleTarget()
        {
            this.PossibleTarget.Clear();
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
            foreach(var point in this.Point)
            {
                point.Dispose();
            }
        }
    }
}
