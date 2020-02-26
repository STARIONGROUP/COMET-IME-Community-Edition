// -------------------------------------------------------------------------------------------------
// <copyright file="RelationshipMatrixModuleTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4RelationshipMatrix.Tests
{
    using System;
    using CDP4Composition;
    using CDP4Composition.Exceptions;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;
    using CDP4Composition.Services;
    using DevExpress.Data.Filtering.Helpers;
    using Microsoft.Practices.Prism.Regions;
    using Microsoft.Practices.ServiceLocation;
    using Moq;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="RelationshipMatrixModule"/>
    /// </summary>
    [TestFixture]
    public class RelationshipMatrixModuleTestFixture
    {
        private Mock<IServiceLocator> serviceLocator;
        private RelationshipMatrixModule relationshipMatrixModule;
        private Mock<IRegionManager> regionManager;
        private Mock<IFluentRibbonManager> fluentRibbonManager;
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IDialogNavigationService> dialogNavigationService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IPluginSettingsService> pluginSettingsService;

        [SetUp]
        public void SetUp()
        {
            this.serviceLocator = new Mock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);

            this.regionManager = new Mock<IRegionManager>();
            this.serviceLocator.Setup(x => x.GetInstance<IRegionManager>()).Returns(this.regionManager.Object);

            this.fluentRibbonManager = new Mock<IFluentRibbonManager>();
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.dialogNavigationService = new Mock<IDialogNavigationService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.pluginSettingsService = new Mock<IPluginSettingsService>();
            
            this.relationshipMatrixModule = new RelationshipMatrixModule(this.regionManager.Object,this.fluentRibbonManager.Object, this.panelNavigationService.Object, this.dialogNavigationService.Object, this.thingDialogNavigationService.Object, this.pluginSettingsService.Object);
            this.pluginSettingsService.Setup(s => s.Read<RelationshipMatrixPluginSettings>())
                .Returns(new RelationshipMatrixPluginSettings(true));
        }

        [Test]
        public void Verify_that_when_plugin_service_can_read_settings_no_exception_is_raised()
        {
           Assert.DoesNotThrow(() => this.relationshipMatrixModule.ReadPluginSettings());
            
            this.pluginSettingsService.Verify(x => x.Read<RelationshipMatrixPluginSettings>());
        }

        [Test]
        public void Verify_that_when_settings_file_cannot_be_read_the_module_recovers_with_the_default_SettingsClass()
        {
            this.pluginSettingsService
                .Setup(x => x.Read<RelationshipMatrixPluginSettings>())
                .Throws<PluginSettingsException>();
            
            Assert.DoesNotThrow(() => this.relationshipMatrixModule.ReadPluginSettings());

            this.pluginSettingsService.Verify(x => x.Write(It.IsAny<RelationshipMatrixPluginSettings>()));
        }
    }
}