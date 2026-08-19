// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StageGateDialogViewModel.cs" company="Starion Group S.A.">
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

    using CDP4Requirements.Rdl;

    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;

    using ReactiveUI;

    /// <summary>
    /// Asks the user to define the project's stage gates while the V&amp;V reference data is being set up, instead of
    /// silently seeding a hard-coded list. Stage gates differ per project, so they are decided exactly once, here,
    /// by the person running the set-up. One row per gate, ordered as the project reviews run.
    /// </summary>
    public class StageGateDialogViewModel : DialogViewModelBase
    {
        /// <summary>
        /// Backing field for <see cref="SelectedGate"/>
        /// </summary>
        private StageGateRowViewModel selectedGate;

        /// <summary>
        /// Initializes a new instance of the <see cref="StageGateDialogViewModel"/> class.
        /// </summary>
        /// <param name="proposedStageGates">The stage gates offered as a starting point, in order.</param>
        public StageGateDialogViewModel(IEnumerable<string> proposedStageGates)
        {
            foreach (var gate in proposedStageGates ?? Enumerable.Empty<string>())
            {
                this.StageGates.Add(new StageGateRowViewModel { Name = gate });
            }

            var hasSelection = this.WhenAnyValue(x => x.SelectedGate).Select(gate => gate != null);

            this.AddGateCommand = ReactiveCommandCreator.Create(this.ExecuteAddGate);
            this.RemoveGateCommand = ReactiveCommandCreator.Create(this.ExecuteRemoveGate, hasSelection);
            this.MoveGateUpCommand = ReactiveCommandCreator.Create(() => this.MoveGate(-1), hasSelection);
            this.MoveGateDownCommand = ReactiveCommandCreator.Create(() => this.MoveGate(1), hasSelection);

            var structuralChanges = this.StageGates.Changed.Select(_ => Unit.Default).StartWith(Unit.Default);

            // the text typed into a row has to count as well: gating OK on structural changes alone left it enabled on
            // the state the last Add or Remove produced, so blanking every name inline still let the dialog be accepted
            var nameEdits = structuralChanges
                .Select(_ => this.StageGates.Select(row => row.WhenAnyValue(x => x.Name).Select(__ => Unit.Default)).Merge())
                .Switch();

            // the list contents drive OK, so an empty or blank-only table cannot be accepted
            var canOk = structuralChanges
                .Merge(nameEdits)
                .Select(_ => this.ParseStageGates().Any());

            this.OkCommand = ReactiveCommandCreator.Create(() => { this.DialogResult = new BaseDialogResult(true); }, canOk);
            this.CancelCommand = ReactiveCommandCreator.Create(() => { this.DialogResult = new BaseDialogResult(false); });
        }

        /// <summary>
        /// Gets the stage gates being edited, one row per gate, in project order.
        /// </summary>
        public ReactiveList<StageGateRowViewModel> StageGates { get; } = new ReactiveList<StageGateRowViewModel>();

        /// <summary>
        /// Gets or sets the gate selected in the table.
        /// </summary>
        public StageGateRowViewModel SelectedGate
        {
            get => this.selectedGate;
            set => this.RaiseAndSetIfChanged(ref this.selectedGate, value);
        }

        /// <summary>
        /// Gets the command that appends a gate and selects it.
        /// </summary>
        public ReactiveCommand<Unit, Unit> AddGateCommand { get; }

        /// <summary>
        /// Gets the command that removes the selected gate.
        /// </summary>
        public ReactiveCommand<Unit, Unit> RemoveGateCommand { get; }

        /// <summary>
        /// Gets the command that moves the selected gate one place earlier.
        /// </summary>
        public ReactiveCommand<Unit, Unit> MoveGateUpCommand { get; }

        /// <summary>
        /// Gets the command that moves the selected gate one place later.
        /// </summary>
        public ReactiveCommand<Unit, Unit> MoveGateDownCommand { get; }

        /// <summary>
        /// Gets the command that accepts the list.
        /// </summary>
        public ReactiveCommand<Unit, Unit> OkCommand { get; }

        /// <summary>
        /// Gets the command that cancels the whole set-up.
        /// </summary>
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }

        /// <summary>
        /// Reads the table into the stage gate list: trimmed, blanks dropped, duplicates removed while keeping the
        /// order the user arranged.
        /// </summary>
        /// <returns>The stage gates, in order.</returns>
        public IReadOnlyList<string> ParseStageGates()
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var gates = new List<string>();

            foreach (var row in this.StageGates)
            {
                var gate = (row.Name ?? string.Empty).Trim();

                if (gate.Length == 0)
                {
                    continue;
                }

                // deduplication is on the derived short-name, not on the typed text: the seeder maps both spaces and
                // hyphens to an underscore, so "In Service" and "In-Service" are two names for one
                // EnumerationValueDefinition and seeding both would leave the RDL with colliding short-names
                if (seen.Add(VandVRdlManifest.ToShortName(gate)))
                {
                    gates.Add(gate);
                }
            }

            return gates;
        }

        /// <summary>
        /// Appends a gate and selects it, so the user can start typing straight away.
        /// </summary>
        private void ExecuteAddGate()
        {
            var gate = new StageGateRowViewModel();

            this.StageGates.Add(gate);
            this.SelectedGate = gate;
        }

        /// <summary>
        /// Removes the selected gate.
        /// </summary>
        private void ExecuteRemoveGate()
        {
            var gate = this.SelectedGate;

            if (gate == null)
            {
                return;
            }

            this.StageGates.Remove(gate);
            this.SelectedGate = null;
        }

        /// <summary>
        /// Moves the selected gate by the supplied offset, keeping the selection on it.
        /// </summary>
        /// <param name="offset">Minus one to move earlier, plus one to move later.</param>
        private void MoveGate(int offset)
        {
            var gate = this.SelectedGate;

            if (gate == null)
            {
                return;
            }

            var index = this.StageGates.IndexOf(gate);
            var target = index + offset;

            if (index < 0 || target < 0 || target >= this.StageGates.Count)
            {
                return;
            }

            this.StageGates.Move(index, target);
            this.SelectedGate = gate;
        }
    }

    /// <summary>
    /// One editable stage gate row.
    /// </summary>
    public class StageGateRowViewModel : ReactiveObject
    {
        /// <summary>
        /// Backing field for <see cref="Name"/>
        /// </summary>
        private string name;

        /// <summary>
        /// Gets or sets the gate name, e.g. <c>CDR</c>.
        /// </summary>
        public string Name
        {
            get => this.name;
            set => this.RaiseAndSetIfChanged(ref this.name, value);
        }
    }
}
