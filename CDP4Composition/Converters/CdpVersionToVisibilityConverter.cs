// ------------------------------------------------------------------------------------------------
// <copyright file="CdpVersionToVisibilityConverter.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Composition
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Data;
    using CDP4Common;    
    using CDP4Common.Comparers;
    using CDP4Common.Helpers;
    using CDP4Dal;

    /// <summary>
    /// The purpose of the <see cref="CdpVersionToVisibilityConverter"/> is to convert the <see cref="CDPVersionAttribute.Version"/> version that is used
    /// to decorate the property of a view-model to a <see cref="Visibility"/> based on the <see cref="ISession.DalVersion"/> that is active.
    /// </summary>
    public class CdpVersionToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts the  CDP Version string to a <see cref="Visibility"/> value, either <see cref="Visibility.Visible"/> or <see cref="Visibility.Collapsed"/>
        /// </summary>
        /// <param name="value">an instance of <see cref="IISession"/></param>
        /// <param name="targetType">The targetType is not used</param>
        /// <param name="parameter">The name of the property that is used to determine <see cref="Visibility"/> of the control</param>
        /// <param name="culture">The culture is not used</param>
        /// <returns>The <see cref="Visibility.Visible"/> based on the provided CDPVersion</returns>
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
                return Visibility.Visible;
            }
            
            var versionAttribute = (CDPVersionAttribute)property.GetCustomAttributes(typeof(CDPVersionAttribute), true)[0];
            var propertyVersion = new Version(versionAttribute.Version);
            
            var comparision = propertyVersion.CompareTo(dalVersion);

            return comparision > 0 ? Visibility.Collapsed : Visibility.Visible;
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
