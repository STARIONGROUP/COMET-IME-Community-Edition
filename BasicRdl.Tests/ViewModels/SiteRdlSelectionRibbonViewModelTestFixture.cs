﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SiteRdlSelectionRibbonViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace BasicRdl.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;

    using BasicRdl.ViewModels;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using CommonServiceLocator;

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
        private SiteRdlSelectionDialogResult result;
        private Mock<IPermissionService> permissionService;
        private SiteReferenceDataLibrary srdl;
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.messageBus = new CDPMessageBus();

            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);

            this.uri = new Uri("https://www.stariongroup.eu");
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
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);

            this.serviceLocator.Setup(x => x.GetInstance<IPermissionService>()).Returns(this.permissionService.Object);

            this.srdl = new SiteReferenceDataLibrary(Guid.NewGuid(), null, this.uri);
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public async Task VerifyThatOpenSiteRdlSelectorCommandOpensDialog()
        {
            var viewmodel = new SiteRdlSelectionRibbonViewModel(this.navigationService.Object, this.messageBus);
            Assert.IsFalse(((ICommand)viewmodel.OpenSiteRdlSelectorCommand).CanExecute(null));

            this.messageBus.SendMessage(new SessionEvent(this.session.Object, SessionStatus.Open));
            Assert.IsTrue(((ICommand)viewmodel.OpenSiteRdlSelectorCommand).CanExecute(null));
            await viewmodel.OpenSiteRdlSelectorCommand.Execute();

            this.navigationService.Verify(x => x.NavigateModal(It.IsAny<IDialogViewModel>()));
        }

        [Test]
        public void VerifyThatSessionArePopulated()
        {
            var viewmodel = new SiteRdlSelectionRibbonViewModel(this.navigationService.Object, this.messageBus);
            Assert.NotNull(this.result);
            this.messageBus.SendMessage(new SessionEvent(this.session.Object, SessionStatus.Open));

            Assert.AreEqual(1, viewmodel.OpenSessions.Count);

            this.messageBus.SendMessage(new SessionEvent(this.session.Object, SessionStatus.Closed));
            Assert.AreEqual(0, viewmodel.OpenSessions.Count);
        }

        [Test]
        public async Task VerifyThatCloseSiteRdlCommandWorks()
        {
            var viewmodel = new SiteRdlSelectionRibbonViewModel(this.navigationService.Object, this.messageBus);
            this.messageBus.SendMessage(new SessionEvent(this.session.Object, SessionStatus.Open));

            Assert.IsFalse(viewmodel.HasOpenSiteRdl);
            Assert.IsFalse(((ICommand)viewmodel.CloseSiteRdlCommand).CanExecute(null));

            this.session.Setup(x => x.OpenReferenceDataLibraries)
                .Returns(() => new List<ReferenceDataLibrary> { this.srdl });

            this.messageBus.SendMessage(new SessionEvent(this.session.Object, SessionStatus.RdlOpened));

            Assert.IsTrue(viewmodel.HasOpenSiteRdl);
            Assert.IsTrue(((ICommand)viewmodel.CloseSiteRdlCommand).CanExecute(null));

            await viewmodel.CloseSiteRdlCommand.Execute();
            this.navigationService.Verify(x => x.NavigateModal(It.IsAny<IDialogViewModel>()));
        }
    }
}
