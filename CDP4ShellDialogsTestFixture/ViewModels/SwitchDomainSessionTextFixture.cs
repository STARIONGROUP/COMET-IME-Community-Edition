// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ModelSelectionSessionTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
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
    class SwitchDomainSessionTextFixture
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

        private readonly Uri uri = new Uri("http://www.rheagroup.com");

        [SetUp]
        public void Setup()
        {
            this.session = new Mock<ISession>() { Name = "http://www.rheagroup.com/" };
            this.assembler = new Assembler(this.uri);
            this.cache = this.assembler.Cache;

            this.permissionService = new Mock<IPermissionService>();

            this.siteDirectory = new SiteDirectory(Guid.NewGuid(), null, uri) { Name = "TestSiteDir" };
            this.modelSetup1 = new EngineeringModelSetup(Guid.NewGuid(), null, uri) { Name = "modelSetup1" };
            this.modelSetup2 = new EngineeringModelSetup(Guid.NewGuid(), null, uri) { Name = "modelSetup2" };
            
            this.iterationSetup11 = new IterationSetup(Guid.NewGuid(), null, uri);
            this.iterationSetup21 = new IterationSetup(Guid.NewGuid(), null, uri);
            this.frozenOnDate = "2022-01-12 12:12:30";
            this.iterationSetup21.FrozenOn = DateTime.Parse(this.frozenOnDate);

            this.iterationSetup22 = new IterationSetup(Guid.NewGuid(), null, uri);

            this.person = new Person(Guid.NewGuid(), null, uri) { GivenName = "testPerson" };
            this.domain = new DomainOfExpertise(Guid.NewGuid(), null, uri) { Name = "domaintest" };
            this.domain2 = new DomainOfExpertise(Guid.NewGuid(), null, uri) { Name = "domaintest2" };

            this.modelSetup1.ActiveDomain.Add(this.domain);
            this.modelSetup1.ActiveDomain.Add(this.domain2);

            this.modelSetup2.ActiveDomain.Add(this.domain);

            this.model = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri) { EngineeringModelSetup = this.modelSetup1 };
            this.iteration11 = new Iteration(Guid.NewGuid(), this.cache, this.uri) { IterationSetup = this.iterationSetup11 };
            this.iteration21 = new Iteration(Guid.NewGuid(), this.cache, this.uri) { IterationSetup = this.iterationSetup21 };
            this.model.Iteration.Add(iteration11);
            this.model.Iteration.Add(iteration21);
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
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
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
