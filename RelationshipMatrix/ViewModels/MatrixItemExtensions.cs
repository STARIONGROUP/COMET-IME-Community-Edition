// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MatrixItemExtensions.cs" company="Starion Group S.A.">
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

namespace CDP4RelationshipMatrix.ViewModels
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// Extension methods that resolve the name and short-name of a <see cref="Thing"/> displayed in the relationship matrix.
    /// </summary>
    /// <remarks>
    /// The matrix historically only handled <see cref="DefinedThing"/>s (which carry a <c>Name</c>/<c>ShortName</c>). Kinds such
    /// as <see cref="File"/> are not <see cref="DefinedThing"/>s and keep their name on the current <see cref="FileRevision"/>,
    /// so both the name and the short-name of a <see cref="File"/> resolve to that revision's name. See GitHub issue #1490.
    /// </remarks>
    public static class MatrixItemExtensions
    {
        /// <summary>
        /// Queries the name to display for the <paramref name="thing"/> in the matrix.
        /// </summary>
        /// <param name="thing">The <see cref="Thing"/> shown in a row or column.</param>
        /// <returns>The name of the <paramref name="thing"/>.</returns>
        public static string QueryDisplayName(this Thing thing)
        {
            switch (thing)
            {
                case File file:
                    return file.CurrentFileRevision?.Name;
                case DefinedThing definedThing:
                    return definedThing.Name;
                default:
                    return thing?.UserFriendlyName;
            }
        }

        /// <summary>
        /// Queries the short-name to display (and to use as the grid field-name) for the <paramref name="thing"/> in the matrix.
        /// </summary>
        /// <param name="thing">The <see cref="Thing"/> shown in a row or column.</param>
        /// <returns>The short-name of the <paramref name="thing"/>; for a <see cref="File"/> this is its current revision name.</returns>
        public static string QueryDisplayShortName(this Thing thing)
        {
            switch (thing)
            {
                case File file:
                    return file.CurrentFileRevision?.Name;
                case DefinedThing definedThing:
                    return definedThing.ShortName;
                default:
                    return thing?.UserFriendlyShortName;
            }
        }
    }
}
