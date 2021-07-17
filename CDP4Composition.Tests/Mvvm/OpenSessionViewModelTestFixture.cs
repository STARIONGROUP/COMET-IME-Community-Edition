// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OpenSessionViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski, Ahmed Abulwafa Ahmed
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

namespace CDP4Composition.Tests.Mvvm
{
    using System;
    using System.Linq;
    using System.Reactive.Concurrency;

    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Events;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;

    using Microsoft.Practices.ServiceLocation;

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

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.uri = new Uri("http://www.rheagroup.com");
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

            ServiceLocator.SetLocatorProvider(new ServiceLocatorProvider(() => this.serviceLocator.Object));
        }

        [Test]
        public void VerifyThatShowPanelWorks()
        {
            var viewmodel = new RibbonMenuItemSessionDependentViewModel("test", this.session.Object, MockInstantiate);
            viewmodel.IsChecked = true;
            viewmodel.ShowPanelCommand.Execute(null);

            this.navigationService.Verify(x => x.OpenInDock(It.IsAny<IPanelViewModel>()));
        }

        [Test]
        public void VerifyThatClosePanelDoesNotCallNavigationIfPanelNull()
        {
            var viewmodel = new RibbonMenuItemSessionDependentViewModel("test", this.session.Object, MockInstantiate);
            viewmodel.IsChecked = false;
            viewmodel.ShowPanelCommand.Execute(null);

            this.navigationService.Verify(x => x.CloseInDock(It.IsAny<IPanelViewModel>()), Times.Never());
        }

        [Test]
        public void VerifyThatModelBrowserIsUncheckedUponCloseEvent()
        {
            var viewmodel = new RibbonMenuItemSessionDependentViewModel("test", this.session.Object, MockInstantiate);
            viewmodel.IsChecked = true;
            viewmodel.ShowPanelCommand.Execute(null);

            var modelbrowser = viewmodel.PanelViewModels.First();
            CDPMessageBus.Current.SendMessage(new NavigationPanelEvent(modelbrowser, this.panelView.Object, PanelStatus.Closed));
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
            public string TargetName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public override void ComputePermission()
            {
            }
        }
    }
}
