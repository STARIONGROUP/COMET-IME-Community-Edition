// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RibbonPartTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests
{
    using System;
    using System.Reflection;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;
    using CDP4Dal.Permission;
    using Moq;
    using NUnit.Framework;
    
    /// <summary>
    /// Suite of tests for the <see cref="RibbonPart"/> class
    /// </summary>
    [TestFixture]
    public class RibbonPartTestFixture
    {
        private int order;
        private string ribbonXmlResourceName;
        private string ribbonXml;
        private Mock<IPanelNavigationService> panelNavigationService;
        private string ribborpartid;

        [SetUp]
        public void SetUp()
        {
            this.order = 1;
            this.ribborpartid = "test";
            this.ribbonXml = string.Format("<test id=\"{0}\">this is a piece of test ribbon xml</test>", this.ribborpartid);
            this.panelNavigationService = new Mock<IPanelNavigationService>();
        }

        [Test]
        public void VerifyThatTheExpectedRibbonXmlIsReturned()
        {
            var ribbonPart = new RibbonPartTesting(this.order, this.panelNavigationService.Object, null, null, null);
            Assert.AreEqual(this.order, ribbonPart.Order);
            Assert.AreEqual(this.ribbonXml, ribbonPart.RibbonXml);
        }

        [Test]
        public void VerifyThatDefaultValueIsReturnedForMethodsThatAreNotOverriden()
        {
            var ribbonPart = new RibbonPartTesting(this.order, this.panelNavigationService.Object, null, null, null);

            Assert.DoesNotThrow(() => ribbonPart.OnAction("testId"));

            Assert.IsEmpty(ribbonPart.GetDescription("testId"));

            Assert.IsEmpty(ribbonPart.GetKeytip("testId"));

            Assert.IsEmpty(ribbonPart.GetScreentip("testId"));

            Assert.IsEmpty(ribbonPart.GetContent("testId"));

            Assert.IsEmpty(ribbonPart.GetLabel("testId"));

            Assert.AreEqual("normal", ribbonPart.GetSize("testId"));

            Assert.IsEmpty(ribbonPart.GetToolTip("testId"));

            Assert.IsEmpty(ribbonPart.GetSupertip("testId"));

            Assert.IsNull(ribbonPart.GetImage("testId"));

            Assert.IsTrue(ribbonPart.GetEnabled("testId"));

            Assert.IsFalse(ribbonPart.GetPressed("testId"));

            Assert.IsTrue(ribbonPart.GetVisible("testId"));

            Assert.IsTrue(ribbonPart.GetShowImage("testId"));

            Assert.IsTrue(ribbonPart.GetShowLabel("testId"));
        }
    }

    internal class RibbonPartTesting : RibbonPart
    {
        internal RibbonPartTesting(int order, IPanelNavigationService panelNavigationService, IThingDialogNavigationService thingDialogNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
            : base(order, panelNavigationService, thingDialogNavigationService, dialogNavigationService, pluginSettingsService)
        {
        }

        protected override string GetRibbonXmlResourceName()
        {
            return "testribbon";
        }

        protected override Assembly GetCurrentAssembly()
        {
            return Assembly.GetExecutingAssembly();            
        }
    }
}
