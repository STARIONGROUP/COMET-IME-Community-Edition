﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NaturalLanguageRibbonViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace CDP4SiteDirectory.Tests.ViewModels
{
    using System;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using CDP4SiteDirectory.ViewModels;

    using CommonServiceLocator;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    public class NaturalLanguageRibbonViewModelTestFixture
    {
        private Mock<ISession> session;
        private Uri uri;
        private SiteDirectory siteDir;
        private Person person;
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IPanelNavigationService> navigationService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IDialogNavigationService> dialogNavigationService;
        private Mock<IPluginSettingsService> pluginSettingsService;
        private Mock<IPanelViewModel> panelViewModel;
        private Assembler assembler;
        private Mock<IPermissionService> permissionService;
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);

            this.messageBus = new CDPMessageBus();
            this.session = new Mock<ISession>();
            this.uri = new Uri("http://www.reahroup.com");

            this.siteDir = new SiteDirectory(Guid.NewGuid(), null, this.uri) { Name = "site dir" };
            this.person = new Person(Guid.NewGuid(), null, this.uri) { GivenName = "John", Surname = "Doe" };

            var language = new NaturalLanguage(Guid.NewGuid(), null, this.uri)
            {
                Name = "test",
                LanguageCode = "te",
                NativeName = "test"
            };

            this.siteDir.NaturalLanguage.Add(language);

            this.serviceLocator = new Mock<IServiceLocator>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.navigationService = new Mock<IPanelNavigationService>();
            this.dialogNavigationService = new Mock<IDialogNavigationService>();
            this.pluginSettingsService = new Mock<IPluginSettingsService>();
            this.session = new Mock<ISession>();
            this.panelViewModel = new Mock<IPanelViewModel>();

            this.serviceLocator.Setup(x => x.GetInstance<IPanelNavigationService>())
                .Returns(this.navigationService.Object);

            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString);
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);

            ServiceLocator.SetLocatorProvider(new ServiceLocatorProvider(() => this.serviceLocator.Object));

            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);

            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatPropertiesArePopulated()
        {
            var viewmodel = new NaturalLanguageRibbonViewModel(this.messageBus);
            var sessionEvent = new SessionEvent(this.session.Object, SessionStatus.Open);
            this.messageBus.SendMessage(sessionEvent);

            Assert.AreEqual(1, viewmodel.OpenSessions.Count);
        }

        [Test]
        public void VerifyThatSessionCloseRemoveFromList()
        {
            var viewmodel = new NaturalLanguageRibbonViewModel(this.messageBus);
            var sessionEvent = new SessionEvent(this.session.Object, SessionStatus.Open);
            var sessionClose = new SessionEvent(this.session.Object, SessionStatus.Closed);
            this.messageBus.SendMessage(sessionEvent);
            this.messageBus.SendMessage(sessionClose);

            Assert.AreEqual(0, viewmodel.OpenSessions.Count);
        }

        [Test]
        public async Task VerifyThatOpenSinglePanelCommandWorks()
        {
            var viewmodel = new NaturalLanguageRibbonViewModel(this.messageBus);
            var sessionEvent = new SessionEvent(this.session.Object, SessionStatus.Open);
            this.messageBus.SendMessage(sessionEvent);

            // Simulate a click on the button open when theres a unique session open
            await viewmodel.OpenSingleBrowserCommand.Execute();
            this.navigationService.Verify(x => x.OpenInDock(It.IsAny<IPanelViewModel>()));
        }

        [Test]
        public void VerifyThatRibbonViewModelCanBeConstructed()
        {
            Assert.DoesNotThrow(() => new DomainOfExpertiseRibbonViewModel(this.messageBus));
        }

        [Test]
        public void VerifyThatInstantiatePanelViewModelReturnsExpectedViewModel()
        {
            var viewmodel = DomainOfExpertiseRibbonViewModel.InstantiatePanelViewModel(
                this.session.Object,
                this.thingDialogNavigationService.Object,
                this.navigationService.Object,
                this.dialogNavigationService.Object,
                this.pluginSettingsService.Object);

            Assert.IsInstanceOf<DomainOfExpertiseBrowserViewModel>(viewmodel);
        }
    }
}
