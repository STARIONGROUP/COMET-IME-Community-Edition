// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVActivityDialogViewModel.cs" company="Starion Group S.A.">
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
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;

    using CDP4Dal;

    using ReactiveUI;

    /// <summary>
    /// The dialog for creating or editing a shared V&amp;V activity: one task (produce the mass budget, run the
    /// power-speed curve test) that performs the verification of many V&amp;V items. Its description, planning,
    /// procedure and execution record are written here once, and every item performed by it inherits whatever it does
    /// not override. The activity number is the activity's short-name, so it is unique by construction instead of
    /// being retyped per item.
    /// </summary>
    public class VandVActivityDialogViewModel : DialogViewModelBase, IDataErrorInfo
    {
        /// <summary>
        /// The zero-based index of the Procedure tab, so selecting a step in the Activities panel opens where the
        /// steps are edited. Tab order is Basic, Procedure, Execution, V&amp;V Items.
        /// </summary>
        public const int ProcedureTabIndex = 1;

        /// <summary>
        /// Matches a valid short-name: letters, digits or underscores, a leading digit allowed, exactly as the V&amp;V
        /// item dialog accepts.
        /// </summary>
        private static readonly Regex ShortNamePattern = new Regex(@"^[a-zA-Z0-9_]+$", RegexOptions.Compiled);

        /// <summary>
        /// The short-names already used in the iteration, so a duplicate activity number is rejected before the server
        /// does.
        /// </summary>
        private readonly HashSet<string> usedShortNames;

        /// <summary>
        /// Backing field for <see cref="ShortName"/>
        /// </summary>
        private string shortName;

        /// <summary>
        /// Backing field for <see cref="Name"/>
        /// </summary>
        private string name;

        /// <summary>
        /// Backing field for <see cref="Owner"/>
        /// </summary>
        private DomainOfExpertise owner;

        /// <summary>
        /// Backing field for <see cref="Report"/>
        /// </summary>
        private RequirementsSpecification report;

        /// <summary>
        /// Backing field for <see cref="SelectedTabIndex"/>
        /// </summary>
        private int selectedTabIndex;

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
        /// Backing field for <see cref="Description"/>
        /// </summary>
        private string description;

        /// <summary>
        /// Backing field for <see cref="Facility"/>
        /// </summary>
        private string facility;

        /// <summary>
        /// Backing field for <see cref="ResponsibleExternal"/>
        /// </summary>
        private string responsibleExternal;

        /// <summary>
        /// Backing field for <see cref="PlannedDate"/>
        /// </summary>
        private DateTime? plannedDate;

        /// <summary>
        /// Backing field for <see cref="PlanReference"/>
        /// </summary>
        private string planReference;

        /// <summary>
        /// Backing field for <see cref="ProcedureReference"/>
        /// </summary>
        private string procedureReference;

        /// <summary>
        /// Backing field for <see cref="Preconditions"/>
        /// </summary>
        private string preconditions;

        /// <summary>
        /// Backing field for <see cref="Conditions"/>
        /// </summary>
        private string conditions;

        /// <summary>
        /// Backing field for <see cref="SelectedStep"/>
        /// </summary>
        private VandVProcedureStep selectedStep;

        /// <summary>
        /// Backing field for <see cref="Status"/>
        /// </summary>
        private string status;

        /// <summary>
        /// Backing field for <see cref="ActualDate"/>
        /// </summary>
        private DateTime? actualDate;

        /// <summary>
        /// Backing field for <see cref="Result"/>
        /// </summary>
        private string result;

        /// <summary>
        /// Backing field for <see cref="EvidenceReference"/>
        /// </summary>
        private string evidenceReference;

        /// <summary>
        /// Initializes a new instance of the <see cref="VandVActivityDialogViewModel"/> class.
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/> the activity lives in.</param>
        /// <param name="session">The <see cref="ISession"/>.</param>
        /// <param name="activity">The activity being edited, or null when creating a new one.</param>
        /// <param name="report">The report to preselect when creating, typically the one the user right-clicked.</param>
        public VandVActivityDialogViewModel(Iteration iteration, ISession session, Requirement activity = null, RequirementsSpecification report = null)
        {
            this.Activity = activity;

            var model = (EngineeringModel)iteration.Container;
            var mrdl = model.EngineeringModelSetup.RequiredRdl.FirstOrDefault();

            this.PossibleMethods = VandVItemDialogViewModel.EnumerationValues(mrdl, VandVParameter.Method);
            this.PossibleStages = VandVItemDialogViewModel.EnumerationValues(mrdl, VandVParameter.Stage);
            this.PossibleLevels = VandVItemDialogViewModel.EnumerationValues(mrdl, VandVParameter.Level);
            this.PossibleStatuses = VandVItemDialogViewModel.EnumerationValues(mrdl, VandVParameter.Status);

            this.PossibleStepResults = VandVItemDialogViewModel.EnumerationValues(mrdl, VandVParameter.StepResult);

            if (!this.PossibleStepResults.Any())
            {
                this.PossibleStepResults = VandVStepResult.All;
            }

            this.PossibleOwners = model.EngineeringModelSetup.ActiveDomain.OrderBy(x => x.Name).ToList();
            this.PossibleReports = VandVActivityQuery.QueryReports(iteration);

            this.usedShortNames = new HashSet<string>(
                iteration.RequirementsSpecification
                    .SelectMany(specification => specification.Requirement)
                    .Where(x => activity == null || x.Iid != activity.Iid)
                    .Select(x => x.ShortName));

            if (activity == null)
            {
                this.ShortName = SuggestShortName(this.usedShortNames);
                this.Status = this.PossibleStatuses.FirstOrDefault();
                this.Owner = session.OpenIterations.TryGetValue(iteration, out var tuple) ? tuple?.Item1 : null;
                this.Report = report;
                this.LinkedItems = new List<VandVActivityItemRowViewModel>();
            }
            else
            {
                this.LoadFrom(iteration, activity);

                var coveredByItem = VandVItemCreator.QueryCoveringMap(iteration);

                this.LinkedItems = VandVActivityQuery.QueryPerformedItems(iteration, activity)
                    .Select(item => new VandVActivityItemRowViewModel(
                        item,
                        coveredByItem.TryGetValue(item.Iid, out var covered) ? covered : null,
                        activity))
                    .ToList();
            }

            var canOk = this.WhenAnyValue(
                    x => x.ShortName,
                    x => x.Name,
                    x => x.Owner,
                    x => x.Method,
                    x => x.Stage)
                .Select(_ => !this.HasValidationErrors());

            var hasSelectedStep = this.WhenAnyValue(x => x.SelectedStep).Select(step => step != null);

            this.AddStepCommand = ReactiveCommandCreator.Create(this.ExecuteAddStep);
            this.RemoveStepCommand = ReactiveCommandCreator.Create(this.ExecuteRemoveStep, hasSelectedStep);
            this.MoveStepUpCommand = ReactiveCommandCreator.Create(() => this.MoveStep(-1), hasSelectedStep);
            this.MoveStepDownCommand = ReactiveCommandCreator.Create(() => this.MoveStep(1), hasSelectedStep);

            this.OkCommand = ReactiveCommandCreator.Create(() => { this.DialogResult = new BaseDialogResult(true); }, canOk);
            this.CancelCommand = ReactiveCommandCreator.Create(() => { this.DialogResult = new BaseDialogResult(false); });
        }

        /// <summary>
        /// Gets the activity being edited, or null when creating a new one.
        /// </summary>
        public Requirement Activity { get; }

        /// <summary>
        /// Gets a value indicating whether the dialog is editing an existing activity.
        /// </summary>
        public bool IsEditMode => this.Activity != null;

        /// <summary>
        /// Gets or sets the index of the tab the dialog shows. See <see cref="ProcedureTabIndex"/>.
        /// </summary>
        public int SelectedTabIndex
        {
            get => this.selectedTabIndex;
            set => this.RaiseAndSetIfChanged(ref this.selectedTabIndex, value);
        }

        /// <summary>
        /// Gets the window title.
        /// </summary>
        public string Title => this.IsEditMode ? "Edit V&V Activity" : "Create V&V Activity";

        /// <summary>
        /// Gets the caption of the confirm button.
        /// </summary>
        public string OkButtonCaption => this.IsEditMode ? "OK" : "Create";

        /// <summary>Gets the possible <c>vnv_method</c> values.</summary>
        public IReadOnlyList<string> PossibleMethods { get; }

        /// <summary>Gets the possible <c>vnv_stage</c> values.</summary>
        public IReadOnlyList<string> PossibleStages { get; }

        /// <summary>Gets the possible <c>vnv_level</c> values.</summary>
        public IReadOnlyList<string> PossibleLevels { get; }

        /// <summary>Gets the possible <c>vnv_status</c> values.</summary>
        public IReadOnlyList<string> PossibleStatuses { get; }

        /// <summary>Gets the possible owning <see cref="DomainOfExpertise"/>s.</summary>
        public IReadOnlyList<DomainOfExpertise> PossibleOwners { get; }

        /// <summary>
        /// Gets the reports (deliverables) defined in the model. Only existing reports are offered: a report is a
        /// deliverable somebody decides to produce, created deliberately with <b>Create Report</b>, never conjured
        /// into existence by a typo in this dialog.
        /// </summary>
        public IReadOnlyList<RequirementsSpecification> PossibleReports { get; }

        /// <summary>
        /// Gets the report references as plain text, for the evidence picker. That combo edits the free-text
        /// <c>vnv_evidence_ref</c>, and binding the report objects to it wrote the type name into the model when one
        /// was picked.
        /// </summary>
        public IReadOnlyList<string> PossibleReportReferences => this.PossibleReports.Select(x => x.ShortName).ToList();

        /// <summary>
        /// Gets a value indicating whether the model has no report yet, so the dialog can point the user at Create
        /// Report instead of showing an inexplicably empty picker.
        /// </summary>
        public bool HasNoReports => !this.PossibleReports.Any();

        /// <summary>
        /// Gets the V&amp;V items this activity performs, with the requirement each one verifies. Read-only: items are
        /// linked from the item dialog's Performed By picker, or in bulk from the Activities browser.
        /// </summary>
        public IReadOnlyList<VandVActivityItemRowViewModel> LinkedItems { get; }

        /// <summary>
        /// Gets the caption of the linked-items tab, naming how many items this activity performs.
        /// </summary>
        public string LinkedItemsCaption => this.LinkedItems.Any()
            ? $"This activity performs {this.LinkedItems.Count} V&V item(s). Verifying that each requirement is met stays per item; running this activity is what moves them all."
            : "No V&V item is performed by this activity yet. Link items from the item dialog's 'Performed By' picker, or in bulk from the Activities browser: right-click the activity and pick 'Create V&V Items for this Activity'.";

        /// <summary>
        /// Gets the selectable step results, read from the RDL and falling back to the manifest.
        /// </summary>
        public IReadOnlyList<string> PossibleStepResults { get; private set; }

        /// <summary>
        /// Gets the procedure steps, in order. The list is authoritative: what is here on OK is what is written.
        /// </summary>
        public ReactiveList<VandVProcedureStep> ProcedureSteps { get; } = new ReactiveList<VandVProcedureStep>();

        /// <summary>Gets or sets the activity number, the activity's short-name. Required.</summary>
        public string ShortName
        {
            get => this.shortName;
            set => this.RaiseAndSetIfChanged(ref this.shortName, value);
        }

        /// <summary>Gets or sets the name of the activity, e.g. "Produce mass budget". Required.</summary>
        public string Name
        {
            get => this.name;
            set => this.RaiseAndSetIfChanged(ref this.name, value);
        }

        /// <summary>Gets or sets the owning <see cref="DomainOfExpertise"/>. Required.</summary>
        public DomainOfExpertise Owner
        {
            get => this.owner;
            set => this.RaiseAndSetIfChanged(ref this.owner, value);
        }

        /// <summary>
        /// Gets or sets the report (deliverable) this activity is recorded in. The report is the activity's
        /// container: picking another one moves the activity, clearing it moves the activity back to the V&amp;V
        /// specification.
        /// </summary>
        public RequirementsSpecification Report
        {
            get => this.report;
            set => this.RaiseAndSetIfChanged(ref this.report, value);
        }

        /// <summary>Gets or sets the verification method.</summary>
        public string Method
        {
            get => this.method;
            set => this.RaiseAndSetIfChanged(ref this.method, value);
        }

        /// <summary>Gets or sets the stage gate.</summary>
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

        /// <summary>Gets or sets the activity description, written once here instead of per item.</summary>
        public string Description
        {
            get => this.description;
            set => this.RaiseAndSetIfChanged(ref this.description, value);
        }

        /// <summary>Gets or sets the facility.</summary>
        public string Facility
        {
            get => this.facility;
            set => this.RaiseAndSetIfChanged(ref this.facility, value);
        }

        /// <summary>Gets or sets the external responsible party.</summary>
        public string ResponsibleExternal
        {
            get => this.responsibleExternal;
            set => this.RaiseAndSetIfChanged(ref this.responsibleExternal, value);
        }

        /// <summary>Gets or sets the planned execution date.</summary>
        public DateTime? PlannedDate
        {
            get => this.plannedDate;
            set => this.RaiseAndSetIfChanged(ref this.plannedDate, value);
        }

        /// <summary>Gets or sets the section of the verification plan this activity is described in.</summary>
        public string PlanReference
        {
            get => this.planReference;
            set => this.RaiseAndSetIfChanged(ref this.planReference, value);
        }

        /// <summary>Gets or sets the identifier of the procedure document this activity follows.</summary>
        public string ProcedureReference
        {
            get => this.procedureReference;
            set => this.RaiseAndSetIfChanged(ref this.procedureReference, value);
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

        /// <summary>
        /// Gets or sets the step selected in the procedure grid.
        /// </summary>
        public VandVProcedureStep SelectedStep
        {
            get => this.selectedStep;
            set => this.RaiseAndSetIfChanged(ref this.selectedStep, value);
        }

        /// <summary>Gets or sets the execution status, inherited by every item this activity performs.</summary>
        public string Status
        {
            get => this.status;
            set => this.RaiseAndSetIfChanged(ref this.status, value);
        }

        /// <summary>Gets or sets the actual execution date.</summary>
        public DateTime? ActualDate
        {
            get => this.actualDate;
            set => this.RaiseAndSetIfChanged(ref this.actualDate, value);
        }

        /// <summary>Gets or sets the recorded result.</summary>
        public string Result
        {
            get => this.result;
            set => this.RaiseAndSetIfChanged(ref this.result, value);
        }

        /// <summary>Gets or sets the evidence reference, typically the report the activity produced.</summary>
        public string EvidenceReference
        {
            get => this.evidenceReference;
            set => this.RaiseAndSetIfChanged(ref this.evidenceReference, value);
        }

        /// <summary>
        /// Gets the command that appends a step to the procedure.
        /// </summary>
        public ReactiveCommand<Unit, Unit> AddStepCommand { get; }

        /// <summary>
        /// Gets the command that removes the selected step.
        /// </summary>
        public ReactiveCommand<Unit, Unit> RemoveStepCommand { get; }

        /// <summary>
        /// Gets the command that moves the selected step one place earlier.
        /// </summary>
        public ReactiveCommand<Unit, Unit> MoveStepUpCommand { get; }

        /// <summary>
        /// Gets the command that moves the selected step one place later.
        /// </summary>
        public ReactiveCommand<Unit, Unit> MoveStepDownCommand { get; }

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
                    case nameof(this.ShortName):
                        if (string.IsNullOrWhiteSpace(this.ShortName))
                        {
                            return "The activity number is mandatory.";
                        }

                        if (!ShortNamePattern.IsMatch(this.ShortName))
                        {
                            return "The activity number may contain only letters, digits and underscores (no spaces or punctuation).";
                        }

                        return this.usedShortNames.Contains(this.ShortName)
                            ? "Another requirement, V&V item or activity already uses this number."
                            : string.Empty;

                    case nameof(this.Name):
                        return string.IsNullOrWhiteSpace(this.Name) ? "The name is mandatory." : string.Empty;

                    case nameof(this.Owner):
                        return this.Owner == null ? "The owner is mandatory." : string.Empty;

                    case nameof(this.Method):
                        return string.IsNullOrWhiteSpace(this.Method) ? "The verification method is mandatory: every item this activity performs inherits it." : string.Empty;

                    case nameof(this.Stage):
                        return string.IsNullOrWhiteSpace(this.Stage) ? "The stage gate is mandatory. An activity is executed once, at one gate; recurring work (a mass budget at PDR and again at CDR) is one activity per gate, each with its own execution record." : string.Empty;

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
                { VandVParameter.Description, this.Description },
                { VandVParameter.Facility, this.Facility },
                { VandVParameter.ExternalResponsible, this.ResponsibleExternal },
                { VandVParameter.PlannedDate, this.PlannedDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) },
                { VandVCloseOut.PlanReferenceShortName, this.PlanReference },
                { VandVParameter.ProcedureReference, this.ProcedureReference },
                { VandVParameter.Preconditions, this.Preconditions },
                { VandVParameter.Conditions, this.Conditions },
                { VandVParameter.Status, this.Status },
                { VandVParameter.ActualDate, this.ActualDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) },
                { VandVParameter.Result, this.Result },
                { VandVParameter.EvidenceReference, this.EvidenceReference }
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
            var validated = new[] { nameof(this.ShortName), nameof(this.Name), nameof(this.Owner), nameof(this.Method), nameof(this.Stage) };

            return validated.Any(property => !string.IsNullOrEmpty(this[property]));
        }

        /// <summary>
        /// Loads the dialog from an existing activity.
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/>.</param>
        /// <param name="activity">The activity being edited.</param>
        private void LoadFrom(Iteration iteration, Requirement activity)
        {
            this.ShortName = activity.ShortName;
            this.Name = activity.Name;
            this.Owner = activity.Owner;
            this.Report = VandVActivityQuery.QueryReport(activity);

            this.Method = Attribute(activity, VandVParameter.Method);
            this.Stage = Attribute(activity, VandVParameter.Stage);
            this.Level = Attribute(activity, VandVParameter.Level);
            this.Description = Attribute(activity, VandVParameter.Description);
            this.Facility = Attribute(activity, VandVParameter.Facility);
            this.ResponsibleExternal = Attribute(activity, VandVParameter.ExternalResponsible);
            this.PlannedDate = ParseDate(Attribute(activity, VandVParameter.PlannedDate));
            this.PlanReference = Attribute(activity, VandVCloseOut.PlanReferenceShortName);
            this.ProcedureReference = Attribute(activity, VandVParameter.ProcedureReference);
            this.Preconditions = Attribute(activity, VandVParameter.Preconditions);
            this.Conditions = Attribute(activity, VandVParameter.Conditions);
            this.Status = Attribute(activity, VandVParameter.Status);
            this.ActualDate = ParseDate(Attribute(activity, VandVParameter.ActualDate));
            this.Result = Attribute(activity, VandVParameter.Result);
            this.EvidenceReference = Attribute(activity, VandVParameter.EvidenceReference);

            this.ProcedureSteps.AddRange(
                VandVProcedureWriter.QuerySteps(iteration, activity).Select(step => new VandVProcedureStep(step)));
        }

        /// <summary>
        /// Suggests the first free activity number of the form <c>ACT_&lt;n&gt;</c>.
        /// </summary>
        /// <param name="usedShortNames">The short-names already taken in the iteration.</param>
        /// <returns>A free activity number.</returns>
        private static string SuggestShortName(ICollection<string> usedShortNames)
        {
            for (var index = 1; ; index++)
            {
                var candidate = $"ACT_{index}";

                if (!usedShortNames.Contains(candidate))
                {
                    return candidate;
                }
            }
        }

        /// <summary>
        /// Reads a V&amp;V attribute off the activity.
        /// </summary>
        /// <param name="activity">The activity.</param>
        /// <param name="parameterTypeShortName">The parameter type short-name.</param>
        /// <returns>The value, or null.</returns>
        private static string Attribute(Requirement activity, string parameterTypeShortName)
        {
            return VandVCoverageQuery.Attribute(activity, parameterTypeShortName);
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
    }
}
