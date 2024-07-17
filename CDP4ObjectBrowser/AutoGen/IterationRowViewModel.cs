// -------------------------------------------------------------------------------------------------
// <copyright file="IterationRowViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2017 Starion Group S.A.
// </copyright>
// <summary>
//   This is an auto-generated class. Any manual changes on this file will be overwritten!
// </summary>
// -------------------------------------------------------------------------------------------------

namespace CDP4ObjectBrowser
{
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.DiagramData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Common.ReportingData;
    using System;
    using System.Reactive.Linq;

    /// <summary>
    /// Row class representing a <see cref="Iteration"/>
    /// </summary>
    public partial class IterationRowViewModel : ObjectBrowserRowViewModel<Iteration>
    {
        /// <summary>
        /// Intermediate folder containing <see cref="OptionRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel optionFolder;

        /// <summary>
        /// Intermediate folder containing <see cref="PublicationRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel publicationFolder;

        /// <summary>
        /// Intermediate folder containing <see cref="PossibleFiniteStateListRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel possibleFiniteStateListFolder;

        /// <summary>
        /// Intermediate folder containing <see cref="ElementRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel elementFolder;

        /// <summary>
        /// Intermediate folder containing <see cref="RelationshipRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel relationshipFolder;

        /// <summary>
        /// Intermediate folder containing <see cref="ExternalIdentifierMapRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel externalIdentifierMapFolder;

        /// <summary>
        /// Intermediate folder containing <see cref="RequirementsSpecificationRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel requirementsSpecificationFolder;

        /// <summary>
        /// Intermediate folder containing <see cref="DomainFileStoreRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel domainFileStoreFolder;

        /// <summary>
        /// Intermediate folder containing <see cref="ActualFiniteStateListRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel actualFiniteStateListFolder;

        /// <summary>
        /// Intermediate folder containing <see cref="RuleVerificationListRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel ruleVerificationListFolder;

        /// <summary>
        /// Intermediate folder containing <see cref="StakeholderRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel stakeholderFolder;

        /// <summary>
        /// Intermediate folder containing <see cref="GoalRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel goalFolder;

        /// <summary>
        /// Intermediate folder containing <see cref="ValueGroupRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel valueGroupFolder;

        /// <summary>
        /// Intermediate folder containing <see cref="StakeholderValueRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel stakeholderValueFolder;

        /// <summary>
        /// Intermediate folder containing <see cref="StakeholderValueMapRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel stakeholderValueMapFolder;

        /// <summary>
        /// Intermediate folder containing <see cref="SharedDiagramStyleRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel sharedDiagramStyleFolder;

        /// <summary>
        /// Intermediate folder containing <see cref="DiagramCanvasRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel diagramCanvasFolder;

