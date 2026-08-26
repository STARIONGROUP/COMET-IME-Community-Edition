// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AnnotationKind.cs" company="Starion Group S.A.">
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
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.ReportingData;

    /// <summary>
    /// One kind of annotation that can be raised from the V&amp;V register, described once so the context menu, the
    /// dialog and the writer all work from the same table instead of hard-coding three special cases.
    /// </summary>
    /// <remarks>
    /// The IME registers a <c>ThingDialog</c> for none of these <see cref="ClassKind"/>s, so every one of the stock
    /// create-annotation commands throws "not registered with the Application" and silently does nothing. That is why
    /// this plugin writes them itself, and why the table covers all five kinds rather than only the three the V&amp;V
    /// non-conformance flow needs.
    /// </remarks>
    public class AnnotationKind
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnnotationKind"/> class.
        /// </summary>
        /// <param name="name">The human-readable kind name.</param>
        /// <param name="classKind">The <see cref="ClassKind"/> the menu item is permissioned against.</param>
        /// <param name="create">Creates an empty instance.</param>
        /// <param name="isReviewRequest">Whether the kind is part of the V&amp;V non-conformance flow.</param>
        private AnnotationKind(string name, ClassKind classKind, Func<EngineeringModelDataAnnotation> create, bool isReviewRequest)
        {
            this.Name = name;
            this.ClassKind = classKind;
            this.Create = create;
            this.IsReviewRequest = isReviewRequest;
        }

        /// <summary>
        /// Gets the human-readable kind name, used as the menu caption and the dialog title.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the <see cref="ClassKind"/>, used for the menu item's icon and permission.
        /// </summary>
        public ClassKind ClassKind { get; }

        /// <summary>
        /// Gets the factory for an empty instance.
        /// </summary>
        public Func<EngineeringModelDataAnnotation> Create { get; }

        /// <summary>
        /// Gets a value indicating whether this kind takes part in the V&amp;V non-conformance flow, that is, whether
        /// it is tracked on the item icons, the NCR sheet and the close-out rule. A change request or a model note is
        /// an annotation but not a concession against a requirement.
        /// </summary>
        public bool IsReviewRequest { get; }

        /// <summary>
        /// Gets a value indicating whether the kind carries the identification fields that only a
        /// <see cref="ModellingAnnotationItem"/> has: short name, title, classification and status.
        /// </summary>
        public bool HasIdentification => this.ClassKind != ClassKind.EngineeringModelDataNote;

        /// <summary>
        /// Gets every annotation kind that can be raised from the register, in menu order.
        /// </summary>
        public static IReadOnlyList<AnnotationKind> All { get; } = new[]
        {
            new AnnotationKind("Review Item Discrepancy", ClassKind.ReviewItemDiscrepancy, () => new ReviewItemDiscrepancy(Guid.NewGuid(), null, null), true),
            new AnnotationKind("Request for Deviation", ClassKind.RequestForDeviation, () => new RequestForDeviation(Guid.NewGuid(), null, null), true),
            new AnnotationKind("Request for Waiver", ClassKind.RequestForWaiver, () => new RequestForWaiver(Guid.NewGuid(), null, null), true),
            new AnnotationKind("Change Request", ClassKind.ChangeRequest, () => new ChangeRequest(Guid.NewGuid(), null, null), false),
            new AnnotationKind("Model Note", ClassKind.EngineeringModelDataNote, () => new EngineeringModelDataNote(Guid.NewGuid(), null, null), false)
        };

        /// <summary>
        /// Names the kind of an existing annotation.
        /// </summary>
        /// <param name="annotation">The annotation.</param>
        /// <returns>The human-readable kind name.</returns>
        public static string Describe(EngineeringModelDataAnnotation annotation)
        {
            return All.FirstOrDefault(kind => kind.ClassKind == annotation.ClassKind)?.Name
                   ?? annotation.ClassKind.ToString();
        }

        /// <summary>
        /// Returns the identifier to show for an annotation.
        /// </summary>
        /// <param name="annotation">The annotation.</param>
        /// <returns>The short-name of a review request, or the kind for a plain note, which carries none.</returns>
        /// <remarks>
        /// Deliberately not <c>UserFriendlyShortName</c>: the SDK leaves it unimplemented on the annotation types, so
        /// it rendered as the literal sentence "User-friendly short-name not implemented." wherever it was shown.
        /// Only a <see cref="ModellingAnnotationItem"/> carries a real short-name; a plain note has none at all.
        /// </remarks>
        public static string QueryShortName(EngineeringModelDataAnnotation annotation)
        {
            var shortName = (annotation as ModellingAnnotationItem)?.ShortName;

            return string.IsNullOrWhiteSpace(shortName) ? Describe(annotation) : shortName;
        }
    }
}
