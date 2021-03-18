// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataSourceExportViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4IME.Tests.ViewModels
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
    using CDP4IME.ViewModels;
    using Microsoft.Practices.ServiceLocation;
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="DataSourceExportViewModel"/>
    /// </summary>
    [TestFixture]
    public class DataSourceExportViewModelTestFixture
    {
        /// <summary>
        /// The view model that is being tested
        /// </summary>
        private DataSourceExportViewModel viewModel;

        /// <summary>
        /// mocked data service
        /// </summary>
        private Mock<IDal> mockedDal;

        /// <summary>
        /// The <see cref="CancellationTokenSource"/>
        /// </summary>
        private CancellationTokenSource tokenSource;

        /// <summary>
        /// mocked metadata
        /// </summary>
        private Mock<IDalMetaData> mockedMetaData;

        /// <summary>
        /// The session.
        /// </summary>
        private Mock<ISession> session;
        private List<Thing> dalOutputs;
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IOpenSaveFileDialogService> fileDialogService;

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.tokenSource = new CancellationTokenSource();
            this.session = new Mock<ISession>();
            this.mockedDal = new Mock<IDal>();
            this.fileDialogService = new Mock<IOpenSaveFileDialogService>();
            var openTaskCompletionSource = new TaskCompletionSource<IEnumerable<Thing>>();
            openTaskCompletionSource.SetResult(this.dalOutputs);
            this.mockedDal.Setup(x => x.IsValidUri(It.IsAny<string>())).Returns(true);
            this.mockedDal.Setup(x => x.Open(It.IsAny<Credentials>(), this.tokenSource.Token)).Returns(openTaskCompletionSource.Task);

            this.mockedMetaData = new Mock<IDalMetaData>();
            this.mockedMetaData.Setup(x => x.Name).Returns("MockedDal");
            this.mockedMetaData.Setup(x => x.DalType).Returns(DalType.File);

            var dataAccessLayerKinds = new List<Lazy<IDal, IDalMetaData>>();
            dataAccessLayerKinds.Add(new Lazy<IDal, IDalMetaData>(() => this.mockedDal.Object, this.mockedMetaData.Object));

            this.serviceLocator = new Mock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<AvailableDals>())
                .Returns(new AvailableDals(dataAccessLayerKinds));

            this.viewModel = new DataSourceExportViewModel(new List<ISession> { this.session.Object }, this.fileDialogService.Object);
        }

        [Test]
        public void VerifyOkCommand()
        {
            this.viewModel.SelectedDal = this.viewModel.AvailableDals.First();
            this.viewModel.SelectedSession = this.viewModel.OpenSessions.First();

            this.viewModel.PasswordRetype = "pass";
            this.viewModel.Password = "pass";
            this.viewModel.Path = @"C:\test\somerandom\no\existant\path\doubletest.zip";

            Assert.AreEqual(this.viewModel.Password, this.viewModel.PasswordRetype);
            Assert.That(this.viewModel.ErrorMessage, Is.Null.Or.Empty);

            Assert.IsNotNull(this.viewModel.SelectedDal);
            Assert.IsNotNull(this.viewModel.SelectedSession);
            Assert.IsTrue(this.viewModel.OkCommand.CanExecute(null));

            this.viewModel.OkCommand.Execute(null);

            Assert.AreEqual("The output directory does not exist.", this.viewModel.ErrorMessage);
        }

        [Test]
        public void VerifyCancelCommand()
        {
            Assert.IsTrue(this.viewModel.CancelCommand.CanExecute(null));
            this.viewModel.CancelCommand.Execute(null);
            
            Assert.IsFalse(this.viewModel.DialogResult.Result.Value);
        }

        [Test]
        public void VerifyBrowseCommand()
        {
            Assert.IsTrue(this.viewModel.BrowseCommand.CanExecute(null));
        }
    }
}