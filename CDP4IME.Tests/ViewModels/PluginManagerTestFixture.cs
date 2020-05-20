// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PluginManagerTestFixture.cs" company="RHEA System S.A.">
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

namespace CDP4IME.Tests.ViewModels
{
    using System;
    using System.Linq;

    using CDP4Composition.Services.AppSettingService;

    using CDP4IME.Settings;

    using CDP4ShellDialogs.ViewModels;

    using Microsoft.Practices.Prism.Modularity;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class PluginManagerTestFixture
    {
        private Mock<IAppSettingsService<ImeAppSettings>> appSettingService;
        private PluginManagerViewModel<ImeAppSettings> viewModel;
        private ImeAppSettings appSettings;

        [SetUp]
        public void Setup()
        {
            this.appSettings = new ImeAppSettings();

            this.appSettingService = new Mock<IAppSettingsService<ImeAppSettings>>();
            this.appSettingService.Setup(x => x.AppSettings).Returns(this.appSettings);

            this.viewModel = new PluginManagerViewModel<ImeAppSettings>(this.appSettingService.Object);
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            Assert.AreEqual(1, this.viewModel.Plugins.Count);
        }

        [Test]
        public void VerifyThatCloseUpdateNotification()
        {
            this.viewModel.CloseCommand.Execute(null);
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

        public class TestModule : IModule
        {
            public void Initialize()
            {
                throw new NotImplementedException();
            }
        }
    }
}
