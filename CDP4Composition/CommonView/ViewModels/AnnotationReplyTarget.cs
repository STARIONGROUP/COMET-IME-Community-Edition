// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AnnotationReplyTarget.cs" company="Starion Group S.A.">
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

namespace CDP4CommonView.ViewModels
{
    using CDP4Common.ReportingData;

    using CDP4Composition.Annotations;

    /// <summary>
    /// One annotation a reply can be aimed at on a shared discussion board, that is, the change request itself or
    /// one of the change proposals answering it.
    /// </summary>
    public class AnnotationReplyTarget
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnnotationReplyTarget"/> class.
        /// </summary>
        /// <param name="annotation">The annotation the reply would be posted on.</param>
        public AnnotationReplyTarget(EngineeringModelDataAnnotation annotation)
        {
            this.Annotation = annotation;
            this.Caption = $"{AnnotationKind.QueryShortName(annotation)} ({AnnotationKind.Describe(annotation)})";
        }

        /// <summary>
        /// Gets the annotation the reply is posted on.
        /// </summary>
        public EngineeringModelDataAnnotation Annotation { get; }

        /// <summary>
        /// Gets the caption shown in the picker.
        /// </summary>
        public string Caption { get; }
    }
}
