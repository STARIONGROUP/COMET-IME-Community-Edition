// -------------------------------------------------------------------------------------------------
// <copyright file="OptionDialogViewModel.cs" company="RHEA System S.A.">
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
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
	using CDP4Dal.Operations;
    using CDP4Dal.Permission;
    using ReactiveUI;
    using System.Reactive;

    /// <summary>
    /// dialog-view-model class representing a <see cref="Option"/>
    /// </summary>
    public partial class OptionDialogViewModel : DefinedThingDialogViewModel<Option>
    {
        /// <summary>
        /// Backing field for <see cref="SelectedNestedElement"/>
        /// </summary>
        private NestedElementRowViewModel selectedNestedElement;


        /// <summary>
        /// Initializes a new instance of the <see cref="OptionDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public OptionDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionDialogViewModel"/> class
        /// </summary>
        /// <param name="option">
        /// The <see cref="Option"/> that is the subject of the current view-model. This is the object
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
        public OptionDialogViewModel(Option option, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(option, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            if(container != null)
            {
                var containerThing = container as Iteration;
                if(containerThing == null)
                {
                    var errorMessage =
                        string.Format(
                            "The container parameter is of type {0}, it shall be of type Iteration",
                            container.GetType());
                    throw new ArgumentException(errorMessage);
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the selected <see cref="NestedElementRowViewModel"/>
        /// </summary>
        public NestedElementRowViewModel SelectedNestedElement
        {
            get { return this.selectedNestedElement; }
            set { this.RaiseAndSetIfChanged(ref this.selectedNestedElement, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="NestedElement"/>
        /// </summary>
        public ReactiveList<NestedElementRowViewModel> NestedElement { get; protected set; }
        
        /// <summary>
        /// Backing field for <see cref="Category"/>s
        /// </summary>
        private ReactiveList<Category> category;

        /// <summary>
        /// Gets or sets the list of selected <see cref="Category"/>s
        /// </summary>
        public ReactiveList<Category> Category 
        { 
            get { return this.category; } 
            set { this.RaiseAndSetIfChanged(ref this.category, value); } 
        }

        /// <summary>
        /// Gets or sets the Possible <see cref="Category"/> for <see cref="Category"/>
        /// </summary>
        public ReactiveList<Category> PossibleCategory { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a NestedElement
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateNestedElementCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a NestedElement
        /// </summary>
        public ReactiveCommand<Unit, Unit> DeleteNestedElementCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a NestedElement
        /// </summary>
        public ReactiveCommand<Unit, Unit> EditNestedElementCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a NestedElement
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectNestedElementCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            
            var canExecuteCreateNestedElementCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedNestedElementCommand = this.WhenAny(vm => vm.SelectedNestedElement, v => v.Value != null);
            var canExecuteEditSelectedNestedElementCommand = this.WhenAny(vm => vm.SelectedNestedElement, v => v.Value != null && !this.IsReadOnly);

            this.CreateNestedElementCommand = ReactiveCommandCreator.Create(canExecuteCreateNestedElementCommand);
            this.CreateNestedElementCommand.Subscribe(_ => this.ExecuteCreateCommand<NestedElement>(this.PopulateNestedElement));

            this.DeleteNestedElementCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedNestedElementCommand);
            this.DeleteNestedElementCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedNestedElement.Thing, this.PopulateNestedElement));

            this.EditNestedElementCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedNestedElementCommand);
            this.EditNestedElementCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedNestedElement.Thing, this.PopulateNestedElement));

            this.InspectNestedElementCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedNestedElementCommand);
            this.InspectNestedElementCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedNestedElement.Thing));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.Category.Clear();
            clone.Category.AddRange(this.Category);

        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.NestedElement = new ReactiveList<NestedElementRowViewModel>();
            this.Category = new ReactiveList<Category>();
            this.PossibleCategory = new ReactiveList<Category>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.PopulateNestedElement();
            this.PopulateCategory();
        }

        /// <summary>
        /// Populates the <see cref="Category"/> property
        /// </summary>
        protected virtual void PopulateCategory()
        {
            this.Category.Clear();

            foreach (var value in this.Thing.Category)
            {
                this.Category.Add(value);
            }
        } 

        /// <summary>
        /// Populates the <see cref="NestedElement"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateNestedElement()
        {
            this.NestedElement.Clear();
            foreach (var thing in this.Thing.NestedElement.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new NestedElementRowViewModel(thing, this.Session, this);
                this.NestedElement.Add(row);
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
            foreach(var nestedElement in this.NestedElement)
            {
                nestedElement.Dispose();
            }
        }
    }
}
