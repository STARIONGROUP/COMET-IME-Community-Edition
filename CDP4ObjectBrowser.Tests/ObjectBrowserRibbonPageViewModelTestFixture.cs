// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ObjectBrowserRibbonPageViewModelTestFixture.cs" company="RHEA System S.A.">
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

namespace CDP4ObjectBrowser.Tests
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

    using CDP4ObjectBrowser.ViewModels;

    using CommonServiceLocator;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    public class ObjectBrowserRibbonPageViewModelTestFixture
    {
        private Mock<IPanelNavigationService> navigationService;
        private Mock<IThingDialogNavigationService> thingNavigationService;
        private Mock<IDialogNavigationService> dialogNavigationService;
        private Mock<IPluginSettingsService> pluginSettingService;
        private Mock<IServiceLocator> servicelocator;
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Uri uri;
        private Person person;
        private SiteDirectory siteDirectory;
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.messageBus = new CDPMessageBus();
            this.uri = new Uri("http://www.rheagroup.com");

            this.person = new Person { GivenName = "John", Surname = "Doe" };
            this.siteDirectory = new SiteDirectory();

            this.session = new Mock<ISession>();

            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDirectory);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);

            this.navigationService = new Mock<IPanelNavigationService>();
            this.thingNavigationService = new Mock<IThingDialogNavigationService>();
            this.dialogNavigationService = new Mock<IDialogNavigationService>();
            this.pluginSettingService = new Mock<IPluginSettingsService>();

            this.servicelocator = new Mock<IServiceLocator>();

            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);

            ServiceLocator.SetLocatorProvider(() => this.servicelocator.Object);

            this.servicelocator.Setup(x => x.GetInstance<IPanelNavigationService>())
                .Returns(this.navigationService.Object);

            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public async Task VerifyThatCommandWorks()
        {
            var vm = new ObjectBrowserRibbonPageViewModel(this.messageBus);
            Assert.IsEmpty(vm.OpenSessions);

            this.messageBus.SendMessage(new SessionEvent(this.session.Object, SessionStatus.Open));

            Assert.AreEqual(1, vm.OpenSessions.Count);

            await vm.OpenSingleBrowserCommand.Execute();

            this.navigationService.Verify(x => x.OpenInDock(It.IsAny<IPanelViewModel>()), Times.Exactly(1));

            await vm.OpenSingleBrowserCommand.Execute();
            this.navigationService.Verify(x => x.OpenInDock(It.IsAny<IPanelViewModel>()), Times.Exactly(2));
        }

        [Test]
        public void Verify_That_RibbonViewModel_Can_Be_Constructed()
        {
            var vm = new ObjectBrowserRibbonPageViewModel(this.messageBus);
            Assert.IsNotNull(vm);
        }

        [Test]
        public void Verify_That_InstantiatePanelViewModel_Returns_Expected_ViewModel()
        {
            var viewModel = ObjectBrowserRibbonPageViewModel.InstantiatePanelViewModel(
                this.session.Object,
                this.thingNavigationService.Object,
                this.navigationService.Object,
                this.dialogNavigationService.Object,
                this.pluginSettingService.Object);

            Assert.IsInstanceOf<ObjectBrowserViewModel>(viewModel);
        }
    }
}
