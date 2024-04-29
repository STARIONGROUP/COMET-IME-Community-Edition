// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RibbonButtonOptionDependentViewModelTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2024 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
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

namespace CDP4Composition.Tests.Mvvm.MenuItems
{
    using System;
    using System.Linq;
    using System.Reactive.Concurrency;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.Events;

    using CommonServiceLocator;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="RibbonButtonOptionDependentViewModel"/> class.
    /// </summary>
    [TestFixture]
    public class RibbonButtonOptionDependentViewModelTestFixture
    {
        /// <summary>
        /// The view-model that is being tested.
        /// </summary>
        private TestClass viewModel;

        private EngineeringModel engineeringModel;
        private Uri uri;
        private Mock<ISession> session;

        private Mock<IServiceLocator> serviceLocator;
        private Mock<IPanelNavigationService> navigation;
        private Mock<IThingDialogNavigationService> dialogNavigation;

        private Assembler assembler;

        private CDPMessageBus messageBus;

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.serviceLocator = new Mock<IServiceLocator>();
            this.navigation = new Mock<IPanelNavigationService>();
            this.dialogNavigation = new Mock<IThingDialogNavigationService>();

            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IPanelNavigationService>()).Returns(this.navigation.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IThingDialogNavigationService>()).Returns(this.dialogNavigation.Object);

            this.uri = new Uri("https://www.stariongroup.eu");
            this.messageBus = new CDPMessageBus();
            this.assembler = new Assembler(this.uri, this.messageBus);
            this.session = new Mock<ISession>();
            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);

            this.viewModel = new TestClass(this.messageBus);

            var engineeringModelSeup = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.engineeringModel = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.engineeringModel.EngineeringModelSetup = engineeringModelSeup;
        }

        [Test]
        public void VerifyThatSessionChangeEventsAreProcessed()
        {
            Assert.IsEmpty(this.viewModel.Sessions);

            var session = new Mock<ISession>();
            var openSessionEvent = new SessionEvent(session.Object, SessionStatus.Open);
            this.messageBus.SendMessage(openSessionEvent);

            Assert.Contains(session.Object, this.viewModel.Sessions);

            var closeSessionEvent = new SessionEvent(session.Object, SessionStatus.Closed);
            this.messageBus.SendMessage(closeSessionEvent);

            Assert.IsEmpty(this.viewModel.Sessions);
        }

        [Test]
        public void VerifyThatAddIterationAndOptionWithoutSessionProduceExceptions()
        {
            var iterationSetup_1 = new IterationSetup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var iteration_1 = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri);
            iteration_1.IterationSetup = iterationSetup_1;
            this.engineeringModel.Iteration.Add(iteration_1);
            var option_1_1 = new Option(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "option1_1" };
            iteration_1.Option.Add(option_1_1);

            Assert.Throws<InvalidOperationException>(() => this.messageBus.SendObjectChangeEvent(iteration_1, EventKind.Added));
            Assert.Throws<InvalidOperationException>(() => this.messageBus.SendObjectChangeEvent(option_1_1, EventKind.Added));
        }

        [Test]
        public void VerifyThatAddAndRemoveIterationAndOptionMessagesAreProcessed()
        {
            Assert.IsEmpty(this.viewModel.OpenIterations);

            var iterationSetup_1 = new IterationSetup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var iteration_1 = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri);
            iteration_1.IterationSetup = iterationSetup_1;
            this.engineeringModel.Iteration.Add(iteration_1);
            var option_1_1 = new Option(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "option1_1" };
            iteration_1.Option.Add(option_1_1);

            this.viewModel.Sessions.Add(this.session.Object);

            this.messageBus.SendObjectChangeEvent(iteration_1, EventKind.Added);

            Assert.AreEqual(1, this.viewModel.OpenIterations.Count);

            var iterationSetup_2 = new IterationSetup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var iteration_2 = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri);
            iteration_2.IterationSetup = iterationSetup_2;
            this.engineeringModel.Iteration.Add(iteration_2);
            var option_2_1 = new Option(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "option2_1" };
            iteration_2.Option.Add(option_2_1);

            this.messageBus.SendObjectChangeEvent(iteration_2, EventKind.Added);
            Assert.AreEqual(2, this.viewModel.OpenIterations.Count);

            this.messageBus.SendObjectChangeEvent(iteration_1, EventKind.Removed);
            Assert.AreEqual(1, this.viewModel.OpenIterations.Count);

            var iterationMenuViewModel = this.viewModel.OpenIterations.Single();

            Assert.AreEqual(1, iterationMenuViewModel.SelectedOptions.Count);

            var option_2_2 = new Option(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "option2_2", RevisionNumber = 1 };
            iteration_2.Option.Add(option_2_2);
            this.messageBus.SendObjectChangeEvent(option_2_2, EventKind.Added);

            Assert.AreEqual(2, iterationMenuViewModel.SelectedOptions.Count);

            this.messageBus.SendObjectChangeEvent(option_2_1, EventKind.Removed);
            Assert.AreEqual(1, iterationMenuViewModel.SelectedOptions.Count);

            var new_name = "option2_2_NEW";
            option_2_2.Name = new_name;
            option_2_2.RevisionNumber = 2;
            this.messageBus.SendObjectChangeEvent(option_2_2, EventKind.Updated);
            Assert.AreEqual(iterationMenuViewModel.SelectedOptions.Single().MenuItemContent, new_name);

            Assert.AreEqual(1, this.viewModel.OpenIterations.Count);
            this.messageBus.SendObjectChangeEvent(option_2_2, EventKind.Removed);
            Assert.AreEqual(0, this.viewModel.OpenIterations.Count);
        }

        private class TestClass : RibbonButtonOptionDependentViewModel
        {
            public TestClass(ICDPMessageBus messageBus) : base(null, messageBus)
            {
            }
        }
    }
}
