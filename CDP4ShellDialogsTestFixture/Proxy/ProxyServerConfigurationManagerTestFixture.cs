// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProxyServerConfigurationManagerTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ShellDialogs.Tests.Proxy
{
    using System;
    using CDP4ShellDialogs.Proxy;
    using Microsoft.Win32;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="ProxyServerConfigurationManager"/> class
    /// </summary>
    [TestFixture]
    public class ProxyServerConfigurationManagerTestFixture
    {
        private ProxyServerConfiguration proxyServerConfiguration;

        [SetUp]
        public void SetUp()
        {
            try
            {
                Registry.CurrentUser.DeleteSubKeyTree(@"SOFTWARE\RHEA\CDP4\ProxyServerConfiguration");
            }
            catch (Exception e)
            {
            }
            
            this.proxyServerConfiguration = new ProxyServerConfiguration()
            {
                Address = "proxy.proxy.com",
                Port = "8080",
                UserName = "John",
                Password = "Doe"
            };
        }

        [TearDown]
        public void TearDown()
        {
            try
            {
                Registry.CurrentUser.DeleteSubKeyTree(@"SOFTWARE\RHEA\CDP4\ProxyServerConfiguration");
            }
            catch (Exception e)
            {
            }
        }

        [Test]
        public void Verify_that_proxysettings_can_be_written_and_read()
        {
            ProxyServerConfigurationManager.Write(this.proxyServerConfiguration);

            var result = ProxyServerConfigurationManager.Read();

            Assert.AreEqual(this.proxyServerConfiguration.Address, result.Address);
            Assert.AreEqual(this.proxyServerConfiguration.Port, result.Port);
            Assert.AreEqual(this.proxyServerConfiguration.UserName, result.UserName);
            Assert.AreEqual(this.proxyServerConfiguration.Password, result.Password);
        }

        [Test]
        public void Verify_that_when_registry_key_does_not_exist_defaultsettings_are_returned()
        {
            var result = ProxyServerConfigurationManager.Read();

            Assert.AreEqual("proxy.cdp4.org", result.Address);
            Assert.AreEqual("8888", result.Port);
            Assert.IsNull(result.UserName);
            Assert.IsNull(result.Password);
        }
    }
}