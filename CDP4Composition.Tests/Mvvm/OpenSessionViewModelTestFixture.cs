// -------------------------------------------------------------------------------------------------
// <copyright file="OpenSessionViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.Mvvm
{
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Events;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;
    using CDP4Dal;
    using CDP4Dal.Permission;
    using Microsoft.Practices.ServiceLocation;
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;
    using System;
    using System.Reactive.Concurrency;

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
            viewmodel.ShowOrClosePanelCommand.Execute(null);

            this.navigationService.Verify(x => x.Open(It.IsAny<IPanelViewModel>(), true));
        }

        [Test]
        public void VerifyThatClosePanelDoesNotCallNavigationIfPanelNull()
        {
            var viewmodel = new RibbonMenuItemSessionDependentViewModel("test", this.session.Object, MockInstantiate);
            viewmodel.IsChecked = false;
            viewmodel.ShowOrClosePanelCommand.Execute(null);

            this.navigationService.Verify(x => x.Close(It.IsAny<IPanelViewModel>(), true), Times.Never());
        }

        [Test]
        public void VerifyThatClosePanelWorks()
        {
            var viewmodel = new RibbonMenuItemSessionDependentViewModel("test", this.session.Object, MockInstantiate);
            viewmodel.IsChecked = true;
            viewmodel.ShowOrClosePanelCommand.Execute(null);

            viewmodel.IsChecked = false;
            viewmodel.ShowOrClosePanelCommand.Execute(null);
            this.navigationService.Verify(x => x.Close(It.IsAny<IPanelViewModel>(), true));
        }

        [Test]
        public void VerifyThatModelBrowserIsUncheckedUponCloseEvent()
        {
            var viewmodel = new RibbonMenuItemSessionDependentViewModel("test", this.session.Object, MockInstantiate);
            viewmodel.IsChecked = true;
            viewmodel.ShowOrClosePanelCommand.Execute(null);

            var modelbrowser = viewmodel.PanelViewModel;
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
            { }

            public string Caption { get; private set; }
            public string ToolTip { get; private set; }
            public string DataSource { get; private set; }

            public override void ComputePermission()
            { }
        }
    }
}