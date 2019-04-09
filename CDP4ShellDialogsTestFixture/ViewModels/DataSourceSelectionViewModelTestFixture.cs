// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataSourceSelectionViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2017 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ShellDialogsTestFixture.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Threading;
    using System.Threading.Tasks;

    using CDP4Common.DTO;
    using CDP4Composition.Navigation;
    using CDP4Dal;
    using CDP4Dal.Composition;
    using CDP4Dal.DAL;
    using CDP4ShellDialogs.ViewModels;
    using Microsoft.Practices.ServiceLocation;
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;
    using CDP4Composition.Utilities;

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
        public void AssertThatOkCommandCanExecuteAndASessionObjectIsSet()
        {
            var viewmodel = new DataSourceSelectionViewModel(this.navService.Object);

            Assert.IsTrue(viewmodel.CancelCommand.CanExecute(null));

            Assert.That(viewmodel.ErrorMessage, Is.Null.Or.Empty);

            Assert.IsNotEmpty(viewmodel.AvailableDataSourceKinds);

            viewmodel.UserName = "John";
            viewmodel.Password = "Dow";
            viewmodel.Uri = "http://www.rheagroup.com";

            Assert.IsTrue(viewmodel.OkCommand.CanExecute(null));
            viewmodel.OkCommand.Execute(null);
            Assert.NotNull(viewmodel.SelectedDataSourceKind);

            Assert.False(viewmodel.BrowseSourceCommand.CanExecute(null));
            viewmodel.SelectedDataSourceKind = viewmodel.AvailableDataSourceKinds.Single(x => x.DalType == DalType.File);
            Assert.IsTrue(viewmodel.BrowseSourceCommand.CanExecute(null));
        }

        [Test]
        public void AssertThatUriManagerDoesNotThrow()
        {
            var viewmodel = new DataSourceSelectionViewModel(this.navService.Object);
            Assert.IsTrue(viewmodel.OpenUriManagerCommand.CanExecute(null));
            Assert.DoesNotThrow(() => viewmodel.OpenUriManagerCommand.Execute(null));
        }

        [Test]
        public void AssertThatProxyManagerDoesNotThrow()
        {
            var viewmodel = new DataSourceSelectionViewModel(this.navService.Object);
            Assert.IsTrue(viewmodel.OpenProxyConfigurationCommand.CanExecute(null));
            Assert.DoesNotThrow(() => viewmodel.OpenProxyConfigurationCommand.Execute(null));
        }

        [Test]
        public void AssertViewModelWorksWithMultipleUris()
        {
            var viewmodel = new DataSourceSelectionViewModel(this.navService.Object);

            Assert.IsTrue(viewmodel.CancelCommand.CanExecute(null));
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
            Assert.IsTrue(viewmodel.OkCommand.CanExecute(null));
            Assert.IsTrue(viewmodel.AvailableUris.Count == 2);

            viewmodel.SelectedDataSourceKind = daltype1;
            Assert.IsTrue(viewmodel.AvailableUris.Count == 1);
        }

        [Test]
        public void VerifyThatCancelWorks()
        {
            var viewmodel = new DataSourceSelectionViewModel(this.navService.Object);

            viewmodel.CancelCommand.Execute(null);

            Assert.IsFalse(viewmodel.HasError);
        }

        [Test]
        public void VerifyThatIfDuplicateSessionExistsErrorExists()
        {
            var sessions = new List<ISession>();
            sessions.Add(this.session.Object);
            var viewmodel = new DataSourceSelectionViewModel(this.navService.Object, sessions);

            viewmodel.UserName = "John";
            viewmodel.Password = "Dow";
            viewmodel.Uri = "http://www.rheagroup.com";

            viewmodel.OkCommand.Execute(null);

            Assert.AreEqual("A session with the username John already exists", viewmodel.ErrorMessage);

            viewmodel.Uri = "http://www.rheagroup.com/";

            viewmodel.OkCommand.Execute(null);

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
    }
}