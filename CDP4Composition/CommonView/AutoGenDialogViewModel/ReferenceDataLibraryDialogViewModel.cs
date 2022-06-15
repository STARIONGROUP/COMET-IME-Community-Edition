// -------------------------------------------------------------------------------------------------
// <copyright file="ReferenceDataLibraryDialogViewModel.cs" company="RHEA System S.A.">
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
    using System.Reactive;

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
        public ReactiveCommand<Unit, Unit> InspectSelectedRequiredRdlCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a Category
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateDefinedCategoryCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a Category
        /// </summary>
        public ReactiveCommand<Unit, Unit> DeleteDefinedCategoryCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a Category
        /// </summary>
        public ReactiveCommand<Unit, Unit> EditDefinedCategoryCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a Category
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectDefinedCategoryCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a ParameterType
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateParameterTypeCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a ParameterType
        /// </summary>
        public ReactiveCommand<Unit, Unit> DeleteParameterTypeCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a ParameterType
        /// </summary>
        public ReactiveCommand<Unit, Unit> EditParameterTypeCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a ParameterType
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectParameterTypeCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a MeasurementScale
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateScaleCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a MeasurementScale
        /// </summary>
        public ReactiveCommand<Unit, Unit> DeleteScaleCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a MeasurementScale
        /// </summary>
        public ReactiveCommand<Unit, Unit> EditScaleCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a MeasurementScale
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectScaleCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a UnitPrefix
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateUnitPrefixCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a UnitPrefix
        /// </summary>
        public ReactiveCommand<Unit, Unit> DeleteUnitPrefixCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a UnitPrefix
        /// </summary>
        public ReactiveCommand<Unit, Unit> EditUnitPrefixCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a UnitPrefix
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectUnitPrefixCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a MeasurementUnit
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateUnitCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a MeasurementUnit
        /// </summary>
        public ReactiveCommand<Unit, Unit> DeleteUnitCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a MeasurementUnit
        /// </summary>
        public ReactiveCommand<Unit, Unit> EditUnitCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a MeasurementUnit
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectUnitCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a FileType
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateFileTypeCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a FileType
        /// </summary>
        public ReactiveCommand<Unit, Unit> DeleteFileTypeCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a FileType
        /// </summary>
        public ReactiveCommand<Unit, Unit> EditFileTypeCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a FileType
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectFileTypeCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a Glossary
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateGlossaryCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a Glossary
        /// </summary>
        public ReactiveCommand<Unit, Unit> DeleteGlossaryCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a Glossary
        /// </summary>
        public ReactiveCommand<Unit, Unit> EditGlossaryCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a Glossary
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectGlossaryCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a ReferenceSource
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateReferenceSourceCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a ReferenceSource
        /// </summary>
        public ReactiveCommand<Unit, Unit> DeleteReferenceSourceCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a ReferenceSource
        /// </summary>
        public ReactiveCommand<Unit, Unit> EditReferenceSourceCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a ReferenceSource
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectReferenceSourceCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a Rule
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateRuleCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a Rule
        /// </summary>
        public ReactiveCommand<Unit, Unit> DeleteRuleCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a Rule
        /// </summary>
        public ReactiveCommand<Unit, Unit> EditRuleCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a Rule
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectRuleCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a Constant
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateConstantCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a Constant
        /// </summary>
        public ReactiveCommand<Unit, Unit> DeleteConstantCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a Constant
        /// </summary>
        public ReactiveCommand<Unit, Unit> EditConstantCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a Constant
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectConstantCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            
            var canExecuteCreateDefinedCategoryCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedDefinedCategoryCommand = this.WhenAny(vm => vm.SelectedDefinedCategory, v => v.Value != null);
            var canExecuteEditSelectedDefinedCategoryCommand = this.WhenAny(vm => vm.SelectedDefinedCategory, v => v.Value != null && !this.IsReadOnly);

            this.CreateDefinedCategoryCommand = ReactiveCommandCreator.Create(canExecuteCreateDefinedCategoryCommand);
            this.CreateDefinedCategoryCommand.Subscribe(_ => this.ExecuteCreateCommand<Category>(this.PopulateDefinedCategory));

            this.DeleteDefinedCategoryCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedDefinedCategoryCommand);
            this.DeleteDefinedCategoryCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedDefinedCategory.Thing, this.PopulateDefinedCategory));

            this.EditDefinedCategoryCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedDefinedCategoryCommand);
            this.EditDefinedCategoryCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedDefinedCategory.Thing, this.PopulateDefinedCategory));

            this.InspectDefinedCategoryCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedDefinedCategoryCommand);
            this.InspectDefinedCategoryCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedDefinedCategory.Thing));
            
            var canExecuteCreateParameterTypeCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedParameterTypeCommand = this.WhenAny(vm => vm.SelectedParameterType, v => v.Value != null);
            var canExecuteEditSelectedParameterTypeCommand = this.WhenAny(vm => vm.SelectedParameterType, v => v.Value != null && !this.IsReadOnly);

            this.CreateParameterTypeCommand = ReactiveCommandCreator.Create(canExecuteCreateParameterTypeCommand);
            this.CreateParameterTypeCommand.Subscribe(_ => this.ExecuteCreateParameterTypeCommand());

            this.DeleteParameterTypeCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedParameterTypeCommand);
            this.DeleteParameterTypeCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedParameterType.Thing, this.PopulateParameterType));

            this.EditParameterTypeCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedParameterTypeCommand);
            this.EditParameterTypeCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedParameterType.Thing, this.PopulateParameterType));

            this.InspectParameterTypeCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedParameterTypeCommand);
            this.InspectParameterTypeCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedParameterType.Thing));
            
            var canExecuteCreateScaleCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedScaleCommand = this.WhenAny(vm => vm.SelectedScale, v => v.Value != null);
            var canExecuteEditSelectedScaleCommand = this.WhenAny(vm => vm.SelectedScale, v => v.Value != null && !this.IsReadOnly);

            this.CreateScaleCommand = ReactiveCommandCreator.Create(canExecuteCreateScaleCommand);
            this.CreateScaleCommand.Subscribe(_ => this.ExecuteCreateScaleCommand());

            this.DeleteScaleCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedScaleCommand);
            this.DeleteScaleCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedScale.Thing, this.PopulateScale));

            this.EditScaleCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedScaleCommand);
            this.EditScaleCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedScale.Thing, this.PopulateScale));

            this.InspectScaleCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedScaleCommand);
            this.InspectScaleCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedScale.Thing));
            
            var canExecuteCreateUnitPrefixCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedUnitPrefixCommand = this.WhenAny(vm => vm.SelectedUnitPrefix, v => v.Value != null);
            var canExecuteEditSelectedUnitPrefixCommand = this.WhenAny(vm => vm.SelectedUnitPrefix, v => v.Value != null && !this.IsReadOnly);

            this.CreateUnitPrefixCommand = ReactiveCommandCreator.Create(canExecuteCreateUnitPrefixCommand);
            this.CreateUnitPrefixCommand.Subscribe(_ => this.ExecuteCreateCommand<UnitPrefix>(this.PopulateUnitPrefix));

            this.DeleteUnitPrefixCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedUnitPrefixCommand);
            this.DeleteUnitPrefixCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedUnitPrefix.Thing, this.PopulateUnitPrefix));

            this.EditUnitPrefixCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedUnitPrefixCommand);
            this.EditUnitPrefixCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedUnitPrefix.Thing, this.PopulateUnitPrefix));

            this.InspectUnitPrefixCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedUnitPrefixCommand);
            this.InspectUnitPrefixCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedUnitPrefix.Thing));
            
            var canExecuteCreateUnitCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedUnitCommand = this.WhenAny(vm => vm.SelectedUnit, v => v.Value != null);
            var canExecuteEditSelectedUnitCommand = this.WhenAny(vm => vm.SelectedUnit, v => v.Value != null && !this.IsReadOnly);

            this.CreateUnitCommand = ReactiveCommandCreator.Create(canExecuteCreateUnitCommand);
            this.CreateUnitCommand.Subscribe(_ => this.ExecuteCreateUnitCommand());

            this.DeleteUnitCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedUnitCommand);
            this.DeleteUnitCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedUnit.Thing, this.PopulateUnit));

            this.EditUnitCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedUnitCommand);
            this.EditUnitCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedUnit.Thing, this.PopulateUnit));

            this.InspectUnitCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedUnitCommand);
            this.InspectUnitCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedUnit.Thing));
            
            var canExecuteCreateFileTypeCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedFileTypeCommand = this.WhenAny(vm => vm.SelectedFileType, v => v.Value != null);
            var canExecuteEditSelectedFileTypeCommand = this.WhenAny(vm => vm.SelectedFileType, v => v.Value != null && !this.IsReadOnly);

            this.CreateFileTypeCommand = ReactiveCommandCreator.Create(canExecuteCreateFileTypeCommand);
            this.CreateFileTypeCommand.Subscribe(_ => this.ExecuteCreateCommand<FileType>(this.PopulateFileType));

            this.DeleteFileTypeCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedFileTypeCommand);
            this.DeleteFileTypeCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedFileType.Thing, this.PopulateFileType));

            this.EditFileTypeCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedFileTypeCommand);
            this.EditFileTypeCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedFileType.Thing, this.PopulateFileType));

            this.InspectFileTypeCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedFileTypeCommand);
            this.InspectFileTypeCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedFileType.Thing));
            
            var canExecuteCreateGlossaryCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedGlossaryCommand = this.WhenAny(vm => vm.SelectedGlossary, v => v.Value != null);
            var canExecuteEditSelectedGlossaryCommand = this.WhenAny(vm => vm.SelectedGlossary, v => v.Value != null && !this.IsReadOnly);

            this.CreateGlossaryCommand = ReactiveCommandCreator.Create(canExecuteCreateGlossaryCommand);
            this.CreateGlossaryCommand.Subscribe(_ => this.ExecuteCreateCommand<Glossary>(this.PopulateGlossary));

            this.DeleteGlossaryCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedGlossaryCommand);
            this.DeleteGlossaryCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedGlossary.Thing, this.PopulateGlossary));

            this.EditGlossaryCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedGlossaryCommand);
            this.EditGlossaryCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedGlossary.Thing, this.PopulateGlossary));

            this.InspectGlossaryCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedGlossaryCommand);
            this.InspectGlossaryCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedGlossary.Thing));
            
            var canExecuteCreateReferenceSourceCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedReferenceSourceCommand = this.WhenAny(vm => vm.SelectedReferenceSource, v => v.Value != null);
            var canExecuteEditSelectedReferenceSourceCommand = this.WhenAny(vm => vm.SelectedReferenceSource, v => v.Value != null && !this.IsReadOnly);

            this.CreateReferenceSourceCommand = ReactiveCommandCreator.Create(canExecuteCreateReferenceSourceCommand);
            this.CreateReferenceSourceCommand.Subscribe(_ => this.ExecuteCreateCommand<ReferenceSource>(this.PopulateReferenceSource));

            this.DeleteReferenceSourceCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedReferenceSourceCommand);
            this.DeleteReferenceSourceCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedReferenceSource.Thing, this.PopulateReferenceSource));

            this.EditReferenceSourceCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedReferenceSourceCommand);
            this.EditReferenceSourceCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedReferenceSource.Thing, this.PopulateReferenceSource));

            this.InspectReferenceSourceCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedReferenceSourceCommand);
            this.InspectReferenceSourceCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedReferenceSource.Thing));
            
            var canExecuteCreateRuleCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedRuleCommand = this.WhenAny(vm => vm.SelectedRule, v => v.Value != null);
            var canExecuteEditSelectedRuleCommand = this.WhenAny(vm => vm.SelectedRule, v => v.Value != null && !this.IsReadOnly);

            this.CreateRuleCommand = ReactiveCommandCreator.Create(canExecuteCreateRuleCommand);
            this.CreateRuleCommand.Subscribe(_ => this.ExecuteCreateRuleCommand());

            this.DeleteRuleCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedRuleCommand);
            this.DeleteRuleCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedRule.Thing, this.PopulateRule));

            this.EditRuleCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedRuleCommand);
            this.EditRuleCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedRule.Thing, this.PopulateRule));

            this.InspectRuleCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedRuleCommand);
            this.InspectRuleCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedRule.Thing));
            
            var canExecuteCreateConstantCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedConstantCommand = this.WhenAny(vm => vm.SelectedConstant, v => v.Value != null);
            var canExecuteEditSelectedConstantCommand = this.WhenAny(vm => vm.SelectedConstant, v => v.Value != null && !this.IsReadOnly);

            this.CreateConstantCommand = ReactiveCommandCreator.Create(canExecuteCreateConstantCommand);
            this.CreateConstantCommand.Subscribe(_ => this.ExecuteCreateCommand<Constant>(this.PopulateConstant));

            this.DeleteConstantCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedConstantCommand);
            this.DeleteConstantCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedConstant.Thing, this.PopulateConstant));

            this.EditConstantCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedConstantCommand);
            this.EditConstantCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedConstant.Thing, this.PopulateConstant));

            this.InspectConstantCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedConstantCommand);
            this.InspectConstantCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedConstant.Thing));
            var canExecuteInspectSelectedRequiredRdlCommand = this.WhenAny(vm => vm.SelectedRequiredRdl, v => v.Value != null);
            this.InspectSelectedRequiredRdlCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedRequiredRdlCommand);
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
