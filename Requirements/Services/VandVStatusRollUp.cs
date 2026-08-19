// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVStatusRollUp.cs" company="Starion Group S.A.">
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
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// How many V&amp;V items in a part of the tree are closed out, failed and still open, the roll-up shown in the
    /// browser's Coverage column so a specification tells you whether you are verified, not merely how many activities
    /// somebody wrote down.
    /// </summary>
    public class VandVStatusRollUp
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VandVStatusRollUp"/> class.
        /// </summary>
        /// <param name="passed">The number of items closed out positively.</param>
        /// <param name="failed">The number of failed items.</param>
        /// <param name="open">The number of items neither passed nor failed.</param>
        public VandVStatusRollUp(int passed, int failed, int open)
        {
            this.Passed = passed;
            this.Failed = failed;
            this.Open = open;
        }

        /// <summary>
        /// Gets the number of items closed out positively (passed, waived, deviated or not applicable).
        /// </summary>
        public int Passed { get; }

        /// <summary>
        /// Gets the number of failed items.
        /// </summary>
        public int Failed { get; }

        /// <summary>
        /// Gets the number of items that are neither passed nor failed.
        /// </summary>
        public int Open { get; }

        /// <summary>
        /// Gets the total number of items rolled up.
        /// </summary>
        public int Total => this.Passed + this.Failed + this.Open;

        /// <summary>
        /// Adds two roll-ups together, so a parent row can accumulate its children.
        /// </summary>
        /// <param name="rollUps">The roll-ups to add.</param>
        /// <returns>The combined roll-up.</returns>
        public static VandVStatusRollUp Sum(IEnumerable<VandVStatusRollUp> rollUps)
        {
            var all = rollUps.ToList();

            return new VandVStatusRollUp(all.Sum(x => x.Passed), all.Sum(x => x.Failed), all.Sum(x => x.Open));
        }

        /// <summary>
        /// Renders the roll-up for a single requirement: how many activities cover it and where they stand.
        /// </summary>
        /// <returns>The display text.</returns>
        public string ToRequirementSummary()
        {
            if (this.Total == 0)
            {
                return "Not covered";
            }

            return $"{this.Total} item(s): {this.Describe()}";
        }

        /// <summary>
        /// Renders the roll-up for a specification or group: how many of its requirements are fully verified.
        /// </summary>
        /// <param name="verifiedRequirements">The number of requirements whose items have all closed out.</param>
        /// <param name="totalRequirements">The number of requirements below this row.</param>
        /// <returns>The display text.</returns>
        public string ToContainerSummary(int verifiedRequirements, int totalRequirements)
        {
            if (totalRequirements == 0)
            {
                return string.Empty;
            }

            var summary = $"{verifiedRequirements}/{totalRequirements} verified";

            return this.Failed == 0 ? summary : $"{summary}, {this.Failed} failed";
        }

        /// <summary>
        /// Renders the non-zero buckets, e.g. "2 passed, 1 open".
        /// </summary>
        /// <returns>The description.</returns>
        private string Describe()
        {
            var parts = new List<string>();

            if (this.Passed > 0)
            {
                parts.Add($"{this.Passed} passed");
            }

            if (this.Failed > 0)
            {
                parts.Add($"{this.Failed} failed");
            }

            if (this.Open > 0)
            {
                parts.Add($"{this.Open} open");
            }

            return string.Join(", ", parts);
        }
    }
}
