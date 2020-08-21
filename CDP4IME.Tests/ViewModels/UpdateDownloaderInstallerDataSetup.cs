// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PluginUpdateDataSetup.cs" company="RHEA System S.A.">
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

    using CDP4Composition.Modularity;

    using CDP4IME.Services;

    using DevExpress.Mvvm.Native;

    using Newtonsoft.Json;

    using NUnit.Framework;

    /// <summary>
    /// Base class for unit test related to Plugin installation 
    /// </summary>
    public class UpdateDownloaderInstallerDataSetup
    {
        private const string TestContent = "TESTCONTENT";
        private const string TestFileName = "test";

        protected (FileInfo, Manifest) Plugin;

        protected Manifest Manifest;
        protected IUpdateFileSystemService UpdateFileSystem;
        protected string BasePath;
        protected string TempPath;
        protected string InstallPath;
        protected string DownloadPath;

        /// <summary>
        /// Base setup wich sets up the file environnement
        /// </summary>
        public virtual void Setup()
        {
            this.BasePath = Path.Combine(Path.GetTempPath(), "UpdateTestFixture", Guid.NewGuid().ToString());
            this.TempPath = Path.Combine(this.BasePath, "Temp");
            this.InstallPath = Path.Combine(this.BasePath, "Plugins");
            this.DownloadPath = Path.Combine(this.BasePath, "Download");

            if (!Directory.Exists(this.DownloadPath))
            {
                Directory.CreateDirectory(this.DownloadPath);
            }
            
            var dataPath = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.TestDirectory, "ViewModels/PluginMockData/"));

            var testPluginDownloadPath = new DirectoryInfo(Path.Combine(this.DownloadPath, "plugins", "CDP4BasicRdl"));
            
            if (!testPluginDownloadPath.Exists)
            {
                testPluginDownloadPath.Create();
            }

            foreach (var file in dataPath.EnumerateFiles())
            {
                File.Copy(file.FullName, Path.Combine(testPluginDownloadPath.FullName, file.Name), true);
            }

            if (!Directory.Exists(this.InstallPath))
            {
                Directory.CreateDirectory(this.InstallPath);
            }

            if (!Directory.Exists(this.TempPath))
            {
                Directory.CreateDirectory(this.TempPath);
            }

            this.Manifest = new Manifest
            {
                Name = "Name",
                Description = "Description",
                Author = "Author",
                Website = "Website",
                Version = "Version",
                ReleaseNote = "ReleaseNote"
            };
            
            var cdp4Ck = new FileInfo(Path.Combine(testPluginDownloadPath.FullName, "CDP4BasicRdl.cdp4ck"));
            this.Manifest = JsonConvert.DeserializeObject<Manifest>(File.ReadAllText(Path.Combine(testPluginDownloadPath.FullName, "CDP4BasicRdl.plugin.manifest")));
            var imeDownloadPath = new DirectoryInfo(Path.Combine(this.DownloadPath, "ime"));

            if (!imeDownloadPath.Exists)
            {
                imeDownloadPath.Create();
                imeDownloadPath.Refresh();
            }
            
            this.UpdateFileSystem = new UpdateFileSystemService((cdp4Ck, this.Manifest))
            {
                TemporaryPath = new DirectoryInfo(Path.Combine(this.TempPath, this.Manifest.Name)),
                InstallationPath = new DirectoryInfo(Path.Combine(this.InstallPath, this.Manifest.Name)),
                ImeDownloadPath = imeDownloadPath,
                PluginDownloadPath = new DirectoryInfo(Path.Combine(this.DownloadPath, "plugins"))
            };

            this.Plugin = (new FileInfo("test"), this.Manifest);
        }

        /// <summary>
        /// Create a test file in the <see cref="destinationPathFullName"/> path
        /// </summary>
        /// <param name="destinationPathFullName">the path where to put the test file</param>
        protected void SetupTestContentForInstallationCancellationPurpose(string destinationPathFullName)
        {
            this.UpdateFileSystem.TemporaryPath.Create();
            this.UpdateFileSystem.InstallationPath.Create();

            var testFileFullName = Path.Combine(destinationPathFullName, TestFileName);

            using var testFileWriter = File.CreateText(testFileFullName);
            testFileWriter.Write(TestContent);
        }

        /// <summary>
        /// Asserts that the file create by <see cref="SetupTestContentForInstallationCancellationPurpose"/> is at the intented location
        /// </summary>
        protected void AssertInstalledTestFileHasBeenRestored()
        {
            var restoredFile = new FileInfo(Path.Combine(this.UpdateFileSystem.InstallationPath.FullName, TestFileName));
            Assert.IsTrue(restoredFile.Exists);
            Assert.AreEqual(File.ReadAllText(restoredFile.FullName), TestContent);
        }
    }
}
