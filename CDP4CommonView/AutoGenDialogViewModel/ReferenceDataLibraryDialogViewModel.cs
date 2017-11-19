// -------------------------------------------------------------------------------------------------
// <copyright file="ReferenceDataLibraryDialogViewModel.cs" company="RHEA System S.A.">
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
    using CDP4Common.Types;
    using CDP4Common.Operations;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Permission;
    using ReactiveUI;

    /// <summary>
    /// dialog-view-model class representing a <see cref="ReferenceDataLibrary"/>
    /// </summary>
    public abstract partial class ReferenceDataLibraryDialogViewModel<T> : DefinedThingDialogViewModel<T> where T : ReferenceDataLibrary
    {
        /// <summary>
        /// Backing field for <see cref="SelectedRequiredRdl"/>
        /// </summary>
        private SiteReferenceDataLibrary selectedRequiredRdl;

        /// <summary>
        /// Backing field for <see cref="SelectedDefinedCategory"/>
        /// </summary>
        private CategoryRowViewModel selectedDefinedCategory;

        /// <summary>
        /// Backing field for <see cref="SelectedParameterType"/>
        /// </summary>
        private IRowViewModelBase<ParameterType> selectedParameterType;

        /// <summary>
        /// Backing field for <see cref="SelectedScale"/>
        /// </summary>
        private IRowViewModelBase<MeasurementScale> selectedScale;

        /// <summary>
        /// Backing field for <see cref="SelectedUnitPrefix"/>
        /// </summary>
        private UnitPrefixRowViewModel selectedUnitPrefix;

        /// <summary>
        /// Backing field for <see cref="SelectedUnit"/>
        /// </summary>
        private IRowViewModelBase<MeasurementUnit> selectedUnit;

        /// <summary>
        /// Backing field for <see cref="SelectedFileType"/>
        /// </summary>
        private FileTypeRowViewModel selectedFileType;

        /// <summary>
        /// Backing field for <see cref="SelectedGlossary"/>
        /// </summary>
        private GlossaryRowViewModel selectedGlossary;

        /// <summary>
        /// Backing field for <see cref="SelectedReferenceSource"/>
        /// </summary>
        private ReferenceSourceRowViewModel selectedReferenceSource;

        /// <summary>
        /// Backing field for <see cref="SelectedRule"/>
        /// </summary>
        private IRowViewModelBase<Rule> selectedRule;

        /// <summary>
        /// Backing field for <see cref="SelectedConstant"/>
        /// </summary>
        private ConstantRowViewModel selectedConstant;


        /// <summary>
        /// Backing field for <see cref="SelectedParameterType"/>Kind
        /// </summary>
        private ClassKind selectedParameterTypeKind;

        /// <summary>
        /// Backing field for <see cref="SelectedMeasurementScale"/>Kind
        /// </summary>
        private ClassKind selectedMeasurementScaleKind;

        /// <summary>
        /// Backing field for <see cref="SelectedMeasurementUnit"/>Kind
        /// </summary>
        private ClassKind selectedMeasurementUnitKind;

        /// <summary>
        /// Backing field for <see cref="SelectedRule"/>Kind
        /// </summary>
        private ClassKind selectedRuleKind;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceDataLibraryDialogViewModel{T}"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        protected ReferenceDataLibraryDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceDataLibraryDialogViewModel{T}"/> class
        /// </summary>
        /// <param name="referenceDataLibrary">
        /// The <see cref="ReferenceDataLibrary"/> that is the subject of the current view-model. This is the object
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
        protected ReferenceDataLibraryDialogViewModel(T referenceDataLibrary, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(referenceDataLibrary, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
        }

        /// <summary>
        /// Gets or sets the SelectedRequiredRdl
        /// </summary>
        public virtual SiteReferenceDataLibrary SelectedRequiredRdl
        {
            get { return this.selectedRequiredRdl; }
            set { this.RaiseAndSetIfChanged(ref this.selectedRequiredRdl, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="SiteReferenceDataLibrary"/>s for <see cref="SelectedRequiredRdl"/>
        /// </summary>
        public ReactiveList<SiteReferenceDataLibrary> PossibleRequiredRdl { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="CategoryRowViewModel"/>
        /// </summary>
        public CategoryRowViewModel SelectedDefinedCategory
        {
            get { return this.selectedDefinedCategory; }
            set { this.RaiseAndSetIfChanged(ref this.selectedDefinedCategory, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="Category"/>
        /// </summary>
        public ReactiveList<CategoryRowViewModel> DefinedCategory { get; protected set; }
        
        /// <summary>
        /// Gets the concrete ParameterType to create
        /// </summary>
        public ClassKind SelectedParameterTypeKind
        {
            get { return this.selectedParameterTypeKind; }
            set { this.RaiseAndSetIfChanged(ref this.selectedParameterTypeKind, value); } 
        }

        /// <summary>
        /// Gets the list of possible <see cref="ClassKind"/> for <see cref="Uml.StructuredClassifiers.Class"/>
        /// </summary>
        public readonly ReactiveList<ClassKind> PossibleParameterTypeKind = new ReactiveList<ClassKind>() 
        { 
            ClassKind.ArrayParameterType,
            ClassKind.EnumerationParameterType,
            ClassKind.BooleanParameterType,
            ClassKind.CompoundParameterType,
            ClassKind.DateParameterType,
            ClassKind.TextParameterType,
            ClassKind.SpecializedQuantityKind,
            ClassKind.SimpleQuantityKind,
            ClassKind.DateTimeParameterType,
            ClassKind.TimeOfDayParameterType,
            ClassKind.DerivedQuantityKind 
        };
        
        /// <summary>
        /// Gets or sets the selected <see cref="IRowViewModelBase{T}"/>
        /// </summary>
        public IRowViewModelBase<ParameterType> SelectedParameterType
        {
            get { return this.selectedParameterType; }
            set { this.RaiseAndSetIfChanged(ref this.selectedParameterType, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="ParameterType"/>
        /// </summary>
        public ReactiveList<IRowViewModelBase<ParameterType>> ParameterType { get; protected set; }
        
        /// <summary>
        /// Gets the concrete MeasurementScale to create
        /// </summary>
        public ClassKind SelectedMeasurementScaleKind
        {
            get { return this.selectedMeasurementScaleKind; }
            set { this.RaiseAndSetIfChanged(ref this.selectedMeasurementScaleKind, value); } 
        }

        /// <summary>
        /// Gets the list of possible <see cref="ClassKind"/> for <see cref="Uml.StructuredClassifiers.Class"/>
        /// </summary>
        public readonly ReactiveList<ClassKind> PossibleMeasurementScaleKind = new ReactiveList<ClassKind>() 
        { 
            ClassKind.CyclicRatioScale,
            ClassKind.OrdinalScale,
            ClassKind.RatioScale,
            ClassKind.IntervalScale,
            ClassKind.LogarithmicScale 
        };
        
        /// <summary>
        /// Gets or sets the selected <see cref="IRowViewModelBase{T}"/>
        /// </summary>
        public IRowViewModelBase<MeasurementScale> SelectedScale
        {
            get { return this.selectedScale; }
            set { this.RaiseAndSetIfChanged(ref this.selectedScale, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="MeasurementScale"/>
        /// </summary>
        public ReactiveList<IRowViewModelBase<MeasurementScale>> Scale { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="UnitPrefixRowViewModel"/>
        /// </summary>
        public UnitPrefixRowViewModel SelectedUnitPrefix
        {
            get { return this.selectedUnitPrefix; }
            set { this.RaiseAndSetIfChanged(ref this.selectedUnitPrefix, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="UnitPrefix"/>
        /// </summary>
        public ReactiveList<UnitPrefixRowViewModel> UnitPrefix { get; protected set; }
        
        /// <summary>
        /// Gets the concrete MeasurementUnit to create
        /// </summary>
        public ClassKind SelectedMeasurementUnitKind
        {
            get { return this.selectedMeasurementUnitKind; }
            set { this.RaiseAndSetIfChanged(ref this.selectedMeasurementUnitKind, value); } 
        }

        /// <summary>
        /// Gets the list of possible <see cref="ClassKind"/> for <see cref="Uml.StructuredClassifiers.Class"/>
        /// </summary>
        public readonly ReactiveList<ClassKind> PossibleMeasurementUnitKind = new ReactiveList<ClassKind>() 
        { 
            ClassKind.LinearConversionUnit,
            ClassKind.DerivedUnit,
            ClassKind.SimpleUnit,
            ClassKind.PrefixedUnit 
        };
        
        /// <summary>
        /// Gets or sets the selected <see cref="IRowViewModelBase{T}"/>
        /// </summary>
        public IRowViewModelBase<MeasurementUnit> SelectedUnit
        {
            get { return this.selectedUnit; }
            set { this.RaiseAndSetIfChanged(ref this.selectedUnit, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="MeasurementUnit"/>
        /// </summary>
        public ReactiveList<IRowViewModelBase<MeasurementUnit>> Unit { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="FileTypeRowViewModel"/>
        /// </summary>
        public FileTypeRowViewModel SelectedFileType
        {
            get { return this.selectedFileType; }
            set { this.RaiseAndSetIfChanged(ref this.selectedFileType, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="FileType"/>
        /// </summary>
        public ReactiveList<FileTypeRowViewModel> FileType { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="GlossaryRowViewModel"/>
        /// </summary>
        public GlossaryRowViewModel SelectedGlossary
        {
            get { return this.selectedGlossary; }
            set { this.RaiseAndSetIfChanged(ref this.selectedGlossary, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="Glossary"/>
        /// </summary>
        public ReactiveList<GlossaryRowViewModel> Glossary { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="ReferenceSourceRowViewModel"/>
        /// </summary>
        public ReferenceSourceRowViewModel SelectedReferenceSource
        {
            get { return this.selectedReferenceSource; }
            set { this.RaiseAndSetIfChanged(ref this.selectedReferenceSource, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="ReferenceSource"/>
        /// </summary>
        public ReactiveList<ReferenceSourceRowViewModel> ReferenceSource { get; protected set; }
        
        /// <summary>
        /// Gets the concrete Rule to create
        /// </summary>
        public ClassKind SelectedRuleKind
        {
            get { return this.selectedRuleKind; }
            set { this.RaiseAndSetIfChanged(ref this.selectedRuleKind, value); } 
        }

        /// <summary>
        /// Gets the list of possible <see cref="ClassKind"/> for <see cref="Uml.StructuredClassifiers.Class"/>
        /// </summary>
        public readonly ReactiveList<ClassKind> PossibleRuleKind = new ReactiveList<ClassKind>() 
        { 
            ClassKind.ReferencerRule,
            ClassKind.BinaryRelationshipRule,
            ClassKind.MultiRelationshipRule,
            ClassKind.DecompositionRule,
            ClassKind.ParameterizedCategoryRule 
        };
        
        /// <summary>
        /// Gets or sets the selected <see cref="IRowViewModelBase{T}"/>
        /// </summary>
        public IRowViewModelBase<Rule> SelectedRule
        {
            get { return this.selectedRule; }
            set { this.RaiseAndSetIfChanged(ref this.selectedRule, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="Rule"/>
        /// </summary>
        public ReactiveList<IRowViewModelBase<Rule>> Rule { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="ConstantRowViewModel"/>
        /// </summary>
        public ConstantRowViewModel SelectedConstant
        {
            get { return this.selectedConstant; }
            set { this.RaiseAndSetIfChanged(ref this.selectedConstant, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="Constant"/>
        /// </summary>
        public ReactiveList<ConstantRowViewModel> Constant { get; protected set; }
        
        /// <summary>
        /// Backing field for <see cref="BaseQuantityKind"/>s
        /// </summary>
        private ReactiveList<QuantityKind> baseQuantityKind;

        /// <summary>
        /// Gets or sets the list of selected <see cref="QuantityKind"/>s
        /// </summary>
        public ReactiveList<QuantityKind> BaseQuantityKind 
        { 
            get { return this.baseQuantityKind; } 
            set { this.RaiseAndSetIfChanged(ref this.baseQuantityKind, value); } 
        }

        /// <summary>
        /// Gets or sets the Possible <see cref="QuantityKind"/> for <see cref="BaseQuantityKind"/>
        /// </summary>
        public ReactiveList<QuantityKind> PossibleBaseQuantityKind { get; protected set; }
        
        /// <summary>
        /// Backing field for <see cref="BaseUnit"/>s
        /// </summary>
        private ReactiveList<MeasurementUnit> baseUnit;

        /// <summary>
        /// Gets or sets the list of selected <see cref="MeasurementUnit"/>s
        /// </summary>
        public ReactiveList<MeasurementUnit> BaseUnit 
        { 
            get { return this.baseUnit; } 
            set { this.RaiseAndSetIfChanged(ref this.baseUnit, value); } 
        }

        /// <summary>
        /// Gets or sets the Possible <see cref="MeasurementUnit"/> for <see cref="BaseUnit"/>
        /// </summary>
        public ReactiveList<MeasurementUnit> PossibleBaseUnit { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedRequiredRdl"/>
        /// </summary>
        public ReactiveCommand<object> InspectSelectedRequiredRdlCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a Category
        /// </summary>
        public ReactiveCommand<object> CreateDefinedCategoryCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a Category
        /// </summary>
        public ReactiveCommand<object> DeleteDefinedCategoryCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a Category
        /// </summary>
        public ReactiveCommand<object> EditDefinedCategoryCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a Category
        /// </summary>
        public ReactiveCommand<object> InspectDefinedCategoryCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a ParameterType
        /// </summary>
        public ReactiveCommand<object> CreateParameterTypeCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a ParameterType
        /// </summary>
        public ReactiveCommand<object> DeleteParameterTypeCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a ParameterType
        /// </summary>
        public ReactiveCommand<object> EditParameterTypeCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a ParameterType
        /// </summary>
        public ReactiveCommand<object> InspectParameterTypeCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a MeasurementScale
        /// </summary>
        public ReactiveCommand<object> CreateScaleCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a MeasurementScale
        /// </summary>
        public ReactiveCommand<object> DeleteScaleCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a MeasurementScale
        /// </summary>
        public ReactiveCommand<object> EditScaleCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a MeasurementScale
        /// </summary>
        public ReactiveCommand<object> InspectScaleCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a UnitPrefix
        /// </summary>
        public ReactiveCommand<object> CreateUnitPrefixCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a UnitPrefix
        /// </summary>
        public ReactiveCommand<object> DeleteUnitPrefixCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a UnitPrefix
        /// </summary>
        public ReactiveCommand<object> EditUnitPrefixCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a UnitPrefix
        /// </summary>
        public ReactiveCommand<object> InspectUnitPrefixCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a MeasurementUnit
        /// </summary>
        public ReactiveCommand<object> CreateUnitCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a MeasurementUnit
        /// </summary>
        public ReactiveCommand<object> DeleteUnitCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a MeasurementUnit
        /// </summary>
        public ReactiveCommand<object> EditUnitCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a MeasurementUnit
        /// </summary>
        public ReactiveCommand<object> InspectUnitCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a FileType
        /// </summary>
        public ReactiveCommand<object> CreateFileTypeCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a FileType
        /// </summary>
        public ReactiveCommand<object> DeleteFileTypeCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a FileType
        /// </summary>
        public ReactiveCommand<object> EditFileTypeCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a FileType
        /// </summary>
        public ReactiveCommand<object> InspectFileTypeCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a Glossary
        /// </summary>
        public ReactiveCommand<object> CreateGlossaryCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a Glossary
        /// </summary>
        public ReactiveCommand<object> DeleteGlossaryCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a Glossary
        /// </summary>
        public ReactiveCommand<object> EditGlossaryCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a Glossary
        /// </summary>
        public ReactiveCommand<object> InspectGlossaryCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a ReferenceSource
        /// </summary>
        public ReactiveCommand<object> CreateReferenceSourceCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a ReferenceSource
        /// </summary>
        public ReactiveCommand<object> DeleteReferenceSourceCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a ReferenceSource
        /// </summary>
        public ReactiveCommand<object> EditReferenceSourceCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a ReferenceSource
        /// </summary>
        public ReactiveCommand<object> InspectReferenceSourceCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a Rule
        /// </summary>
        public ReactiveCommand<object> CreateRuleCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a Rule
        /// </summary>
        public ReactiveCommand<object> DeleteRuleCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a Rule
        /// </summary>
        public ReactiveCommand<object> EditRuleCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a Rule
        /// </summary>
        public ReactiveCommand<object> InspectRuleCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a Constant
        /// </summary>
        public ReactiveCommand<object> CreateConstantCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a Constant
        /// </summary>
        public ReactiveCommand<object> DeleteConstantCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a Constant
        /// </summary>
        public ReactiveCommand<object> EditConstantCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a Constant
        /// </summary>
        public ReactiveCommand<object> InspectConstantCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            
            var canExecuteCreateDefinedCategoryCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedDefinedCategoryCommand = this.WhenAny(vm => vm.SelectedDefinedCategory, v => v.Value != null);
            var canExecuteEditSelectedDefinedCategoryCommand = this.WhenAny(vm => vm.SelectedDefinedCategory, v => v.Value != null && !this.IsReadOnly);

            this.CreateDefinedCategoryCommand = ReactiveCommand.Create(canExecuteCreateDefinedCategoryCommand);
            this.CreateDefinedCategoryCommand.Subscribe(_ => this.ExecuteCreateCommand<Category>(this.PopulateDefinedCategory));

            this.DeleteDefinedCategoryCommand = ReactiveCommand.Create(canExecuteEditSelectedDefinedCategoryCommand);
            this.DeleteDefinedCategoryCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedDefinedCategory.Thing, this.PopulateDefinedCategory));

            this.EditDefinedCategoryCommand = ReactiveCommand.Create(canExecuteEditSelectedDefinedCategoryCommand);
            this.EditDefinedCategoryCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedDefinedCategory.Thing, this.PopulateDefinedCategory));

            this.InspectDefinedCategoryCommand = ReactiveCommand.Create(canExecuteInspectSelectedDefinedCategoryCommand);
            this.InspectDefinedCategoryCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedDefinedCategory.Thing));
            
            var canExecuteCreateParameterTypeCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedParameterTypeCommand = this.WhenAny(vm => vm.SelectedParameterType, v => v.Value != null);
            var canExecuteEditSelectedParameterTypeCommand = this.WhenAny(vm => vm.SelectedParameterType, v => v.Value != null && !this.IsReadOnly);

            this.CreateParameterTypeCommand = ReactiveCommand.Create(canExecuteCreateParameterTypeCommand);
            this.CreateParameterTypeCommand.Subscribe(_ => this.ExecuteCreateParameterTypeCommand());

            this.DeleteParameterTypeCommand = ReactiveCommand.Create(canExecuteEditSelectedParameterTypeCommand);
            this.DeleteParameterTypeCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedParameterType.Thing, this.PopulateParameterType));

            this.EditParameterTypeCommand = ReactiveCommand.Create(canExecuteEditSelectedParameterTypeCommand);
            this.EditParameterTypeCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedParameterType.Thing, this.PopulateParameterType));

            this.InspectParameterTypeCommand = ReactiveCommand.Create(canExecuteInspectSelectedParameterTypeCommand);
            this.InspectParameterTypeCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedParameterType.Thing));
            
            var canExecuteCreateScaleCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedScaleCommand = this.WhenAny(vm => vm.SelectedScale, v => v.Value != null);
            var canExecuteEditSelectedScaleCommand = this.WhenAny(vm => vm.SelectedScale, v => v.Value != null && !this.IsReadOnly);

            this.CreateScaleCommand = ReactiveCommand.Create(canExecuteCreateScaleCommand);
            this.CreateScaleCommand.Subscribe(_ => this.ExecuteCreateScaleCommand());

            this.DeleteScaleCommand = ReactiveCommand.Create(canExecuteEditSelectedScaleCommand);
            this.DeleteScaleCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedScale.Thing, this.PopulateScale));

            this.EditScaleCommand = ReactiveCommand.Create(canExecuteEditSelectedScaleCommand);
            this.EditScaleCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedScale.Thing, this.PopulateScale));

            this.InspectScaleCommand = ReactiveCommand.Create(canExecuteInspectSelectedScaleCommand);
            this.InspectScaleCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedScale.Thing));
            
            var canExecuteCreateUnitPrefixCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedUnitPrefixCommand = this.WhenAny(vm => vm.SelectedUnitPrefix, v => v.Value != null);
            var canExecuteEditSelectedUnitPrefixCommand = this.WhenAny(vm => vm.SelectedUnitPrefix, v => v.Value != null && !this.IsReadOnly);

            this.CreateUnitPrefixCommand = ReactiveCommand.Create(canExecuteCreateUnitPrefixCommand);
            this.CreateUnitPrefixCommand.Subscribe(_ => this.ExecuteCreateCommand<UnitPrefix>(this.PopulateUnitPrefix));

            this.DeleteUnitPrefixCommand = ReactiveCommand.Create(canExecuteEditSelectedUnitPrefixCommand);
            this.DeleteUnitPrefixCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedUnitPrefix.Thing, this.PopulateUnitPrefix));

            this.EditUnitPrefixCommand = ReactiveCommand.Create(canExecuteEditSelectedUnitPrefixCommand);
            this.EditUnitPrefixCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedUnitPrefix.Thing, this.PopulateUnitPrefix));

            this.InspectUnitPrefixCommand = ReactiveCommand.Create(canExecuteInspectSelectedUnitPrefixCommand);
            this.InspectUnitPrefixCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedUnitPrefix.Thing));
            
            var canExecuteCreateUnitCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedUnitCommand = this.WhenAny(vm => vm.SelectedUnit, v => v.Value != null);
            var canExecuteEditSelectedUnitCommand = this.WhenAny(vm => vm.SelectedUnit, v => v.Value != null && !this.IsReadOnly);

            this.CreateUnitCommand = ReactiveCommand.Create(canExecuteCreateUnitCommand);
            this.CreateUnitCommand.Subscribe(_ => this.ExecuteCreateUnitCommand());

            this.DeleteUnitCommand = ReactiveCommand.Create(canExecuteEditSelectedUnitCommand);
            this.DeleteUnitCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedUnit.Thing, this.PopulateUnit));

            this.EditUnitCommand = ReactiveCommand.Create(canExecuteEditSelectedUnitCommand);
            this.EditUnitCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedUnit.Thing, this.PopulateUnit));

            this.InspectUnitCommand = ReactiveCommand.Create(canExecuteInspectSelectedUnitCommand);
            this.InspectUnitCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedUnit.Thing));
            
            var canExecuteCreateFileTypeCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedFileTypeCommand = this.WhenAny(vm => vm.SelectedFileType, v => v.Value != null);
            var canExecuteEditSelectedFileTypeCommand = this.WhenAny(vm => vm.SelectedFileType, v => v.Value != null && !this.IsReadOnly);

            this.CreateFileTypeCommand = ReactiveCommand.Create(canExecuteCreateFileTypeCommand);
            this.CreateFileTypeCommand.Subscribe(_ => this.ExecuteCreateCommand<FileType>(this.PopulateFileType));

            this.DeleteFileTypeCommand = ReactiveCommand.Create(canExecuteEditSelectedFileTypeCommand);
            this.DeleteFileTypeCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedFileType.Thing, this.PopulateFileType));

            this.EditFileTypeCommand = ReactiveCommand.Create(canExecuteEditSelectedFileTypeCommand);
            this.EditFileTypeCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedFileType.Thing, this.PopulateFileType));

            this.InspectFileTypeCommand = ReactiveCommand.Create(canExecuteInspectSelectedFileTypeCommand);
            this.InspectFileTypeCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedFileType.Thing));
            
            var canExecuteCreateGlossaryCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedGlossaryCommand = this.WhenAny(vm => vm.SelectedGlossary, v => v.Value != null);
            var canExecuteEditSelectedGlossaryCommand = this.WhenAny(vm => vm.SelectedGlossary, v => v.Value != null && !this.IsReadOnly);

            this.CreateGlossaryCommand = ReactiveCommand.Create(canExecuteCreateGlossaryCommand);
            this.CreateGlossaryCommand.Subscribe(_ => this.ExecuteCreateCommand<Glossary>(this.PopulateGlossary));

            this.DeleteGlossaryCommand = ReactiveCommand.Create(canExecuteEditSelectedGlossaryCommand);
            this.DeleteGlossaryCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedGlossary.Thing, this.PopulateGlossary));

            this.EditGlossaryCommand = ReactiveCommand.Create(canExecuteEditSelectedGlossaryCommand);
            this.EditGlossaryCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedGlossary.Thing, this.PopulateGlossary));

            this.InspectGlossaryCommand = ReactiveCommand.Create(canExecuteInspectSelectedGlossaryCommand);
            this.InspectGlossaryCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedGlossary.Thing));
            
            var canExecuteCreateReferenceSourceCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedReferenceSourceCommand = this.WhenAny(vm => vm.SelectedReferenceSource, v => v.Value != null);
            var canExecuteEditSelectedReferenceSourceCommand = this.WhenAny(vm => vm.SelectedReferenceSource, v => v.Value != null && !this.IsReadOnly);

            this.CreateReferenceSourceCommand = ReactiveCommand.Create(canExecuteCreateReferenceSourceCommand);
            this.CreateReferenceSourceCommand.Subscribe(_ => this.ExecuteCreateCommand<ReferenceSource>(this.PopulateReferenceSource));

            this.DeleteReferenceSourceCommand = ReactiveCommand.Create(canExecuteEditSelectedReferenceSourceCommand);
            this.DeleteReferenceSourceCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedReferenceSource.Thing, this.PopulateReferenceSource));

            this.EditReferenceSourceCommand = ReactiveCommand.Create(canExecuteEditSelectedReferenceSourceCommand);
            this.EditReferenceSourceCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedReferenceSource.Thing, this.PopulateReferenceSource));

            this.InspectReferenceSourceCommand = ReactiveCommand.Create(canExecuteInspectSelectedReferenceSourceCommand);
            this.InspectReferenceSourceCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedReferenceSource.Thing));
            
            var canExecuteCreateRuleCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedRuleCommand = this.WhenAny(vm => vm.SelectedRule, v => v.Value != null);
            var canExecuteEditSelectedRuleCommand = this.WhenAny(vm => vm.SelectedRule, v => v.Value != null && !this.IsReadOnly);

            this.CreateRuleCommand = ReactiveCommand.Create(canExecuteCreateRuleCommand);
            this.CreateRuleCommand.Subscribe(_ => this.ExecuteCreateRuleCommand());

            this.DeleteRuleCommand = ReactiveCommand.Create(canExecuteEditSelectedRuleCommand);
            this.DeleteRuleCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedRule.Thing, this.PopulateRule));

            this.EditRuleCommand = ReactiveCommand.Create(canExecuteEditSelectedRuleCommand);
            this.EditRuleCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedRule.Thing, this.PopulateRule));

            this.InspectRuleCommand = ReactiveCommand.Create(canExecuteInspectSelectedRuleCommand);
            this.InspectRuleCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedRule.Thing));
            
            var canExecuteCreateConstantCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedConstantCommand = this.WhenAny(vm => vm.SelectedConstant, v => v.Value != null);
            var canExecuteEditSelectedConstantCommand = this.WhenAny(vm => vm.SelectedConstant, v => v.Value != null && !this.IsReadOnly);

            this.CreateConstantCommand = ReactiveCommand.Create(canExecuteCreateConstantCommand);
            this.CreateConstantCommand.Subscribe(_ => this.ExecuteCreateCommand<Constant>(this.PopulateConstant));

            this.DeleteConstantCommand = ReactiveCommand.Create(canExecuteEditSelectedConstantCommand);
            this.DeleteConstantCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedConstant.Thing, this.PopulateConstant));

            this.EditConstantCommand = ReactiveCommand.Create(canExecuteEditSelectedConstantCommand);
            this.EditConstantCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedConstant.Thing, this.PopulateConstant));

            this.InspectConstantCommand = ReactiveCommand.Create(canExecuteInspectSelectedConstantCommand);
            this.InspectConstantCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedConstant.Thing));
            var canExecuteInspectSelectedRequiredRdlCommand = this.WhenAny(vm => vm.SelectedRequiredRdl, v => v.Value != null);
            this.InspectSelectedRequiredRdlCommand = ReactiveCommand.Create(canExecuteInspectSelectedRequiredRdlCommand);
            this.InspectSelectedRequiredRdlCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedRequiredRdl));
        }

        /// <summary>
        /// Executes the Create <see cref="ICommand"/> for the abstract <see cref="ParameterType"/>
        /// </summary>
        protected void ExecuteCreateParameterTypeCommand()
        {
            switch (this.SelectedParameterTypeKind)
            {
                case ClassKind.ArrayParameterType:
                    this.ExecuteCreateCommand<ArrayParameterType>(this.PopulateParameterType);
                    break;
                case ClassKind.EnumerationParameterType:
                    this.ExecuteCreateCommand<EnumerationParameterType>(this.PopulateParameterType);
                    break;
                case ClassKind.BooleanParameterType:
                    this.ExecuteCreateCommand<BooleanParameterType>(this.PopulateParameterType);
                    break;
                case ClassKind.CompoundParameterType:
                    this.ExecuteCreateCommand<CompoundParameterType>(this.PopulateParameterType);
                    break;
                case ClassKind.DateParameterType:
                    this.ExecuteCreateCommand<DateParameterType>(this.PopulateParameterType);
                    break;
                case ClassKind.TextParameterType:
                    this.ExecuteCreateCommand<TextParameterType>(this.PopulateParameterType);
                    break;
                case ClassKind.SpecializedQuantityKind:
                    this.ExecuteCreateCommand<SpecializedQuantityKind>(this.PopulateParameterType);
                    break;
                case ClassKind.SimpleQuantityKind:
                    this.ExecuteCreateCommand<SimpleQuantityKind>(this.PopulateParameterType);
                    break;
                case ClassKind.DateTimeParameterType:
                    this.ExecuteCreateCommand<DateTimeParameterType>(this.PopulateParameterType);
                    break;
                case ClassKind.TimeOfDayParameterType:
                    this.ExecuteCreateCommand<TimeOfDayParameterType>(this.PopulateParameterType);
                    break;
                case ClassKind.DerivedQuantityKind:
                    this.ExecuteCreateCommand<DerivedQuantityKind>(this.PopulateParameterType);
                    break;
                default:
                    break;

            }
        }

        /// <summary>
        /// Executes the Create <see cref="ICommand"/> for the abstract <see cref="MeasurementScale"/>
        /// </summary>
        protected void ExecuteCreateScaleCommand()
        {
            switch (this.SelectedMeasurementScaleKind)
            {
                case ClassKind.CyclicRatioScale:
                    this.ExecuteCreateCommand<CyclicRatioScale>(this.PopulateScale);
                    break;
                case ClassKind.OrdinalScale:
                    this.ExecuteCreateCommand<OrdinalScale>(this.PopulateScale);
                    break;
                case ClassKind.RatioScale:
                    this.ExecuteCreateCommand<RatioScale>(this.PopulateScale);
                    break;
                case ClassKind.IntervalScale:
                    this.ExecuteCreateCommand<IntervalScale>(this.PopulateScale);
                    break;
                case ClassKind.LogarithmicScale:
                    this.ExecuteCreateCommand<LogarithmicScale>(this.PopulateScale);
                    break;
                default:
                    break;

            }
        }

        /// <summary>
        /// Executes the Create <see cref="ICommand"/> for the abstract <see cref="MeasurementUnit"/>
        /// </summary>
        protected void ExecuteCreateUnitCommand()
        {
            switch (this.SelectedMeasurementUnitKind)
            {
                case ClassKind.LinearConversionUnit:
                    this.ExecuteCreateCommand<LinearConversionUnit>(this.PopulateUnit);
                    break;
                case ClassKind.DerivedUnit:
                    this.ExecuteCreateCommand<DerivedUnit>(this.PopulateUnit);
                    break;
                case ClassKind.SimpleUnit:
                    this.ExecuteCreateCommand<SimpleUnit>(this.PopulateUnit);
                    break;
                case ClassKind.PrefixedUnit:
                    this.ExecuteCreateCommand<PrefixedUnit>(this.PopulateUnit);
                    break;
                default:
                    break;

            }
        }

        /// <summary>
        /// Executes the Create <see cref="ICommand"/> for the abstract <see cref="Rule"/>
        /// </summary>
        protected void ExecuteCreateRuleCommand()
        {
            switch (this.SelectedRuleKind)
            {
                case ClassKind.ReferencerRule:
                    this.ExecuteCreateCommand<ReferencerRule>(this.PopulateRule);
                    break;
                case ClassKind.BinaryRelationshipRule:
                    this.ExecuteCreateCommand<BinaryRelationshipRule>(this.PopulateRule);
                    break;
                case ClassKind.MultiRelationshipRule:
                    this.ExecuteCreateCommand<MultiRelationshipRule>(this.PopulateRule);
                    break;
                case ClassKind.DecompositionRule:
                    this.ExecuteCreateCommand<DecompositionRule>(this.PopulateRule);
                    break;
                case ClassKind.ParameterizedCategoryRule:
                    this.ExecuteCreateCommand<ParameterizedCategoryRule>(this.PopulateRule);
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

            clone.RequiredRdl = this.SelectedRequiredRdl;

            if (!clone.BaseQuantityKind.SortedItems.Values.SequenceEqual(this.BaseQuantityKind))
            {
                var baseQuantityKindCount = this.BaseQuantityKind.Count;
                for (var i = 0; i < baseQuantityKindCount; i++)
                {
                    var item = this.BaseQuantityKind[i];
                    var currentIndex = clone.BaseQuantityKind.IndexOf(item);

                    if (currentIndex != -1 && currentIndex != i)
                    {
                        clone.BaseQuantityKind.Move(currentIndex, i);
                    }
                    else if (currentIndex == -1)
                    {
                        clone.BaseQuantityKind.Insert(i, item);
                    }
                }

                // remove items that are no longer referenced
                for (var i = baseQuantityKindCount; i < clone.BaseQuantityKind.Count; i++)
                {
                    var toRemove = clone.BaseQuantityKind[i];
                    clone.BaseQuantityKind.Remove(toRemove);
                }
            }

            clone.BaseUnit.Clear();
            clone.BaseUnit.AddRange(this.BaseUnit);

        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PossibleRequiredRdl = new ReactiveList<SiteReferenceDataLibrary>();
            this.DefinedCategory = new ReactiveList<CategoryRowViewModel>();
            this.ParameterType = new ReactiveList<IRowViewModelBase<ParameterType>>();
            this.BaseQuantityKind = new ReactiveList<QuantityKind>();
            this.PossibleBaseQuantityKind = new ReactiveList<QuantityKind>();
            this.Scale = new ReactiveList<IRowViewModelBase<MeasurementScale>>();
            this.UnitPrefix = new ReactiveList<UnitPrefixRowViewModel>();
            this.Unit = new ReactiveList<IRowViewModelBase<MeasurementUnit>>();
            this.BaseUnit = new ReactiveList<MeasurementUnit>();
            this.PossibleBaseUnit = new ReactiveList<MeasurementUnit>();
            this.FileType = new ReactiveList<FileTypeRowViewModel>();
            this.Glossary = new ReactiveList<GlossaryRowViewModel>();
            this.ReferenceSource = new ReactiveList<ReferenceSourceRowViewModel>();
            this.Rule = new ReactiveList<IRowViewModelBase<Rule>>();
            this.Constant = new ReactiveList<ConstantRowViewModel>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.SelectedRequiredRdl = this.Thing.RequiredRdl;
            this.PopulatePossibleRequiredRdl();
            this.PopulateDefinedCategory();
            this.PopulateParameterType();
            this.PopulateScale();
            this.PopulateUnitPrefix();
            this.PopulateUnit();
            this.PopulateFileType();
            this.PopulateGlossary();
            this.PopulateReferenceSource();
            this.PopulateRule();
            this.PopulateConstant();
            this.PopulateBaseUnit();
        }

        /// <summary>
        /// Populates the <see cref="BaseUnit"/> property
        /// </summary>
        protected virtual void PopulateBaseUnit()
        {
            this.BaseUnit.Clear();

            foreach (var value in this.Thing.BaseUnit)
            {
                this.BaseUnit.Add(value);
            }
        } 

        /// <summary>
        /// Populates the <see cref="DefinedCategory"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateDefinedCategory()
        {
            this.DefinedCategory.Clear();
            foreach (var thing in this.Thing.DefinedCategory.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new CategoryRowViewModel(thing, this.Session, this);
                this.DefinedCategory.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="ParameterType"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateParameterType()
        {
            // this method needs to be overriden.
        }

        /// <summary>
        /// Populates the <see cref="Scale"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateScale()
        {
            // this method needs to be overriden.
        }

        /// <summary>
        /// Populates the <see cref="UnitPrefix"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateUnitPrefix()
        {
            this.UnitPrefix.Clear();
            foreach (var thing in this.Thing.UnitPrefix.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new UnitPrefixRowViewModel(thing, this.Session, this);
                this.UnitPrefix.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="Unit"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateUnit()
        {
            // this method needs to be overriden.
        }

        /// <summary>
        /// Populates the <see cref="FileType"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateFileType()
        {
            this.FileType.Clear();
            foreach (var thing in this.Thing.FileType.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new FileTypeRowViewModel(thing, this.Session, this);
                this.FileType.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="Glossary"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateGlossary()
        {
            this.Glossary.Clear();
            foreach (var thing in this.Thing.Glossary.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new GlossaryRowViewModel(thing, this.Session, this);
                this.Glossary.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="ReferenceSource"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateReferenceSource()
        {
            this.ReferenceSource.Clear();
            foreach (var thing in this.Thing.ReferenceSource.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new ReferenceSourceRowViewModel(thing, this.Session, this);
                this.ReferenceSource.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="Rule"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateRule()
        {
            // this method needs to be overriden.
        }

        /// <summary>
        /// Populates the <see cref="Constant"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateConstant()
        {
            this.Constant.Clear();
            foreach (var thing in this.Thing.Constant.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new ConstantRowViewModel(thing, this.Session, this);
                this.Constant.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="PossibleRequiredRdl"/> property
        /// </summary>
        protected virtual void PopulatePossibleRequiredRdl()
        {
            this.PossibleRequiredRdl.Clear();
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
            foreach(var definedCategory in this.DefinedCategory)
            {
                definedCategory.Dispose();
            }
            foreach(var parameterType in this.ParameterType)
            {
                parameterType.Dispose();
            }
            foreach(var scale in this.Scale)
            {
                scale.Dispose();
            }
            foreach(var unitPrefix in this.UnitPrefix)
            {
                unitPrefix.Dispose();
            }
            foreach(var unit in this.Unit)
            {
                unit.Dispose();
            }
            foreach(var fileType in this.FileType)
            {
                fileType.Dispose();
            }
            foreach(var glossary in this.Glossary)
            {
                glossary.Dispose();
            }
            foreach(var referenceSource in this.ReferenceSource)
            {
                referenceSource.Dispose();
            }
            foreach(var rule in this.Rule)
            {
                rule.Dispose();
            }
            foreach(var constant in this.Constant)
            {
                constant.Dispose();
            }
        }
    }
}
