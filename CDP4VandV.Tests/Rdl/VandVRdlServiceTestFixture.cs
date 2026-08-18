// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVRdlServiceTestFixture.cs" company="Starion Group S.A.">
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

namespace CDP4VandV.Tests.Rdl
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using CDP4VandV.Rdl;
    using CDP4VandV.ViewModels;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Operations;
    using CDP4Dal.Permission;

    using Moq;

    using NUnit.Framework;

    using DTO = CDP4Common.DTO;

    /// <summary>
    /// Suite of tests for the <see cref="VandVRdlService"/> check-and-seed logic.
    /// </summary>
    [TestFixture]
    public class VandVRdlServiceTestFixture
    {
        private readonly Uri uri = new Uri("http://test.com");

        private CDPMessageBus messageBus;
        private Assembler assembler;
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;

        private SiteDirectory siteDirectory;
        private EngineeringModelSetup engineeringModelSetup;
        private SiteReferenceDataLibrary srdl;
        private ModelReferenceDataLibrary mrdl;

        private OperationContainer capturedOperationContainer;
        private VandVRdlService service;

        [SetUp]
        public void SetUp()
        {
            this.messageBus = new CDPMessageBus();
            this.assembler = new Assembler(this.uri, this.messageBus);
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();

            this.siteDirectory = new SiteDirectory(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.engineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.srdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "site rdl", ShortName = "SITERDL" };
            this.mrdl = new ModelReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "model rdl", ShortName = "MODELRDL", RequiredRdl = this.srdl };

            this.siteDirectory.Model.Add(this.engineeringModelSetup);
            this.siteDirectory.SiteReferenceDataLibrary.Add(this.srdl);
            this.engineeringModelSetup.RequiredRdl.Add(this.mrdl);

            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);

            this.capturedOperationContainer = null;
            this.session
                .Setup(x => x.Write(It.IsAny<OperationContainer>()))
                .Callback<OperationContainer>(container => this.capturedOperationContainer = container)
                .Returns(Task.CompletedTask);

            this.service = new VandVRdlService();
        }

        [Test]
        public async Task VerifyThatUserDefinedStageGatesReplaceTheManifestExamples()
        {
            var check = this.service.Check(this.mrdl);
            var projectGates = new List<string> { "MDR", "PRR", "Flight Acceptance" };

            await this.service.Seed(this.session.Object, this.mrdl, check, projectGates);

            var stageType = this.capturedOperationContainer.Operations
                .Select(x => x.ModifiedThing)
                .OfType<DTO.EnumerationParameterType>()
                .Single(x => x.ShortName == "vnv_stage");

            var valueDefinitions = this.capturedOperationContainer.Operations
                .Select(x => x.ModifiedThing)
                .OfType<DTO.EnumerationValueDefinition>()
                .Where(x => stageType.ValueDefinition.Select(ordered => ordered.V).Contains(x.Iid))
                .Select(x => x.Name)
                .ToList();

            Assert.That(valueDefinitions, Is.EquivalentTo(projectGates), "the project's gates are seeded, not the examples");
            Assert.That(valueDefinitions, Does.Not.Contain("SRR"), "no hard-coded example survives");

            // the other enumerations are untouched by the override
            var methodType = this.capturedOperationContainer.Operations
                .Select(x => x.ModifiedThing)
                .OfType<DTO.EnumerationParameterType>()
                .Single(x => x.ShortName == "vnv_method");

            Assert.That(methodType.ValueDefinition, Has.Count.GreaterThan(0));
        }

        [Test]
        public void VerifyThatTheStageGateDialogEditsRowsAndParsesThemInOrder()
        {
            var dialog = new StageGateDialogViewModel(new[] { "SRR", "PDR" });

            Assert.That(dialog.StageGates.Select(x => x.Name), Is.EqualTo(new[] { "SRR", "PDR" }), "the proposal prefills the table");

            dialog.StageGates.Clear();
            dialog.StageGates.Add(new StageGateRowViewModel { Name = "  MDR  " });
            dialog.StageGates.Add(new StageGateRowViewModel { Name = "PRR" });
            dialog.StageGates.Add(new StageGateRowViewModel { Name = "   " });
            dialog.StageGates.Add(new StageGateRowViewModel { Name = "prr" });
            dialog.StageGates.Add(new StageGateRowViewModel { Name = "Flight Acceptance" });

            Assert.That(dialog.ParseStageGates(), Is.EqualTo(new[] { "MDR", "PRR", "Flight Acceptance" }),
                "trimmed, blanks dropped, case-insensitive duplicates removed, order kept");

            // the order of the rows is the order of the seeded gates
            dialog.StageGates.Move(4, 1);

            Assert.That(dialog.ParseStageGates(), Is.EqualTo(new[] { "MDR", "Flight Acceptance", "PRR" }));
        }

        [Test]
        public void VerifyThatCheckOnVirginModelReportsEverythingMissing()
        {
            var result = this.service.Check(this.mrdl);

            Assert.That(result.HasMissingItems, Is.True);
            Assert.That(result.MissingParameterTypes, Has.Count.EqualTo(VandVRdlManifest.ParameterTypes.Count));
            Assert.That(result.MissingCategories, Has.Count.EqualTo(VandVRdlManifest.Categories.Count));
            Assert.That(result.MissingParameterizedCategoryRules, Has.Count.EqualTo(VandVRdlManifest.ParameterizedCategoryRules.Count));
        }

        [Test]
        public void VerifyThatCheckFindsEverythingAlreadyPresentInTheChain()
        {
            this.PopulateChainWithFullManifest();

            var result = this.service.Check(this.mrdl);

            Assert.That(result.HasMissingItems, Is.False);
            Assert.That(result.MissingParameterTypes, Is.Empty);
            Assert.That(result.MissingCategories, Is.Empty);
            Assert.That(result.MissingParameterizedCategoryRules, Is.Empty);
        }

        [Test]
        public void VerifyThatCheckReportsOnlyThePartiallyMissingItems()
        {
            this.srdl.ParameterType.Add(new TextParameterType(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "vnv_method", Name = "vnv_method", Symbol = "vnv_method" });
            this.srdl.ParameterType.Add(new TextParameterType(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "vnv_status", Name = "vnv_status", Symbol = "vnv_status" });

            var result = this.service.Check(this.mrdl);

            Assert.That(result.MissingParameterTypes, Has.Count.EqualTo(VandVRdlManifest.ParameterTypes.Count - 2));
            Assert.That(result.MissingParameterTypes.Select(x => x.ShortName), Does.Not.Contain("vnv_method"));
            Assert.That(result.MissingParameterTypes.Select(x => x.ShortName), Does.Not.Contain("vnv_status"));
        }

        [Test]
        public async Task VerifyThatSeedWritesEveryMissingThingInOneTransaction()
        {
            var check = this.service.Check(this.mrdl);

            await this.service.Seed(this.session.Object, this.mrdl, check);

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()), Times.Once);
            Assert.That(this.capturedOperationContainer, Is.Not.Null);

            var operations = this.capturedOperationContainer.Operations.ToList();

            Assert.That(operations.Count(o => o.ModifiedThing is DTO.ParameterType), Is.EqualTo(VandVRdlManifest.ParameterTypes.Count));
            Assert.That(operations.Count(o => o.ModifiedThing is DTO.Category), Is.EqualTo(VandVRdlManifest.Categories.Count));
            Assert.That(operations.Count(o => o.ModifiedThing is DTO.ParameterizedCategoryRule), Is.EqualTo(VandVRdlManifest.ParameterizedCategoryRules.Count));
        }

        [Test]
        public async Task VerifyThatSeedResolvesSuperCategoriesAndRuleReferences()
        {
            var check = this.service.Check(this.mrdl);

            await this.service.Seed(this.session.Object, this.mrdl, check);

            var operations = this.capturedOperationContainer.Operations.ToList();

            var subCategory = operations.Select(o => o.ModifiedThing).OfType<DTO.Category>().Single(c => c.ShortName == "VerificationItem");
            Assert.That(subCategory.SuperCategory, Is.Not.Empty, "the Verification Item sub-category must reference its VnV Item super-category");

            var rule = operations.Select(o => o.ModifiedThing).OfType<DTO.ParameterizedCategoryRule>().Single();
            Assert.That(rule.Category, Is.Not.EqualTo(Guid.Empty), "the rule must reference a category");
            Assert.That(rule.ParameterType, Has.Count.EqualTo(4), "the rule must make the four mandatory attributes required");
        }

        [Test]
        public void VerifyThatCheckResolvesTheRequirementTargetCategoryCaseInsensitively()
        {
            var requirementCategory = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "req", Name = "Requirement" };
            requirementCategory.PermissibleClass.Add(ClassKind.Requirement);
            this.srdl.DefinedCategory.Add(requirementCategory);

            var result = this.service.Check(this.mrdl);

            Assert.That(result.RequirementTargetCategory, Is.EqualTo(requirementCategory));
            Assert.That(result.MissingBinaryRelationshipRules, Has.Count.EqualTo(VandVRdlManifest.BinaryRelationshipRules.Count));
        }

        [Test]
        public void VerifyThatCheckReturnsNullTargetCategoryWhenTheModelHasNoRequirementCategory()
        {
            var result = this.service.Check(this.mrdl);

            Assert.That(result.RequirementTargetCategory, Is.Null);
        }

        [Test]
        public async Task VerifyThatSeedCreatesRelationshipRulesTargetingTheRequirementCategory()
        {
            var requirementCategory = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "REQUIREMENT", Name = "Requirement" };
            requirementCategory.PermissibleClass.Add(ClassKind.Requirement);
            this.srdl.DefinedCategory.Add(requirementCategory);

            var check = this.service.Check(this.mrdl);

            await this.service.Seed(this.session.Object, this.mrdl, check);

            var relationshipRules = this.capturedOperationContainer.Operations
                .Select(o => o.ModifiedThing)
                .OfType<DTO.BinaryRelationshipRule>()
                .ToList();

            Assert.That(relationshipRules, Has.Count.EqualTo(VandVRdlManifest.BinaryRelationshipRules.Count));
            Assert.That(relationshipRules.Select(x => x.TargetCategory), Is.All.EqualTo(requirementCategory.Iid));
            Assert.That(relationshipRules.All(x => x.RelationshipCategory != Guid.Empty), Is.True);
            Assert.That(relationshipRules.All(x => x.SourceCategory != Guid.Empty), Is.True);
        }

        [Test]
        public async Task VerifyThatSeedSkipsRelationshipRulesWhenNoRequirementCategoryExists()
        {
            var check = this.service.Check(this.mrdl);

            await this.service.Seed(this.session.Object, this.mrdl, check);

            var relationshipRules = this.capturedOperationContainer.Operations
                .Select(o => o.ModifiedThing)
                .OfType<DTO.BinaryRelationshipRule>()
                .ToList();

            Assert.That(relationshipRules, Is.Empty);
        }

        [Test]
        public async Task VerifyThatSeedWritesNothingWhenNothingIsMissing()
        {
            this.PopulateChainWithFullManifest();
            var check = this.service.Check(this.mrdl);

            await this.service.Seed(this.session.Object, this.mrdl, check);

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()), Times.Never);
        }

        [Test]
        public void VerifyThatCanSeedReflectsThePermissionService()
        {
            Assert.That(this.service.CanSeed(this.session.Object, this.mrdl), Is.True);

            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(false);

            Assert.That(this.service.CanSeed(this.session.Object, this.mrdl), Is.False);
        }

        /// <summary>
        /// Seeds the whole <see cref="VandVRdlManifest"/> into the <see cref="srdl"/> (higher in the chain than the
        /// <see cref="mrdl"/>) so the chain lookup finds every item and the check reports nothing missing.
        /// </summary>
        private void PopulateChainWithFullManifest()
        {
            foreach (var definition in VandVRdlManifest.ParameterTypes)
            {
                this.srdl.ParameterType.Add(new TextParameterType(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = definition.ShortName, Name = definition.Name, Symbol = definition.ShortName });
            }

            foreach (var definition in VandVRdlManifest.Categories)
            {
                this.srdl.DefinedCategory.Add(new Category(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = definition.ShortName, Name = definition.Name });
            }

            foreach (var definition in VandVRdlManifest.ParameterizedCategoryRules)
            {
                this.srdl.Rule.Add(new ParameterizedCategoryRule(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = definition.ShortName, Name = definition.Name });
            }

            foreach (var definition in VandVRdlManifest.BinaryRelationshipRules)
            {
                this.srdl.Rule.Add(new BinaryRelationshipRule(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = definition.ShortName, Name = definition.Name });
            }
        }
    }
}
