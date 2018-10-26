// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AddinRibbonPartTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4AddinCE.Tests.OfficeRibbon
{
    using System;
    using System.Collections.Generic;
    using System.IO.Packaging;
    using System.Threading.Tasks;
    using System.Windows;
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
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
    [TestFixture, RequiresSTA]
    public class AddinRibbonPartTestFixture
    {
        private Uri uri;
        private AddinRibbonPart ribbonPart;
        private int amountOfRibbonControls;
        private int order;
        private string ribbonxmlname;
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IDialogNavigationService> dialogNavigationService;
        private Mock<ISession> session;
        private Assembler assembler;

        [SetUp]
        public void SetUp()
        {
            this.SetupRecognizePackUir();

            this.uri = new Uri("http://www.rheageoup.com");
            this.assembler = new Assembler(this.uri);

            this.session = new Mock<ISession>();
            var siteDirectory = new Lazy<Thing>(() => new SiteDirectory(Guid.NewGuid(), null, new Uri("http://test.com")));
            this.assembler.Cache.GetOrAdd(new Tuple<Guid, Guid?>(siteDirectory.Value.Iid, null), siteDirectory);
            this.session.Setup(x => x.DataSourceUri).Returns("test");
            this.session.Setup(x => x.Assembler).Returns(this.assembler);

            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.dialogNavigationService = new Mock<IDialogNavigationService>();
            this.serviceLocator = new Mock<IServiceLocator>();

            this.amountOfRibbonControls = 4;
            this.order = 1;

            this.ribbonPart = new AddinRibbonPart(this.order, this.panelNavigationService.Object, null, null);

            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IDialogNavigationService>())
                .Returns(this.dialogNavigationService.Object);

            var dals = new List<Lazy<IDal, IDalMetaData>>();
            var availableDals = new AvailableDals(dals);
            this.serviceLocator.Setup(x => x.GetInstance<AvailableDals>()).Returns(availableDals);

            var fluentRibbonManager = new FluentRibbonManager();
            fluentRibbonManager.IsActive = true;
            fluentRibbonManager.RegisterRibbonPart(this.ribbonPart);
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

            var openSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Open);
            CDPMessageBus.Current.SendMessage(openSessionEvent);

            Assert.IsFalse(this.ribbonPart.GetEnabled("CDP4_Open"));
            Assert.IsTrue(this.ribbonPart.GetEnabled("CDP4_Close"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("CDP4_ProxySettings"));

            var closeSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Closed);
            CDPMessageBus.Current.SendMessage(closeSessionEvent);

            Assert.IsTrue(this.ribbonPart.GetEnabled("CDP4_Open"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("CDP4_Close"));
            Assert.IsTrue(this.ribbonPart.GetEnabled("CDP4_ProxySettings"));
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
        public void Verify_that_when_proxyserver_on_action_dialogisnavigated()
        {
            this.ribbonPart.OnAction("CDP4_ProxySettings");
            this.dialogNavigationService.Verify(x => x.NavigateModal(It.IsAny<IDialogViewModel>()));
        }
    }
}