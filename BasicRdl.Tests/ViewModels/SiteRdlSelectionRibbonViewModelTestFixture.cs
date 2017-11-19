// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SiteRdlSelectionRibbonViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BasicRdl.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Concurrency;

    using BasicRdl.ViewModels;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;    
    using Microsoft.Practices.ServiceLocation;
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;

    [TestFixture]
    public class SiteRdlSelectionRibbonViewModelTestFixture
    {
        private Uri uri;
        private Mock<IServiceLocator> serviceLocator;
        private Mock<ISession> session;
        private Mock<IDialogNavigationService> navigationService;
        private Mock<IDialogViewModel> siteRdlSelectionViewModel;
        private SiteRdlSelectionDialogResult result;
        private Mock<IPermissionService> permissionService;
        private SiteReferenceDataLibrary srdl;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);

            this.uri = new Uri("http://www.rheagroup.com");
            this.serviceLocator = new Mock<IServiceLocator>();
            this.navigationService = new Mock<IDialogNavigationService>();
            this.session = new Mock<ISession>();

            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IDialogNavigationService>())
                .Returns(this.navigationService.Object);

            var siteRdl = new SiteReferenceDataLibrary();
            var siteDir = new SiteDirectory(Guid.NewGuid(), null, null);
            siteDir.SiteReferenceDataLibrary.Add(siteRdl);

            this.result = new SiteRdlSelectionDialogResult(true);
            this.navigationService.Setup(x => x.NavigateModal(It.IsAny<IDialogViewModel>())).Returns(this.result);

            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(siteDir);

            this.serviceLocator.Setup(x => x.GetInstance<IPermissionService>()).Returns(this.permissionService.Object);

            this.srdl = new SiteReferenceDataLibrary(Guid.NewGuid(), null, this.uri);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatOpenSiteRdlSelectorCommandOpensDialog()
        {
            var viewmodel = new SiteRdlSelectionRibbonViewModel();
            Assert.IsFalse(viewmodel.OpenSiteRdlSelectorCommand.CanExecute(null));

            CDPMessageBus.Current.SendMessage(new SessionEvent(this.session.Object, SessionStatus.Open));
            Assert.IsTrue(viewmodel.OpenSiteRdlSelectorCommand.CanExecute(null));
            viewmodel.OpenSiteRdlSelectorCommand.Execute(null);
            
            this.navigationService.Verify(x => x.NavigateModal(It.IsAny<IDialogViewModel>()));
        }

        [Test]
        public void VerifyThatSessionArePopulated()
        {
            var viewmodel = new SiteRdlSelectionRibbonViewModel();
            Assert.NotNull(this.result);
            CDPMessageBus.Current.SendMessage(new SessionEvent(this.session.Object, SessionStatus.Open));

            Assert.AreEqual(1, viewmodel.OpenSessions.Count);

            CDPMessageBus.Current.SendMessage(new SessionEvent(this.session.Object, SessionStatus.Closed));
            Assert.AreEqual(0, viewmodel.OpenSessions.Count);
        }

        [Test]
        public void VerifyThatCloseSiteRdlCommandWorks()
        {
            var viewmodel = new SiteRdlSelectionRibbonViewModel();
            CDPMessageBus.Current.SendMessage(new SessionEvent(this.session.Object, SessionStatus.Open));

            Assert.IsFalse(viewmodel.HasOpenSiteRdl);
            Assert.IsFalse(viewmodel.CloseSiteRdlCommand.CanExecute(null));

            this.session.Setup(x => x.OpenReferenceDataLibraries)
                .Returns(() => new List<ReferenceDataLibrary> {this.srdl});
            CDPMessageBus.Current.SendMessage(new SessionEvent(this.session.Object, SessionStatus.RdlOpened));

            Assert.IsTrue(viewmodel.HasOpenSiteRdl);
            Assert.IsTrue(viewmodel.CloseSiteRdlCommand.CanExecute(null));

            viewmodel.CloseSiteRdlCommand.Execute(null);
            this.navigationService.Verify(x => x.NavigateModal(It.IsAny<IDialogViewModel>()));
        }
    }
}