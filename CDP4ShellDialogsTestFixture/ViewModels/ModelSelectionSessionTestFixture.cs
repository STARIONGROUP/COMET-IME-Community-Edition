// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ModelSelectionSessionTestFixture.cs" company="RHEA System S.A.">
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
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            this.messageBus = new CDPMessageBus();
            this.session = new Mock<ISession>() { Name = "http://www.rheagroup.com/" };
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
            var viewmodel = new ModelSelectionSessionRowViewModel(this.siteDirectory, this.session.Object);
            Assert.AreEqual(this.siteDirectory.Name + $" ({this.siteDirectory.IDalUri})", viewmodel.Name.Insert(13, this.session.Name));

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
