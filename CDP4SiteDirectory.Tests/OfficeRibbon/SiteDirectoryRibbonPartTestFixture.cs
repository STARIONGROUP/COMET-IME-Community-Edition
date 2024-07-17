// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SiteDirectoryRibbonPartTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2024 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4SiteDirectory.Tests.OfficeRibbon
{
    using System;
    using System.IO.Packaging;
    using System.Reactive.Concurrency;
    using System.Threading;
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.Services;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using CDP4SiteDirectory.ViewModels;

    using CommonServiceLocator;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="SiteDirectoryRibbonPart"/> class
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class SiteDirectoryRibbonPartTestFixture
    {
        private Uri uri;
        private int amountOfRibbonControls;
        private int order;
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IThingDialogNavigationService> dialogNavigationService;
        private Mock<IPermissionService> permittingPermissionService;
        private Mock<IFilterStringService> filterStringService;
        private Mock<ISession> session;
        private Person person;
        private SiteDirectoryRibbonPart ribbonPart;
        private CDPMessageBus messageBus;

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.messageBus = new CDPMessageBus();
            this.uri = new Uri("https://www.stariongroup.eu");
            this.SetupRecognizePackUir();

            this.session = new Mock<ISession>();
            var siteDirectory = new SiteDirectory(Guid.NewGuid(), null, this.uri);
            this.person = new Person(Guid.NewGuid(), null, this.uri) { GivenName = "John", Surname = "Doe" };

            this.session.Setup(x => x.DataSourceUri).Returns("test");
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(siteDirectory);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.dialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.filterStringService = new Mock<IFilterStringService>();

            this.serviceLocator = new Mock<IServiceLocator>();

            this.permittingPermissionService = new Mock<IPermissionService>();
            this.permittingPermissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permittingPermissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.permittingPermissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);

            this.session.Setup(x => x.PermissionService).Returns(this.permittingPermissionService.Object);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);

            this.amountOfRibbonControls = 10;
            this.order = 1;

            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);

            this.serviceLocator.Setup(x => x.GetInstance<IThingDialogNavigationService>())
                .Returns(this.dialogNavigationService.Object);

            this.serviceLocator.Setup(x => x.GetInstance<IFilterStringService>()).Returns(this.filterStringService.Object);

            this.ribbonPart = new SiteDirectoryRibbonPart(this.order, this.panelNavigationService.Object, this.dialogNavigationService.Object, null, null, this.messageBus);
        }

        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
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
                Application.ResourceAssembly = typeof(SiteDirectoryRibbonPart).Assembly;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        [Test]
        public void VerifyThatTheOrderAndXmlAreLoaded()
        {
            Assert.AreEqual(this.order, this.ribbonPart.Order);
            Assert.AreEqual(this.amountOfRibbonControls, this.ribbonPart.ControlIdentifiers.Count);
        }

        [Test]
        public void VerifyThatIfFluentRibbonIsNotActiveTheSessionEventHasNoEffect()
        {
            var fluentRibbonManager = new FluentRibbonManager();
            fluentRibbonManager.IsActive = false;
            fluentRibbonManager.RegisterRibbonPart(this.ribbonPart);

            var sessionEvent = new SessionEvent(this.session.Object, SessionStatus.Open);
            this.messageBus.SendMessage(sessionEvent);
            Assert.IsNull(this.ribbonPart.Session);
        }

        [Test]
        public void VerifyThatIfFluentRibbonIsNullTheSessionEventHasNoEffect()
        {
            var sessionEvent = new SessionEvent(this.session.Object, SessionStatus.Open);
            this.messageBus.SendMessage(sessionEvent);
            Assert.IsNull(this.ribbonPart.Session);
        }

        [Test]
        public void VerifyThatRibbonPartHandlesSessionOpenAndCloseEvent()
        {
            var fluentRibbonManager = new FluentRibbonManager();
            fluentRibbonManager.IsActive = true;
            fluentRibbonManager.RegisterRibbonPart(this.ribbonPart);

            var openSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Open);
            this.messageBus.SendMessage(openSessionEvent);

            Assert.AreEqual(this.session.Object, this.ribbonPart.Session);

            var closeSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Closed);
            this.messageBus.SendMessage(closeSessionEvent);

            Assert.IsNull(this.ribbonPart.Session);
        }

        [Test]
        public void VerifyThatOnActionInvokesTheNavigationServiceOpenAndCloseMethod()
        {
            var fluentRibbonManager = new FluentRibbonManager();
            fluentRibbonManager.IsActive = true;
            fluentRibbonManager.RegisterRibbonPart(this.ribbonPart);

            // open viemodels
            var openSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Open);
            this.messageBus.SendMessage(openSessionEvent);

            this.ribbonPart.OnAction("ShowDomainsOfExpertise");
            this.panelNavigationService.Verify(x => x.OpenExistingOrOpenInAddIn(It.IsAny<DomainOfExpertiseBrowserViewModel>()));

            this.ribbonPart.OnAction("ShowModels");
            this.panelNavigationService.Verify(x => x.OpenExistingOrOpenInAddIn(It.IsAny<ModelBrowserViewModel>()));

            this.ribbonPart.OnAction("ShowLanguages");
            this.panelNavigationService.Verify(x => x.OpenExistingOrOpenInAddIn(It.IsAny<NaturalLanguageBrowserViewModel>()));

            this.ribbonPart.OnAction("ShowOrganizations");
            this.panelNavigationService.Verify(x => x.OpenExistingOrOpenInAddIn(It.IsAny<OrganizationBrowserViewModel>()));

            this.ribbonPart.OnAction("ShowRoles");
            this.panelNavigationService.Verify(x => x.OpenExistingOrOpenInAddIn(It.IsAny<RoleBrowserViewModel>()));

            this.ribbonPart.OnAction("ShowPersons");
            this.panelNavigationService.Verify(x => x.OpenExistingOrOpenInAddIn(It.IsAny<PersonBrowserViewModel>()));

            this.ribbonPart.OnAction("ShowSiteRDLs");
            this.panelNavigationService.Verify(x => x.OpenExistingOrOpenInAddIn(It.IsAny<SiteRdlBrowserViewModel>()));

            // close viewmodels
            var closeSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Closed);
            this.messageBus.SendMessage(closeSessionEvent);

            this.panelNavigationService.Verify(x => x.CloseInAddIn(It.IsAny<DomainOfExpertiseBrowserViewModel>()));
            this.panelNavigationService.Verify(x => x.CloseInAddIn(It.IsAny<ModelBrowserViewModel>()));
            this.panelNavigationService.Verify(x => x.CloseInAddIn(It.IsAny<NaturalLanguageBrowserViewModel>()));
            this.panelNavigationService.Verify(x => x.CloseInAddIn(It.IsAny<OrganizationBrowserViewModel>()));
            this.panelNavigationService.Verify(x => x.CloseInAddIn(It.IsAny<RoleBrowserViewModel>()));
            this.panelNavigationService.Verify(x => x.CloseInAddIn(It.IsAny<PersonBrowserViewModel>()));
            this.panelNavigationService.Verify(x => x.CloseInAddIn(It.IsAny<SiteRdlBrowserViewModel>()));
        }

        [Test]
        public void VerifyThatOnActionDoesNotInvokePanelNavigationWhenSessionIsNull()
        {
            this.ribbonPart.OnAction("ShowDomainsOfExpertise");
            this.panelNavigationService.Verify(x => x.OpenInAddIn(It.IsAny<DomainOfExpertiseBrowserViewModel>()), Times.Never);
        }

        [Test]
        public void VerifyThatGetEnabledReturnsExpectedResult()
        {
            var fluentRibbonManager = new FluentRibbonManager();
            fluentRibbonManager.IsActive = true;
            fluentRibbonManager.RegisterRibbonPart(this.ribbonPart);

            Assert.IsFalse(this.ribbonPart.GetEnabled("ShowDomainsOfExpertise"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("ShowModels"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("ShowLanguages"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("ShowOrganizations"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("ShowPersons"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("ShowRoles"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("ShowSiteRDLs"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("unknownRibbonControlId"));

            var openSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Open);
            this.messageBus.SendMessage(openSessionEvent);

            Assert.IsTrue(this.ribbonPart.GetEnabled("ShowDomainsOfExpertise"));
            Assert.IsTrue(this.ribbonPart.GetEnabled("ShowModels"));
            Assert.IsTrue(this.ribbonPart.GetEnabled("ShowLanguages"));
            Assert.IsTrue(this.ribbonPart.GetEnabled("ShowOrganizations"));
            Assert.IsTrue(this.ribbonPart.GetEnabled("ShowPersons"));
            Assert.IsTrue(this.ribbonPart.GetEnabled("ShowRoles"));
            Assert.IsTrue(this.ribbonPart.GetEnabled("ShowSiteRDLs"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("unknownRibbonControlId"));

            var closeSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Closed);
            this.messageBus.SendMessage(closeSessionEvent);

            Assert.IsFalse(this.ribbonPart.GetEnabled("ShowDomainsOfExpertise"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("ShowModels"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("ShowLanguages"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("ShowOrganizations"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("ShowPersons"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("ShowRoles"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("ShowSiteRDLs"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("unknownRibbonControlId"));
        }

        [Test]
        public void VerifyThatImageIsReturned()
        {
            var domainsOfExpertiseImage = this.ribbonPart.GetImage("ShowDomainsOfExpertise");
            Assert.IsNotNull(domainsOfExpertiseImage);

            var modelsImage = this.ribbonPart.GetImage("ShowModels");
            Assert.IsNotNull(modelsImage);

            var languagesImage = this.ribbonPart.GetImage("ShowLanguages");
            Assert.IsNotNull(languagesImage);

            var organizationsImage = this.ribbonPart.GetImage("ShowOrganizations");
            Assert.IsNotNull(organizationsImage);

            var personsImage = this.ribbonPart.GetImage("ShowPersons");
            Assert.IsNotNull(personsImage);

            var rolesImage = this.ribbonPart.GetImage("ShowRoles");
            Assert.IsNotNull(rolesImage);

            var siteRDLImage = this.ribbonPart.GetImage("ShowSiteRDLs");
            Assert.IsNotNull(siteRDLImage);

            var unknownImage = this.ribbonPart.GetImage("unknownRibbonControlId");
            Assert.IsNull(unknownImage);
        }
    }
}
