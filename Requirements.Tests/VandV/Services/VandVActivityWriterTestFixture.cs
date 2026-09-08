// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVActivityWriterTestFixture.cs" company="Starion Group S.A.">
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

    using CDP4Requirements.Rdl;
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
    /// Suite of tests for the <see cref="VandVActivityWriter"/> class.
    /// </summary>
    [TestFixture]
    public class VandVActivityWriterTestFixture
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
        private RequirementsSpecification vandVSpecification;
        private Requirement requirement;
        private DomainOfExpertise domain;

        private OperationContainer capturedOperationContainer;
        private VandVActivityWriter writer;

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
            this.requirement = new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "REQ_1", Name = "A requirement", Owner = this.domain };
            this.specification.Requirement.Add(this.requirement);
            this.iteration.RequirementsSpecification.Add(this.specification);

            this.vandVSpecification = new RequirementsSpecification(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = VandVItemCreator.VandVSpecificationShortName, Name = "V&V Plan", Owner = this.domain };
            this.iteration.RequirementsSpecification.Add(this.vandVSpecification);

            this.SeedVandVReferenceData();

            this.RegisterInCache(this.iteration, null);
            this.RegisterInCache(this.specification, this.iteration.Iid);
            this.RegisterInCache(this.vandVSpecification, this.iteration.Iid);
            this.RegisterInCache(this.requirement, this.iteration.Iid);

            this.capturedOperationContainer = null;
            this.session
                .Setup(x => x.Write(It.IsAny<OperationContainer>()))
                .Callback<OperationContainer>(container => this.capturedOperationContainer = container)
                .Returns(Task.CompletedTask);

            this.writer = new VandVActivityWriter();
        }

        [Test]
        public async Task VerifyThatCreateWritesTheActivityItsAttributesAndItsReport()
        {
            var attributes = new Dictionary<string, string>
            {
                { VandVParameter.Method, "Analysis" },
                { VandVParameter.Status, "Planned" }
            };

            var report = this.AddReport("FAT_Report");

            await this.writer.CreateAsync(this.session.Object, this.iteration, "ACT_1", "Produce mass budget", this.domain, attributes, report);

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()), Times.Once);

            var written = this.capturedOperationContainer.Operations.Select(o => o.ModifiedThing).ToList();

            var activity = written.OfType<DTO.Requirement>().Single(x => x.ShortName == "ACT_1");
            Assert.Multiple(() =>
            {
                Assert.That(activity.Category, Is.Not.Empty, "the activity must be categorized VnV Activity");
                Assert.That(written.OfType<DTO.SimpleParameterValue>().Count(), Is.EqualTo(2), "one SimpleParameterValue per supplied attribute");
            });

            var writtenReport = written.OfType<DTO.RequirementsSpecification>().Single(x => x.ShortName == "FAT_Report");
            Assert.That(writtenReport.Requirement, Does.Contain(activity.Iid), "the activity is created inside the report it was given");
        }

        [Test]
        public async Task VerifyThatCreateReportWritesACategorizedSpecification()
        {
            await this.writer.CreateReportAsync(this.session.Object, this.iteration, "FAT_REPORT_1", "FAT Report 1", this.domain);

            var report = this.capturedOperationContainer.Operations
                .Select(o => o.ModifiedThing)
                .OfType<DTO.RequirementsSpecification>()
                .Single(x => x.ShortName == "FAT_REPORT_1");

            Assert.That(report.Category, Is.Not.Empty, "a report is a specification categorized VnV Report");
        }

        [Test]
        public async Task VerifyThatUpdatingTheReportMovesTheActivityIntoIt()
        {
            var activity = this.AddActivity("ACT_1");
            var report = this.AddReport("FAT_Report");

            await this.writer.UpdateAsync(
                this.session.Object,
                activity,
                "ACT_1",
                "Produce mass budget",
                this.domain,
                new Dictionary<string, string>(),
                report);

            var written = this.capturedOperationContainer.Operations.Select(o => o.ModifiedThing).ToList();

            var writtenReport = written.OfType<DTO.RequirementsSpecification>().Single(x => x.ShortName == "FAT_Report");
            Assert.That(writtenReport.Requirement, Does.Contain(activity.Iid), "the activity moves into the report, the stock container-change way");

            await this.writer.UpdateAsync(
                this.session.Object,
                activity,
                "ACT_1",
                "Produce mass budget",
                this.domain,
                new Dictionary<string, string>(),
                null);

            written = this.capturedOperationContainer.Operations.Select(o => o.ModifiedThing).ToList();

            Assert.That(
                written.OfType<DTO.RequirementsSpecification>().Any(x => x.ShortName == "FAT_Report"),
                Is.False,
                "an activity already outside every report stays put on a blank report");
        }

        [Test]
        public async Task VerifyThatSetPerformedByCreatesTheLink()
        {
            var item = this.AddVnVItem("VNV_REQ_1_1");
            var activity = this.AddActivity("ACT_1");

            await this.writer.SetPerformedByAsync(this.session.Object, this.iteration, item, activity);

            var relationship = this.capturedOperationContainer.Operations
                .Select(o => o.ModifiedThing)
                .OfType<DTO.BinaryRelationship>()
                .Single();

            Assert.Multiple(() =>
            {
                Assert.That(relationship.Source, Is.EqualTo(item.Iid));
                Assert.That(relationship.Target, Is.EqualTo(activity.Iid));
                Assert.That(relationship.Category, Is.Not.Empty, "the relationship must be categorized performedBy");
            });
        }

        [Test]
        public async Task VerifyThatAnUnchangedPerformedByWritesNothing()
        {
            var item = this.AddVnVItem("VNV_REQ_1_1");
            var activity = this.AddActivity("ACT_1");
            this.AddPerformedByRelationship(item, activity);

            await this.writer.SetPerformedByAsync(this.session.Object, this.iteration, item, activity);

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()), Times.Never, "an unchanged assignment writes nothing");
        }

        [Test]
        public async Task VerifyThatDetachingDeletesThePerformedByLink()
        {
            var item = this.AddVnVItem("VNV_REQ_1_1");
            var activity = this.AddActivity("ACT_1");
            var relationship = this.AddPerformedByRelationship(item, activity);

            await this.writer.SetPerformedByAsync(this.session.Object, this.iteration, item, null);

            var deletion = this.capturedOperationContainer.Operations
                .Single(o => o.OperationKind == OperationKind.Delete);

            Assert.That(deletion.ModifiedThing.Iid, Is.EqualTo(relationship.Iid));
        }

        [Test]
        public async Task VerifyThatCreateItemsWritesOneThinItemPerRequirementInOneTransaction()
        {
            var activity = this.AddActivity("ACT_1");

            var secondRequirement = new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "REQ_2", Name = "Another requirement", Owner = this.domain };
            this.specification.Requirement.Add(secondRequirement);

            var count = await this.writer.CreateItemsAsync(
                this.session.Object,
                activity,
                new List<Requirement> { this.requirement, secondRequirement },
                this.domain,
                VandVCategory.Verifies,
                new Dictionary<Guid, string>
                {
                    { this.requirement.Iid, "as per mass budget" },
                    { secondRequirement.Iid, "as per the speed trial" }
                });

            Assert.That(count, Is.EqualTo(2));
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()), Times.Once, "the whole batch is one transaction");

            var written = this.capturedOperationContainer.Operations.Select(o => o.ModifiedThing).ToList();

            var items = written.OfType<DTO.Requirement>().ToList();
            Assert.Multiple(() =>
            {
                Assert.That(items.Select(x => x.ShortName), Is.EquivalentTo(new[] { "VNV_REQ_1_1", "VNV_REQ_2_1" }));
                Assert.That(written.OfType<DTO.BinaryRelationship>().Count(), Is.EqualTo(4), "a verifies and a performedBy link per item");
                Assert.That(written.OfType<DTO.SimpleParameterValue>().Count(), Is.EqualTo(4), "status and acceptance criteria per item");
            });

            var criteria = written.OfType<DTO.SimpleParameterValue>().SelectMany(x => x.Value).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(criteria, Does.Contain("as per mass budget"));
                Assert.That(criteria, Does.Contain("as per the speed trial"), "each requirement keeps its own acceptance criteria");
            });
        }

        [Test]
        public async Task VerifyThatCreateItemsWritesNoAcceptanceCriteriaForARequirementGivenNone()
        {
            var activity = this.AddActivity("ACT_1");

            var count = await this.writer.CreateItemsAsync(
                this.session.Object,
                activity,
                new List<Requirement> { this.requirement },
                this.domain,
                VandVCategory.Verifies,
                new Dictionary<Guid, string>());

            Assert.That(count, Is.EqualTo(1));

            var values = this.capturedOperationContainer.Operations
                .Select(o => o.ModifiedThing)
                .OfType<DTO.SimpleParameterValue>()
                .ToList();

            Assert.That(values.Count, Is.EqualTo(1), "only the status is written; the dialog is what stops an item being created without criteria");
        }

        [Test]
        public async Task VerifyThatApplyToItemsWritesOnlyTheSuppliedValues()
        {
            var item = this.AddVnVItem("VNV_REQ_1_1");
            this.SetAttribute(item, VandVParameter.Status, "Planned");

            var attributes = new Dictionary<string, string>
            {
                { VandVParameter.Status, "Passed" },
                { VandVParameter.Result, "mass budget issue 3" },
                { VandVParameter.EvidenceReference, string.Empty }
            };

            var count = await this.writer.ApplyToItemsAsync(this.session.Object, this.iteration, new List<Requirement> { item }, attributes);

            Assert.That(count, Is.EqualTo(1));

            var values = this.capturedOperationContainer.Operations
                .Select(o => o.ModifiedThing)
                .OfType<DTO.SimpleParameterValue>()
                .ToList();

            Assert.That(values, Has.Count.EqualTo(2), "the blank evidence is skipped, nothing is cleared by a bulk apply");
        }

        [Test]
        public async Task VerifyThatLinkingExistingItemsCreatesAndRetargetsInOneTransaction()
        {
            var standalone = this.AddVnVItem("VNV_REQ_1_1");

            var elsewhere = this.AddVnVItem("VNV_REQ_1_2");
            var otherActivity = this.AddActivity("ACT_9");
            var existingLink = this.AddPerformedByRelationship(elsewhere, otherActivity);

            var activity = this.AddActivity("ACT_1");

            var count = await this.writer.LinkItemsAsync(this.session.Object, this.iteration, new List<Requirement> { standalone, elsewhere }, activity);

            Assert.That(count, Is.EqualTo(2));
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()), Times.Once, "the whole batch is one transaction");

            var relationships = this.capturedOperationContainer.Operations
                .Select(o => o.ModifiedThing)
                .OfType<DTO.BinaryRelationship>()
                .ToList();

            Assert.Multiple(() =>
            {
                Assert.That(relationships, Has.Count.EqualTo(2));
                Assert.That(relationships.All(x => x.Target == activity.Iid), Is.True, "both items end up performed by the picked activity");
                Assert.That(relationships.Any(x => x.Iid == existingLink.Iid), Is.True, "an item performed elsewhere is retargeted, not given a second link");
            });
        }

        [Test]
        public async Task VerifyThatLinkingKeepsTheItemsOwnPlanningUnlessAskedToClearIt()
        {
            var activity = this.AddActivity("ACT_1");

            var item = this.AddVnVItem("VNV_REQ_1_1");
            this.SetAttribute(item, VandVParameter.Method, "Test");
            this.SetAttribute(item, VandVParameter.Stage, "FAT");

            await this.writer.LinkItemsAsync(this.session.Object, this.iteration, new List<Requirement> { item }, activity);

            Assert.That(
                this.capturedOperationContainer.Operations.Select(o => o.ModifiedThing).OfType<DTO.SimpleParameterValue>().Any(),
                Is.False,
                "an item's own method and stage survive a plain link, so one requirement can be verified a little differently by the same task");

            var second = this.AddVnVItem("VNV_REQ_1_2");
            this.SetAttribute(second, VandVParameter.Method, "Test");
            this.SetAttribute(second, VandVParameter.Stage, "FAT");

            await this.writer.LinkItemsAsync(this.session.Object, this.iteration, new List<Requirement> { second }, activity, true);

            var deletions = this.capturedOperationContainer.Operations
                .Where(o => o.OperationKind == OperationKind.Delete)
                .Select(o => o.ModifiedThing)
                .OfType<DTO.SimpleParameterValue>()
                .ToList();

            Assert.That(deletions, Has.Count.EqualTo(2), "asking for it removes the item's own method and stage so they follow the activity");
        }

        [Test]
        public async Task VerifyThatLinkingAnItemAlreadyPerformedByTheActivityWritesNothing()
        {
            var activity = this.AddActivity("ACT_1");
            var item = this.AddVnVItem("VNV_REQ_1_1");
            this.AddPerformedByRelationship(item, activity);

            var count = await this.writer.LinkItemsAsync(this.session.Object, this.iteration, new List<Requirement> { item }, activity);

            Assert.That(count, Is.EqualTo(0));
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()), Times.Never);
        }

        /// <summary>
        /// Puts the V&amp;V categories and parameter types the writer needs into the site RDL.
        /// </summary>
        private void SeedVandVReferenceData()
        {
            foreach (var shortName in VandVCategory.RequiredForItemWrite)
            {
                var category = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = shortName, Name = shortName };
                category.PermissibleClass.Add(ClassKind.Requirement);
                category.PermissibleClass.Add(ClassKind.BinaryRelationship);
                category.PermissibleClass.Add(ClassKind.RequirementsSpecification);

                this.srdl.DefinedCategory.Add(category);
            }

            foreach (var definition in VandVRdlManifest.ParameterTypes)
            {
                this.srdl.ParameterType.Add(new TextParameterType(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = definition.ShortName, Name = definition.Name, Symbol = definition.ShortName });
            }
        }

        private void RegisterInCache(Thing thing, Guid? iterationIid)
        {
            this.assembler.Cache.TryAdd(new CacheKey(thing.Iid, iterationIid), new Lazy<Thing>(() => thing));
        }

        private Requirement AddVnVItem(string shortName)
        {
            var item = new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = shortName, Name = shortName, Owner = this.domain };
            item.Category.Add(this.srdl.DefinedCategory.Single(x => x.ShortName == VandVCategory.VnVItem));
            this.vandVSpecification.Requirement.Add(item);
            this.RegisterInCache(item, this.iteration.Iid);

            return item;
        }

        private RequirementsSpecification AddReport(string shortName)
        {
            var report = new RequirementsSpecification(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = shortName, Name = shortName, Owner = this.domain };
            report.Category.Add(this.srdl.DefinedCategory.Single(x => x.ShortName == VandVCategory.VnVReport));
            this.iteration.RequirementsSpecification.Add(report);
            this.RegisterInCache(report, this.iteration.Iid);

            return report;
        }

        private Requirement AddActivity(string shortName)
        {
            var activity = new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = shortName, Name = shortName, Owner = this.domain };
            activity.Category.Add(this.srdl.DefinedCategory.Single(x => x.ShortName == VandVCategory.VnVActivity));
            this.vandVSpecification.Requirement.Add(activity);
            this.RegisterInCache(activity, this.iteration.Iid);

            return activity;
        }

        private BinaryRelationship AddPerformedByRelationship(Requirement item, Requirement activity)
        {
            var relationship = new BinaryRelationship(Guid.NewGuid(), this.assembler.Cache, this.uri) { Source = item, Target = activity, Owner = this.domain };
            relationship.Category.Add(this.srdl.DefinedCategory.Single(x => x.ShortName == VandVCategory.PerformedBy));
            this.iteration.Relationship.Add(relationship);
            this.RegisterInCache(relationship, this.iteration.Iid);

            return relationship;
        }

        private void SetAttribute(Requirement requirement, string parameterTypeShortName, string value)
        {
            var simpleParameterValue = new SimpleParameterValue(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                ParameterType = this.srdl.ParameterType.Single(x => x.ShortName == parameterTypeShortName),
                Value = new ValueArray<string>(new[] { value })
            };

            requirement.ParameterValue.Add(simpleParameterValue);
            this.RegisterInCache(simpleParameterValue, this.iteration.Iid);
        }
    }
}
