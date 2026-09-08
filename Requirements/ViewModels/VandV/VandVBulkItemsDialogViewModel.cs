// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVBulkItemsDialogViewModel.cs" company="Starion Group S.A.">
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
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;

    using CDP4Requirements.Services;
    using CDP4Requirements.ViewModels.Rows;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;

    using CDP4Dal;

    using ReactiveUI;

    /// <summary>
    /// The dialog for creating one thin V&amp;V item per ticked requirement, all performed by the same shared activity.
    /// This is the "thirty mass requirements, one mass budget" gesture: the activity carries the description, the
    /// procedure and the execution record, the items carry only the per-requirement traceability and judgement.
    /// </summary>
    public class VandVBulkItemsDialogViewModel : DialogViewModelBase
    {
        /// <summary>
        /// Backing field for <see cref="LinkType"/>
        /// </summary>
        private string linkType;

        /// <summary>
        /// Backing field for <see cref="Owner"/>
        /// </summary>
        private DomainOfExpertise owner;

        /// <summary>
        /// Backing field for <see cref="AcceptanceCriteria"/>
        /// </summary>
        private string acceptanceCriteria;

        /// <summary>
        /// Backing field for <see cref="Validation"/>
        /// </summary>
        private readonly ObservableAsPropertyHelper<string> validation;

        /// <summary>
        /// Initializes a new instance of the <see cref="VandVBulkItemsDialogViewModel"/> class.
        /// </summary>
        /// <param name="activity">The activity that will perform the created items.</param>
        /// <param name="iteration">The <see cref="Iteration"/> the requirements live in.</param>
        /// <param name="session">The <see cref="ISession"/>.</param>
        public VandVBulkItemsDialogViewModel(Requirement activity, Iteration iteration, ISession session)
        {
            this.Activity = activity;

            var model = (EngineeringModel)iteration.Container;
            this.PossibleOwners = model.EngineeringModelSetup.ActiveDomain.OrderBy(x => x.Name).ToList();
            this.PossibleLinkTypes = new[] { VandVItemDialogViewModel.VerifiesLink, VandVItemDialogViewModel.ValidatesLink };
            this.LinkType = VandVItemDialogViewModel.VerifiesLink;
            this.Owner = session.OpenIterations.TryGetValue(iteration, out var tuple) ? tuple?.Item1 : null;

            var alreadyPerformed = new HashSet<Guid>(
                VandVActivityQuery.QueryPerformedItems(iteration, activity)
                    .Select(item => VandVItemCreator.QueryCoveringRelationship(iteration, item)?.Target?.Iid ?? Guid.Empty));

            this.PossibleRequirements = VandVCoverageQuery.Build(iteration).Coverages
                .Where(coverage => !alreadyPerformed.Contains(coverage.Requirement.Iid))
                .Select(coverage => new VandVBulkItemRowViewModel(
                    coverage.Requirement,
                    coverage.VandVItems.Any()
                        ? $"{coverage.Requirement.ShortName}: {coverage.Requirement.Name} (already covered by {coverage.VandVItems.Count} item(s))"
                        : $"{coverage.Requirement.ShortName}: {coverage.Requirement.Name}"))
                .ToList();

            var rowChanged = this.PossibleRequirements
                .Select(row => row.WhenAnyValue(x => x.IsSelected, x => x.AcceptanceCriteria).Select(_ => Unit.Default))
                .Merge()
                .StartWith(Unit.Default);

            var stateChanged = rowChanged
                .Merge(this.WhenAnyValue(x => x.Owner, x => x.AcceptanceCriteria).Select(_ => Unit.Default));

            this.validation = stateChanged
                .Select(_ => this.DescribeValidation())
                .ToProperty(this, x => x.Validation);

            var canOk = stateChanged.Select(_ =>
                this.Owner != null
                && this.SelectedRows.Any()
                && this.SelectedRows.All(row => !string.IsNullOrWhiteSpace(this.EffectiveAcceptanceCriteria(row))));

            this.SelectAllCommand = ReactiveCommandCreator.Create(() => this.SetSelection(true));
            this.ClearSelectionCommand = ReactiveCommandCreator.Create(() => this.SetSelection(false));
            this.ApplyDefaultCommand = ReactiveCommandCreator.Create(this.ApplyDefaultToSelection);

            this.OkCommand = ReactiveCommandCreator.Create(() => { this.DialogResult = new BaseDialogResult(true); }, canOk);
            this.CancelCommand = ReactiveCommandCreator.Create(() => { this.DialogResult = new BaseDialogResult(false); });
        }

        /// <summary>
        /// Gets the activity that will perform the created items.
        /// </summary>
        public Requirement Activity { get; }

        /// <summary>
        /// Gets the window title.
        /// </summary>
        public string Title => $"Create V&V Items performed by {this.Activity.ShortName}";

        /// <summary>
        /// Gets the caption describing the activity the items will point at.
        /// </summary>
        public string ActivityCaption => $"{this.Activity.ShortName}: {this.Activity.Name}";

        /// <summary>
        /// Gets the requirements on offer, one tick-box row each. Requirements this activity already performs an item
        /// for are not offered.
        /// </summary>
        public IReadOnlyList<VandVBulkItemRowViewModel> PossibleRequirements { get; }

        /// <summary>
        /// Gets the possible link types (<c>verifies</c> / <c>validates</c>).
        /// </summary>
        public IReadOnlyList<string> PossibleLinkTypes { get; }

        /// <summary>
        /// Gets the possible owning <see cref="DomainOfExpertise"/>s.
        /// </summary>
        public IReadOnlyList<DomainOfExpertise> PossibleOwners { get; }

        /// <summary>
        /// Gets or sets whether the created items verify or validate their requirement.
        /// </summary>
        public string LinkType
        {
            get => this.linkType;
            set => this.RaiseAndSetIfChanged(ref this.linkType, value);
        }

        /// <summary>
        /// Gets or sets the owner of the created items.
        /// </summary>
        public DomainOfExpertise Owner
        {
            get => this.owner;
            set => this.RaiseAndSetIfChanged(ref this.owner, value);
        }

        /// <summary>
        /// Gets or sets the default acceptance criteria, used for every ticked requirement that states none of its own.
        /// </summary>
        public string AcceptanceCriteria
        {
            get => this.acceptanceCriteria;
            set => this.RaiseAndSetIfChanged(ref this.acceptanceCriteria, value);
        }

        /// <summary>
        /// Gets the line above the buttons saying what still blocks creation, or an empty string when nothing does.
        /// </summary>
        public string Validation => this.validation.Value;

        /// <summary>
        /// Gets the ticked requirements.
        /// </summary>
        public IReadOnlyList<Requirement> SelectedRequirements =>
            this.SelectedRows.Select(row => row.Requirement).ToList();

        /// <summary>
        /// Gets the acceptance criteria each ticked requirement's item will carry, keyed by the requirement's
        /// <see cref="CDP4Common.CommonData.Thing.Iid"/>.
        /// </summary>
        public IReadOnlyDictionary<Guid, string> AcceptanceCriteriaByRequirement =>
            this.SelectedRows.ToDictionary(row => row.Requirement.Iid, this.EffectiveAcceptanceCriteria);

        /// <summary>
        /// Gets the command that copies the default onto every ticked requirement that has no criteria yet.
        /// </summary>
        public ReactiveCommand<Unit, Unit> ApplyDefaultCommand { get; }

        /// <summary>
        /// Gets the ticked rows.
        /// </summary>
        private IReadOnlyList<VandVBulkItemRowViewModel> SelectedRows =>
            this.PossibleRequirements.Where(row => row.IsSelected).ToList();

        /// <summary>
        /// Gets the command that ticks every requirement.
        /// </summary>
        public ReactiveCommand<Unit, Unit> SelectAllCommand { get; }

        /// <summary>
        /// Gets the command that unticks every requirement.
        /// </summary>
        public ReactiveCommand<Unit, Unit> ClearSelectionCommand { get; }

        /// <summary>
        /// Gets the command that accepts the dialog. Enabled once at least one requirement is ticked.
        /// </summary>
        public ReactiveCommand<Unit, Unit> OkCommand { get; }

        /// <summary>
        /// Gets the command that cancels the dialog.
        /// </summary>
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }

        /// <summary>
        /// Ticks or unticks every requirement on offer.
        /// </summary>
        /// <param name="isSelected">The selection to apply.</param>
        private void SetSelection(bool isSelected)
        {
            foreach (var row in this.PossibleRequirements)
            {
                row.IsSelected = isSelected;
            }
        }

        /// <summary>
        /// Writes the default onto every ticked requirement that states no criteria of its own, so the rest can be
        /// edited from a filled-in starting point rather than from nothing.
        /// </summary>
        private void ApplyDefaultToSelection()
        {
            foreach (var row in this.SelectedRows.Where(row => string.IsNullOrWhiteSpace(row.AcceptanceCriteria)))
            {
                row.AcceptanceCriteria = this.AcceptanceCriteria;
            }
        }

        /// <summary>
        /// Resolves the acceptance criteria a row's item will carry: its own where it states any, the dialog's default
        /// otherwise.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <returns>The effective acceptance criteria, which may still be blank.</returns>
        private string EffectiveAcceptanceCriteria(VandVBulkItemRowViewModel row)
        {
            return string.IsNullOrWhiteSpace(row.AcceptanceCriteria) ? this.AcceptanceCriteria : row.AcceptanceCriteria;
        }

        /// <summary>
        /// Says what still blocks creation.
        /// </summary>
        /// <returns>The message, or an empty string when nothing blocks it.</returns>
        private string DescribeValidation()
        {
            if (this.Owner == null)
            {
                return "Pick an owner.";
            }

            var selected = this.SelectedRows;

            if (!selected.Any())
            {
                return "Tick at least one requirement.";
            }

            var missing = selected.Count(row => string.IsNullOrWhiteSpace(this.EffectiveAcceptanceCriteria(row)));

            if (missing == 0)
            {
                return string.Empty;
            }

            return missing == selected.Count
                ? $"All {missing} ticked requirements need acceptance criteria: fill in the default above, or give each its own."
                : $"{missing} of {selected.Count} ticked requirements have no acceptance criteria.";
        }

    }
}
