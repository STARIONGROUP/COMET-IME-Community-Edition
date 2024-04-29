// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SwitchDomainSessionTextFixture.cs" company="Starion Group S.A.">
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

namespace CDP4ShellDialogs.Tests.ViewModels
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;
    using CDP4Dal.Permission;

    using CDP4ShellDialogs.ViewModels;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    internal class SwitchDomainSessionTextFixture
    {
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;

        private SiteDirectory siteDirectory;
        private EngineeringModelSetup modelSetup1;
        private EngineeringModelSetup modelSetup2;
        private IterationSetup iterationSetup11;
        private IterationSetup iterationSetup21;
        private IterationSetup iterationSetup22;
        private EngineeringModel model;
        private Iteration iteration11;
        private Iteration iteration21;
        private Person person;
        private Participant participant;
        private DomainOfExpertise domain;
        private DomainOfExpertise domain2;
        private string frozenOnDate;

        private Assembler assembler;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;

        private readonly Uri uri = new Uri("https://www.stariongroup.eu");
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            this.session = new Mock<ISession> { Name = "https://www.stariongroup.eu/" };
            this.messageBus = new CDPMessageBus();
            this.assembler = new Assembler(this.uri, this.messageBus);
            this.cache = this.assembler.Cache;

            this.permissionService = new Mock<IPermissionService>();

            this.siteDirectory = new SiteDirectory(Guid.NewGuid(), null, this.uri) { Name = "TestSiteDir" };
            this.modelSetup1 = new EngineeringModelSetup(Guid.NewGuid(), null, this.uri) { Name = "modelSetup1" };
            this.modelSetup2 = new EngineeringModelSetup(Guid.NewGuid(), null, this.uri) { Name = "modelSetup2" };

            this.iterationSetup11 = new IterationSetup(Guid.NewGuid(), null, this.uri);
            this.iterationSetup21 = new IterationSetup(Guid.NewGuid(), null, this.uri);
            this.frozenOnDate = "2022-01-12 12:12:30";
            this.iterationSetup21.FrozenOn = DateTime.Parse(this.frozenOnDate);

            this.iterationSetup22 = new IterationSetup(Guid.NewGuid(), null, this.uri);

            this.person = new Person(Guid.NewGuid(), null, this.uri) { GivenName = "testPerson" };
            this.domain = new DomainOfExpertise(Guid.NewGuid(), null, this.uri) { Name = "domaintest" };
            this.domain2 = new DomainOfExpertise(Guid.NewGuid(), null, this.uri) { Name = "domaintest2" };

            this.modelSetup1.ActiveDomain.Add(this.domain);
            this.modelSetup1.ActiveDomain.Add(this.domain2);

            this.modelSetup2.ActiveDomain.Add(this.domain);

            this.model = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri) { EngineeringModelSetup = this.modelSetup1 };
            this.iteration11 = new Iteration(Guid.NewGuid(), this.cache, this.uri) { IterationSetup = this.iterationSetup11 };
            this.iteration21 = new Iteration(Guid.NewGuid(), this.cache, this.uri) { IterationSetup = this.iterationSetup21 };
            this.model.Iteration.Add(this.iteration11);
            this.model.Iteration.Add(this.iteration21);
            this.participant = new Participant(Guid.NewGuid(), this.cache, this.uri) { Person = this.person, SelectedDomain = this.domain };
            this.participant.Domain.Add(this.domain);
            this.participant.Domain.Add(this.domain2);

            this.person.DefaultDomain = this.domain;
            this.participant.SelectedDomain = this.domain;
            this.modelSetup1.Participant.Add(this.participant);

            this.modelSetup1.IterationSetup.Add(this.iterationSetup11);
            this.modelSetup2.IterationSetup.Add(this.iterationSetup21);
            this.modelSetup2.IterationSetup.Add(this.iterationSetup22);
            this.siteDirectory.Model.Add(this.modelSetup1);
            this.siteDirectory.Model.Add(this.modelSetup2);
            this.siteDirectory.Person.Add(this.person);
            this.modelSetup2.Participant.Add(this.participant);

            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            var openIterations = new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>();
            openIterations.Add(this.iteration11, new Tuple<DomainOfExpertise, Participant>(this.domain, this.participant));
            openIterations.Add(this.iteration21, new Tuple<DomainOfExpertise, Participant>(this.domain, this.participant));
            this.session.Setup(x => x.OpenIterations).Returns(openIterations);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatPropertiesAreSetAndAdded()
        {
            var viewmodel = new SwitchDomainSessionRowViewModel(this.siteDirectory, this.session.Object);
            Assert.AreEqual(this.siteDirectory.Name + $" ({this.siteDirectory.IDalUri})", viewmodel.Name.Insert(13, this.session.Name));

            var models = viewmodel.EngineeringModelSetupRowViewModels;

            // Check for unique values
            Assert.AreEqual(1, models.Count);

            var modelSetup1 = models.First();
            Assert.AreEqual(this.modelSetup1.Name, modelSetup1.Name);

            Assert.AreEqual(1, modelSetup1.IterationSetupRowViewModels.Count);

            var modelSelectionIterationSetupRowViewModel = modelSetup1.IterationSetupRowViewModels.First();
            Assert.IsTrue(modelSelectionIterationSetupRowViewModel.Name.Contains(this.iterationSetup11.IterationNumber.ToString(CultureInfo.InvariantCulture)));
            Assert.AreEqual("Active", modelSelectionIterationSetupRowViewModel.FrozenOnDate);
            Assert.AreEqual(this.domain, modelSelectionIterationSetupRowViewModel.SelectedDomain);
            Assert.AreEqual(2, modelSelectionIterationSetupRowViewModel.DomainOfExpertises.Count);
        }
    }
}
