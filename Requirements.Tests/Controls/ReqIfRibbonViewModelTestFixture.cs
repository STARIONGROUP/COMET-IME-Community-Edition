// -------------------------------------------------------------------------------------------------
// <copyright file="ReqIfRibbonViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Tests.Controls
{
    using System;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Navigation;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Requirements.ViewModels;
    using Microsoft.Practices.ServiceLocation;
    using Moq;
    using NUnit.Framework;

    using ReqIFSharp;

    [TestFixture]
    internal class ReqIfRibbonViewModelTestFixture
    {
        private Mock<ISession> session;
        private Iteration iteration;
        private readonly Uri uri = new Uri("http://test.com");

        private Mock<IServiceLocator> serviceLocator;
        private Mock<IDialogNavigationService> dialogNavigationService;

        private Assembler assembler;
        private Mock<IOpenSaveFileDialogService> fileDialogService;

        [SetUp]
        public void Setup()
        {
            this.serviceLocator = new Mock<IServiceLocator>();
            this.fileDialogService = new Mock<IOpenSaveFileDialogService>();
            this.dialogNavigationService = new Mock<IDialogNavigationService>();
            this.dialogNavigationService.Setup(x => x.NavigateModal(It.IsAny<ReqIfExportDialogViewModel>()));
            this.dialogNavigationService.Setup(x => x.NavigateModal(It.IsAny<ReqIfImportDialogViewModel>()));

            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            
            this.serviceLocator.Setup(x => x.GetInstance<IDialogNavigationService>())
                .Returns(this.dialogNavigationService.Object);

            this.serviceLocator.Setup(x => x.GetInstance<IOpenSaveFileDialogService>())
                .Returns(this.fileDialogService.Object);

            this.assembler = new Assembler(this.uri);
            this.session = new Mock<ISession>();

            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString);
            this.session.Setup(x => x.Assembler).Returns(this.assembler);

            var iterationSetup = new IterationSetup() { IterationNumber = 42 };

            this.iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                IterationSetup = iterationSetup, Container = new EngineeringModel() { EngineeringModelSetup = new EngineeringModelSetup() { Name = "42" } }
            };
        }
        
        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatSessionEventsAreCaught()
        {
            var vm = new ReqIfRibbonViewModel();
            var sessionEvent = new SessionEvent(this.session.Object, SessionStatus.Open);
            CDPMessageBus.Current.SendMessage(sessionEvent);

            Assert.AreEqual(1, vm.Sessions.Count);

            sessionEvent = new SessionEvent(this.session.Object, SessionStatus.Closed);
            CDPMessageBus.Current.SendMessage(sessionEvent);

            Assert.AreEqual(0, vm.Sessions.Count);
        }

        [Test]
        public void VerifyThatIterationEventAreCaughtFailed()
        {
            var vm = new ReqIfRibbonViewModel();
            Assert.Throws<InvalidOperationException>(() => CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Added));
        }

        [Test]
        public void VerifyThatITerationEventAreCaught()
        {
            var vm = new ReqIfRibbonViewModel();
            var sessionEvent = new SessionEvent(this.session.Object, SessionStatus.Open);
            CDPMessageBus.Current.SendMessage(sessionEvent);

            Assert.IsFalse(vm.ExportCommand.CanExecute(null));

            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Added);

            Assert.IsTrue(vm.ExportCommand.CanExecute(null));
            Assert.AreEqual(1, vm.Iterations.Count);

            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Removed);

            Assert.AreEqual(0, vm.Iterations.Count);
        }

        [Test]
        public void VerifyExportCommand()
        {
            var vm = new ReqIfRibbonViewModel();
            vm.Iterations.Add(this.iteration);
            Assert.IsTrue(vm.ExportCommand.CanExecute(null));
            vm.ExportCommand.Execute(null);
            this.dialogNavigationService.Verify(x => x.NavigateModal(It.IsAny<ReqIfExportDialogViewModel>()), Times.Once);
        }
        
        [Test]
        public void VerifyImportCommand()
        {
            var vm = new ReqIfRibbonViewModel();
            Assert.IsFalse(vm.ImportCommand.CanExecute(null));
            vm.Iterations.Add(this.iteration);
            vm.Sessions.Add(this.session.Object);
            Assert.IsTrue(vm.ImportCommand.CanExecute(null));
            this.dialogNavigationService.Setup(x => x.NavigateModal(It.IsAny<ReqIfImportDialogViewModel>())).Returns(new ReqIfImportResult(new ReqIF(), this.iteration, new ImportMappingConfiguration(), null));
            Assert.DoesNotThrow(() => vm.ImportCommand.Execute(null));
            this.dialogNavigationService.Setup(x => x.NavigateModal(It.IsAny<ReqIfImportDialogViewModel>())).Returns(new ReqIfImportResult(new ReqIF(), this.iteration, new ImportMappingConfiguration(), false));
            Assert.DoesNotThrow(() => vm.ImportCommand.Execute(null));
            this.dialogNavigationService.Setup(x => x.NavigateModal(It.IsAny<ReqIfImportDialogViewModel>())).Returns(new ReqIfImportResult(new ReqIF(), this.iteration, new ImportMappingConfiguration(), true));
            Assert.DoesNotThrow(() => vm.ImportCommand.Execute(null));
            this.dialogNavigationService.Verify(x => x.NavigateModal(It.IsAny<ReqIfImportDialogViewModel>()), Times.Exactly(3));
        }
    }
}