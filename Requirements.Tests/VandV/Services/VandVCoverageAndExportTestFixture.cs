// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVCoverageAndExportTestFixture.cs" company="Starion Group S.A.">
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
    using System.IO;
    using System.Linq;

    using CDP4Requirements.Services;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;

    using ClosedXML.Excel;

    using NUnit.Framework;

    using IOFile = System.IO.File;

    /// <summary>
    /// Suite of tests for the <see cref="VandVCoverageQuery"/> and the <see cref="VandVWorkbookExporter"/>.
    /// </summary>
    [TestFixture]
    public class VandVCoverageAndExportTestFixture
    {
        private readonly Uri uri = new Uri("http://test.com");

        private CDPMessageBus messageBus;
        private Assembler assembler;
        private SiteReferenceDataLibrary srdl;
        private ModelReferenceDataLibrary mrdl;
        private EngineeringModel model;
        private Iteration iteration;
        private RequirementsSpecification specification;
        private DomainOfExpertise domain;
        private Category vnvItemCategory;
        private Category verifiesCategory;

        [SetUp]
        public void SetUp()
        {
            this.messageBus = new CDPMessageBus();
            this.assembler = new Assembler(this.uri, this.messageBus);

            this.srdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "SITERDL" };
            this.mrdl = new ModelReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "MRDL", RequiredRdl = this.srdl };
            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "SYS", Name = "System" };

            var modelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "MODEL", Name = "model" };
            modelSetup.RequiredRdl.Add(this.mrdl);

            this.model = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.uri) { EngineeringModelSetup = modelSetup };
            this.iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.model.Iteration.Add(this.iteration);

            this.specification = new RequirementsSpecification(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "SPEC", Owner = this.domain };
            this.iteration.RequirementsSpecification.Add(this.specification);

            this.vnvItemCategory = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "VnVItem", Name = "VnV Item" };
            this.vnvItemCategory.PermissibleClass.Add(ClassKind.Requirement);

            this.verifiesCategory = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "verifies", Name = "verifies" };
            this.verifiesCategory.PermissibleClass.Add(ClassKind.BinaryRelationship);

            this.srdl.DefinedCategory.Add(this.vnvItemCategory);
            this.srdl.DefinedCategory.Add(this.verifiesCategory);
        }

        [Test]
        public void VerifyThatCoverageDistinguishesCoveredFromUncoveredRequirements()
        {
            var covered = this.AddRequirement("REQ-1", false);
            this.AddRequirement("REQ-2", false);

            var item = this.AddRequirement("VNV-1", true);
            this.SetAttribute(item, "vnv_stage", "FAT");
            this.SetAttribute(item, "vnv_method", "Test");
            this.SetAttribute(item, "vnv_status", "Passed");
            this.AddVerifies(item, covered);

            var result = VandVCoverageQuery.Build(this.iteration);

            Assert.Multiple(() =>
            {
                Assert.That(result.Coverages, Has.Count.EqualTo(2), "V&V items are not themselves rows");
                Assert.That(result.UncoveredCount, Is.EqualTo(1));
            });

            var coveredEntry = result.Coverages.Single(x => x.Requirement.ShortName == "REQ-1");
            Assert.Multiple(() =>
            {
                Assert.That(coveredEntry.VandVItems, Has.Count.EqualTo(1));
                Assert.That(coveredEntry.CellText("FAT"), Is.EqualTo("VNV-1: Test (Passed)"));
                Assert.That(coveredEntry.CellText("PDR"), Is.Empty, "nothing is planned at PDR");
            });
        }

        [Test]
        public void VerifyThatStageColumnsComeFromTheRdlWhenPresent()
        {
            var stageType = new EnumerationParameterType(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "vnv_stage", Name = "stage" };
            stageType.ValueDefinition.Add(new EnumerationValueDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "GATE-A", ShortName = "GATE_A" });
            stageType.ValueDefinition.Add(new EnumerationValueDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "GATE-B", ShortName = "GATE_B" });
            this.srdl.ParameterType.Add(stageType);

            var result = VandVCoverageQuery.Build(this.iteration);

            Assert.That(result.Stages, Is.EqualTo(new[] { "GATE-A", "GATE-B" }), "a project's own gates replace the manifest defaults");
        }

        [Test]
        public void VerifyThatStageColumnsFallBackToTheManifestAndIncludeUsedStages()
        {
            var requirement = this.AddRequirement("REQ-1", false);
            var item = this.AddRequirement("VNV-1", true);
            this.SetAttribute(item, "vnv_stage", "CUSTOM-GATE");
            this.AddVerifies(item, requirement);

            var result = VandVCoverageQuery.Build(this.iteration);

            Assert.Multiple(() =>
            {
                Assert.That(result.Stages, Does.Contain("PDR"), "manifest defaults are used when the RDL has no vnv_stage");
                Assert.That(result.Stages, Does.Contain("CUSTOM-GATE"), "a stage actually in use is always a column");
            });
        }

        [Test]
        public void VerifyThatTheWorkbookIsWrittenWithAllSheets()
        {
            var requirement = this.AddRequirement("REQ-1", false);
            var item = this.AddRequirement("VNV-1", true);
            this.SetAttribute(item, "vnv_stage", "FAT");
            this.SetAttribute(item, "vnv_method", "Test");
            this.SetAttribute(item, "vnv_status", "Passed");
            this.AddVerifies(item, requirement);

            this.AddRequirement("REQ-UNCOVERED", false);

            var path = Path.Combine(Path.GetTempPath(), $"vnv_{Guid.NewGuid():N}.xlsx");

            try
            {
                new VandVWorkbookExporter().Export(this.iteration, path);

                Assert.That(IOFile.Exists(path), Is.True);

                using (var workbook = new XLWorkbook(path))
                {
                    var names = workbook.Worksheets.Select(x => x.Name).ToList();
                    Assert.That(names, Is.EquivalentTo(new[] { "VCD", "VCRM", "Activities", "Execution Records", "Procedures", "NCRs" }));

                    var vcd = workbook.Worksheet("VCD");
                    Assert.That(vcd.Cell(1, 1).GetString(), Is.EqualTo("Requirement"));
                    Assert.That(
                        vcd.Column(5).CellsUsed().Any(c => c.GetString() == "(not covered)"),
                        Is.True,
                        "an uncovered requirement is still listed in the VCD");

                    var execution = workbook.Worksheet("Execution Records");
                    Assert.That(execution.Cell(2, 5).GetString(), Is.EqualTo("Passed"), "an executed item appears in the execution records");
                }
            }
            finally
            {
                if (IOFile.Exists(path))
                {
                    IOFile.Delete(path);
                }
            }
        }

        [Test]
        public void VerifyThatPlannedItemsAreExcludedFromExecutionRecords()
        {
            var requirement = this.AddRequirement("REQ-1", false);
            var item = this.AddRequirement("VNV-1", true);
            this.SetAttribute(item, "vnv_status", "Planned");
            this.AddVerifies(item, requirement);

            var path = Path.Combine(Path.GetTempPath(), $"vnv_{Guid.NewGuid():N}.xlsx");

            try
            {
                new VandVWorkbookExporter().Export(this.iteration, path);

                using (var workbook = new XLWorkbook(path))
                {
                    var execution = workbook.Worksheet("Execution Records");
                    Assert.That(execution.Cell(2, 1).GetString(), Is.Empty, "a merely planned activity is not an execution record");
                }
            }
            finally
            {
                if (IOFile.Exists(path))
                {
                    IOFile.Delete(path);
                }
            }
        }

        [Test]
        public void VerifyThatAllThreeRequestKindsReachTheNcrSheetWithTraceabilityUpFront()
        {
            var requirement = this.AddRequirement("REQ-1", false);
            var vandVItem = this.AddRequirement("VNV-1", true);
            this.AddVerifies(vandVItem, requirement);

            this.AddRequest(new ReviewItemDiscrepancy(Guid.NewGuid(), this.assembler.Cache, this.uri), "RID-1", vandVItem, AnnotationStatusKind.OPEN);
            this.AddRequest(new RequestForDeviation(Guid.NewGuid(), this.assembler.Cache, this.uri), "RFD-1", vandVItem, AnnotationStatusKind.DONE);
            this.AddRequest(new RequestForWaiver(Guid.NewGuid(), this.assembler.Cache, this.uri), "RFW-1", requirement, AnnotationStatusKind.CLOSED);

            var path = Path.Combine(Path.GetTempPath(), $"vandv-ncr-{Guid.NewGuid()}.xlsx");

            try
            {
                new VandVWorkbookExporter().Export(this.iteration, path);

                using (var workbook = new XLWorkbook(path))
                {
                    var sheet = workbook.Worksheet("NCRs");

                    Assert.That(sheet.Cell(1, 1).GetString(), Is.EqualTo("Type"), "the kind of request leads the sheet");
                    Assert.That(sheet.Cell(1, 5).GetString(), Is.EqualTo("V&V Item"));
                    Assert.That(sheet.Cell(1, 6).GetString(), Is.EqualTo("Requirement"));

                    var rows = Enumerable.Range(2, 3)
                        .Select(row => new
                        {
                            Type = sheet.Cell(row, 1).GetString(),
                            Id = sheet.Cell(row, 2).GetString(),
                            Open = sheet.Cell(row, 3).GetString(),
                            VandVItem = sheet.Cell(row, 5).GetString(),
                            Requirement = sheet.Cell(row, 6).GetString()
                        })
                        .ToList();

                    var rid = rows.Single(x => x.Id == "RID-1");
                    Assert.That(rid.Type, Is.EqualTo("Review Item Discrepancy"));
                    Assert.That(rid.Open, Is.EqualTo("OPEN"));
                    Assert.That(rid.VandVItem, Is.EqualTo("VNV-1"), "a request on a V&V item names the item");
                    Assert.That(rid.Requirement, Is.EqualTo("REQ-1"), "and the requirement that item verifies");

                    var rfd = rows.Single(x => x.Id == "RFD-1");
                    Assert.That(rfd.Type, Is.EqualTo("Request for Deviation"), "RFDs used to be filtered out entirely");
                    Assert.That(rfd.Open, Is.EqualTo("closed"), "DONE means implemented, not outstanding");

                    var rfw = rows.Single(x => x.Id == "RFW-1");
                    Assert.That(rfw.Type, Is.EqualTo("Request for Waiver"));
                    Assert.That(rfw.VandVItem, Is.Empty, "a request raised straight on a requirement has no V&V item");
                    Assert.That(rfw.Requirement, Is.EqualTo("REQ-1"));
                }
            }
            finally
            {
                if (IOFile.Exists(path))
                {
                    IOFile.Delete(path);
                }
            }
        }

        [Test]
        public void VerifyThatTheAnnotationStateIsOpenUntilEveryRequestIsClosedOut()
        {
            var requirement = this.AddRequirement("REQ-1", false);
            var vandVItem = this.AddRequirement("VNV-1", true);
            this.AddVerifies(vandVItem, requirement);

            Assert.That(AnnotationQuery.SummarizeState(AnnotationQuery.QueryFor(this.iteration, vandVItem)), Is.EqualTo(AnnotationState.None));

            var rid = this.AddRequest(new ReviewItemDiscrepancy(Guid.NewGuid(), this.assembler.Cache, this.uri), "RID-1", vandVItem, AnnotationStatusKind.OPEN);
            var rfd = this.AddRequest(new RequestForDeviation(Guid.NewGuid(), this.assembler.Cache, this.uri), "RFD-1", vandVItem, AnnotationStatusKind.CLOSED);

            Assert.That(AnnotationQuery.SummarizeState(AnnotationQuery.QueryFor(this.iteration, vandVItem)), Is.EqualTo(AnnotationState.Open), "one request is still open");

            rid.Status = AnnotationStatusKind.DONE;

            Assert.Multiple(() =>
            {
                Assert.That(AnnotationQuery.SummarizeState(AnnotationQuery.QueryFor(this.iteration, vandVItem)), Is.EqualTo(AnnotationState.Resolved));
                Assert.That(AnnotationQuery.IsOpen(rfd), Is.False);
            });
        }

        private ModellingAnnotationItem AddRequest(ModellingAnnotationItem annotation, string shortName, Thing annotatedThing, AnnotationStatusKind status)
        {
            annotation.ShortName = shortName;
            annotation.Title = shortName;
            annotation.Status = status;
            annotation.Owner = this.domain;

            var reference = new ModellingThingReference(annotatedThing);
            annotation.PrimaryAnnotatedThing = reference;
            annotation.RelatedThing.Add(reference);

            this.model.ModellingAnnotation.Add(annotation);

            return annotation;
        }

        private Requirement AddRequirement(string shortName, bool isVnVItem)
        {
            var requirement = new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = shortName, Name = shortName, Owner = this.domain };

            if (isVnVItem)
            {
                requirement.Category.Add(this.vnvItemCategory);
            }

            this.specification.Requirement.Add(requirement);
            return requirement;
        }

        private void AddVerifies(Requirement source, Requirement target)
        {
            var relationship = new BinaryRelationship(Guid.NewGuid(), this.assembler.Cache, this.uri) { Source = source, Target = target, Owner = this.domain };
            relationship.Category.Add(this.verifiesCategory);
            this.iteration.Relationship.Add(relationship);
        }

        private void SetAttribute(Requirement item, string parameterTypeShortName, string value)
        {
            item.SetVandVAttribute(parameterTypeShortName, value, this.assembler.Cache, this.uri);
        }
    }
}
