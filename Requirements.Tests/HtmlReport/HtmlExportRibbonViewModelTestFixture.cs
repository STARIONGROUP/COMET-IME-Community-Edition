// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HtmlExportRibbonViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace CDP4Requirements.Tests.HtmlReport
{
    using System;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;

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
        private readonly Uri uri = new Uri("https://www.stariongroup.eu");
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IDialogNavigationService> dialogNavigationService;
        private Mock<IOpenSaveFileDialogService> openSaveFileDialogService;
        private CDPMessageBus messageBus;

        [SetUp]
        public void SetUp()
        {
            this.messageBus = new CDPMessageBus();
            this.serviceLocator = new Mock<IServiceLocator>();
            this.dialogNavigationService = new Mock<IDialogNavigationService>();
            this.openSaveFileDialogService = new Mock<IOpenSaveFileDialogService>();

            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IDialogNavigationService>()).Returns(this.dialogNavigationService.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IOpenSaveFileDialogService>()).Returns(this.openSaveFileDialogService.Object);
            this.serviceLocator.Setup(x => x.GetInstance<ICDPMessageBus>()).Returns(this.messageBus);

            this.assembler = new Assembler(this.uri, this.messageBus);
            this.session = new Mock<ISession>();
            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);

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
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatSessionEventsAreCaught()
        {
            var vm = new HtmlExportRibbonViewModel();
            var sessionEvent = new SessionEvent(this.session.Object, SessionStatus.Open);
            this.messageBus.SendMessage(sessionEvent);

            Assert.AreEqual(1, vm.Sessions.Count);

            sessionEvent = new SessionEvent(this.session.Object, SessionStatus.Closed);
            this.messageBus.SendMessage(sessionEvent);

            Assert.AreEqual(0, vm.Sessions.Count);
        }

        [Test]
        public void VerifyThatIterationEventAreCaughtFailed()
        {
            var vm = new HtmlExportRibbonViewModel();
            Assert.Throws<InvalidOperationException>(() => this.messageBus.SendObjectChangeEvent(this.iteration, EventKind.Added));
        }

        [Test]
        public void VerifyThatITerationEventAreCaught()
        {
            var vm = new HtmlExportRibbonViewModel();
            var sessionEvent = new SessionEvent(this.session.Object, SessionStatus.Open);
            this.messageBus.SendMessage(sessionEvent);

            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString);
            this.messageBus.SendObjectChangeEvent(this.iteration, EventKind.Added);

            this.messageBus.SendObjectChangeEvent(this.iteration, EventKind.Removed);

            Assert.AreEqual(0, vm.Iterations.Count);
        }

        [Test]
        public void VerifyThatExportCommandIsEnabledAndDisabled()
        {
            var vm = new HtmlExportRibbonViewModel();
            var sessionEvent = new SessionEvent(this.session.Object, SessionStatus.Open);
            this.messageBus.SendMessage(sessionEvent);

            Assert.IsFalse(((ICommand)vm.ExportCommand).CanExecute(null));

            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString);
            this.messageBus.SendObjectChangeEvent(this.iteration, EventKind.Added);

            Assert.IsTrue(((ICommand)vm.ExportCommand).CanExecute(null));
        }

        [Test]
        public async Task VerifyThatExportCommandIsExecuted()
        {
            var vm = new HtmlExportRibbonViewModel();
            var sessionEvent = new SessionEvent(this.session.Object, SessionStatus.Open);
            this.messageBus.SendMessage(sessionEvent);

            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString);
            this.messageBus.SendObjectChangeEvent(this.iteration, EventKind.Added);

            await vm.ExportCommand.Execute();

            this.dialogNavigationService.Verify(x => x.NavigateModal(It.IsAny<HtmlExportRequirementsSpecificationSelectionDialogViewModel>()));
        }
    }
}
