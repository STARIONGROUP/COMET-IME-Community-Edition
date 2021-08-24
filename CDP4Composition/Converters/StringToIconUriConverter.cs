// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StringToIconUriConverter.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Nathanael Smiechowski, Ahmed Ahmed, Simon Wood
// 
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
// 
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
// 
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//    Lesser General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    using CDP4Common.CommonData;

    using CDP4Composition.Services;

    using DevExpress.Xpf.Core;
    using DevExpress.Xpf.Core.Native;

    using Microsoft.Practices.ServiceLocation;

    /// <summary>
    /// The purpose of the <see cref="ThingToIconUriConverter" /> is to return an icon based on the
    /// provided <see cref="Thing" />. The icon is returned as a string
    /// </summary>
    public class StringToIconUriConverter : IValueConverter
    {
        /// <summary>
        /// The <see cref="IIconCacheService" />
        /// </summary>
        private IIconCacheService iconCacheService;

        /// <summary>
        /// Returns an GetImage (icon) based on the <see cref="Thing" /> that is provided
        /// </summary>
        /// <param name="value">An instance of <see cref="Thing" /> for which an Icon needs to be returned</param>
        /// <param name="targetType">The parameter is not used.</param>
        /// <param name="parameter">The parameter is not used.</param>
        /// <param name="culture">The parameter is not used.</param>
        /// <returns>
        /// A <see cref="Uri" /> to an GetImage
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            var imageInfo = new DXImageConverter().ConvertFrom(value) as DXImageInfo;

            if (imageInfo == null)
            {
                return null;
            }

            var uri = new Uri(imageInfo.MakeUri().ToString());

            return this.QueryIIconCacheService().QueryBitmapImage(uri);
        }

        /// <summary>
        /// Not supported
        /// </summary>
        /// <param name="value">The parameter is not used.</param>
        /// <param name="targetTypes">The parameter is not used.</param>
        /// <param name="parameter">The parameter is not used.</param>
        /// <param name="culture">The parameter is not used.</param>
        /// <returns>a <see cref="NotSupportedException" /> is thrown</returns>
        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Queries the instance of the <see cref="IIconCacheService" /> that is to be used
        /// </summary>
        /// <returns>
        /// An instance of <see cref="IIconCacheService" />
        /// </returns>
        private IIconCacheService QueryIIconCacheService()
        {
            return this.iconCacheService ??= ServiceLocator.Current.GetInstance<IIconCacheService>();
        }
    }
}
