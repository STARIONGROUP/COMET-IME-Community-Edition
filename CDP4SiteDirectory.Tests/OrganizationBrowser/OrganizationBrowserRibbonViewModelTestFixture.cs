// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OrganizationBrowserRibbonViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace CDP4SiteDirectory.Tests.OrganizationBrowser
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
    public class OrganizationBrowserRibbonViewModelTestFixture
    {
        private Uri uri;
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IPanelNavigationService> navigationService;
        private Mock<IDialogNavigationService> dialogNavigationService;
        private Mock<IPluginSettingsService> pluginSettingsService;
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Person person;
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.messageBus = new CDPMessageBus();
            this.uri = new Uri("https://www.stariongroup.eu");
            this.session = new Mock<ISession>();
            this.serviceLocator = new Mock<IServiceLocator>();
            this.navigationService = new Mock<IPanelNavigationService>();

            var siteDirectory = new SiteDirectory(Guid.NewGuid(), null, null);
            this.person = new Person(Guid.NewGuid(), null, this.uri) { GivenName = "John", Surname = "Doe" };

            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString);
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(siteDirectory);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);

            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);

            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.dialogNavigationService = new Mock<IDialogNavigationService>();
            this.pluginSettingsService = new Mock<IPluginSettingsService>();

            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);

            ServiceLocator.SetLocatorProvider(new ServiceLocatorProvider(() => this.serviceLocator.Object));

            this.serviceLocator.Setup(x => x.GetInstance<IPermissionService>()).Returns(this.permissionService.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IThingDialogNavigationService>()).Returns(this.thingDialogNavigationService.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IPanelNavigationService>()).Returns(this.navigationService.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IDialogNavigationService>()).Returns(this.dialogNavigationService.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IPluginSettingsService>()).Returns(this.pluginSettingsService.Object);
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatSessionArePopulated()
        {
            var viewmodel = new OrganizationBrowserRibbonViewModel(this.messageBus);

            this.messageBus.SendMessage(new SessionEvent(this.session.Object, SessionStatus.Open));
            Assert.AreEqual(1, viewmodel.OpenSessions.Count);

            this.messageBus.SendMessage(new SessionEvent(this.session.Object, SessionStatus.Closed));
            Assert.AreEqual(0, viewmodel.OpenSessions.Count);
        }

        [Test]
        public async Task VerifyThatOpenCloseSingleBrowserWorks()
        {
            var vm = new OrganizationBrowserRibbonViewModel(this.messageBus);

            await vm.OpenSingleBrowserCommand.Execute();
            Assert.Throws<MockException>(() => this.navigationService.Verify(x => x.OpenInDock(It.IsAny<IPanelViewModel>())));

            this.messageBus.SendMessage(new SessionEvent(this.session.Object, SessionStatus.Open));
            await vm.OpenSingleBrowserCommand.Execute();

            this.navigationService.Verify(x => x.OpenInDock(It.IsAny<IPanelViewModel>()), Times.Exactly(1));

            await vm.OpenSingleBrowserCommand.Execute();
            this.navigationService.Verify(x => x.OpenInDock(It.IsAny<IPanelViewModel>()), Times.Exactly(2));
        }

        [Test]
        public void Verify_That_RibbonViewModel_Can_Be_Constructed()
        {
            Assert.DoesNotThrow(() => new OrganizationBrowserRibbonViewModel(this.messageBus));
        }

        [Test]
        public void Verify_That_InstantiatePanelViewModel_Returns_Expected_ViewModel()
        {
            var viewmodel = OrganizationBrowserRibbonViewModel.InstantiatePanelViewModel(
                this.session.Object,
                this.thingDialogNavigationService.Object,
                this.navigationService.Object,
                this.dialogNavigationService.Object,
                this.pluginSettingsService.Object);

            Assert.IsInstanceOf<OrganizationBrowserViewModel>(viewmodel);
        }
    }
}
