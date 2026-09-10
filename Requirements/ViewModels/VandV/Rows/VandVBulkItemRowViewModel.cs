// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVBulkItemRowViewModel.cs" company="Starion Group S.A.">
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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;

    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Mvvm;

    using ReactiveUI;

    /// <summary>
    /// One requirement offered for bulk V&amp;V item creation: whether an item is wanted for it, and the acceptance
    /// criteria that item will carry.
    /// </summary>
    /// <remarks>
    /// Acceptance criteria are per requirement by nature, so they cannot be a single value shared by the whole batch.
    /// The dialog's own box supplies a default and this row overrides it where the requirement is judged against
    /// something else. Where the requirement already states a threshold in a <see cref="ParametricConstraint"/> that
    /// constraint is offered in a picker, exactly as the V&amp;V item dialog offers it: chosen and applied on demand,
    /// never filled in on the user's behalf.
    /// </remarks>
    public class VandVBulkItemRowViewModel : SelectableThingRowViewModel
    {
        /// <summary>
        /// Backing field for <see cref="AcceptanceCriteria"/>
        /// </summary>
        private string acceptanceCriteria;

        /// <summary>
        /// Backing field for <see cref="SelectedParametricConstraint"/>
        /// </summary>
        private ConstraintChoiceRowViewModel selectedParametricConstraint;

        /// <summary>
        /// Initializes a new instance of the <see cref="VandVBulkItemRowViewModel"/> class.
        /// </summary>
        /// <param name="requirement">The <see cref="Requirement"/> being offered.</param>
        /// <param name="display">The text shown next to the tick box.</param>
        public VandVBulkItemRowViewModel(Requirement requirement, string display)
            : base(requirement, display)
        {
            this.Requirement = requirement;
            this.PossibleParametricConstraints = ConstraintChoiceRowViewModel.Build(requirement);
            this.selectedParametricConstraint = this.PossibleParametricConstraints.FirstOrDefault();

            this.UseParametricConstraintCommand = ReactiveCommandCreator.Create(
                this.ExecuteUseParametricConstraint,
                this.WhenAnyValue(x => x.SelectedParametricConstraint).Select(choice => choice != null));
        }

        /// <summary>
        /// Gets the <see cref="Requirement"/> this row represents.
        /// </summary>
        public Requirement Requirement { get; }

        /// <summary>
        /// Gets the parametric constraints the requirement states, offered as acceptance criteria.
        /// </summary>
        public IReadOnlyList<ConstraintChoiceRowViewModel> PossibleParametricConstraints { get; }

        /// <summary>
        /// Gets a value indicating whether the requirement states any parametric constraint at all. The picker is
        /// hidden for the requirements that state none.
        /// </summary>
        public bool HasParametricConstraints => this.PossibleParametricConstraints.Any();

        /// <summary>
        /// Gets or sets the acceptance criteria for this requirement's item. Blank means the dialog's default applies.
        /// </summary>
        public string AcceptanceCriteria
        {
            get => this.acceptanceCriteria;
            set => this.RaiseAndSetIfChanged(ref this.acceptanceCriteria, value);
        }

        /// <summary>
        /// Gets or sets the constraint <see cref="UseParametricConstraintCommand"/> will copy in.
        /// </summary>
        public ConstraintChoiceRowViewModel SelectedParametricConstraint
        {
            get => this.selectedParametricConstraint;
            set => this.RaiseAndSetIfChanged(ref this.selectedParametricConstraint, value);
        }

        /// <summary>
        /// Gets the command that copies the selected parametric constraint's expression into the acceptance criteria.
        /// </summary>
        public ReactiveCommand<Unit, Unit> UseParametricConstraintCommand { get; }

        /// <summary>
        /// Copies the selected constraint's expression into the acceptance criteria, appending it when text is already
        /// present so nothing the user wrote is lost.
        /// </summary>
        private void ExecuteUseParametricConstraint()
        {
            var expression = this.SelectedParametricConstraint?.QueryExpressionText();

            if (string.IsNullOrWhiteSpace(expression))
            {
                return;
            }

            this.AcceptanceCriteria = string.IsNullOrWhiteSpace(this.AcceptanceCriteria)
                ? expression
                : $"{this.AcceptanceCriteria}{Environment.NewLine}{expression}";
        }
    }
}
