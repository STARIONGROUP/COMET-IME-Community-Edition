// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClasskindToIconConverter.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
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
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Data;

    using CDP4Common.CommonData;
    using CDP4Common.Helpers;    

    using CDP4Composition.Services;    
    
    using CommonServiceLocator;
    
    /// <summary>
    /// The element definition browser icon converter
    /// </summary>
    public class ClasskindToIconConverter : IMultiValueConverter
    {
        /// <summary>
        /// The <see cref="IIconCacheService"/>
        /// </summary>
        private IIconCacheService iconCacheService;

        /// <summary>
        /// The generic <see cref="ThingToIconUriConverter"/>
        /// </summary>
        private static readonly ThingToIconUriConverter GenericConverter = new ThingToIconUriConverter();

        /// <summary>
        /// Returns an GetImage (icon) based on the <see cref="ClassKind"/> that is provided
        /// </summary>
        /// <param name="value">A <see cref="ClassKind"/> for which an Icon needs to be returned</param>
        /// <param name="targetType">The parameter is not used.</param>
        /// <param name="parameter">The <see cref="ClassKind"/> of the overlay to use</param>
        /// <param name="culture">The parameter is not used.</param>
        /// <returns>
        /// A <see cref="Uri"/> to an GetImage
        /// </returns>
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            var classKind = value.OfType<ClassKind?>().SingleOrDefault();
            if (classKind == null)
            {
                return null;
            }

            var baseUri = new Uri(IconUtilities.ImageUri(classKind.Value).ToString());
            return this.QueryIIconCacheService().QueryBitmapImage(baseUri); 
        }

        /// <summary>
        /// Queries the instance of the <see cref="IIconCacheService"/> that is to be used
        /// </summary>
        /// <returns>
        /// An instance of <see cref="IIconCacheService"/>
        /// </returns>
        private IIconCacheService QueryIIconCacheService()
        {
            if (this.iconCacheService == null)
            {
                this.iconCacheService = ServiceLocator.Current.GetInstance<IIconCacheService>();
            }

            return this.iconCacheService;
        }

        /// <summary>
        /// Not supported
        /// </summary>
        /// <param name="value">The parameter is not used.</param>
        /// <param name="targetTypes">The parameter is not used.</param>
        /// <param name="parameter">The parameter is not used.</param>
        /// <param name="culture">The parameter is not used.</param>
        /// <returns>a <see cref="NotSupportedException"/> is thrown</returns>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}