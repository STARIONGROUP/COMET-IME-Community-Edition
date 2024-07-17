// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Utils.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2018 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ShellDialogs
{
    using System.Text.RegularExpressions;

    /// <summary>
    /// Utilities class with string extension methods
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Asserts whether the provided ip is a valid IP V4
        /// </summary>
        /// <param name="ip">
        /// a string representation of an IP
        /// </param>
        /// <returns>
        /// true or false
        /// </returns>
        public static bool IsValidIp(this string ip)
        {
            if (ip == null)
            {
                return false;
            }

            var validIpAddressRegex = new Regex(@"((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)");
            if (validIpAddressRegex.Match(ip).Success)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Asserts whether the provided ip is a valid hostname
        /// </summary>
        /// <param name="ip">
        /// a string representation of an IP
        /// </param>
        /// <returns>
        /// true or false
        /// </returns>
        public static bool IsValidHostName(this string hostname)
        {
            if (hostname == null)
            {
                return false;
            }

            var validHostnameRegex = new Regex(@"^(([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\-]*[a-zA-Z0-9])\.)*([A-Za-z0-9]|[A-Za-z0-9][A-Za-z0-9\-]*[A-Za-z0-9])$");
            if (validHostnameRegex.Match(hostname).Success)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Asserts whether the provided port is a valid port
        /// </summary>
        /// <param name="port">
        /// a string representation of an Port
        /// </param>
        /// <returns>
        /// true or false
        /// </returns>
        public static bool IsValidPort(this string port)
        {
            if (port == null)
            {
                return false;
            }

            var p = 0;
            if (int.TryParse(port, out p))
            {
                if (p >= 1 && p <= 65535)
                {
                    return true;
                }
            }
            
            return false;
        }
    }
}