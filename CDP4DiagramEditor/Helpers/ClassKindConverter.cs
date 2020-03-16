// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClassKindConverter.cs" company="RHEA System S.A.">
//   Copyright (c) 2017 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4DiagramEditor.Helpers
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using CDP4Common.CommonData;
    using CDP4Composition.Converters;

    /// <summary>
    /// The purpose of the <see cref="ClassKindConverter"/> is to convert the ClassKind to a 
    /// string including <![CDATA[<< and >> ]]> characters to make them appear like stereotypes in UML
    /// </summary>
    public class ClassKindConverter : IValueConverter
    {
        /// <summary>
        /// Converts the <see cref="ClassKind"/> into a split string and preprends and appends <![CDATA[<< >>]]>
        /// </summary>
        /// <param name="value">An instance of an object which needs to be converted.</param>
        /// <param name="targetType">The parameter is not used.</param>
        /// <param name="parameter">The parameter is not used.</param>
        /// <param name="culture">The parameter is not used.</param>
        /// <returns>
        /// A split string preprended and appended with <![CDATA[<< >>]]>
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var converter = new CamelCaseToSpaceConverter();
            return string.Format("<<{0}>>", converter.Convert(value, targetType, parameter, culture));
        }

        /// <summary>
        /// not supported
        /// </summary>
        /// <param name="value">The parameter is not used.</param>
        /// <param name="targetType">The parameter is not used.</param>
        /// <param name="parameter">The parameter is not used.</param>
        /// <param name="culture">The parameter is not used.</param>
        /// <returns>a <see cref="NotSupportedException"/> is thrown</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}