        /// <summary>
        /// Initializes a new instance of the <see cref="IterationRowViewModel"/> class
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase"/> that is the container of this <see cref="IRowViewModelBase"/></param>
        public IterationRowViewModel(Iteration iteration, ISession session, IViewModelBase<Thing> containerViewModel) : base(iteration, session, containerViewModel)
        {
            this.optionFolder = new CDP4Composition.FolderRowViewModel("Option", "Option", this.Session, this);
            this.ContainedRows.Add(this.optionFolder);
            this.publicationFolder = new CDP4Composition.FolderRowViewModel("Publication", "Publication", this.Session, this);
            this.ContainedRows.Add(this.publicationFolder);
            this.possibleFiniteStateListFolder = new CDP4Composition.FolderRowViewModel("Possible Finite State List", "Possible Finite State List", this.Session, this);
            this.ContainedRows.Add(this.possibleFiniteStateListFolder);
            this.elementFolder = new CDP4Composition.FolderRowViewModel("Element", "Element", this.Session, this);
            this.ContainedRows.Add(this.elementFolder);
            this.relationshipFolder = new CDP4Composition.FolderRowViewModel("Relationship", "Relationship", this.Session, this);
            this.ContainedRows.Add(this.relationshipFolder);
            this.externalIdentifierMapFolder = new CDP4Composition.FolderRowViewModel("External Identifier Map", "External Identifier Map", this.Session, this);
            this.ContainedRows.Add(this.externalIdentifierMapFolder);
            this.requirementsSpecificationFolder = new CDP4Composition.FolderRowViewModel("Requirements Specification", "Requirements Specification", this.Session, this);
            this.ContainedRows.Add(this.requirementsSpecificationFolder);
            this.domainFileStoreFolder = new CDP4Composition.FolderRowViewModel("Domain File Store", "Domain File Store", this.Session, this);
            this.ContainedRows.Add(this.domainFileStoreFolder);
            this.actualFiniteStateListFolder = new CDP4Composition.FolderRowViewModel("Actual Finite State List", "Actual Finite State List", this.Session, this);
            this.ContainedRows.Add(this.actualFiniteStateListFolder);
            this.ruleVerificationListFolder = new CDP4Composition.FolderRowViewModel("Rule Verification List", "Rule Verification List", this.Session, this);
            this.ContainedRows.Add(this.ruleVerificationListFolder);
            this.stakeholderFolder = new CDP4Composition.FolderRowViewModel("Stakeholder", "Stakeholder", this.Session, this);
            this.ContainedRows.Add(this.stakeholderFolder);
            this.goalFolder = new CDP4Composition.FolderRowViewModel("Goal", "Goal", this.Session, this);
            this.ContainedRows.Add(this.goalFolder);
            this.valueGroupFolder = new CDP4Composition.FolderRowViewModel("Value Group", "Value Group", this.Session, this);
            this.ContainedRows.Add(this.valueGroupFolder);
            this.stakeholderValueFolder = new CDP4Composition.FolderRowViewModel("Stakeholder Value", "Stakeholder Value", this.Session, this);
            this.ContainedRows.Add(this.stakeholderValueFolder);
            this.stakeholderValueMapFolder = new CDP4Composition.FolderRowViewModel("Stakeholder Value Map", "Stakeholder Value Map", this.Session, this);
            this.ContainedRows.Add(this.stakeholderValueMapFolder);
            this.sharedDiagramStyleFolder = new CDP4Composition.FolderRowViewModel("Shared Diagram Style", "Shared Diagram Style", this.Session, this);
            this.ContainedRows.Add(this.sharedDiagramStyleFolder);
            this.diagramCanvasFolder = new CDP4Composition.FolderRowViewModel("Diagram Canvas", "Diagram Canvas", this.Session, this);
            this.ContainedRows.Add(this.diagramCanvasFolder);
            this.UpdateProperties();
            this.UpdateColumnValues();
        }

        /// <summary>
        /// The event-handler that is invoked by the subscription that listens for updates
        /// on the <see cref="Thing"/> that is being represented by the view-model
        /// </summary>
        /// <param name="objectChange">
        /// The payload of the event that is being handled
        /// </param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateProperties();
        }

