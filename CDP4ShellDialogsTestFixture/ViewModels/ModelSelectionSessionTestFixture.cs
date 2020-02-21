// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ModelSelectionSessionTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ShellDialogs.Tests.RowViewModels
{
    using System;
    using System.Globalization;
    using System.Linq;
    using CDP4Common.SiteDirectoryData;
    using CDP4Dal;
    using CDP4Dal.Permission;
    using CDP4ShellDialogs.ViewModels;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class ModelSelectionSessionTestFixture
    {
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService; 

        private SiteDirectory siteDirectory;
        private EngineeringModelSetup model1;
        private EngineeringModelSetup model2;
        private IterationSetup iteration11;
        private IterationSetup iteration21;
        private IterationSetup iteration22;
        private Person person;
        private DomainOfExpertise domain;
        private string frozenOnDate;

        [SetUp]
        public void Setup()
        {
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();

            this.siteDirectory = new SiteDirectory(Guid.NewGuid(), null, new Uri("http://www.rheagroup.com")) { Name = "TestSiteDir" };
            this.model1 = new EngineeringModelSetup(Guid.NewGuid(), null, new Uri("http://www.rheagroup.com")) { Name = "model1" };
            this.model2 = new EngineeringModelSetup(Guid.NewGuid(), null, new Uri("http://www.rheagroup.com")) { Name = "model2" };
            this.iteration11 = new IterationSetup(Guid.NewGuid(), null, new Uri("http://www.rheagroup.com"));
            this.iteration21 = new IterationSetup(Guid.NewGuid(), null, new Uri("http://www.rheagroup.com"));
            this.frozenOnDate = "1992-01-12 12:12:30";
            this.iteration21.FrozenOn = DateTime.Parse(this.frozenOnDate);

            this.iteration22 = new IterationSetup(Guid.NewGuid(), null, new Uri("http://www.rheagroup.com"));
            
            this.person = new Person(Guid.NewGuid(), null, new Uri("http://www.rheagroup.com")) { GivenName = "testPerson" };
            this.domain = new DomainOfExpertise(Guid.NewGuid(), null, new Uri("http://www.rheagroup.com")) { Name = "domaintest" };

            this.person.DefaultDomain = this.domain;
            this.model1.Participant.Add(new Participant(Guid.NewGuid(), null, new Uri("http://www.rheagroup.com"))
            {
                Person = this.person,
                Domain = { this.domain }
            });

            this.model1.IterationSetup.Add(this.iteration11);
            this.model2.IterationSetup.Add(this.iteration21);
            this.model2.IterationSetup.Add(this.iteration22);
            this.siteDirectory.Model.Add(this.model1);
            this.siteDirectory.Model.Add(this.model2);
            this.siteDirectory.Person.Add(this.person);
            this.model2.Participant.Add(new Participant(Guid.NewGuid(), null, new Uri("http://www.rheagroup.com"))
            {
                Person = this.person,
                Domain = { this.domain }
            });

            this.session.Setup(x => x.ActivePerson).Returns(this.person);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatPropertiesAreSetAndAdded()
        {
            var viewmodel = new ModelSelectionSessionRowViewModel(this.siteDirectory, this.session.Object);
            Assert.AreEqual(this.siteDirectory.Name + string.Format(" ({0})", this.siteDirectory.IDalUri), viewmodel.Name);

            var models = viewmodel.EngineeringModelSetupRowViewModels;
            Assert.AreEqual(2, models.Count);

            var model1 = models.First();
            Assert.AreEqual(this.model1.Name, model1.Name);

            Assert.AreEqual(1, model1.IterationSetupRowViewModels.Count);

            var modelSelectionIterationSetupRowViewModel = model1.IterationSetupRowViewModels.First();
            Assert.IsTrue(modelSelectionIterationSetupRowViewModel.Name.Contains(this.iteration11.IterationNumber.ToString(CultureInfo.InvariantCulture)));
            Assert.AreEqual("Active", modelSelectionIterationSetupRowViewModel.FrozenOnDate);
            Assert.AreEqual(this.domain, modelSelectionIterationSetupRowViewModel.SelectedDomain);
            Assert.AreEqual(1, modelSelectionIterationSetupRowViewModel.DomainOfExpertises.Count);
        }

        [Test]
        public void VerifyThatTheFrozenDateIsAsExpected()
        {
            var viewmodel = new ModelSelectionSessionRowViewModel(this.siteDirectory, this.session.Object);
            var models = viewmodel.EngineeringModelSetupRowViewModels;
            var engineeringModelSetupRowViewModel = models.Single(x => x.Thing == this.model2);

            var frozenSelectionIterationSetupRowViewModel = engineeringModelSetupRowViewModel.IterationSetupRowViewModels.Single(x => x.Thing == this.iteration21);
            Assert.AreEqual(this.frozenOnDate, frozenSelectionIterationSetupRowViewModel.FrozenOnDate);

            var activeSelectionIterationSetupRowViewModel = engineeringModelSetupRowViewModel.IterationSetupRowViewModels.Single(x => x.Thing == this.iteration22);
            Assert.AreEqual("Active", activeSelectionIterationSetupRowViewModel.FrozenOnDate);
        }
    }
}