// -------------------------------------------------------------------------------------------------
// <copyright file="TeamCompositionBrowserViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2017 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Tests.TeamComposition
{
    using System;
    using System.Collections.Concurrent;
    using System.Reactive.Concurrency;
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    using Microsoft.Practices.ServiceLocation;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Permission;
    using CDP4SiteDirectory.ViewModels;
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;

    [TestFixture]
    public class TeamCompositionBrowserRibbonViewModelTestFixture
    {
        private Uri uri;
        private Mock<IDialogNavigationService> dialogNavigationService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IPermissionService> permissionService;
        private Mock<ISession> session;
        private Person person;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private Mock<IServiceLocator> serviceLocator;

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.uri = new Uri("http://www.rheagroup.com");
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

            this.dialogNavigationService = new Mock<IDialogNavigationService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.permissionService = new Mock<IPermissionService>();
            this.session = new Mock<ISession>();

            this.person = new Person(Guid.NewGuid(), this.cache, this.uri);
            this.person.Surname = "John";
            this.person.GivenName = "Doe";

            var engineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri);
            var dialogResult = new EngineeringModelSetupSelectionResult(true, this.session.Object,  engineeringModelSetup);
            this.dialogNavigationService.Setup(x => x.NavigateModal(It.IsAny<IDialogViewModel>())).Returns(dialogResult);

            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatInstantiateWorks()
        {
            var engineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), null, null);
            var vm = TeamCompositionBrowserRibbonViewModel.InstantiatePanelViewModel(engineeringModelSetup, this.session.Object, null, null, null);
            Assert.IsNotNull(vm);
        }
    }
}
