// -------------------------------------------------------------------------------------------------
// <copyright file="CDP4ScriptingModuleTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2017 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Scripting.Tests
{
    using CDP4Composition.Navigation;
    using Microsoft.Practices.Prism.Regions;
    using Moq;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="CDP4ScriptingModule"/>
    /// </summary>
    [TestFixture]
    public class CDP4ScriptingModuleTestFixture
    {
        private CDP4ScriptingModule scriptingModule;

        private Mock<IRegionManager> regionManager;
        private Mock<IPanelNavigationService> panelNavigationService;

        [SetUp]
        public void SetUp()
        {
            this.regionManager = new Mock<IRegionManager>();
            this.panelNavigationService = new Mock<IPanelNavigationService>();            
        }

        [Test]
        public void VerifyThatPropertiesAreSetByConstructor()
        {
            this.scriptingModule = new CDP4ScriptingModule(this.regionManager.Object, this.panelNavigationService.Object);

            Assert.AreEqual(this.regionManager.Object, this.scriptingModule.RegionManager);
            Assert.AreEqual(this.panelNavigationService.Object, this.scriptingModule.PanelNavigationService);        
        }
    }
}
