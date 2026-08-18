// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVProcedureWriterTestFixture.cs" company="Starion Group S.A.">
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

namespace CDP4Requirements.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using CDP4Requirements.Services;

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
    /// Suite of tests for the <see cref="VandVProcedureWriter"/>, the ordered steps of a verification procedure.
    /// </summary>
    [TestFixture]
    public class VandVProcedureWriterTestFixture
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
        private Category stepCategory;
        private Category hasStepCategory;

        private OperationContainer captured;
        private VandVProcedureWriter writer;

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

            this.stepCategory = this.AddCategory("VnVStep", ClassKind.Requirement);
            this.hasStepCategory = this.AddCategory("hasStep", ClassKind.BinaryRelationship);

            foreach (var shortName in new[] { "vnv_step_no", "vnv_step_action", "vnv_step_expected", "vnv_step_actual", "vnv_step_result" })
            {
                this.srdl.ParameterType.Add(new TextParameterType(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = shortName, Name = shortName });
            }

            this.assembler.Cache.TryAdd(new CacheKey(this.iteration.Iid, null), new Lazy<Thing>(() => this.iteration));
            this.assembler.Cache.TryAdd(new CacheKey(this.specification.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.specification));

            this.captured = null;
            this.session
                .Setup(x => x.Write(It.IsAny<OperationContainer>()))
                .Callback<OperationContainer>(c => this.captured = c)
                .Returns(Task.CompletedTask);

            this.writer = new VandVProcedureWriter();
        }

        [Test]
        public void VerifyThatNullArgumentsAreRejected()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => this.writer.WriteAsync(null, this.iteration, this.vandVItem, new List<VandVProcedureStep>()));
            Assert.ThrowsAsync<ArgumentNullException>(() => this.writer.WriteAsync(this.session.Object, this.iteration, null, new List<VandVProcedureStep>()));
        }

        [Test]
        public async Task VerifyThatNothingIsWrittenForAnEmptyProcedure()
        {
            await this.writer.WriteAsync(this.session.Object, this.iteration, this.vandVItem, new List<VandVProcedureStep>());

            Assert.That(this.captured, Is.Null, "an item with no procedure must cost no round-trip");
        }

        [Test]
        public async Task VerifyThatStepsAreCreatedNumberedAndLinkedToTheItem()
        {
            var steps = new List<VandVProcedureStep>
            {
                new VandVProcedureStep { Action = "Power on the unit", ExpectedResult = "Green LED" },
                new VandVProcedureStep { Action = "Apply 5 V", ExpectedResult = "Draws 100 mA", ActualResult = "Drew 98 mA", Result = "Pass" }
            };

            await this.writer.WriteAsync(this.session.Object, this.iteration, this.vandVItem, steps);

            var created = this.captured.Operations.Select(o => o.ModifiedThing).OfType<DTO.Requirement>().ToList();
            var links = this.captured.Operations.Select(o => o.ModifiedThing).OfType<DTO.BinaryRelationship>().ToList();

            Assert.That(created, Has.Count.EqualTo(2));
            Assert.That(links, Has.Count.EqualTo(2), "each step is linked back to the item it belongs to");
            Assert.That(links.All(x => x.Source == this.vandVItem.Iid), Is.True);
            Assert.That(links.All(x => x.Category.Contains(this.hasStepCategory.Iid)), Is.True);
            Assert.That(created.All(x => x.Category.Contains(this.stepCategory.Iid)), Is.True);

            Assert.That(created.Select(x => x.ShortName), Is.EquivalentTo(new[] { "VNV_1_S01", "VNV_1_S02" }));
            Assert.That(created.Any(x => x.Name == "Step 1: Power on the unit"), Is.True);

            var values = this.captured.Operations.Select(o => o.ModifiedThing).OfType<DTO.SimpleParameterValue>().ToList();

            Assert.That(values.Any(x => x.Value.Contains("Power on the unit")), Is.True);
            Assert.That(values.Any(x => x.Value.Contains("Drew 98 mA")), Is.True);
            Assert.That(values.Count(x => x.Value.Contains("1") || x.Value.Contains("2")), Is.GreaterThan(0), "steps carry their number");
        }

        [Test]
        public async Task VerifyThatARemovedStepAndItsLinkAreDeleted()
        {
            var kept = this.AddStoredStep(1, "Power on");
            var removed = this.AddStoredStep(2, "Obsolete step");

            await this.writer.WriteAsync(
                this.session.Object,
                this.iteration,
                this.vandVItem,
                new List<VandVProcedureStep> { new VandVProcedureStep(kept) });

            var deleted = this.captured.Operations.Where(o => o.OperationKind == OperationKind.Delete).Select(o => o.ModifiedThing).ToList();
            var updated = this.captured.Operations.Where(o => o.OperationKind != OperationKind.Delete).Select(o => o.ModifiedThing).ToList();

            // a Requirement is deprecatable and the SDK refuses to hard delete one, so a dropped step is deprecated
            var deprecated = updated.OfType<DTO.Requirement>().SingleOrDefault(x => x.Iid == removed.Iid);

            Assert.That(deprecated, Is.Not.Null, "the dropped step is written back");
            Assert.That(deprecated.IsDeprecated, Is.True, "and is deprecated, which is how a requirement is retired");
            Assert.That(deleted.OfType<DTO.BinaryRelationship>(), Is.Not.Empty, "the link that held it is deleted outright");
            Assert.That(updated.OfType<DTO.Requirement>().Any(x => x.Iid == kept.Iid && x.IsDeprecated), Is.False, "the kept step stays live");

            var iterationUpdate = this.captured.Operations
                .Where(o => o.OperationKind != OperationKind.Delete)
                .Select(o => o.ModifiedThing)
                .OfType<DTO.Iteration>()
                .SingleOrDefault();

            if (iterationUpdate != null)
            {
                var deletedIids = deleted.Select(x => x.Iid).ToList();
                Assert.That(iterationUpdate.Relationship.Intersect(deletedIids), Is.Empty, "the iteration must not reference a deleted link");
            }
        }

        [Test]
        public void VerifyThatStepsComeBackInOrderAndUnnumberedOnesSortLast()
        {
            this.AddStoredStep(2, "second");
            this.AddStoredStep(1, "first");
            var unnumbered = this.AddStoredStep(null, "no number");

            var steps = VandVProcedureWriter.QuerySteps(this.iteration, this.vandVItem);

            Assert.That(steps.Select(x => VandVCoverageQuery.Attribute(x, "vnv_step_action")),
                Is.EqualTo(new[] { "first", "second", "no number" }));

            Assert.That(VandVProcedureWriter.QueryStepNumber(unnumbered), Is.EqualTo(int.MaxValue));
            Assert.That(VandVProcedureWriter.IsStep(steps.First()), Is.True);
            Assert.That(VandVProcedureWriter.IsStep(this.vandVItem), Is.False, "the item itself is not one of its own steps");
        }

        private Category AddCategory(string shortName, ClassKind permissibleClass)
        {
            var category = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = shortName, Name = shortName };
            category.PermissibleClass.Add(permissibleClass);
            this.srdl.DefinedCategory.Add(category);

            return category;
        }

        private Requirement AddStoredStep(int? number, string action)
        {
            var step = new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                ShortName = $"VNV_1_S{number?.ToString() ?? "X"}",
                Owner = this.domain
            };

            step.Category.Add(this.stepCategory);

            if (number.HasValue)
            {
                this.AddValue(step, "vnv_step_no", number.Value.ToString());
            }

            this.AddValue(step, "vnv_step_action", action);
            this.specification.Requirement.Add(step);

            var link = new BinaryRelationship(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Source = this.vandVItem,
                Target = step,
                Owner = this.domain
            };

            link.Category.Add(this.hasStepCategory);
            this.iteration.Relationship.Add(link);

            // a thing that is not in the cache has no Original on Clone(), so a delete would be a silent no-op;
            // the second element of a CacheKey is the iteration iid, not the direct container
            this.assembler.Cache.TryAdd(new CacheKey(step.Iid, this.iteration.Iid), new Lazy<Thing>(() => step));
            this.assembler.Cache.TryAdd(new CacheKey(link.Iid, this.iteration.Iid), new Lazy<Thing>(() => link));

            return step;
        }

        private void AddValue(Requirement step, string parameterTypeShortName, string value)
        {
            step.ParameterValue.Add(new SimpleParameterValue(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                ParameterType = new TextParameterType(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = parameterTypeShortName },
                Value = new ValueArray<string>(new[] { value })
            });
        }
    }
}
