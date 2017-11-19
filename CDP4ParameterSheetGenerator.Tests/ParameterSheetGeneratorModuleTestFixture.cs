// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterSheetGeneratorModuleTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ParameterSheetGenerator.Tests
{
    using CDP4Composition;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal.Permission;

    using CDP4OfficeInfrastructure;

    using Moq;

    using NUnit;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="ParameterSheetGeneratorModule"/> class.
    /// </summary>
    [TestFixture]
    public class ParameterSheetGeneratorModuleTestFixture
    {
        private Mock<IPanelNavigationService> panelNavigationService;

        private Mock<IPermissionService> permissionService;

        private Mock<IFluentRibbonManager> fluentRibbonManager;

        private Mock<IThingDialogNavigationService> thingDialogNavigationService;

        private Mock<IOfficeApplicationWrapper> officeApplicationWrapper;

        private Mock<IDialogNavigationService> dialogNavigationService;
        
        [SetUp]
        public void SetUp()
        {
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.permissionService = new Mock<IPermissionService>();
            this.fluentRibbonManager = new Mock<IFluentRibbonManager>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.officeApplicationWrapper = new Mock<IOfficeApplicationWrapper>();
            this.dialogNavigationService = new Mock<IDialogNavigationService>();
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void VerifyThatModuleIsInitialized()
        {
            var module = new ParameterSheetGeneratorModule(
                this.fluentRibbonManager.Object,
                this.panelNavigationService.Object,
                this.thingDialogNavigationService.Object, this.dialogNavigationService.Object, this.officeApplicationWrapper.Object);

            module.Initialize();

            Assert.AreEqual(this.fluentRibbonManager.Object, module.RibbonManager);
            Assert.AreEqual(this.panelNavigationService.Object, module.PanelNavigationService);
            Assert.AreEqual(this.thingDialogNavigationService.Object, module.ThingDialogNavigationService);
            Assert.AreEqual(this.dialogNavigationService.Object, module.DialogNavigationService); 
            Assert.AreEqual(this.officeApplicationWrapper.Object, module.OfficeApplicationWrapper);
        }
    }
}
