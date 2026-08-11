// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVCoverageWriterTestFixture.cs" company="Starion Group S.A.">
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

    using CDP4DalCommon.Protocol.Operations;

    using Moq;

    using NUnit.Framework;

    using DTO = CDP4Common.DTO;

    /// <summary>
    /// Suite of tests for the <see cref="VandVCoverageWriter"/>, the partial-coverage links of a V&amp;V item.
    /// </summary>
    [TestFixture]
    public class VandVCoverageWriterTestFixture
    {
        private readonly Uri uri = new Uri("http://test.com");

        private CDPMessageBus messageBus;
        private Assembler assembler;
        private Mock<ISession> session;
        private SiteReferenceDataLibrary srdl;
        private Iteration iteration;
        private RequirementsSpecification specification;
        private Requirement vandVItem;
        private DomainOfExpertise domain;
        private ElementDefinition elementDefinition;
        private Parameter parameter;
        private Option optionA;
        private Option optionB;
        private ActualFiniteState stateOn;
        private ActualFiniteStateList stateList;

        private OperationContainer captured;
        private VandVCoverageWriter writer;

        [SetUp]
        public void SetUp()
        {
            this.messageBus = new CDPMessageBus();
            this.assembler = new Assembler(this.uri, this.messageBus);
            this.session = new Mock<ISession>();

            this.srdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "SITERDL" };
            var mrdl = new ModelReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "MRDL", RequiredRdl = this.srdl };
            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "SYS" };

            var modelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            modelSetup.RequiredRdl.Add(mrdl);

            var model = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.uri) { EngineeringModelSetup = modelSetup };
            this.iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri);
            model.Iteration.Add(this.iteration);

            this.specification = new RequirementsSpecification(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "VNV", Owner = this.domain };
            this.vandVItem = new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "VNV_1", Owner = this.domain };
            this.specification.Requirement.Add(this.vandVItem);
            this.iteration.RequirementsSpecification.Add(this.specification);

            this.optionA = new Option(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "OPT_A", Name = "Option A" };
            this.optionB = new Option(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "OPT_B", Name = "Option B" };
            this.iteration.Option.Add(this.optionA);
            this.iteration.Option.Add(this.optionB);

            this.stateOn = new ActualFiniteState(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.stateList = new ActualFiniteStateList(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.stateList.ActualState.Add(this.stateOn);
            this.iteration.ActualFiniteStateList.Add(this.stateList);

            this.elementDefinition = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "ED1", Name = "Element 1", Owner = this.domain };
            this.parameter = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.uri) { Owner = this.domain };
            this.elementDefinition.Parameter.Add(this.parameter);
            this.iteration.Element.Add(this.elementDefinition);

            foreach (var shortName in new[] { "coversParameter", "coversOption", "coversState", "verifiedOn" })
            {
                var category = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = shortName, Name = shortName };
                category.PermissibleClass.Add(ClassKind.BinaryRelationship);
                this.srdl.DefinedCategory.Add(category);
            }

            this.assembler.Cache.TryAdd(new CacheKey(this.iteration.Iid, null), new Lazy<Thing>(() => this.iteration));

            this.captured = null;
            this.session
                .Setup(x => x.Write(It.IsAny<OperationContainer>()))
                .Callback<OperationContainer>(c => this.captured = c)
                .Returns(Task.CompletedTask);

            this.writer = new VandVCoverageWriter();
        }

        [Test]
        public async Task VerifyThatParameterAndElementLinksAreWritten()
        {
            await this.writer.SetCoverageAsync(this.session.Object, this.iteration, this.vandVItem, this.parameter, this.elementDefinition, null, null);

            var relationships = this.captured.Operations.Select(o => o.ModifiedThing).OfType<DTO.BinaryRelationship>().ToList();

            Assert.That(relationships, Has.Count.EqualTo(2));
            Assert.That(relationships.Select(x => x.Target), Does.Contain(this.parameter.Iid));
            Assert.That(relationships.Select(x => x.Target), Does.Contain(this.elementDefinition.Iid));
            Assert.That(relationships.All(x => x.Source == this.vandVItem.Iid), Is.True);
        }

        [Test]
        public async Task VerifyThatOptionAndStateLinksAreWritten()
        {
            await this.writer.SetCoverageAsync(
                this.session.Object,
                this.iteration,
                this.vandVItem,
                this.parameter,
                this.elementDefinition,
                new List<Option> { this.optionA },
                new List<ActualFiniteState> { this.stateOn });

            var relationships = this.captured.Operations.Select(o => o.ModifiedThing).OfType<DTO.BinaryRelationship>().ToList();

            Assert.That(relationships, Has.Count.EqualTo(4), "parameter + element + one option + one state");
            Assert.That(relationships.Select(x => x.Target), Does.Contain(this.optionA.Iid));
            Assert.That(relationships.Select(x => x.Target), Does.Contain(this.stateOn.Iid));
            Assert.That(relationships.Select(x => x.Target), Does.Not.Contain(this.optionB.Iid), "only the chosen option is linked");
        }

        [Test]
        public async Task VerifyThatNothingIsWrittenWhenNoCoverageIsSupplied()
        {
            await this.writer.SetCoverageAsync(this.session.Object, this.iteration, this.vandVItem, null, null, null, null);

            var relationships = this.captured.Operations.Select(o => o.ModifiedThing).OfType<DTO.BinaryRelationship>().ToList();

            Assert.That(relationships, Is.Empty);
        }

        [Test]
        public void VerifyThatExistingCoverageIsQueriedBackPerCategory()
        {
            this.AddCoverageRelationship(this.parameter, "coversParameter");
            this.AddCoverageRelationship(this.elementDefinition, "verifiedOn");
            this.AddCoverageRelationship(this.optionA, "coversOption");

            Assert.That(VandVCoverageWriter.QueryCoverageRelationships(this.iteration, this.vandVItem), Has.Count.EqualTo(3));
            Assert.That(
                VandVCoverageWriter.QueryCoveredThings<ParameterOrOverrideBase>(this.iteration, this.vandVItem, VandVCoverageWriter.CoversParameter).Single(),
                Is.EqualTo(this.parameter));
            Assert.That(
                VandVCoverageWriter.QueryCoveredThings<Option>(this.iteration, this.vandVItem, VandVCoverageWriter.CoversOption).Single(),
                Is.EqualTo(this.optionA));
        }

        [Test]
        public async Task VerifyThatSettingCoverageReplacesTheExistingLinks()
        {
            this.AddCoverageRelationship(this.optionB, "coversOption");

            await this.writer.SetCoverageAsync(
                this.session.Object,
                this.iteration,
                this.vandVItem,
                null,
                null,
                new List<Option> { this.optionA },
                null);

            var deleted = this.captured.Operations.Where(o => o.OperationKind == OperationKind.Delete).ToList();

            Assert.That(deleted, Is.Not.Empty, "the stale coverage link is removed rather than accumulating");

            // the update DTO of the iteration must not still reference the relationship deleted in the same
            // transaction, or the whole write is inconsistent and rejected server-side
            var deletedIids = deleted.Select(o => o.ModifiedThing.Iid).ToList();

            var iterationUpdate = this.captured.Operations
                .Where(o => o.OperationKind != OperationKind.Delete)
                .Select(o => o.ModifiedThing)
                .OfType<CDP4Common.DTO.Iteration>()
                .SingleOrDefault();

            if (iterationUpdate != null)
            {
                Assert.That(iterationUpdate.Relationship.Intersect(deletedIids), Is.Empty, "the iteration update references a deleted relationship");
            }
        }

        private void AddCoverageRelationship(Thing target, string categoryShortName)
        {
            var category = this.srdl.DefinedCategory.Single(x => x.ShortName == categoryShortName);

            var relationship = new BinaryRelationship(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Source = this.vandVItem,
                Target = target,
                Owner = this.domain
            };

            relationship.Category.Add(category);
            this.iteration.Relationship.Add(relationship);

            // a relationship that is not in the cache has no Original on Clone(), so the transaction would treat it
            // as newly added and the delete would be a silent no-op, mirror reality by caching it
            this.assembler.Cache.TryAdd(new CacheKey(relationship.Iid, this.iteration.Iid), new Lazy<Thing>(() => relationship));
        }
    }
}