        /// <summary>
        /// Updates all the properties rows
        /// /// </summary>
        private void UpdateProperties()
        {
            this.ComputeRows(this.Thing.Option, this.optionFolder, this.AddOptionRowViewModel);
            this.ComputeRows(this.Thing.Publication, this.publicationFolder, this.AddPublicationRowViewModel);
            this.ComputeRows(this.Thing.PossibleFiniteStateList, this.possibleFiniteStateListFolder, this.AddPossibleFiniteStateListRowViewModel);
            this.ComputeRows(this.Thing.Element, this.elementFolder, this.AddElementRowViewModel);
            this.ComputeRows(this.Thing.Relationship, this.relationshipFolder, this.AddRelationshipRowViewModel);
            this.ComputeRows(this.Thing.ExternalIdentifierMap, this.externalIdentifierMapFolder, this.AddExternalIdentifierMapRowViewModel);
            this.ComputeRows(this.Thing.RequirementsSpecification, this.requirementsSpecificationFolder, this.AddRequirementsSpecificationRowViewModel);
            this.ComputeRows(this.Thing.DomainFileStore, this.domainFileStoreFolder, this.AddDomainFileStoreRowViewModel);
            this.ComputeRows(this.Thing.ActualFiniteStateList, this.actualFiniteStateListFolder, this.AddActualFiniteStateListRowViewModel);
            this.ComputeRows(this.Thing.RuleVerificationList, this.ruleVerificationListFolder, this.AddRuleVerificationListRowViewModel);
            this.ComputeRows(this.Thing.Stakeholder, this.stakeholderFolder, this.AddStakeholderRowViewModel);
            this.ComputeRows(this.Thing.Goal, this.goalFolder, this.AddGoalRowViewModel);
            this.ComputeRows(this.Thing.ValueGroup, this.valueGroupFolder, this.AddValueGroupRowViewModel);
            this.ComputeRows(this.Thing.StakeholderValue, this.stakeholderValueFolder, this.AddStakeholderValueRowViewModel);
            this.ComputeRows(this.Thing.StakeholderValueMap, this.stakeholderValueMapFolder, this.AddStakeholderValueMapRowViewModel);
            this.ComputeRows(this.Thing.SharedDiagramStyle, this.sharedDiagramStyleFolder, this.AddSharedDiagramStyleRowViewModel);
            this.ComputeRows(this.Thing.DiagramCanvas, this.diagramCanvasFolder, this.AddDiagramCanvasRowViewModel);
        }
        /// <summary>
        /// Add an Option row view model to the list of <see cref="Option"/>
        /// </summary>
        /// <param name="option">
        /// The <see cref="Option"/> that is to be added
        /// </param>
        private OptionRowViewModel AddOptionRowViewModel(Option option)
        {
            return new OptionRowViewModel(option, this.Session, this);
        }
        /// <summary>
        /// Add an Publication row view model to the list of <see cref="Publication"/>
        /// </summary>
        /// <param name="publication">
        /// The <see cref="Publication"/> that is to be added
        /// </param>
        private PublicationRowViewModel AddPublicationRowViewModel(Publication publication)
        {
            return new PublicationRowViewModel(publication, this.Session, this);
        }
        /// <summary>
        /// Add an Possible Finite State List row view model to the list of <see cref="PossibleFiniteStateList"/>
        /// </summary>
        /// <param name="possibleFiniteStateList">
        /// The <see cref="PossibleFiniteStateList"/> that is to be added
        /// </param>
        private PossibleFiniteStateListRowViewModel AddPossibleFiniteStateListRowViewModel(PossibleFiniteStateList possibleFiniteStateList)
        {
            return new PossibleFiniteStateListRowViewModel(possibleFiniteStateList, this.Session, this);
        }
        /// <summary>
        /// Add an Element row view model to the list of <see cref="Element"/>
        /// </summary>
        /// <param name="element">
        /// The <see cref="Element"/> that is to be added
        /// </param>
        private ElementDefinitionRowViewModel AddElementRowViewModel(ElementDefinition element)
        {
            return new ElementDefinitionRowViewModel(element, this.Session, this);
        }
        /// <summary>
        /// Add an Relationship row view model to the list of <see cref="Relationship"/>
        /// </summary>
        /// <param name="relationship">
        /// The <see cref="Relationship"/> that is to be added
        /// </param>
        private IRelationshipRowViewModel<Relationship> AddRelationshipRowViewModel(Relationship relationship)
        {
        var multiRelationship = relationship as MultiRelationship;
        if (multiRelationship != null)
        {
            return new MultiRelationshipRowViewModel(multiRelationship, this.Session, this);
        }
        var binaryRelationship = relationship as BinaryRelationship;
        if (binaryRelationship != null)
        {
            return new BinaryRelationshipRowViewModel(binaryRelationship, this.Session, this);
        }
        throw new Exception("No Relationship to return");
        }
        /// <summary>
        /// Add an External Identifier Map row view model to the list of <see cref="ExternalIdentifierMap"/>
        /// </summary>
        /// <param name="externalIdentifierMap">
        /// The <see cref="ExternalIdentifierMap"/> that is to be added
        /// </param>
        private ExternalIdentifierMapRowViewModel AddExternalIdentifierMapRowViewModel(ExternalIdentifierMap externalIdentifierMap)
        {
            return new ExternalIdentifierMapRowViewModel(externalIdentifierMap, this.Session, this);
        }
        /// <summary>
        /// Add an Requirements Specification row view model to the list of <see cref="RequirementsSpecification"/>
        /// </summary>
        /// <param name="requirementsSpecification">
        /// The <see cref="RequirementsSpecification"/> that is to be added
        /// </param>
        private RequirementsSpecificationRowViewModel AddRequirementsSpecificationRowViewModel(RequirementsSpecification requirementsSpecification)
        {
            return new RequirementsSpecificationRowViewModel(requirementsSpecification, this.Session, this);
        }
        /// <summary>
        /// Add an Domain File Store row view model to the list of <see cref="DomainFileStore"/>
        /// </summary>
        /// <param name="domainFileStore">
        /// The <see cref="DomainFileStore"/> that is to be added
        /// </param>
        private DomainFileStoreRowViewModel AddDomainFileStoreRowViewModel(DomainFileStore domainFileStore)
        {
            return new DomainFileStoreRowViewModel(domainFileStore, this.Session, this);
        }
        /// <summary>
        /// Add an Actual Finite State List row view model to the list of <see cref="ActualFiniteStateList"/>
        /// </summary>
        /// <param name="actualFiniteStateList">
        /// The <see cref="ActualFiniteStateList"/> that is to be added
        /// </param>
        private ActualFiniteStateListRowViewModel AddActualFiniteStateListRowViewModel(ActualFiniteStateList actualFiniteStateList)
        {
            return new ActualFiniteStateListRowViewModel(actualFiniteStateList, this.Session, this);
        }
        /// <summary>
        /// Add an Rule Verification List row view model to the list of <see cref="RuleVerificationList"/>
        /// </summary>
        /// <param name="ruleVerificationList">
        /// The <see cref="RuleVerificationList"/> that is to be added
        /// </param>
        private RuleVerificationListRowViewModel AddRuleVerificationListRowViewModel(RuleVerificationList ruleVerificationList)
        {
            return new RuleVerificationListRowViewModel(ruleVerificationList, this.Session, this);
        }
        /// <summary>
        /// Add an Stakeholder row view model to the list of <see cref="Stakeholder"/>
        /// </summary>
        /// <param name="stakeholder">
        /// The <see cref="Stakeholder"/> that is to be added
        /// </param>
        private StakeholderRowViewModel AddStakeholderRowViewModel(Stakeholder stakeholder)
        {
            return new StakeholderRowViewModel(stakeholder, this.Session, this);
        }
        /// <summary>
        /// Add an Goal row view model to the list of <see cref="Goal"/>
        /// </summary>
        /// <param name="goal">
        /// The <see cref="Goal"/> that is to be added
        /// </param>
        private GoalRowViewModel AddGoalRowViewModel(Goal goal)
        {
            return new GoalRowViewModel(goal, this.Session, this);
        }
        /// <summary>
        /// Add an Value Group row view model to the list of <see cref="ValueGroup"/>
        /// </summary>
        /// <param name="valueGroup">
        /// The <see cref="ValueGroup"/> that is to be added
        /// </param>
        private ValueGroupRowViewModel AddValueGroupRowViewModel(ValueGroup valueGroup)
        {
            return new ValueGroupRowViewModel(valueGroup, this.Session, this);
        }
        /// <summary>
        /// Add an Stakeholder Value row view model to the list of <see cref="StakeholderValue"/>
        /// </summary>
        /// <param name="stakeholderValue">
        /// The <see cref="StakeholderValue"/> that is to be added
        /// </param>
        private StakeholderValueRowViewModel AddStakeholderValueRowViewModel(StakeholderValue stakeholderValue)
        {
            return new StakeholderValueRowViewModel(stakeholderValue, this.Session, this);
        }
        /// <summary>
        /// Add an Stakeholder Value Map row view model to the list of <see cref="StakeholderValueMap"/>
        /// </summary>
        /// <param name="stakeholderValueMap">
        /// The <see cref="StakeholderValueMap"/> that is to be added
        /// </param>
        private StakeHolderValueMapRowViewModel AddStakeholderValueMapRowViewModel(StakeHolderValueMap stakeholderValueMap)
        {
            return new StakeHolderValueMapRowViewModel(stakeholderValueMap, this.Session, this);
        }
        /// <summary>
        /// Add an Shared Diagram Style row view model to the list of <see cref="SharedDiagramStyle"/>
        /// </summary>
        /// <param name="sharedDiagramStyle">
        /// The <see cref="SharedDiagramStyle"/> that is to be added
        /// </param>
        private SharedStyleRowViewModel AddSharedDiagramStyleRowViewModel(SharedStyle sharedDiagramStyle)
        {
            return new SharedStyleRowViewModel(sharedDiagramStyle, this.Session, this);
        }
        /// <summary>
        /// Add an Diagram Canvas row view model to the list of <see cref="DiagramCanvas"/>
        /// </summary>
        /// <param name="diagramCanvas">
        /// The <see cref="DiagramCanvas"/> that is to be added
        /// </param>
        private DiagramCanvasRowViewModel AddDiagramCanvasRowViewModel(DiagramCanvas diagramCanvas)
        {
            return new DiagramCanvasRowViewModel(diagramCanvas, this.Session, this);
        }
    }
}
