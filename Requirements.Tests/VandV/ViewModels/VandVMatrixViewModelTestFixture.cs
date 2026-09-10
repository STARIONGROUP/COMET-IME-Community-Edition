// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVMatrixViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace CDP4Requirements.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Concurrency;
    using System.Threading;

    using CDP4Requirements.ViewModels;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="VandVMatrixViewModel"/> class.
    /// </summary>
    /// <remarks>
    /// The matrix is opened from the register rather than from the ribbon, so no <c>RibbonMenuItem</c> owns it and
    /// nothing else closes it. These tests pin the two self-close paths: without them the panel stayed docked, bound
    /// to an iteration that is no longer in the cache, after a model close or a disconnect.
    /// </remarks>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class VandVMatrixViewModelTestFixture
    {
        private readonly Uri uri = new Uri("http://test.com");

        private CDPMessageBus messageBus;
        private Assembler assembler;
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IDialogNavigationService> dialogNavigationService;

        private Person person;
        private DomainOfExpertise domain;
        private Participant participant;
        private EngineeringModel model;
        private EngineeringModelSetup modelSetup;
        private Iteration iteration;
        private IterationSetup iterationSetup;
        private RequirementsSpecification specification;

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.messageBus = new CDPMessageBus();
            this.assembler = new Assembler(this.uri, this.messageBus);
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.dialogNavigationService = new Mock<IDialogNavigationService>();

            this.permissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);

            this.person = new Person(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "test" };
            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "domain", ShortName = "SYS" };
            this.participant = new Participant(Guid.NewGuid(), this.assembler.Cache, this.uri) { Person = this.person };
            this.participant.Domain.Add(this.domain);

            this.model = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.modelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "model" };
            this.iterationSetup = new IterationSetup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri) { IterationSetup = this.iterationSetup };
            this.specification = new RequirementsSpecification(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "SPEC", Name = "spec", Owner = this.domain };

            this.modelSetup.IterationSetup.Add(this.iterationSetup);
            this.modelSetup.Participant.Add(this.participant);
            this.model.EngineeringModelSetup = this.modelSetup;
            this.model.Iteration.Add(this.iteration);
            this.iteration.RequirementsSpecification.Add(this.specification);

            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString());
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>> { { this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, this.participant) } });
            this.session.Setup(x => x.QuerySelectedDomainOfExpertise(this.iteration)).Returns(this.domain);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatTheMatrixClosesItselfWhenTheModelIsClosed()
        {
            var matrix = this.CreateMatrix();

            this.messageBus.SendObjectChangeEvent(this.iteration, EventKind.Removed);

            this.panelNavigationService.Verify(x => x.CloseInDock(matrix), Times.Once);
        }

        [Test]
        public void VerifyThatTheMatrixIgnoresTheCloseOfAnotherModel()
        {
            var matrix = this.CreateMatrix();

            var otherIteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri) { IterationSetup = this.iterationSetup };

            this.messageBus.SendObjectChangeEvent(otherIteration, EventKind.Removed);

            this.panelNavigationService.Verify(x => x.CloseInDock(It.IsAny<IPanelViewModel>()), Times.Never);
        }

        [Test]
        public void VerifyThatTheMatrixClosesItselfWhenTheSessionIsClosed()
        {
            var matrix = this.CreateMatrix();

            this.messageBus.SendMessage(new SessionEvent(this.session.Object, SessionStatus.Closed));

            this.panelNavigationService.Verify(x => x.CloseInDock(matrix), Times.Once);
        }

        [Test]
        public void VerifyThatRebuildsAreDeferredWhileTheAssemblerIsMidBatch()
        {
            var matrix = this.CreateMatrix();

            var requirement = new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "REQ-1", Name = "req", Owner = this.domain };
            this.specification.Requirement.Add(requirement);

            this.messageBus.SendMessage(new SessionEvent(this.session.Object, SessionStatus.BeginUpdate));
            this.messageBus.SendObjectChangeEvent(requirement, EventKind.Added);

            Assert.That(matrix.MatrixTable.Rows.Count, Is.Zero);

            this.messageBus.SendMessage(new SessionEvent(this.session.Object, SessionStatus.EndUpdate));

            Assert.That(matrix.MatrixTable.Rows.Count, Is.EqualTo(1));
        }

        private VandVMatrixViewModel CreateMatrix()
        {
            return new VandVMatrixViewModel(this.iteration, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, this.dialogNavigationService.Object, null);
        }
    }
}
