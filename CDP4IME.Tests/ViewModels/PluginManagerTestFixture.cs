// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PluginManagerTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4IME.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Composition.Services.AppSettingService;

    using CDP4IME.Settings;

    using CDP4ShellDialogs.ViewModels;

    using Microsoft.Practices.Prism.Modularity;
    using Microsoft.Practices.ServiceLocation;
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

            //this.appSettings.Plugins.Add(new PluginSettingsMetaData { Assembly = "Test Assembly", Company = "RHEA",Description= "Plugin Description",IsEnabled= true,IsMandatory= false, Name = "test", Key= "PluginFolder", Version= "5.1" });

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
