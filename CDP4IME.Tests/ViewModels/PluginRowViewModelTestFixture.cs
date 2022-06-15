// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PluginRowViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
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

namespace COMET.Tests.ViewModels
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Threading;
    using System.Threading.Tasks;

    using CDP4Composition.Modularity;

    using COMET.ViewModels;

    using Newtonsoft.Json;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    public class PluginRowViewModelTestFixture : UpdateDownloaderInstallerDataSetup
    {
        [SetUp]
        public override void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            base.Setup();
        }

        [TearDown]
        public void Teardown()
        {
            try
            {
                if (Directory.Exists(this.BasePath))
                {
                    Directory.Delete(this.BasePath, true);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        [Test]
        public void VerifyProperties()
        {
            var viewModel = new PluginRowViewModel(this.Plugin);

            Assert.AreEqual(viewModel.Name, this.Manifest.Name);
            Assert.AreEqual(viewModel.Version, $"Version {this.Manifest.Version}");
            Assert.AreEqual(viewModel.Description, this.Manifest.Description);
            Assert.AreEqual(viewModel.Author, this.Manifest.Author);
            Assert.AreEqual(viewModel.ReleaseNote, this.Manifest.ReleaseNote);
            Assert.AreEqual(viewModel.Progress, 0);

            Assert.IsNotNull(viewModel.FileSystem.UpdateCdp4CkFileInfo);

            Assert.IsNotNull(viewModel.FileSystem.InstallationPath);

            Assert.IsNotNull(viewModel.FileSystem.TemporaryPath);
            Assert.IsTrue(viewModel.FileSystem.TemporaryPath.FullName.Contains("Temp"));
        }

        [Test]
        public async Task VerifyInstallation()
        {
            var viewModel = new PluginRowViewModel(this.Plugin, this.UpdateFileSystem);
            
            await viewModel.Install(new CancellationToken(false)); 
            
            Assert.IsTrue(new DirectoryInfo(this.InstallPath).Exists);

            var filesEnumeration = this.UpdateFileSystem.InstallationPath.EnumerateFiles("*", SearchOption.AllDirectories);
            Assert.AreEqual(5, filesEnumeration.Count());
            
            var newVersionManifest = JsonConvert.DeserializeObject<Manifest>(File.ReadAllText(Path.Combine(this.UpdateFileSystem.InstallationPath.FullName, "CDP4BasicRdl.plugin.manifest")));
            Assert.AreEqual(new Version("5.5.5.5"), new Version(newVersionManifest.Version));
            Assert.AreEqual(this.Manifest.Description, newVersionManifest.Description);
            Thread.Sleep(1);
        }

        [Test]
        public async Task VerifyCancelation()
        {
            if (!this.UpdateFileSystem.TemporaryPath.Exists)
            {
                this.UpdateFileSystem.TemporaryPath.Create();
                this.UpdateFileSystem.TemporaryPath.Refresh();
            }

            this.SetupTestContentForInstallationCancellationPurpose(this.UpdateFileSystem.TemporaryPath.FullName);

            var viewModel = new PluginRowViewModel(this.Plugin, this.UpdateFileSystem);
            await viewModel.HandlingCancelationOfInstallation();

            this.AssertInstalledTestFileHasBeenRestored();
        }
    }
}
