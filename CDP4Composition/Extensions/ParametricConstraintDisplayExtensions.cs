// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParametricConstraintDisplayExtensions.cs" company="Starion Group S.A.">
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

namespace CDP4Composition.Extensions
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Extensions;

    /// <summary>
    /// Extension methods for <see cref="ParametricConstraint"/> that produce a human-readable display string. A
    /// <see cref="ParametricConstraint"/> has no name or short-name (its <see cref="CDP4Common.CommonData.Thing.UserFriendlyName"/>
    /// and <see cref="CDP4Common.CommonData.Thing.UserFriendlyShortName"/> fall back to the SDK "not implemented" text), so its tree
    /// of <see cref="BooleanExpression"/>s is shown instead, prefixed with the short-name of the owning <see cref="Requirement"/>
    /// to disambiguate similar expressions.
    /// </summary>
    public static class ParametricConstraintDisplayExtensions
    {
        /// <summary>
        /// Gets a human-readable display string for the <paramref name="parametricConstraint"/>.
        /// </summary>
        /// <param name="parametricConstraint">The <see cref="ParametricConstraint"/> to represent.</param>
        /// <returns>
        /// The expression tree of the <see cref="ParametricConstraint"/> (or its <see cref="CDP4Common.CommonData.ClassKind"/> when
        /// it has no expressions), prefixed with the short-name of the owning <see cref="Requirement"/> when available.
        /// </returns>
        public static string GetDisplayName(this ParametricConstraint parametricConstraint)
        {
            if (parametricConstraint == null)
            {
                return string.Empty;
            }

            var expression = parametricConstraint.ToExpressionString();

            var display = string.IsNullOrWhiteSpace(expression)
                ? parametricConstraint.ClassKind.ToString()
                : expression;

            if (parametricConstraint.Container is Requirement requirement && !string.IsNullOrWhiteSpace(requirement.ShortName))
            {
                display = $"{requirement.ShortName}: {display}";
            }

            return display;
        }
    }
}
