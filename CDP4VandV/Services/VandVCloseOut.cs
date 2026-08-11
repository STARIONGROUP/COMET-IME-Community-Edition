// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVCloseOut.cs" company="Starion Group S.A.">
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
    using System;

    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// The close-out vocabulary of the V&amp;V register, kept in one place so the browser, the roll-up, the built-in
    /// rules and the workbook cannot drift apart on what "compliant" or "closed" means.
    /// </summary>
    /// <remarks>
    /// ECSS-E-ST-10-02 Annex B treats three things as distinct, and so does this plugin:
    /// the <b>execution status</b> (<c>vnv_status</c>: did the activity happen and what did it show),
    /// the <b>compliance status</b> (<c>vnv_compliance</c>: does the design meet the requirement),
    /// and the <b>close-out status</b> (<c>vnv_closed</c>: has the customer accepted it, and why).
    /// </remarks>
    public static class VandVCloseOut
    {
        /// <summary>
        /// The parameter type short-name of the compliance status.
        /// </summary>
        public const string ComplianceShortName = "vnv_compliance";

        /// <summary>
        /// The parameter type short-name of the close-out flag.
        /// </summary>
        public const string ClosedShortName = "vnv_closed";

        /// <summary>
        /// The parameter type short-name of the close-out reason.
        /// </summary>
        public const string CloseOutReasonShortName = "vnv_closeout_reason";

        /// <summary>
        /// The parameter type short-name of the person who closed the item out.
        /// </summary>
        public const string ClosedByShortName = "vnv_closed_by";

        /// <summary>
        /// The parameter type short-name of the close-out date.
        /// </summary>
        public const string ClosedOnShortName = "vnv_closed_on";

        /// <summary>
        /// The parameter type short-name of the verification plan reference.
        /// </summary>
        public const string PlanReferenceShortName = "vnv_plan_ref";

        /// <summary>
        /// The compliance status of an item whose compliance has not been judged yet.
        /// </summary>
        public const string NotAssessed = "Not Assessed";

        /// <summary>
        /// Gets the selectable compliance statuses, in the order they are offered.
        /// </summary>
        public static string[] PossibleCompliances { get; } =
        {
            NotAssessed,
            "Compliant",
            "Partially Compliant",
            "Non-Compliant",
            "Not Applicable"
        };

        /// <summary>
        /// Reads the close-out flag of a V&amp;V item.
        /// </summary>
        /// <param name="item">The V&amp;V item.</param>
        /// <returns>true when the item has been closed out.</returns>
        public static bool IsClosed(Requirement item)
        {
            return IsTrue(VandVCoverageQuery.Attribute(item, ClosedShortName));
        }

        /// <summary>
        /// Reads the compliance status of a V&amp;V item.
        /// </summary>
        /// <param name="item">The V&amp;V item.</param>
        /// <returns>The compliance status, or <see cref="NotAssessed"/> when it carries none.</returns>
        public static string QueryCompliance(Requirement item)
        {
            var compliance = VandVCoverageQuery.Attribute(item, ComplianceShortName);

            return string.IsNullOrWhiteSpace(compliance) ? NotAssessed : compliance;
        }

        /// <summary>
        /// Asserts whether a compliance status blocks close-out. Non-compliant and partially compliant items may only
        /// be closed out through a waiver or deviation, which the rule checks for separately.
        /// </summary>
        /// <param name="compliance">The compliance status.</param>
        /// <returns>true when the status is a shortfall against the requirement.</returns>
        public static bool IsShortfall(string compliance)
        {
            return VandVCoverageQuery.AreSameEnumValue(compliance, "Non-Compliant")
                   || VandVCoverageQuery.AreSameEnumValue(compliance, "Partially Compliant");
        }

        /// <summary>
        /// Parses a boolean attribute value. Values are stored as text, and the stock parameter-value editor and this
        /// plugin's dialog do not agree on casing, so the comparison is deliberately loose.
        /// </summary>
        /// <param name="value">The stored value.</param>
        /// <returns>true when the value denotes true.</returns>
        public static bool IsTrue(string value)
        {
            return !string.IsNullOrWhiteSpace(value)
                   && (value.Equals("true", StringComparison.OrdinalIgnoreCase)
                       || value.Equals("yes", StringComparison.OrdinalIgnoreCase)
                       || value == "1"
                       || value == "-1");
        }
    }
}
