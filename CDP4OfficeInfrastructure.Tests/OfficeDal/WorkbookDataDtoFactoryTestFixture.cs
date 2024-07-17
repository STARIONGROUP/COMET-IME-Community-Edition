// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorkbookDataDtoFactoryTestFixture.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4OfficeInfrastructure.Tests.OfficeDal
{
    using System;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4OfficeInfrastructure.OfficeDal;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="WorkbookDataDtoFactory"/> class.
    /// </summary>
    [TestFixture]
    public class WorkbookDataDtoFactoryTestFixture
    {
        private SiteDirectory siteDirectory;

        private EngineeringModel engineeringModel;

        private Iteration iteration;

        /// <summary>
        /// this <see cref="Person"/> is referenced by a participant
        /// </summary>
        private Person person1;

        /// <summary>
        /// this <see cref="Person"/> is not referenced by a participant
        /// </summary>
        private Person person2;

        [SetUp]
        public void SetUp()
        {
            this.siteDirectory = new SiteDirectory(Guid.NewGuid(), null, null);

            var domain1 = new DomainOfExpertise(Guid.NewGuid(), null, null);
            this.siteDirectory.Domain.Add(domain1);
            var domain2 = new DomainOfExpertise(Guid.NewGuid(), null, null);
            this.siteDirectory.Domain.Add(domain2);

            var alias = new Alias(Guid.NewGuid(), null, null);
            domain1.Alias.Add(alias);

            var domainGroup1 = new DomainOfExpertiseGroup(Guid.NewGuid(), null, null);
            this.siteDirectory.DomainGroup.Add(domainGroup1);
            var domainGroup2 = new DomainOfExpertiseGroup(Guid.NewGuid(), null, null);
            this.siteDirectory.DomainGroup.Add(domainGroup2);

            var engineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), null, null);
            this.siteDirectory.Model.Add(engineeringModelSetup);
            var iterationSetup = new IterationSetup(Guid.NewGuid(), null, null);
            engineeringModelSetup.IterationSetup.Add(iterationSetup);

            var naturalLanguage1 = new NaturalLanguage(Guid.NewGuid(), null, null);
            this.siteDirectory.NaturalLanguage.Add(naturalLanguage1);
            var naturalLanguage2 = new NaturalLanguage(Guid.NewGuid(), null, null);
            this.siteDirectory.NaturalLanguage.Add(naturalLanguage2);

            var organization1 = new Organization(Guid.NewGuid(), null, null);
            this.siteDirectory.Organization.Add(organization1);
            var organization2 = new Organization(Guid.NewGuid(), null, null);
            this.siteDirectory.Organization.Add(organization2);

            var participantRole1 = new ParticipantRole(Guid.NewGuid(), null, null);
            this.siteDirectory.ParticipantRole.Add(participantRole1);
            var participantRole2 = new ParticipantRole(Guid.NewGuid(), null, null);
            this.siteDirectory.ParticipantRole.Add(participantRole2);

            this.person1 = new Person(Guid.NewGuid(), null, null);
            this.siteDirectory.Person.Add(this.person1);
            this.person2 = new Person(Guid.NewGuid(), null, null);
            this.siteDirectory.Person.Add(this.person2);

            var personRole1 = new PersonRole(Guid.NewGuid(), null, null);
            this.siteDirectory.PersonRole.Add(personRole1);
            var personRole2 = new PersonRole(Guid.NewGuid(), null, null);
            this.siteDirectory.PersonRole.Add(personRole2);

            var siteReferenceDataLibrary1 = new SiteReferenceDataLibrary(Guid.NewGuid(), null, null);
            this.siteDirectory.SiteReferenceDataLibrary.Add(siteReferenceDataLibrary1);
            var siteReferenceDataLibrary2 = new SiteReferenceDataLibrary(Guid.NewGuid(), null, null);
            this.siteDirectory.SiteReferenceDataLibrary.Add(siteReferenceDataLibrary2);

            this.engineeringModel = new EngineeringModel(Guid.NewGuid(), null, null);
            this.engineeringModel.EngineeringModelSetup = engineeringModelSetup;
            engineeringModelSetup.EngineeringModelIid = this.engineeringModel.Iid;

            this.iteration = new Iteration(Guid.NewGuid(), null, null);
            this.iteration.IterationSetup = iterationSetup;
            iterationSetup.IterationIid = this.iteration.Iid;

            this.engineeringModel.Iteration.Add(this.iteration);

            var modelReferenceDataLibrary = new ModelReferenceDataLibrary(Guid.NewGuid(), null, null);
            modelReferenceDataLibrary.RequiredRdl = siteReferenceDataLibrary1;

            engineeringModelSetup.RequiredRdl.Add(modelReferenceDataLibrary);

            var participant = new Participant(Guid.NewGuid(), null, null);
            participant.Person = this.person1;
            participant.Domain.Add(domain1);
            participant.Role = participantRole1;
            
            engineeringModelSetup.Participant.Add(participant);

            var elementDefinition1 = new ElementDefinition(Guid.NewGuid(), null, null);
            this.iteration.Element.Add(elementDefinition1);
            var elementDefinition2 = new ElementDefinition(Guid.NewGuid(), null, null);
            this.iteration.Element.Add(elementDefinition2);

            var elementUsage = new ElementUsage(Guid.NewGuid(), null, null);
            elementDefinition1.ContainedElement.Add(elementUsage);
            elementUsage.ElementDefinition = elementDefinition2;
        }

        [Test]
        public void VerifyThatDtosAreAdded()
        {
            var factory = new WorkbookDataDtoFactory(this.iteration);
            factory.Process();

            Assert.IsTrue(factory.SiteDirectoryThings.Any(x => x.Iid == this.person1.Iid));
            Assert.IsFalse(factory.SiteDirectoryThings.Any(x => x.Iid == this.person2.Iid));

            Assert.IsTrue(factory.SiteDirectoryThings.Any(x => x.ClassKind == ClassKind.Alias));

            Assert.IsFalse(factory.SiteDirectoryThings.Any(x => x.ClassKind == ClassKind.Definition));

            var elementDefinitions = factory.IterationThings.Where(x => x.ClassKind == ClassKind.ElementDefinition);
            Assert.AreEqual(2, elementDefinitions.Count());

            var elementUsage = factory.IterationThings.Where(x => x.ClassKind == ClassKind.ElementUsage);
            Assert.AreEqual(1, elementUsage.Count());
        }
    }
}

