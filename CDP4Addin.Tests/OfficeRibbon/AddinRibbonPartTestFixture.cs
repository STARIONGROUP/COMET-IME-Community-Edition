// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AddinRibbonPartTestFixture.cs" company="RHEA System S.A.">
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

namespace CDP4Addin.Tests.OfficeRibbon
{
    using System;
    using System.Collections.Generic;
    using System.IO.Packaging;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;

    using CDP4Addin.Tests.Utils;

    using CDP4AddinCE;
    using CDP4AddinCE.Settings;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.Services.AppSettingService;
    using CDP4Composition.Utilities;

    using CDP4Dal;
    using CDP4Dal.Composition;
    using CDP4Dal.DAL;
    using CDP4Dal.Events;

    using Microsoft.Practices.ServiceLocation;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// suite of tests for the <see cref="AddinRibbonPart"/> class
    /// </summary>
    [TestFixture, Apartment(ApartmentState.STA)]
    public class AddinRibbonPartTestFixture
    {
        private Uri uri;
        private AddinRibbonPart ribbonPart;
        private int amountOfRibbonControls;
        private int order;
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IDialogNavigationService> dialogNavigationService;
        private Mock<ISession> session;
        private Assembler assembler;
        private Mock<IAppSettingsService<AddinAppSettings>> appSettingService;
        private SiteDirectory siteDirectory;

        [SetUp]
        public void SetUp()
        {
            this.SetupRecognizePackUir();

            this.uri = new Uri("http://www.rheageoup.com");
            this.assembler = new Assembler(this.uri);
            this.siteDirectory = new SiteDirectory(Guid.NewGuid(), null, new Uri("http://test.com"));

            this.session = new Mock<ISession>();
            var LazySiteDirectory = new Lazy<Thing>(() => this.siteDirectory);
            this.assembler.Cache.GetOrAdd(new CacheKey(LazySiteDirectory.Value.Iid, null), LazySiteDirectory);

            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDirectory);
            this.session.Setup(x => x.DataSourceUri).Returns("test");
            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            var iterationDictionary = new Dictionary<CDP4Common.EngineeringModelData.Iteration, Tuple<CDP4Common.SiteDirectoryData.DomainOfExpertise, CDP4Common.SiteDirectoryData.Participant>>();
            this.session.Setup(x => x.OpenIterations).Returns(iterationDictionary);

            this.appSettingService =new Mock<IAppSettingsService<AddinAppSettings>>();
            this.appSettingService.Setup(x => x.AppSettings).Returns(new AddinAppSettings());

            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.dialogNavigationService = new Mock<IDialogNavigationService>();
            this.serviceLocator = new Mock<IServiceLocator>();

            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);

            var dals = new List<Lazy<IDal, IDalMetaData>>();
            var availableDals = new AvailableDals(dals);
            this.serviceLocator.Setup(x => x.GetInstance<AvailableDals>()).Returns(availableDals);

            this.serviceLocator.Setup(s => s.GetInstance<IAssemblyLocationLoader>()).Returns(new AssemblyLocationLoader());

            this.amountOfRibbonControls = 9;
            this.order = 1;

            this.ribbonPart = new AddinRibbonPart(this.order, this.panelNavigationService.Object, null, this.dialogNavigationService.Object, null, this.appSettingService.Object);

            var fluentRibbonManager = new FluentRibbonManager();
            fluentRibbonManager.IsActive = true;
            fluentRibbonManager.RegisterRibbonPart(this.ribbonPart);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        /// <summary>
        /// Pack Uri's are not recognized untill they have been registered in the appl domain
        /// This method makes sure pack Uri's do no throw an exception regarding incorrect port.
        /// </summary>
        private void SetupRecognizePackUir()
        {
            PackUriHelper.Create(new Uri("reliable://0"));
            new FrameworkElement();

            try
            {
                Application.ResourceAssembly = typeof(AddinRibbonPart).Assembly;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex);
            }
        }

        [Test]
        public void VerifyThatTheOrderAndXmlAreLoaded()
        {
            Assert.AreEqual(this.order, this.ribbonPart.Order);
            Assert.AreEqual(this.amountOfRibbonControls, this.ribbonPart.ControlIdentifiers.Count);
        }

        [Test]
        public void VerifyThatGetEnabledReturnsExpectedResult()
        {
            Assert.IsTrue(this.ribbonPart.GetEnabled("CDP4_Open"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("CDP4_Close"));
            Assert.IsTrue(this.ribbonPart.GetEnabled("CDP4_ProxySettings"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("CDP4_SelectModelToOpen"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("CDP4_SelectModelToClose"));
            Assert.IsTrue(this.ribbonPart.GetEnabled("CDP4_Plugins"));

            var openSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Open);
            CDPMessageBus.Current.SendMessage(openSessionEvent);

            Assert.IsFalse(this.ribbonPart.GetEnabled("CDP4_Open"));
            Assert.IsTrue(this.ribbonPart.GetEnabled("CDP4_Close"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("CDP4_ProxySettings"));
            Assert.IsTrue(this.ribbonPart.GetEnabled("CDP4_SelectModelToOpen"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("CDP4_SelectModelToClose"));
            Assert.IsTrue(this.ribbonPart.GetEnabled("CDP4_Plugins"));

            var closeSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Closed);
            CDPMessageBus.Current.SendMessage(closeSessionEvent);

            Assert.IsTrue(this.ribbonPart.GetEnabled("CDP4_Open"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("CDP4_Close"));
            Assert.IsTrue(this.ribbonPart.GetEnabled("CDP4_ProxySettings"));
        }

        [Test]
        public async Task VerifyThatOnActionSelectModelToCloseOpenDialog()
        {
            var openSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Open);
            CDPMessageBus.Current.SendMessage(openSessionEvent);

            await this.ribbonPart.OnAction("CDP4_SelectModelToClose");
            this.dialogNavigationService.Verify(x => x.NavigateModal(It.IsAny<IDialogViewModel>()));
        }

        [Test]
        public async Task VerifyThatOpenAPluginManagerCommandNavigatesToPluginWindow()
        {
            await this.ribbonPart.OnAction("CDP4_Plugins");
            this.dialogNavigationService.Verify(x => x.NavigateModal(It.IsAny<IDialogViewModel>()));
        }

        [Test]
        public async Task VerifyThatOnActionPerformsExpectedResult()
        {
            await this.ribbonPart.OnAction("CDP4_Open");
            this.dialogNavigationService.Verify(x => x.NavigateModal(It.IsAny<IDialogViewModel>()));

            var openSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Open);
            CDPMessageBus.Current.SendMessage(openSessionEvent);

            await this.ribbonPart.OnAction("CDP4_Close");
            this.session.Verify(x => x.Close());
        }

        [Test]
        public async Task Verify_that_when_proxyserver_on_action_dialogisnavigated()
        {
            await this.ribbonPart.OnAction("CDP4_ProxySettings");
            this.dialogNavigationService.Verify(x => x.NavigateModal(It.IsAny<IDialogViewModel>()));
        }

        [Test]
        public async Task Verify_that_when_On_Action_CDP4_SelectModelToOpen_NavigatesTo_ModelOpeningDialogViewModel()
        {
            var openSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Open);
            CDPMessageBus.Current.SendMessage(openSessionEvent);

            await this.ribbonPart.OnAction("CDP4_SelectModelToOpen");

            this.dialogNavigationService.Verify(x => x.NavigateModal(It.IsAny<IDialogViewModel>()));
        }
    }
}