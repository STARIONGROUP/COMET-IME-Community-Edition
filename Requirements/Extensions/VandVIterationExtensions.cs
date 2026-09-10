// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVIterationExtensions.cs" company="Starion Group S.A.">
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

namespace CDP4Requirements.Extensions
{
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// Extension methods on <see cref="Iteration"/> shared by the V&amp;V capability.
    /// </summary>
    public static class VandVIterationExtensions
    {
        /// <summary>
        /// Returns every non-deprecated <see cref="Requirement"/> in the iteration, from the non-deprecated
        /// specifications only.
        /// </summary>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> whose requirements are enumerated.
        /// </param>
        /// <returns>
        /// The live requirements, skipping any that are deprecated or that live in a deprecated specification.
        /// </returns>
        /// <remarks>
        /// The V&amp;V services and rules repeatedly need "the requirements that still count": the pair of deprecation
        /// filters used to be written out at every call site, so a change to what "live" means (a spec kind to exclude,
        /// a second deprecation flag) had to be found in half a dozen places. This is deliberately not filtered by
        /// short-name or category, so a caller that needs the V&amp;V specification excluded or only the activities adds
        /// its own <c>Where</c> on top.
        /// </remarks>
        public static IEnumerable<Requirement> QueryNonDeprecatedRequirements(this Iteration iteration)
        {
            return iteration.RequirementsSpecification
                .Where(specification => !specification.IsDeprecated)
                .SelectMany(specification => specification.Requirement)
                .Where(requirement => !requirement.IsDeprecated);
        }
    }
}
