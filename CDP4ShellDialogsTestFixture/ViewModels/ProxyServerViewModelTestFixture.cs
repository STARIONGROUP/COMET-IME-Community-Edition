// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProxyServerViewModelTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2022 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using CDP4ShellDialogs.Proxy;

namespace CDP4ShellDialogs.Tests.ViewModels
{
    using System;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;

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
                Registry.CurrentUser.DeleteSubKeyTree(@"SOFTWARE\STARION\CDP4\ProxyServerConfiguration");
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
                Registry.CurrentUser.DeleteSubKeyTree(@"SOFTWARE\STARION\CDP4\ProxyServerConfiguration");
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

            Assert.IsTrue(((ICommand)vm.OkCommand).CanExecute(null));

            Assert.IsTrue(((ICommand)vm.CancelCommand).CanExecute(null));
        }

        [Test]
        public void Verify_that_when_invalid_address_ok_not_enabled()
        {
            var vm = new ProxyServerViewModel();

            vm.Address = "%$#@";

            Assert.IsFalse(((ICommand)vm.OkCommand).CanExecute(null));

            vm.Address = "proxy.cdp4.org";

            Assert.IsTrue(((ICommand)vm.OkCommand).CanExecute(null));
        }

        [Test]
        public void Verify_that_when_invalid_port_ok_not_enabled()
        {
            var vm = new ProxyServerViewModel();
            
            vm.Port = "rtyuio";

            Assert.IsFalse(((ICommand)vm.OkCommand).CanExecute(null));

            vm.Port = "8080";

            Assert.IsTrue(((ICommand)vm.OkCommand).CanExecute(null));
        }

        [Test]
        public async Task Verify_that_when_ok_executed_registry_is_written_and_result_is_true()
        {
            var vm = new ProxyServerViewModel();
            vm.Address = "192.168.0.100";
            vm.Port = "1234";
            vm.UserName = "John";
            vm.Password = "Doe";

            await vm.OkCommand.Execute();

            Assert.AreEqual(true, vm.DialogResult.Result.Value);

            var proxyServerConfiguration = ProxyServerConfigurationManager.Read();

            Assert.AreEqual("192.168.0.100", proxyServerConfiguration.Address);
            Assert.AreEqual("1234", proxyServerConfiguration.Port);
            Assert.AreEqual("John", proxyServerConfiguration.UserName);
            Assert.AreEqual("Doe", proxyServerConfiguration.Password);
        }

        [Test]
        public async Task Verify_that_when_cancel_executed_registry_is_not_written_and_result_is_false()
        {
            var vm = new ProxyServerViewModel();
            vm.Address = "test.cdp4.org";
            vm.Port = "1234";
            vm.UserName = "John";
            vm.Password = "Doe";

            await vm.CancelCommand.Execute();

            Assert.AreEqual(false, vm.DialogResult.Result.Value);

            var proxyServerConfiguration = ProxyServerConfigurationManager.Read();

            Assert.AreNotEqual("test.cdp4.org", proxyServerConfiguration.Address);
            Assert.AreNotEqual("1234", proxyServerConfiguration.Port);
            Assert.AreNotEqual("John", proxyServerConfiguration.UserName);
            Assert.AreNotEqual("Doe", proxyServerConfiguration.Password);
        }
    }
}