// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BasicRdlRibbonPartTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
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
    using Microsoft.Practices.ServiceLocation;
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;
    
    /// <summary>
    /// TestFixture for the <see cref="BasicRdlRibbonPart"/>
    /// </summary>
    [TestFixture, Apartment(ApartmentState.STA)]    
    public class BasicRdlRibbonPartTestFixture
    {
        private Uri uri;

        /// <summary>
        /// The <see cref="RibbonPart"/> under test
        /// </summary>
        private BasicRdlRibbonPart ribbonPart;
        private int amountOfRibbonControls;
        private int order;
        private string ribbonxmlname;
        private Mock<IServiceLocator> serviceLocator;

        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IThingDialogNavigationService> dialogNavigationService;
        private Mock<IPermissionService> permittingPermissionService;
        private Mock<ISession> session;
        private Person person;
        private Mock<IFavoritesService> favoritesService;
        private Mock<IFilterStringService> filterStringService;

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.uri = new Uri("http://www.rheageoup.com");
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

            this.amountOfRibbonControls = 6;
            this.order = 1;

            this.ribbonPart = new BasicRdlRibbonPart(this.order, this.panelNavigationService.Object, null, null, null, this.favoritesService.Object);

            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IThingDialogNavigationService>()).Returns(this.dialogNavigationService.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IFilterStringService>()).Returns(this.filterStringService.Object);
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
                Application.ResourceAssembly = typeof(BasicRdlRibbonPart).Assembly;
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
        public void VerifyThatIfFluentRibbonIsNotActiveTheSessionEventHasNoEffect()
        {
            var fluentRibbonManager = new FluentRibbonManager();
            fluentRibbonManager.IsActive = false;
            fluentRibbonManager.RegisterRibbonPart(this.ribbonPart);

            var sessionEvent = new SessionEvent(this.session.Object, SessionStatus.Open);
            CDPMessageBus.Current.SendMessage(sessionEvent);
            Assert.IsNull(this.ribbonPart.Session);
        }

        [Test]
        public void VerifyThatIfFluentRibbonIsNullTheSessionEventHasNoEffect()
        {
            var sessionEvent = new SessionEvent(this.session.Object, SessionStatus.Open);
            CDPMessageBus.Current.SendMessage(sessionEvent);
            Assert.IsNull(this.ribbonPart.Session);
        }

        [Test]
        public void VerifyThatRibbonPartHandlesSessionOpenAndCloseEvent()
        {
            var fluentRibbonManager = new FluentRibbonManager();
            fluentRibbonManager.IsActive = true;
            fluentRibbonManager.RegisterRibbonPart(this.ribbonPart);

            var openSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Open);
            CDPMessageBus.Current.SendMessage(openSessionEvent);

            Assert.AreEqual(this.session.Object, this.ribbonPart.Session);

            var closeSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Closed);
            CDPMessageBus.Current.SendMessage(closeSessionEvent);

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
            CDPMessageBus.Current.SendMessage(openSessionEvent);

            await this.ribbonPart.OnAction("ShowMeasurementUnits");
            this.panelNavigationService.Verify(x => x.Open(It.IsAny<MeasurementUnitsBrowserViewModel>(), false));

            await this.ribbonPart.OnAction("ShowMeasurementScales");
            this.panelNavigationService.Verify(x => x.Open(It.IsAny<MeasurementScalesBrowserViewModel>(), false));

            await this.ribbonPart.OnAction("ShowParameterTypes");
            this.panelNavigationService.Verify(x => x.Open(It.IsAny<ParameterTypesBrowserViewModel>(), false));

            await this.ribbonPart.OnAction("ShowRules");
            this.panelNavigationService.Verify(x => x.Open(It.IsAny<RulesBrowserViewModel>(), false));

            await this.ribbonPart.OnAction("ShowCategories");
            this.panelNavigationService.Verify(x => x.Open(It.IsAny<CategoryBrowserViewModel>(), false));

            // close viewmodels
            var closeSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Closed);
            CDPMessageBus.Current.SendMessage(closeSessionEvent);

            this.panelNavigationService.Verify(x => x.Close(It.IsAny<MeasurementUnitsBrowserViewModel>(), false));
            this.panelNavigationService.Verify(x => x.Close(It.IsAny<MeasurementScalesBrowserViewModel>(), false));
            this.panelNavigationService.Verify(x => x.Close(It.IsAny<ParameterTypesBrowserViewModel>(), false));
            this.panelNavigationService.Verify(x => x.Close(It.IsAny<RulesBrowserViewModel>(), false));
            this.panelNavigationService.Verify(x => x.Close(It.IsAny<CategoryBrowserViewModel>(), false));
        }

        [Test]
        public async Task VerifyThatOnActionDoesNotInvokePanelNavigationWhenSessionIsNull()
        {
            await this.ribbonPart.OnAction("ShowMeasurementUnits");
            this.panelNavigationService.Verify(x => x.Open(It.IsAny<MeasurementUnitsBrowserViewModel>(), false), Times.Never);
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
            CDPMessageBus.Current.SendMessage(openSessionEvent);

            Assert.IsTrue(this.ribbonPart.GetEnabled("ShowMeasurementUnits"));
            Assert.IsTrue(this.ribbonPart.GetEnabled("ShowMeasurementScales"));
            Assert.IsTrue(this.ribbonPart.GetEnabled("ShowParameterTypes"));
            Assert.IsTrue(this.ribbonPart.GetEnabled("ShowRules"));
            Assert.IsTrue(this.ribbonPart.GetEnabled("ShowCategories"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("unknownRibbonControlId"));

            var closeSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Closed);
            CDPMessageBus.Current.SendMessage(closeSessionEvent);

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