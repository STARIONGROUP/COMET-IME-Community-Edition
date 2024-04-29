// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProxyServerConfigurationManager.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2018 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ShellDialogs.Proxy
{
    using System;
    using System.Security.Cryptography;
    using System.Text;
    using Microsoft.Win32;
    using NLog;
    
    /// <summary>
    /// The purpose of the <see cref="ProxyServerConfigurationManager"/> is to store and
    /// retrieve the <see cref="ProxyServerConfiguration"/> to and from the Registry
    /// </summary>
    internal static class ProxyServerConfigurationManager
    {
        /// <summary>
        /// the web-proxy configuration registry key path
        /// </summary>
        private const string RegistryPath = @"SOFTWARE\STARION\CDP4\ProxyServerConfiguration";

        /// <summary>
        /// constant used as key for the port value
        /// </summary>
        private const string Port = "port";

        /// <summary>
        /// constant used as key for the address value
        /// </summary>
        private const string Address = "address";

        /// <summary>
        /// constant used as key for the userName value
        /// </summary>
        private const string UserName = "userName";

        /// <summary>
        /// constant used as key for the password value
        /// </summary>
        private const string Password = "password";
        
        /// <summary>
        /// The NLog logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// loads the proxy server settings from the registry
        /// </summary>
        internal static ProxyServerConfiguration Read()
        {
            var proxyServerConfiguration = new ProxyServerConfiguration();
            
            var ProxyServerConfigurationKey = Registry.CurrentUser.OpenSubKey(RegistryPath);

            if (ProxyServerConfigurationKey != null)
            {
                proxyServerConfiguration.Address = ProxyServerConfigurationKey.GetValue(Address).ToString();
                proxyServerConfiguration.Port = ProxyServerConfigurationKey.GetValue(Port).ToString();
                proxyServerConfiguration.UserName = ProxyServerConfigurationKey.GetValue(UserName).ToString();
                proxyServerConfiguration.Password = DecryptString(ProxyServerConfigurationKey.GetValue(Password).ToString());
                
                ProxyServerConfigurationKey.Close();
            }
            else
            {
                proxyServerConfiguration.Address = "proxy.cdp4.org";
                proxyServerConfiguration.Port = "8888";
                proxyServerConfiguration.UserName = null;
                proxyServerConfiguration.Password = null;
            }

            return proxyServerConfiguration;
        }

        /// <summary>
        /// Writes the <see cref="ProxyServerConfiguration"/> to the registry
        /// </summary>
        /// <param name="proxyServerConfiguration">
        /// The <see cref="ProxyServerConfiguration"/> to write.
        /// </param>
        internal static void Write(ProxyServerConfiguration proxyServerConfiguration)
        {
            var ProxyServerConfigurationKey = Registry.CurrentUser.CreateSubKey(RegistryPath);

            ProxyServerConfigurationKey.SetValue(Address, proxyServerConfiguration.Address);
            ProxyServerConfigurationKey.SetValue(Port, proxyServerConfiguration.Port);
            ProxyServerConfigurationKey.SetValue(UserName, proxyServerConfiguration.UserName ?? string.Empty);
            
            var encryptedPassword = EncryptString(proxyServerConfiguration.Password ?? string.Empty);
            ProxyServerConfigurationKey.SetValue(Password, encryptedPassword);

            ProxyServerConfigurationKey.Close();
        }

        /// <summary>
        /// Encrypts the provided string
        /// </summary>
        /// <param name="noSecret">
        /// the string that is to be converted into a secret
        /// </param>
        /// <returns>
        /// the encrypted string
        /// </returns>
        private static string EncryptString(string noSecret)
        {
            var bytes = Encoding.UTF8.GetBytes(noSecret);
            var encryptedBytes = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encryptedBytes);
        }

        /// <summary>
        /// decrypts the provided secret
        /// </summary>
        /// <param name="secret">
        /// the secret that is to be decrypted
        /// </param>
        /// <returns>
        /// a decrypted string
        /// </returns>
        private static string DecryptString(string secret)
        {
            try
            {
                var encryptedBytes = Convert.FromBase64String(secret);
                var decryptedBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(decryptedBytes, 0, decryptedBytes.Length);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return string.Empty;
        }
    }
}