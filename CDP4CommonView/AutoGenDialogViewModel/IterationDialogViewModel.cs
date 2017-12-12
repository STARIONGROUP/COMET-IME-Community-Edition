// -------------------------------------------------------------------------------------------------
// <copyright file="IterationDialogViewModel.cs" company="RHEA System S.A.">
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
    using CDP4Common.EngineeringModelData;
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
    /// dialog-view-model class representing a <see cref="Iteration"/>
    /// </summary>
    public partial class IterationDialogViewModel : DialogViewModelBase<Iteration>
    {
        /// <summary>
        /// Backing field for <see cref="SourceIterationIid"/>
        /// </summary>
        private Guid? sourceIterationIid;

        /// <summary>
        /// Backing field for <see cref="SelectedIterationSetup"/>
        /// </summary>
        private IterationSetup selectedIterationSetup;

        /// <summary>
        /// Backing field for <see cref="SelectedTopElement"/>
        /// </summary>
        private ElementDefinition selectedTopElement;

        /// <summary>
        /// Backing field for <see cref="SelectedDefaultOption"/>
        /// </summary>
        private Option selectedDefaultOption;

        /// <summary>
        /// Backing field for <see cref="SelectedOption"/>
        /// </summary>
        private OptionRowViewModel selectedOption;

        /// <summary>
        /// Backing field for <see cref="SelectedPublication"/>
        /// </summary>
        private PublicationRowViewModel selectedPublication;

        /// <summary>
        /// Backing field for <see cref="SelectedPossibleFiniteStateList"/>
        /// </summary>
        private PossibleFiniteStateListRowViewModel selectedPossibleFiniteStateList;

        /// <summary>
        /// Backing field for <see cref="SelectedElement"/>
        /// </summary>
        private ElementDefinitionRowViewModel selectedElement;

        /// <summary>
        /// Backing field for <see cref="SelectedRelationship"/>
        /// </summary>
        private IRowViewModelBase<Relationship> selectedRelationship;

        /// <summary>
        /// Backing field for <see cref="SelectedExternalIdentifierMap"/>
        /// </summary>
        private ExternalIdentifierMapRowViewModel selectedExternalIdentifierMap;

        /// <summary>
        /// Backing field for <see cref="SelectedRequirementsSpecification"/>
        /// </summary>
        private RequirementsSpecificationRowViewModel selectedRequirementsSpecification;

        /// <summary>
        /// Backing field for <see cref="SelectedDomainFileStore"/>
        /// </summary>
        private DomainFileStoreRowViewModel selectedDomainFileStore;

        /// <summary>
        /// Backing field for <see cref="SelectedActualFiniteStateList"/>
        /// </summary>
        private ActualFiniteStateListRowViewModel selectedActualFiniteStateList;

        /// <summary>
        /// Backing field for <see cref="SelectedRuleVerificationList"/>
        /// </summary>
        private RuleVerificationListRowViewModel selectedRuleVerificationList;

        /// <summary>
        /// Backing field for <see cref="SelectedStakeholder"/>
        /// </summary>
        private StakeholderRowViewModel selectedStakeholder;

        /// <summary>
        /// Backing field for <see cref="SelectedGoal"/>
        /// </summary>
        private GoalRowViewModel selectedGoal;

        /// <summary>
        /// Backing field for <see cref="SelectedValueGroup"/>
        /// </summary>
        private ValueGroupRowViewModel selectedValueGroup;

        /// <summary>
        /// Backing field for <see cref="SelectedStakeholderValue"/>
        /// </summary>
        private StakeholderValueRowViewModel selectedStakeholderValue;

        /// <summary>
        /// Backing field for <see cref="SelectedStakeholderValueMap"/>
        /// </summary>
        private StakeHolderValueMapRowViewModel selectedStakeholderValueMap;

        /// <summary>
        /// Backing field for <see cref="SelectedSharedDiagramStyle"/>
        /// </summary>
        private SharedStyleRowViewModel selectedSharedDiagramStyle;

        /// <summary>
        /// Backing field for <see cref="SelectedDiagramCanvas"/>
        /// </summary>
        private DiagramCanvasRowViewModel selectedDiagramCanvas;


        /// <summary>
        /// Backing field for <see cref="SelectedRelationship"/>Kind
        /// </summary>
        private ClassKind selectedRelationshipKind;

        /// <summary>
        /// Initializes a new instance of the <see cref="IterationDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public IterationDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IterationDialogViewModel"/> class
        /// </summary>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> that is the subject of the current view-model. This is the object
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
        public IterationDialogViewModel(Iteration iteration, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(iteration, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            if(container != null)
            {
                var containerThing = container as EngineeringModel;
                if(containerThing == null)
                {
                    var errorMessage =
                        string.Format(
                            "The container parameter is of type {0}, it shall be of type EngineeringModel",
                            container.GetType());
                    throw new ArgumentException(errorMessage);
                }
            }
        }

        /// <summary>
        /// Gets or sets the SourceIterationIid
        /// </summary>
        public virtual Guid? SourceIterationIid
        {
            get { return this.sourceIterationIid; }
            set { this.RaiseAndSetIfChanged(ref this.sourceIterationIid, value); }
        }

        /// <summary>
        /// Gets or sets the SelectedIterationSetup
        /// </summary>
        public virtual IterationSetup SelectedIterationSetup
        {
            get { return this.selectedIterationSetup; }
            set { this.RaiseAndSetIfChanged(ref this.selectedIterationSetup, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="IterationSetup"/>s for <see cref="SelectedIterationSetup"/>
        /// </summary>
        public ReactiveList<IterationSetup> PossibleIterationSetup { get; protected set; }

        /// <summary>
        /// Gets or sets the SelectedTopElement
        /// </summary>
        public virtual ElementDefinition SelectedTopElement
        {
            get { return this.selectedTopElement; }
            set { this.RaiseAndSetIfChanged(ref this.selectedTopElement, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="ElementDefinition"/>s for <see cref="SelectedTopElement"/>
        /// </summary>
        public ReactiveList<ElementDefinition> PossibleTopElement { get; protected set; }

        /// <summary>
        /// Gets or sets the SelectedDefaultOption
        /// </summary>
        public virtual Option SelectedDefaultOption
        {
            get { return this.selectedDefaultOption; }
            set { this.RaiseAndSetIfChanged(ref this.selectedDefaultOption, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="Option"/>s for <see cref="SelectedDefaultOption"/>
        /// </summary>
        public ReactiveList<Option> PossibleDefaultOption { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="OptionRowViewModel"/>
        /// </summary>
        public OptionRowViewModel SelectedOption
        {
            get { return this.selectedOption; }
            set { this.RaiseAndSetIfChanged(ref this.selectedOption, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="Option"/>
        /// </summary>
        public ReactiveList<OptionRowViewModel> Option { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="PublicationRowViewModel"/>
        /// </summary>
        public PublicationRowViewModel SelectedPublication
        {
            get { return this.selectedPublication; }
            set { this.RaiseAndSetIfChanged(ref this.selectedPublication, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="Publication"/>
        /// </summary>
        public ReactiveList<PublicationRowViewModel> Publication { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="PossibleFiniteStateListRowViewModel"/>
        /// </summary>
        public PossibleFiniteStateListRowViewModel SelectedPossibleFiniteStateList
        {
            get { return this.selectedPossibleFiniteStateList; }
            set { this.RaiseAndSetIfChanged(ref this.selectedPossibleFiniteStateList, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="PossibleFiniteStateList"/>
        /// </summary>
        public ReactiveList<PossibleFiniteStateListRowViewModel> PossibleFiniteStateList { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="ElementDefinitionRowViewModel"/>
        /// </summary>
        public ElementDefinitionRowViewModel SelectedElement
        {
            get { return this.selectedElement; }
            set { this.RaiseAndSetIfChanged(ref this.selectedElement, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="ElementDefinition"/>
        /// </summary>
        public ReactiveList<ElementDefinitionRowViewModel> Element { get; protected set; }
        
        /// <summary>
        /// Gets the concrete Relationship to create
        /// </summary>
        public ClassKind SelectedRelationshipKind
        {
            get { return this.selectedRelationshipKind; }
            set { this.RaiseAndSetIfChanged(ref this.selectedRelationshipKind, value); } 
        }

        /// <summary>
        /// Gets the list of possible <see cref="ClassKind"/> for <see cref="Uml.StructuredClassifiers.Class"/>
        /// </summary>
        public readonly ReactiveList<ClassKind> PossibleRelationshipKind = new ReactiveList<ClassKind>() 
        { 
            ClassKind.MultiRelationship,
            ClassKind.BinaryRelationship 
        };
        
        /// <summary>
        /// Gets or sets the selected <see cref="IRowViewModelBase{T}"/>
        /// </summary>
        public IRowViewModelBase<Relationship> SelectedRelationship
        {
            get { return this.selectedRelationship; }
            set { this.RaiseAndSetIfChanged(ref this.selectedRelationship, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="Relationship"/>
        /// </summary>
        public ReactiveList<IRowViewModelBase<Relationship>> Relationship { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="ExternalIdentifierMapRowViewModel"/>
        /// </summary>
        public ExternalIdentifierMapRowViewModel SelectedExternalIdentifierMap
        {
            get { return this.selectedExternalIdentifierMap; }
            set { this.RaiseAndSetIfChanged(ref this.selectedExternalIdentifierMap, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="ExternalIdentifierMap"/>
        /// </summary>
        public ReactiveList<ExternalIdentifierMapRowViewModel> ExternalIdentifierMap { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="RequirementsSpecificationRowViewModel"/>
        /// </summary>
        public RequirementsSpecificationRowViewModel SelectedRequirementsSpecification
        {
            get { return this.selectedRequirementsSpecification; }
            set { this.RaiseAndSetIfChanged(ref this.selectedRequirementsSpecification, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="RequirementsSpecification"/>
        /// </summary>
        public ReactiveList<RequirementsSpecificationRowViewModel> RequirementsSpecification { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="DomainFileStoreRowViewModel"/>
        /// </summary>
        public DomainFileStoreRowViewModel SelectedDomainFileStore
        {
            get { return this.selectedDomainFileStore; }
            set { this.RaiseAndSetIfChanged(ref this.selectedDomainFileStore, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="DomainFileStore"/>
        /// </summary>
        public ReactiveList<DomainFileStoreRowViewModel> DomainFileStore { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="ActualFiniteStateListRowViewModel"/>
        /// </summary>
        public ActualFiniteStateListRowViewModel SelectedActualFiniteStateList
        {
            get { return this.selectedActualFiniteStateList; }
            set { this.RaiseAndSetIfChanged(ref this.selectedActualFiniteStateList, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="ActualFiniteStateList"/>
        /// </summary>
        public ReactiveList<ActualFiniteStateListRowViewModel> ActualFiniteStateList { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="RuleVerificationListRowViewModel"/>
        /// </summary>
        public RuleVerificationListRowViewModel SelectedRuleVerificationList
        {
            get { return this.selectedRuleVerificationList; }
            set { this.RaiseAndSetIfChanged(ref this.selectedRuleVerificationList, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="RuleVerificationList"/>
        /// </summary>
        public ReactiveList<RuleVerificationListRowViewModel> RuleVerificationList { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="StakeholderRowViewModel"/>
        /// </summary>
        public StakeholderRowViewModel SelectedStakeholder
        {
            get { return this.selectedStakeholder; }
            set { this.RaiseAndSetIfChanged(ref this.selectedStakeholder, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="Stakeholder"/>
        /// </summary>
        public ReactiveList<StakeholderRowViewModel> Stakeholder { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="GoalRowViewModel"/>
        /// </summary>
        public GoalRowViewModel SelectedGoal
        {
            get { return this.selectedGoal; }
            set { this.RaiseAndSetIfChanged(ref this.selectedGoal, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="Goal"/>
        /// </summary>
        public ReactiveList<GoalRowViewModel> Goal { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="ValueGroupRowViewModel"/>
        /// </summary>
        public ValueGroupRowViewModel SelectedValueGroup
        {
            get { return this.selectedValueGroup; }
            set { this.RaiseAndSetIfChanged(ref this.selectedValueGroup, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="ValueGroup"/>
        /// </summary>
        public ReactiveList<ValueGroupRowViewModel> ValueGroup { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="StakeholderValueRowViewModel"/>
        /// </summary>
        public StakeholderValueRowViewModel SelectedStakeholderValue
        {
            get { return this.selectedStakeholderValue; }
            set { this.RaiseAndSetIfChanged(ref this.selectedStakeholderValue, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="StakeholderValue"/>
        /// </summary>
        public ReactiveList<StakeholderValueRowViewModel> StakeholderValue { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="StakeHolderValueMapRowViewModel"/>
        /// </summary>
        public StakeHolderValueMapRowViewModel SelectedStakeholderValueMap
        {
            get { return this.selectedStakeholderValueMap; }
            set { this.RaiseAndSetIfChanged(ref this.selectedStakeholderValueMap, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="StakeHolderValueMap"/>
        /// </summary>
        public ReactiveList<StakeHolderValueMapRowViewModel> StakeholderValueMap { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="SharedStyleRowViewModel"/>
        /// </summary>
        public SharedStyleRowViewModel SelectedSharedDiagramStyle
        {
            get { return this.selectedSharedDiagramStyle; }
            set { this.RaiseAndSetIfChanged(ref this.selectedSharedDiagramStyle, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="SharedStyle"/>
        /// </summary>
        public ReactiveList<SharedStyleRowViewModel> SharedDiagramStyle { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="DiagramCanvasRowViewModel"/>
        /// </summary>
        public DiagramCanvasRowViewModel SelectedDiagramCanvas
        {
            get { return this.selectedDiagramCanvas; }
            set { this.RaiseAndSetIfChanged(ref this.selectedDiagramCanvas, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="DiagramCanvas"/>
        /// </summary>
        public ReactiveList<DiagramCanvasRowViewModel> DiagramCanvas { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedIterationSetup"/>
        /// </summary>
        public ReactiveCommand<object> InspectSelectedIterationSetupCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedTopElement"/>
        /// </summary>
        public ReactiveCommand<object> InspectSelectedTopElementCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedDefaultOption"/>
        /// </summary>
        public ReactiveCommand<object> InspectSelectedDefaultOptionCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a Option
        /// </summary>
        public ReactiveCommand<object> CreateOptionCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a Option
        /// </summary>
        public ReactiveCommand<object> DeleteOptionCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a Option
        /// </summary>
        public ReactiveCommand<object> EditOptionCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a Option
        /// </summary>
        public ReactiveCommand<object> InspectOptionCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Move-Up <see cref="ICommand"/> to move up the order of a Option 
        /// </summary>
        public ReactiveCommand<object> MoveUpOptionCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Move-Down <see cref="ICommand"/> to move down the order of a Option
        /// </summary>
        public ReactiveCommand<object> MoveDownOptionCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a Publication
        /// </summary>
        public ReactiveCommand<object> CreatePublicationCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a Publication
        /// </summary>
        public ReactiveCommand<object> DeletePublicationCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a Publication
        /// </summary>
        public ReactiveCommand<object> EditPublicationCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a Publication
        /// </summary>
        public ReactiveCommand<object> InspectPublicationCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a PossibleFiniteStateList
        /// </summary>
        public ReactiveCommand<object> CreatePossibleFiniteStateListCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a PossibleFiniteStateList
        /// </summary>
        public ReactiveCommand<object> DeletePossibleFiniteStateListCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a PossibleFiniteStateList
        /// </summary>
        public ReactiveCommand<object> EditPossibleFiniteStateListCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a PossibleFiniteStateList
        /// </summary>
        public ReactiveCommand<object> InspectPossibleFiniteStateListCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a ElementDefinition
        /// </summary>
        public ReactiveCommand<object> CreateElementCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a ElementDefinition
        /// </summary>
        public ReactiveCommand<object> DeleteElementCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a ElementDefinition
        /// </summary>
        public ReactiveCommand<object> EditElementCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a ElementDefinition
        /// </summary>
        public ReactiveCommand<object> InspectElementCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a Relationship
        /// </summary>
        public ReactiveCommand<object> CreateRelationshipCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a Relationship
        /// </summary>
        public ReactiveCommand<object> DeleteRelationshipCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a Relationship
        /// </summary>
        public ReactiveCommand<object> EditRelationshipCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a Relationship
        /// </summary>
        public ReactiveCommand<object> InspectRelationshipCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a ExternalIdentifierMap
        /// </summary>
        public ReactiveCommand<object> CreateExternalIdentifierMapCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a ExternalIdentifierMap
        /// </summary>
        public ReactiveCommand<object> DeleteExternalIdentifierMapCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a ExternalIdentifierMap
        /// </summary>
        public ReactiveCommand<object> EditExternalIdentifierMapCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a ExternalIdentifierMap
        /// </summary>
        public ReactiveCommand<object> InspectExternalIdentifierMapCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a RequirementsSpecification
        /// </summary>
        public ReactiveCommand<object> CreateRequirementsSpecificationCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a RequirementsSpecification
        /// </summary>
        public ReactiveCommand<object> DeleteRequirementsSpecificationCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a RequirementsSpecification
        /// </summary>
        public ReactiveCommand<object> EditRequirementsSpecificationCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a RequirementsSpecification
        /// </summary>
        public ReactiveCommand<object> InspectRequirementsSpecificationCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a DomainFileStore
        /// </summary>
        public ReactiveCommand<object> CreateDomainFileStoreCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a DomainFileStore
        /// </summary>
        public ReactiveCommand<object> DeleteDomainFileStoreCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a DomainFileStore
        /// </summary>
        public ReactiveCommand<object> EditDomainFileStoreCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a DomainFileStore
        /// </summary>
        public ReactiveCommand<object> InspectDomainFileStoreCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a ActualFiniteStateList
        /// </summary>
        public ReactiveCommand<object> CreateActualFiniteStateListCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a ActualFiniteStateList
        /// </summary>
        public ReactiveCommand<object> DeleteActualFiniteStateListCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a ActualFiniteStateList
        /// </summary>
        public ReactiveCommand<object> EditActualFiniteStateListCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a ActualFiniteStateList
        /// </summary>
        public ReactiveCommand<object> InspectActualFiniteStateListCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a RuleVerificationList
        /// </summary>
        public ReactiveCommand<object> CreateRuleVerificationListCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a RuleVerificationList
        /// </summary>
        public ReactiveCommand<object> DeleteRuleVerificationListCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a RuleVerificationList
        /// </summary>
        public ReactiveCommand<object> EditRuleVerificationListCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a RuleVerificationList
        /// </summary>
        public ReactiveCommand<object> InspectRuleVerificationListCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a Stakeholder
        /// </summary>
        public ReactiveCommand<object> CreateStakeholderCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a Stakeholder
        /// </summary>
        public ReactiveCommand<object> DeleteStakeholderCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a Stakeholder
        /// </summary>
        public ReactiveCommand<object> EditStakeholderCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a Stakeholder
        /// </summary>
        public ReactiveCommand<object> InspectStakeholderCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a Goal
        /// </summary>
        public ReactiveCommand<object> CreateGoalCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a Goal
        /// </summary>
        public ReactiveCommand<object> DeleteGoalCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a Goal
        /// </summary>
        public ReactiveCommand<object> EditGoalCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a Goal
        /// </summary>
        public ReactiveCommand<object> InspectGoalCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a ValueGroup
        /// </summary>
        public ReactiveCommand<object> CreateValueGroupCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a ValueGroup
        /// </summary>
        public ReactiveCommand<object> DeleteValueGroupCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a ValueGroup
        /// </summary>
        public ReactiveCommand<object> EditValueGroupCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a ValueGroup
        /// </summary>
        public ReactiveCommand<object> InspectValueGroupCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a StakeholderValue
        /// </summary>
        public ReactiveCommand<object> CreateStakeholderValueCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a StakeholderValue
        /// </summary>
        public ReactiveCommand<object> DeleteStakeholderValueCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a StakeholderValue
        /// </summary>
        public ReactiveCommand<object> EditStakeholderValueCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a StakeholderValue
        /// </summary>
        public ReactiveCommand<object> InspectStakeholderValueCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a StakeHolderValueMap
        /// </summary>
        public ReactiveCommand<object> CreateStakeholderValueMapCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a StakeHolderValueMap
        /// </summary>
        public ReactiveCommand<object> DeleteStakeholderValueMapCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a StakeHolderValueMap
        /// </summary>
        public ReactiveCommand<object> EditStakeholderValueMapCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a StakeHolderValueMap
        /// </summary>
        public ReactiveCommand<object> InspectStakeholderValueMapCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a SharedStyle
        /// </summary>
        public ReactiveCommand<object> CreateSharedDiagramStyleCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a SharedStyle
        /// </summary>
        public ReactiveCommand<object> DeleteSharedDiagramStyleCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a SharedStyle
        /// </summary>
        public ReactiveCommand<object> EditSharedDiagramStyleCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a SharedStyle
        /// </summary>
        public ReactiveCommand<object> InspectSharedDiagramStyleCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a DiagramCanvas
        /// </summary>
        public ReactiveCommand<object> CreateDiagramCanvasCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a DiagramCanvas
        /// </summary>
        public ReactiveCommand<object> DeleteDiagramCanvasCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a DiagramCanvas
        /// </summary>
        public ReactiveCommand<object> EditDiagramCanvasCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a DiagramCanvas
        /// </summary>
        public ReactiveCommand<object> InspectDiagramCanvasCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            
            var canExecuteCreateOptionCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedOptionCommand = this.WhenAny(vm => vm.SelectedOption, v => v.Value != null);
            var canExecuteEditSelectedOptionCommand = this.WhenAny(vm => vm.SelectedOption, v => v.Value != null && !this.IsReadOnly);

            this.CreateOptionCommand = ReactiveCommand.Create(canExecuteCreateOptionCommand);
            this.CreateOptionCommand.Subscribe(_ => this.ExecuteCreateCommand<Option>(this.PopulateOption));

            this.DeleteOptionCommand = ReactiveCommand.Create(canExecuteEditSelectedOptionCommand);
            this.DeleteOptionCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedOption.Thing, this.PopulateOption));

            this.EditOptionCommand = ReactiveCommand.Create(canExecuteEditSelectedOptionCommand);
            this.EditOptionCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedOption.Thing, this.PopulateOption));

            this.InspectOptionCommand = ReactiveCommand.Create(canExecuteInspectSelectedOptionCommand);
            this.InspectOptionCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedOption.Thing));

            this.MoveUpOptionCommand = ReactiveCommand.Create(canExecuteEditSelectedOptionCommand);
            this.MoveUpOptionCommand.Subscribe(_ => this.ExecuteMoveUpCommand(this.Option, this.SelectedOption));

            this.MoveDownOptionCommand = ReactiveCommand.Create(canExecuteEditSelectedOptionCommand);
            this.MoveDownOptionCommand.Subscribe(_ => this.ExecuteMoveDownCommand(this.Option, this.SelectedOption));
            
            var canExecuteCreatePublicationCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedPublicationCommand = this.WhenAny(vm => vm.SelectedPublication, v => v.Value != null);
            var canExecuteEditSelectedPublicationCommand = this.WhenAny(vm => vm.SelectedPublication, v => v.Value != null && !this.IsReadOnly);

            this.CreatePublicationCommand = ReactiveCommand.Create(canExecuteCreatePublicationCommand);
            this.CreatePublicationCommand.Subscribe(_ => this.ExecuteCreateCommand<Publication>(this.PopulatePublication));

            this.DeletePublicationCommand = ReactiveCommand.Create(canExecuteEditSelectedPublicationCommand);
            this.DeletePublicationCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedPublication.Thing, this.PopulatePublication));

            this.EditPublicationCommand = ReactiveCommand.Create(canExecuteEditSelectedPublicationCommand);
            this.EditPublicationCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedPublication.Thing, this.PopulatePublication));

            this.InspectPublicationCommand = ReactiveCommand.Create(canExecuteInspectSelectedPublicationCommand);
            this.InspectPublicationCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedPublication.Thing));
            
            var canExecuteCreatePossibleFiniteStateListCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedPossibleFiniteStateListCommand = this.WhenAny(vm => vm.SelectedPossibleFiniteStateList, v => v.Value != null);
            var canExecuteEditSelectedPossibleFiniteStateListCommand = this.WhenAny(vm => vm.SelectedPossibleFiniteStateList, v => v.Value != null && !this.IsReadOnly);

            this.CreatePossibleFiniteStateListCommand = ReactiveCommand.Create(canExecuteCreatePossibleFiniteStateListCommand);
            this.CreatePossibleFiniteStateListCommand.Subscribe(_ => this.ExecuteCreateCommand<PossibleFiniteStateList>(this.PopulatePossibleFiniteStateList));

            this.DeletePossibleFiniteStateListCommand = ReactiveCommand.Create(canExecuteEditSelectedPossibleFiniteStateListCommand);
            this.DeletePossibleFiniteStateListCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedPossibleFiniteStateList.Thing, this.PopulatePossibleFiniteStateList));

            this.EditPossibleFiniteStateListCommand = ReactiveCommand.Create(canExecuteEditSelectedPossibleFiniteStateListCommand);
            this.EditPossibleFiniteStateListCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedPossibleFiniteStateList.Thing, this.PopulatePossibleFiniteStateList));

            this.InspectPossibleFiniteStateListCommand = ReactiveCommand.Create(canExecuteInspectSelectedPossibleFiniteStateListCommand);
            this.InspectPossibleFiniteStateListCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedPossibleFiniteStateList.Thing));
            
            var canExecuteCreateElementCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedElementCommand = this.WhenAny(vm => vm.SelectedElement, v => v.Value != null);
            var canExecuteEditSelectedElementCommand = this.WhenAny(vm => vm.SelectedElement, v => v.Value != null && !this.IsReadOnly);

            this.CreateElementCommand = ReactiveCommand.Create(canExecuteCreateElementCommand);
            this.CreateElementCommand.Subscribe(_ => this.ExecuteCreateCommand<ElementDefinition>(this.PopulateElement));

            this.DeleteElementCommand = ReactiveCommand.Create(canExecuteEditSelectedElementCommand);
            this.DeleteElementCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedElement.Thing, this.PopulateElement));

            this.EditElementCommand = ReactiveCommand.Create(canExecuteEditSelectedElementCommand);
            this.EditElementCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedElement.Thing, this.PopulateElement));

            this.InspectElementCommand = ReactiveCommand.Create(canExecuteInspectSelectedElementCommand);
            this.InspectElementCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedElement.Thing));
            
            var canExecuteCreateRelationshipCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedRelationshipCommand = this.WhenAny(vm => vm.SelectedRelationship, v => v.Value != null);
            var canExecuteEditSelectedRelationshipCommand = this.WhenAny(vm => vm.SelectedRelationship, v => v.Value != null && !this.IsReadOnly);

            this.CreateRelationshipCommand = ReactiveCommand.Create(canExecuteCreateRelationshipCommand);
            this.CreateRelationshipCommand.Subscribe(_ => this.ExecuteCreateRelationshipCommand());

            this.DeleteRelationshipCommand = ReactiveCommand.Create(canExecuteEditSelectedRelationshipCommand);
            this.DeleteRelationshipCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedRelationship.Thing, this.PopulateRelationship));

            this.EditRelationshipCommand = ReactiveCommand.Create(canExecuteEditSelectedRelationshipCommand);
            this.EditRelationshipCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedRelationship.Thing, this.PopulateRelationship));

            this.InspectRelationshipCommand = ReactiveCommand.Create(canExecuteInspectSelectedRelationshipCommand);
            this.InspectRelationshipCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedRelationship.Thing));
            
            var canExecuteCreateExternalIdentifierMapCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedExternalIdentifierMapCommand = this.WhenAny(vm => vm.SelectedExternalIdentifierMap, v => v.Value != null);
            var canExecuteEditSelectedExternalIdentifierMapCommand = this.WhenAny(vm => vm.SelectedExternalIdentifierMap, v => v.Value != null && !this.IsReadOnly);

            this.CreateExternalIdentifierMapCommand = ReactiveCommand.Create(canExecuteCreateExternalIdentifierMapCommand);
            this.CreateExternalIdentifierMapCommand.Subscribe(_ => this.ExecuteCreateCommand<ExternalIdentifierMap>(this.PopulateExternalIdentifierMap));

            this.DeleteExternalIdentifierMapCommand = ReactiveCommand.Create(canExecuteEditSelectedExternalIdentifierMapCommand);
            this.DeleteExternalIdentifierMapCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedExternalIdentifierMap.Thing, this.PopulateExternalIdentifierMap));

            this.EditExternalIdentifierMapCommand = ReactiveCommand.Create(canExecuteEditSelectedExternalIdentifierMapCommand);
            this.EditExternalIdentifierMapCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedExternalIdentifierMap.Thing, this.PopulateExternalIdentifierMap));

            this.InspectExternalIdentifierMapCommand = ReactiveCommand.Create(canExecuteInspectSelectedExternalIdentifierMapCommand);
            this.InspectExternalIdentifierMapCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedExternalIdentifierMap.Thing));
            
            var canExecuteCreateRequirementsSpecificationCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedRequirementsSpecificationCommand = this.WhenAny(vm => vm.SelectedRequirementsSpecification, v => v.Value != null);
            var canExecuteEditSelectedRequirementsSpecificationCommand = this.WhenAny(vm => vm.SelectedRequirementsSpecification, v => v.Value != null && !this.IsReadOnly);

            this.CreateRequirementsSpecificationCommand = ReactiveCommand.Create(canExecuteCreateRequirementsSpecificationCommand);
            this.CreateRequirementsSpecificationCommand.Subscribe(_ => this.ExecuteCreateCommand<RequirementsSpecification>(this.PopulateRequirementsSpecification));

            this.DeleteRequirementsSpecificationCommand = ReactiveCommand.Create(canExecuteEditSelectedRequirementsSpecificationCommand);
            this.DeleteRequirementsSpecificationCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedRequirementsSpecification.Thing, this.PopulateRequirementsSpecification));

            this.EditRequirementsSpecificationCommand = ReactiveCommand.Create(canExecuteEditSelectedRequirementsSpecificationCommand);
            this.EditRequirementsSpecificationCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedRequirementsSpecification.Thing, this.PopulateRequirementsSpecification));

            this.InspectRequirementsSpecificationCommand = ReactiveCommand.Create(canExecuteInspectSelectedRequirementsSpecificationCommand);
            this.InspectRequirementsSpecificationCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedRequirementsSpecification.Thing));
            
            var canExecuteCreateDomainFileStoreCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedDomainFileStoreCommand = this.WhenAny(vm => vm.SelectedDomainFileStore, v => v.Value != null);
            var canExecuteEditSelectedDomainFileStoreCommand = this.WhenAny(vm => vm.SelectedDomainFileStore, v => v.Value != null && !this.IsReadOnly);

            this.CreateDomainFileStoreCommand = ReactiveCommand.Create(canExecuteCreateDomainFileStoreCommand);
            this.CreateDomainFileStoreCommand.Subscribe(_ => this.ExecuteCreateCommand<DomainFileStore>(this.PopulateDomainFileStore));

            this.DeleteDomainFileStoreCommand = ReactiveCommand.Create(canExecuteEditSelectedDomainFileStoreCommand);
            this.DeleteDomainFileStoreCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedDomainFileStore.Thing, this.PopulateDomainFileStore));

            this.EditDomainFileStoreCommand = ReactiveCommand.Create(canExecuteEditSelectedDomainFileStoreCommand);
            this.EditDomainFileStoreCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedDomainFileStore.Thing, this.PopulateDomainFileStore));

            this.InspectDomainFileStoreCommand = ReactiveCommand.Create(canExecuteInspectSelectedDomainFileStoreCommand);
            this.InspectDomainFileStoreCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedDomainFileStore.Thing));
            
            var canExecuteCreateActualFiniteStateListCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedActualFiniteStateListCommand = this.WhenAny(vm => vm.SelectedActualFiniteStateList, v => v.Value != null);
            var canExecuteEditSelectedActualFiniteStateListCommand = this.WhenAny(vm => vm.SelectedActualFiniteStateList, v => v.Value != null && !this.IsReadOnly);

            this.CreateActualFiniteStateListCommand = ReactiveCommand.Create(canExecuteCreateActualFiniteStateListCommand);
            this.CreateActualFiniteStateListCommand.Subscribe(_ => this.ExecuteCreateCommand<ActualFiniteStateList>(this.PopulateActualFiniteStateList));

            this.DeleteActualFiniteStateListCommand = ReactiveCommand.Create(canExecuteEditSelectedActualFiniteStateListCommand);
            this.DeleteActualFiniteStateListCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedActualFiniteStateList.Thing, this.PopulateActualFiniteStateList));

            this.EditActualFiniteStateListCommand = ReactiveCommand.Create(canExecuteEditSelectedActualFiniteStateListCommand);
            this.EditActualFiniteStateListCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedActualFiniteStateList.Thing, this.PopulateActualFiniteStateList));

            this.InspectActualFiniteStateListCommand = ReactiveCommand.Create(canExecuteInspectSelectedActualFiniteStateListCommand);
            this.InspectActualFiniteStateListCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedActualFiniteStateList.Thing));
            
            var canExecuteCreateRuleVerificationListCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedRuleVerificationListCommand = this.WhenAny(vm => vm.SelectedRuleVerificationList, v => v.Value != null);
            var canExecuteEditSelectedRuleVerificationListCommand = this.WhenAny(vm => vm.SelectedRuleVerificationList, v => v.Value != null && !this.IsReadOnly);

            this.CreateRuleVerificationListCommand = ReactiveCommand.Create(canExecuteCreateRuleVerificationListCommand);
            this.CreateRuleVerificationListCommand.Subscribe(_ => this.ExecuteCreateCommand<RuleVerificationList>(this.PopulateRuleVerificationList));

            this.DeleteRuleVerificationListCommand = ReactiveCommand.Create(canExecuteEditSelectedRuleVerificationListCommand);
            this.DeleteRuleVerificationListCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedRuleVerificationList.Thing, this.PopulateRuleVerificationList));

            this.EditRuleVerificationListCommand = ReactiveCommand.Create(canExecuteEditSelectedRuleVerificationListCommand);
            this.EditRuleVerificationListCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedRuleVerificationList.Thing, this.PopulateRuleVerificationList));

            this.InspectRuleVerificationListCommand = ReactiveCommand.Create(canExecuteInspectSelectedRuleVerificationListCommand);
            this.InspectRuleVerificationListCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedRuleVerificationList.Thing));
            
            var canExecuteCreateStakeholderCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedStakeholderCommand = this.WhenAny(vm => vm.SelectedStakeholder, v => v.Value != null);
            var canExecuteEditSelectedStakeholderCommand = this.WhenAny(vm => vm.SelectedStakeholder, v => v.Value != null && !this.IsReadOnly);

            this.CreateStakeholderCommand = ReactiveCommand.Create(canExecuteCreateStakeholderCommand);
            this.CreateStakeholderCommand.Subscribe(_ => this.ExecuteCreateCommand<Stakeholder>(this.PopulateStakeholder));

            this.DeleteStakeholderCommand = ReactiveCommand.Create(canExecuteEditSelectedStakeholderCommand);
            this.DeleteStakeholderCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedStakeholder.Thing, this.PopulateStakeholder));

            this.EditStakeholderCommand = ReactiveCommand.Create(canExecuteEditSelectedStakeholderCommand);
            this.EditStakeholderCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedStakeholder.Thing, this.PopulateStakeholder));

            this.InspectStakeholderCommand = ReactiveCommand.Create(canExecuteInspectSelectedStakeholderCommand);
            this.InspectStakeholderCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedStakeholder.Thing));
            
            var canExecuteCreateGoalCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedGoalCommand = this.WhenAny(vm => vm.SelectedGoal, v => v.Value != null);
            var canExecuteEditSelectedGoalCommand = this.WhenAny(vm => vm.SelectedGoal, v => v.Value != null && !this.IsReadOnly);

            this.CreateGoalCommand = ReactiveCommand.Create(canExecuteCreateGoalCommand);
            this.CreateGoalCommand.Subscribe(_ => this.ExecuteCreateCommand<Goal>(this.PopulateGoal));

            this.DeleteGoalCommand = ReactiveCommand.Create(canExecuteEditSelectedGoalCommand);
            this.DeleteGoalCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedGoal.Thing, this.PopulateGoal));

            this.EditGoalCommand = ReactiveCommand.Create(canExecuteEditSelectedGoalCommand);
            this.EditGoalCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedGoal.Thing, this.PopulateGoal));

            this.InspectGoalCommand = ReactiveCommand.Create(canExecuteInspectSelectedGoalCommand);
            this.InspectGoalCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedGoal.Thing));
            
            var canExecuteCreateValueGroupCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedValueGroupCommand = this.WhenAny(vm => vm.SelectedValueGroup, v => v.Value != null);
            var canExecuteEditSelectedValueGroupCommand = this.WhenAny(vm => vm.SelectedValueGroup, v => v.Value != null && !this.IsReadOnly);

            this.CreateValueGroupCommand = ReactiveCommand.Create(canExecuteCreateValueGroupCommand);
            this.CreateValueGroupCommand.Subscribe(_ => this.ExecuteCreateCommand<ValueGroup>(this.PopulateValueGroup));

            this.DeleteValueGroupCommand = ReactiveCommand.Create(canExecuteEditSelectedValueGroupCommand);
            this.DeleteValueGroupCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedValueGroup.Thing, this.PopulateValueGroup));

            this.EditValueGroupCommand = ReactiveCommand.Create(canExecuteEditSelectedValueGroupCommand);
            this.EditValueGroupCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedValueGroup.Thing, this.PopulateValueGroup));

            this.InspectValueGroupCommand = ReactiveCommand.Create(canExecuteInspectSelectedValueGroupCommand);
            this.InspectValueGroupCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedValueGroup.Thing));
            
            var canExecuteCreateStakeholderValueCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedStakeholderValueCommand = this.WhenAny(vm => vm.SelectedStakeholderValue, v => v.Value != null);
            var canExecuteEditSelectedStakeholderValueCommand = this.WhenAny(vm => vm.SelectedStakeholderValue, v => v.Value != null && !this.IsReadOnly);

            this.CreateStakeholderValueCommand = ReactiveCommand.Create(canExecuteCreateStakeholderValueCommand);
            this.CreateStakeholderValueCommand.Subscribe(_ => this.ExecuteCreateCommand<StakeholderValue>(this.PopulateStakeholderValue));

            this.DeleteStakeholderValueCommand = ReactiveCommand.Create(canExecuteEditSelectedStakeholderValueCommand);
            this.DeleteStakeholderValueCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedStakeholderValue.Thing, this.PopulateStakeholderValue));

            this.EditStakeholderValueCommand = ReactiveCommand.Create(canExecuteEditSelectedStakeholderValueCommand);
            this.EditStakeholderValueCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedStakeholderValue.Thing, this.PopulateStakeholderValue));

            this.InspectStakeholderValueCommand = ReactiveCommand.Create(canExecuteInspectSelectedStakeholderValueCommand);
            this.InspectStakeholderValueCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedStakeholderValue.Thing));
            
            var canExecuteCreateStakeholderValueMapCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedStakeholderValueMapCommand = this.WhenAny(vm => vm.SelectedStakeholderValueMap, v => v.Value != null);
            var canExecuteEditSelectedStakeholderValueMapCommand = this.WhenAny(vm => vm.SelectedStakeholderValueMap, v => v.Value != null && !this.IsReadOnly);

            this.CreateStakeholderValueMapCommand = ReactiveCommand.Create(canExecuteCreateStakeholderValueMapCommand);
            this.CreateStakeholderValueMapCommand.Subscribe(_ => this.ExecuteCreateCommand<StakeHolderValueMap>(this.PopulateStakeholderValueMap));

            this.DeleteStakeholderValueMapCommand = ReactiveCommand.Create(canExecuteEditSelectedStakeholderValueMapCommand);
            this.DeleteStakeholderValueMapCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedStakeholderValueMap.Thing, this.PopulateStakeholderValueMap));

            this.EditStakeholderValueMapCommand = ReactiveCommand.Create(canExecuteEditSelectedStakeholderValueMapCommand);
            this.EditStakeholderValueMapCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedStakeholderValueMap.Thing, this.PopulateStakeholderValueMap));

            this.InspectStakeholderValueMapCommand = ReactiveCommand.Create(canExecuteInspectSelectedStakeholderValueMapCommand);
            this.InspectStakeholderValueMapCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedStakeholderValueMap.Thing));
            
            var canExecuteCreateSharedDiagramStyleCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedSharedDiagramStyleCommand = this.WhenAny(vm => vm.SelectedSharedDiagramStyle, v => v.Value != null);
            var canExecuteEditSelectedSharedDiagramStyleCommand = this.WhenAny(vm => vm.SelectedSharedDiagramStyle, v => v.Value != null && !this.IsReadOnly);

            this.CreateSharedDiagramStyleCommand = ReactiveCommand.Create(canExecuteCreateSharedDiagramStyleCommand);
            this.CreateSharedDiagramStyleCommand.Subscribe(_ => this.ExecuteCreateCommand<SharedStyle>(this.PopulateSharedDiagramStyle));

            this.DeleteSharedDiagramStyleCommand = ReactiveCommand.Create(canExecuteEditSelectedSharedDiagramStyleCommand);
            this.DeleteSharedDiagramStyleCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedSharedDiagramStyle.Thing, this.PopulateSharedDiagramStyle));

            this.EditSharedDiagramStyleCommand = ReactiveCommand.Create(canExecuteEditSelectedSharedDiagramStyleCommand);
            this.EditSharedDiagramStyleCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedSharedDiagramStyle.Thing, this.PopulateSharedDiagramStyle));

            this.InspectSharedDiagramStyleCommand = ReactiveCommand.Create(canExecuteInspectSelectedSharedDiagramStyleCommand);
            this.InspectSharedDiagramStyleCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedSharedDiagramStyle.Thing));
            
            var canExecuteCreateDiagramCanvasCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedDiagramCanvasCommand = this.WhenAny(vm => vm.SelectedDiagramCanvas, v => v.Value != null);
            var canExecuteEditSelectedDiagramCanvasCommand = this.WhenAny(vm => vm.SelectedDiagramCanvas, v => v.Value != null && !this.IsReadOnly);

            this.CreateDiagramCanvasCommand = ReactiveCommand.Create(canExecuteCreateDiagramCanvasCommand);
            this.CreateDiagramCanvasCommand.Subscribe(_ => this.ExecuteCreateCommand<DiagramCanvas>(this.PopulateDiagramCanvas));

            this.DeleteDiagramCanvasCommand = ReactiveCommand.Create(canExecuteEditSelectedDiagramCanvasCommand);
            this.DeleteDiagramCanvasCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedDiagramCanvas.Thing, this.PopulateDiagramCanvas));

            this.EditDiagramCanvasCommand = ReactiveCommand.Create(canExecuteEditSelectedDiagramCanvasCommand);
            this.EditDiagramCanvasCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedDiagramCanvas.Thing, this.PopulateDiagramCanvas));

            this.InspectDiagramCanvasCommand = ReactiveCommand.Create(canExecuteInspectSelectedDiagramCanvasCommand);
            this.InspectDiagramCanvasCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedDiagramCanvas.Thing));
            var canExecuteInspectSelectedIterationSetupCommand = this.WhenAny(vm => vm.SelectedIterationSetup, v => v.Value != null);
            this.InspectSelectedIterationSetupCommand = ReactiveCommand.Create(canExecuteInspectSelectedIterationSetupCommand);
            this.InspectSelectedIterationSetupCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedIterationSetup));
            var canExecuteInspectSelectedTopElementCommand = this.WhenAny(vm => vm.SelectedTopElement, v => v.Value != null);
            this.InspectSelectedTopElementCommand = ReactiveCommand.Create(canExecuteInspectSelectedTopElementCommand);
            this.InspectSelectedTopElementCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedTopElement));
            var canExecuteInspectSelectedDefaultOptionCommand = this.WhenAny(vm => vm.SelectedDefaultOption, v => v.Value != null);
            this.InspectSelectedDefaultOptionCommand = ReactiveCommand.Create(canExecuteInspectSelectedDefaultOptionCommand);
            this.InspectSelectedDefaultOptionCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedDefaultOption));
        }

        /// <summary>
        /// Executes the Create <see cref="ICommand"/> for the abstract <see cref="Relationship"/>
        /// </summary>
        protected void ExecuteCreateRelationshipCommand()
        {
            switch (this.SelectedRelationshipKind)
            {
                case ClassKind.MultiRelationship:
                    this.ExecuteCreateCommand<MultiRelationship>(this.PopulateRelationship);
                    break;
                case ClassKind.BinaryRelationship:
                    this.ExecuteCreateCommand<BinaryRelationship>(this.PopulateRelationship);
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

            clone.SourceIterationIid = this.SourceIterationIid;
            clone.IterationSetup = this.SelectedIterationSetup;
            clone.TopElement = this.SelectedTopElement;
            clone.DefaultOption = this.SelectedDefaultOption;

            if (!clone.Option.SortedItems.Values.SequenceEqual(this.Option.Select(x => x.Thing)))
            {
                var itemCount = this.Option.Count;
                for (var i = 0; i < itemCount; i++)
                {
                    var item = this.Option[i].Thing;
                    var currentIndex = clone.Option.IndexOf(item);

                    if (currentIndex != i)
                    {
                        clone.Option.Move(currentIndex, i);
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
            this.PossibleIterationSetup = new ReactiveList<IterationSetup>();
            this.PossibleTopElement = new ReactiveList<ElementDefinition>();
            this.PossibleDefaultOption = new ReactiveList<Option>();
            this.Option = new ReactiveList<OptionRowViewModel>();
            this.Publication = new ReactiveList<PublicationRowViewModel>();
            this.PossibleFiniteStateList = new ReactiveList<PossibleFiniteStateListRowViewModel>();
            this.Element = new ReactiveList<ElementDefinitionRowViewModel>();
            this.Relationship = new ReactiveList<IRowViewModelBase<Relationship>>();
            this.ExternalIdentifierMap = new ReactiveList<ExternalIdentifierMapRowViewModel>();
            this.RequirementsSpecification = new ReactiveList<RequirementsSpecificationRowViewModel>();
            this.DomainFileStore = new ReactiveList<DomainFileStoreRowViewModel>();
            this.ActualFiniteStateList = new ReactiveList<ActualFiniteStateListRowViewModel>();
            this.RuleVerificationList = new ReactiveList<RuleVerificationListRowViewModel>();
            this.Stakeholder = new ReactiveList<StakeholderRowViewModel>();
            this.Goal = new ReactiveList<GoalRowViewModel>();
            this.ValueGroup = new ReactiveList<ValueGroupRowViewModel>();
            this.StakeholderValue = new ReactiveList<StakeholderValueRowViewModel>();
            this.StakeholderValueMap = new ReactiveList<StakeHolderValueMapRowViewModel>();
            this.SharedDiagramStyle = new ReactiveList<SharedStyleRowViewModel>();
            this.DiagramCanvas = new ReactiveList<DiagramCanvasRowViewModel>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.SourceIterationIid = this.Thing.SourceIterationIid;
            this.SelectedIterationSetup = this.Thing.IterationSetup;
            this.PopulatePossibleIterationSetup();
            this.SelectedTopElement = this.Thing.TopElement;
            this.PopulatePossibleTopElement();
            this.SelectedDefaultOption = this.Thing.DefaultOption;
            this.PopulatePossibleDefaultOption();
            this.PopulateOption();
            this.PopulatePublication();
            this.PopulatePossibleFiniteStateList();
            this.PopulateElement();
            this.PopulateRelationship();
            this.PopulateExternalIdentifierMap();
            this.PopulateRequirementsSpecification();
            this.PopulateDomainFileStore();
            this.PopulateActualFiniteStateList();
            this.PopulateRuleVerificationList();
            this.PopulateStakeholder();
            this.PopulateGoal();
            this.PopulateValueGroup();
            this.PopulateStakeholderValue();
            this.PopulateStakeholderValueMap();
            this.PopulateSharedDiagramStyle();
            this.PopulateDiagramCanvas();
        }

        /// <summary>
        /// Populates the <see cref="Option"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateOption()
        {
            this.Option.Clear();
            foreach (Option thing in this.Thing.Option.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new OptionRowViewModel(thing, this.Session, this);
                this.Option.Add(row);
                row.Index = this.Thing.Option.IndexOf(thing);
            }
        }

        /// <summary>
        /// Populates the <see cref="Publication"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulatePublication()
        {
            this.Publication.Clear();
            foreach (var thing in this.Thing.Publication.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new PublicationRowViewModel(thing, this.Session, this);
                this.Publication.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="PossibleFiniteStateList"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulatePossibleFiniteStateList()
        {
            this.PossibleFiniteStateList.Clear();
            foreach (var thing in this.Thing.PossibleFiniteStateList.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new PossibleFiniteStateListRowViewModel(thing, this.Session, this);
                this.PossibleFiniteStateList.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="Element"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateElement()
        {
            this.Element.Clear();
            foreach (var thing in this.Thing.Element.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new ElementDefinitionRowViewModel(thing, this.Session, this);
                this.Element.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="Relationship"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateRelationship()
        {
            // this method needs to be overriden.
        }

        /// <summary>
        /// Populates the <see cref="ExternalIdentifierMap"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateExternalIdentifierMap()
        {
            this.ExternalIdentifierMap.Clear();
            foreach (var thing in this.Thing.ExternalIdentifierMap.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new ExternalIdentifierMapRowViewModel(thing, this.Session, this);
                this.ExternalIdentifierMap.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="RequirementsSpecification"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateRequirementsSpecification()
        {
            this.RequirementsSpecification.Clear();
            foreach (var thing in this.Thing.RequirementsSpecification.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new RequirementsSpecificationRowViewModel(thing, this.Session, this);
                this.RequirementsSpecification.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="DomainFileStore"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateDomainFileStore()
        {
            this.DomainFileStore.Clear();
            foreach (var thing in this.Thing.DomainFileStore.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new DomainFileStoreRowViewModel(thing, this.Session, this);
                this.DomainFileStore.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="ActualFiniteStateList"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateActualFiniteStateList()
        {
            this.ActualFiniteStateList.Clear();
            foreach (var thing in this.Thing.ActualFiniteStateList.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new ActualFiniteStateListRowViewModel(thing, this.Session, this);
                this.ActualFiniteStateList.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="RuleVerificationList"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateRuleVerificationList()
        {
            this.RuleVerificationList.Clear();
            foreach (var thing in this.Thing.RuleVerificationList.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new RuleVerificationListRowViewModel(thing, this.Session, this);
                this.RuleVerificationList.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="Stakeholder"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateStakeholder()
        {
            this.Stakeholder.Clear();
            foreach (var thing in this.Thing.Stakeholder.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new StakeholderRowViewModel(thing, this.Session, this);
                this.Stakeholder.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="Goal"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateGoal()
        {
            this.Goal.Clear();
            foreach (var thing in this.Thing.Goal.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new GoalRowViewModel(thing, this.Session, this);
                this.Goal.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="ValueGroup"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateValueGroup()
        {
            this.ValueGroup.Clear();
            foreach (var thing in this.Thing.ValueGroup.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new ValueGroupRowViewModel(thing, this.Session, this);
                this.ValueGroup.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="StakeholderValue"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateStakeholderValue()
        {
            this.StakeholderValue.Clear();
            foreach (var thing in this.Thing.StakeholderValue.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new StakeholderValueRowViewModel(thing, this.Session, this);
                this.StakeholderValue.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="StakeholderValueMap"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateStakeholderValueMap()
        {
            this.StakeholderValueMap.Clear();
            foreach (var thing in this.Thing.StakeholderValueMap.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new StakeHolderValueMapRowViewModel(thing, this.Session, this);
                this.StakeholderValueMap.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="SharedDiagramStyle"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateSharedDiagramStyle()
        {
            this.SharedDiagramStyle.Clear();
            foreach (var thing in this.Thing.SharedDiagramStyle.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new SharedStyleRowViewModel(thing, this.Session, this);
                this.SharedDiagramStyle.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="DiagramCanvas"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateDiagramCanvas()
        {
            this.DiagramCanvas.Clear();
            foreach (var thing in this.Thing.DiagramCanvas.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new DiagramCanvasRowViewModel(thing, this.Session, this);
                this.DiagramCanvas.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="PossibleIterationSetup"/> property
        /// </summary>
        protected virtual void PopulatePossibleIterationSetup()
        {
            this.PossibleIterationSetup.Clear();
        }

        /// <summary>
        /// Populates the <see cref="PossibleTopElement"/> property
        /// </summary>
        protected virtual void PopulatePossibleTopElement()
        {
            this.PossibleTopElement.Clear();
        }

        /// <summary>
        /// Populates the <see cref="PossibleDefaultOption"/> property
        /// </summary>
        protected virtual void PopulatePossibleDefaultOption()
        {
            this.PossibleDefaultOption.Clear();
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
            foreach(var option in this.Option)
            {
                option.Dispose();
            }
            foreach(var publication in this.Publication)
            {
                publication.Dispose();
            }
            foreach(var possibleFiniteStateList in this.PossibleFiniteStateList)
            {
                possibleFiniteStateList.Dispose();
            }
            foreach(var element in this.Element)
            {
                element.Dispose();
            }
            foreach(var relationship in this.Relationship)
            {
                relationship.Dispose();
            }
            foreach(var externalIdentifierMap in this.ExternalIdentifierMap)
            {
                externalIdentifierMap.Dispose();
            }
            foreach(var requirementsSpecification in this.RequirementsSpecification)
            {
                requirementsSpecification.Dispose();
            }
            foreach(var domainFileStore in this.DomainFileStore)
            {
                domainFileStore.Dispose();
            }
            foreach(var actualFiniteStateList in this.ActualFiniteStateList)
            {
                actualFiniteStateList.Dispose();
            }
            foreach(var ruleVerificationList in this.RuleVerificationList)
            {
                ruleVerificationList.Dispose();
            }
            foreach(var stakeholder in this.Stakeholder)
            {
                stakeholder.Dispose();
            }
            foreach(var goal in this.Goal)
            {
                goal.Dispose();
            }
            foreach(var valueGroup in this.ValueGroup)
            {
                valueGroup.Dispose();
            }
            foreach(var stakeholderValue in this.StakeholderValue)
            {
                stakeholderValue.Dispose();
            }
            foreach(var stakeholderValueMap in this.StakeholderValueMap)
            {
                stakeholderValueMap.Dispose();
            }
            foreach(var sharedDiagramStyle in this.SharedDiagramStyle)
            {
                sharedDiagramStyle.Dispose();
            }
            foreach(var diagramCanvas in this.DiagramCanvas)
            {
                diagramCanvas.Dispose();
            }
        }
    }
}
