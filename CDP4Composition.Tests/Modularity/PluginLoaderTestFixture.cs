// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PluginLoaderTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft,
//            Nathanael Smiechowski, Kamil Wojnowski
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

namespace CDP4Composition.Tests.Modularity
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using CDP4Composition.Modularity;
    using CDP4Composition.Services.AppSettingService;
    using CDP4Composition.Tests.Utilities;
    using CDP4Composition.Utilities;

    using CDP4IME.Settings;

    using Microsoft.Practices.ServiceLocation;

    using Moq;

    using Newtonsoft.Json;

    using NUnit.Framework;

    [TestFixture]
    public class PluginLoaderTestFixture
    {
        private Mock<IServiceLocator> serviceLocator;

        private Mock<IAppSettingsService<ImeAppSettings>> appSettingsService;
        private ImeAppSettings appSettings;

        private const string ImeFolder = @"CDP4IME\bin\Debug";
        private const string AppSettingsJson = "AppSettingsTest.json";

        [OneTimeSetUp]
        public void Setup()
        {
            var testDirectory = Path.Combine(Assembly.GetExecutingAssembly().Location, @"../../../../");
            testDirectory = Path.GetFullPath(Path.Combine(testDirectory, ImeFolder));

            this.serviceLocator = new Mock<IServiceLocator>();
            this.serviceLocator.Setup(s => s.GetInstance<IAssemblyLocationLoader>()).Returns(new AssemblyLocationLoader());

            this.appSettingsService = new Mock<IAppSettingsService<ImeAppSettings>>();

            this.appSettings = JsonConvert.DeserializeObject<ImeAppSettings>(File.ReadAllText(Path.Combine(Assembly.GetExecutingAssembly().Location, @"../Modularity/", AppSettingsJson)));
            this.appSettingsService.Setup(x => x.AppSettings).Returns(this.appSettings);
            
            this.serviceLocator.Setup(x => x.GetInstance<IAppSettingsService<ImeAppSettings>>())
                .Returns(this.appSettingsService.Object);

            Directory.SetCurrentDirectory(testDirectory); 
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
        }

        [Test]
        public void VerifyImeManifestAreLoaded()
        {
            var pluginLoader = new PluginLoader<ImeAppSettings>();
            Assert.IsNotEmpty(pluginLoader.ManifestsList);
        }

        [Test]
        public void VerifyDisabledPluginsAreNotGettingLoaded()
        {
            var pluginLoader = new PluginLoader<ImeAppSettings>();
            Assert.IsNotEmpty(pluginLoader.ManifestsList);
            Assert.IsNotEmpty(pluginLoader.DisabledPlugins);
        }
    }
}
