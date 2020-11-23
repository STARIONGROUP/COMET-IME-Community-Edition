// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReferenceSourceRibbonViewModelTestFixture.cs" company="RHEA System S.A.">
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

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition;
    using CDP4Composition.Navigation;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using Microsoft.Practices.ServiceLocation;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="ReferenceSourceRibbonViewModel"/>.
    /// </summary>
    [TestFixture]
    public class ReferenceSourceRibbonViewModelTestFixture
    {
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IPanelNavigationService> navigationService;
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Uri uri;
        private Person person;

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.uri = new Uri("http://www.rheagroup.com");
            this.session = new Mock<ISession>();
            this.serviceLocator = new Mock<IServiceLocator>();
            this.navigationService = new Mock<IPanelNavigationService>();

            var siteDirectory = new SiteDirectory(Guid.NewGuid(), null, null);
            this.person = new Person(Guid.NewGuid(), null, this.uri) { GivenName = "John", Surname = "Doe" };

            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(siteDirectory);
            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);

            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);

            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IPanelNavigationService>()).Returns(this.navigationService.Object);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatSessionArePopulated()
        {
            var viewmodel = new ReferenceSourceRibbonViewModel();

            CDPMessageBus.Current.SendMessage(new SessionEvent(this.session.Object, SessionStatus.Open));
            Assert.AreEqual(1, viewmodel.OpenSessions.Count);

            CDPMessageBus.Current.SendMessage(new SessionEvent(this.session.Object, SessionStatus.Closed));
            Assert.AreEqual(0, viewmodel.OpenSessions.Count);
        }

        [Test]
        public void VerifyThatOpenCloseSingleBrowserWorks()
        {
            var vm = new ReferenceSourceRibbonViewModel();

            CDPMessageBus.Current.SendMessage(new SessionEvent(this.session.Object, SessionStatus.Open));
            vm.OpenSingleBrowserCommand.Execute(null);

            this.navigationService.Verify(x => x.Open(It.IsAny<IPanelViewModel>(), true));

            vm.OpenSingleBrowserCommand.Execute(null);
            this.navigationService.Verify(x => x.Close(It.IsAny<IPanelViewModel>(), true));
        }
    }
}
