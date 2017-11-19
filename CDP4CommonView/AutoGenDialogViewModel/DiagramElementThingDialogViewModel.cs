// -------------------------------------------------------------------------------------------------
// <copyright file="DiagramElementThingDialogViewModel.cs" company="RHEA System S.A.">
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
    using CDP4Common.Operations;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Permission;
    using ReactiveUI;

    /// <summary>
    /// dialog-view-model class representing a <see cref="DiagramElementThing"/>
    /// </summary>
    public abstract partial class DiagramElementThingDialogViewModel<T> : DiagramElementContainerDialogViewModel<T> where T : DiagramElementThing
    {
        /// <summary>
        /// Backing field for <see cref="SelectedDepictedThing"/>
        /// </summary>
        private Thing selectedDepictedThing;

        /// <summary>
        /// Backing field for <see cref="SelectedSharedStyle"/>
        /// </summary>
        private SharedStyle selectedSharedStyle;

        /// <summary>
        /// Backing field for <see cref="SelectedLocalStyle"/>
        /// </summary>
        private OwnedStyleRowViewModel selectedLocalStyle;


        /// <summary>
        /// Initializes a new instance of the <see cref="DiagramElementThingDialogViewModel{T}"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        protected DiagramElementThingDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagramElementThingDialogViewModel{T}"/> class
        /// </summary>
        /// <param name="diagramElementThing">
        /// The <see cref="DiagramElementThing"/> that is the subject of the current view-model. This is the object
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
        protected DiagramElementThingDialogViewModel(T diagramElementThing, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(diagramElementThing, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
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
        /// Gets or sets the SelectedDepictedThing
        /// </summary>
        public virtual Thing SelectedDepictedThing
        {
            get { return this.selectedDepictedThing; }
            set { this.RaiseAndSetIfChanged(ref this.selectedDepictedThing, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="Thing"/>s for <see cref="SelectedDepictedThing"/>
        /// </summary>
        public ReactiveList<Thing> PossibleDepictedThing { get; protected set; }

        /// <summary>
        /// Gets or sets the SelectedSharedStyle
        /// </summary>
        public virtual SharedStyle SelectedSharedStyle
        {
            get { return this.selectedSharedStyle; }
            set { this.RaiseAndSetIfChanged(ref this.selectedSharedStyle, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="SharedStyle"/>s for <see cref="SelectedSharedStyle"/>
        /// </summary>
        public ReactiveList<SharedStyle> PossibleSharedStyle { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="OwnedStyleRowViewModel"/>
        /// </summary>
        public OwnedStyleRowViewModel SelectedLocalStyle
        {
            get { return this.selectedLocalStyle; }
            set { this.RaiseAndSetIfChanged(ref this.selectedLocalStyle, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="OwnedStyle"/>
        /// </summary>
        public ReactiveList<OwnedStyleRowViewModel> LocalStyle { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedDepictedThing"/>
        /// </summary>
        public ReactiveCommand<object> InspectSelectedDepictedThingCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedSharedStyle"/>
        /// </summary>
        public ReactiveCommand<object> InspectSelectedSharedStyleCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a OwnedStyle
        /// </summary>
        public ReactiveCommand<object> CreateLocalStyleCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a OwnedStyle
        /// </summary>
        public ReactiveCommand<object> DeleteLocalStyleCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a OwnedStyle
        /// </summary>
        public ReactiveCommand<object> EditLocalStyleCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a OwnedStyle
        /// </summary>
        public ReactiveCommand<object> InspectLocalStyleCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            
            var canExecuteCreateLocalStyleCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedLocalStyleCommand = this.WhenAny(vm => vm.SelectedLocalStyle, v => v.Value != null);
            var canExecuteEditSelectedLocalStyleCommand = this.WhenAny(vm => vm.SelectedLocalStyle, v => v.Value != null && !this.IsReadOnly);

            this.CreateLocalStyleCommand = ReactiveCommand.Create(canExecuteCreateLocalStyleCommand);
            this.CreateLocalStyleCommand.Subscribe(_ => this.ExecuteCreateCommand<OwnedStyle>(this.PopulateLocalStyle));

            this.DeleteLocalStyleCommand = ReactiveCommand.Create(canExecuteEditSelectedLocalStyleCommand);
            this.DeleteLocalStyleCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedLocalStyle.Thing, this.PopulateLocalStyle));

            this.EditLocalStyleCommand = ReactiveCommand.Create(canExecuteEditSelectedLocalStyleCommand);
            this.EditLocalStyleCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedLocalStyle.Thing, this.PopulateLocalStyle));

            this.InspectLocalStyleCommand = ReactiveCommand.Create(canExecuteInspectSelectedLocalStyleCommand);
            this.InspectLocalStyleCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedLocalStyle.Thing));
            var canExecuteInspectSelectedDepictedThingCommand = this.WhenAny(vm => vm.SelectedDepictedThing, v => v.Value != null);
            this.InspectSelectedDepictedThingCommand = ReactiveCommand.Create(canExecuteInspectSelectedDepictedThingCommand);
            this.InspectSelectedDepictedThingCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedDepictedThing));
            var canExecuteInspectSelectedSharedStyleCommand = this.WhenAny(vm => vm.SelectedSharedStyle, v => v.Value != null);
            this.InspectSelectedSharedStyleCommand = ReactiveCommand.Create(canExecuteInspectSelectedSharedStyleCommand);
            this.InspectSelectedSharedStyleCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedSharedStyle));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.DepictedThing = this.SelectedDepictedThing;
            clone.SharedStyle = this.SelectedSharedStyle;
        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PossibleDepictedThing = new ReactiveList<Thing>();
            this.PossibleSharedStyle = new ReactiveList<SharedStyle>();
            this.LocalStyle = new ReactiveList<OwnedStyleRowViewModel>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.SelectedDepictedThing = this.Thing.DepictedThing;
            this.PopulatePossibleDepictedThing();
            this.SelectedSharedStyle = this.Thing.SharedStyle;
            this.PopulatePossibleSharedStyle();
            this.PopulateLocalStyle();
        }

        /// <summary>
        /// Populates the <see cref="LocalStyle"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateLocalStyle()
        {
            this.LocalStyle.Clear();
            foreach (var thing in this.Thing.LocalStyle.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new OwnedStyleRowViewModel(thing, this.Session, this);
                this.LocalStyle.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="PossibleDepictedThing"/> property
        /// </summary>
        protected virtual void PopulatePossibleDepictedThing()
        {
            this.PossibleDepictedThing.Clear();
        }

        /// <summary>
        /// Populates the <see cref="PossibleSharedStyle"/> property
        /// </summary>
        protected virtual void PopulatePossibleSharedStyle()
        {
            this.PossibleSharedStyle.Clear();
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
            foreach(var localStyle in this.LocalStyle)
            {
                localStyle.Dispose();
            }
        }
    }
}
