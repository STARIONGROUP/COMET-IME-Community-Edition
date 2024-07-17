// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UriConfig.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Utilities
{
    /// <summary>
    /// The purpose of the <see cref="UriConfig"/> class is to represent an uri configuration entry
    /// </summary>
    public class UriConfig
    {
        /// <summary>
        /// Gets or sets the <see cref="Alias"/> of the uri
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Uri"/> of the uri
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DalType"/> of the uri
        /// </summary>
        public string DalType { get; set; }
    }
}