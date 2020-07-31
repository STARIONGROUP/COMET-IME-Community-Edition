// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PluginInstallerViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Kamil Wojnowski
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4IME.Tests.ViewModels
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Threading;
    using System.Threading.Tasks;

    using CDP4Composition.Modularity;
    using CDP4Composition.ViewModels;

    using CDP4IME.Behaviors;
    using CDP4IME.ViewModels;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture, Apartment(ApartmentState.STA)]
    public class PluginInstallerViewModelTestFixture : PluginUpdateDataSetup
    {
        private IEnumerable<(FileInfo cdp4ckFile, Manifest manifest)> updatablePlugins;
        private Mock<IPluginInstallerBehavior> behavior;
        private PluginInstallerViewModel viewModel;

        [SetUp]
        public override void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            base.Setup();

            this.updatablePlugins = new List<(FileInfo cdp4ckFile, Manifest manifest)>()
            {
                this.Plugin
            };

            this.behavior = new Mock<IPluginInstallerBehavior>();
            this.behavior.Setup(x => x.Close());

            this.viewModel = new PluginInstallerViewModel(this.updatablePlugins);
        }
        
        [TearDown]
        public void Teardown()
        {
            if (Directory.Exists(this.BasePath))
            {
                File.SetAttributes(this.BasePath, FileAttributes.Normal);
                Directory.Delete(this.BasePath, true);
            }
        }

        [Test]
        public void VerifyPropertiesAreSet()
        {
            var vm = new PluginInstallerViewModel(this.updatablePlugins);
            Assert.IsNotEmpty(vm.AvailablePlugins);
            Assert.AreSame(vm.UpdatablePlugins, this.updatablePlugins);
            Assert.IsFalse(vm.IsInstallationInProgress);
            Assert.IsNull(vm.CancellationTokenSource);
        }
        
        [Test]
        public void VerifySelectAllUpdateCheckBoxCommand()
        {
            var vm = new PluginInstallerViewModel(this.updatablePlugins);
            Assert.IsTrue(vm.SelectAllUpdateCheckBoxCommand.CanExecute(null));

            vm.SelectAllUpdateCheckBoxCommand.Execute(null);
            Assert.IsTrue(vm.AvailablePlugins.All(p => p.IsSelectedForInstallation));

            vm.SelectAllUpdateCheckBoxCommand.Execute(null);
            Assert.IsTrue(vm.AvailablePlugins.All(p => !p.IsSelectedForInstallation));
        }
        
        [Test]
        public async Task VerifyCancelCommand()
        {
            this.viewModel.Behavior = this.behavior.Object;

            this.viewModel.IsInstallationInProgress = true;
            Assert.IsTrue(this.viewModel.CancelCommand.CanExecute(null));
            await this.viewModel.CancelCommand.ExecuteAsyncTask(null);
            this.behavior.Verify(x => x.Close(), Times.Once);
            this.viewModel.IsInstallationInProgress = false;
        }

        [Test]
        public async Task VerifyInstallCommand()
        {
            this.viewModel.Behavior = this.behavior.Object;
            this.viewModel.AvailablePlugins.First().FileSystem = this.PluginFileSystem;
            Assert.IsTrue(this.viewModel.InstallCommand.CanExecute(null));
            this.viewModel.AvailablePlugins.First().IsSelectedForInstallation = false;
            await this.viewModel.InstallCommand.ExecuteAsyncTask(null);
            this.behavior.Verify(x => x.Close(), Times.Never);

            this.viewModel.AvailablePlugins.First().IsSelectedForInstallation = true;
            await this.viewModel.InstallCommand.ExecuteAsyncTask(null);
        }
        
        [Test]
        public async Task VerifyFailingInstallCommand()
        {
            this.PluginFileSystem.UpdateCdp4CkFileInfo.Delete();
            this.viewModel.Behavior = this.behavior.Object;
            this.viewModel.AvailablePlugins.First().FileSystem = this.PluginFileSystem;
            Assert.IsTrue(this.viewModel.InstallCommand.CanExecute(null));
            this.viewModel.AvailablePlugins.First().IsSelectedForInstallation = true;
            await this.viewModel.InstallCommand.ExecuteAsyncTask(null);
            this.behavior.Verify(x => x.Close(), Times.Never);
        }

        /// <summary>
        /// This test is creating a large file (100MB) so it can have a chance to Invoke the cancellation on the CancellationToken
        /// The Task may fail on some system.
        /// </summary>
        /// <returns>The Task may fail on some system</returns>
        [Test]
        public async Task VerifyCancellationToken()
        {
            File.Delete(this.PluginFileSystem.UpdateCdp4CkFileInfo.FullName);

            this.SetupTestContentForCancellationPurpose(this.PluginFileSystem.InstallationPath.FullName);

            using (var largeFile = new FileStream(this.PluginFileSystem.UpdateCdp4CkFileInfo.FullName, FileMode.CreateNew))
            {
                largeFile.Seek(100L * 1024 * 1024, SeekOrigin.Begin);
                largeFile.WriteByte(0);
                largeFile.Close();
            }

            this.viewModel.Behavior = this.behavior.Object;
            this.viewModel.AvailablePlugins.First().FileSystem = this.PluginFileSystem;
            Assert.IsTrue(this.viewModel.InstallCommand.CanExecute(null));
            this.viewModel.AvailablePlugins.First().IsSelectedForInstallation = true;

            await Task.WhenAll(
                this.viewModel.InstallCommand.ExecuteAsyncTask(null),
                Task.Run(() =>
                {
                    Task.Delay(1);
                    return this.viewModel.CancelCommand.ExecuteAsyncTask(null);
                }));

            this.AssertCreatedTestFileHasBeenRestored();
        }
    }
}
