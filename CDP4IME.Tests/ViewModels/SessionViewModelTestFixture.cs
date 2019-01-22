// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SessionViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4IME.Tests.ViewModels
{
    using System.Threading;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Navigation;
    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Events;
    using CDP4IME.ViewModels;
    using Microsoft.Practices.ServiceLocation;
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Threading.Tasks;

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
            this.dalOutputs.Add(sitedirectory);
            this.tokenSource = new CancellationTokenSource();
            CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(SiteDirectory)).Subscribe(x => this.OnEvent(x.ChangedThing));

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
            CDPMessageBus.Current.ClearSubscriptions();
            this.cache.Clear();
        }

        [Test]
        public void VerifyThatViewModelUriEquilsSessionObjectUri()
        {
            Assert.AreEqual(this.uri, this.sessionViewModel.DataSourceUri);
        }

        [Test]
        public void VerifyThatWhenSessionIsClosedTheIsClosedPropertyIsTrue()
        {
            Assert.IsFalse(this.sessionViewModel.IsClosed);

            this.sessionViewModel.Close.Execute(null);

            Assert.IsTrue(this.sessionViewModel.IsClosed);
        }

        [Test]
        public async Task VerifyThatRefreshUpdatesTheLastUpdateDatetimeAndSynchronizeData()
        {
            var updatedSiteDir = new CDP4Common.DTO.SiteDirectory(this.dalOutputs.Single().Iid, 30);

            var openTaskCompletionSource = new TaskCompletionSource<IEnumerable<CDP4Common.DTO.Thing>>();
            openTaskCompletionSource.SetResult(this.GetTestDtos());
            this.mockedDal.Setup(x => x.Open(It.IsAny<Credentials>(), this.tokenSource.Token)).Returns(openTaskCompletionSource.Task);

            var readTaskCompletionSource = new TaskCompletionSource<IEnumerable<CDP4Common.DTO.Thing>>();
            readTaskCompletionSource.SetResult(new List<CDP4Common.DTO.Thing> { updatedSiteDir });
            this.mockedDal.Setup(x => x.Read(It.IsAny<CDP4Common.DTO.Thing>(), It.IsAny<CancellationToken>(), It.IsAny<IQueryAttributes>())).Returns(readTaskCompletionSource.Task);

            var datetime = DateTime.Now;

            await this.sessionViewModel.Session.Open();
            this.cache.Clear();

            this.sessionViewModel.Refresh.Execute(null);

            Assert.IsFalse(datetime > this.sessionViewModel.LastUpdateDateTime);
            Assert.AreNotEqual(0, this.cache.Count);
        }

        [Test]
        public async Task VerifyThatReloadUpdatesTheLastUpdateDatetime()
        {
            var updatedSiteDir = new CDP4Common.DTO.SiteDirectory(this.dalOutputs.Single().Iid, 30);

            var openTaskCompletionSource = new TaskCompletionSource<IEnumerable<CDP4Common.DTO.Thing>>();
            openTaskCompletionSource.SetResult(this.GetTestDtos());
            this.mockedDal.Setup(x => x.Open(It.IsAny<Credentials>(), this.tokenSource.Token)).Returns(openTaskCompletionSource.Task);

            var readTaskCompletionSource = new TaskCompletionSource<IEnumerable<CDP4Common.DTO.Thing>>();
            readTaskCompletionSource.SetResult(new List<CDP4Common.DTO.Thing> { updatedSiteDir });
            this.mockedDal.Setup(x => x.Read(It.IsAny<CDP4Common.DTO.Thing>(), It.IsAny<CancellationToken>(), It.IsAny<IQueryAttributes>())).Returns(readTaskCompletionSource.Task);

            await this.sessionViewModel.Session.Open();
            this.cache.Clear();

            var datetime = DateTime.Now;
            this.sessionViewModel.Reload.Execute(null);

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

            this.sessionViewModel.Reload.Execute(null);
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

            this.sessionViewModel.Refresh.Execute(null);
            Assert.AreEqual("test failure", this.sessionViewModel.ErrorMsg);
            Assert.IsTrue(this.sessionViewModel.HasError);
        }

        [Test]
        public void VerifyThatHideAllCommandWork()
        {
            var servicelocator = new Mock<IServiceLocator>();
            var navigation = new Mock<IPanelNavigationService>();
            ServiceLocator.SetLocatorProvider(() => servicelocator.Object);
            servicelocator.Setup(x => x.GetInstance<IPanelNavigationService>()).Returns(navigation.Object);

            this.sessionViewModel.HideAll.Execute(null);
            navigation.Verify(x => x.Close(this.uri));
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