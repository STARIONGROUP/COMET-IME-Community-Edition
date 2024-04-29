// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AppliedTheme.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition
{
    using DevExpress.Xpf.Core;

    /// <summary>
    /// The purpose of the <see cref="AppliedTheme"/> class is to provide
    /// access to the Theme name that is applied to the application.
    /// </summary>
    public static class AppliedTheme
    {
        /// <summary>
        /// The default Theme name
        /// </summary>
        private const string DefaultThemeName = "Seven";

        /// <summary>
        /// backing field for the <see cref="ThemeName"/> property.
        /// </summary>
        private static string themeName;

        /// <summary>
        /// Gets or sets the Theme Name that is applied to the whole application
        /// </summary>
        public static string ThemeName 
        {
            get
            {
                return themeName ?? DefaultThemeName;
            }

            set
            {
                themeName = ValidateTheme(value);
            }
        }

        /// <summary>
        /// validates the theme name, if it is invalid the default theme "seven" is returned
        /// </summary>
        /// <param name="name">
        /// the name of the theme
        /// </param>
        /// <returns>
        /// the name of the validated theme
        /// </returns>
        private static string ValidateTheme(string name)
        {
            switch (name)
            {
                case Theme.DXStyleName:
                case Theme.DeepBlueName:
                case Theme.HybridAppName:
                case Theme.LightGrayName:
                case Theme.MetropolisDarkName:
                case Theme.MetropolisLightName:
                case Theme.Office2007BlackName:
                case Theme.Office2007BlueName:
                case Theme.Office2007SilverName:
                case Theme.Office2013DarkGrayName:
                case Theme.Office2013LightGrayName:
                case Theme.Office2013LightGrayTouchName:
                case Theme.Office2013Name:
                case Theme.Office2013TouchName:
                case Theme.SevenName:
                case Theme.TouchlineDarkName:
                case Theme.VS2010Name:                    
                    return name;
                default:
                    return DefaultThemeName;
            }
        }
    }
}
