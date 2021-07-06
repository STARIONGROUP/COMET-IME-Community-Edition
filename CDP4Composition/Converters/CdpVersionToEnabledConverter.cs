// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CdpVersionToVisibilityConverter.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Alexander van Delft, Nathanael Smiechowski
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

namespace CDP4Composition
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Windows.Data;

    using CDP4Common;

    using CDP4Dal;

    /// <summary>
    /// The purpose of the <see cref="CdpVersionToEnabledConverter"/> is to convert the <see cref="CDPVersionAttribute.Version"/> version that is used
    /// to decorate the property of a view-model to a Enabled state based on the <see cref="ISession.DalVersion"/> that is active.
    /// </summary>
    public class CdpVersionToEnabledConverter : IValueConverter
    {
        /// <summary>
        /// Converts the  CDP Version string to a boolean value, either true or false
        /// </summary>
        /// <param name="value">an instance of <see cref="IISession"/></param>
        /// <param name="targetType">The targetType is not used</param>
        /// <param name="parameter">The name of the property that is used to determine enabled state of the control</param>
        /// <param name="culture">The culture is not used</param>
        /// <returns>The Enabled state based on the provided CDPVersion</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var viewmodel = value as IISession;

            if (viewmodel == null)
            {
                return Binding.DoNothing;
            }

            var dalVersion = viewmodel.Session.DalVersion;

            var propertyName = parameter as string;

            if (string.IsNullOrEmpty(propertyName))
            {
                return Binding.DoNothing;
            }

            var type = value.GetType();
            var property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

            if (property == null)
            {
                return Binding.DoNothing;
            }

            if (!property.IsDefined(typeof(CDPVersionAttribute), true))
            {
                return true;
            }

            var versionAttribute = (CDPVersionAttribute)property.GetCustomAttributes(typeof(CDPVersionAttribute), true)[0];
            var propertyVersion = new Version(versionAttribute.Version);

            var comparision = propertyVersion.CompareTo(dalVersion);

            return comparision <= 0;
        }

        /// <summary>Not supported</summary>
        /// <returns>The method is not supported</returns>
        /// <param name="value">The method is not supported.</param>
        /// <param name="targetType">The method is not supported.</param>
        /// <param name="parameter">The method is not supported.</param>
        /// <param name="culture">The method is not supported.</param>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
