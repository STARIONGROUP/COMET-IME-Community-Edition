// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterSheetGeneratorRibbonPartTestFixture.cs" company="Starion Group S.A.">
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

namespace CDP4ParameterSheetGenerator.Tests.OfficeRibbon
{
    using System;
    using System.Collections.Generic;
    using System.IO.Packaging;
    using System.Reactive.Concurrency;
    using System.Threading;
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using CDP4OfficeInfrastructure;

    using CommonServiceLocator;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="ParameterSheetGeneratorRibbonPart"/>
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class ParameterSheetGeneratorRibbonPartTestFixture
    {
        private Uri uri;

        /// <summary>
        /// The <see cref="RibbonPart"/> under test
        /// </summary>
        private ParameterSheetGeneratorRibbonPart ribbonPart;

        private int amountOfRibbonControls;

        private int order;

        private string ribbonxmlname;

        private Mock<IServiceLocator> serviceLocator;
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IDialogNavigationService> dialogNavigationService;
        private Mock<IPermissionService> permittingPermissionService;
        private Mock<IPluginSettingsService> pluginSettingsService;

        private Mock<IOfficeApplicationWrapper> officeApplicationWrapper;
        private Mock<IExcelQuery> excelQuery;

        private Mock<ISession> session;

        private Assembler assembler;
        private CDPMessageBus messageBus;

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.messageBus = new CDPMessageBus();
            this.uri = new Uri("https://www.stariongroup.eu");
            this.SetupRecognizePackUir();

            this.assembler = new Assembler(this.uri, this.messageBus);

            this.session = new Mock<ISession>();

            var dtos = new List<CDP4Common.DTO.Thing>();

            var siteDirectory = new CDP4Common.DTO.SiteDirectory(Guid.NewGuid(), 0);
            dtos.Add(siteDirectory);
            var engineeringModel = new CDP4Common.DTO.EngineeringModel(Guid.NewGuid(), 0);
            dtos.Add(engineeringModel);
            var iteration = new CDP4Common.DTO.Iteration(Guid.NewGuid(), 0);
            engineeringModel.Iteration.Add(iteration.Iid);
            dtos.Add(iteration);

            this.assembler.Synchronize(dtos);

            this.session.Setup(x => x.DataSourceUri).Returns("test");
            this.session.Setup(x => x.Assembler).Returns(this.assembler);

            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.dialogNavigationService = new Mock<IDialogNavigationService>();
            this.serviceLocator = new Mock<IServiceLocator>();

            this.excelQuery = new Mock<IExcelQuery>();
            this.excelQuery.Setup(x => x.IsActiveWorkbookAvailable(It.IsAny<NetOffice.ExcelApi.Application>())).Returns(true);
            this.officeApplicationWrapper = new Mock<IOfficeApplicationWrapper>();

            this.permittingPermissionService = new Mock<IPermissionService>();
            this.permittingPermissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permittingPermissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);

            this.pluginSettingsService = new Mock<IPluginSettingsService>();

            this.amountOfRibbonControls = 10;
            this.order = 1;

            this.ribbonPart = new ParameterSheetGeneratorRibbonPart(this.order, this.panelNavigationService.Object, this.thingDialogNavigationService.Object, this.dialogNavigationService.Object, this.pluginSettingsService.Object, this.officeApplicationWrapper.Object, this.messageBus);

            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);

            this.serviceLocator.Setup(x => x.GetInstance<IThingDialogNavigationService>())
                .Returns(this.thingDialogNavigationService.Object);
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
                Application.ResourceAssembly = typeof(ParameterSheetGeneratorRibbonPart).Assembly;
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
        public void VerifyThatGetEnabledReturnsExpectedResult()
        {
            var fluentRibbonManager = new FluentRibbonManager();
            fluentRibbonManager.IsActive = true;
            fluentRibbonManager.RegisterRibbonPart(this.ribbonPart);

            Assert.IsFalse(this.ribbonPart.GetEnabled("Rebuild"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("SynchronizeAll"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("SynchronizeParameters"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("SynchronizeSubscriptions"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("SubmitAll"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("SubmitParameters"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("SubmitSubscriptions"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("unknownRibbonControlId"));

            var openSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Open);
            this.messageBus.SendMessage(openSessionEvent);

            this.ribbonPart.ExcelQuery = this.excelQuery.Object;

            Assert.IsFalse(this.ribbonPart.GetEnabled("Rebuild"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("SynchronizeAll"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("SynchronizeParameters"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("SynchronizeSubscriptions"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("SubmitAll"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("SubmitParameters"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("SubmitSubscriptions"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("unknownRibbonControlId"));

            var closeSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Closed);
            this.messageBus.SendMessage(closeSessionEvent);

            Assert.IsFalse(this.ribbonPart.GetEnabled("Rebuild"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("SynchronizeAll"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("SynchronizeParameters"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("SynchronizeSubscriptions"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("SubmitAll"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("SubmitParameters"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("SubmitSubscriptions"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("unknownRibbonControlId"));
        }

        [Test]
        public void VerifyThatButtonsAreEnabledWhenIterationIsLoaded()
        {
            var fluentRibbonManager = new FluentRibbonManager();
            fluentRibbonManager.IsActive = true;
            fluentRibbonManager.RegisterRibbonPart(this.ribbonPart);

            Assert.IsFalse(this.ribbonPart.GetEnabled("Rebuild"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("SynchronizeAll"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("SynchronizeParameters"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("SynchronizeSubscriptions"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("SubmitAll"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("SubmitParameters"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("SubmitSubscriptions"));
            Assert.IsFalse(this.ribbonPart.GetEnabled("unknownRibbonControlId"));

            var openSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Open);
            this.messageBus.SendMessage(openSessionEvent);

            this.ribbonPart.ExcelQuery = this.excelQuery.Object;

            var iterationSetup = new IterationSetup(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                IterationNumber = 1
            };

            var iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri);
            iteration.IterationSetup = iterationSetup;

            this.messageBus.SendObjectChangeEvent(iteration, EventKind.Added);

            Assert.IsTrue(this.ribbonPart.GetEnabled("Rebuild"));

            // the other ribbon buttons depend on NetOffice and cannot be properly unit tested
        }

        [Test]
        public void VerifyThatWhenSessionIsClosedAfterModelWasOpenIterationsAreCleaned()
        {
            var fluentRibbonManager = new FluentRibbonManager();
            fluentRibbonManager.IsActive = true;
            fluentRibbonManager.RegisterRibbonPart(this.ribbonPart);

            Assert.IsFalse(this.ribbonPart.GetEnabled("Rebuild"));

            var openSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Open);
            this.messageBus.SendMessage(openSessionEvent);

            this.ribbonPart.ExcelQuery = this.excelQuery.Object;

            var iterationSetup = new IterationSetup(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                IterationNumber = 1
            };

            var iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri);
            iteration.IterationSetup = iterationSetup;

            this.messageBus.SendObjectChangeEvent(iteration, EventKind.Added);

            Assert.IsTrue(this.ribbonPart.GetEnabled("Rebuild"));

            Assert.AreEqual(1, this.ribbonPart.Iterations.Count);

            var closeSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Closed);
            this.messageBus.SendMessage(closeSessionEvent);

            Assert.IsFalse(this.ribbonPart.GetEnabled("Rebuild"));

            Assert.AreEqual(0, this.ribbonPart.Iterations.Count);
        }
    }
}
