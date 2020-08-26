// -------------------------------------------------------------------------------------------------
// <copyright file="ConstantDialogViewModel.cs" company="RHEA System S.A.">
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
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
	using CDP4Dal.Operations;
    using CDP4Dal.Permission;
    using ReactiveUI;

    /// <summary>
    /// dialog-view-model class representing a <see cref="Constant"/>
    /// </summary>
    public partial class ConstantDialogViewModel : DefinedThingDialogViewModel<Constant>
    {
        /// <summary>
        /// Backing field for <see cref="IsDeprecated"/>
        /// </summary>
        private bool isDeprecated;

        /// <summary>
        /// Backing field for <see cref="SelectedParameterType"/>
        /// </summary>
        private ParameterType selectedParameterType;

        /// <summary>
        /// Backing field for <see cref="SelectedScale"/>
        /// </summary>
        private MeasurementScale selectedScale;


        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public ConstantDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantDialogViewModel"/> class
        /// </summary>
        /// <param name="constant">
        /// The <see cref="Constant"/> that is the subject of the current view-model. This is the object
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
        public ConstantDialogViewModel(Constant constant, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(constant, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
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
        /// Gets or sets the IsDeprecated
        /// </summary>
        public virtual bool IsDeprecated
        {
            get { return this.isDeprecated; }
            set { this.RaiseAndSetIfChanged(ref this.isDeprecated, value); }
        }

        /// <summary>
        /// Backing field for Value
        /// </summary>
        public ReactiveList<PrimitiveRow<string>> value;

        /// <summary>
        /// Gets or sets the Value
        /// </summary>
        public ReactiveList<PrimitiveRow<string>> Value
        {
            get { return this.value; }
            set { this.RaiseAndSetIfChanged(ref this.value, value); }
        }

        /// <summary>
        /// Gets or sets the SelectedParameterType
        /// </summary>
        public virtual ParameterType SelectedParameterType
        {
            get { return this.selectedParameterType; }
            set { this.RaiseAndSetIfChanged(ref this.selectedParameterType, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="ParameterType"/>s for <see cref="SelectedParameterType"/>
        /// </summary>
        public ReactiveList<ParameterType> PossibleParameterType { get; protected set; }

        /// <summary>
        /// Gets or sets the SelectedScale
        /// </summary>
        public virtual MeasurementScale SelectedScale
        {
            get { return this.selectedScale; }
            set { this.RaiseAndSetIfChanged(ref this.selectedScale, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="MeasurementScale"/>s for <see cref="SelectedScale"/>
        /// </summary>
        public ReactiveList<MeasurementScale> PossibleScale { get; protected set; }
        
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
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedParameterType"/>
        /// </summary>
        public ReactiveCommand<object> InspectSelectedParameterTypeCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedScale"/>
        /// </summary>
        public ReactiveCommand<object> InspectSelectedScaleCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            var canExecuteInspectSelectedParameterTypeCommand = this.WhenAny(vm => vm.SelectedParameterType, v => v.Value != null);
            this.InspectSelectedParameterTypeCommand = ReactiveCommand.Create(canExecuteInspectSelectedParameterTypeCommand);
            this.InspectSelectedParameterTypeCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedParameterType));
            var canExecuteInspectSelectedScaleCommand = this.WhenAny(vm => vm.SelectedScale, v => v.Value != null);
            this.InspectSelectedScaleCommand = ReactiveCommand.Create(canExecuteInspectSelectedScaleCommand);
            this.InspectSelectedScaleCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedScale));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.IsDeprecated = this.IsDeprecated;
            clone.Value = new ValueArray<string>(this.Value.OrderBy(x => x.Index).Select(x => x.Value), this.Thing);
 
            clone.ParameterType = this.SelectedParameterType;
            clone.Scale = this.SelectedScale;
            clone.Category.Clear();
            clone.Category.AddRange(this.Category);

        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.Value = new ReactiveList<PrimitiveRow<string>>();
            this.PossibleParameterType = new ReactiveList<ParameterType>();
            this.PossibleScale = new ReactiveList<MeasurementScale>();
            this.Category = new ReactiveList<Category>();
            this.PossibleCategory = new ReactiveList<Category>();
            this.PopulatePossibleContainer();
        }

        /// <summary>
        /// Populate the possible containers for a <see cref="Constant"/>
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
            this.IsDeprecated = this.Thing.IsDeprecated;
            this.PopulateValue();
            this.SelectedParameterType = this.Thing.ParameterType;
            this.PopulatePossibleParameterType();
            this.SelectedScale = this.Thing.Scale;
            this.PopulatePossibleScale();
            this.PopulateCategory();
        }

        /// <summary>
        /// Populates the <see cref="Value"/> property
        /// </summary>
        protected virtual void PopulateValue()
        {
            this.Value.Clear();
            foreach(var value in this.Thing.Value)
            {
                this.Value.Add(new PrimitiveRow<string> { Index = this.Value.Count, Value = value });
            }
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
        /// Populates the <see cref="PossibleParameterType"/> property
        /// </summary>
        protected virtual void PopulatePossibleParameterType()
        {
            this.PossibleParameterType.Clear();
        }

        /// <summary>
        /// Populates the <see cref="PossibleScale"/> property
        /// </summary>
        protected virtual void PopulatePossibleScale()
        {
            this.PossibleScale.Clear();
        }
    }
}
