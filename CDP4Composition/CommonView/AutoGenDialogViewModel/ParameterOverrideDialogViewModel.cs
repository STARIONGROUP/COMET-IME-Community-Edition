// -------------------------------------------------------------------------------------------------
// <copyright file="ParameterOverrideDialogViewModel.cs" company="RHEA System S.A.">
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
    using System.Reactive;

    /// <summary>
    /// dialog-view-model class representing a <see cref="ParameterOverride"/>
    /// </summary>
    public partial class ParameterOverrideDialogViewModel : ParameterOrOverrideBaseDialogViewModel<ParameterOverride>
    {
        /// <summary>
        /// Backing field for <see cref="SelectedParameter"/>
        /// </summary>
        private Parameter selectedParameter;

        /// <summary>
        /// Backing field for <see cref="SelectedValueSet"/>
        /// </summary>
        private ParameterOverrideValueSetRowViewModel selectedValueSet;


        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterOverrideDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public ParameterOverrideDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterOverrideDialogViewModel"/> class
        /// </summary>
        /// <param name="parameterOverride">
        /// The <see cref="ParameterOverride"/> that is the subject of the current view-model. This is the object
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
        public ParameterOverrideDialogViewModel(ParameterOverride parameterOverride, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(parameterOverride, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            if(container != null)
            {
                var containerThing = container as ElementUsage;
                if(containerThing == null)
                {
                    var errorMessage =
                        string.Format(
                            "The container parameter is of type {0}, it shall be of type ElementUsage",
                            container.GetType());
                    throw new ArgumentException(errorMessage);
                }
            }
        }

        /// <summary>
        /// Gets or sets the SelectedParameter
        /// </summary>
        public virtual Parameter SelectedParameter
        {
            get { return this.selectedParameter; }
            set { this.RaiseAndSetIfChanged(ref this.selectedParameter, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="Parameter"/>s for <see cref="SelectedParameter"/>
        /// </summary>
        public ReactiveList<Parameter> PossibleParameter { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="ParameterOverrideValueSetRowViewModel"/>
        /// </summary>
        public ParameterOverrideValueSetRowViewModel SelectedValueSet
        {
            get { return this.selectedValueSet; }
            set { this.RaiseAndSetIfChanged(ref this.selectedValueSet, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="ParameterOverrideValueSet"/>
        /// </summary>
        public ReactiveList<ParameterOverrideValueSetRowViewModel> ValueSet { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedParameter"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectSelectedParameterCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a ParameterOverrideValueSet
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateValueSetCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a ParameterOverrideValueSet
        /// </summary>
        public ReactiveCommand<Unit, Unit> DeleteValueSetCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a ParameterOverrideValueSet
        /// </summary>
        public ReactiveCommand<Unit, Unit> EditValueSetCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a ParameterOverrideValueSet
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectValueSetCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            
            var canExecuteCreateValueSetCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedValueSetCommand = this.WhenAny(vm => vm.SelectedValueSet, v => v.Value != null);
            var canExecuteEditSelectedValueSetCommand = this.WhenAny(vm => vm.SelectedValueSet, v => v.Value != null && !this.IsReadOnly);

            this.CreateValueSetCommand = ReactiveCommandCreator.Create(canExecuteCreateValueSetCommand);
            this.CreateValueSetCommand.Subscribe(_ => this.ExecuteCreateCommand<ParameterOverrideValueSet>(this.PopulateValueSet));

            this.DeleteValueSetCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedValueSetCommand);
            this.DeleteValueSetCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedValueSet.Thing, this.PopulateValueSet));

            this.EditValueSetCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedValueSetCommand);
            this.EditValueSetCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedValueSet.Thing, this.PopulateValueSet));

            this.InspectValueSetCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedValueSetCommand);
            this.InspectValueSetCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedValueSet.Thing));
            var canExecuteInspectSelectedParameterCommand = this.WhenAny(vm => vm.SelectedParameter, v => v.Value != null);
            this.InspectSelectedParameterCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedParameterCommand);
            this.InspectSelectedParameterCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedParameter));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.Parameter = this.SelectedParameter;
        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PossibleParameter = new ReactiveList<Parameter>();
            this.ValueSet = new ReactiveList<ParameterOverrideValueSetRowViewModel>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.SelectedParameter = this.Thing.Parameter;
            this.PopulatePossibleParameter();
            this.PopulateValueSet();
        }

        /// <summary>
        /// Populates the <see cref="ValueSet"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateValueSet()
        {
            this.ValueSet.Clear();
            foreach (var thing in this.Thing.ValueSet.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new ParameterOverrideValueSetRowViewModel(thing, this.Session, this);
                this.ValueSet.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="PossibleParameter"/> property
        /// </summary>
        protected virtual void PopulatePossibleParameter()
        {
            this.PossibleParameter.Clear();
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
            foreach(var valueSet in this.ValueSet)
            {
                valueSet.Dispose();
            }
        }
    }
}
