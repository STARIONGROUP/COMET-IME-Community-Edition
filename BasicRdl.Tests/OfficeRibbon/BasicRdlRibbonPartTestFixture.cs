﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BasicRdlRibbonPartTestFixture.cs" company="Starion Group S.A.">
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

namespace BasicRdl.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO.Packaging;
    using System.Reactive.Concurrency;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;

    using BasicRdl.ViewModels;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.Services;
    using CDP4Composition.Services.FavoritesService;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using CommonServiceLocator;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// TestFixture for the <see cref="BasicRdlRibbonPart"/>
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class BasicRdlRibbonPartTestFixture
    {
        private Uri uri;
        private BasicRdlRibbonPart ribbonPart;
        private int amountOfRibbonControls;
        private int order;
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IThingDialogNavigationService> dialogNavigationService;
        private Mock<IPermissionService> permittingPermissionService;
        private Mock<ISession> session;
        private Person person;
        private Mock<IFavoritesService> favoritesService;
        private Mock<IFilterStringService> filterStringService;
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
            this.favoritesService = new Mock<IFavoritesService>();
            this.session.Setup(x => x.DataSourceUri).Returns("test");
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(siteDirectory);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.dialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.serviceLocator = new Mock<IServiceLocator>();
            this.filterStringService = new Mock<IFilterStringService>();

            this.permittingPermissionService = new Mock<IPermissionService>();
            this.permittingPermissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permittingPermissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.permittingPermissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);

            this.favoritesService.Setup(x => x.GetFavoriteItemsCollectionByType(It.IsAny<ISession>(), It.IsAny<Type>()))
                .Returns(new HashSet<Guid>());

            this.favoritesService.Setup(x =>
                x.SubscribeToChanges(It.IsAny<ISession>(), It.IsAny<Type>(), It.IsAny<Action<HashSet<Guid>>>())).Returns(new Mock<IDisposable>().Object);

            this.session.Setup(x => x.PermissionService).Returns(this.permittingPermissionService.Object);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);

            this.amountOfRibbonControls = 6;
            this.order = 1;

            this.ribbonPart = new BasicRdlRibbonPart(this.order, this.panelNavigationService.Object, null, null, null, this.favoritesService.Object, this.messageBus);

            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IThingDialogNavigationService>()).Returns(this.dialogNavigationService.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IFilterStringService>()).Returns(this.filterStringService.Object);
        }

        [TearDown]
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
                Application.ResourceAssembly = typeof(BasicRdlRibbonPart).Assembly;
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
        public async Task VerifyThatOnActionInvokesTheNavigationServiceOpenAndCloseMethod()
        {
            var fluentRibbonManager = new FluentRibbonManager();
            fluentRibbonManager.IsActive = true;
            fluentRibbonManager.RegisterRibbonPart(this.ribbonPart);

            // open viemodels
            var openSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Open);
            this.messageBus.SendMessage(openSessionEvent);

            await this.ribbonPart.OnAction("ShowMeasurementUnits");
            this.panelNavigationService.Verify(x => x.OpenExistingOrOpenInAddIn(It.IsAny<MeasurementUnitsBrowserViewModel>()));

            await this.ribbonPart.OnAction("ShowMeasurementScales");
            this.panelNavigationService.Verify(x => x.OpenExistingOrOpenInAddIn(It.IsAny<MeasurementScalesBrowserViewModel>()));

            await this.ribbonPart.OnAction("ShowParameterTypes");
            this.panelNavigationService.Verify(x => x.OpenExistingOrOpenInAddIn(It.IsAny<ParameterTypesBrowserViewModel>()));

            await this.ribbonPart.OnAction("ShowRules");
            this.panelNavigationService.Verify(x => x.OpenExistingOrOpenInAddIn(It.IsAny<RulesBrowserViewModel>()));

            await this.ribbonPart.OnAction("ShowCategories");
            this.panelNavigationService.Verify(x => x.OpenExistingOrOpenInAddIn(It.IsAny<CategoryBrowserViewModel>()));

            // close viewmodels
            var closeSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Closed);
            this.messageBus.SendMessage(closeSessionEvent);

            this.panelNavigationService.Verify(x => x.CloseInAddIn(It.IsAny<MeasurementUnitsBrowserViewModel>()));
            this.panelNavigationService.Verify(x => x.CloseInAddIn(It.IsAny<MeasurementScalesBrowserViewModel>()));
            this.panelNavigationService.Verify(x => x.CloseInAddIn(It.IsAny<ParameterTypesBrowserViewModel>()));
            this.panelNavigationService.Verify(x => x.CloseInAddIn(It.IsAny<RulesBrowserViewModel>()));
            this.panelNavigationService.Verify(x => x.CloseInAddIn(It.IsAny<CategoryBrowserViewModel>()));
        }

        [Test]
        public async Task VerifyThatOnActionDoesNotInvokePanelNavigationWhenSessionIsNull()
        {
            await this.ribbonPart.OnAction("ShowMeasurementUnits");
            this.panelNavigationService.Verify(x => x.OpenInAddIn(It.IsAny<MeasurementUnitsBrowserViewModel>()), Times.Never);
        }

        [Test]
        public void VerifyThatGetEnabledReturnsExpectedResult()
        {
            var fluentRibbonManager = new FluentRibbonManager();
            fluentRibbonManager.IsActive = true;
            fluentRibbonManager.RegisterRibbonPart(this.ribbonPart);

            Assert.IsFalse(this.ribbonPart.GetEnabled("ShowMeasurementUnits"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("ShowMeasurementScales"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("ShowParameterTypes"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("ShowRules"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("ShowCategories"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("unknownRibbonControlId"));

            var openSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Open);
            this.messageBus.SendMessage(openSessionEvent);

            Assert.IsTrue(this.ribbonPart.GetEnabled("ShowMeasurementUnits"));
            Assert.IsTrue(this.ribbonPart.GetEnabled("ShowMeasurementScales"));
            Assert.IsTrue(this.ribbonPart.GetEnabled("ShowParameterTypes"));
            Assert.IsTrue(this.ribbonPart.GetEnabled("ShowRules"));
            Assert.IsTrue(this.ribbonPart.GetEnabled("ShowCategories"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("unknownRibbonControlId"));

            var closeSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Closed);
            this.messageBus.SendMessage(closeSessionEvent);

            Assert.IsFalse(this.ribbonPart.GetEnabled("ShowMeasurementUnits"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("ShowMeasurementScales"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("ShowParameterTypes"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("ShowRules"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("ShowCategories"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("unknownRibbonControlId"));
        }

        [Test]
        public void VerifyThatImageIsReturned()
        {
            var measurementUnitImage = this.ribbonPart.GetImage("ShowMeasurementUnits");
            Assert.IsNotNull(measurementUnitImage);

            var measurementScalesImage = this.ribbonPart.GetImage("ShowMeasurementScales");
            Assert.IsNotNull(measurementScalesImage);

            var parameterTypesImage = this.ribbonPart.GetImage("ShowParameterTypes");
            Assert.IsNotNull(parameterTypesImage);

            var rulesImage = this.ribbonPart.GetImage("ShowRules");
            Assert.IsNotNull(rulesImage);

            var categoriesImage = this.ribbonPart.GetImage("ShowCategories");
            Assert.IsNotNull(categoriesImage);

            var unknownImage = this.ribbonPart.GetImage("unknownRibbonControlId");
            Assert.IsNull(unknownImage);
        }
    }
}
