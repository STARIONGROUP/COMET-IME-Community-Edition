// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClasskindToIconConverter.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
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
    using Microsoft.Practices.ServiceLocation;
    
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