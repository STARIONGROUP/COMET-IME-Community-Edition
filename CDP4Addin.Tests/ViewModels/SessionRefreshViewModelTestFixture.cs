// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SessionRefreshViewModelTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2026 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Rowan de Voogt
//
//    This file is part of CDP4-COMET IME Community Edition.
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

namespace CDP4Addin.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using CDP4AddinCE.ViewModels;

    using CDP4Dal;
    using CDP4Dal.DAL;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="SessionRefreshViewModel"/>
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class SessionRefreshViewModelTestFixture
    {
        /// <summary>
        /// mocked data service
        /// </summary>
        private Mock<IDal> mockedDal;

        /// <summary>
        /// The view-model under test
        /// </summary>
        private SessionRefreshViewModel viewModel;

        private List<CDP4Common.DTO.Thing> dalOutputs;

        private CDPMessageBus messageBus;

        private Session session;

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.messageBus = new CDPMessageBus();
            this.dalOutputs = new List<CDP4Common.DTO.Thing>();

            var sitedirectory = new CDP4Common.DTO.SiteDirectory(Guid.NewGuid(), 22);

            var person = new CDP4Common.DTO.Person(Guid.NewGuid(), 22)
            {
                ShortName = "John"
            };

            sitedirectory.Person.Add(person.Iid);
            this.dalOutputs.Add(sitedirectory);
            this.dalOutputs.Add(person);

            this.mockedDal = new Mock<IDal>();
            this.mockedDal.Setup(x => x.Close());

            var credentials = new Credentials("John", "Doe", new Uri("https://www.stariongroup.eu/"));

            this.session = new Session(this.mockedDal.Object, credentials, this.messageBus);

            this.viewModel = new SessionRefreshViewModel(this.session);

            var openTaskCompletionSource = new TaskCompletionSource<IEnumerable<CDP4Common.DTO.Thing>>();
            openTaskCompletionSource.SetResult(this.dalOutputs);
            this.mockedDal.Setup(x => x.Open(It.IsAny<Credentials>(), It.IsAny<CancellationToken>())).Returns(openTaskCompletionSource.Task);
        }

        [TearDown]
        public void TearDown()
        {
            this.viewModel.Dispose();
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatConstructorThrowsOnNullSession()
        {
            Assert.Throws<ArgumentNullException>(() => new SessionRefreshViewModel(null));
        }

        [Test]
        public void VerifyThatAutoRefreshCanBeSet()
        {
            Assert.AreEqual(SessionRefreshViewModel.DefaultRefreshInterval, this.viewModel.AutoRefreshInterval);

            this.viewModel.AutoRefreshInterval = 50;
            Assert.AreEqual(50, this.viewModel.AutoRefreshInterval);

            this.viewModel.IsAutoRefreshEnabled = true;
            Assert.IsTrue(this.viewModel.IsAutoRefreshEnabled);
            Assert.AreEqual(50, this.viewModel.AutoRefreshSecondsLeft);
        }

        [Test]
        public void VerifyThatSetIntervalFromTextParsesAndClamps()
        {
            this.viewModel.SetIntervalFromText("120");
            Assert.AreEqual(120, this.viewModel.AutoRefreshInterval);

            this.viewModel.SetIntervalFromText("9000");
            Assert.AreEqual(SessionRefreshViewModel.MaximumRefreshInterval, this.viewModel.AutoRefreshInterval);

            this.viewModel.SetIntervalFromText("1");
            Assert.AreEqual(SessionRefreshViewModel.MinimumRefreshInterval, this.viewModel.AutoRefreshInterval);

            // invalid input is ignored and the previous value kept
            this.viewModel.SetIntervalFromText("not-a-number");
            Assert.AreEqual(SessionRefreshViewModel.MinimumRefreshInterval, this.viewModel.AutoRefreshInterval);
        }

        [Test]
        public async Task VerifyThatRefreshUpdatesTheLastUpdateDateTime()
        {
            var updatedSiteDir = new CDP4Common.DTO.SiteDirectory(this.dalOutputs[0].Iid, 30);

            var readTaskCompletionSource = new TaskCompletionSource<IEnumerable<CDP4Common.DTO.Thing>>();
            readTaskCompletionSource.SetResult(new List<CDP4Common.DTO.Thing> { updatedSiteDir });
            this.mockedDal.Setup(x => x.Read(It.IsAny<CDP4Common.DTO.Thing>(), It.IsAny<CancellationToken>(), It.IsAny<IQueryAttributes>())).Returns(readTaskCompletionSource.Task);

            await this.session.Open();

            var datetime = DateTime.Now;
            await this.viewModel.Refresh.Execute();

            Assert.IsFalse(datetime > this.viewModel.LastUpdateDateTime);
        }

        [Test]
        public async Task VerifyThatReloadUpdatesTheLastUpdateDateTime()
        {
            var updatedSiteDir = new CDP4Common.DTO.SiteDirectory(this.dalOutputs[0].Iid, 30);

            var readTaskCompletionSource = new TaskCompletionSource<IEnumerable<CDP4Common.DTO.Thing>>();
            readTaskCompletionSource.SetResult(new List<CDP4Common.DTO.Thing> { updatedSiteDir });
            this.mockedDal.Setup(x => x.Read(It.IsAny<CDP4Common.DTO.Thing>(), It.IsAny<CancellationToken>(), It.IsAny<IQueryAttributes>())).Returns(readTaskCompletionSource.Task);

            await this.session.Open();

            var datetime = DateTime.Now;
            await this.viewModel.Reload.Execute();

            Assert.IsFalse(datetime > this.viewModel.LastUpdateDateTime);
        }

        [Test]
        public async Task VerifyThatErrorMessageIsReceivedUponRefreshFailure()
        {
            await this.session.Open();

            this.mockedDal.Setup(x => x.Read(It.IsAny<CDP4Common.DTO.Thing>(), It.IsAny<CancellationToken>(), It.IsAny<IQueryAttributes>()))
                .ThrowsAsync(new Exception("test failure"));

            await this.viewModel.Refresh.Execute();

            Assert.AreEqual("test failure", this.viewModel.ErrorMessage);
        }

        [Test]
        public async Task VerifyThatErrorMessageIsReceivedUponReloadFailure()
        {
            await this.session.Open();

            this.mockedDal.Setup(x => x.Read(It.IsAny<CDP4Common.DTO.Thing>(), It.IsAny<CancellationToken>(), It.IsAny<IQueryAttributes>()))
                .ThrowsAsync(new Exception("test failure"));

            await this.viewModel.Reload.Execute();

            Assert.AreEqual("test failure", this.viewModel.ErrorMessage);
        }
    }
}
