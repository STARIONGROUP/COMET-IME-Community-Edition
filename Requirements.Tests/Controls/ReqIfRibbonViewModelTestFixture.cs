// -------------------------------------------------------------------------------------------------
// <copyright file="ReqIfRibbonViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Tests.Controls
{
    using System;
    using CDP4Common.EngineeringModelData;
    using CDP4Composition.Navigation;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Requirements.ViewModels;
    using Microsoft.Practices.ServiceLocation;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    internal class ReqIfRibbonViewModelTestFixture
    {
        private Mock<ISession> session;
        private Iteration iteration;
        private readonly Uri uri = new Uri("http://test.com");

        private Mock<IServiceLocator> serviceLocator;
        private Mock<IDialogNavigationService> dialogNavigationService;

        private Assembler assembler;
            
        [SetUp]
        public void Setup()
        {
            this.serviceLocator = new Mock<IServiceLocator>();
            this.dialogNavigationService = new Mock<IDialogNavigationService>();

            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IDialogNavigationService>())
                .Returns(this.dialogNavigationService.Object);
            this.assembler = new Assembler(this.uri);
            this.session = new Mock<ISession>();

            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            
            this.iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri);
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
        [ExpectedException(typeof(InvalidOperationException))]
        public void VerifyThatIterationEventAreCaughtFailed()
        {
            var vm = new ReqIfRibbonViewModel();
            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Added);
        }

        [Test]
        public void VerifyThatITerationEventAreCaught()
        {
            var vm = new ReqIfRibbonViewModel();
            var sessionEvent = new SessionEvent(this.session.Object, SessionStatus.Open);
            CDPMessageBus.Current.SendMessage(sessionEvent);

            Assert.IsFalse(vm.ExportCommand.CanExecute(null));

            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString);
            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Added);

            Assert.IsTrue(vm.ExportCommand.CanExecute(null));
            Assert.AreEqual(1, vm.Iterations.Count);

            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Removed);

            Assert.AreEqual(0, vm.Iterations.Count);
        }
    }
}