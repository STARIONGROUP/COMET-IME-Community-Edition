// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVInheritanceNote.cs" company="Starion Group S.A.">
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

    /// <summary>
    /// The wording and the completeness test for a planning attribute a V&amp;V item can either state itself or inherit
    /// from the shared activity that performs it. Lifted out of the item dialog, which was carrying too much, and made
    /// static so the register, the rules and any future consumer read the inheritance the same way.
    /// </summary>
    public static class VandVInheritanceNote
    {
        /// <summary>
        /// Builds the line shown under a picker that a shared activity can supply: what the activity states, and
        /// whether the value typed on the item is overriding it. Saying so matters, because the item silently winning
        /// over its activity is exactly the sort of thing that is discovered far too late.
        /// </summary>
        /// <param name="activity">The activity performing the item, or null when none does.</param>
        /// <param name="label">The human name of the attribute, e.g. "stage gate".</param>
        /// <param name="ownValue">The value currently entered on the item.</param>
        /// <param name="parameterTypeShortName">The parameter type short-name to read off the activity.</param>
        /// <returns>The note, or an empty string when no activity performs the item.</returns>
        public static string Describe(Requirement activity, string label, string ownValue, string parameterTypeShortName)
        {
            if (activity == null)
            {
                return string.Empty;
            }

            var activityValue = VandVCoverageQuery.Attribute(activity, parameterTypeShortName);

            if (string.IsNullOrWhiteSpace(activityValue))
            {
                return $"{activity.ShortName} states no {label}, so this item has to.";
            }

            if (string.IsNullOrWhiteSpace(ownValue))
            {
                return $"Inherited from {activity.ShortName}: {activityValue}. Leave empty to follow the activity.";
            }

            return VandVCoverageQuery.AreSameEnumValue(ownValue, activityValue)
                ? $"Same as {activity.ShortName}. Clear it to simply follow the activity."
                : $"Overrides {activity.ShortName}, which states {activityValue}. This item keeps '{ownValue}'.";
        }

        /// <summary>
        /// Asserts whether a planning attribute is stated either on the item or on the activity performing it.
        /// </summary>
        /// <param name="activity">The activity performing the item, or null when none does.</param>
        /// <param name="ownValue">The value typed on the item.</param>
        /// <param name="parameterTypeShortName">The parameter type short-name to read off the activity.</param>
        /// <returns>true when either the item or its activity supplies the attribute.</returns>
        public static bool IsSuppliedByItemOrActivity(Requirement activity, string ownValue, string parameterTypeShortName)
        {
            if (!string.IsNullOrWhiteSpace(ownValue))
            {
                return true;
            }

            return activity != null && !string.IsNullOrWhiteSpace(VandVCoverageQuery.Attribute(activity, parameterTypeShortName));
        }
    }
}
