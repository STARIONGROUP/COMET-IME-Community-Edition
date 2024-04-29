// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CultureInfoUtility.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Utilities
{
    using System.Globalization;
    using System.Linq;
    using NLog;

    /// <summary>
    /// The utility class to retrieve <see cref="CultureInfo"/>
    /// </summary>
    public static class CultureInfoUtility
    {
        /// <summary>
        /// The default culture
        /// </summary>
        public const string DefaultCultureName = "en";

        /// <summary>
        /// Initializes the <see cref="CultureInfoUtility"/> static class
        /// </summary>
        static CultureInfoUtility()
        {
            CultureInfoAvailable = CultureInfo.GetCultures(CultureTypes.NeutralCultures).Where(x => !string.IsNullOrWhiteSpace(x.Name)).ToArray();
            DefaultCulture = CultureInfoAvailable.Single(c => c.Name == DefaultCultureName);
        }

        /// <summary>
        /// Gets the default language code for the application
        /// </summary>
        public static CultureInfo DefaultCulture { get; private set; }

        /// <summary>
        /// Gets the collection of <see cref="CultureInfo"/> available
        /// </summary>
        public static CultureInfo[] CultureInfoAvailable { get; private set; }
    }
}