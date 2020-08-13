// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PluginRowViewModelTestFixture.cs" company="RHEA System S.A.">
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
    using System;
    using System.IO;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Threading;

    using CDP4Composition.Modularity;
    using CDP4Composition.ViewModels;

    using CDP4IME.ViewModels;

    using Newtonsoft.Json;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    public class RowViewModelTestFixture : UpdateDownloaderInstallerDataSetup
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
            if (Directory.Exists(this.BasePath))
            {
                File.SetAttributes(this.BasePath, FileAttributes.Normal);
                Directory.Delete(this.BasePath, true);
            }
        }

        [Test]
        public void VerifyProperties()
        {
            var viewModel = new PluginRowViewModel(this.Plugin);

            Assert.AreEqual(viewModel.Name, this.Manifest.Name);
            Assert.AreEqual(viewModel.Version, $"version {this.Manifest.Version}");
            Assert.AreEqual(viewModel.Description, this.Manifest.Description);
            Assert.AreEqual(viewModel.Author, this.Manifest.Author);
            Assert.AreEqual(viewModel.ReleaseNote, this.Manifest.ReleaseNote);

            Assert.IsNotNull(viewModel.FileSystem.UpdateCdp4CkFileInfo);

            Assert.IsNotNull(viewModel.FileSystem.InstallationPath);

            Assert.IsNotNull(viewModel.FileSystem.TemporaryPath);
            Assert.IsTrue(viewModel.FileSystem.TemporaryPath.FullName.Contains("Temp"));
        }

        [Test]
        public void VerifyInstallation()
        {
            var viewModel = new PluginRowViewModel(this.Plugin, this.PluginFileSystem);
            
            viewModel.Install(); 
            
            Assert.IsTrue(new DirectoryInfo(this.InstallPath).Exists);

            var filesEnumeration = this.PluginFileSystem.InstallationPath.EnumerateFiles("*", SearchOption.AllDirectories);
            Assert.AreEqual(5, filesEnumeration.Count());
            
            var newVersionManifest = JsonConvert.DeserializeObject<Manifest>(File.ReadAllText(Path.Combine(this.PluginFileSystem.InstallationPath.FullName, "CDP4BasicRdl.plugin.manifest")));
            Assert.AreEqual(new Version("5.5.5.5"), new Version(newVersionManifest.Version));
            Assert.AreEqual(this.Manifest.Description, newVersionManifest.Description);
            Thread.Sleep(1);
        }

        [Test]
        public void VerifyCancelation()
        {
            this.SetupTestContentForCancellationPurpose(this.PluginFileSystem.TemporaryPath.FullName);

            var viewModel = new PluginRowViewModel(this.Plugin, this.PluginFileSystem);
            viewModel.HandlingCancelation();

            this.AssertCreatedTestFileHasBeenRestored();
        }
    }
}
