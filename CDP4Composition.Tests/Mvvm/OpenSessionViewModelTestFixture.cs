// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OpenSessionViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace CDP4Composition.Tests.Mvvm
{
    using System;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Events;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;

    using CommonServiceLocator;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    public class OpenSessionViewModelTestFixture
    {
        private Mock<IServiceLocator> serviceLocator;

        private Mock<IPanelNavigationService> navigationService;

        private Mock<IPanelView> panelView;

        private Mock<ISession> session;

        private Assembler assembler;

        private Uri uri;

        private Person person;

        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.messageBus = new CDPMessageBus();
            this.uri = new Uri("https://www.stariongroup.eu");
            this.serviceLocator = new Mock<IServiceLocator>();
            this.navigationService = new Mock<IPanelNavigationService>();
            this.session = new Mock<ISession>();
            this.panelView = new Mock<IPanelView>();

            var siteDirectory = new SiteDirectory(Guid.NewGuid(), null, this.uri);
            this.person = new Person(Guid.NewGuid(), null, this.uri) { GivenName = "John", Surname = "Doe" };

            this.serviceLocator.Setup(x => x.GetInstance<IPanelNavigationService>())
                .Returns(this.navigationService.Object);

            this.session.Setup(x => x.DataSourceUri).Returns("test");
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(siteDirectory);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);

            ServiceLocator.SetLocatorProvider(new ServiceLocatorProvider(() => this.serviceLocator.Object));
        }

        [Test]
        public async Task VerifyThatShowPanelWorks()
        {
            var viewmodel = new RibbonMenuItemSessionDependentViewModel("test", this.session.Object, MockInstantiate);
            viewmodel.IsChecked = true;
            await viewmodel.ShowPanelCommand.Execute();

            this.navigationService.Verify(x => x.OpenInDock(It.IsAny<IPanelViewModel>()));
        }

        [Test]
        public void VerifyThatClosePanelDoesNotCallNavigationIfPanelNull()
        {
            var viewmodel = new RibbonMenuItemSessionDependentViewModel("test", this.session.Object, MockInstantiate);
            viewmodel.IsChecked = false;
            viewmodel.ShowPanelCommand.Execute();

            this.navigationService.Verify(x => x.CloseInDock(It.IsAny<IPanelViewModel>()), Times.Never());
        }

        [Test]
        public async Task VerifyThatModelBrowserIsUncheckedUponCloseEvent()
        {
            var viewmodel = new RibbonMenuItemSessionDependentViewModel("test", this.session.Object, MockInstantiate);
            viewmodel.IsChecked = true;
            await viewmodel.ShowPanelCommand.Execute();

            var modelbrowser = viewmodel.PanelViewModels.First();
            this.messageBus.SendMessage(new NavigationPanelEvent(modelbrowser, this.panelView.Object, PanelStatus.Closed));
            Assert.IsFalse(viewmodel.IsChecked);
        }

        private static IPanelViewModel MockInstantiate(ISession ses, IThingDialogNavigationService thingDialogService, IPanelNavigationService panelService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
        {
            return new PanelViewModel(ses, ses.RetrieveSiteDirectory(), thingDialogService, panelService, dialogNavigationService, pluginSettingsService);
        }

        private class PanelViewModel : BrowserViewModelBase<SiteDirectory>, IPanelViewModel
        {
            public PanelViewModel(ISession session, SiteDirectory siteDir, IThingDialogNavigationService dialogNav, IPanelNavigationService panelNav, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
                : base(siteDir, session, dialogNav, panelNav, dialogNavigationService, pluginSettingsService)
            {
            }

            public string Caption { get; private set; }

            public string ToolTip { get; private set; }

            public string DataSource { get; private set; }

            public string TargetName { get; set; }

            public override void ComputePermission()
            {
            }
        }
    }
}
