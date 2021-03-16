// -------------------------------------------------------------------------------------------------
// <copyright file="LogLevelImageConverter.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4LogInfo.Views
{
    using System;
    using System.Globalization;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    using DevExpress.Xpf.Core;
    using DevExpress.Xpf.Core.Native;
    using DevExpress.Xpf.Editors;

    /// <summary>
    /// The Image Converter that returns an icon from the logLevel
    /// </summary>
    public class LogLevelImageConverter : IValueConverter
    {
        /// <summary>
        /// Convert from the LogLevel to an Image
        /// </summary>
        /// <param name="value">The <see cref="LogLevel"/></param>
        /// <param name="targetType">The <see cref="ImageSource"/></param>
        /// <param name="parameter">The Parameters (null)</param>
        /// <param name="culture">The <see cref="CultureInfo"/></param>
        /// <returns>The Icon associated with the <see cref="LogLevel"/></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var logLevel = value.ToString();
            switch (logLevel)
            {
                case "Fatal":
                    return new DXImageExtension { Image = new DXImageConverter().ConvertFrom("Cancel_16x16.png") as DXImageInfo }.ProvideValue(null);
                case "Error":
                    return "pack://application:,,,/CDP4Composition;component/Resources/Images/Log/Error_16x16.png";
                case "Warn":
                    return "pack://application:,,,/CDP4Composition;component/Resources/Images/Log/Warning_16x16.png";
                case "Info":
                    return "pack://application:,,,/CDP4Composition;component/Resources/Images/Log/Info_16x16.png";
                case "Debug":
                    return new DXImageExtension { Image = new DXImageConverter().ConvertFrom("BugReport_16x16.png") as DXImageInfo }.ProvideValue(null);
                case "Trace":
                    return new DXImageExtension { Image = new DXImageConverter().ConvertFrom("Windows_16x16.png") as DXImageInfo }.ProvideValue(null);
                default:
                    return "pack://application:,,,/CDP4Composition;component/Resources/Images/comet.ico";
            }
        }

        /// <summary>
        /// Convert Back, not implemented
        /// </summary>
        /// <param name="value">The Image</param>
        /// <param name="targetType">the loglevel</param>
        /// <param name="parameter">the parameters</param>
        /// <param name="culture">the culture info</param>
        /// <returns>the loglevel</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}