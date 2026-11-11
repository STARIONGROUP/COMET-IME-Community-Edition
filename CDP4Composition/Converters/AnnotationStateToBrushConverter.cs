// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AnnotationStateToBrushConverter.cs" company="Starion Group S.A.">
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

namespace CDP4Composition.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Media;

    using CDP4Composition.Annotations;

    /// <summary>
    /// Converts an <see cref="AnnotationState"/> into the <see cref="Brush"/> the annotation text of a row is drawn
    /// in, so an item carrying an open annotation stands out from one whose annotations are all closed out.
    /// </summary>
    public class AnnotationStateToBrushConverter : IValueConverter
    {
        /// <summary>
        /// The text colour of an item carrying at least one annotation that still needs action.
        /// </summary>
        private static readonly Brush OpenBrush = new SolidColorBrush(Color.FromRgb(0xC0, 0x39, 0x2B));

        /// <summary>
        /// The text colour of an item whose annotations are all closed out, or that carries only kinds with no
        /// status at all such as a model note.
        /// </summary>
        private static readonly Brush ResolvedBrush = new SolidColorBrush(Color.FromRgb(0x60, 0x60, 0x60));

        /// <summary>
        /// Converts an <see cref="AnnotationState"/> into a <see cref="Brush"/>.
        /// </summary>
        /// <param name="value">The <see cref="AnnotationState"/>.</param>
        /// <param name="targetType">The parameter is not used.</param>
        /// <param name="parameter">The parameter is not used.</param>
        /// <param name="culture">The parameter is not used.</param>
        /// <returns>The text colour; the resolved colour when nothing was raised, since the text is empty anyway.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is AnnotationState.Open ? OpenBrush : ResolvedBrush;
        }

        /// <summary>
        /// Not supported.
        /// </summary>
        /// <param name="value">The parameter is not used.</param>
        /// <param name="targetType">The parameter is not used.</param>
        /// <param name="parameter">The parameter is not used.</param>
        /// <param name="culture">The parameter is not used.</param>
        /// <returns>A <see cref="NotSupportedException"/> is thrown.</returns>
        /// <exception cref="NotSupportedException">Always; the conversion is one-way.</exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("The AnnotationStateToBrushConverter is a one-way converter.");
        }
    }
}
