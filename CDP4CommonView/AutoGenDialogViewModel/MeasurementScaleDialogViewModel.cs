// -------------------------------------------------------------------------------------------------
// <copyright file="MeasurementScaleDialogViewModel.cs" company="RHEA System S.A.">
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
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Operations;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Permission;
    using ReactiveUI;

    /// <summary>
    /// dialog-view-model class representing a <see cref="MeasurementScale"/>
    /// </summary>
    public abstract partial class MeasurementScaleDialogViewModel<T> : DefinedThingDialogViewModel<T> where T : MeasurementScale
    {
        /// <summary>
        /// Backing field for <see cref="NumberSet"/>
        /// </summary>
        private NumberSetKind numberSet;

        /// <summary>
        /// Backing field for <see cref="MinimumPermissibleValue"/>
        /// </summary>
        private string minimumPermissibleValue;

        /// <summary>
        /// Backing field for <see cref="IsMinimumInclusive"/>
        /// </summary>
        private bool isMinimumInclusive;

        /// <summary>
        /// Backing field for <see cref="MaximumPermissibleValue"/>
        /// </summary>
        private string maximumPermissibleValue;

        /// <summary>
        /// Backing field for <see cref="IsMaximumInclusive"/>
        /// </summary>
        private bool isMaximumInclusive;

        /// <summary>
        /// Backing field for <see cref="PositiveValueConnotation"/>
        /// </summary>
        private string positiveValueConnotation;

        /// <summary>
        /// Backing field for <see cref="NegativeValueConnotation"/>
        /// </summary>
        private string negativeValueConnotation;

        /// <summary>
        /// Backing field for <see cref="IsDeprecated"/>
        /// </summary>
        private bool isDeprecated;

        /// <summary>
        /// Backing field for <see cref="SelectedUnit"/>
        /// </summary>
        private MeasurementUnit selectedUnit;

        /// <summary>
        /// Backing field for <see cref="SelectedValueDefinition"/>
        /// </summary>
        private ScaleValueDefinitionRowViewModel selectedValueDefinition;

        /// <summary>
        /// Backing field for <see cref="SelectedMappingToReferenceScale"/>
        /// </summary>
        private MappingToReferenceScaleRowViewModel selectedMappingToReferenceScale;


        /// <summary>
        /// Initializes a new instance of the <see cref="MeasurementScaleDialogViewModel{T}"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        protected MeasurementScaleDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MeasurementScaleDialogViewModel{T}"/> class
        /// </summary>
        /// <param name="measurementScale">
        /// The <see cref="MeasurementScale"/> that is the subject of the current view-model. This is the object
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
        protected MeasurementScaleDialogViewModel(T measurementScale, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(measurementScale, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
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
        /// Gets or sets the NumberSet
        /// </summary>
        public virtual NumberSetKind NumberSet
        {
            get { return this.numberSet; }
            set { this.RaiseAndSetIfChanged(ref this.numberSet, value); }
        }

        /// <summary>
        /// Gets or sets the MinimumPermissibleValue
        /// </summary>
        public virtual string MinimumPermissibleValue
        {
            get { return this.minimumPermissibleValue; }
            set { this.RaiseAndSetIfChanged(ref this.minimumPermissibleValue, value); }
        }

        /// <summary>
        /// Gets or sets the IsMinimumInclusive
        /// </summary>
        public virtual bool IsMinimumInclusive
        {
            get { return this.isMinimumInclusive; }
            set { this.RaiseAndSetIfChanged(ref this.isMinimumInclusive, value); }
        }

        /// <summary>
        /// Gets or sets the MaximumPermissibleValue
        /// </summary>
        public virtual string MaximumPermissibleValue
        {
            get { return this.maximumPermissibleValue; }
            set { this.RaiseAndSetIfChanged(ref this.maximumPermissibleValue, value); }
        }

        /// <summary>
        /// Gets or sets the IsMaximumInclusive
        /// </summary>
        public virtual bool IsMaximumInclusive
        {
            get { return this.isMaximumInclusive; }
            set { this.RaiseAndSetIfChanged(ref this.isMaximumInclusive, value); }
        }

        /// <summary>
        /// Gets or sets the PositiveValueConnotation
        /// </summary>
        public virtual string PositiveValueConnotation
        {
            get { return this.positiveValueConnotation; }
            set { this.RaiseAndSetIfChanged(ref this.positiveValueConnotation, value); }
        }

        /// <summary>
        /// Gets or sets the NegativeValueConnotation
        /// </summary>
        public virtual string NegativeValueConnotation
        {
            get { return this.negativeValueConnotation; }
            set { this.RaiseAndSetIfChanged(ref this.negativeValueConnotation, value); }
        }

        /// <summary>
        /// Gets or sets the IsDeprecated
        /// </summary>
        public virtual bool IsDeprecated
        {
            get { return this.isDeprecated; }
            set { this.RaiseAndSetIfChanged(ref this.isDeprecated, value); }
        }

        /// <summary>
        /// Gets or sets the SelectedUnit
        /// </summary>
        public virtual MeasurementUnit SelectedUnit
        {
            get { return this.selectedUnit; }
            set { this.RaiseAndSetIfChanged(ref this.selectedUnit, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="MeasurementUnit"/>s for <see cref="SelectedUnit"/>
        /// </summary>
        public ReactiveList<MeasurementUnit> PossibleUnit { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="ScaleValueDefinitionRowViewModel"/>
        /// </summary>
        public ScaleValueDefinitionRowViewModel SelectedValueDefinition
        {
            get { return this.selectedValueDefinition; }
            set { this.RaiseAndSetIfChanged(ref this.selectedValueDefinition, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="ScaleValueDefinition"/>
        /// </summary>
        public ReactiveList<ScaleValueDefinitionRowViewModel> ValueDefinition { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="MappingToReferenceScaleRowViewModel"/>
        /// </summary>
        public MappingToReferenceScaleRowViewModel SelectedMappingToReferenceScale
        {
            get { return this.selectedMappingToReferenceScale; }
            set { this.RaiseAndSetIfChanged(ref this.selectedMappingToReferenceScale, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="MappingToReferenceScale"/>
        /// </summary>
        public ReactiveList<MappingToReferenceScaleRowViewModel> MappingToReferenceScale { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedUnit"/>
        /// </summary>
        public ReactiveCommand<object> InspectSelectedUnitCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a ScaleValueDefinition
        /// </summary>
        public ReactiveCommand<object> CreateValueDefinitionCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a ScaleValueDefinition
        /// </summary>
        public ReactiveCommand<object> DeleteValueDefinitionCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a ScaleValueDefinition
        /// </summary>
        public ReactiveCommand<object> EditValueDefinitionCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a ScaleValueDefinition
        /// </summary>
        public ReactiveCommand<object> InspectValueDefinitionCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a MappingToReferenceScale
        /// </summary>
        public ReactiveCommand<object> CreateMappingToReferenceScaleCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a MappingToReferenceScale
        /// </summary>
        public ReactiveCommand<object> DeleteMappingToReferenceScaleCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a MappingToReferenceScale
        /// </summary>
        public ReactiveCommand<object> EditMappingToReferenceScaleCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a MappingToReferenceScale
        /// </summary>
        public ReactiveCommand<object> InspectMappingToReferenceScaleCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            
            var canExecuteCreateValueDefinitionCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedValueDefinitionCommand = this.WhenAny(vm => vm.SelectedValueDefinition, v => v.Value != null);
            var canExecuteEditSelectedValueDefinitionCommand = this.WhenAny(vm => vm.SelectedValueDefinition, v => v.Value != null && !this.IsReadOnly);

            this.CreateValueDefinitionCommand = ReactiveCommand.Create(canExecuteCreateValueDefinitionCommand);
            this.CreateValueDefinitionCommand.Subscribe(_ => this.ExecuteCreateCommand<ScaleValueDefinition>(this.PopulateValueDefinition));

            this.DeleteValueDefinitionCommand = ReactiveCommand.Create(canExecuteEditSelectedValueDefinitionCommand);
            this.DeleteValueDefinitionCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedValueDefinition.Thing, this.PopulateValueDefinition));

            this.EditValueDefinitionCommand = ReactiveCommand.Create(canExecuteEditSelectedValueDefinitionCommand);
            this.EditValueDefinitionCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedValueDefinition.Thing, this.PopulateValueDefinition));

            this.InspectValueDefinitionCommand = ReactiveCommand.Create(canExecuteInspectSelectedValueDefinitionCommand);
            this.InspectValueDefinitionCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedValueDefinition.Thing));
            
            var canExecuteCreateMappingToReferenceScaleCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedMappingToReferenceScaleCommand = this.WhenAny(vm => vm.SelectedMappingToReferenceScale, v => v.Value != null);
            var canExecuteEditSelectedMappingToReferenceScaleCommand = this.WhenAny(vm => vm.SelectedMappingToReferenceScale, v => v.Value != null && !this.IsReadOnly);

            this.CreateMappingToReferenceScaleCommand = ReactiveCommand.Create(canExecuteCreateMappingToReferenceScaleCommand);
            this.CreateMappingToReferenceScaleCommand.Subscribe(_ => this.ExecuteCreateCommand<MappingToReferenceScale>(this.PopulateMappingToReferenceScale));

            this.DeleteMappingToReferenceScaleCommand = ReactiveCommand.Create(canExecuteEditSelectedMappingToReferenceScaleCommand);
            this.DeleteMappingToReferenceScaleCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedMappingToReferenceScale.Thing, this.PopulateMappingToReferenceScale));

            this.EditMappingToReferenceScaleCommand = ReactiveCommand.Create(canExecuteEditSelectedMappingToReferenceScaleCommand);
            this.EditMappingToReferenceScaleCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedMappingToReferenceScale.Thing, this.PopulateMappingToReferenceScale));

            this.InspectMappingToReferenceScaleCommand = ReactiveCommand.Create(canExecuteInspectSelectedMappingToReferenceScaleCommand);
            this.InspectMappingToReferenceScaleCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedMappingToReferenceScale.Thing));
            var canExecuteInspectSelectedUnitCommand = this.WhenAny(vm => vm.SelectedUnit, v => v.Value != null);
            this.InspectSelectedUnitCommand = ReactiveCommand.Create(canExecuteInspectSelectedUnitCommand);
            this.InspectSelectedUnitCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedUnit));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.NumberSet = this.NumberSet;
            clone.MinimumPermissibleValue = this.MinimumPermissibleValue;
            clone.IsMinimumInclusive = this.IsMinimumInclusive;
            clone.MaximumPermissibleValue = this.MaximumPermissibleValue;
            clone.IsMaximumInclusive = this.IsMaximumInclusive;
            clone.PositiveValueConnotation = this.PositiveValueConnotation;
            clone.NegativeValueConnotation = this.NegativeValueConnotation;
            clone.IsDeprecated = this.IsDeprecated;
            clone.Unit = this.SelectedUnit;
        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PossibleUnit = new ReactiveList<MeasurementUnit>();
            this.ValueDefinition = new ReactiveList<ScaleValueDefinitionRowViewModel>();
            this.MappingToReferenceScale = new ReactiveList<MappingToReferenceScaleRowViewModel>();
            this.PopulatePossibleContainer();
        }

        /// <summary>
        /// Populate the possible containers for a <see cref="MeasurementScale"/>
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
            this.NumberSet = this.Thing.NumberSet;
            this.MinimumPermissibleValue = this.Thing.MinimumPermissibleValue;
            this.IsMinimumInclusive = this.Thing.IsMinimumInclusive;
            this.MaximumPermissibleValue = this.Thing.MaximumPermissibleValue;
            this.IsMaximumInclusive = this.Thing.IsMaximumInclusive;
            this.PositiveValueConnotation = this.Thing.PositiveValueConnotation;
            this.NegativeValueConnotation = this.Thing.NegativeValueConnotation;
            this.IsDeprecated = this.Thing.IsDeprecated;
            this.SelectedUnit = this.Thing.Unit;
            this.PopulatePossibleUnit();
            this.PopulateValueDefinition();
            this.PopulateMappingToReferenceScale();
        }

        /// <summary>
        /// Populates the <see cref="ValueDefinition"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateValueDefinition()
        {
            this.ValueDefinition.Clear();
            foreach (var thing in this.Thing.ValueDefinition.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new ScaleValueDefinitionRowViewModel(thing, this.Session, this);
                this.ValueDefinition.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="MappingToReferenceScale"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateMappingToReferenceScale()
        {
            this.MappingToReferenceScale.Clear();
            foreach (var thing in this.Thing.MappingToReferenceScale.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new MappingToReferenceScaleRowViewModel(thing, this.Session, this);
                this.MappingToReferenceScale.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="PossibleUnit"/> property
        /// </summary>
        protected virtual void PopulatePossibleUnit()
        {
            this.PossibleUnit.Clear();
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
            foreach(var valueDefinition in this.ValueDefinition)
            {
                valueDefinition.Dispose();
            }
            foreach(var mappingToReferenceScale in this.MappingToReferenceScale)
            {
                mappingToReferenceScale.Dispose();
            }
        }
    }
}
