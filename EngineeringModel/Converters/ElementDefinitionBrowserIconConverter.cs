// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementDefinitionBrowserIconConverter.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Converters
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Data;
    using System.Windows.Media.Imaging;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Helpers;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition;

    /// <summary>
    /// The element definition browser icon converter
    /// </summary>
    public class ElementDefinitionBrowserIconConverter : IMultiValueConverter
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
            var genericConverter = new ThingToIconUriConverter();
            var parameterBase = value.OfType<ParameterBase>().SingleOrDefault();

            ClassKind valuesetRowType;
            if (parameterBase == null || parameter == null || !Enum.TryParse(parameter.ToString(), out valuesetRowType))
            {
                return genericConverter.Convert(value, targetType, parameter, culture);
            }

            var isCompound = parameterBase.ParameterType is CompoundParameterType;

            // Value set row
            // row representing an option
            if (valuesetRowType == ClassKind.Option)
            {
                var optionUri = new Uri(IconUtilities.ImageUri(ClassKind.Option).ToString());
                if (parameterBase.StateDependence != null || isCompound)
                {
                    return new BitmapImage(optionUri);
                }
                else
                {
                    var uri = new Uri(IconUtilities.ImageUri(parameterBase.ClassKind).ToString());
                    return IconUtilities.WithOverlay(uri, optionUri);
                }
            }

            // Row representing state
            var stateUri = new Uri(IconUtilities.ImageUri(ClassKind.ActualFiniteState).ToString());
            if (isCompound)
            {
                return new BitmapImage(stateUri);
            }

            var baseUri = new Uri(IconUtilities.ImageUri(parameterBase.ClassKind).ToString());
            return IconUtilities.WithOverlay(baseUri, stateUri);
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