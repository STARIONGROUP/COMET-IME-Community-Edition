// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SvgHelper.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Kamil Wojnowski
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Grapher.Utilities
{
    using System;
    using System.IO;
    using System.Windows.Media;

    using DevExpress.Xpf.Core;
    using DevExpress.Xpf.Core.Native;

    /// <summary>
    /// Helps with SVG images converting them to <see cref="ImageSource"/> using DevExpress Svg helpers
    /// </summary>
    public static class SvgHelper
    {
        /// <summary>
        /// Base resource path for devexpress svg
        /// </summary>
        private const string DevExpressResourceBasePath = "DevExpress.Images.v20.2;component/SvgImages/";

        /// <summary>
        /// Converts Svg bytes to ImageSource
        /// </summary>
        /// <param name="path"></param>
        /// <returns>a usable <see cref="ImageSource"/></returns>
        public static ImageSource ToImageSource(Uri path)
        {
            using var stream = SvgImageHelper.CreateStream(path);
            object unused = null;
            var image = SvgImageHelper.GetOrCreateSvgImage(stream, ref unused);
            return WpfSvgRenderer.CreateImageSource(image, 1d, null, null, true);
        }

        /// <summary>
        /// Converts Svg bytes to ImageSource
        /// </summary>
        /// <param name="resourceName">the name of the resource. If using a devexpress SVG</param>
        /// <param name="isSvgFromDevepress">Whether to use <see cref="DevExpressResourceBasePath"/></param>
        /// <returns>a usable <see cref="ImageSource"/></returns>
        public static ImageSource ToImageSource(string resourceName, bool isSvgFromDevepress = true)
        {
            var uri = isSvgFromDevepress || !resourceName.StartsWith("pack://") ? new Uri(Path.Combine("pack://application:,,,/", DevExpressResourceBasePath, resourceName)) : new Uri(resourceName);
            return ToImageSource(uri);
        }
    }
}
