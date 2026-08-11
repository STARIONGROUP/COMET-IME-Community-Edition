// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVItemRowViewModel.cs" company="Starion Group S.A.">
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

namespace CDP4VandV.ViewModels.Rows
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using CDP4VandV.Services;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Composition.DragDrop;
    using CDP4Composition.Mvvm;

    using CDP4Dal;

    using ReactiveUI;

    /// <summary>
    /// The row representing a single V&amp;V item (a <see cref="Requirement"/> categorized <c>VnV Item</c>) in the VCD
    /// browser. The V&amp;V attributes carried as <see cref="SimpleParameterValue"/>s are projected onto plain CLR
    /// properties so the DevExpress grid can filter, sort, group and choose columns over them.
    /// </summary>
    public class VandVItemRowViewModel : RowViewModelBase<Requirement>, IDropTarget
    {
        /// <summary>
        /// Backing field for <see cref="Name"/>
        /// </summary>
        private string name;

        /// <summary>
        /// Backing field for <see cref="ShortName"/>
        /// </summary>
        private string shortName;

        /// <summary>
        /// Backing field for <see cref="Definition"/>
        /// </summary>
        private string definition;

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
        /// Backing field for <see cref="Status"/>
        /// </summary>
        private string status;

        /// <summary>
        /// Backing field for <see cref="Criticality"/>
        /// </summary>
        private string criticality;

        /// <summary>
        /// Backing field for <see cref="PlannedDate"/>
        /// </summary>
        private string plannedDate;

        /// <summary>
        /// Backing field for <see cref="ActualDate"/>
        /// </summary>
        private string actualDate;

        /// <summary>
        /// Backing field for <see cref="ActivityNumber"/>
        /// </summary>
        private string activityNumber;

        /// <summary>
        /// Backing field for <see cref="Acceptance"/>
        /// </summary>
        private string acceptance;

        /// <summary>
        /// Backing field for <see cref="Result"/>
        /// </summary>
        private string result;

        /// <summary>
        /// Backing field for <see cref="EvidenceReference"/>
        /// </summary>
        private string evidenceReference;

        /// <summary>
        /// Backing field for <see cref="Owner"/>
        /// </summary>
        private string owner;

        /// <summary>
        /// Backing field for <see cref="AnnotationState"/>
        /// </summary>
        private AnnotationState annotationState;

        /// <summary>
        /// Backing field for <see cref="Compliance"/>
        /// </summary>
        private string compliance;

        /// <summary>
        /// Backing field for <see cref="CloseOut"/>
        /// </summary>
        private string closeOut;

        /// <summary>
        /// Backing field for <see cref="Analysis"/>
        /// </summary>
        private string analysis;

        /// <summary>
        /// Backing field for <see cref="Procedure"/>
        /// </summary>
        private string procedure;

        /// <summary>
        /// Initializes a new instance of the <see cref="VandVItemRowViewModel"/> class.
        /// </summary>
        /// <param name="requirement">The <see cref="Requirement"/> that is the V&amp;V item.</param>
        /// <param name="session">The <see cref="ISession"/>.</param>
        /// <param name="containerViewModel">The container <see cref="IViewModelBase{Thing}"/>.</param>
        public VandVItemRowViewModel(Requirement requirement, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(requirement, session, containerViewModel)
        {
            this.SetProperties();
        }

        /// <summary>Gets the name of the V&amp;V item.</summary>
        public string Name
        {
            get => this.name;
            private set => this.RaiseAndSetIfChanged(ref this.name, value);
        }

        /// <summary>Gets the short-name of the V&amp;V item.</summary>
        public string ShortName
        {
            get => this.shortName;
            private set => this.RaiseAndSetIfChanged(ref this.shortName, value);
        }

        /// <summary>Gets the first definition of the V&amp;V item.</summary>
        public string Definition
        {
            get => this.definition;
            private set => this.RaiseAndSetIfChanged(ref this.definition, value);
        }

        /// <summary>Gets the verification method (<c>vnv_method</c>).</summary>
        public string Method
        {
            get => this.method;
            private set => this.RaiseAndSetIfChanged(ref this.method, value);
        }

        /// <summary>Gets the stage gate (<c>vnv_stage</c>).</summary>
        public string Stage
        {
            get => this.stage;
            private set => this.RaiseAndSetIfChanged(ref this.stage, value);
        }

        /// <summary>Gets the integration level (<c>vnv_level</c>).</summary>
        public string Level
        {
            get => this.level;
            private set => this.RaiseAndSetIfChanged(ref this.level, value);
        }

        /// <summary>Gets the status (<c>vnv_status</c>).</summary>
        public string Status
        {
            get => this.status;
            private set => this.RaiseAndSetIfChanged(ref this.status, value);
        }

        /// <summary>Gets the criticality (<c>vnv_criticality</c>).</summary>
        public string Criticality
        {
            get => this.criticality;
            private set => this.RaiseAndSetIfChanged(ref this.criticality, value);
        }

        /// <summary>Gets the planned date (<c>vnv_planned_date</c>).</summary>
        public string PlannedDate
        {
            get => this.plannedDate;
            private set => this.RaiseAndSetIfChanged(ref this.plannedDate, value);
        }

        /// <summary>Gets the actual date (<c>vnv_actual_date</c>).</summary>
        public string ActualDate
        {
            get => this.actualDate;
            private set => this.RaiseAndSetIfChanged(ref this.actualDate, value);
        }

        /// <summary>Gets the activity number (<c>vnv_activity_no</c>).</summary>
        public string ActivityNumber
        {
            get => this.activityNumber;
            private set => this.RaiseAndSetIfChanged(ref this.activityNumber, value);
        }

        /// <summary>Gets the acceptance criteria (<c>vnv_acceptance</c>).</summary>
        public string Acceptance
        {
            get => this.acceptance;
            private set => this.RaiseAndSetIfChanged(ref this.acceptance, value);
        }

        /// <summary>Gets the result (<c>vnv_result</c>).</summary>
        public string Result
        {
            get => this.result;
            private set => this.RaiseAndSetIfChanged(ref this.result, value);
        }

        /// <summary>
        /// Gets the compliance status (<c>vnv_compliance</c>): does the design meet the requirement, as opposed to
        /// whether the activity has run.
        /// </summary>
        public string Compliance
        {
            get => this.compliance;
            private set => this.RaiseAndSetIfChanged(ref this.compliance, value);
        }

        /// <summary>
        /// Gets the close-out status, "Closed" plus the reason once the item has been accepted.
        /// </summary>
        public string CloseOut
        {
            get => this.closeOut;
            private set => this.RaiseAndSetIfChanged(ref this.closeOut, value);
        }

        /// <summary>
        /// Gets the automatic analysis verdict for an item that covers a parameter constrained by its requirement.
        /// </summary>
        public string Analysis
        {
            get => this.analysis;
            private set => this.RaiseAndSetIfChanged(ref this.analysis, value);
        }

        /// <summary>
        /// Gets the procedure summary: how many steps the activity has and where they stand, or the procedure
        /// reference when the procedure is written down elsewhere.
        /// </summary>
        public string Procedure
        {
            get => this.procedure;
            private set => this.RaiseAndSetIfChanged(ref this.procedure, value);
        }

        /// <summary>Gets the evidence reference (<c>vnv_evidence_ref</c>).</summary>
        public string EvidenceReference
        {
            get => this.evidenceReference;
            private set => this.RaiseAndSetIfChanged(ref this.evidenceReference, value);
        }

        /// <summary>Gets the responsible domain (native <c>Owner</c>).</summary>
        public string Owner
        {
            get => this.owner;
            private set => this.RaiseAndSetIfChanged(ref this.owner, value);
        }

        /// <summary>
        /// Accepts a <see cref="ParameterOrOverrideBase"/> dragged from the Element Definitions browser, so a V&amp;V
        /// item can be coupled to the parameter it measures without opening the dialog.
        /// </summary>
        /// <param name="dropInfo">The <see cref="IDropInfo"/>.</param>
        public void DragOver(IDropInfo dropInfo)
        {
            dropInfo.Effects = dropInfo.Payload is ParameterOrOverrideBase
                ? System.Windows.DragDropEffects.Copy
                : System.Windows.DragDropEffects.None;
        }

        /// <summary>
        /// Handles the drop of a parameter onto this V&amp;V item by raising <see cref="ParameterDropped"/>; the browser
        /// owns the write so this row stays free of transaction logic.
        /// </summary>
        /// <param name="dropInfo">The <see cref="IDropInfo"/>.</param>
        /// <returns>A completed <see cref="Task"/>.</returns>
        public Task Drop(IDropInfo dropInfo)
        {
            if (dropInfo.Payload is ParameterOrOverrideBase parameter)
            {
                this.ParameterDropped?.Invoke(this, parameter);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Raised when a parameter is dropped onto this V&amp;V item.
        /// </summary>
        public event EventHandler<ParameterOrOverrideBase> ParameterDropped;

        /// <summary>
        /// Re-reads the V&amp;V attribute values from the <see cref="Requirement"/>'s <see cref="SimpleParameterValue"/>s.
        /// Called by the browser when a contained <see cref="SimpleParameterValue"/> is updated.
        /// </summary>
        public void RefreshAttributes()
        {
            this.SetProperties();
        }

        /// <summary>
        /// Gets the review-request state of this item, none raised, at least one still open, or all closed out. The
        /// browser renders this as a distinct icon so a blocked item is visible without opening any menu.
        /// </summary>
        public AnnotationState AnnotationState
        {
            get => this.annotationState;
            private set => this.RaiseAndSetIfChanged(ref this.annotationState, value);
        }

        /// <summary>
        /// Re-reads the procedure summary.
        /// </summary>
        public void RefreshProcedure()
        {
            var iteration = this.Thing.GetContainerOfType<Iteration>();
            var reference = this.Attribute("vnv_procedure_ref");

            if (iteration == null)
            {
                this.Procedure = reference;
                return;
            }

            var steps = VandVProcedureWriter.QuerySteps(iteration, this.Thing);

            if (!steps.Any())
            {
                this.Procedure = string.IsNullOrWhiteSpace(reference) ? "No steps" : reference;
                return;
            }

            // "recorded" means a human has set the step's Result to something other than Not Run. Nothing is
            // executed by the tool; this counts how much of the as-run record has been filled in.
            var recorded = steps.Count(step =>
            {
                var result = VandVCoverageQuery.Attribute(step, "vnv_step_result");
                return !string.IsNullOrWhiteSpace(result) && !VandVCoverageQuery.AreSameEnumValue(result, "Not Run");
            });

            var failed = steps.Count(step => VandVCoverageQuery.AreSameEnumValue(VandVCoverageQuery.Attribute(step, "vnv_step_result"), "Fail"));

            var summary = $"{recorded} of {steps.Count} step(s) recorded";

            this.Procedure = failed == 0 ? summary : $"{summary}, {failed} failed";
        }

        /// <summary>
        /// Re-evaluates the covered parameter against the requirement's parametric constraints.
        /// </summary>
        public void RefreshAnalysis()
        {
            var iteration = this.Thing.GetContainerOfType<Iteration>();

            this.Analysis = iteration == null
                ? string.Empty
                : VandVAnalysisChecker.Check(iteration, this.Thing).Display;
        }

        /// <summary>
        /// Re-reads the review requests raised against this item.
        /// </summary>
        public void RefreshAnnotationState()
        {
            var iteration = this.Thing.GetContainerOfType<Iteration>();

            this.AnnotationState = iteration == null
                ? AnnotationState.None
                : AnnotationQuery.SummarizeState(AnnotationQuery.QueryFor(iteration, this.Thing));
        }

        /// <summary>
        /// Refreshes the projected properties when the underlying <see cref="Requirement"/> changes.
        /// </summary>
        protected override void UpdateThingStatus()
        {
            base.UpdateThingStatus();
            this.SetProperties();
        }

        /// <summary>
        /// Updates the projected properties from the underlying <see cref="Requirement"/>.
        /// </summary>
        private void SetProperties()
        {
            this.Name = this.Thing.Name;
            this.ShortName = this.Thing.ShortName;
            this.Definition = this.Thing.Definition.FirstOrDefault()?.Content;
            this.Method = this.Attribute("vnv_method");
            this.Stage = this.Attribute("vnv_stage");
            this.Level = this.Attribute("vnv_level");
            this.Status = this.Attribute("vnv_status");
            this.Criticality = this.Attribute("vnv_criticality");
            this.PlannedDate = this.Attribute("vnv_planned_date");
            this.ActualDate = this.Attribute("vnv_actual_date");
            this.ActivityNumber = this.Attribute("vnv_activity_no");
            this.Acceptance = this.Attribute("vnv_acceptance");
            this.Result = this.Attribute("vnv_result");
            this.EvidenceReference = this.Attribute("vnv_evidence_ref");
            this.Owner = this.Thing.Owner?.ShortName;
            this.Compliance = VandVCloseOut.QueryCompliance(this.Thing);

            var reason = this.Attribute(VandVCloseOut.CloseOutReasonShortName);

            this.CloseOut = VandVCloseOut.IsClosed(this.Thing)
                ? (string.IsNullOrWhiteSpace(reason) ? "Closed" : $"Closed: {reason}")
                : "Open";

            this.RefreshAnnotationState();
            this.RefreshAnalysis();
            this.RefreshProcedure();
        }

        /// <summary>
        /// Returns the first value of the <see cref="SimpleParameterValue"/> whose <see cref="ParameterType"/> has the
        /// supplied short-name, or null when the V&amp;V item does not carry that attribute.
        /// </summary>
        /// <param name="shortName">The parameter type short-name.</param>
        /// <returns>The attribute value, or null.</returns>
        private string Attribute(string shortName)
        {
            return this.Thing.ParameterValue
                .FirstOrDefault(x => x.ParameterType != null && x.ParameterType.ShortName == shortName)?
                .Value.FirstOrDefault();
        }
    }
}
