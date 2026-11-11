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

namespace CDP4Composition.Annotations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;

    /// <summary>
    /// Queries the ECSS-E-TM-10-25 annotations raised against a model <see cref="Thing"/>. Shared by the browsers
    /// (row badge, icon and context menu), the V&amp;V close-out rules and the workbook exporter, so all of them
    /// agree on what is raised against what and on what an open annotation is.
    /// </summary>
    public static class AnnotationQuery
    {
        /// <summary>
        /// The <see cref="AnnotationStatusKind"/>s that mean the annotation no longer needs action.
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
        /// Every <see cref="AnnotationStatusKind"/> a raised annotation can be moved to, with the caption to offer
        /// it under, in lifecycle order.
        /// </summary>
        /// <remarks>
        /// The normal path is OPEN, then DONE once the work is in the model, then CLOSED once it has been accepted.
        /// The last three are exits without work: raised in error, valid but deliberately not being fixed, and does
        /// not apply. ECSS defines the enum but the SDK ships it undocumented, so the captions are this
        /// application's wording.
        /// </remarks>
        public static IReadOnlyList<(AnnotationStatusKind Status, string Caption)> StatusChoices { get; } =
            new[]
            {
                (AnnotationStatusKind.OPEN, "Reopen"),
                (AnnotationStatusKind.DONE, "Mark Implemented"),
                (AnnotationStatusKind.CLOSED, "Close (Accepted)"),
                (AnnotationStatusKind.INVALID, "Mark Invalid (Raised in Error)"),
                (AnnotationStatusKind.WONTFIX, "Mark Won't Fix"),
                (AnnotationStatusKind.NOT_APPLICABLE, "Mark Not Applicable")
            };

        /// <summary>
        /// Returns every annotation raised against a <see cref="Thing"/>, of any kind.
        /// </summary>
        /// <param name="thing">The annotated <see cref="Thing"/>.</param>
        /// <returns>The annotations, ordered by identifier; empty when the thing is not model data.</returns>
        /// <remarks>
        /// Model notes live in <see cref="EngineeringModel.GenericNote"/> rather than
        /// <see cref="EngineeringModel.ModellingAnnotation"/>, which is why both lists are searched. Anything that
        /// can be created must be findable afterwards, or it is write-only.
        /// </remarks>
        public static IReadOnlyList<EngineeringModelDataAnnotation> QueryAllFor(Thing thing)
        {
            var model = thing?.GetContainerOfType<EngineeringModel>();

            if (model == null)
            {
                return [];
            }

            return model.ModellingAnnotation
                .Cast<EngineeringModelDataAnnotation>()
                .Concat(model.GenericNote)
                .Where(annotation => Annotates(annotation, thing))
                .OrderBy(AnnotationKind.QueryShortName)
                .ToList();
        }

        /// <summary>
        /// Returns the formal concessions raised against a <see cref="Thing"/>, that is, the Review Item
        /// Discrepancies, Requests for Deviation and Requests for Waiver, leaving out the change requests and model
        /// notes that are annotations but not concessions.
        /// </summary>
        /// <param name="thing">The annotated <see cref="Thing"/>.</param>
        /// <returns>The concessions, ordered by short-name; empty when the thing is not model data.</returns>
        public static IReadOnlyList<ModellingAnnotationItem> QueryReviewRequestsFor(Thing thing)
        {
            var model = thing?.GetContainerOfType<EngineeringModel>();

            if (model == null)
            {
                return [];
            }

            return model.ModellingAnnotation
                .Where(annotation => IsReviewRequest(annotation) && Annotates(annotation, thing))
                .OrderBy(annotation => annotation.ShortName)
                .ToList();
        }

        /// <summary>
        /// Asserts whether an annotation takes part in the formal concession flow. The distinction lives in
        /// <see cref="AnnotationKind"/> rather than in a hard-coded type test.
        /// </summary>
        /// <param name="annotation">The annotation.</param>
        /// <returns>true when it is a RID, request for deviation or request for waiver.</returns>
        public static bool IsReviewRequest(EngineeringModelDataAnnotation annotation)
        {
            return AnnotationKind.Query(annotation)?.IsReviewRequest == true;
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
        /// Summarises a set of annotations as a single state, so a row can show at a glance whether it is clean,
        /// blocked or resolved.
        /// </summary>
        /// <param name="annotations">The annotations raised against the thing.</param>
        /// <returns>The overall state.</returns>
        public static AnnotationState SummarizeState(IEnumerable<EngineeringModelDataAnnotation> annotations)
        {
            var all = annotations.ToList();

            if (all.Count == 0)
            {
                return AnnotationState.None;
            }

            return all.Any(IsOpen) ? AnnotationState.Open : AnnotationState.Resolved;
        }

        /// <summary>
        /// Builds the short badge shown in the annotation column of a browser: the identifier of the annotation that
        /// most needs attention, plus how many others there are.
        /// </summary>
        /// <param name="annotations">The annotations raised against the thing.</param>
        /// <returns>The badge text, or an empty string when nothing was raised.</returns>
        public static string DescribeIndicator(IEnumerable<EngineeringModelDataAnnotation> annotations)
        {
            var all = annotations.ToList();

            if (all.Count == 0)
            {
                return string.Empty;
            }

            var leading = all.FirstOrDefault(IsOpen) ?? all[0];
            var identifier = AnnotationKind.QueryShortName(leading);

            return all.Count == 1 ? identifier : $"{identifier} +{all.Count - 1}";
        }

        /// <summary>
        /// Suggests a short-name for a new annotation, so the author is not asked to invent an identifier and the
        /// model keeps one naming scheme.
        /// </summary>
        /// <param name="thing">The <see cref="Thing"/> the annotation is raised against.</param>
        /// <param name="kind">The kind being raised.</param>
        /// <returns>A short-name of the form <c>THING-KIND-nnn</c> that is free in the model.</returns>
        public static string SuggestShortName(Thing thing, AnnotationKind kind)
        {
            var subject = (thing as DefinedThing)?.ShortName;
            var prefix = string.IsNullOrWhiteSpace(subject)
                ? kind.Acronym
                : $"{subject}-{kind.Acronym}";

            var model = thing?.GetContainerOfType<EngineeringModel>();

            var taken = model == null
                ? new HashSet<string>()
                : new HashSet<string>(model.ModellingAnnotation.Select(annotation => annotation.ShortName));

            var sequence = 1;

            while (taken.Contains($"{prefix}-{sequence:D3}"))
            {
                sequence++;
            }

            return $"{prefix}-{sequence:D3}";
        }

        /// <summary>
        /// Builds the tooltip listing every annotation raised against a thing, one line of kind, identifier and
        /// status per annotation, so the reader does not have to open a menu to see what is outstanding.
        /// </summary>
        /// <param name="annotations">The annotations raised against the thing.</param>
        /// <returns>The tooltip text, or an empty string when nothing was raised.</returns>
        public static string DescribeSummary(IEnumerable<EngineeringModelDataAnnotation> annotations)
        {
            var lines = annotations.Select(Describe).ToList();

            return string.Join(Environment.NewLine, lines);
        }

        /// <summary>
        /// Describes a single annotation as one tooltip line.
        /// </summary>
        /// <param name="annotation">The annotation.</param>
        /// <returns>The kind, identifier and, where the kind has one, the status.</returns>
        private static string Describe(EngineeringModelDataAnnotation annotation)
        {
            var status = DescribeStatus(annotation);
            var identifier = AnnotationKind.QueryShortName(annotation);
            var kind = AnnotationKind.Describe(annotation);

            return string.IsNullOrEmpty(status)
                ? $"{kind}: {identifier}"
                : $"{kind}: {identifier} ({status})";
        }

        /// <summary>
        /// Asserts whether an annotation refers to a <see cref="Thing"/>.
        /// </summary>
        /// <param name="annotation">The annotation.</param>
        /// <param name="thing">The candidate annotated <see cref="Thing"/>.</param>
        /// <returns>true when the annotation names the thing.</returns>
        private static bool Annotates(EngineeringModelDataAnnotation annotation, Thing thing)
        {
            return annotation.RelatedThing.Any(reference => reference.ReferencedThing == thing)
                   || annotation.PrimaryAnnotatedThing?.ReferencedThing == thing;
        }
    }
}
