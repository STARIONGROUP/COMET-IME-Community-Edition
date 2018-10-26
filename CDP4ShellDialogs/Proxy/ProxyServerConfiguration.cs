// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProxyServerConfiguration.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ShellDialogs.Proxy
{
    /// <summary>
    /// settings class to serialize and deserialize the web-proxy server settings
    /// </summary>
    internal struct ProxyServerConfiguration
    {
        /// <summary>
        /// Gets or sets the Address
        /// </summary>
        internal string Address { get; set; }

        /// <summary>
        /// Gets or sets the Port
        /// </summary>
        internal string Port { get; set; } 

        /// <summary>
        /// Gets or sets the UserName
        /// </summary>
        internal string UserName { get; set; }

        /// <summary>
        /// Gets or sets the Password
        /// </summary>
        internal string Password { get; set; }
    }
}