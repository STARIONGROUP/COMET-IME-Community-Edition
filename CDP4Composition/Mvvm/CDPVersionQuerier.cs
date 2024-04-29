// ------------------------------------------------------------------------------------------------
// <copyright file="IVersioned.cs" company="Starion Group S.A.">
//   Copyright (c) 2016 Starion Group S.A.
// </copyright>
// -----------------------------------------------------------------------------------------------

namespace CDP4Composition.Mvvm
{
    using System;
    using CDP4Common;

    /// <summary>
    /// The purpose of the <see cref="CDPVersionQuerier"/> is to query an <see cref="IVersioned"/> instance for
    /// it's <see cref="CDPVersionAttribute"/> and return a <see cref="Version"/> instance
    /// </summary>
    public static class CDPVersionQuerier
    {
        /// <summary>
        /// Queries the <see cref="CDPVersionAttribute"/> and returns a corresponding <see cref="Version"/> 
        /// </summary>
        /// <returns>
        /// An instance of <see cref="Version"/>
        /// </returns>
        /// <remarks>
        /// In case the <see cref="IVersioned"/> <paramref name="versioned"/> is not decorated with a <see cref="CDPVersionAttribute"/>
        /// the default <see cref="Version"/> is returned that
        /// </remarks>
        public static Version QueryCdpVersion(this object versioned)
        {
            var viewModelType = versioned.GetType();
            var dalExportAttribute = (CDPVersionAttribute)Attribute.GetCustomAttribute(viewModelType, typeof(CDPVersionAttribute));
            if (dalExportAttribute != null)
            {
                var version = dalExportAttribute.Version;
                return new Version(version);
            }

            return new Version("1.0.0");
        }
    }
}
