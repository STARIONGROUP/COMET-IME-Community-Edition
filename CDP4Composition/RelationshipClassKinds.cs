// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RelationshipClassKinds.cs" company="Starion Group S.A.">
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

namespace CDP4Composition
{
    using System.Collections.Generic;

    using CDP4Common.CommonData;

    /// <summary>
    /// The curated sets of <see cref="ClassKind"/>s that the plugin pickers offer
    /// </summary>
    public static class RelationshipClassKinds
    {
        /// <summary>
        /// The <see cref="ClassKind"/>s that the relationship oriented plugins offer as a source or a root, instead of
        /// every categorizable <see cref="ClassKind"/> of the model. Shared so that the Relationship Matrix and the
        /// relationship traceability diagram cannot drift apart.
        /// </summary>
        public static readonly IReadOnlyList<ClassKind> Default = new List<ClassKind>
        {
            ClassKind.ElementDefinition,
            ClassKind.ElementUsage,
            ClassKind.NestedElement,
            ClassKind.Option,
            ClassKind.Parameter,
            ClassKind.ParametricConstraint,
            ClassKind.RequirementsSpecification,
            ClassKind.RequirementsGroup,
            ClassKind.Requirement
        };
    }
}
