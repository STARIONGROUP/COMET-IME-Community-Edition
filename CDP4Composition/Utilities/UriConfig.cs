// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UriConfig.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// <summary>
//   The purpose of the  is to represent a uri configuration entry
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

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
        /// Gets or sets the <see cref="ProxyUri"/> of the uri
        /// </summary>
        public string ProxyUri { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DalType"/> of the uri
        /// </summary>
        public string DalType { get; set; }
    }

}
