// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReqIfExportDialogViewModelTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2026 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
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

namespace CDP4Requirements.Tests.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using System.Xml.Schema;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.Navigation;
    using CDP4Composition.Services;

    using CDP4Dal;

    using CDP4Requirements.Rdl;
    using CDP4Requirements.ViewModels;

    using CommonServiceLocator;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    using ReqIFSharp;

    [TestFixture]
    internal class ReqIfExportDialogViewModelTestFixture
    {
        private Mock<ISession> session;
        private Assembler assembler;
        private Mock<IOpenSaveFileDialogService> fileDialogService;
        private Mock<IReqIFSerializer> serializer;
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IMessageBoxService> messageBoxService;
        private EngineeringModel model;
        private Iteration iteration;
        private SiteDirectory sitedir;
        private EngineeringModelSetup modelsetup;
        private IterationSetup iterationSetup;

        private readonly Uri uri = new Uri("http://test.com");
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.messageBus = new CDPMessageBus();
            this.serviceLocator = new Mock<IServiceLocator>();
            this.messageBoxService = new Mock<IMessageBoxService>();

            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IMessageBoxService>()).Returns(this.messageBoxService.Object);

            this.session = new Mock<ISession>();
            this.assembler = new Assembler(this.uri, this.messageBus);
            this.serializer = new Mock<IReqIFSerializer>();
            this.fileDialogService = new Mock<IOpenSaveFileDialogService>();
            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString());
            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);

            this.sitedir = new SiteDirectory(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.modelsetup = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "model" };
            this.iterationSetup = new IterationSetup(Guid.NewGuid(), this.assembler.Cache, this.uri);

            this.sitedir.Model.Add(this.modelsetup);
            this.modelsetup.IterationSetup.Add(this.iterationSetup);

            this.model = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.uri) { EngineeringModelSetup = this.modelsetup };
            this.iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri) { IterationSetup = this.iterationSetup };
            this.model.Iteration.Add(this.iteration);
        }

        [Test]
        public void VerifyThatConstructorWorks()
        {
            var vm = new ReqIfExportDialogViewModel(new List<ISession> { this.session.Object }, new List<Iteration> { this.iteration }, this.fileDialogService.Object, this.serializer.Object);
            Assert.Multiple(() =>
            {
                Assert.That(vm, Is.Not.Null);
                Assert.That(vm.Sessions, Is.Not.Null);
                Assert.That(vm.Iterations, Is.Not.Null);
                Assert.That(vm.OkCommand, Is.Not.Null);
                Assert.That(vm.CancelCommand, Is.Not.Null);
                Assert.That(vm.BrowseCommand, Is.Not.Null);
                Assert.That(vm.IncludeDeprecated, Is.False);
            });
        }

        [Test]
        public void VerifyThatExceptionRaises1()
        {
            Assert.Throws<ArgumentNullException>(() => new ReqIfExportDialogViewModel(null, new List<Iteration>(), this.fileDialogService.Object, this.serializer.Object));
        }

        [Test]
        public void VerifyThatExceptionRaises2()
        {
            Assert.Throws<ArgumentNullException>(() => new ReqIfExportDialogViewModel(new List<ISession>(), null, this.fileDialogService.Object, this.serializer.Object));
        }

        [Test]
        public void VerifyThatExceptionRaises3()
        {
            Assert.Throws<ArgumentNullException>(() => new ReqIfExportDialogViewModel(new List<ISession>(), new List<Iteration>(), null, this.serializer.Object));
        }

        [Test]
        public void VerifyThatExceptionRaises4()
        {
            Assert.Throws<ArgumentNullException>(() => new ReqIfExportDialogViewModel(new List<ISession>(), new List<Iteration>(), this.fileDialogService.Object, null));
        }

        [Test]
        public void VerifyThatSelectingIterationPopulatesRequirementsSpecificationsSelectedByDefault()
        {
            var spec1 = new RequirementsSpecification(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "spec1", Name = "Specification 1" };
            var spec2 = new RequirementsSpecification(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "spec2", Name = "Specification 2" };
            this.iteration.RequirementsSpecification.Add(spec1);
            this.iteration.RequirementsSpecification.Add(spec2);

            var vm = new ReqIfExportDialogViewModel(new List<ISession> { this.session.Object }, new List<Iteration> { this.iteration }, this.fileDialogService.Object, this.serializer.Object);

            Assert.That(vm.RequirementsSpecifications, Is.Empty);

            vm.SelectedIteration = vm.Iterations.First();

            Assert.Multiple(() =>
            {
                Assert.That(vm.RequirementsSpecifications, Has.Count.EqualTo(2));
                Assert.That(vm.RequirementsSpecifications.All(x => x.IsSelected), Is.True);
            });
        }

        [Test]
        public async Task VerifyThatExportWithoutSelectedSpecificationIsBlocked()
        {
            var spec1 = new RequirementsSpecification(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "spec1", Name = "Specification 1" };
            this.iteration.RequirementsSpecification.Add(spec1);

            this.fileDialogService.Setup(
                    x => x.GetSaveFileDialog(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 1))
                .Returns("test");

            var vm = new ReqIfExportDialogViewModel(new List<ISession> { this.session.Object }, new List<Iteration> { this.iteration }, this.fileDialogService.Object, this.serializer.Object);

            vm.SelectedIteration = vm.Iterations.First();
            await vm.BrowseCommand.Execute();

            foreach (var row in vm.RequirementsSpecifications)
            {
                row.IsSelected = false;
            }

            await vm.ExecuteOk();

            Assert.That(vm.ErrorMessage, Is.EqualTo("Select at least one requirements specification to export."));
            this.serializer.Verify(x => x.Serialize(It.IsAny<ReqIF>(), It.IsAny<string>(), It.IsAny<ValidationEventHandler>()), Times.Never);
        }

        [Test]
        public async Task VerifyThatPreviewSpecObjectTypesListsTheExportedTypes()
        {
            var spec = new RequirementsSpecification(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "spec", Name = "Specification" };
            var requirement = new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "req1", Name = "Requirement 1" };
            spec.Requirement.Add(requirement);
            this.iteration.RequirementsSpecification.Add(spec);

            var vm = new ReqIfExportDialogViewModel(new List<ISession> { this.session.Object }, new List<Iteration> { this.iteration }, this.fileDialogService.Object, this.serializer.Object);

            vm.SelectedIteration = vm.Iterations.First();

            Assert.That(vm.SpecObjectTypesPreview, Is.Empty);

            await vm.PreviewSpecObjectTypesCommand.Execute();

            var requirementType = vm.SpecObjectTypesPreview.Single(x => x.Name == "Requirement");
            Assert.Multiple(() =>
            {
                Assert.That(requirementType.NumberOfObjects, Is.EqualTo(1));
                Assert.That(requirementType.DistinguishingAttributes, Is.EqualTo("(no extra parameters)"));
            });
        }

        [Test]
        public void VerifyThatDeprecatedSpecificationsAreHiddenUnlessIncludeDeprecatedIsChecked()
        {
            var normalSpec = new RequirementsSpecification(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "spec", Name = "Specification" };
            var deprecatedSpec = new RequirementsSpecification(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "dep", Name = "Deprecated", IsDeprecated = true };
            this.iteration.RequirementsSpecification.Add(normalSpec);
            this.iteration.RequirementsSpecification.Add(deprecatedSpec);

            var vm = new ReqIfExportDialogViewModel(new List<ISession> { this.session.Object }, new List<Iteration> { this.iteration }, this.fileDialogService.Object, this.serializer.Object);

            vm.SelectedIteration = vm.Iterations.First();

            // the deprecated specification is hidden by default (Include Deprecated is off)
            Assert.Multiple(() =>
            {
                Assert.That(vm.RequirementsSpecifications.Select(x => x.RequirementsSpecification), Does.Contain(normalSpec));
                Assert.That(vm.RequirementsSpecifications.Select(x => x.RequirementsSpecification), Does.Not.Contain(deprecatedSpec));
            });

            // turning on Include Deprecated reveals it, selected by default
            vm.IncludeDeprecated = true;

            Assert.Multiple(() =>
            {
                Assert.That(vm.RequirementsSpecifications.Select(x => x.RequirementsSpecification), Does.Contain(deprecatedSpec));
                Assert.That(vm.RequirementsSpecifications.All(x => x.IsSelected), Is.True);
            });
        }

        [Test]
        public async Task VerifyThatPreviewWithoutSelectedSpecificationIsBlocked()
        {
            var spec = new RequirementsSpecification(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "spec", Name = "Specification" };
            this.iteration.RequirementsSpecification.Add(spec);

            var vm = new ReqIfExportDialogViewModel(new List<ISession> { this.session.Object }, new List<Iteration> { this.iteration }, this.fileDialogService.Object, this.serializer.Object);

            vm.SelectedIteration = vm.Iterations.First();

            foreach (var row in vm.RequirementsSpecifications)
            {
                row.IsSelected = false;
            }

            await vm.PreviewSpecObjectTypesCommand.Execute();

            Assert.Multiple(() =>
            {
                Assert.That(vm.SpecObjectTypesPreview, Is.Empty);
                Assert.That(vm.ErrorMessage, Is.EqualTo("Select at least one requirements specification to preview."));
            });
        }

        [Test]
        public async Task VeriyThatOkCommandWorks()
        {
            var spec = new RequirementsSpecification(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "spec", Name = "Specification" };
            this.iteration.RequirementsSpecification.Add(spec);

            var vm = new ReqIfExportDialogViewModel(new List<ISession> { this.session.Object }, new List<Iteration> { this.iteration }, this.fileDialogService.Object, this.serializer.Object);

            this.fileDialogService.Setup(
                    x => x.GetSaveFileDialog(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 1))
                .Returns("test");

            vm.SelectedIteration = vm.Iterations.First();
            Assert.Multiple(() =>
            {
                Assert.That(vm.SelectedIteration.IterationNumber, Is.Not.Null.Or.Empty);
                Assert.That(vm.SelectedIteration.Model, Is.Not.Null.Or.Empty);
                Assert.That(vm.SelectedIteration.DataSourceUri, Is.Not.Null.Or.Empty);
                Assert.That(vm.SelectedIteration.Iteration, Is.Not.Null);
            });

            Assert.Multiple(() =>
            {
                Assert.That(((ICommand)vm.OkCommand).CanExecute(null), Is.False);
                Assert.That(((ICommand)vm.CancelCommand).CanExecute(null), Is.True);
                Assert.That(((ICommand)vm.BrowseCommand).CanExecute(null), Is.True);
            });

            await vm.BrowseCommand.Execute();
            Assert.Multiple(() =>
            {
                Assert.That(vm.Path, Is.Not.Null.Or.Empty);

                Assert.That(((ICommand)vm.OkCommand).CanExecute(null), Is.True);
            });

            await vm.ExecuteOk();
            Assert.That(vm.DialogResult, Is.Not.Null);
        }

        [Test]
        public async Task VerifyThatValidityChecksOnlyTheSelectedSpecifications()
        {
            var srdl = this.AddModelRdl();

            var category = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "reqcat", Name = "req cat" };
            category.PermissibleClass.Add(ClassKind.Requirement);
            var parameterType = new BooleanParameterType(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "bool", Name = "bool" };
            var rule = new ParameterizedCategoryRule(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "ParamRule", Name = "param rule", Category = category };
            rule.ParameterType.Add(parameterType);

            srdl.ParameterType.Add(parameterType);
            srdl.DefinedCategory.Add(category);
            srdl.Rule.Add(rule);

            // a violating requirement (categorized, but missing the mandatory parameter) in a specification we will NOT export
            var violatingSpec = new RequirementsSpecification(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "viol", Name = "Violating" };
            var violatingRequirement = new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "vr", Name = "Violating requirement" };
            violatingRequirement.Category.Add(category);
            violatingSpec.Requirement.Add(violatingRequirement);

            // a clean specification we will export
            var cleanSpec = new RequirementsSpecification(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "clean", Name = "Clean" };
            cleanSpec.Requirement.Add(new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "cr", Name = "Clean requirement" });

            this.iteration.RequirementsSpecification.Add(violatingSpec);
            this.iteration.RequirementsSpecification.Add(cleanSpec);

            this.fileDialogService.Setup(x => x.GetSaveFileDialog(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 1)).Returns("test");

            var vm = new ReqIfExportDialogViewModel(new List<ISession> { this.session.Object }, new List<Iteration> { this.iteration }, this.fileDialogService.Object, this.serializer.Object);

            vm.SelectedIteration = vm.Iterations.First();
            await vm.BrowseCommand.Execute();

            foreach (var row in vm.RequirementsSpecifications)
            {
                row.IsSelected = row.RequirementsSpecification == cleanSpec;
            }

            await vm.ExecuteOk();

            Assert.That(vm.ErrorMessage, Is.Null.Or.Empty, "the violation lives in an unselected specification, so it must not block the export");
            this.serializer.Verify(x => x.Serialize(It.IsAny<ReqIF>(), It.IsAny<string>(), It.IsAny<ValidationEventHandler>()), Times.Once);
        }

        [Test]
        public async Task VerifyThatAVnVItemInheritingMethodAndStageDoesNotBlockTheExport()
        {
            var srdl = this.AddModelRdl();

            var methodType = new EnumerationParameterType(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = VandVParameter.Method, Name = "V&V Method" };
            methodType.ValueDefinition.Add(new EnumerationValueDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "Test", ShortName = "Test" });
            var stageType = new EnumerationParameterType(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = VandVParameter.Stage, Name = "V&V Stage Gate" };
            stageType.ValueDefinition.Add(new EnumerationValueDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "FAT", ShortName = "FAT" });
            var acceptanceType = new TextParameterType(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = VandVParameter.AcceptanceCriteria, Name = "V&V Acceptance Criteria" };
            var statusType = new EnumerationParameterType(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = VandVParameter.Status, Name = "V&V Status" };
            statusType.ValueDefinition.Add(new EnumerationValueDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "Planned", ShortName = "Planned" });

            srdl.ParameterType.Add(methodType);
            srdl.ParameterType.Add(stageType);
            srdl.ParameterType.Add(acceptanceType);
            srdl.ParameterType.Add(statusType);

            var vnvItemCategory = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = VandVCategory.VnVItem, Name = "VnV Item" };
            vnvItemCategory.PermissibleClass.Add(ClassKind.Requirement);
            var vnvActivityCategory = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = VandVCategory.VnVActivity, Name = "VnV Activity" };
            vnvActivityCategory.PermissibleClass.Add(ClassKind.Requirement);
            var performedByCategory = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = VandVCategory.PerformedBy, Name = "performed by" };
            performedByCategory.PermissibleClass.Add(ClassKind.BinaryRelationship);

            srdl.DefinedCategory.Add(vnvItemCategory);
            srdl.DefinedCategory.Add(vnvActivityCategory);
            srdl.DefinedCategory.Add(performedByCategory);

            var itemRule = new ParameterizedCategoryRule(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "VnVItemAttributesRule", Name = "V&V Item mandatory attributes", Category = vnvItemCategory };
            itemRule.ParameterType.Add(methodType);
            itemRule.ParameterType.Add(stageType);
            itemRule.ParameterType.Add(acceptanceType);
            itemRule.ParameterType.Add(statusType);
            srdl.Rule.Add(itemRule);

            var item = new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "VNV_SYS_REQ1", Name = "Verify SYS-REQ1" };
            item.Category.Add(vnvItemCategory);
            item.ParameterValue.Add(new SimpleParameterValue(Guid.NewGuid(), this.assembler.Cache, this.uri) { ParameterType = acceptanceType, Value = new ValueArray<string>(new[] { "meets spec" }) });
            item.ParameterValue.Add(new SimpleParameterValue(Guid.NewGuid(), this.assembler.Cache, this.uri) { ParameterType = statusType, Value = new ValueArray<string>(new[] { "Planned" }) });

            var itemSpec = new RequirementsSpecification(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "VNV", Name = "V&V" };
            itemSpec.Requirement.Add(item);

            var activity = new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "ACT_1", Name = "Produce mass budget" };
            activity.Category.Add(vnvActivityCategory);
            activity.ParameterValue.Add(new SimpleParameterValue(Guid.NewGuid(), this.assembler.Cache, this.uri) { ParameterType = methodType, Value = new ValueArray<string>(new[] { "Test" }) });
            activity.ParameterValue.Add(new SimpleParameterValue(Guid.NewGuid(), this.assembler.Cache, this.uri) { ParameterType = stageType, Value = new ValueArray<string>(new[] { "FAT" }) });

            var activitySpec = new RequirementsSpecification(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "ACTS", Name = "Activities" };
            activitySpec.Requirement.Add(activity);

            var performedBy = new BinaryRelationship(Guid.NewGuid(), this.assembler.Cache, this.uri) { Source = item, Target = activity };
            performedBy.Category.Add(performedByCategory);

            this.iteration.RequirementsSpecification.Add(itemSpec);
            this.iteration.RequirementsSpecification.Add(activitySpec);
            this.iteration.Relationship.Add(performedBy);

            this.fileDialogService.Setup(x => x.GetSaveFileDialog(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 1)).Returns("test");

            var vm = new ReqIfExportDialogViewModel(new List<ISession> { this.session.Object }, new List<Iteration> { this.iteration }, this.fileDialogService.Object, this.serializer.Object);

            vm.SelectedIteration = vm.Iterations.First();
            await vm.BrowseCommand.Execute();

            foreach (var row in vm.RequirementsSpecifications)
            {
                row.IsSelected = row.RequirementsSpecification == itemSpec;
            }

            await vm.ExecuteOk();

            Assert.That(vm.ErrorMessage, Is.Null.Or.Empty, "the item leaves method and stage to its activity, which supplies them, so it is complete and must not block the export");
            this.serializer.Verify(x => x.Serialize(It.IsAny<ReqIF>(), It.IsAny<string>(), It.IsAny<ValidationEventHandler>()), Times.Once);
        }

        /// <summary>
        /// Adds a model reference data library (chained to a site RDL) to the model setup, so rules can be resolved.
        /// </summary>
        /// <returns>The site reference data library the reference data is added to.</returns>
        private SiteReferenceDataLibrary AddModelRdl()
        {
            var srdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "SITERDL", Name = "site rdl" };
            this.sitedir.SiteReferenceDataLibrary.Add(srdl);

            var mrdl = new ModelReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "MODELRDL", Name = "model rdl", RequiredRdl = srdl };
            this.modelsetup.RequiredRdl.Add(mrdl);

            return srdl;
        }
    }
}
