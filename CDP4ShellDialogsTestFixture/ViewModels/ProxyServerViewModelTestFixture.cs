// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProxyServerViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using CDP4ShellDialogs.Proxy;

namespace CDP4ShellDialogs.Tests.ViewModels
{
    using System;
    using CDP4ShellDialogs.ViewModels;
    using Microsoft.Win32;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="ProxyServerViewModelTestFixture"/> class.
    /// </summary>
    [TestFixture]
    public class ProxyServerViewModelTestFixture
    {
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
        public void Verify_that_the_ProxyServerViewModel_can_be_constructed()
        {
            var vm = new ProxyServerViewModel();

            Assert.AreEqual("proxy.cdp4.org", vm.Address);
            Assert.AreEqual("8888", vm.Port);
            Assert.IsNull(vm.UserName);
            Assert.IsNull(vm.Password);

            Assert.IsTrue(vm.OkCommand.CanExecute(null));

            Assert.IsTrue(vm.CancelCommand.CanExecute(null));
        }

        [Test]
        public void Verify_that_when_invalid_address_ok_not_enabled()
        {
            var vm = new ProxyServerViewModel();

            vm.Address = "%$#@";

            Assert.IsFalse(vm.OkCommand.CanExecute(null));

            vm.Address = "proxy.cdp4.org";

            Assert.IsTrue(vm.OkCommand.CanExecute(null));
        }

        [Test]
        public void Verify_that_when_invalid_port_ok_not_enabled()
        {
            var vm = new ProxyServerViewModel();
            
            vm.Port = "rtyuio";

            Assert.IsFalse(vm.OkCommand.CanExecute(null));

            vm.Port = "8080";

            Assert.IsTrue(vm.OkCommand.CanExecute(null));
        }

        [Test]
        public void Verify_that_when_ok_executed_registry_is_written_and_result_is_true()
        {
            var vm = new ProxyServerViewModel();
            vm.Address = "192.168.0.100";
            vm.Port = "1234";
            vm.UserName = "John";
            vm.Password = "Doe";

            vm.OkCommand.Execute(null);

            Assert.AreEqual(true, vm.DialogResult.Result.Value);

            var proxyServerConfiguration = ProxyServerConfigurationManager.Read();

            Assert.AreEqual("192.168.0.100", proxyServerConfiguration.Address);
            Assert.AreEqual("1234", proxyServerConfiguration.Port);
            Assert.AreEqual("John", proxyServerConfiguration.UserName);
            Assert.AreEqual("Doe", proxyServerConfiguration.Password);
        }

        [Test]
        public void Verify_that_when_cancel_executed_registry_is_not_written_and_result_is_false()
        {
            var vm = new ProxyServerViewModel();
            vm.Address = "test.cdp4.org";
            vm.Port = "1234";
            vm.UserName = "John";
            vm.Password = "Doe";

            vm.CancelCommand.Execute(null);

            Assert.AreEqual(false, vm.DialogResult.Result.Value);

            var proxyServerConfiguration = ProxyServerConfigurationManager.Read();

            Assert.AreNotEqual("test.cdp4.org", proxyServerConfiguration.Address);
            Assert.AreNotEqual("1234", proxyServerConfiguration.Port);
            Assert.AreNotEqual("John", proxyServerConfiguration.UserName);
            Assert.AreNotEqual("Doe", proxyServerConfiguration.Password);
        }
    }
}