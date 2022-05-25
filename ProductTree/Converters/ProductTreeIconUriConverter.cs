// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProductTreeIconUriConverter.cs" company="RHEA System S.A.">
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
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ProductTree
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Data;
    using System.Windows.Media.Imaging;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.Helpers;

    using CDP4Composition.Mvvm;

    using CDP4ProductTree.ViewModels;

    /// <summary>
    /// The purpose of the <see cref="ProductTreeIconUriConverter" /> is to return an icon based on the
    /// provided <see cref="ProductTreeIconUriConverter" />. The icon is returned as a string
    /// </summary>
    public class ProductTreeIconUriConverter : IMultiValueConverter
    {
        /// <summary>
        /// Returns an GetImage (icon) if a ParameterOrOverride value is provided to display in the product tree.
        /// </summary>
        /// <param name="value">A row containing a <see cref="ParameterOrOverrideBase" /> for which an Icon needs to be returned</param>
        /// <param name="targetType">The parameter is not used.</param>
        /// <param name="parameter">The parameter is not used.</param>
        /// <param name="culture">The parameter is not used.</param>
        /// <returns>
        /// A <see cref="Uri" /> to an GetImage
        /// </returns>
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value.First() == null)
            {
                return null;
            }

            var thingStatus = value.FirstOrDefault() as ThingStatus;
            var usage = value.OfType<ParameterUsageKind>().SingleOrDefault();

            if (thingStatus == null)
            {
                return null;
            }

            return this.GetIconForParameterOverride(thingStatus, usage);
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
        /// The get icon for parameter override.
        /// </summary>
        /// <param name="thingStatus">
        /// The parameter or override.
        /// </param>
        /// <returns>
        /// The <see cref="object" />.
        /// </returns>
        private object GetIconForParameterOverride(ThingStatus thingStatus, ParameterUsageKind usage)
        {
            Uri uri = null;

            switch (usage)
            {
                case ParameterUsageKind.Unused:
                    uri = new Uri("pack://application:,,,/CDP4Composition;component/Resources/Images/orangeball.jpg");
                    break;
                case ParameterUsageKind.SubscribedByOthers:
                    uri = new Uri("pack://application:,,,/CDP4Composition;component/Resources/Images/blueball.gif");
                    break;
                case ParameterUsageKind.Subscribed:
                    uri = new Uri("pack://application:,,,/CDP4Composition;component/Resources/Images/whiteball.jpg");
                    break;
            }

            if (uri == null)
            {
                return null;
            }

            return thingStatus.HasError
                ? IconUtilities.WithErrorOverlay(uri)
                : thingStatus.HasRelationship
                    ? IconUtilities.WithOverlay(uri, IconUtilities.RelationshipOverlayUri)
                    : new BitmapImage(uri);
        }
    }
}
