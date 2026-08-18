// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AnnotationQuery.cs" company="Starion Group S.A.">
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

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;

    /// <summary>
    /// Queries the review requests, Review Item Discrepancies, Requests for Deviation and Requests for Waiver, raised
    /// against a model's V&amp;V items and requirements. Shared by the browser (row state and context menu), the icon
    /// selector and the workbook exporter so all three agree on what "open" means.
    /// </summary>
    public static class AnnotationQuery
    {
        /// <summary>
        /// The <see cref="AnnotationStatusKind"/>s that mean the request no longer needs action.
        /// </summary>
        private static readonly AnnotationStatusKind[] ClosedStatuses =
        {
            AnnotationStatusKind.CLOSED,
            AnnotationStatusKind.DONE,
            AnnotationStatusKind.INVALID,
            AnnotationStatusKind.NOT_APPLICABLE,
            AnnotationStatusKind.WONTFIX
        };

        /// <summary>
        /// Returns every review request in the iteration's model, with the V&amp;V item and requirement it concerns
        /// already resolved.
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/>.</param>
        /// <returns>The review requests, ordered by short-name.</returns>
        public static IReadOnlyList<VandVAnnotationTrace> Query(Iteration iteration)
        {
            var model = iteration.Container as EngineeringModel;

            if (model == null)
            {
                return new List<VandVAnnotationTrace>();
            }

            return model.ModellingAnnotation
                .Where(IsReviewRequest)
                .Select(annotation => Describe(iteration, annotation))
                .OrderBy(x => x.Annotation.ShortName)
                .ToList();
        }

        /// <summary>
        /// Returns the review requests raised against a single <see cref="Thing"/>.
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/>.</param>
        /// <param name="thing">The annotated <see cref="Thing"/>.</param>
        /// <returns>The review requests, ordered by short-name.</returns>
        public static IReadOnlyList<ModellingAnnotationItem> QueryFor(Iteration iteration, Thing thing)
        {
            var model = iteration.Container as EngineeringModel;

            if (model == null || thing == null)
            {
                return new List<ModellingAnnotationItem>();
            }

            return model.ModellingAnnotation
                .Where(annotation =>
                    IsReviewRequest(annotation)
                    && annotation.RelatedThing.Any(reference => reference.ReferencedThing == thing))
                .OrderBy(annotation => annotation.ShortName)
                .ToList();
        }

        /// <summary>
        /// Returns every annotation raised against a single <see cref="Thing"/>, of any kind, including the change
        /// requests and model notes that <see cref="QueryFor"/> filters out.
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/>.</param>
        /// <param name="thing">The annotated <see cref="Thing"/>.</param>
        /// <returns>The annotations, ordered by short-name.</returns>
        /// <remarks>
        /// The context menu lists these, so anything the register can create can also be found and worked afterwards.
        /// Model notes live in <see cref="EngineeringModel.GenericNote"/> rather than
        /// <see cref="EngineeringModel.ModellingAnnotation"/>, which is why both lists are searched.
        /// </remarks>
        public static IReadOnlyList<EngineeringModelDataAnnotation> QueryAllFor(Iteration iteration, Thing thing)
        {
            var model = iteration.Container as EngineeringModel;

            if (model == null || thing == null)
            {
                return new List<EngineeringModelDataAnnotation>();
            }

            return model.ModellingAnnotation
                .Cast<EngineeringModelDataAnnotation>()
                .Concat(model.GenericNote)
                .Where(annotation => annotation.RelatedThing.Any(reference => reference.ReferencedThing == thing))
                .OrderBy(annotation => annotation.UserFriendlyShortName)
                .ToList();
        }

        /// <summary>
        /// Asserts whether an annotation still needs action. A kind that carries no status, such as a model note,
        /// never needs action.
        /// </summary>
        /// <param name="annotation">The annotation.</param>
        /// <returns>true when the annotation is not closed out.</returns>
        public static bool IsOpen(EngineeringModelDataAnnotation annotation)
        {
            return annotation is ModellingAnnotationItem modellingAnnotation && !ClosedStatuses.Contains(modellingAnnotation.Status);
        }

        /// <summary>
        /// Describes the status of an annotation for display, or an empty string for a kind that has none.
        /// </summary>
        /// <param name="annotation">The annotation.</param>
        /// <returns>The status text.</returns>
        public static string DescribeStatus(EngineeringModelDataAnnotation annotation)
        {
            return (annotation as ModellingAnnotationItem)?.Status.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Summarises the review requests raised against a V&amp;V item as a single state, so a row can show at a
        /// glance whether it is clean, blocked or resolved.
        /// </summary>
        /// <param name="annotations">The requests raised against the item.</param>
        /// <returns>The overall state.</returns>
        public static AnnotationState SummarizeState(IEnumerable<EngineeringModelDataAnnotation> annotations)
        {
            var all = annotations.ToList();

            if (!all.Any())
            {
                return AnnotationState.None;
            }

            return all.Any(IsOpen) ? AnnotationState.Open : AnnotationState.Resolved;
        }

        /// <summary>
        /// Asserts whether an annotation takes part in the V&amp;V non-conformance flow. Change requests and model
        /// notes can be raised from the register but are not concessions against a requirement, so they are excluded
        /// from the icons, the NCR sheet and the close-out rule. The distinction lives in
        /// <see cref="AnnotationKind"/>, not in a hard-coded type test.
        /// </summary>
        /// <param name="annotation">The annotation.</param>
        /// <returns>true when it is a RID, request for deviation or request for waiver.</returns>
        private static bool IsReviewRequest(EngineeringModelDataAnnotation annotation)
        {
            return AnnotationKind.All.Any(kind => kind.ClassKind == annotation.ClassKind && kind.IsReviewRequest);
        }

        /// <summary>
        /// Resolves the V&amp;V item and requirement an annotation concerns. An annotation raised on a V&amp;V item
        /// also names the requirement that item verifies; one raised on a requirement leaves the V&amp;V item empty.
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/>.</param>
        /// <param name="annotation">The annotation.</param>
        /// <returns>The described review request.</returns>
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
