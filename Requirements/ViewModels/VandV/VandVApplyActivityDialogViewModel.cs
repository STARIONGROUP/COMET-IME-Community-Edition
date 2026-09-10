// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVApplyActivityDialogViewModel.cs" company="Starion Group S.A.">
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
    using System.Globalization;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;

    using CDP4Requirements.Rdl;
    using CDP4Requirements.Services;
    using CDP4Requirements.ViewModels.Rows;

    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;

    using CDP4Dal;

    using ReactiveUI;

    /// <summary>
    /// The dialog that applies an executed activity's result to the V&amp;V items it performs, in one confirmed bulk
    /// write: the execution record is copied onto every ticked item, and, when explicitly requested, the items are
    /// closed out with a stated reason. Items whose analysis check is violated, that have an open review request, or
    /// that are already closed start out unticked, so the bulk gesture cannot silently close what needs a human look.
    /// </summary>
    /// <remarks>
    /// This stays within the plugin's discipline: execution status may follow the activity, but compliance and
    /// close-out are per-requirement judgements, so they are applied only when the user ticks the close-out box and
    /// states the reason, exactly what ECSS-E-ST-10-02 expects of a close-out record.
    /// </remarks>
    public class VandVApplyActivityDialogViewModel : DialogViewModelBase
    {
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
        /// Backing field for <see cref="AlsoCloseOut"/>
        /// </summary>
        private bool alsoCloseOut;

        /// <summary>
        /// Backing field for <see cref="Compliance"/>
        /// </summary>
        private string compliance;

        /// <summary>
        /// Backing field for <see cref="CloseOutReason"/>
        /// </summary>
        private string closeOutReason;

        /// <summary>
        /// Initializes a new instance of the <see cref="VandVApplyActivityDialogViewModel"/> class.
        /// </summary>
        /// <param name="activity">The executed activity.</param>
        /// <param name="iteration">The <see cref="Iteration"/>.</param>
        /// <param name="session">The <see cref="ISession"/>, used to prefill who closes out.</param>
        public VandVApplyActivityDialogViewModel(Requirement activity, Iteration iteration, ISession session)
        {
            this.Activity = activity;

            var model = (EngineeringModel)iteration.Container;
            var mrdl = model.EngineeringModelSetup.RequiredRdl.FirstOrDefault();

            this.PossibleStatuses = VandVItemDialogViewModel.EnumerationValues(mrdl, VandVParameter.Status);
            this.PossibleCompliances = VandVItemDialogViewModel.EnumerationValues(mrdl, VandVCloseOut.ComplianceShortName);

            if (!this.PossibleCompliances.Any())
            {
                this.PossibleCompliances = VandVCloseOut.PossibleCompliances;
            }

            this.Status = VandVCoverageQuery.Attribute(activity, VandVParameter.Status);
            this.ActualDate = ParseDate(VandVCoverageQuery.Attribute(activity, VandVParameter.ActualDate));
            this.Result = VandVCoverageQuery.Attribute(activity, VandVParameter.Result);
            this.EvidenceReference = VandVCoverageQuery.Attribute(activity, VandVParameter.EvidenceReference)
                                     ?? VandVActivityQuery.QueryReport(activity)?.ShortName;

            this.Compliance = this.PossibleCompliances.FirstOrDefault(x => VandVCoverageQuery.AreSameEnumValue(x, VandVCompliance.Compliant))
                              ?? this.PossibleCompliances.FirstOrDefault();

            this.CloseOutReason = $"Verified by activity {activity.ShortName}: {activity.Name}";
            this.ClosedBy = session.ActivePerson?.Name;

            var coveredByItem = VandVItemCreator.QueryCoveringMap(iteration);

            this.Items = VandVActivityQuery.QueryPerformedItems(iteration, activity)
                .Select(item => BuildItemRow(iteration, item, coveredByItem.TryGetValue(item.Iid, out var covered) ? covered : null))
                .ToList();

            var selectionChanged = this.Items
                .Select(row => row.WhenAnyValue(x => x.IsSelected).Select(_ => Unit.Default))
                .Merge()
                .StartWith(Unit.Default);

            var canOk = selectionChanged
                .Merge(this.WhenAnyValue(x => x.AlsoCloseOut, x => x.CloseOutReason).Select(_ => Unit.Default))
                .Select(_ => this.Items.Any(row => row.IsSelected)
                             && (!this.AlsoCloseOut || !string.IsNullOrWhiteSpace(this.CloseOutReason)));

            this.OkCommand = ReactiveCommandCreator.Create(() => { this.DialogResult = new BaseDialogResult(true); }, canOk);
            this.CancelCommand = ReactiveCommandCreator.Create(() => { this.DialogResult = new BaseDialogResult(false); });
        }

        /// <summary>
        /// Gets the executed activity.
        /// </summary>
        public Requirement Activity { get; }

        /// <summary>
        /// Gets the window title.
        /// </summary>
        public string Title => $"Apply {this.Activity.ShortName} to its V&V Items";

        /// <summary>
        /// Gets the caption describing the activity being applied.
        /// </summary>
        public string ActivityCaption => $"{this.Activity.ShortName}: {this.Activity.Name}";

        /// <summary>
        /// Gets the performed items, one tick-box row each. Guarded items (violated analysis check, open review
        /// request, already closed) start out unticked, with the guard named in the row text.
        /// </summary>
        public IReadOnlyList<SelectableThingRowViewModel> Items { get; }

        /// <summary>
        /// Gets the possible <c>vnv_status</c> values.
        /// </summary>
        public IReadOnlyList<string> PossibleStatuses { get; }

        /// <summary>
        /// Gets the possible <c>vnv_compliance</c> values.
        /// </summary>
        public IReadOnlyList<string> PossibleCompliances { get; }

        /// <summary>
        /// Gets or sets the execution status applied to every ticked item.
        /// </summary>
        public string Status
        {
            get => this.status;
            set => this.RaiseAndSetIfChanged(ref this.status, value);
        }

        /// <summary>
        /// Gets or sets the actual date applied to every ticked item.
        /// </summary>
        public DateTime? ActualDate
        {
            get => this.actualDate;
            set => this.RaiseAndSetIfChanged(ref this.actualDate, value);
        }

        /// <summary>
        /// Gets or sets the result applied to every ticked item.
        /// </summary>
        public string Result
        {
            get => this.result;
            set => this.RaiseAndSetIfChanged(ref this.result, value);
        }

        /// <summary>
        /// Gets or sets the evidence reference applied to every ticked item.
        /// </summary>
        public string EvidenceReference
        {
            get => this.evidenceReference;
            set => this.RaiseAndSetIfChanged(ref this.evidenceReference, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the ticked items are also closed out. Off by default: closing out
        /// is a judgement, not a side effect.
        /// </summary>
        public bool AlsoCloseOut
        {
            get => this.alsoCloseOut;
            set => this.RaiseAndSetIfChanged(ref this.alsoCloseOut, value);
        }

        /// <summary>
        /// Gets or sets the compliance recorded when closing out.
        /// </summary>
        public string Compliance
        {
            get => this.compliance;
            set => this.RaiseAndSetIfChanged(ref this.compliance, value);
        }

        /// <summary>
        /// Gets or sets the close-out reason, mandatory when closing out.
        /// </summary>
        public string CloseOutReason
        {
            get => this.closeOutReason;
            set => this.RaiseAndSetIfChanged(ref this.closeOutReason, value);
        }

        /// <summary>
        /// Gets who closes the items out, prefilled with the active person.
        /// </summary>
        public string ClosedBy { get; }

        /// <summary>
        /// Gets the ticked items.
        /// </summary>
        public IReadOnlyList<Requirement> SelectedItems =>
            this.Items.Where(row => row.IsSelected).Select(row => row.Thing).OfType<Requirement>().ToList();

        /// <summary>
        /// Gets the command that accepts the dialog. Enabled once at least one item is ticked, and, when closing out,
        /// a reason is stated.
        /// </summary>
        public ReactiveCommand<Unit, Unit> OkCommand { get; }

        /// <summary>
        /// Gets the command that cancels the dialog.
        /// </summary>
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }

        /// <summary>
        /// Builds the attribute values applied to every ticked item. Only non-blank values are written, and the
        /// close-out record is included only when <see cref="AlsoCloseOut"/> is ticked.
        /// </summary>
        /// <returns>The attribute values, keyed by parameter type short-name.</returns>
        public IReadOnlyDictionary<string, string> BuildAttributes()
        {
            var attributes = new Dictionary<string, string>
            {
                { VandVParameter.Status, this.Status },
                { VandVParameter.ActualDate, this.ActualDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) },
                { VandVParameter.Result, this.Result },
                { VandVParameter.EvidenceReference, this.EvidenceReference }
            };

            if (this.AlsoCloseOut)
            {
                attributes.Add(VandVCloseOut.ComplianceShortName, this.Compliance);
                attributes.Add(VandVCloseOut.ClosedShortName, "true");
                attributes.Add(VandVCloseOut.CloseOutReasonShortName, this.CloseOutReason);
                attributes.Add(VandVCloseOut.ClosedByShortName, this.ClosedBy);
                attributes.Add(VandVCloseOut.ClosedOnShortName, DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            }

            return attributes;
        }

        /// <summary>
        /// Builds the tick-box row for one performed item, unticked and annotated when a guard applies.
        /// </summary>
        /// <param name="iteration">The iteration.</param>
        /// <param name="item">The performed item.</param>
        /// <param name="covered">The requirement the item verifies, resolved once by the caller.</param>
        /// <returns>The row.</returns>
        private static SelectableThingRowViewModel BuildItemRow(Iteration iteration, Requirement item, Requirement covered)
        {
            var display = covered == null ? item.ShortName : $"{item.ShortName} ({covered.ShortName})";

            var guards = new List<string>();

            if (VandVAnalysisChecker.Check(iteration, item).State == VandVAnalysisState.Violated)
            {
                guards.Add("analysis check VIOLATED");
            }

            if (AnnotationQuery.QueryFor(iteration, item).Any(AnnotationQuery.IsOpen))
            {
                guards.Add("open review request");
            }

            if (VandVCloseOut.IsClosed(item))
            {
                guards.Add("already closed");
            }

            return guards.Any()
                ? new SelectableThingRowViewModel(item, $"{display}  [{string.Join(", ", guards)}]")
                : new SelectableThingRowViewModel(item, display, true);
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
    }
}
