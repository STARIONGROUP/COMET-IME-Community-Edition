// -------------------------------------------------------------------------------------------------
// <copyright file="ElementUsageDialogViewModel.cs" company="RHEA System S.A.">
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
    
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
	using CDP4Dal.Operations;
    using CDP4Dal.Permission;
    using ReactiveUI;

    /// <summary>
    /// dialog-view-model class representing a <see cref="ElementUsage"/>
    /// </summary>
    public partial class ElementUsageDialogViewModel : ElementBaseDialogViewModel<ElementUsage>
    {
        /// <summary>
        /// Backing field for <see cref="InterfaceEnd"/>
        /// </summary>
        private InterfaceEndKind interfaceEnd;

        /// <summary>
        /// Backing field for <see cref="SelectedElementDefinition"/>
        /// </summary>
        private ElementDefinition selectedElementDefinition;

        /// <summary>
        /// Backing field for <see cref="SelectedParameterOverride"/>
        /// </summary>
        private ParameterOverrideRowViewModel selectedParameterOverride;


        /// <summary>
        /// Initializes a new instance of the <see cref="ElementUsageDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public ElementUsageDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementUsageDialogViewModel"/> class
        /// </summary>
        /// <param name="elementUsage">
        /// The <see cref="ElementUsage"/> that is the subject of the current view-model. This is the object
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
        public ElementUsageDialogViewModel(ElementUsage elementUsage, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(elementUsage, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            if(container != null)
            {
                var containerThing = container as ElementDefinition;
                if(containerThing == null)
                {
                    var errorMessage =
                        string.Format(
                            "The container parameter is of type {0}, it shall be of type ElementDefinition",
                            container.GetType());
                    throw new ArgumentException(errorMessage);
                }
            }
        }

        /// <summary>
        /// Gets or sets the InterfaceEnd
        /// </summary>
        public virtual InterfaceEndKind InterfaceEnd
        {
            get { return this.interfaceEnd; }
            set { this.RaiseAndSetIfChanged(ref this.interfaceEnd, value); }
        }

        /// <summary>
        /// Gets or sets the SelectedElementDefinition
        /// </summary>
        public virtual ElementDefinition SelectedElementDefinition
        {
            get { return this.selectedElementDefinition; }
            set { this.RaiseAndSetIfChanged(ref this.selectedElementDefinition, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="ElementDefinition"/>s for <see cref="SelectedElementDefinition"/>
        /// </summary>
        public ReactiveList<ElementDefinition> PossibleElementDefinition { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="ParameterOverrideRowViewModel"/>
        /// </summary>
        public ParameterOverrideRowViewModel SelectedParameterOverride
        {
            get { return this.selectedParameterOverride; }
            set { this.RaiseAndSetIfChanged(ref this.selectedParameterOverride, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="ParameterOverride"/>
        /// </summary>
        public ReactiveList<ParameterOverrideRowViewModel> ParameterOverride { get; protected set; }
        
        /// <summary>
        /// Backing field for <see cref="ExcludeOption"/>s
        /// </summary>
        private ReactiveList<Option> excludeOption;

        /// <summary>
        /// Gets or sets the list of selected <see cref="Option"/>s
        /// </summary>
        public ReactiveList<Option> ExcludeOption 
        { 
            get { return this.excludeOption; } 
            set { this.RaiseAndSetIfChanged(ref this.excludeOption, value); } 
        }

        /// <summary>
        /// Gets or sets the Possible <see cref="Option"/> for <see cref="ExcludeOption"/>
        /// </summary>
        public ReactiveList<Option> PossibleExcludeOption { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedElementDefinition"/>
        /// </summary>
        public ReactiveCommand<object> InspectSelectedElementDefinitionCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a ParameterOverride
        /// </summary>
        public ReactiveCommand<object> CreateParameterOverrideCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a ParameterOverride
        /// </summary>
        public ReactiveCommand<object> DeleteParameterOverrideCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a ParameterOverride
        /// </summary>
        public ReactiveCommand<object> EditParameterOverrideCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a ParameterOverride
        /// </summary>
        public ReactiveCommand<object> InspectParameterOverrideCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            
            var canExecuteCreateParameterOverrideCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedParameterOverrideCommand = this.WhenAny(vm => vm.SelectedParameterOverride, v => v.Value != null);
            var canExecuteEditSelectedParameterOverrideCommand = this.WhenAny(vm => vm.SelectedParameterOverride, v => v.Value != null && !this.IsReadOnly);

            this.CreateParameterOverrideCommand = ReactiveCommand.Create(canExecuteCreateParameterOverrideCommand);
            this.CreateParameterOverrideCommand.Subscribe(_ => this.ExecuteCreateCommand<ParameterOverride>(this.PopulateParameterOverride));

            this.DeleteParameterOverrideCommand = ReactiveCommand.Create(canExecuteEditSelectedParameterOverrideCommand);
            this.DeleteParameterOverrideCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedParameterOverride.Thing, this.PopulateParameterOverride));

            this.EditParameterOverrideCommand = ReactiveCommand.Create(canExecuteEditSelectedParameterOverrideCommand);
            this.EditParameterOverrideCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedParameterOverride.Thing, this.PopulateParameterOverride));

            this.InspectParameterOverrideCommand = ReactiveCommand.Create(canExecuteInspectSelectedParameterOverrideCommand);
            this.InspectParameterOverrideCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedParameterOverride.Thing));
            var canExecuteInspectSelectedElementDefinitionCommand = this.WhenAny(vm => vm.SelectedElementDefinition, v => v.Value != null);
            this.InspectSelectedElementDefinitionCommand = ReactiveCommand.Create(canExecuteInspectSelectedElementDefinitionCommand);
            this.InspectSelectedElementDefinitionCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedElementDefinition));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.InterfaceEnd = this.InterfaceEnd;
            clone.ElementDefinition = this.SelectedElementDefinition;
            clone.ExcludeOption.Clear();
            clone.ExcludeOption.AddRange(this.ExcludeOption);

        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PossibleElementDefinition = new ReactiveList<ElementDefinition>();
            this.ParameterOverride = new ReactiveList<ParameterOverrideRowViewModel>();
            this.ExcludeOption = new ReactiveList<Option>();
            this.PossibleExcludeOption = new ReactiveList<Option>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.InterfaceEnd = this.Thing.InterfaceEnd;
            this.SelectedElementDefinition = this.Thing.ElementDefinition;
            this.PopulatePossibleElementDefinition();
            this.PopulateParameterOverride();
            this.PopulateExcludeOption();
        }

        /// <summary>
        /// Populates the <see cref="ExcludeOption"/> property
        /// </summary>
        protected virtual void PopulateExcludeOption()
        {
            this.ExcludeOption.Clear();

            foreach (var value in this.Thing.ExcludeOption)
            {
                this.ExcludeOption.Add(value);
            }
        } 

        /// <summary>
        /// Populates the <see cref="ParameterOverride"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateParameterOverride()
        {
            this.ParameterOverride.Clear();
            foreach (var thing in this.Thing.ParameterOverride.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new ParameterOverrideRowViewModel(thing, this.Session, this);
                this.ParameterOverride.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="PossibleElementDefinition"/> property
        /// </summary>
        protected virtual void PopulatePossibleElementDefinition()
        {
            this.PossibleElementDefinition.Clear();
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
            foreach(var parameterOverride in this.ParameterOverride)
            {
                parameterOverride.Dispose();
            }
        }
    }
}
