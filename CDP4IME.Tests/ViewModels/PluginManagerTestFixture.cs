// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PluginManagerTestFixture.cs" company="Starion Group S.A.">
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

namespace COMET.Tests.ViewModels
{
    using System.IO;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    using CDP4Composition.Modularity;
    using CDP4Composition.Services.AppSettingService;
    using CDP4Composition.Utilities;

    using CDP4ShellDialogs.ViewModels;

    using COMET.Settings;

    using CommonServiceLocator;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class PluginManagerTestFixture
    {
        private Mock<IAppSettingsService<ImeAppSettings>> appSettingService;
        private PluginManagerViewModel<ImeAppSettings> viewModel;
        private ImeAppSettings appSettings;
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IAssemblyInformationService> assemblyLocationLoader;

        [SetUp]
        public void Setup()
        {
            this.appSettings = new ImeAppSettings();

            this.serviceLocator = new Mock<IServiceLocator>();

            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);

            this.appSettingService = new Mock<IAppSettingsService<ImeAppSettings>>();
            this.appSettingService.Setup(x => x.AppSettings).Returns(this.appSettings);

            this.assemblyLocationLoader = new Mock<IAssemblyInformationService>();

            var testDirectory = Path.Combine(Assembly.GetExecutingAssembly().Location, @"../../../../../");
#if DEBUG
            var frameworkVersion = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).Name;
            this.assemblyLocationLoader.Setup(x => x.GetLocation()).Returns(Path.GetFullPath(Path.Combine(testDirectory, $@"CDP4IME\bin\Debug\{frameworkVersion}")));
#else
            this.assemblyLocationLoader.Setup(x => x.GetLocation()).Returns(Path.GetFullPath(testDirectory));
#endif
            this.serviceLocator.Setup(s => s.GetInstance<IAssemblyInformationService>()).Returns(this.assemblyLocationLoader.Object);

            this.serviceLocator.Setup(s => s.GetInstance<IAppSettingsService<ImeAppSettings>>()).Returns(this.appSettingService.Object);

            this.viewModel = new PluginManagerViewModel<ImeAppSettings>(this.appSettingService.Object);
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            Assert.AreEqual(PluginUtilities.GetPluginManifests().Count(), this.viewModel.Plugins.Count);
        }

        [Test]
        public async Task VerifyThatCloseUpdateNotification()
        {
            await this.viewModel.CloseCommand.Execute();
            var dialogResultResult = this.viewModel.DialogResult.Result;
            Assert.IsFalse(dialogResultResult != null && dialogResultResult.Value);
        }

        [Test]
        public void VerifyThatSelectedPluginIsSet()
        {
            this.viewModel.SelectedPlugin = this.viewModel.Plugins.First();
            Assert.AreEqual(this.viewModel.Plugins.First(), this.viewModel.SelectedPlugin);
            Assert.IsNotNull(this.viewModel.SelectedPlugin.AssemblyName);
            Assert.IsNotNull(this.viewModel.SelectedPlugin.Name);
            Assert.IsNotNull(this.viewModel.SelectedPlugin.Description);
            Assert.IsNotNull(this.viewModel.SelectedPlugin.Company);
            Assert.IsNotNull(this.viewModel.SelectedPlugin.Version);
        }

        [Test]
        public async Task VerifyDisabledPluginsUpdatesSettingFile()
        {
            var currentlyDisabledInSettings = new PluginLoader<ImeAppSettings>().DisabledPlugins;
            var alreadyDisabled = this.viewModel.Plugins.Count(p => !p.IsPluginEnabled);

            Assert.AreEqual(currentlyDisabledInSettings.Count, alreadyDisabled);
            var oneToDisabled = this.viewModel.Plugins.First(p => p.IsPluginEnabled);
            Assert.IsNotNull(oneToDisabled);
            oneToDisabled.IsPluginEnabled = false;
            oneToDisabled.IsRowDirty = true;
            await this.viewModel.SaveCommand.Execute();
            var newCount = this.viewModel.Plugins.Count(p => !p.IsPluginEnabled);

            Assert.Greater(newCount, alreadyDisabled);
            Assert.AreEqual(newCount, new PluginLoader<ImeAppSettings>().DisabledPlugins.Count);
        }
    }
}
