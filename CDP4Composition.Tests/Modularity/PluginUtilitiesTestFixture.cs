// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PluginUtilitiesTestFixture.cs" company="Starion Group S.A.">
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

namespace CDP4Composition.Tests.Modularity
{
    using System.IO;
    using System.Reflection;

    using CDP4Composition.Modularity;
    using CDP4Composition.Services.AppSettingService;
    using CDP4Composition.Utilities;

    using CommonServiceLocator;

    using Moq;

    using Newtonsoft.Json;

    using NUnit.Framework;

    [TestFixture]
    public class PluginUtilitiesTestFixture
    {
        private Mock<IServiceLocator> serviceLocator;

        private Mock<IAppSettingsService<TestAppSettings>> appSettingsService;
        private TestAppSettings appSettings;
        private Mock<IAssemblyInformationService> assemblyLocationLoader;

        private string BuildFolder;
        private const string AppSettingsJson = "AppSettingsTest.json";
        private const string PluginName = "CDP4ServicesDal";

        [SetUp]
        public void Setup()
        {
#if DEBUG
            this.BuildFolder = $"CDP4IME{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}Debug";
#else
            this.BuildFolder = $"CDP4ServicesDal{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}Release";
#endif

            var frameworkVersion = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).Name;
            var testDirectory = Path.Combine(Assembly.GetExecutingAssembly().Location, $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}");
            testDirectory = Path.GetFullPath(Path.Combine(testDirectory, $"{this.BuildFolder}{Path.DirectorySeparatorChar}{frameworkVersion}"));

            this.assemblyLocationLoader = new Mock<IAssemblyInformationService>();
            this.assemblyLocationLoader.Setup(x => x.GetLocation()).Returns(testDirectory);

            this.serviceLocator = new Mock<IServiceLocator>();
            this.serviceLocator.Setup(s => s.GetInstance<IAssemblyInformationService>()).Returns(this.assemblyLocationLoader.Object);

            this.appSettingsService = new Mock<IAppSettingsService<TestAppSettings>>();

            this.appSettings = JsonConvert.DeserializeObject<TestAppSettings>(File.ReadAllText(Path.Combine(Assembly.GetExecutingAssembly().Location, $"..{Path.DirectorySeparatorChar}Modularity{Path.DirectorySeparatorChar}", AppSettingsJson)));
            this.appSettingsService.Setup(x => x.AppSettings).Returns(this.appSettings);

            this.serviceLocator.Setup(x => x.GetInstance<IAppSettingsService<TestAppSettings>>())
                .Returns(this.appSettingsService.Object);

            Directory.SetCurrentDirectory(testDirectory);
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
        }

        [Test]
        public void VerifyImeManifestsAreReturned()
        {
            Assert.IsNotEmpty(PluginUtilities.GetPluginManifests());
        }

        [Test]
        public void VerifyPluginDirectoryExistsWorks()
        {
            var directoryInfo = PluginUtilities.PluginDirectoryExists(out var specificPluginFolderExists);
#if DEBUG
            Assert.IsTrue(specificPluginFolderExists);
            Assert.IsTrue(directoryInfo.FullName.EndsWith(PluginUtilities.PluginDirectoryName));
#else
            Assert.IsFalse(specificPluginFolderExists);
            Assert.IsFalse(directoryInfo.FullName.EndsWith(PluginUtilities.PluginDirectoryName));
#endif
            this.assemblyLocationLoader.Setup(x => x.GetLocation()).Returns(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            directoryInfo = PluginUtilities.PluginDirectoryExists(out specificPluginFolderExists);
            Assert.IsFalse(specificPluginFolderExists);
            Assert.AreEqual(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), directoryInfo.FullName);
        }

        [Test]
        public void VerifyTemporaryDirectoryExists()
        {
            var directoryInfo = PluginUtilities.GetTempDirectoryInfo(PluginName);

            Assert.IsNotNull(directoryInfo);
            Assert.IsNotNull(directoryInfo.Parent);
            Assert.IsTrue(directoryInfo.Parent.Exists);
            Assert.IsFalse(directoryInfo.Exists);
        }
    }

    public class TestAppSettings : AppSettings
    {

    }
}
