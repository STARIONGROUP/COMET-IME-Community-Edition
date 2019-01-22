// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ModelOpeningDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Concurrency;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4EngineeringModel.ViewModels;
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;

    [TestFixture]
    public class ModelOpeningDialogViewModelTestFixture
    {
        private Uri uri;
        private Mock<ISession> session;
        private SiteDirectory siteDirectory;
        private EngineeringModelSetup model1;
        private EngineeringModelSetup model2;
        private IterationSetup iteration11;
        private IterationSetup iteration21;
        private Person person;
        private Participant participant;
        private DomainOfExpertise domain;
        private Assembler assembler;

        private Credentials credentials;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.uri = new Uri("http://www.rheagroup.com");
            this.credentials = new Credentials("John", "Doe", this.uri);
            this.session = new Mock<ISession>();

            this.siteDirectory = new SiteDirectory(Guid.NewGuid(), null, this.uri) { Name = "TestSiteDir" };
            var model1RDL = new ModelReferenceDataLibrary(Guid.NewGuid(), null, null) { Name = "model1RDL" };
            var model2RDL = new ModelReferenceDataLibrary(Guid.NewGuid(), null, null) { Name = "model2RDL" };
            this.model1 = new EngineeringModelSetup(Guid.NewGuid(), null, this.uri) { Name = "model1" };
            this.model1.RequiredRdl.Add(model1RDL);
            this.model2 = new EngineeringModelSetup(Guid.NewGuid(), null, this.uri) { Name = "model2" };
            this.model2.RequiredRdl.Add(model2RDL);
            this.iteration11 = new IterationSetup(Guid.NewGuid(), null, this.uri);
            this.iteration21 = new IterationSetup(Guid.NewGuid(), null, this.uri);
            this.person = new Person(Guid.NewGuid(), null, this.uri) { GivenName = "testPerson" };
            this.participant = new Participant(Guid.NewGuid(), null, this.uri)
            {
                Person = this.person,
                Domain = { this.domain }
            };
            this.domain = new DomainOfExpertise(Guid.NewGuid(), null, this.uri) { Name = "domaintest" };

            this.person.DefaultDomain = this.domain;
            this.model1.Participant.Add(this.participant);
            this.model2.Participant.Add(this.participant);

            this.model1.IterationSetup.Add(this.iteration11);
            this.model2.IterationSetup.Add(this.iteration21);

            this.siteDirectory.Model.Add(this.model1);
            this.siteDirectory.Model.Add(this.model2);
            this.siteDirectory.Person.Add(this.person);

            this.assembler = new Assembler(this.uri);

            var lazysiteDirectory = new Lazy<Thing>(() => this.siteDirectory);
            this.assembler.Cache.GetOrAdd(new CacheKey(lazysiteDirectory.Value.Iid, null) , lazysiteDirectory);
            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDirectory);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>());
            this.session.Setup(x => x.Credentials).Returns(this.credentials);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatModelOpeningDialogReturnResult()
        {
            var sessions = new List<ISession> {this.session.Object};

            var viewmodel = new ModelOpeningDialogViewModel(sessions);
            viewmodel.SelectedIterations.Add(new ModelSelectionIterationSetupRowViewModel(this.iteration11, this.participant, this.session.Object));
            Assert.AreEqual("Iteration Selection", viewmodel.DialogTitle);
            viewmodel.SelectCommand.Execute(null);

            var res = viewmodel.DialogResult;
            Assert.IsNotNull(res);
            Assert.IsTrue(res.Result.Value);

            this.session.Verify(x => x.Read(It.IsAny<Iteration>(), It.IsAny<DomainOfExpertise>()));
        }

        [Test]
        public void VerifyThatErrorAreCaught()
        {
            var sessions = new List<ISession> { this.session.Object };
            this.session.Setup(x => x.Read(It.IsAny<Iteration>(), It.IsAny<DomainOfExpertise>())).Throws(new Exception("test"));


            var viewmodel = new ModelOpeningDialogViewModel(sessions);
            viewmodel.SelectedIterations.Add(new ModelSelectionIterationSetupRowViewModel(this.iteration11, this.participant, this.session.Object));

            viewmodel.SelectCommand.Execute(null);
            this.session.Verify(x => x.Read(It.IsAny<Iteration>(), It.IsAny<DomainOfExpertise>()));

            Assert.IsTrue(viewmodel.HasError);
            Assert.AreEqual("test", viewmodel.ErrorMessage);
        }

        [Test]
        public void VerifyThatSelectedItemCanOnlyContainIterationRow()
        {
            var sessions = new List<ISession> { this.session.Object };
            var viewmodel = new ModelOpeningDialogViewModel(sessions);

            viewmodel.SelectedIterations.Add(new ModelSelectionEngineeringModelSetupRowViewModel(this.model1, this.session.Object));

            Assert.AreEqual(0, viewmodel.SelectedIterations.Count);
        }

        [Test]
        public void VerifyThatExecuteCancelWork()
        {
            var sessions = new List<ISession> { this.session.Object };
            var viewmodel = new ModelOpeningDialogViewModel(sessions);
            viewmodel.SelectedIterations.Add(new ModelSelectionIterationSetupRowViewModel(this.iteration11, this.participant, this.session.Object));

            viewmodel.CancelCommand.Execute(null);

            var res = viewmodel.DialogResult;
            Assert.IsNotNull(res);
            Assert.IsFalse(res.Result.Value);
            this.session.Verify(x => x.Read(It.IsAny<Iteration>(), It.IsAny<DomainOfExpertise>()), Times.Never);
        }
    }
}
