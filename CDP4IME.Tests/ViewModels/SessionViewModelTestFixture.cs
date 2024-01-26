// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SessionViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMET.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Navigation;

    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Events;

    using COMET.ViewModels;

    using CommonServiceLocator;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="SessionViewModel"/>
    /// </summary>
    public class SessionViewModelTestFixture
    {
        private List<Thing> cache;

        /// <summary>
        /// mocked data service
        /// </summary>
        private Mock<IDal> mockedDal;

        /// <summary>
        /// The view-model under test
        /// </summary>
        private SessionViewModel sessionViewModel;

        /// <summary>
        /// the uri of the Credentials
        /// </summary>
        private string uri;

        private List<CDP4Common.DTO.Thing> dalOutputs;

        private CancellationTokenSource tokenSource;

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;



            this.cache = new List<Thing>();
            this.dalOutputs = new List<CDP4Common.DTO.Thing>();
            var sitedirectory = new CDP4Common.DTO.SiteDirectory(Guid.NewGuid(), 22);
            var person = new CDP4Common.DTO.Person(Guid.NewGuid(), 22)
            {
                ShortName = "John"
            };

            sitedirectory.Person.Add(person.Iid);
            this.dalOutputs.Add(sitedirectory);
            this.dalOutputs.Add(person);
            this.tokenSource = new CancellationTokenSource();
            this.messageBus.Listen<ObjectChangedEvent>(typeof(SiteDirectory)).Subscribe(x => this.OnEvent(x.ChangedThing));

            this.mockedDal = new Mock<IDal>();
            this.mockedDal.Setup(x => x.Close());

            this.uri = "http://www.rheagroup.com/";
            var credentials = new Credentials("John", "Doe", new Uri(this.uri));

            var session = new Session(this.mockedDal.Object, credentials);
            this.sessionViewModel = new SessionViewModel(session);
            var openTaskCompletionSource = new TaskCompletionSource<IEnumerable<CDP4Common.DTO.Thing>>();
            openTaskCompletionSource.SetResult(this.dalOutputs);
            this.mockedDal.Setup(x => x.Open(It.IsAny<Credentials>(), It.IsAny<CancellationToken>())).Returns(openTaskCompletionSource.Task);
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
            this.cache.Clear();
        }

        [Test]
        public void VerifyThatViewModelUriEquilsSessionObjectUri()
        {
            Assert.AreEqual(this.uri, this.sessionViewModel.DataSourceUri);
        }

        [Test]
        public async Task VerifyThatWhenSessionIsClosedTheIsClosedPropertyIsTrue()
        {
            Assert.IsFalse(this.sessionViewModel.IsClosed);

            await this.sessionViewModel.Close.Execute();

            Assert.IsTrue(this.sessionViewModel.IsClosed);
        }

        [Test]
        public async Task VerifyThatRefreshUpdatesTheLastUpdateDatetimeAndSynchronizeData()
        {
            var updatedSiteDir = new CDP4Common.DTO.SiteDirectory(this.dalOutputs.Single(x => x.ClassKind == ClassKind.SiteDirectory).Iid, 30);

            var openTaskCompletionSource = new TaskCompletionSource<IEnumerable<CDP4Common.DTO.Thing>>();
            openTaskCompletionSource.SetResult(this.GetTestDtos());
            this.mockedDal.Setup(x => x.Open(It.IsAny<Credentials>(), this.tokenSource.Token)).Returns(openTaskCompletionSource.Task);

            var readTaskCompletionSource = new TaskCompletionSource<IEnumerable<CDP4Common.DTO.Thing>>();
            readTaskCompletionSource.SetResult(new List<CDP4Common.DTO.Thing> { updatedSiteDir });
            this.mockedDal.Setup(x => x.Read(It.IsAny<CDP4Common.DTO.Thing>(), It.IsAny<CancellationToken>(), It.IsAny<IQueryAttributes>())).Returns(readTaskCompletionSource.Task);

            var datetime = DateTime.Now;

            await this.sessionViewModel.Session.Open();
            this.cache.Clear();

            await this.sessionViewModel.Refresh.Execute();

            Assert.IsFalse(datetime > this.sessionViewModel.LastUpdateDateTime);
            Assert.AreNotEqual(0, this.cache.Count);
        }

        [Test]
        public async Task VerifyThatReloadUpdatesTheLastUpdateDatetime()
        {
            var updatedSiteDir = new CDP4Common.DTO.SiteDirectory(this.dalOutputs.Single(x => x.ClassKind == ClassKind.SiteDirectory).Iid, 30);

            var openTaskCompletionSource = new TaskCompletionSource<IEnumerable<CDP4Common.DTO.Thing>>();
            openTaskCompletionSource.SetResult(this.GetTestDtos());
            this.mockedDal.Setup(x => x.Open(It.IsAny<Credentials>(), this.tokenSource.Token)).Returns(openTaskCompletionSource.Task);

            var readTaskCompletionSource = new TaskCompletionSource<IEnumerable<CDP4Common.DTO.Thing>>();
            readTaskCompletionSource.SetResult(new List<CDP4Common.DTO.Thing> { updatedSiteDir });
            this.mockedDal.Setup(x => x.Read(It.IsAny<CDP4Common.DTO.Thing>(), It.IsAny<CancellationToken>(), It.IsAny<IQueryAttributes>())).Returns(readTaskCompletionSource.Task);

            await this.sessionViewModel.Session.Open();
            this.cache.Clear();

            var datetime = DateTime.Now;
            await this.sessionViewModel.Reload.Execute();

            Assert.IsFalse(datetime > this.sessionViewModel.LastUpdateDateTime);
            Assert.AreNotEqual(0, this.cache.Count);
        }

        [Test]
        public void VerifyThatLastUpdateDateTimeHintIsUpdated()
        {
            var datetime = DateTime.Now;
            var expectedHint = "Last Updated at " + datetime.ToString(CultureInfo.InvariantCulture);
            this.sessionViewModel.LastUpdateDateTime = datetime;

            Assert.AreEqual(expectedHint, this.sessionViewModel.LastUpdateDateTimeHint);
        }

        [Test]
        public void VerifyThatAutoRefreshCanBeSet()
        {
            this.sessionViewModel.AutoRefreshInterval = 50;
            Assert.AreEqual(50, this.sessionViewModel.AutoRefreshInterval);

            this.sessionViewModel.IsAutoRefreshEnabled = true;
            Assert.IsTrue(this.sessionViewModel.IsAutoRefreshEnabled);
        }

        [Test]
        public async Task VerifyThatErrorMsgIsReceivedUponReloadFailure()
        {
            this.mockedDal.Setup(x => x.Open(It.IsAny<Credentials>(), this.tokenSource.Token)).Returns(Task.Run(() => this.GetTestDtos()));

            this.mockedDal.Setup(x => x.Read(It.IsAny<CDP4Common.DTO.Thing>(), It.IsAny<CancellationToken>(), It.IsAny<IQueryAttributes>()))
                .Throws(new Exception("test failure"));

            await this.sessionViewModel.Session.Open();
            this.cache.Clear();

            await this.sessionViewModel.Reload.Execute();
            Assert.IsTrue(this.sessionViewModel.ErrorMsg == "test failure");
            Assert.IsTrue(this.sessionViewModel.HasError);
        }

        [Test]
        public async Task VerifyThatErrorMsgIsReceivedUponRefreshFailure()
        {
            var openTaskCompletionSource = new TaskCompletionSource<IEnumerable<CDP4Common.DTO.Thing>>();
            openTaskCompletionSource.SetResult(this.GetTestDtos());
            this.mockedDal.Setup(x => x.Open(It.IsAny<Credentials>(), this.tokenSource.Token)).Returns(openTaskCompletionSource.Task);

            this.mockedDal.Setup(x => x.Read(It.IsAny<CDP4Common.DTO.Thing>(), It.IsAny<CancellationToken>(), It.IsAny<IQueryAttributes>()))
                .ThrowsAsync(new Exception("test failure"));

            await this.sessionViewModel.Session.Open();
            this.cache.Clear();

            await this.sessionViewModel.Refresh.Execute();
            Assert.AreEqual("test failure", this.sessionViewModel.ErrorMsg);
            Assert.IsTrue(this.sessionViewModel.HasError);
        }

        [Test]
        public async Task VerifyThatHideAllCommandWork()
        {
            var servicelocator = new Mock<IServiceLocator>();
            var navigation = new Mock<IPanelNavigationService>();
            ServiceLocator.SetLocatorProvider(() => servicelocator.Object);
            servicelocator.Setup(x => x.GetInstance<IPanelNavigationService>()).Returns(navigation.Object);

            await this.sessionViewModel.HideAll.Execute();
            navigation.Verify(x => x.CloseInDock(this.uri));
        }

        private void OnEvent(Thing thing)
        {
            this.cache.Add(thing);
        }

        private IEnumerable<CDP4Common.DTO.Thing> GetTestDtos()
        {
            return this.dalOutputs;
        }
    }
}