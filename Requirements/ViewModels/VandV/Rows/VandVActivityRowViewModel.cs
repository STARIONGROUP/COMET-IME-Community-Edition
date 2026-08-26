// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVActivityRowViewModel.cs" company="Starion Group S.A.">
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

namespace CDP4Requirements.ViewModels.Rows
{
    using System.Collections.Generic;

    using CDP4Requirements.Rdl;
    using CDP4Requirements.Services;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Mvvm;

    using CDP4Dal;

    using ReactiveUI;

    /// <summary>
    /// The row representing a shared V&amp;V activity in the Activities view: one task performing the verification of
    /// the V&amp;V items nested under it. The Coverage column rolls those items up, so one glance says whether running
    /// this activity has moved its requirements.
    /// </summary>
    public class VandVActivityRowViewModel : RowViewModelBase<Requirement>
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
        /// Backing field for <see cref="Coverage"/>
        /// </summary>
        private string coverage;

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
        /// Backing field for <see cref="PlannedDate"/>
        /// </summary>
        private string plannedDate;

        /// <summary>
        /// Backing field for <see cref="ActualDate"/>
        /// </summary>
        private string actualDate;

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
        /// Initializes a new instance of the <see cref="VandVActivityRowViewModel"/> class.
        /// </summary>
        /// <param name="activity">The <see cref="Requirement"/> that is the shared activity.</param>
        /// <param name="session">The <see cref="ISession"/>.</param>
        /// <param name="containerViewModel">The container <see cref="IViewModelBase{Thing}"/>.</param>
        public VandVActivityRowViewModel(Requirement activity, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(activity, session, containerViewModel)
        {
            this.SetProperties();
        }

        /// <summary>Gets the name of the activity.</summary>
        public string Name
        {
            get => this.name;
            private set => this.RaiseAndSetIfChanged(ref this.name, value);
        }

        /// <summary>Gets the activity number, the activity's short-name.</summary>
        public string ShortName
        {
            get => this.shortName;
            private set => this.RaiseAndSetIfChanged(ref this.shortName, value);
        }

        /// <summary>
        /// Gets the roll-up of the V&amp;V items this activity performs, e.g. "14 item(s): 12 passed, 2 open".
        /// </summary>
        public string Coverage
        {
            get => this.coverage;
            private set => this.RaiseAndSetIfChanged(ref this.coverage, value);
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

        /// <summary>Gets the execution status (<c>vnv_status</c>).</summary>
        public string Status
        {
            get => this.status;
            private set => this.RaiseAndSetIfChanged(ref this.status, value);
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

        /// <summary>Gets the recorded result (<c>vnv_result</c>).</summary>
        public string Result
        {
            get => this.result;
            private set => this.RaiseAndSetIfChanged(ref this.result, value);
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
        /// Refreshes the roll-up of the V&amp;V items this activity performs. The items are supplied rather than read
        /// off the contained rows, because the rows nested under an activity are its procedure steps: this panel
        /// shows the work, and the coverage of that work is this one column.
        /// </summary>
        /// <param name="performedItems">The V&amp;V items the activity performs.</param>
        public void RefreshCoverage(IEnumerable<Requirement> performedItems)
        {
            var rollUp = VandVCoverageQuery.RollUp(performedItems);

            this.Coverage = rollUp.Total == 0 ? "No V&V items" : rollUp.ToRequirementSummary();
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
            this.Method = VandVCoverageQuery.Attribute(this.Thing, VandVParameter.Method);
            this.Stage = VandVCoverageQuery.Attribute(this.Thing, VandVParameter.Stage);
            this.Level = VandVCoverageQuery.Attribute(this.Thing, VandVParameter.Level);
            this.Status = VandVCoverageQuery.Attribute(this.Thing, VandVParameter.Status);
            this.PlannedDate = VandVCoverageQuery.Attribute(this.Thing, VandVParameter.PlannedDate);
            this.ActualDate = VandVCoverageQuery.Attribute(this.Thing, VandVParameter.ActualDate);
            this.Result = VandVCoverageQuery.Attribute(this.Thing, VandVParameter.Result);
            this.EvidenceReference = VandVCoverageQuery.Attribute(this.Thing, VandVParameter.EvidenceReference);
            this.Owner = this.Thing.Owner?.ShortName;

        }
    }
}
