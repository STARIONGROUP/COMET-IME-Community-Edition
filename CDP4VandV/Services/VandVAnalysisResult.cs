// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVAnalysisResult.cs" company="Starion Group S.A.">
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

namespace CDP4VandV.Services
{
    /// <summary>
    /// The outcome of evaluating a V&amp;V item's covered parameter against the requirement's parametric constraints.
    /// </summary>
    public class VandVAnalysisResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VandVAnalysisResult"/> class.
        /// </summary>
        /// <param name="state">The outcome.</param>
        /// <param name="detail">The human-readable explanation.</param>
        private VandVAnalysisResult(VandVAnalysisState state, string detail)
        {
            this.State = state;
            this.Detail = detail;
        }

        /// <summary>
        /// Gets the outcome.
        /// </summary>
        public VandVAnalysisState State { get; }

        /// <summary>
        /// Gets the explanation, naming the parameter, the slice and the limit that was missed.
        /// </summary>
        public string Detail { get; }

        /// <summary>
        /// Gets the text shown in the browser's Analysis Check column and in the VCD.
        /// </summary>
        /// <remarks>
        /// Every state says something. An empty cell reads as "checked and fine", which is exactly wrong for an item
        /// that was never checked at all, so the not-applicable state reports why it was skipped instead.
        /// </remarks>
        public string Display
        {
            get
            {
                switch (this.State)
                {
                    case VandVAnalysisState.Satisfied:
                        return "Meets constraint: " + this.Detail;
                    case VandVAnalysisState.Violated:
                        return "VIOLATED: " + this.Detail;
                    default:
                        return "Not checked: " + this.Detail;
                }
            }
        }

        /// <summary>
        /// Builds a result for an item that cannot be checked automatically.
        /// </summary>
        /// <param name="reason">Why it cannot be checked.</param>
        /// <returns>The result.</returns>
        public static VandVAnalysisResult NotApplicable(string reason)
        {
            return new VandVAnalysisResult(VandVAnalysisState.NotApplicable, reason);
        }

        /// <summary>
        /// Builds a result for an item whose parameter meets every constraint.
        /// </summary>
        /// <param name="detail">The explanation.</param>
        /// <returns>The result.</returns>
        public static VandVAnalysisResult Satisfied(string detail)
        {
            return new VandVAnalysisResult(VandVAnalysisState.Satisfied, detail);
        }

        /// <summary>
        /// Builds a result for an item whose parameter breaks a constraint.
        /// </summary>
        /// <param name="detail">The explanation.</param>
        /// <returns>The result.</returns>
        public static VandVAnalysisResult Violated(string detail)
        {
            return new VandVAnalysisResult(VandVAnalysisState.Violated, detail);
        }
    }

    /// <summary>
    /// The outcome of an automatic analysis check.
    /// </summary>
    public enum VandVAnalysisState
    {
        /// <summary>
        /// The item cannot be checked automatically, for instance because it covers no parameter.
        /// </summary>
        NotApplicable,

        /// <summary>
        /// Every constraint on the covered parameter holds against its current value.
        /// </summary>
        Satisfied,

        /// <summary>
        /// At least one constraint is broken by the covered parameter's current value.
        /// </summary>
        Violated
    }
}
