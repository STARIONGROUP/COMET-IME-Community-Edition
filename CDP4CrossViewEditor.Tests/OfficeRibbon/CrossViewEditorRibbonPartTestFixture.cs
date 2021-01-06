// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CrossViewEditorRibbonPartTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Cozmin Velciu, Adrian Chivu
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

namespace CDP4CrossViewEditor.Tests.OfficeRibbon
{
    using System;
    using System.Reactive.Concurrency;
    using System.Threading;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;
    using CDP4Dal.Events;

    using CDP4OfficeInfrastructure;

    using Microsoft.Practices.ServiceLocation;

    using Moq;

    using NetOffice.ExcelApi;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="CrossViewEditorRibbonPart"/>
    /// </summary>
    [TestFixture, Apartment(ApartmentState.STA)]
    public class CrossViewEditorRibbonPartTestFixture
    {
        /// <summary>
        /// The current ribbon button label
        /// </summary>
        private const string RibbonButtonId = "Editor";

        /// <summary>
        /// The current session associated <see cref="Assembler"></see>
        /// </summary>
        private Assembler assembler;

        /// <summary>
        /// The current assembler <see cref="Uri"/>
        /// </summary>
        private Uri uri;

        /// <summary>
        /// The current active person <see cref="Person"/>
        /// </summary>
        private Person person;

        /// <summary>
        /// The <see cref="RibbonPart"/> under test
        /// </summary>
        private CrossViewEditorRibbonPart ribbonPart;

        /// <summary>
        /// Total amounf of Ribbon controls
        /// </summary>
        private int amountOfRibbonControls;

        /// <summary>
        /// Ribbon button index order
        /// </summary>
        private int order;

        /// <summary>
        /// Mock <see cref="IServiceLocator"/>
        /// </summary>
        private Mock<IServiceLocator> serviceLocator;

        /// <summary>
        /// Mock <see cref="IPanelNavigationService"/>
        /// </summary>
        private Mock<IPanelNavigationService> panelNavigationService;

        /// <summary>
        /// Mock <see cref="IThingDialogNavigationService"/>
        /// </summary>
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;

        /// <summary>
        /// Mock <see cref="IDialogNavigationService"/>
        /// </summary>
        private Mock<IDialogNavigationService> dialogNavigationService;

        /// <summary>
        /// Mock <see cref="IPluginSettingsService"/>
        /// </summary>
        private Mock<IPluginSettingsService> pluginSettingsService;

        /// <summary>
        /// Mock <see cref="IOfficeApplicationWrapper"/>
        /// </summary>
        private Mock<IOfficeApplicationWrapper> officeApplicationWrapper;

        /// <summary>
        /// Mock <see cref="ISession"/>
        /// </summary>
        private Mock<ISession> session;

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.uri = new Uri("http://www.rheageoup.com");
            this.person = new Person(Guid.NewGuid(), null, this.uri) { GivenName = "John", Surname = "Doe" };
            this.assembler = new Assembler(this.uri);
            this.session = new Mock<ISession>();
            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);

            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.dialogNavigationService = new Mock<IDialogNavigationService>();
            this.serviceLocator = new Mock<IServiceLocator>();
            this.officeApplicationWrapper = new Mock<IOfficeApplicationWrapper>();
            this.officeApplicationWrapper.Setup(x => x.Excel).Returns(It.IsAny<Application>);
            this.pluginSettingsService = new Mock<IPluginSettingsService>();

            this.amountOfRibbonControls = 2;
            this.order = 1000;

