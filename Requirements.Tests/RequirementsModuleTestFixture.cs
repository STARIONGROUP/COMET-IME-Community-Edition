// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequirementsModuleTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Tests
{
    using System;

    using CDP4Composition;
    using CDP4Composition.Exceptions;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Requirements.Settings.JsonConverters;
    using CDP4Requirements.Views;

    using DevExpress.Pdf.Native.BouncyCastle.Asn1.Cms;
    using DevExpress.XtraRichEdit.Layout.Engine;

    using Microsoft.Practices.Prism.Regions;
    using Microsoft.Practices.ServiceLocation;
    using Moq;

    using Newtonsoft.Json;

    using NUnit.Framework;

    /// <summary>
    /// TestFixture for the <see cref="RequirementsModule"/>
    /// </summary>
    [TestFixture]
    public class RequirementsModuleTestFixture
    {
        private Mock<IFluentRibbonManager> ribbonManager;
        private Mock<IDialogNavigationService> dialogNAvigationService;
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IPluginSettingsService> pluginSettingService;
        private Mock<IRegionManager> regionManager;

        [SetUp]
        public void Setup()
        {
            this.ribbonManager = new Mock<IFluentRibbonManager>();
            this.ribbonManager.Setup(x => x.RegisterRibbonPart(It.IsAny<RequirementRibbonPart>()));

            this.regionManager = new Mock<IRegionManager>();
            
            this.dialogNAvigationService = new Mock<IDialogNavigationService>();
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();

            this.pluginSettingService = new Mock<IPluginSettingsService>();
            this.pluginSettingService.Setup(x => x.Write(It.IsAny<RequirementsModuleSettings>(), It.IsAny<JsonConverter>()));
            this.serviceLocator = new Mock<IServiceLocator>();
            this.serviceLocator.Setup(x => x.GetInstance<IPluginSettingsService>()).Returns(this.pluginSettingService.Object);
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
        }

        [Test]
        public void VerifyThatRegionManagerIsSet()
        {
            var expected = new RegionManager();
            var module = new RequirementsModule(expected, this.ribbonManager.Object, this.panelNavigationService.Object, this.thingDialogNavigationService.Object, this.dialogNAvigationService.Object, null, null);
            Assert.AreEqual(expected, module.RegionManager);
        }

        [Test]
        public void VerifyInitialize()
        {
            var module = new RequirementsModule(this.regionManager.Object, this.ribbonManager.Object, this.panelNavigationService.Object, this.thingDialogNavigationService.Object, this.dialogNAvigationService.Object, this.pluginSettingService.Object, null);

            Assert.Throws<NullReferenceException>(() =>
            {
                this.pluginSettingService.Setup(x => x.Read<RequirementsModuleSettings>(true, It.IsAny<JsonConverter[]>())).Returns(new RequirementsModuleSettings());
                module.Initialize();
            });

            Assert.Throws<NullReferenceException>(() =>
            {
                this.pluginSettingService.Setup(x => x.Read<RequirementsModuleSettings>(true, It.IsAny<JsonConverter[]>())).Returns(default(RequirementsModuleSettings));
                module.Initialize();
            });

            Assert.Throws<NullReferenceException>(() =>
            {
                this.pluginSettingService.Setup(x => x.Read<RequirementsModuleSettings>(true, It.IsAny<JsonConverter[]>())).Throws<PluginSettingsException>();
                module.Initialize();
            });

            Assert.Throws<JsonException>(() =>
            {
                this.pluginSettingService.Setup(x => x.Read<RequirementsModuleSettings>(true, It.IsAny<JsonConverter[]>())).Throws<JsonException>();
                module.Initialize();
            });

            this.pluginSettingService.Verify(x => x.Write(It.IsAny<RequirementsModuleSettings>()), Times.Once);
            this.pluginSettingService.Verify(x => x.Read<RequirementsModuleSettings>(true, It.IsAny<JsonConverter[]>()), Times.Exactly(4));
        }
    }
}
