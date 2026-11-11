// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVAnnotationQuery.cs" company="Starion Group S.A.">
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

    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;

    using CDP4Composition.Annotations;

    /// <summary>
    /// Resolves the traceability of the formal concessions raised in a model, that is, which V&amp;V item and which
    /// requirement each Review Item Discrepancy, Request for Deviation and Request for Waiver concerns, so the NCR
    /// sheet can lead with traceability instead of making the reader chase references.
    /// </summary>
    /// <remarks>
    /// Only the V&amp;V traceability lives here; everything that is true of an annotation regardless of V&amp;V, the
    /// kind table, the per-thing lookup and the open / closed semantics, lives in
    /// <see cref="AnnotationQuery"/> so that every browser shares it.
    /// </remarks>
    public static class VandVAnnotationQuery
    {
        /// <summary>
        /// Returns every concession in the iteration's model, with the V&amp;V item and requirement it concerns
        /// already resolved.
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/>.</param>
        /// <returns>The concessions, ordered by short-name.</returns>
        public static IReadOnlyList<VandVAnnotationTrace> Query(Iteration iteration)
        {
            if (!(iteration.Container is EngineeringModel model))
            {
                return new List<VandVAnnotationTrace>();
            }

            return model.ModellingAnnotation
                .Where(AnnotationQuery.IsReviewRequest)
                .Select(annotation => Describe(iteration, annotation))
                .OrderBy(trace => trace.Annotation.ShortName)
                .ToList();
        }

        /// <summary>
        /// Resolves the V&amp;V item and requirement a concession concerns. One raised on a V&amp;V item also names
        /// the requirement that item verifies; one raised on a requirement leaves the V&amp;V item empty.
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/>.</param>
        /// <param name="annotation">The concession.</param>
        /// <returns>The described concession.</returns>
        private static VandVAnnotationTrace Describe(Iteration iteration, ModellingAnnotationItem annotation)
        {
            var annotated = annotation.PrimaryAnnotatedThing?.ReferencedThing
                            ?? annotation.RelatedThing.FirstOrDefault()?.ReferencedThing;

            var requirement = annotated as Requirement;

            if (requirement != null && VandVCoverageQuery.IsVnVItem(requirement))
            {
                var covered = VandVItemCreator.QueryCoveringRelationship(iteration, requirement)?.Target as Requirement;

                return new VandVAnnotationTrace(annotation, requirement, covered);
            }

            return new VandVAnnotationTrace(annotation, null, requirement);
        }
    }
}
