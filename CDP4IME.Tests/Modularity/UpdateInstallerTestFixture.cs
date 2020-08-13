// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UpdateInstallerTestFixture.cs" company="RHEA System S.A.">
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

namespace CDP4IME.Tests.Modularity
{
    using System;
    using System.IO;
    using System.Threading;

    using CDP4IME.Modularity;
    using CDP4IME.Services;
    using CDP4IME.Views;

    using Moq;

    using NUnit.Framework;

    [TestFixture, Apartment(ApartmentState.STA)]
    public class UpdateInstallerTestFixture
    {
        private Mock<IPluginInstallerViewInvokerService> viewInvoker;
        private string downloadPath;

        [SetUp]
        public void Setup()
        {
            this.viewInvoker = new Mock<IPluginInstallerViewInvokerService>();
            this.viewInvoker.Setup(x => x.ShowDialog(It.IsAny<UpdateDownloaderInstaller>()));

            var appData = Environment.GetFolderPath(folder: Environment.SpecialFolder.ApplicationData);
            this.downloadPath = Path.Combine(path1: appData, path2: "RHEA/CDP4/DownloadCache/plugins");

            if (!Directory.Exists(this.downloadPath))
            {
                Directory.CreateDirectory(this.downloadPath);
            }
        }

        [TearDown]
        public void Teardown()
        {
            if (Directory.Exists(this.downloadPath))
            {
                Directory.Delete(this.downloadPath, true);
            }
        }

        [Test]
        public void VerifyCheckAndInstall()
        {
            UpdateInstaller.CheckAndInstall(this.viewInvoker.Object);
            this.viewInvoker.Verify(x => x.ShowDialog(It.IsAny<UpdateDownloaderInstaller>()), Times.Never);
            
            var dataPath = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.TestDirectory, "ViewModels/PluginMockData/"));

            foreach (var file in dataPath.EnumerateFiles())
            {
                var destination = Path.Combine(this.downloadPath, Path.GetFileNameWithoutExtension(file.Name));

                if (!Directory.Exists(destination))
                {
                    Directory.CreateDirectory(destination);
                }
                
                File.Copy(file.FullName, Path.Combine(destination, file.Name), true);
            }

            UpdateInstaller.CheckAndInstall(this.viewInvoker.Object);
            this.viewInvoker.Verify(x => x.ShowDialog(It.IsAny<UpdateDownloaderInstaller>()), Times.Once);
        }
    }
}
