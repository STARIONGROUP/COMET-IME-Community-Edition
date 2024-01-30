// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RibbonPartTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2024 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests
{
    using System.Reflection;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;

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
            var ribbonPart = new RibbonPartTesting(this.order, this.panelNavigationService.Object, null, null, null, new CDPMessageBus());
            Assert.AreEqual(this.order, ribbonPart.Order);
            Assert.AreEqual(this.ribbonXml, ribbonPart.RibbonXml);
        }

        [Test]
        public void VerifyThatDefaultValueIsReturnedForMethodsThatAreNotOverriden()
        {
            var ribbonPart = new RibbonPartTesting(this.order, this.panelNavigationService.Object, null, null, null, new CDPMessageBus());

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
        internal RibbonPartTesting(int order, IPanelNavigationService panelNavigationService, IThingDialogNavigationService thingDialogNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService, ICDPMessageBus cdpMessageBus)
            : base(order, panelNavigationService, thingDialogNavigationService, dialogNavigationService, pluginSettingsService, cdpMessageBus)
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