            this.ribbonPart = new CrossViewEditorRibbonPart(this.order, this.panelNavigationService.Object, this.thingDialogNavigationService.Object, this.dialogNavigationService.Object, this.pluginSettingsService.Object, this.officeApplicationWrapper.Object);

            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);

            this.serviceLocator.Setup(x => x.GetInstance<IThingDialogNavigationService>())
                .Returns(this.thingDialogNavigationService.Object);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
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
            var fluentRibbonManager = new FluentRibbonManager { IsActive = false };
            fluentRibbonManager.RegisterRibbonPart(this.ribbonPart);

            var sessionEvent = new SessionEvent(this.session.Object, SessionStatus.Open);
            CDPMessageBus.Current.SendMessage(sessionEvent);
            Assert.IsNull(this.ribbonPart.Session);

            var closeSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Closed);
            CDPMessageBus.Current.SendMessage(closeSessionEvent);
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
            var fluentRibbonManager = new FluentRibbonManager { IsActive = true };
            fluentRibbonManager.RegisterRibbonPart(this.ribbonPart);

            var openSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Open);
            CDPMessageBus.Current.SendMessage(openSessionEvent);

            Assert.AreEqual(this.session.Object, this.ribbonPart.Session);

            var closeSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Closed);
            CDPMessageBus.Current.SendMessage(closeSessionEvent);

            Assert.IsNull(this.ribbonPart.Session);
        }

        [Test]
        public void VerifyThatGetEnabledReturnsExpectedResult()
        {
            var fluentRibbonManager = new FluentRibbonManager { IsActive = true };
            fluentRibbonManager.RegisterRibbonPart(this.ribbonPart);

            Assert.IsFalse(this.ribbonPart.GetEnabled(RibbonButtonId));

            var openSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Open);
            CDPMessageBus.Current.SendMessage(openSessionEvent);

            Assert.IsFalse(this.ribbonPart.GetEnabled(RibbonButtonId));

            var closeSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Closed);
            CDPMessageBus.Current.SendMessage(closeSessionEvent);

            Assert.IsFalse(this.ribbonPart.GetEnabled(RibbonButtonId));
        }

        [Test]
        public void VerifyThatButtonsAreEnabledWhenIterationIsLoaded()
        {
            var fluentRibbonManager = new FluentRibbonManager { IsActive = true };
            fluentRibbonManager.RegisterRibbonPart(this.ribbonPart);

            Assert.IsFalse(this.ribbonPart.GetEnabled(RibbonButtonId));

            var openSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Open);
            CDPMessageBus.Current.SendMessage(openSessionEvent);
            CDPMessageBus.Current.SendObjectChangeEvent(this.CreateIteration(), EventKind.Added);

            Assert.IsTrue(this.ribbonPart.GetEnabled(RibbonButtonId));

            var closeSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Closed);
            CDPMessageBus.Current.SendMessage(closeSessionEvent);
        }

        [Test]
        public void VerifyThatWhenSessionIsClosedAfterModelWasOpenIterationsAreCleaned()
        {
            var fluentRibbonManager = new FluentRibbonManager { IsActive = true };
            fluentRibbonManager.RegisterRibbonPart(this.ribbonPart);

            Assert.IsFalse(this.ribbonPart.GetEnabled(RibbonButtonId));

            var openSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Open);
            CDPMessageBus.Current.SendMessage(openSessionEvent);
            CDPMessageBus.Current.SendObjectChangeEvent(this.CreateIteration(), EventKind.Added);

            Assert.IsTrue(this.ribbonPart.GetEnabled(RibbonButtonId));
            Assert.AreEqual(1, this.ribbonPart.Iterations.Count);

            var closeSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Closed);
            CDPMessageBus.Current.SendMessage(closeSessionEvent);

            Assert.IsFalse(this.ribbonPart.GetEnabled(RibbonButtonId));
            Assert.AreEqual(0, this.ribbonPart.Iterations.Count);
        }

        [Test]
        public void VerifyThatOnActionIsExecuted()
        {
            var fluentRibbonManager = new FluentRibbonManager { IsActive = true };
            fluentRibbonManager.RegisterRibbonPart(this.ribbonPart);

            var openSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Open);
            CDPMessageBus.Current.SendMessage(openSessionEvent);

            var iteration = this.CreateIteration();

            CDPMessageBus.Current.SendObjectChangeEvent(iteration, EventKind.Added);
            Assert.AreEqual(1, this.ribbonPart.Iterations.Count);

            this.ribbonPart.GetContent(RibbonButtonId);

            Assert.DoesNotThrowAsync(async () => await this.ribbonPart.OnAction($"Editor_{iteration.Iid}", iteration.Iid.ToString()));

            CDPMessageBus.Current.SendObjectChangeEvent(iteration, EventKind.Removed);
            Assert.AreEqual(0, this.ribbonPart.Iterations.Count);

            var closeSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Closed);
            CDPMessageBus.Current.SendMessage(closeSessionEvent);
        }

        [Test]
        public void VerifyThatOnActionIsNotExecutedWhenSessionIsNull()
        {
            var fluentRibbonManager = new FluentRibbonManager { IsActive = true };
            fluentRibbonManager.RegisterRibbonPart(this.ribbonPart);

            var iteration = this.CreateIteration();
            CDPMessageBus.Current.SendObjectChangeEvent(iteration, EventKind.Added);

            Assert.DoesNotThrowAsync(async () => await this.ribbonPart.OnAction($"Editor_{iteration.Iid}", iteration.Iid.ToString()));

            Assert.AreEqual(0, this.ribbonPart.Iterations.Count);
        }

        [Test]
        public void VerifyThatOnActionIsNotExecutedWhenInvalidControlIsSpecified()
        {
            var fluentRibbonManager = new FluentRibbonManager { IsActive = true };
            fluentRibbonManager.RegisterRibbonPart(this.ribbonPart);

            var openSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Open);
            CDPMessageBus.Current.SendMessage(openSessionEvent);

            var iteration = this.CreateIteration();

            Assert.DoesNotThrowAsync(async () => await this.ribbonPart.OnAction($"Editor", iteration.Iid.ToString()));

            Assert.AreEqual(0, this.ribbonPart.Iterations.Count);
        }

        [Test]
        public void VerifyThatOnActionIsNotExecutedWhenInterationIsNull()
        {
            var fluentRibbonManager = new FluentRibbonManager { IsActive = true };
            fluentRibbonManager.RegisterRibbonPart(this.ribbonPart);

            Assert.DoesNotThrowAsync(async () => await this.ribbonPart.OnAction($"Editor_", string.Empty));

            Assert.AreEqual(0, this.ribbonPart.Iterations.Count);
        }

        /// <summary>
        /// Create a new iteration in order to simulate model opening
        /// </summary>
        /// <returns></returns>
        private Iteration CreateIteration()
        {
            return new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Container = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.uri)
                {
                    EngineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.uri)
                },
                IterationSetup = new IterationSetup(Guid.NewGuid(), this.assembler.Cache, this.uri)
                {
                    IterationNumber = 1
                }
            };
        }
    }
}
