// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVItemCreatorTestFixture.cs" company="Starion Group S.A.">
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

namespace CDP4VandV.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using CDP4VandV.Services;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;
    using CDP4Dal.Operations;
    using CDP4Dal.Permission;

    using Moq;

    using NUnit.Framework;

    using DTO = CDP4Common.DTO;

    /// <summary>
    /// Suite of tests for the <see cref="VandVItemCreator"/> class.
    /// </summary>
    [TestFixture]
    public class VandVItemCreatorTestFixture
    {
        private readonly Uri uri = new Uri("http://test.com");

        private CDPMessageBus messageBus;
        private Assembler assembler;
        private Mock<ISession> session;
        private SiteDirectory siteDirectory;
        private SiteReferenceDataLibrary srdl;
        private ModelReferenceDataLibrary mrdl;
        private EngineeringModel model;
        private EngineeringModelSetup modelSetup;
        private Iteration iteration;
        private RequirementsSpecification specification;
        private Requirement requirement;
        private DomainOfExpertise domain;

        private OperationContainer capturedOperationContainer;
        private VandVItemCreator creator;

        [SetUp]
        public void SetUp()
        {
            this.messageBus = new CDPMessageBus();
            this.assembler = new Assembler(this.uri, this.messageBus);
            this.session = new Mock<ISession>();

            this.siteDirectory = new SiteDirectory(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.srdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "SITERDL" };
            this.mrdl = new ModelReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "MRDL", RequiredRdl = this.srdl };
            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "SYS", Name = "System" };

            this.modelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.modelSetup.RequiredRdl.Add(this.mrdl);
            this.siteDirectory.Model.Add(this.modelSetup);
            this.siteDirectory.SiteReferenceDataLibrary.Add(this.srdl);

            this.model = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.uri) { EngineeringModelSetup = this.modelSetup };
            this.iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.model.Iteration.Add(this.iteration);

            this.specification = new RequirementsSpecification(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "SPEC", Owner = this.domain };
            this.requirement = new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "REQ-1", Name = "A requirement", Owner = this.domain };
            this.specification.Requirement.Add(this.requirement);
            this.iteration.RequirementsSpecification.Add(this.specification);

            this.SeedVandVReferenceData();

            // the iteration must be in the cache so Clone() carries an Original, otherwise the transaction
            // treats the clone as a brand-new Iteration, which ThingTransaction rightly refuses to create.
            this.assembler.Cache.TryAdd(new CacheKey(this.iteration.Iid, null), new Lazy<Thing>(() => this.iteration));
            this.assembler.Cache.TryAdd(new CacheKey(this.specification.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.specification));

            this.capturedOperationContainer = null;
            this.session
                .Setup(x => x.Write(It.IsAny<OperationContainer>()))
                .Callback<OperationContainer>(container => this.capturedOperationContainer = container)
                .Returns(Task.CompletedTask);

            this.session.Setup(x => x.PermissionService).Returns(Mock.Of<IPermissionService>());

            this.creator = new VandVItemCreator();
        }

        [Test]
        public void VerifyThatCanCreateReflectsTheSeededReferenceData()
        {
            Assert.That(VandVItemCreator.CanCreate(this.iteration), Is.True);
        }

        [Test]
        public void VerifyThatCanCreateIsFalseWithoutTheReferenceData()
        {
            this.srdl.DefinedCategory.Clear();

            Assert.That(VandVItemCreator.CanCreate(this.iteration), Is.False);
        }

        [Test]
        public void VerifyThatTheSuggestedShortNameIsUniqueInTheIteration()
        {
            Assert.That(VandVItemCreator.SuggestShortName(this.requirement), Is.EqualTo("VNV_REQ_1_1"));

            this.specification.Requirement.Add(new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "VNV_REQ_1_1" });

            Assert.That(VandVItemCreator.SuggestShortName(this.requirement), Is.EqualTo("VNV_REQ_1_2"));
        }

        [Test]
        public async Task VerifyThatCreateWritesTheItemAttributesRelationshipAndSpecification()
        {
            var attributes = new Dictionary<string, string>
            {
                { "vnv_method", "Test" },
                { "vnv_stage", "FAT" },
                { "vnv_acceptance", "Endurance >= 24h" },
                { "vnv_status", "Planned" }
            };

            await this.creator.CreateAsync(this.session.Object, this.requirement, "VNV_REQ_1_1", "Verify endurance", this.domain, attributes);

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()), Times.Once);

            var written = this.capturedOperationContainer.Operations.Select(o => o.ModifiedThing).ToList();

            var vandVItem = written.OfType<DTO.Requirement>().Single(x => x.ShortName == "VNV_REQ_1_1");
            Assert.That(vandVItem.Category, Is.Not.Empty, "the V&V item must be categorized VnV Item");

            Assert.That(written.OfType<DTO.SimpleParameterValue>().Count(), Is.EqualTo(4), "one SimpleParameterValue per supplied attribute");

            var relationship = written.OfType<DTO.BinaryRelationship>().Single();
            Assert.That(relationship.Source, Is.EqualTo(vandVItem.Iid));
            Assert.That(relationship.Target, Is.EqualTo(this.requirement.Iid));
            Assert.That(relationship.Category, Is.Not.Empty, "the relationship must be categorized verifies");

            Assert.That(
                written.OfType<DTO.RequirementsSpecification>().Any(x => x.ShortName == VandVItemCreator.VandVSpecificationShortName),
                Is.True,
                "the V&V specification is created for the user on first use");
        }

        [Test]
        public async Task VerifyThatEmptyAttributesAreNotWritten()
        {
            var attributes = new Dictionary<string, string>
            {
                { "vnv_method", "Analysis" },
                { "vnv_stage", string.Empty },
                { "vnv_acceptance", null }
            };

            await this.creator.CreateAsync(this.session.Object, this.requirement, "VNV_REQ_1_1", "Verify by analysis", this.domain, attributes);

            var values = this.capturedOperationContainer.Operations.Select(o => o.ModifiedThing).OfType<DTO.SimpleParameterValue>().ToList();

            Assert.That(values, Has.Count.EqualTo(1), "blank fields are skipped");
        }

        /// <summary>
        /// Puts the V&amp;V categories and parameter types the creator needs into the site RDL.
        /// </summary>
        private void SeedVandVReferenceData()
        {
            var vnvItem = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "VnVItem", Name = "VnV Item" };
            vnvItem.PermissibleClass.Add(ClassKind.Requirement);

            var verifies = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "verifies", Name = "verifies" };
            verifies.PermissibleClass.Add(ClassKind.BinaryRelationship);

            this.srdl.DefinedCategory.Add(vnvItem);
            this.srdl.DefinedCategory.Add(verifies);

            foreach (var shortName in new[] { "vnv_method", "vnv_stage", "vnv_acceptance", "vnv_status" })
            {
                this.srdl.ParameterType.Add(new TextParameterType(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = shortName, Name = shortName, Symbol = shortName });
            }
        }
    }
}
