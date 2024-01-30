// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequirementRibbonPartTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2024 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4Requirements.Tests.OfficeRibbon
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Concurrency;
    using System.Threading;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using CDP4OfficeInfrastructure;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="RequirementRibbonPart"/> class
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class RequirementRibbonPartTestFixture
    {
        /// <summary>
        /// The <see cref="RibbonPart"/> under test
        /// </summary>
        private RequirementRibbonPart ribbonPart;

        private int amountOfRibbonControls;
        private int order;
        private string ribbonxmlname;
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IDialogNavigationService> positiveDialogNavigationService;
        private Mock<IPermissionService> permissionService;
        private Mock<IDialogResult> negativeDialogResult;
        private Mock<IDialogNavigationService> negativeDialogNavigationService;
        private Mock<IOfficeApplicationWrapper> officeApplicationWrapper;
        private Mock<ISession> session;
        private Assembler assembler;
        private CDPMessageBus messageBus;

        private Uri uri = new Uri("http://test.com");
        private SiteDirectory sitedir;
        private EngineeringModelSetup modelSetup;
        private EngineeringModel model;
        private IterationSetup iterationSetup;
        private Iteration iteration;
        private Person person;
        private Participant participant;
        private DomainOfExpertise domain;

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.messageBus = new CDPMessageBus();
            this.assembler = new Assembler(this.uri, this.messageBus);
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);

            this.officeApplicationWrapper = new Mock<IOfficeApplicationWrapper>();

            // inputs
            this.sitedir = new SiteDirectory(Guid.NewGuid(), null, this.uri);
            this.modelSetup = new EngineeringModelSetup(Guid.NewGuid(), null, this.uri);
            this.iterationSetup = new IterationSetup(Guid.NewGuid(), null, this.uri);
            this.person = new Person(Guid.NewGuid(), null, this.uri);
            this.domain = new DomainOfExpertise(Guid.NewGuid(), null, this.uri);
            this.domain.ShortName = "SYS";

            this.participant = new Participant(Guid.NewGuid(), null, this.uri) { Person = this.person };

            this.sitedir.Model.Add(this.modelSetup);
            this.sitedir.Person.Add(this.person);
            this.sitedir.Domain.Add(this.domain);
            this.modelSetup.IterationSetup.Add(this.iterationSetup);
            this.modelSetup.Participant.Add(this.participant);

            this.model = new EngineeringModel(Guid.NewGuid(), null, this.uri) { EngineeringModelSetup = this.modelSetup };
            this.iteration = new Iteration(Guid.NewGuid(), null, this.uri) { IterationSetup = this.iterationSetup };

            this.model.Iteration.Add(this.iteration);

            var siteDirectory = new Lazy<Thing>(() => this.sitedir);
            this.assembler.Cache.GetOrAdd(new CacheKey(siteDirectory.Value.Iid, null), siteDirectory);
            this.session.Setup(x => x.DataSourceUri).Returns("test");
            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.sitedir);

            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.amountOfRibbonControls = 4;
            this.order = 1;

            // Setup negative dialog result - selection of iterationsetup
            this.SetupNegativeDialogResult();
            this.positiveDialogNavigationService = new Mock<IDialogNavigationService>();

            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
            {
                { this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, null) }
            });

            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);
        }

        /// <summary>
        /// setup the negative dialog result
        /// </summary>
        private void SetupNegativeDialogResult()
        {
            bool? nullnegresult = null;
            this.negativeDialogResult = new Mock<IDialogResult>();
            this.negativeDialogResult.Setup(x => x.Result).Returns(nullnegresult);
            this.negativeDialogNavigationService = new Mock<IDialogNavigationService>();

            this.negativeDialogNavigationService.Setup(x => x.NavigateModal(It.IsAny<IDialogViewModel>()))
                .Returns(this.negativeDialogResult.Object);
        }

        [Test]
        public void VerifyThatIfFluentRibbonIsNotActiveTheSessionEventHasNoEffect()
        {
            this.ribbonPart = new RequirementRibbonPart(this.order, this.panelNavigationService.Object, this.negativeDialogNavigationService.Object, null, null, this.officeApplicationWrapper.Object, this.messageBus);

            var fluentRibbonManager = new FluentRibbonManager();
            fluentRibbonManager.IsActive = false;
            fluentRibbonManager.RegisterRibbonPart(this.ribbonPart);

            var sessionEvent = new SessionEvent(this.session.Object, SessionStatus.Open);
            this.messageBus.SendMessage(sessionEvent);
            Assert.IsNull(this.ribbonPart.Session);
        }

        [Test]
        public void VerifyThatIfFluentRibbonIsNullTheSessionEventHasNoEffect()
        {
            this.ribbonPart = new RequirementRibbonPart(this.order, this.panelNavigationService.Object, this.negativeDialogNavigationService.Object, null, null, this.officeApplicationWrapper.Object, this.messageBus);

            var sessionEvent = new SessionEvent(this.session.Object, SessionStatus.Open);
            this.messageBus.SendMessage(sessionEvent);
            Assert.IsNull(this.ribbonPart.Session);
        }

        [Test]
        public void VerifyThatRibbonPartHandlesSessionOpenAndCloseEvent()
        {
            this.ribbonPart = new RequirementRibbonPart(this.order, this.panelNavigationService.Object, this.negativeDialogNavigationService.Object, null, null, this.officeApplicationWrapper.Object, this.messageBus);

            var fluentRibbonManager = new FluentRibbonManager();
            fluentRibbonManager.IsActive = true;
            fluentRibbonManager.RegisterRibbonPart(this.ribbonPart);

            var openSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Open);
            this.messageBus.SendMessage(openSessionEvent);

            Assert.AreEqual(this.session.Object, this.ribbonPart.Session);

            var closeSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Closed);
            this.messageBus.SendMessage(closeSessionEvent);

            Assert.IsNull(this.ribbonPart.Session);
        }

        [Test]
        public void VerifyThatButtonsAreEnabledAsExpected()
        {
            this.ribbonPart = new RequirementRibbonPart(this.order, this.panelNavigationService.Object, this.negativeDialogNavigationService.Object, null, null, this.officeApplicationWrapper.Object, this.messageBus);

            var fluentRibbonManager = new FluentRibbonManager();
            fluentRibbonManager.IsActive = true;
            fluentRibbonManager.RegisterRibbonPart(this.ribbonPart);

            Assert.IsFalse(this.ribbonPart.GetEnabled("ShowRequirements"));

            var openSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Open);
            this.messageBus.SendMessage(openSessionEvent);

            Assert.IsFalse(this.ribbonPart.GetEnabled("ShowRequirements"));

            this.messageBus.SendObjectChangeEvent(this.iteration, EventKind.Added);
            Assert.AreEqual(1, this.ribbonPart.Iterations.Count);

            Assert.IsTrue(this.ribbonPart.GetEnabled("ShowRequirements"));

            this.messageBus.SendObjectChangeEvent(this.iteration, EventKind.Removed);
            Assert.AreEqual(0, this.ribbonPart.Iterations.Count);

            Assert.IsFalse(this.ribbonPart.GetEnabled("ShowRequirements"));

            var closeSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Closed);
            this.messageBus.SendMessage(closeSessionEvent);

            Assert.IsFalse(this.ribbonPart.GetEnabled("ShowRequirements"));
        }

        [Test]
        public void VerifyThatOnActionProductTreeWorks()
        {
            this.ribbonPart = new RequirementRibbonPart(this.order, this.panelNavigationService.Object, this.positiveDialogNavigationService.Object, null, null, this.officeApplicationWrapper.Object, this.messageBus);

            var fluentRibbonManager = new FluentRibbonManager();
            fluentRibbonManager.IsActive = true;
            fluentRibbonManager.RegisterRibbonPart(this.ribbonPart);

            var openSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Open);
            this.messageBus.SendMessage(openSessionEvent);

            this.messageBus.SendObjectChangeEvent(this.iteration, EventKind.Added);
            Assert.AreEqual(1, this.ribbonPart.Iterations.Count);

            var content = this.ribbonPart.GetContent("ShowRequirements");
            this.ribbonPart.OnAction(string.Format("ShowRequirement_{0}", this.iteration.Iid), string.Format("{0}", this.iteration.Iid));

            this.panelNavigationService.Verify(x => x.OpenInAddIn(It.IsAny<IPanelViewModel>()));

            this.messageBus.SendObjectChangeEvent(this.iteration, EventKind.Removed);
            Assert.AreEqual(0, this.ribbonPart.Iterations.Count);
        }
    }
}
