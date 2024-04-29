// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IconToImageSourceConverter.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2022 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// <remarks>
//    Ideas taken from ExceptionReporter.NET: https://github.com/PandaWood/ExceptionReporter.NET
// </remarks>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.ExceptionReporting.Converters
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Interop;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    /// <summary>
    /// An <see cref="IValueConverter"/> that converts an icon to an image 
    /// </summary>
    public class IconToImageSourceConverter : IValueConverter
    {
        /// <summary>
        /// Converts an icon to an image
        /// </summary>
        /// <param name="value">The icon</param>
        /// <param name="targetType">the target <see cref="Type"/></param>
        /// <param name="parameter">Conditional parameter</param>
        /// <param name="culture">The <see cref="CultureInfo"/></param>
        /// <returns>An <see cref="object"/></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Icon icon)
            {
                Trace.TraceWarning("Attempted to convert {0} instead of Icon object in IconToImageSourceConverter", value);
                return null;
            }

            ImageSource imageSource = Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            return imageSource;
        }

        /// <summary>
        /// Not implemented
        /// </summary>
        /// <param name="value">The image</param>
        /// <param name="targetType">the target <see cref="Type"/></param>
        /// <param name="parameter">Conditional parameter</param>
        /// <param name="culture">The <see cref="CultureInfo"/></param>
        /// <returns>An <see cref="object"/></returns>
        /// <exception cref="NotSupportedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
