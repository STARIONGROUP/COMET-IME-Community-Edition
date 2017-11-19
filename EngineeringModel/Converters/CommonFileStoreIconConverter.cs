// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommonFileStoreIconConverter.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Converters
{
    using System;    
    using System.Windows.Media.Imaging;
    using CDP4Common.CommonData;
    using System.Globalization;    
    using System.Windows.Data;    
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Helpers;

    /// <summary>
    /// The common file store icon converter
    /// </summary>
    public class CommonFileStoreIconConverter : IMultiValueConverter
    {
        /// <summary>
        /// Returns an GetImage (icon) based on the <see cref="Thing"/> that is provided
        /// </summary>
        /// <param name="value">An instance of <see cref="Thing"/> for which an Icon needs to be returned</param>
        /// <param name="targetType">The parameter is not used.</param>
        /// <param name="parameter">The <see cref="ClassKind"/> of the overlay to use</param>
        /// <param name="culture">The parameter is not used.</param>
        /// <returns>
        /// A <see cref="Uri"/> to an GetImage
        /// </returns>
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            if (value.Length == 0)
            {
                return null;
            }

            var file = value[0] as File;
            if (file  != null)
            {
                var fileIconUri = new Uri(IconUtilities.ImageUri(file.ClassKind).ToString());
                return new BitmapImage(fileIconUri);
            }

            var thing = value[0] as Thing;
            if (thing != null)
            {
                var fileIconUri = new Uri(IconUtilities.ImageUri(thing.ClassKind).ToString());
                return new BitmapImage(fileIconUri);
            }

            var isLocked = value[0] as bool?;
            if (isLocked != null && isLocked.Value)
            {
                var lockedUri = new Uri("pack://application:,,,/CDP4Composition;component/Resources/Images/Thing/lock.png", UriKind.RelativeOrAbsolute);
                return new BitmapImage(lockedUri);
            }

            return null;
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