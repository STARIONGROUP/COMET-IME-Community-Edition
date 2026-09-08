// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TraceabilityDetailsBuilder.cs" company="Starion Group S.A.">
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

namespace CDP4Grapher.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// Builds the copy-pasteable details text of a <see cref="Thing"/> shown on the Details tab of the traceability
    /// diagram
    /// </summary>
    public static class TraceabilityDetailsBuilder
    {
        /// <summary>
        /// Builds the details of a <see cref="Thing"/>: its kind, name, short name, owner, categories, the endpoints of
        /// a relationship and its definition, one labelled value per line
        /// </summary>
        /// <param name="thing">The selected <see cref="Thing"/>, or null</param>
        /// <returns>The details text, or an empty string when nothing is selected</returns>
        public static string Build(Thing thing)
        {
            if (thing == null)
            {
                return string.Empty;
            }

            var lines = new List<string> { $"Kind: {thing.ClassKind}" };

            if (thing is INamedThing namedThing)
            {
                lines.Add($"Name: {namedThing.Name}");
            }

            if (thing is IShortNamedThing shortNamedThing)
            {
                lines.Add($"Short name: {shortNamedThing.ShortName}");
            }

            if (thing is IOwnedThing { Owner: not null } ownedThing)
            {
                lines.Add($"Owner: {ownedThing.Owner.ShortName}");
            }

            if (thing is ICategorizableThing categorizableThing && categorizableThing.Category.Any())
            {
                lines.Add($"Categories: {string.Join(", ", categorizableThing.Category.Select(x => x.Name))}");
            }

            if (thing is BinaryRelationship binaryRelationship)
            {
                lines.Add($"Source: {binaryRelationship.Source?.UserFriendlyName}");
                lines.Add($"Target: {binaryRelationship.Target?.UserFriendlyName}");
            }

            if (thing is MultiRelationship multiRelationship)
            {
                lines.Add($"Related: {string.Join(", ", multiRelationship.RelatedThing.Select(x => x.UserFriendlyName))}");
            }

            var definition = (thing as DefinedThing)?.Definition.FirstOrDefault()?.Content;

            if (!string.IsNullOrWhiteSpace(definition))
            {
                lines.Add($"Definition: {definition}");
            }

            return string.Join(Environment.NewLine, lines);
        }
    }
}
