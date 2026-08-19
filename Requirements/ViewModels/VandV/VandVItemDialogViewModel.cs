// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVItemDialogViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2026 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Rowan de Voogt
//
//    This file is part of CDP4-COMET IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Text.RegularExpressions;

    using CDP4Requirements.Rdl;
    using CDP4Requirements.Services;
    using CDP4Requirements.ViewModels.Rows;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.Extensions;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;

    using CDP4Dal;

    using ReactiveUI;

    /// <summary>
    /// The dialog for creating or editing a V&amp;V item against a single <see cref="Requirement"/>. Every V&amp;V
    /// attribute is a first-class field, laid out in Basic / Planning / Execution tabs to mirror the stock Requirement
    /// dialog, with the enumeration pick-lists read from the model's RDL.
    /// </summary>
    /// <remarks>
    /// This is a plain <see cref="DialogViewModelBase"/> opened through <see cref="IDialogNavigationService"/>, it is
    /// deliberately not a second <c>[ThingDialogViewModelExport(ClassKind.Requirement)]</c>, which would collide with
    /// the stock Requirement dialog and stop the IME from starting.
    /// </remarks>
    public class VandVItemDialogViewModel : DialogViewModelBase, IDataErrorInfo
    {
        /// <summary>
        /// The zero-based index of the Coverage tab, so a caller can open the dialog straight on it.
        /// Tab order is Basic, Planning, Procedure, Execution, Compliance, Coverage.
        /// </summary>
        public const int CoverageTabIndex = 5;

        /// <summary>
        /// The zero-based index of the Procedure tab, so editing a step opens where the step lives.
        /// </summary>
        public const int ProcedureTabIndex = 2;

        /// <summary>
        /// The link type denoting verification against the requirement baseline.
        /// </summary>
        public const string VerifiesLink = VandVCategory.Verifies;

        /// <summary>
        /// The link type denoting validation against the mission / stakeholder need.
        /// </summary>
        public const string ValidatesLink = VandVCategory.Validates;

        /// <summary>
        /// Matches a valid short-name: letters, digits or underscores. Deliberately permits a leading digit, because
        /// the model itself accepts short-names such as <c>10R</c> and the dialog must not be stricter than the server.
        /// </summary>
        private static readonly Regex ShortNamePattern = new Regex(@"^[a-zA-Z0-9_]+$", RegexOptions.Compiled);

        /// <summary>
        /// The short-names already used in the iteration, so a duplicate can be rejected before the server does.
        /// </summary>
        private readonly HashSet<string> usedShortNames;

        /// <summary>
        /// How many V&amp;V items already cover the requirement, so a second item is numbered rather than repeating the
        /// first item's name.
        /// </summary>
        private readonly int coveringItemCount;

        /// <summary>
        /// The last name this dialog generated itself, so a link-type change only rewrites a name the user has not
        /// touched.
        /// </summary>
        private string lastGeneratedName;

        /// <summary>
        /// Backing field for <see cref="SelectedTabIndex"/>
        /// </summary>
        private int selectedTabIndex;

        /// <summary>
        /// Backing field for <see cref="SelectedParametricConstraint"/>
        /// </summary>
        private ConstraintChoiceRowViewModel selectedParametricConstraint;

        /// <summary>
        /// Backing field for <see cref="SelectedElementDefinition"/>
        /// </summary>
        private ElementDefinition selectedElementDefinition;

        /// <summary>
        /// Backing field for <see cref="SelectedParameter"/>
        /// </summary>
        private ParameterOrOverrideBase selectedParameter;

        /// <summary>
        /// Backing field for <see cref="ShortName"/>
        /// </summary>
        private string shortName;

        /// <summary>
        /// Backing field for <see cref="Name"/>
        /// </summary>
        private string name;

        /// <summary>
        /// Backing field for <see cref="LinkType"/>
        /// </summary>
        private string linkType;

        /// <summary>
        /// Backing field for <see cref="Method"/>
        /// </summary>
        private string method;

        /// <summary>
        /// Backing field for <see cref="Stage"/>
        /// </summary>
        private string stage;

        /// <summary>
        /// Backing field for <see cref="Level"/>
        /// </summary>
        private string level;

        /// <summary>
        /// Backing field for <see cref="Criticality"/>
        /// </summary>
        private string criticality;

        /// <summary>
        /// Backing field for <see cref="Status"/>
        /// </summary>
        private string status;

        /// <summary>
        /// Backing field for <see cref="Acceptance"/>
        /// </summary>
        private string acceptance;

        /// <summary>
        /// Backing field for <see cref="Description"/>
        /// </summary>
        private string description;

        /// <summary>
        /// Backing field for <see cref="Preconditions"/>
        /// </summary>
        private string preconditions;

        /// <summary>
        /// Backing field for <see cref="Conditions"/>
        /// </summary>
        private string conditions;

        /// <summary>
        /// Backing field for <see cref="Facility"/>
        /// </summary>
        private string facility;

        /// <summary>
        /// Backing field for <see cref="ActivityNumber"/>
        /// </summary>
        private string activityNumber;

        /// <summary>
        /// Backing field for <see cref="ResponsibleExternal"/>
        /// </summary>
        private string responsibleExternal;

        /// <summary>
        /// Backing field for <see cref="CoverageNote"/>
        /// </summary>
        private string coverageNote;

        /// <summary>
        /// Backing field for <see cref="Compliance"/>
        /// </summary>
        private string compliance;

        /// <summary>
        /// Backing field for <see cref="AnalysisCheck"/>
        /// </summary>
        private string analysisCheck;

        /// <summary>
        /// Backing field for <see cref="ProcedureReference"/>
        /// </summary>
        private string procedureReference;

        /// <summary>
        /// Backing field for <see cref="SelectedStep"/>
        /// </summary>
        private VandVProcedureStep selectedStep;

        /// <summary>
        /// Backing field for <see cref="IsClosed"/>
        /// </summary>
        private bool isClosed;

        /// <summary>
        /// Backing field for <see cref="CloseOutReason"/>
        /// </summary>
        private string closeOutReason;

        /// <summary>
        /// Backing field for <see cref="ClosedBy"/>
        /// </summary>
        private string closedBy;

        /// <summary>
        /// Backing field for <see cref="ClosedOn"/>
        /// </summary>
        private DateTime? closedOn;

        /// <summary>
        /// Backing field for <see cref="PlanReference"/>
        /// </summary>
        private string planReference;

        /// <summary>
        /// Backing field for <see cref="EvidenceReference"/>
        /// </summary>
        private string evidenceReference;

        /// <summary>
        /// Backing field for <see cref="Result"/>
        /// </summary>
        private string result;

        /// <summary>
        /// Backing field for <see cref="PlannedDate"/>
        /// </summary>
        private DateTime? plannedDate;

        /// <summary>
        /// Backing field for <see cref="ActualDate"/>
        /// </summary>
        private DateTime? actualDate;

        /// <summary>
        /// Backing field for <see cref="Owner"/>
        /// </summary>
        private DomainOfExpertise owner;

        /// <summary>
        /// Initializes a new instance of the <see cref="VandVItemDialogViewModel"/> class for creating a new V&amp;V item.
        /// </summary>
        /// <param name="requirement">The <see cref="Requirement"/> the V&amp;V item will cover.</param>
        /// <param name="session">The <see cref="ISession"/>.</param>
        public VandVItemDialogViewModel(Requirement requirement, ISession session)
            : this(requirement, session, null, VerifiesLink)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VandVItemDialogViewModel"/> class.
        /// </summary>
        /// <param name="requirement">The <see cref="Requirement"/> the V&amp;V item covers.</param>
        /// <param name="session">The <see cref="ISession"/>.</param>
        /// <param name="vandVItem">The V&amp;V item being edited, or null when creating a new one.</param>
        /// <param name="existingLinkType">The current link type when editing.</param>
        public VandVItemDialogViewModel(Requirement requirement, ISession session, Requirement vandVItem, string existingLinkType)
        {
            this.Requirement = requirement;
            this.VandVItem = vandVItem;

            var iteration = requirement.GetContainerOfType<Iteration>();
            var model = (EngineeringModel)iteration.Container;
            var mrdl = model.EngineeringModelSetup.RequiredRdl.FirstOrDefault();

            this.PossibleMethods = EnumerationValues(mrdl, VandVParameter.Method);
            this.PossibleStages = EnumerationValues(mrdl, VandVParameter.Stage);
            this.PossibleLevels = EnumerationValues(mrdl, VandVParameter.Level);
            this.PossibleCriticalities = EnumerationValues(mrdl, VandVParameter.Criticality);
            this.PossibleStatuses = EnumerationValues(mrdl, VandVParameter.Status);
            this.PossibleCompliances = EnumerationValues(mrdl, VandVCloseOut.ComplianceShortName);

            if (!this.PossibleCompliances.Any())
            {
                this.PossibleCompliances = VandVCloseOut.PossibleCompliances;
            }

            this.PossibleStepResults = EnumerationValues(mrdl, VandVParameter.StepResult);

            if (!this.PossibleStepResults.Any())
            {
                this.PossibleStepResults = VandVStepResult.All;
            }

            this.PossibleLinkTypes = new[] { VerifiesLink, ValidatesLink };
            this.PossibleOwners = model.EngineeringModelSetup.ActiveDomain.OrderBy(x => x.Name).ToList();
            this.PossibleParametricConstraints = BuildConstraintChoices(requirement);

            this.coveringItemCount = VandVItemCreator.CountCoveringItems(iteration, requirement);

            this.usedShortNames = new HashSet<string>(
                iteration.RequirementsSpecification
                    .SelectMany(specification => specification.Requirement)
                    .Where(x => vandVItem == null || x.Iid != vandVItem.Iid)
                    .Select(x => x.ShortName));

            this.PossibleElementDefinitions = iteration.Element.OrderBy(x => x.Name).ToList();

            this.Subscriptions.Add(
                this.WhenAnyValue(x => x.SelectedElementDefinition)
                    .Subscribe(_ => this.PopulateParameters()));

            this.Subscriptions.Add(
                this.WhenAnyValue(x => x.SelectedParameter)
                    .Subscribe(_ =>
                    {
                        this.PopulateOptionsAndStates(iteration);
                        this.RefreshAnalysisCheck(requirement);
                    }));

            if (vandVItem == null)
            {
                this.LoadDefaults(requirement, iteration, session);
            }
            else
            {
                this.LoadFrom(vandVItem, existingLinkType);
                this.LoadCoverage(iteration, vandVItem);

                this.ProcedureSteps.AddRange(
                    VandVProcedureWriter.QuerySteps(iteration, vandVItem).Select(step => new VandVProcedureStep(step)));
            }

            // merged rather than one WhenAnyValue: the close-out fields take the count past the overloads
            // ReactiveUI provides, and they must gate OK too, or a closed item can be saved with no reason
            var identificationChanged = this.WhenAnyValue(
                    x => x.ShortName,
                    x => x.Name,
                    x => x.Owner,
                    x => x.Method,
                    x => x.Stage,
                    x => x.Acceptance,
                    x => x.LinkType)
                .Select(_ => Unit.Default);

            var closeOutChanged = this.WhenAnyValue(x => x.IsClosed, x => x.CloseOutReason).Select(_ => Unit.Default);

            var canOk = identificationChanged
                .Merge(closeOutChanged)
                .Select(_ => !this.HasValidationErrors());

            var hasSelectedStep = this.WhenAnyValue(x => x.SelectedStep).Select(step => step != null);

            this.AddStepCommand = ReactiveCommandCreator.Create(this.ExecuteAddStep);
            this.RemoveStepCommand = ReactiveCommandCreator.Create(this.ExecuteRemoveStep, hasSelectedStep);
            this.MoveStepUpCommand = ReactiveCommandCreator.Create(() => this.MoveStep(-1), hasSelectedStep);
            this.MoveStepDownCommand = ReactiveCommandCreator.Create(() => this.MoveStep(1), hasSelectedStep);

            this.UseParametricConstraintCommand = ReactiveCommandCreator.Create(
                this.ExecuteUseParametricConstraint,
                this.WhenAnyValue(x => x.SelectedParametricConstraint).Select(x => x != null));

            this.Subscriptions.Add(
                this.WhenAnyValue(x => x.LinkType)
                    .Skip(1)
                    .Subscribe(_ => this.RegenerateNameIfUntouched()));

            this.OkCommand = ReactiveCommandCreator.Create(this.ExecuteOk, canOk);
            this.CancelCommand = ReactiveCommandCreator.Create(this.ExecuteCancel);
        }

        /// <summary>
        /// Gets the <see cref="Requirement"/> the V&amp;V item covers.
        /// </summary>
        public Requirement Requirement { get; }

        /// <summary>
        /// Gets the V&amp;V item being edited, or null when creating a new one.
        /// </summary>
        public Requirement VandVItem { get; }

        /// <summary>
        /// Gets a value indicating whether the dialog is editing an existing V&amp;V item.
        /// </summary>
        public bool IsEditMode => this.VandVItem != null;

        /// <summary>
        /// Gets the window title.
        /// </summary>
        public string Title => this.IsEditMode ? "Edit V&V Item" : "Create V&V Item";

        /// <summary>
        /// Gets the caption of the confirm button.
        /// </summary>
        public string OkButtonCaption => this.IsEditMode ? "OK" : "Create";

        /// <summary>
        /// Gets the caption describing which requirement is covered.
        /// </summary>
        public string RequirementCaption => $"{this.Requirement.ShortName}: {this.Requirement.Name}";

        /// <summary>Gets the possible link types (<c>verifies</c> / <c>validates</c>).</summary>
        public IReadOnlyList<string> PossibleLinkTypes { get; }

        /// <summary>Gets the possible <c>vnv_method</c> values.</summary>
        public IReadOnlyList<string> PossibleMethods { get; }

        /// <summary>Gets the possible <c>vnv_stage</c> values.</summary>
        public IReadOnlyList<string> PossibleStages { get; }

        /// <summary>Gets the possible <c>vnv_level</c> values.</summary>
        public IReadOnlyList<string> PossibleLevels { get; }

        /// <summary>Gets the possible <c>vnv_criticality</c> values.</summary>
        public IReadOnlyList<string> PossibleCriticalities { get; }

        /// <summary>Gets the possible <c>vnv_status</c> values.</summary>
        public IReadOnlyList<string> PossibleStatuses { get; }

        /// <summary>
        /// Gets the selectable compliance statuses, read from the RDL and falling back to the manifest.
        /// </summary>
        public IReadOnlyList<string> PossibleCompliances { get; }

        /// <summary>
        /// Gets or sets the compliance status: does the design meet the requirement.
        /// </summary>
        public string Compliance
        {
            get => this.compliance;
            set => this.RaiseAndSetIfChanged(ref this.compliance, value);
        }

        /// <summary>
        /// Gets the automatic verdict of the covered parameter against the requirement's parametric constraints. It is
        /// read-only on purpose: it informs the compliance judgement, it never makes it.
        /// </summary>
        public string AnalysisCheck
        {
            get => this.analysisCheck;
            private set => this.RaiseAndSetIfChanged(ref this.analysisCheck, value);
        }

        /// <summary>
        /// Gets the procedure steps, in order. The list is authoritative: what is here on OK is what is written.
        /// </summary>
        public ReactiveList<VandVProcedureStep> ProcedureSteps { get; } = new ReactiveList<VandVProcedureStep>();

        /// <summary>
        /// Gets the selectable step results, read from the RDL and falling back to the manifest.
        /// </summary>
        public IReadOnlyList<string> PossibleStepResults { get; private set; }

        /// <summary>
        /// Gets or sets the step selected in the procedure grid.
        /// </summary>
        public VandVProcedureStep SelectedStep
        {
            get => this.selectedStep;
            set => this.RaiseAndSetIfChanged(ref this.selectedStep, value);
        }

        /// <summary>
        /// Gets or sets the identifier of the procedure document this activity follows.
        /// </summary>
        public string ProcedureReference
        {
            get => this.procedureReference;
            set => this.RaiseAndSetIfChanged(ref this.procedureReference, value);
        }

        /// <summary>
        /// Gets the command that appends a step to the procedure.
        /// </summary>
        public ReactiveCommand<Unit, Unit> AddStepCommand { get; private set; }

        /// <summary>
        /// Gets the command that removes the selected step.
        /// </summary>
        public ReactiveCommand<Unit, Unit> RemoveStepCommand { get; private set; }

        /// <summary>
        /// Gets the command that moves the selected step one place earlier.
        /// </summary>
        public ReactiveCommand<Unit, Unit> MoveStepUpCommand { get; private set; }

        /// <summary>
        /// Gets the command that moves the selected step one place later.
        /// </summary>
        public ReactiveCommand<Unit, Unit> MoveStepDownCommand { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the item has been closed out.
        /// </summary>
        public bool IsClosed
        {
            get => this.isClosed;
            set => this.RaiseAndSetIfChanged(ref this.isClosed, value);
        }

        /// <summary>
        /// Gets or sets the reason the item was closed out, which ECSS requires alongside the close-out status.
        /// </summary>
        public string CloseOutReason
        {
            get => this.closeOutReason;
            set => this.RaiseAndSetIfChanged(ref this.closeOutReason, value);
        }

        /// <summary>
        /// Gets or sets who closed the item out.
        /// </summary>
        public string ClosedBy
        {
            get => this.closedBy;
            set => this.RaiseAndSetIfChanged(ref this.closedBy, value);
        }

        /// <summary>
        /// Gets or sets when the item was closed out.
        /// </summary>
        public DateTime? ClosedOn
        {
            get => this.closedOn;
            set => this.RaiseAndSetIfChanged(ref this.closedOn, value);
        }

        /// <summary>
        /// Gets or sets the section of the verification plan this activity is described in.
        /// </summary>
        public string PlanReference
        {
            get => this.planReference;
            set => this.RaiseAndSetIfChanged(ref this.planReference, value);
        }

        /// <summary>Gets the possible owning <see cref="DomainOfExpertise"/>s.</summary>
        public IReadOnlyList<DomainOfExpertise> PossibleOwners { get; }

        /// <summary>
        /// Gets the <see cref="ParametricConstraint"/>s already defined on the covered requirement. These are the
        /// machine-evaluable acceptance criteria; one can be copied into the human-readable acceptance text so the two
        /// say the same thing.
        /// </summary>
        public IReadOnlyList<ConstraintChoiceRowViewModel> PossibleParametricConstraints { get; }

        /// <summary>
        /// Gets a value indicating whether the covered requirement has any parametric constraint to offer.
        /// </summary>
        public bool HasParametricConstraints => this.PossibleParametricConstraints.Any();

        /// <summary>
        /// Gets or sets the index of the tab the dialog shows. See <see cref="CoverageTabIndex"/>.
        /// </summary>
        public int SelectedTabIndex
        {
            get => this.selectedTabIndex;
            set => this.RaiseAndSetIfChanged(ref this.selectedTabIndex, value);
        }

        /// <summary>
        /// Gets or sets the parametric constraint selected for insertion into the acceptance criteria.
        /// </summary>
        public ConstraintChoiceRowViewModel SelectedParametricConstraint
        {
            get => this.selectedParametricConstraint;
            set => this.RaiseAndSetIfChanged(ref this.selectedParametricConstraint, value);
        }

        /// <summary>
        /// Gets the command that copies the selected parametric constraint's expression into the acceptance criteria.
        /// </summary>
        public ReactiveCommand<Unit, Unit> UseParametricConstraintCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ElementDefinition"/>s the activity can be carried out on.
        /// </summary>
        public IReadOnlyList<ElementDefinition> PossibleElementDefinitions { get; private set; }

        /// <summary>
        /// Gets the parameters of the selected <see cref="ElementDefinition"/>.
        /// </summary>
        public ReactiveList<ParameterOrOverrideBase> PossibleParameters { get; } = new ReactiveList<ParameterOrOverrideBase>();

        /// <summary>
        /// Gets the options offered when the selected parameter is option-dependent.
        /// </summary>
        public ReactiveList<SelectableThingRowViewModel> PossibleOptions { get; } = new ReactiveList<SelectableThingRowViewModel>();

        /// <summary>
        /// Gets the actual finite states offered when the selected parameter is state-dependent.
        /// </summary>
        public ReactiveList<SelectableThingRowViewModel> PossibleStates { get; } = new ReactiveList<SelectableThingRowViewModel>();

        /// <summary>
        /// Gets the options this V&amp;V activity covers, the ticked rows of <see cref="PossibleOptions"/>.
        /// </summary>
        public IReadOnlyList<Option> SelectedOptions =>
            this.PossibleOptions.Where(x => x.IsSelected).Select(x => x.Thing).OfType<Option>().ToList();

        /// <summary>
        /// Gets the actual finite states this V&amp;V activity covers, the ticked rows of <see cref="PossibleStates"/>.
        /// </summary>
        public IReadOnlyList<ActualFiniteState> SelectedStates =>
            this.PossibleStates.Where(x => x.IsSelected).Select(x => x.Thing).OfType<ActualFiniteState>().ToList();

        /// <summary>
        /// Gets or sets the <see cref="ElementDefinition"/> the activity is carried out on.
        /// </summary>
        public ElementDefinition SelectedElementDefinition
        {
            get => this.selectedElementDefinition;
            set => this.RaiseAndSetIfChanged(ref this.selectedElementDefinition, value);
        }

        /// <summary>
        /// Gets or sets the parameter this activity measures.
        /// </summary>
        public ParameterOrOverrideBase SelectedParameter
        {
            get => this.selectedParameter;
            set => this.RaiseAndSetIfChanged(ref this.selectedParameter, value);
        }

        /// <summary>
        /// Gets a value indicating whether the selected parameter is option-dependent, so the option picker applies.
        /// </summary>
        public bool IsParameterOptionDependent => this.SelectedParameter?.IsOptionDependent == true;

        /// <summary>
        /// Gets a value indicating whether the selected parameter is state-dependent, so the state picker applies.
        /// </summary>
        public bool IsParameterStateDependent => this.SelectedParameter?.StateDependence != null;

        /// <summary>Gets or sets the short-name of the V&amp;V item. Required.</summary>
        public string ShortName
        {
            get => this.shortName;
            set => this.RaiseAndSetIfChanged(ref this.shortName, value);
        }

        /// <summary>Gets or sets the name of the V&amp;V item. Required.</summary>
        public string Name
        {
            get => this.name;
            set => this.RaiseAndSetIfChanged(ref this.name, value);
        }

        /// <summary>Gets or sets whether this item verifies or validates the requirement. Required.</summary>
        public string LinkType
        {
            get => this.linkType;
            set => this.RaiseAndSetIfChanged(ref this.linkType, value);
        }

        /// <summary>Gets or sets the verification method. Required.</summary>
        public string Method
        {
            get => this.method;
            set => this.RaiseAndSetIfChanged(ref this.method, value);
        }

        /// <summary>Gets or sets the stage gate. Required.</summary>
        public string Stage
        {
            get => this.stage;
            set => this.RaiseAndSetIfChanged(ref this.stage, value);
        }

        /// <summary>Gets or sets the integration level.</summary>
        public string Level
        {
            get => this.level;
            set => this.RaiseAndSetIfChanged(ref this.level, value);
        }

        /// <summary>Gets or sets the criticality.</summary>
        public string Criticality
        {
            get => this.criticality;
            set => this.RaiseAndSetIfChanged(ref this.criticality, value);
        }

        /// <summary>Gets or sets the status.</summary>
        public string Status
        {
            get => this.status;
            set => this.RaiseAndSetIfChanged(ref this.status, value);
        }

        /// <summary>Gets or sets the acceptance criteria. Required.</summary>
        public string Acceptance
        {
            get => this.acceptance;
            set => this.RaiseAndSetIfChanged(ref this.acceptance, value);
        }

        /// <summary>Gets or sets the activity description.</summary>
        public string Description
        {
            get => this.description;
            set => this.RaiseAndSetIfChanged(ref this.description, value);
        }

        /// <summary>Gets or sets the entry conditions.</summary>
        public string Preconditions
        {
            get => this.preconditions;
            set => this.RaiseAndSetIfChanged(ref this.preconditions, value);
        }

        /// <summary>Gets or sets the environmental and operational conditions.</summary>
        public string Conditions
        {
            get => this.conditions;
            set => this.RaiseAndSetIfChanged(ref this.conditions, value);
        }

        /// <summary>Gets or sets the facility.</summary>
        public string Facility
        {
            get => this.facility;
            set => this.RaiseAndSetIfChanged(ref this.facility, value);
        }

        /// <summary>Gets or sets the programme activity number.</summary>
        public string ActivityNumber
        {
            get => this.activityNumber;
            set => this.RaiseAndSetIfChanged(ref this.activityNumber, value);
        }

        /// <summary>Gets or sets the external responsible party.</summary>
        public string ResponsibleExternal
        {
            get => this.responsibleExternal;
            set => this.RaiseAndSetIfChanged(ref this.responsibleExternal, value);
        }

        /// <summary>Gets or sets the coverage note.</summary>
        public string CoverageNote
        {
            get => this.coverageNote;
            set => this.RaiseAndSetIfChanged(ref this.coverageNote, value);
        }

        /// <summary>Gets or sets the evidence reference.</summary>
        public string EvidenceReference
        {
            get => this.evidenceReference;
            set => this.RaiseAndSetIfChanged(ref this.evidenceReference, value);
        }

        /// <summary>Gets or sets the recorded result.</summary>
        public string Result
        {
            get => this.result;
            set => this.RaiseAndSetIfChanged(ref this.result, value);
        }

        /// <summary>Gets or sets the planned execution date.</summary>
        public DateTime? PlannedDate
        {
            get => this.plannedDate;
            set => this.RaiseAndSetIfChanged(ref this.plannedDate, value);
        }

        /// <summary>Gets or sets the actual execution date.</summary>
        public DateTime? ActualDate
        {
            get => this.actualDate;
            set => this.RaiseAndSetIfChanged(ref this.actualDate, value);
        }

        /// <summary>Gets or sets the owning <see cref="DomainOfExpertise"/>. Required.</summary>
        public DomainOfExpertise Owner
        {
            get => this.owner;
            set => this.RaiseAndSetIfChanged(ref this.owner, value);
        }

        /// <summary>
        /// Gets the command that accepts the dialog. Enabled once every required field is valid.
        /// </summary>
        public ReactiveCommand<Unit, Unit> OkCommand { get; }

        /// <summary>
        /// Gets the command that cancels the dialog.
        /// </summary>
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }

        /// <summary>
        /// Gets an error message for the object as a whole. Not used; per-property validation is used instead.
        /// </summary>
        public string Error => string.Empty;

        /// <summary>
        /// Validates a single property, so the editors show the standard error adornment.
        /// </summary>
        /// <param name="columnName">The property name.</param>
        /// <returns>The validation message, or an empty string when valid.</returns>
        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(this.CloseOutReason):
                        return this.IsClosed && string.IsNullOrWhiteSpace(this.CloseOutReason)
                            ? "A closed item must state why it was closed out."
                            : null;

                    case nameof(this.ShortName):
                        if (string.IsNullOrWhiteSpace(this.ShortName))
                        {
                            return "The short name is mandatory.";
                        }

                        if (!ShortNamePattern.IsMatch(this.ShortName))
                        {
                            return "The short name may contain only letters, digits and underscores (no spaces or punctuation).";
                        }

                        return this.usedShortNames.Contains(this.ShortName)
                            ? "Another requirement or V&V item already uses this short name."
                            : string.Empty;

                    case nameof(this.Name):
                        return string.IsNullOrWhiteSpace(this.Name) ? "The name is mandatory." : string.Empty;

                    case nameof(this.Owner):
                        return this.Owner == null ? "The owner is mandatory." : string.Empty;

                    case nameof(this.LinkType):
                        return string.IsNullOrWhiteSpace(this.LinkType) ? "Choose whether this item verifies or validates the requirement." : string.Empty;

                    case nameof(this.Method):
                        return string.IsNullOrWhiteSpace(this.Method) ? "The verification method is mandatory." : string.Empty;

                    case nameof(this.Stage):
                        return string.IsNullOrWhiteSpace(this.Stage) ? "The stage gate is mandatory." : string.Empty;

                    case nameof(this.Acceptance):
                        return string.IsNullOrWhiteSpace(this.Acceptance) ? "The acceptance criteria are mandatory." : string.Empty;

                    default:
                        return string.Empty;
                }
            }
        }

        /// <summary>
        /// Builds the V&amp;V attribute values keyed by parameter type short-name. Empty fields are omitted on create
        /// and returned as empty on edit, so clearing a field removes its value.
        /// </summary>
        /// <returns>The attribute values.</returns>
        public IReadOnlyDictionary<string, string> BuildAttributes()
        {
            var attributes = new Dictionary<string, string>
            {
                { VandVParameter.Method, this.Method },
                { VandVParameter.Stage, this.Stage },
                { VandVParameter.Level, this.Level },
                { VandVParameter.Criticality, this.Criticality },
                { VandVParameter.Status, this.Status },
                { VandVParameter.AcceptanceCriteria, this.Acceptance },
                { VandVParameter.Description, this.Description },
                { VandVParameter.Preconditions, this.Preconditions },
                { VandVParameter.Conditions, this.Conditions },
                { VandVParameter.Facility, this.Facility },
                { VandVParameter.ActivityNumber, this.ActivityNumber },
                { VandVParameter.ExternalResponsible, this.ResponsibleExternal },
                { VandVParameter.CoverageNote, this.CoverageNote },
                { VandVParameter.EvidenceReference, this.EvidenceReference },
                { VandVParameter.Result, this.Result },
                { VandVCloseOut.ComplianceShortName, this.Compliance },
                { VandVCloseOut.ClosedShortName, this.IsClosed ? "true" : string.Empty },
                { VandVCloseOut.CloseOutReasonShortName, this.CloseOutReason },
                { VandVCloseOut.ClosedByShortName, this.ClosedBy },
                { VandVCloseOut.ClosedOnShortName, this.ClosedOn?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) },
                { VandVCloseOut.PlanReferenceShortName, this.PlanReference },
                { VandVParameter.ProcedureReference, this.ProcedureReference },
                { VandVParameter.PlannedDate, this.PlannedDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) },
                { VandVParameter.ActualDate, this.ActualDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) }
            };

            return this.IsEditMode
                ? attributes
                : attributes.Where(x => !string.IsNullOrWhiteSpace(x.Value)).ToDictionary(x => x.Key, x => x.Value);
        }

        /// <summary>
        /// Asserts whether any required field currently fails validation.
        /// </summary>
        /// <returns>true when the dialog cannot be accepted.</returns>
        private bool HasValidationErrors()
        {
            var validated = new[]
            {
                nameof(this.ShortName), nameof(this.Name), nameof(this.Owner), nameof(this.LinkType),
                nameof(this.Method), nameof(this.Stage), nameof(this.Acceptance), nameof(this.CloseOutReason)
            };

            return validated.Any(property => !string.IsNullOrEmpty(this[property]));
        }

        /// <summary>
        /// Applies the defaults for a newly created V&amp;V item.
        /// </summary>
        /// <param name="requirement">The covered requirement.</param>
        /// <param name="iteration">The iteration.</param>
        /// <param name="session">The session.</param>
        private void LoadDefaults(Requirement requirement, Iteration iteration, ISession session)
        {
            this.ShortName = VandVItemCreator.SuggestShortName(requirement);
            this.LinkType = VerifiesLink;
            this.lastGeneratedName = this.BuildSuggestedName();
            this.Name = this.lastGeneratedName;
            this.Status = this.PossibleStatuses.FirstOrDefault();
            // explicitly, not simply the first RDL value: an item nobody has judged is Not Assessed
            this.Compliance = this.PossibleCompliances.FirstOrDefault(x => VandVCoverageQuery.AreSameEnumValue(x, VandVCloseOut.NotAssessed))
                              ?? this.PossibleCompliances.FirstOrDefault();
            this.Owner = session.OpenIterations.TryGetValue(iteration, out var tuple) ? tuple?.Item1 : null;
        }

        /// <summary>
        /// Loads the dialog from an existing V&amp;V item.
        /// </summary>
        /// <param name="vandVItem">The V&amp;V item being edited.</param>
        /// <param name="existingLinkType">The current link type.</param>
        private void LoadFrom(Requirement vandVItem, string existingLinkType)
        {
            this.ShortName = vandVItem.ShortName;
            this.Name = vandVItem.Name;
            this.Owner = vandVItem.Owner;
            this.LinkType = string.IsNullOrWhiteSpace(existingLinkType) ? VerifiesLink : existingLinkType;

            this.Method = Attribute(vandVItem, VandVParameter.Method);
            this.Stage = Attribute(vandVItem, VandVParameter.Stage);
            this.Level = Attribute(vandVItem, VandVParameter.Level);
            this.Criticality = Attribute(vandVItem, VandVParameter.Criticality);
            this.Status = Attribute(vandVItem, VandVParameter.Status);
            this.Acceptance = Attribute(vandVItem, VandVParameter.AcceptanceCriteria);
            this.Description = Attribute(vandVItem, VandVParameter.Description);
            this.Preconditions = Attribute(vandVItem, VandVParameter.Preconditions);
            this.Conditions = Attribute(vandVItem, VandVParameter.Conditions);
            this.Facility = Attribute(vandVItem, VandVParameter.Facility);
            this.ActivityNumber = Attribute(vandVItem, VandVParameter.ActivityNumber);
            this.ResponsibleExternal = Attribute(vandVItem, VandVParameter.ExternalResponsible);
            this.CoverageNote = Attribute(vandVItem, VandVParameter.CoverageNote);
            this.EvidenceReference = Attribute(vandVItem, VandVParameter.EvidenceReference);
            this.Result = Attribute(vandVItem, VandVParameter.Result);
            this.Compliance = VandVCloseOut.QueryCompliance(vandVItem);
            this.IsClosed = VandVCloseOut.IsClosed(vandVItem);
            this.CloseOutReason = Attribute(vandVItem, VandVCloseOut.CloseOutReasonShortName);
            this.ClosedBy = Attribute(vandVItem, VandVCloseOut.ClosedByShortName);
            this.ClosedOn = ParseDate(Attribute(vandVItem, VandVCloseOut.ClosedOnShortName));
            this.PlanReference = Attribute(vandVItem, VandVCloseOut.PlanReferenceShortName);
            this.ProcedureReference = Attribute(vandVItem, VandVParameter.ProcedureReference);
            this.PlannedDate = ParseDate(Attribute(vandVItem, VandVParameter.PlannedDate));
            this.ActualDate = ParseDate(Attribute(vandVItem, VandVParameter.ActualDate));
        }

        /// <summary>
        /// Reads a V&amp;V attribute off an item.
        /// </summary>
        /// <param name="item">The V&amp;V item.</param>
        /// <param name="shortName">The parameter type short-name.</param>
        /// <returns>The value, or null.</returns>
        private static string Attribute(Requirement item, string shortName)
        {
            return item.ParameterValue
                .FirstOrDefault(x => x.ParameterType != null && x.ParameterType.ShortName == shortName)?
                .Value.FirstOrDefault();
        }

        /// <summary>
        /// Parses a stored date value.
        /// </summary>
        /// <param name="value">The stored value.</param>
        /// <returns>The date, or null when absent or unparseable.</returns>
        private static DateTime? ParseDate(string value)
        {
            return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed) ? parsed : (DateTime?)null;
        }

        /// <summary>
        /// Returns the values offered for a V&amp;V enumeration attribute, read from the
        /// <see cref="EnumerationParameterType"/> in the model's RDL chain so a project can add its own stage gates,
        /// methods or levels as reference data without a plugin rebuild. Falls back to the
        /// <see cref="VandVRdlManifest"/> defaults when the RDL has no such parameter type yet.
        /// </summary>
        /// <param name="mrdl">The model's <see cref="ReferenceDataLibrary"/>, or null.</param>
        /// <param name="parameterTypeShortName">The parameter type short-name.</param>
        /// <returns>The values to offer in the pick-list.</returns>
        private static IReadOnlyList<string> EnumerationValues(ReferenceDataLibrary mrdl, string parameterTypeShortName)
        {
            var fromRdl = mrdl?
                .QueryParameterTypesFromChainOfRdls()
                .OfType<EnumerationParameterType>()
                .FirstOrDefault(x => x.ShortName == parameterTypeShortName)?
                .ValueDefinition
                .Select(x => x.Name)
                .ToList();

            if (fromRdl != null && fromRdl.Any())
            {
                return fromRdl;
            }

            return VandVRdlManifest.ParameterTypes
                .FirstOrDefault(x => x.ShortName == parameterTypeShortName)?
                .EnumerationValues ?? new List<string>();
        }

        /// <summary>
        /// Pre-selects the coverage of an existing V&amp;V item so the Coverage tab opens on what is already linked.
        /// </summary>
        /// <param name="iteration">The iteration.</param>
        /// <param name="vandVItem">The V&amp;V item being edited.</param>
        private void LoadCoverage(Iteration iteration, Requirement vandVItem)
        {
            var parameter = VandVCoverageWriter.QueryCoveredThings<ParameterOrOverrideBase>(iteration, vandVItem, VandVCoverageWriter.CoversParameter).FirstOrDefault();
            var element = VandVCoverageWriter.QueryCoveredThings<ElementDefinition>(iteration, vandVItem, VandVCoverageWriter.VerifiedOn).FirstOrDefault();

            // the parameter is assigned first on purpose: setting the element definition runs PopulateParameters
            // synchronously, and that is where a stored ParameterOverride gets added to the list so the combo can show
            // it. Assigning the parameter afterwards left it selected but absent from the list, i.e. blank on screen
            this.SelectedParameter = parameter;
            this.SelectedElementDefinition = element ?? QueryOwningElement(parameter);

            var coveredOptions = VandVCoverageWriter.QueryCoveredThings<Option>(iteration, vandVItem, VandVCoverageWriter.CoversOption).Select(x => x.Iid).ToList();
            var coveredStates = VandVCoverageWriter.QueryCoveredThings<ActualFiniteState>(iteration, vandVItem, VandVCoverageWriter.CoversState).Select(x => x.Iid).ToList();

            foreach (var row in this.PossibleOptions.Where(x => coveredOptions.Contains(x.Thing.Iid)))
            {
                row.IsSelected = true;
            }

            foreach (var row in this.PossibleStates.Where(x => coveredStates.Contains(x.Thing.Iid)))
            {
                row.IsSelected = true;
            }
        }

        /// <summary>
        /// Pre-selects a parameter dropped onto the V&amp;V item, together with its owning element definition.
        /// </summary>
        /// <param name="parameter">The dropped parameter.</param>
        public void PreselectParameter(ParameterOrOverrideBase parameter)
        {
            // assigned in this order for the reason given in LoadCoverage
            this.SelectedParameter = parameter;
            this.SelectedElementDefinition = QueryOwningElement(parameter);
        }

        /// <summary>
        /// Resolves the element a parameter belongs to. A <see cref="ParameterOverride"/> lives in an
        /// <see cref="ElementUsage"/>, so its element is reached through the usage.
        /// </summary>
        /// <param name="parameter">The parameter, or null.</param>
        /// <returns>The owning <see cref="ElementDefinition"/>, or null.</returns>
        private static ElementDefinition QueryOwningElement(ParameterOrOverrideBase parameter)
        {
            return parameter?.Container as ElementDefinition
                   ?? (parameter?.Container as ElementUsage)?.ElementDefinition;
        }

        /// <summary>
        /// Appends a step to the procedure and selects it, so the user can start typing straight away.
        /// </summary>
        private void ExecuteAddStep()
        {
            var step = new VandVProcedureStep { Number = this.ProcedureSteps.Count + 1 };

            this.ProcedureSteps.Add(step);
            this.SelectedStep = step;
            this.RenumberSteps();
        }

        /// <summary>
        /// Removes the selected step. The stored step is only deleted from the model when the dialog is accepted.
        /// </summary>
        private void ExecuteRemoveStep()
        {
            var step = this.SelectedStep;

            if (step == null)
            {
                return;
            }

            this.ProcedureSteps.Remove(step);
            this.SelectedStep = null;
            this.RenumberSteps();
        }

        /// <summary>
        /// Moves the selected step by the supplied offset, keeping the selection on it.
        /// </summary>
        /// <param name="offset">Minus one to move earlier, plus one to move later.</param>
        private void MoveStep(int offset)
        {
            var step = this.SelectedStep;

            if (step == null)
            {
                return;
            }

            var index = this.ProcedureSteps.IndexOf(step);
            var target = index + offset;

            if (index < 0 || target < 0 || target >= this.ProcedureSteps.Count)
            {
                return;
            }

            this.ProcedureSteps.Move(index, target);
            this.SelectedStep = step;
            this.RenumberSteps();
        }

        /// <summary>
        /// Renumbers the steps from their position, so the numbers always read one upwards.
        /// </summary>
        private void RenumberSteps()
        {
            for (var index = 0; index < this.ProcedureSteps.Count; index++)
            {
                this.ProcedureSteps[index].Number = index + 1;
            }
        }

        /// <summary>
        /// Re-evaluates the covered parameter against the requirement's parametric constraints, so the Compliance tab
        /// shows the verdict as soon as a parameter is picked, before the item is even saved.
        /// </summary>
        /// <param name="requirement">The requirement being verified.</param>
        private void RefreshAnalysisCheck(Requirement requirement)
        {
            var result = VandVAnalysisChecker.Evaluate(
                requirement,
                this.SelectedParameter,
                this.SelectedOptions.Select(x => x.Iid).ToList(),
                this.SelectedStates.Select(x => x.Iid).ToList());

            this.AnalysisCheck = result.Display;
        }

        /// <summary>
        /// Refreshes the parameter list when the element definition changes.
        /// </summary>
        private void PopulateParameters()
        {
            var current = this.SelectedParameter;

            this.PossibleParameters.Clear();

            if (this.SelectedElementDefinition != null)
            {
                this.PossibleParameters.AddRange(this.SelectedElementDefinition.Parameter.OrderBy(x => x.ParameterType?.Name));
            }

            // a stored ParameterOverride is kept visible (its usage points at the selected element), but a
            // parameter belonging to a different element is dropped: keeping it let a mismatched element and
            // parameter pairing be saved as coverage
            if (current != null && (this.SelectedElementDefinition == null || QueryOwningElement(current) == this.SelectedElementDefinition))
            {
                if (!this.PossibleParameters.Contains(current))
                {
                    this.PossibleParameters.Add(current);
                }

                this.SelectedParameter = current;
            }
            else if (!this.PossibleParameters.Contains(this.SelectedParameter))
            {
                this.SelectedParameter = null;
            }
        }

        /// <summary>
        /// Refreshes the option and state pickers from the selected parameter's dependencies, dropping any selection
        /// that no longer applies.
        /// </summary>
        /// <param name="iteration">The iteration.</param>
        private void PopulateOptionsAndStates(Iteration iteration)
        {
            var tickedOptions = this.PossibleOptions.Where(x => x.IsSelected).Select(x => x.Thing.Iid).ToList();
            var tickedStates = this.PossibleStates.Where(x => x.IsSelected).Select(x => x.Thing.Iid).ToList();

            this.PossibleOptions.Clear();
            this.PossibleStates.Clear();

            if (this.SelectedParameter?.IsOptionDependent == true)
            {
                foreach (Option option in iteration.Option)
                {
                    this.PossibleOptions.Add(new SelectableThingRowViewModel(option, option.Name, tickedOptions.Contains(option.Iid)));
                }
            }

            if (this.SelectedParameter?.StateDependence != null)
            {
                foreach (ActualFiniteState state in this.SelectedParameter.StateDependence.ActualState)
                {
                    this.PossibleStates.Add(new SelectableThingRowViewModel(state, state.ShortName, tickedStates.Contains(state.Iid)));
                }
            }

            this.RaisePropertyChanged(nameof(this.IsParameterOptionDependent));
            this.RaisePropertyChanged(nameof(this.IsParameterStateDependent));
        }

        /// <summary>
        /// Builds one picker entry for each parametric constraint of the requirement, plus one for every relational
        /// expression inside it, so a multi-expression constraint can be split across several V&amp;V items.
        /// </summary>
        /// <param name="requirement">The covered requirement.</param>
        /// <returns>The choices.</returns>
        private static IReadOnlyList<ConstraintChoiceRowViewModel> BuildConstraintChoices(Requirement requirement)
        {
            var choices = new List<ConstraintChoiceRowViewModel>();

            foreach (ParametricConstraint constraint in requirement.ParametricConstraint)
            {
                var expressions = constraint.Expression.OfType<RelationalExpression>().ToList();

                choices.Add(new ConstraintChoiceRowViewModel(constraint, null));

                if (expressions.Count > 1)
                {
                    choices.AddRange(expressions.Select(expression => new ConstraintChoiceRowViewModel(constraint, expression)));
                }
            }

            return choices;
        }

        /// <summary>
        /// Builds the suggested name: the action implied by the link type, the requirement, and an ordinal when the
        /// requirement is already covered so two items never share a name.
        /// </summary>
        /// <returns>The suggested name.</returns>
        private string BuildSuggestedName()
        {
            var verb = this.LinkType == ValidatesLink ? "Validate" : "Verify";
            var ordinal = this.coveringItemCount + 1;

            return ordinal > 1
                ? $"{verb} {this.Requirement.ShortName} ({ordinal})"
                : $"{verb} {this.Requirement.ShortName}";
        }

        /// <summary>
        /// Rewrites the name when the link type changes, but only while the name is still the one this dialog
        /// generated, a name the user typed is never overwritten.
        /// </summary>
        private void RegenerateNameIfUntouched()
        {
            if (this.IsEditMode || this.Name != this.lastGeneratedName)
            {
                return;
            }

            this.lastGeneratedName = this.BuildSuggestedName();
            this.Name = this.lastGeneratedName;
        }

        /// <summary>
        /// Copies the selected parametric constraint's expression into the acceptance criteria, appending it when text
        /// is already present so nothing the user wrote is lost.
        /// </summary>
        private void ExecuteUseParametricConstraint()
        {
            var choice = this.SelectedParametricConstraint;

            if (choice == null)
            {
                return;
            }

            var expression = choice.ExpressionText;

            if (!string.IsNullOrWhiteSpace(expression))
            {
                this.Acceptance = string.IsNullOrWhiteSpace(this.Acceptance)
                    ? expression
                    : $"{this.Acceptance}{Environment.NewLine}{expression}";
            }

            // a constraint bound to a parameter tells us exactly what this activity measures, so set the coverage too
            var parameter = choice.LinkedParameter;

            if (parameter != null)
            {
                this.PreselectParameter(parameter);
            }
        }

        /// <summary>
        /// Accepts the dialog.
        /// </summary>
        private void ExecuteOk()
        {
            this.DialogResult = new BaseDialogResult(true);
        }

        /// <summary>
        /// Cancels the dialog.
        /// </summary>
        private void ExecuteCancel()
        {
            this.DialogResult = new BaseDialogResult(false);
        }
    }
}
