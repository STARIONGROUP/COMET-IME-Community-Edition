// -------------------------------------------------------------------------------------------------
// <copyright file="ReferenceDataLibraryRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2017 RHEA System S.A.
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
    /// Row class representing a <see cref="ReferenceDataLibrary"/>
    /// </summary>
    public abstract partial class ReferenceDataLibraryRowViewModel<T> : DefinedThingRowViewModel<T>, IReferenceDataLibraryRowViewModel<T> where T :ReferenceDataLibrary
    {
        /// <summary>
        /// Intermediate folder containing <see cref="DefinedCategoryRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel definedCategoryFolder;

        /// <summary>
        /// Intermediate folder containing <see cref="ParameterTypeRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel parameterTypeFolder;

        /// <summary>
        /// Intermediate folder containing <see cref="ScaleRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel scaleFolder;

        /// <summary>
        /// Intermediate folder containing <see cref="UnitPrefixRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel unitPrefixFolder;

        /// <summary>
        /// Intermediate folder containing <see cref="UnitRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel unitFolder;

        /// <summary>
        /// Intermediate folder containing <see cref="FileTypeRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel fileTypeFolder;

        /// <summary>
        /// Intermediate folder containing <see cref="GlossaryRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel glossaryFolder;

        /// <summary>
        /// Intermediate folder containing <see cref="ReferenceSourceRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel referenceSourceFolder;

        /// <summary>
        /// Intermediate folder containing <see cref="RuleRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel ruleFolder;

        /// <summary>
        /// Intermediate folder containing <see cref="ConstantRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel constantFolder;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceDataLibraryRowViewModel{T}"/> class
        /// </summary>
        /// <param name="referenceDataLibrary">The <see cref="ReferenceDataLibrary"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase"/> that is the container of this <see cref="IRowViewModelBase"/></param>
        protected ReferenceDataLibraryRowViewModel(T referenceDataLibrary, ISession session, IViewModelBase<Thing> containerViewModel) : base(referenceDataLibrary, session, containerViewModel)
        {
            this.definedCategoryFolder = new CDP4Composition.FolderRowViewModel("Defined Category", "Defined Category", this.Session, this);
            this.ContainedRows.Add(this.definedCategoryFolder);
            this.parameterTypeFolder = new CDP4Composition.FolderRowViewModel("Parameter Type", "Parameter Type", this.Session, this);
            this.ContainedRows.Add(this.parameterTypeFolder);
            this.scaleFolder = new CDP4Composition.FolderRowViewModel("Scale", "Scale", this.Session, this);
            this.ContainedRows.Add(this.scaleFolder);
            this.unitPrefixFolder = new CDP4Composition.FolderRowViewModel("Unit Prefix", "Unit Prefix", this.Session, this);
            this.ContainedRows.Add(this.unitPrefixFolder);
            this.unitFolder = new CDP4Composition.FolderRowViewModel("Unit", "Unit", this.Session, this);
            this.ContainedRows.Add(this.unitFolder);
            this.fileTypeFolder = new CDP4Composition.FolderRowViewModel("File Type", "File Type", this.Session, this);
            this.ContainedRows.Add(this.fileTypeFolder);
            this.glossaryFolder = new CDP4Composition.FolderRowViewModel("Glossary", "Glossary", this.Session, this);
            this.ContainedRows.Add(this.glossaryFolder);
            this.referenceSourceFolder = new CDP4Composition.FolderRowViewModel("Reference Source", "Reference Source", this.Session, this);
            this.ContainedRows.Add(this.referenceSourceFolder);
            this.ruleFolder = new CDP4Composition.FolderRowViewModel("Rule", "Rule", this.Session, this);
            this.ContainedRows.Add(this.ruleFolder);
            this.constantFolder = new CDP4Composition.FolderRowViewModel("Constant", "Constant", this.Session, this);
            this.ContainedRows.Add(this.constantFolder);

            var rdlListener = this.messageBus.Listen<SessionEvent>()
                .Where(objectChange => objectChange.Status == SessionStatus.RdlOpened || objectChange.Status == SessionStatus.RdlClosed)
                .Subscribe(_ => this.UpdateProperties());
            this.Disposables.Add(rdlListener);

            this.UpdateProperties();
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
            this.ComputeRows(this.Thing.DefinedCategory, this.definedCategoryFolder, this.AddDefinedCategoryRowViewModel);
            this.ComputeRows(this.Thing.ParameterType, this.parameterTypeFolder, this.AddParameterTypeRowViewModel);
            this.ComputeRows(this.Thing.Scale, this.scaleFolder, this.AddScaleRowViewModel);
            this.ComputeRows(this.Thing.UnitPrefix, this.unitPrefixFolder, this.AddUnitPrefixRowViewModel);
            this.ComputeRows(this.Thing.Unit, this.unitFolder, this.AddUnitRowViewModel);
            this.ComputeRows(this.Thing.FileType, this.fileTypeFolder, this.AddFileTypeRowViewModel);
            this.ComputeRows(this.Thing.Glossary, this.glossaryFolder, this.AddGlossaryRowViewModel);
            this.ComputeRows(this.Thing.ReferenceSource, this.referenceSourceFolder, this.AddReferenceSourceRowViewModel);
            this.ComputeRows(this.Thing.Rule, this.ruleFolder, this.AddRuleRowViewModel);
            this.ComputeRows(this.Thing.Constant, this.constantFolder, this.AddConstantRowViewModel);
        }
        /// <summary>
        /// Add an Defined Category row view model to the list of <see cref="DefinedCategory"/>
        /// </summary>
        /// <param name="definedCategory">
        /// The <see cref="DefinedCategory"/> that is to be added
        /// </param>
        private CategoryRowViewModel AddDefinedCategoryRowViewModel(Category definedCategory)
        {
            return new CategoryRowViewModel(definedCategory, this.Session, this);
        }
        /// <summary>
        /// Add an Parameter Type row view model to the list of <see cref="ParameterType"/>
        /// </summary>
        /// <param name="parameterType">
        /// The <see cref="ParameterType"/> that is to be added
        /// </param>
        private IParameterTypeRowViewModel<ParameterType> AddParameterTypeRowViewModel(ParameterType parameterType)
        {
        var arrayParameterType = parameterType as ArrayParameterType;
        if (arrayParameterType != null)
        {
            return new ArrayParameterTypeRowViewModel(arrayParameterType, this.Session, this);
        }
        var enumerationParameterType = parameterType as EnumerationParameterType;
        if (enumerationParameterType != null)
        {
            return new EnumerationParameterTypeRowViewModel(enumerationParameterType, this.Session, this);
        }
        var booleanParameterType = parameterType as BooleanParameterType;
        if (booleanParameterType != null)
        {
            return new BooleanParameterTypeRowViewModel(booleanParameterType, this.Session, this);
        }
        var compoundParameterType = parameterType as CompoundParameterType;
        if (compoundParameterType != null)
        {
            return new CompoundParameterTypeRowViewModel(compoundParameterType, this.Session, this);
        }
        var dateParameterType = parameterType as DateParameterType;
        if (dateParameterType != null)
        {
            return new DateParameterTypeRowViewModel(dateParameterType, this.Session, this);
        }
        var textParameterType = parameterType as TextParameterType;
        if (textParameterType != null)
        {
            return new TextParameterTypeRowViewModel(textParameterType, this.Session, this);
        }
        var specializedQuantityKind = parameterType as SpecializedQuantityKind;
        if (specializedQuantityKind != null)
        {
            return new SpecializedQuantityKindRowViewModel(specializedQuantityKind, this.Session, this);
        }
        var simpleQuantityKind = parameterType as SimpleQuantityKind;
        if (simpleQuantityKind != null)
        {
            return new SimpleQuantityKindRowViewModel(simpleQuantityKind, this.Session, this);
        }
        var dateTimeParameterType = parameterType as DateTimeParameterType;
        if (dateTimeParameterType != null)
        {
            return new DateTimeParameterTypeRowViewModel(dateTimeParameterType, this.Session, this);
        }
        var timeOfDayParameterType = parameterType as TimeOfDayParameterType;
        if (timeOfDayParameterType != null)
        {
            return new TimeOfDayParameterTypeRowViewModel(timeOfDayParameterType, this.Session, this);
        }
        var derivedQuantityKind = parameterType as DerivedQuantityKind;
        if (derivedQuantityKind != null)
        {
            return new DerivedQuantityKindRowViewModel(derivedQuantityKind, this.Session, this);
        }
        throw new Exception("No ParameterType to return");
        }
        /// <summary>
        /// Add an Scale row view model to the list of <see cref="MeasurementScale"/>
        /// </summary>
        /// <param name="scale">
        /// The <see cref="Scale"/> that is to be added
        /// </param>
        private IMeasurementScaleRowViewModel<MeasurementScale> AddScaleRowViewModel(MeasurementScale scale)
        {
        var cyclicRatioScale = scale as CyclicRatioScale;
        if (cyclicRatioScale != null)
        {
            return new CyclicRatioScaleRowViewModel(cyclicRatioScale, this.Session, this);
        }
        var ordinalScale = scale as OrdinalScale;
        if (ordinalScale != null)
        {
            return new OrdinalScaleRowViewModel(ordinalScale, this.Session, this);
        }
        var ratioScale = scale as RatioScale;
        if (ratioScale != null)
        {
            return new RatioScaleRowViewModel(ratioScale, this.Session, this);
        }
        var intervalScale = scale as IntervalScale;
        if (intervalScale != null)
        {
            return new IntervalScaleRowViewModel(intervalScale, this.Session, this);
        }
        var logarithmicScale = scale as LogarithmicScale;
        if (logarithmicScale != null)
        {
            return new LogarithmicScaleRowViewModel(logarithmicScale, this.Session, this);
        }
        throw new Exception("No MeasurementScale to return");
        }
        /// <summary>
        /// Add an Unit Prefix row view model to the list of <see cref="UnitPrefix"/>
        /// </summary>
        /// <param name="unitPrefix">
        /// The <see cref="UnitPrefix"/> that is to be added
        /// </param>
        private UnitPrefixRowViewModel AddUnitPrefixRowViewModel(UnitPrefix unitPrefix)
        {
            return new UnitPrefixRowViewModel(unitPrefix, this.Session, this);
        }
        /// <summary>
        /// Add an Unit row view model to the list of <see cref="MeasurementUnit"/>
        /// </summary>
        /// <param name="unit">
        /// The <see cref="Unit"/> that is to be added
        /// </param>
        private IMeasurementUnitRowViewModel<MeasurementUnit> AddUnitRowViewModel(MeasurementUnit unit)
        {
        var linearConversionUnit = unit as LinearConversionUnit;
        if (linearConversionUnit != null)
        {
            return new LinearConversionUnitRowViewModel(linearConversionUnit, this.Session, this);
        }
        var derivedUnit = unit as DerivedUnit;
        if (derivedUnit != null)
        {
            return new DerivedUnitRowViewModel(derivedUnit, this.Session, this);
        }
        var simpleUnit = unit as SimpleUnit;
        if (simpleUnit != null)
        {
            return new SimpleUnitRowViewModel(simpleUnit, this.Session, this);
        }
        var prefixedUnit = unit as PrefixedUnit;
        if (prefixedUnit != null)
        {
            return new PrefixedUnitRowViewModel(prefixedUnit, this.Session, this);
        }
        throw new Exception("No MeasurementUnit to return");
        }
        /// <summary>
        /// Add an File Type row view model to the list of <see cref="FileType"/>
        /// </summary>
        /// <param name="fileType">
        /// The <see cref="FileType"/> that is to be added
        /// </param>
        private FileTypeRowViewModel AddFileTypeRowViewModel(FileType fileType)
        {
            return new FileTypeRowViewModel(fileType, this.Session, this);
        }
        /// <summary>
        /// Add an Glossary row view model to the list of <see cref="Glossary"/>
        /// </summary>
        /// <param name="glossary">
        /// The <see cref="Glossary"/> that is to be added
        /// </param>
        private GlossaryRowViewModel AddGlossaryRowViewModel(Glossary glossary)
        {
            return new GlossaryRowViewModel(glossary, this.Session, this);
        }
        /// <summary>
        /// Add an Reference Source row view model to the list of <see cref="ReferenceSource"/>
        /// </summary>
        /// <param name="referenceSource">
        /// The <see cref="ReferenceSource"/> that is to be added
        /// </param>
        private ReferenceSourceRowViewModel AddReferenceSourceRowViewModel(ReferenceSource referenceSource)
        {
            return new ReferenceSourceRowViewModel(referenceSource, this.Session, this);
        }
        /// <summary>
        /// Add an Rule row view model to the list of <see cref="Rule"/>
        /// </summary>
        /// <param name="rule">
        /// The <see cref="Rule"/> that is to be added
        /// </param>
        private IRuleRowViewModel<Rule> AddRuleRowViewModel(Rule rule)
        {
        var referencerRule = rule as ReferencerRule;
        if (referencerRule != null)
        {
            return new ReferencerRuleRowViewModel(referencerRule, this.Session, this);
        }
        var binaryRelationshipRule = rule as BinaryRelationshipRule;
        if (binaryRelationshipRule != null)
        {
            return new BinaryRelationshipRuleRowViewModel(binaryRelationshipRule, this.Session, this);
        }
        var multiRelationshipRule = rule as MultiRelationshipRule;
        if (multiRelationshipRule != null)
        {
            return new MultiRelationshipRuleRowViewModel(multiRelationshipRule, this.Session, this);
        }
        var decompositionRule = rule as DecompositionRule;
        if (decompositionRule != null)
        {
            return new DecompositionRuleRowViewModel(decompositionRule, this.Session, this);
        }
        var parameterizedCategoryRule = rule as ParameterizedCategoryRule;
        if (parameterizedCategoryRule != null)
        {
            return new ParameterizedCategoryRuleRowViewModel(parameterizedCategoryRule, this.Session, this);
        }
        throw new Exception("No Rule to return");
        }
        /// <summary>
        /// Add an Constant row view model to the list of <see cref="Constant"/>
        /// </summary>
        /// <param name="constant">
        /// The <see cref="Constant"/> that is to be added
        /// </param>
        private ConstantRowViewModel AddConstantRowViewModel(Constant constant)
        {
            return new ConstantRowViewModel(constant, this.Session, this);
        }
    }
}
