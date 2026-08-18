// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVAnnotationTrace.cs" company="Starion Group S.A.">
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
    using CDP4Common.ReportingData;

    /// <summary>
    /// A review request together with the V&amp;V item and requirement it concerns, so the NCR sheet can lead with
    /// traceability instead of making the reader chase references.
    /// </summary>
    public class VandVAnnotationTrace
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VandVAnnotationTrace"/> class.
        /// </summary>
        /// <param name="annotation">The review request.</param>
        /// <param name="vandVItem">The V&amp;V item it was raised on, if any.</param>
        /// <param name="requirement">The requirement it ultimately concerns, if any.</param>
        public VandVAnnotationTrace(ModellingAnnotationItem annotation, Requirement vandVItem, Requirement requirement)
        {
            this.Annotation = annotation;
            this.VandVItem = vandVItem;
            this.Requirement = requirement;
        }

        /// <summary>
        /// Gets the review request.
        /// </summary>
        public ModellingAnnotationItem Annotation { get; }

        /// <summary>
        /// Gets the V&amp;V item the request was raised on, or null when it was raised directly on a requirement.
        /// </summary>
        public Requirement VandVItem { get; }

        /// <summary>
        /// Gets the requirement the request concerns, or null when it could not be resolved.
        /// </summary>
        public Requirement Requirement { get; }
    }

    /// <summary>
    /// The review-request state of a V&amp;V item, shown as a distinct icon in the browser.
    /// </summary>
    public enum AnnotationState
    {
        /// <summary>
        /// No review request has been raised against the item.
        /// </summary>
        None,

        /// <summary>
        /// At least one review request is still open and needs action.
        /// </summary>
        Open,

        /// <summary>
        /// Every review request raised against the item has been closed out.
        /// </summary>
        Resolved
    }
}
