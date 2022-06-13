// -------------------------------------------------------------------------------------------------
// <copyright file="HtmlExportRibbonViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2016 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Tests.HtmlReport
{
    using System;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Navigation;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Requirements.ViewModels;
    using CommonServiceLocator;
    using Moq;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="HtmlExportRibbonViewModel"/> class.
    /// </summary>
    [TestFixture]
    public class HtmlExportRibbonViewModelTestFixture
    {
        private Mock<ISession> session;
        private Assembler assembler;
        private Iteration iteration;
        private readonly Uri uri = new Uri("http://www.rheagroup.com");
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IDialogNavigationService> dialogNavigationService;
        private Mock<IOpenSaveFileDialogService> openSaveFileDialogService;

        [SetUp]
        public void SetUp()
        {
            this.serviceLocator = new Mock<IServiceLocator>();
            this.dialogNavigationService = new Mock<IDialogNavigationService>();
            this.openSaveFileDialogService = new Mock<IOpenSaveFileDialogService>();

            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IDialogNavigationService>()).Returns(this.dialogNavigationService.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IOpenSaveFileDialogService>()).Returns(this.openSaveFileDialogService.Object);

            this.assembler = new Assembler(this.uri);
            this.session = new Mock<ISession>();
            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri);

            var iterationSetup = new IterationSetup(Guid.NewGuid(), this.assembler.Cache, this.uri)
                                     {
                                         IterationNumber = 1
                                     };

            this.iteration.IterationSetup = iterationSetup;

            var engineeringModel = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var engineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.uri);

            engineeringModel.EngineeringModelSetup = engineeringModelSetup;
            engineeringModel.Iteration.Add(this.iteration);

        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatSessionEventsAreCaught()
        {
            var vm = new HtmlExportRibbonViewModel();
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
            var vm = new HtmlExportRibbonViewModel();
            Assert.Throws<InvalidOperationException>(() => CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Added));
        }

        [Test]
        public void VerifyThatITerationEventAreCaught()
        {
            var vm = new HtmlExportRibbonViewModel();
            var sessionEvent = new SessionEvent(this.session.Object, SessionStatus.Open);
            CDPMessageBus.Current.SendMessage(sessionEvent);
            
            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString);
            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Added);
            
            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Removed);

            Assert.AreEqual(0, vm.Iterations.Count);
        }

        [Test]
        public void VerifyThatExportCommandIsEnabledAndDisabled()
        {
            var vm = new HtmlExportRibbonViewModel();
            var sessionEvent = new SessionEvent(this.session.Object, SessionStatus.Open);
            CDPMessageBus.Current.SendMessage(sessionEvent);

            Assert.IsFalse(vm.ExportCommand.CanExecute(null));

            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString);
            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Added);

            Assert.IsTrue(vm.ExportCommand.CanExecute(null));
        }

        [Test]
        public void VerifyThatExportCommandIsExecuted()
        {
            var vm = new HtmlExportRibbonViewModel();
            var sessionEvent = new SessionEvent(this.session.Object, SessionStatus.Open);
            CDPMessageBus.Current.SendMessage(sessionEvent);
            
            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString);
            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Added);
            
            vm.ExportCommand.Execute(null);

            this.dialogNavigationService.Verify(x => x.NavigateModal(It.IsAny<HtmlExportRequirementsSpecificationSelectionDialogViewModel>()));
        }
    }
}
