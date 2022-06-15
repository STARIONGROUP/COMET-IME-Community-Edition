// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataSourceSelectionViewModelTestFixture.cs" company="RHEA System S.A.">
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

namespace CDP4ShellDialogsTestFixture.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Input;

    using CDP4Common.DTO;

    using CDP4Composition.Navigation;
    using CDP4Composition.Utilities;
    
    using CDP4Dal;
    using CDP4Dal.Composition;
    using CDP4Dal.DAL;
    
    using CDP4ShellDialogs.ViewModels;
    
    using CommonServiceLocator;
    
    using Moq;
    
    using NUnit.Framework;
    
    using ReactiveUI;
    
    /// <summary>
    /// Suite of tests for the <see cref="DataSourceSelectionViewModel"/>
    /// </summary>
    [TestFixture]
    public class DataSourceSelectionViewModelTestFixture
    {
        private Mock<ISession> session;

        private Credentials credentials;

        /// <summary>
        /// mocked data service
        /// </summary>
        private Mock<IDal> mockedDal;

        /// <summary>
        /// mocked metadata
        /// </summary>
        private Mock<IDalMetaData> mockedMetaData;
        private Mock<IServiceLocator> serviceLocator;
        private CancellationTokenSource tokenSource;
        private List<Thing> dalOutputs;

        private Mock<IDialogNavigationService> navService;

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.session = new Mock<ISession>();
            this.tokenSource = new CancellationTokenSource();
            this.mockedDal = new Mock<IDal>();
            this.navService = new Mock<IDialogNavigationService>();
            this.mockedDal.Setup(x => x.IsValidUri(It.IsAny<string>())).Returns(true);
            var openTaskCompletionSource = new TaskCompletionSource<IEnumerable<Thing>>();
            openTaskCompletionSource.SetResult(this.dalOutputs);
            this.mockedDal.Setup(x => x.Open(It.IsAny<Credentials>(), this.tokenSource.Token)).Returns(openTaskCompletionSource.Task);

            this.mockedMetaData = new Mock<IDalMetaData>();
            this.mockedMetaData.Setup(x => x.Name).Returns("MockedDal");

            var mockedMetaData2 = new Mock<IDalMetaData>();
            mockedMetaData2.Setup(x => x.Name).Returns("MockedDal2");
            mockedMetaData2.Setup(x => x.DalType).Returns(DalType.File);

            var dataAccessLayerKinds = new List<Lazy<IDal, IDalMetaData>>();
            dataAccessLayerKinds.Add(
                new Lazy<IDal, IDalMetaData>(() => this.mockedDal.Object, this.mockedMetaData.Object));
            dataAccessLayerKinds.Add(new Lazy<IDal, IDalMetaData>(() => this.mockedDal.Object, mockedMetaData2.Object));

            this.serviceLocator = new Mock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<AvailableDals>())
                .Returns(new AvailableDals(dataAccessLayerKinds));

            this.credentials = new Credentials("John", "Doe", new Uri("http://www.rheagroup.com"));
            this.session.Setup(x => x.DataSourceUri).Returns("http://www.rheagroup.com");
            this.session.Setup(x => x.Credentials).Returns(this.credentials);
        }

        [Test]
        public async Task AssertThatOkCommandCanExecuteAndASessionObjectIsSet()
        {
            var viewmodel = new DataSourceSelectionViewModel(this.navService.Object);

            Assert.IsTrue(((ICommand)viewmodel.CancelCommand).CanExecute(null));

            Assert.That(viewmodel.ErrorMessage, Is.Null.Or.Empty);

            Assert.IsNotEmpty(viewmodel.AvailableDataSourceKinds);

            viewmodel.UserName = "John";
            viewmodel.Password = "Dow";
            viewmodel.Uri = "http://www.rheagroup.com";

            Assert.IsTrue(((ICommand)viewmodel.OkCommand).CanExecute(null));
            await viewmodel.OkCommand.Execute().Catch(Observable.Return(Unit.Default));
            Assert.NotNull(viewmodel.SelectedDataSourceKind);

            Assert.False(((ICommand)viewmodel.BrowseSourceCommand).CanExecute(null));
            viewmodel.SelectedDataSourceKind = viewmodel.AvailableDataSourceKinds.Single(x => x.DalType == DalType.File);
            Assert.IsTrue(((ICommand)viewmodel.BrowseSourceCommand).CanExecute(null));
        }

        [Test]
        public async Task AssertThatUriManagerDoesNotThrow()
        {
            var viewmodel = new DataSourceSelectionViewModel(this.navService.Object);
            Assert.IsTrue(((ICommand)viewmodel.OpenUriManagerCommand).CanExecute(null));
            Assert.DoesNotThrowAsync(async () => await viewmodel.OpenUriManagerCommand.Execute());
        }

        [Test]
        public async Task AssertThatProxyManagerDoesNotThrow()
        {
            var viewmodel = new DataSourceSelectionViewModel(this.navService.Object);
            Assert.IsTrue(((ICommand)viewmodel.OpenProxyConfigurationCommand).CanExecute(null));
            Assert.DoesNotThrowAsync(async () => await viewmodel.OpenProxyConfigurationCommand.Execute());
        }

        [Test]
        public void AssertViewModelWorksWithMultipleUris()
        {
            var viewmodel = new DataSourceSelectionViewModel(this.navService.Object);

            Assert.IsTrue(((ICommand)viewmodel.CancelCommand).CanExecute(null));
            Assert.That(viewmodel.ErrorMessage, Is.Null.Or.Empty);

            Assert.IsNotEmpty(viewmodel.AvailableDataSourceKinds);

            viewmodel.UserName = "John";
            viewmodel.Password = "Dow";

            var daltype0 = viewmodel.AvailableDataSourceKinds[0];
            var daltype1 = viewmodel.AvailableDataSourceKinds[1];
            viewmodel.SelectedDataSourceKind = null;

            UriConfig cl1 = new UriConfig() { Alias = "BadAlias", Uri = "KKK", DalType = daltype0.DalType.ToString() };
            UriRowViewModel row1 = new UriRowViewModel() { UriConfig = cl1 };

            UriConfig cl2 = new UriConfig() { Uri = "http://www.rheagroup.com", DalType = daltype0.DalType.ToString() };
            UriRowViewModel row2 = new UriRowViewModel() { UriConfig = cl2 };

            UriConfig cl3 = new UriConfig() { Uri = "http://www.rheagroup.com", DalType = daltype1.DalType.ToString() };
            UriRowViewModel row3 = new UriRowViewModel() { UriConfig = cl3 };

            viewmodel.AllDefinedUris.Clear();
            viewmodel.AllDefinedUris.Add(row1);
            viewmodel.AllDefinedUris.Add(row2);
            viewmodel.AllDefinedUris.Add(row3);

            viewmodel.SelectedDataSourceKind = daltype0;
            viewmodel.SelectedUri = viewmodel.AvailableUris.Last();
            Assert.IsTrue(((ICommand)viewmodel.OkCommand).CanExecute(null));
            Assert.IsTrue(viewmodel.AvailableUris.Count == 2);

            viewmodel.SelectedDataSourceKind = daltype1;
            Assert.IsTrue(viewmodel.AvailableUris.Count == 1);
        }

        [Test]
        public async Task VerifyThatCancelWorks()
        {
            var viewmodel = new DataSourceSelectionViewModel(this.navService.Object);

            await viewmodel.CancelCommand.Execute();

            Assert.IsFalse(viewmodel.HasError);
        }

        [Test]
        public async Task VerifyThatIfDuplicateSessionExistsErrorExists()
        {
            var sessions = new List<ISession>();
            sessions.Add(this.session.Object);
            var viewmodel = new DataSourceSelectionViewModel(this.navService.Object, sessions);

            viewmodel.UserName = "John";
            viewmodel.Password = "Dow";
            viewmodel.Uri = "http://www.rheagroup.com";

            await viewmodel.OkCommand.Execute();

            Assert.AreEqual("A session with the username John already exists", viewmodel.ErrorMessage);

            viewmodel.Uri = "http://www.rheagroup.com/";

            await viewmodel.OkCommand.Execute();

            Assert.AreEqual("A session with the username John already exists", viewmodel.ErrorMessage);
        }

        [Test]
        public void Verify_that_when_proxy_is_enabled_proxy_address_and_port_are_set()
        {
            var vm = new DataSourceSelectionViewModel(this.navService.Object);
            Assert.IsFalse(vm.IsProxyEnabled);
            Assert.AreEqual(string.Empty, vm.ProxyUri);
            Assert.AreEqual(string.Empty, vm.ProxyPort);

            vm.IsProxyEnabled = true;

            Assert.AreNotEqual(string.Empty, vm.ProxyUri);
            Assert.AreNotEqual(string.Empty, vm.ProxyPort);

            vm.IsProxyEnabled = false;

            Assert.AreEqual(string.Empty, vm.ProxyUri);
            Assert.AreEqual(string.Empty, vm.ProxyPort);
        }

        [Test]
        public void AssertThatShowPasswordButtonTextMatchesState()
        {
            var viewmodel = new DataSourceSelectionViewModel(this.navService.Object);
            // When password is hidden button should be Show
            viewmodel.IsPasswordVisible = false;
            Assert.AreEqual(viewmodel.ShowPasswordButtonText, "Show");
            // When password is visible it should show Hide
            viewmodel.IsPasswordVisible = true;
            Assert.AreEqual(viewmodel.ShowPasswordButtonText, "Hide");
        }
    }
}