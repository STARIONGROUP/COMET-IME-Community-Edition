// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThingToIconUriConverter.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
// 
//    This file is part of CDP4-COMET-IME Community Edition.
//    The CDP4-COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
// 
//    The CDP4-COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
// 
//    The CDP4-COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition
{
    using System;
    using System.Drawing;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Data;
    using System.Windows.Media.Imaging;

    using CDP4Common.CommonData;
    using CDP4Common.Helpers;

    using CDP4Composition.Mvvm;
    using CDP4Composition.Services;

    using DevExpress.Xpf.Core;

    using Microsoft.Practices.ServiceLocation;

    /// <summary>
    /// The purpose of the <see cref="ThingToIconUriConverter" /> is to return an icon based on the
    /// provided <see cref="Thing" />. The icon is returned as a string
    /// </summary>
    public class ThingToIconUriConverter : IMultiValueConverter
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
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            var rowStatus = value.OfType<RowStatusKind>().SingleOrDefault();
            var thing = value.OfType<Thing>().SingleOrDefault();
            var thingStatus = value.OfType<ThingStatus>().SingleOrDefault();

            Uri uri;

            if (thing == null && thingStatus == null)
            {
                return null;
            }

            var classKind = thing != null ? thing.ClassKind : thingStatus.Thing.ClassKind;

            switch (rowStatus)
            {
                case RowStatusKind.Active:
                    uri = new Uri(IconUtilities.ImageUri(classKind).ToString());
                    break;
                case RowStatusKind.Inactive:
                    uri = new Uri(this.GrayScaleImageUri(classKind).ToString());
                    break;
                default:
                    uri = new Uri(IconUtilities.ImageUri(classKind).ToString());
                    break;
            }

            if (thing != null)
            {
                if (thing.ValidationErrors.Any())
                {
                    return this.QueryIIconCacheService().QueryErrorOverlayBitmapSource(uri);
                }

                return this.QueryIIconCacheService().QueryBitmapImage(uri);
            }

            if (thingStatus.HasError)
            {
                return this.QueryIIconCacheService().QueryErrorOverlayBitmapSource(uri);
            }

            if (thingStatus.IsLocked)
            {
                return this.QueryIIconCacheService().QueryOverlayBitmapSource(uri, IconUtilities.LockedOverlayUri, OverlayPositionKind.TopRight);
            }

            if (thingStatus.IsHidden)
            {
                return this.QueryIIconCacheService().QueryOverlayBitmapSource(uri, IconUtilities.HiddenOverlayUri, OverlayPositionKind.TopRight);
            }

            if (thingStatus.HasRelationship)
            {
                return this.QueryIIconCacheService().QueryOverlayBitmapSource(uri, IconUtilities.RelationshipOverlayUri, OverlayPositionKind.TopRight);
            }

            if (thingStatus.IsFavorite)
            {
                return this.QueryIIconCacheService().QueryOverlayBitmapSource(uri, IconUtilities.FavoriteOverlayUri, OverlayPositionKind.BottomRight);
            }

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
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Returns an instance of <see cref="Image" /> based on the provided <see cref="ClassKind" />
        /// </summary>
        /// <param name="classKind">
        /// the subject <see cref="ClassKind" />
        /// </param>
        /// <param name="getsmallicon">
        /// Indicates whether a small or large icon should be returned.
        /// </param>
        /// <returns>
        /// An of <see cref="Image" /> that corresponds to the subject <see cref="ClassKind" />
        /// </returns>
        public Image GetImage(ClassKind classKind, bool getsmallicon = true)
        {
            Uri imageUri;

            if (IconUtilities.ImageUri(classKind, getsmallicon) is string convertedstring)
            {
                imageUri = new Uri(convertedstring);
                var image = new BitmapImage(imageUri);
                var bitmap = IconUtilities.BitmapImage2Bitmap(image);
                return bitmap;
            }

            if (IconUtilities.ImageUri(classKind, getsmallicon) is DXImageExtension convertedDXImageExtension)
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.UriSource = convertedDXImageExtension.Image.MakeUri();
                image.EndInit();

                var bitmap = IconUtilities.BitmapImage2Bitmap(image);
                return bitmap;
            }

            return null;
        }

        /// <summary>
        /// Returns the <see cref="Uri" /> of the resource in grayscale
        /// </summary>
        /// <param name="classKind">
        /// The <see cref="ClassKind" /> for which in icon needs to be provided
        /// </param>
        /// <param name="getsmallicon">
        /// Indicates whether a small or large icon should be returned.
        /// </param>
        /// <returns>
        /// A <see cref="Uri" /> that points to a resource
        /// </returns>
        private object GrayScaleImageUri(ClassKind classKind, bool getsmallicon = true)
        {
            var compositionroot = "pack://application:,,,/CDP4Composition;component/Resources/Images/Thing/";
            var imagesize = getsmallicon ? "_16x16" : "_32x32";
            string imagename;
            var imageextension = ".png";

            switch (classKind)
            {
                case ClassKind.Participant:
                    imagename = "grayscaleParticipant";
                    return $"{compositionroot}{imagename}{imagesize}{imageextension}";
                case ClassKind.Person:
                    imagename = "grayscalePerson";
                    return $"{compositionroot}{imagename}{imagesize}{imageextension}";
                case ClassKind.IterationSetup:
                    imagename = "grayscaleIterationSetup";
                    return $"{compositionroot}{imagename}{imagesize}{imageextension}";
                default:
                    // Iteration Setup for now used as default
                    imagename = "grayscaleIterationSetup";
                    return $"{compositionroot}{imagename}{imagesize}{imageextension}";
            }
        }

        /// <summary>
        /// Queries the instance of the <see cref="IIconCacheService" /> that is to be used
        /// </summary>
        /// <returns>
        /// An instance of <see cref="IIconCacheService" />
        /// </returns>
        private IIconCacheService QueryIIconCacheService()
        {
            return this.iconCacheService ?? (this.iconCacheService = ServiceLocator.Current.GetInstance<IIconCacheService>());
        }
    }
}
