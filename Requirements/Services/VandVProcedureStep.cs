// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVProcedureStep.cs" company="Starion Group S.A.">
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

namespace CDP4Requirements.Services
{
    using CDP4Common.EngineeringModelData;

    using ReactiveUI;

    /// <summary>
    /// One editable row of a verification procedure: what to do, what is expected, and what was actually observed.
    /// </summary>
    /// <remarks>
    /// Deliberately not a <c>RowViewModelBase</c>. The dialog edits the procedure inside a transaction and only
    /// writes it on OK, so the rows must be free-standing values that survive being reordered and removed without
    /// touching the model. <see cref="Thing"/> is null for a step the user has just added.
    /// </remarks>
    public class VandVProcedureStep : ReactiveObject
    {
        /// <summary>
        /// Backing field for <see cref="Number"/>
        /// </summary>
        private int number;

        /// <summary>
        /// Backing field for <see cref="Action"/>
        /// </summary>
        private string action;

        /// <summary>
        /// Backing field for <see cref="ExpectedResult"/>
        /// </summary>
        private string expectedResult;

        /// <summary>
        /// Backing field for <see cref="ActualResult"/>
        /// </summary>
        private string actualResult;

        /// <summary>
        /// Backing field for <see cref="Result"/>
        /// </summary>
        private string result;

        /// <summary>
        /// Initializes a new instance of the <see cref="VandVProcedureStep"/> class for a step the user is adding.
        /// </summary>
        public VandVProcedureStep()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VandVProcedureStep"/> class from a stored step.
        /// </summary>
        /// <param name="step">The stored step <see cref="Requirement"/>.</param>
        public VandVProcedureStep(Requirement step)
        {
            this.Thing = step;
            this.number = VandVProcedureWriter.QueryStepNumber(step);
            this.action = VandVCoverageQuery.Attribute(step, "vnv_step_action");
            this.expectedResult = VandVCoverageQuery.Attribute(step, "vnv_step_expected");
            this.actualResult = VandVCoverageQuery.Attribute(step, "vnv_step_actual");
            this.result = VandVCoverageQuery.Attribute(step, "vnv_step_result");
        }

        /// <summary>
        /// Gets the stored step, or null when the user has just added this row.
        /// </summary>
        public Requirement Thing { get; }

        /// <summary>
        /// Gets or sets the step number. Renumbered from the row order when the procedure is written.
        /// </summary>
        public int Number
        {
            get => this.number;
            set => this.RaiseAndSetIfChanged(ref this.number, value);
        }

        /// <summary>
        /// Gets or sets what the operator must do.
        /// </summary>
        public string Action
        {
            get => this.action;
            set => this.RaiseAndSetIfChanged(ref this.action, value);
        }

        /// <summary>
        /// Gets or sets what should be observed if the step passes.
        /// </summary>
        public string ExpectedResult
        {
            get => this.expectedResult;
            set => this.RaiseAndSetIfChanged(ref this.expectedResult, value);
        }

        /// <summary>
        /// Gets or sets what was actually observed, the as-run record.
        /// </summary>
        public string ActualResult
        {
            get => this.actualResult;
            set => this.RaiseAndSetIfChanged(ref this.actualResult, value);
        }

        /// <summary>
        /// Gets or sets the step outcome.
        /// </summary>
        public string Result
        {
            get => this.result;
            set => this.RaiseAndSetIfChanged(ref this.result, value);
        }
    }
}
