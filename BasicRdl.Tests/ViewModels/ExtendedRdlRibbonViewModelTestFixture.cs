// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BasicRDLRibbonViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski
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

namespace BasicRDL.Tests
{
    using System;
    using System.Reactive.Concurrency;

    using BasicRdl.ViewModels;

    using CDP4Common.SiteDirectoryData;

    using CDP4Composition;
    using CDP4Composition.Navigation;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using CommonServiceLocator;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="BasicRDLRibbonViewModel"/>
    /// </summary>
    [TestFixture]
    public class BasicRDLRibbonViewModelTestFixture
    {
        private Uri uri;
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IPanelNavigationService> navigationService;
        private Mock<IPermissionService> permissionService; 
        private Mock<ISession> session;
        private Person person;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.uri = new Uri("http://www.rheagroup.com");
            this.session = new Mock<ISession>();
            this.serviceLocator = new Mock<IServiceLocator>();
            this.navigationService = new Mock<IPanelNavigationService>();
            this.permissionService = new Mock<IPermissionService>();

            var siteDirectory = new SiteDirectory(Guid.NewGuid(), null, null);
            this.person = new Person(Guid.NewGuid(), null, this.uri) { GivenName = "John", Surname = "Doe" };

            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(siteDirectory);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);

            this.serviceLocator.Setup(x => x.GetInstance<IPanelNavigationService>())
                .Returns(this.navigationService.Object);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyConstantBrowserRibbon()
        {
            var viewmodel = new ConstantBrowserRibbonViewModel();

            CDPMessageBus.Current.SendMessage(new SessionEvent(this.session.Object, SessionStatus.Open));
            viewmodel.OpenSingleBrowserCommand.Execute(null);

            this.navigationService.Verify(x => x.OpenInDock(It.IsAny<IPanelViewModel>()));
        }

        [Test]
        public void VerifyGlossaryBrowserRibbon()
        {
            var viewmodel = new GlossaryBrowserRibbonViewModel();

            CDPMessageBus.Current.SendMessage(new SessionEvent(this.session.Object, SessionStatus.Open));
            viewmodel.OpenSingleBrowserCommand.Execute(null);

            this.navigationService.Verify(x => x.OpenInDock(It.IsAny<IPanelViewModel>()));
        }
    }
}