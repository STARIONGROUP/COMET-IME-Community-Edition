// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVBrowserViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace CDP4Requirements.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Threading;
    using System.Windows;
    using System.Windows.Input;

    using CDP4Requirements.Services;
    using CDP4Requirements.ViewModels;
    using CDP4Requirements.ViewModels.Rows;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.DragDrop;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="VandVBrowserViewModel"/> class.
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class VandVBrowserViewModelTestFixture
    {
        private readonly Uri uri = new Uri("http://test.com");

        private CDPMessageBus messageBus;
        private Assembler assembler;
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IDialogNavigationService> dialogNavigationService;

        private Person person;
        private DomainOfExpertise domain;
        private Participant participant;
        private EngineeringModel model;
        private EngineeringModelSetup modelSetup;
        private Iteration iteration;
        private IterationSetup iterationSetup;
        private RequirementsSpecification specification;
        private Category vnvItemCategory;
        private Category verifiesCategory;

        private PropertyInfoHolder revision;

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.messageBus = new CDPMessageBus();
            this.assembler = new Assembler(this.uri, this.messageBus);
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.dialogNavigationService = new Mock<IDialogNavigationService>();
            this.revision = new PropertyInfoHolder();

            this.permissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);

            this.person = new Person(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "test" };
            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "domain", ShortName = "SYS" };
            this.participant = new Participant(Guid.NewGuid(), this.assembler.Cache, this.uri) { Person = this.person };
            this.participant.Domain.Add(this.domain);

            this.model = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.modelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "model" };
            this.iterationSetup = new IterationSetup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri) { IterationSetup = this.iterationSetup };
            this.specification = new RequirementsSpecification(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "SPEC", Name = "spec", Owner = this.domain };

            this.modelSetup.IterationSetup.Add(this.iterationSetup);
            this.modelSetup.Participant.Add(this.participant);
            this.model.EngineeringModelSetup = this.modelSetup;
            this.model.Iteration.Add(this.iteration);
            this.iteration.RequirementsSpecification.Add(this.specification);

            this.vnvItemCategory = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "VnVItem", Name = "VnV Item" };
            this.vnvItemCategory.PermissibleClass.Add(ClassKind.Requirement);

            this.verifiesCategory = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "verifies", Name = "verifies" };
            this.verifiesCategory.PermissibleClass.Add(ClassKind.BinaryRelationship);

            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString());
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>> { { this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, this.participant) } });
            this.session.Setup(x => x.QuerySelectedDomainOfExpertise(this.iteration)).Returns(this.domain);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatOnlySystemRequirementsBecomeRootRows()
        {
            this.AddRequirement("VNV-1", true);
            this.AddRequirement("REQ-1", false);

            var browser = this.CreateBrowser();

            Assert.That(browser.RequirementRows, Has.Count.EqualTo(1), "V&V items are children, not root rows");
            Assert.That(browser.RequirementRows.Single().ShortName, Is.EqualTo("REQ-1"));
            Assert.That(browser.RequirementRows.Single().Coverage, Is.EqualTo("Not covered"));
        }

        [Test]
        public void VerifyThatAVerifiesRelationshipNestsTheVnVItemUnderItsRequirement()
        {
            var requirement = this.AddRequirement("REQ-1", false);
            var vnvItem = this.AddRequirement("VNV-1", true);
            this.AddVerifiesRelationship(vnvItem, requirement);

            var browser = this.CreateBrowser();
            var row = browser.RequirementRows.Single();

            Assert.That(row.ContainedRows, Has.Count.EqualTo(1));
            Assert.That(row.ContainedRows.OfType<VandVItemRowViewModel>().Single().ShortName, Is.EqualTo("VNV-1"));
            Assert.That(row.Coverage, Does.Contain("1 item(s)"));
        }

        [Test]
        public void VerifyThatVnVItemAttributesAreProjected()
        {
            var requirement = this.AddRequirement("REQ-1", false);
            var vnvItem = this.AddRequirement("VNV-1", true);
            this.AddSimpleParameterValue(vnvItem, "vnv_method", "Test");
            this.AddSimpleParameterValue(vnvItem, "vnv_status", "Passed");
            this.AddVerifiesRelationship(vnvItem, requirement);

            var browser = this.CreateBrowser();
            var row = browser.RequirementRows.Single().ContainedRows.OfType<VandVItemRowViewModel>().Single();

            Assert.That(row.Method, Is.EqualTo("Test"));
            Assert.That(row.Status, Is.EqualTo("Passed"));
            Assert.That(row.Owner, Is.EqualTo("SYS"));
        }

        [Test]
        public void VerifyThatCaptionIsSet()
        {
            var browser = this.CreateBrowser();

            Assert.That(browser.Caption, Does.Contain("V&V Register"));
            Assert.That(browser.TargetName, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void VerifyThatAddingARequirementAddsARootRow()
        {
            var browser = this.CreateBrowser();
            Assert.That(browser.RequirementRows, Is.Empty);

            var requirement = this.AddRequirement("REQ-NEW", false);
            this.revision.Set(this.iteration, 10);
            this.revision.Set(requirement, 10);
            this.messageBus.SendObjectChangeEvent(requirement, EventKind.Added);

            Assert.That(browser.RequirementRows.Select(x => x.ShortName), Does.Contain("REQ-NEW"));
        }

        [Test]
        public void VerifyThatCreateVandVItemCommandIsOnlyEnabledForARequirementRow()
        {
            var requirement = this.AddRequirement("REQ-1", false);
            var browser = this.CreateBrowser();

            Assert.That(((ICommand)browser.CreateVandVItemCommand).CanExecute(null), Is.False, "nothing selected");

            browser.SelectedThing = browser.RequirementRows.Single();

            Assert.That(((ICommand)browser.CreateVandVItemCommand).CanExecute(null), Is.True);
        }

        [Test]
        public void VerifyThatStatusRollsUpFromItemsToRequirementToSpecification()
        {
            var requirement = this.AddRequirement("REQ-1", false);
            var other = this.AddRequirement("REQ-2", false);

            var passedItem = this.AddRequirement("VNV-1", true);
            this.SetStatus(passedItem, "Passed");
            this.AddVerifiesRelationship(passedItem, requirement);

            var openItem = this.AddRequirement("VNV-2", true);
            this.SetStatus(openItem, "In Progress");
            this.AddVerifiesRelationship(openItem, requirement);

            var failedItem = this.AddRequirement("VNV-3", true);
            this.SetStatus(failedItem, "Failed");
            this.AddVerifiesRelationship(failedItem, other);

            var browser = this.CreateBrowser();

            var requirementRow = browser.RequirementRows.Single(x => x.ShortName == "REQ-1");
            Assert.That(requirementRow.Coverage, Is.EqualTo("2 item(s): 1 passed, 1 open"));
            Assert.That(requirementRow.IsVerified, Is.False, "one item is still open");

            var otherRow = browser.RequirementRows.Single(x => x.ShortName == "REQ-2");
            Assert.That(otherRow.Coverage, Is.EqualTo("1 item(s): 1 failed"));

            Assert.That(browser.SpecificationRows.OfType<VandVSpecificationRowViewModel>().Single().Coverage, Is.EqualTo("0/2 verified, 1 failed"));
        }

        [Test]
        public void VerifyThatARequirementCountsAsVerifiedOnlyWhenEveryItemHasClosedOut()
        {
            var requirement = this.AddRequirement("REQ-1", false);

            var waived = this.AddRequirement("VNV-1", true);
            this.SetStatus(waived, "Waived");
            this.AddVerifiesRelationship(waived, requirement);

            var browser = this.CreateBrowser();

            Assert.That(browser.RequirementRows.Single().IsVerified, Is.True, "a waived item is dispositioned, not outstanding");
            Assert.That(browser.SpecificationRows.OfType<VandVSpecificationRowViewModel>().Single().Coverage, Is.EqualTo("1/1 verified"));
        }

        [Test]
        public void VerifyThatTheRollUpFollowsAStatusChangeOnAnItem()
        {
            var requirement = this.AddRequirement("REQ-1", false);
            var item = this.AddRequirement("VNV-1", true);
            this.SetStatus(item, "Executed");
            this.AddVerifiesRelationship(item, requirement);

            var browser = this.CreateBrowser();
            var requirementRow = browser.RequirementRows.Single();

            Assert.That(requirementRow.Coverage, Is.EqualTo("1 item(s): 1 open"), "Executed is not yet a verdict");
            Assert.That(browser.SpecificationRows.OfType<VandVSpecificationRowViewModel>().Single().Coverage, Is.EqualTo("0/1 verified"));

            var statusValue = item.ParameterValue.Single(x => x.ParameterType.ShortName == "vnv_status");
            statusValue.Value = new ValueArray<string>(new[] { "Passed" });

            this.messageBus.SendObjectChangeEvent(statusValue, EventKind.Updated);

            Assert.That(requirementRow.Coverage, Is.EqualTo("1 item(s): 1 passed"), "the roll-up must follow the status write");
            Assert.That(browser.SpecificationRows.OfType<VandVSpecificationRowViewModel>().Single().Coverage, Is.EqualTo("1/1 verified"), "and so must the container roll-up");
        }

        [Test]
        public void VerifyThatTheMatrixCellNamesTheVandVItem()
        {
            var requirement = this.AddRequirement("REQ-1", false);
            var item = this.AddRequirement("VNV-1", true);
            this.SetAttribute(item, "vnv_stage", "CDR");
            this.SetAttribute(item, "vnv_method", "Inspection");
            this.SetStatus(item, "Planned");
            this.AddVerifiesRelationship(item, requirement);

            var coverage = VandVCoverageQuery.Build(this.iteration).Coverages.Single(x => x.Requirement == requirement);

            Assert.That(coverage.CellText("CDR"), Is.EqualTo("VNV-1: Inspection (Planned)"));
        }

        [Test]
        public void VerifyThatTheStageFilterNarrowsTheItemsShown()
        {
            var requirement = this.AddRequirement("REQ-1", false);

            var atCdr = this.AddRequirement("VNV-CDR", true);
            this.SetAttribute(atCdr, "vnv_stage", "CDR");
            this.AddVerifiesRelationship(atCdr, requirement);

            var atPdr = this.AddRequirement("VNV-PDR", true);
            this.SetAttribute(atPdr, "vnv_stage", "PDR");
            this.AddVerifiesRelationship(atPdr, requirement);

            var uncovered = this.AddRequirement("REQ-2", false);

            var browser = this.CreateBrowser();

            Assert.That(browser.RequirementRows.Single(x => x.ShortName == "REQ-1").ContainedRows, Has.Count.EqualTo(2), "every stage shows by default");
            Assert.That(browser.RequirementRows.Select(x => x.ShortName), Does.Contain("REQ-2"));

            browser.SelectedStage = "CDR";

            var filtered = browser.RequirementRows.Single();

            Assert.That(filtered.ShortName, Is.EqualTo("REQ-1"), "a requirement with nothing at this stage drops out");
            Assert.That(filtered.ContainedRows.OfType<VandVItemRowViewModel>().Select(x => x.ShortName), Is.EqualTo(new[] { "VNV-CDR" }));
            Assert.That(filtered.Coverage, Is.Not.EqualTo("Not covered"), "the roll-up must not invent a gap the model does not have");

            browser.SelectedStage = browser.PossibleStages.First();

            Assert.That(browser.RequirementRows.Single(x => x.ShortName == "REQ-1").ContainedRows, Has.Count.EqualTo(2), "the first entry filters nothing");
            Assert.That(browser.RequirementRows.Select(x => x.ShortName), Does.Contain("REQ-2"));
        }

        [Test]
        public void VerifyThatProcedureStepsNestUnderTheirItem()
        {
            var requirement = this.AddRequirement("REQ-1", false);
            var item = this.AddRequirement("VNV-1", true);
            this.AddVerifiesRelationship(item, requirement);

            var stepCategory = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "VnVStep" };
            var hasStepCategory = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "hasStep" };

            var step = new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "VNV-1_S01", Owner = this.domain };
            step.Category.Add(stepCategory);
            this.SetAttribute(step, "vnv_step_no", "1");
            this.SetAttribute(step, "vnv_step_action", "Power on");
            this.SetAttribute(step, "vnv_step_expected", "Green LED");
            this.specification.Requirement.Add(step);

            var link = new BinaryRelationship(Guid.NewGuid(), this.assembler.Cache, this.uri) { Source = item, Target = step };
            link.Category.Add(hasStepCategory);
            this.iteration.Relationship.Add(link);

            var browser = this.CreateBrowser();
            var itemRow = browser.RequirementRows.Single().ContainedRows.OfType<VandVItemRowViewModel>().Single();

            var stepRow = itemRow.ContainedRows.OfType<VandVStepRowViewModel>().SingleOrDefault();

            Assert.That(stepRow, Is.Not.Null, "the Register view nests the procedure under its item");
            Assert.That(stepRow.StepAction, Is.EqualTo("Power on"));
            Assert.That(stepRow.StepExpected, Is.EqualTo("Green LED"));
            Assert.That(itemRow.Procedure, Is.EqualTo("0 of 1 step(s) recorded"), "nothing has been run yet");

            Assert.That(browser.RequirementRows.Single().RollUp.Total, Is.EqualTo(1), "the roll-up counts the item, not its steps");
        }

        [Test]
        public void VerifyThatTheViewDecidesWhichColumnGroupsAreShown()
        {
            var browser = this.CreateBrowser();

            Assert.That(browser.SelectedView.Name, Is.EqualTo("Register (VCD)"));
            Assert.That(browser.SelectedView.ShowsPlanning && browser.SelectedView.ShowsExecution && browser.SelectedView.ShowsCompliance, Is.True,
                "the register view shows everything");

            var planning = browser.PossibleViews.Single(x => x.Name == "Planning");

            Assert.That(planning.ShowsPlanning, Is.True);
            Assert.That(planning.ShowsExecution || planning.ShowsCompliance || planning.ShowsProcedure, Is.False,
                "a planning view must not bury the planner in execution columns");

            var compliance = browser.PossibleViews.Single(x => x.Name == "Compliance");

            Assert.That(compliance.ShowsCompliance, Is.True);
            Assert.That(compliance.ShowsPlanning, Is.False);
        }

        [Test]
        public void VerifyThatTheExpansionStateSurvivesARefresh()
        {
            var requirement = this.AddRequirement("REQ-1", false);
            var item = this.AddRequirement("VNV-1", true);
            this.AddVerifiesRelationship(item, requirement);

            var browser = this.CreateBrowser();
            var specificationRow = browser.SpecificationRows.Single();
            var requirementRow = browser.RequirementRows.Single();

            specificationRow.IsExpanded = true;
            requirementRow.IsExpanded = true;

            this.SetStatus(item, "Passed");
            this.messageBus.SendObjectChangeEvent(item, EventKind.Updated);

            Assert.That(browser.SpecificationRows.Single().IsExpanded, Is.True, "the specification must stay open");
            Assert.That(browser.RequirementRows.Single().IsExpanded, Is.True, "and so must the requirement the user opened");
        }

        [Test]
        public void VerifyThatBothEnumValueSpellingsRollUpTheSame()
        {
            Assert.That(VandVCoverageQuery.AreSameEnumValue("Not Applicable", "Not_Applicable"), Is.True);
            Assert.That(VandVCoverageQuery.AreSameEnumValue("Failed", "failed"), Is.True, "casing must not matter either");
            Assert.That(VandVCoverageQuery.AreSameEnumValue("Passed", "In Progress"), Is.False);

            var requirement = this.AddRequirement("REQ-1", false);
            var passed = this.AddRequirement("VNV-1", true);
            this.SetStatus(passed, "Not_Applicable");
            this.AddVerifiesRelationship(passed, requirement);

            var open = this.AddRequirement("VNV-2", true);
            this.SetStatus(open, "In Progress");
            this.AddVerifiesRelationship(open, requirement);

            var browser = this.CreateBrowser();
            var rollUp = browser.RequirementRows.Single().RollUp;

            Assert.That(rollUp.Passed, Is.EqualTo(1), "the shortName spelling still counts as closed out");
            Assert.That(rollUp.Open, Is.EqualTo(1));
        }

        [Test]
        public void VerifyThatADragIsForwardedToTheRowItIsOver()
        {
            var requirement = this.AddRequirement("REQ-1", false);
            var vnvItem = this.AddRequirement("VNV-1", true);
            this.AddVerifiesRelationship(vnvItem, requirement);

            var browser = this.CreateBrowser();
            var itemRow = browser.RequirementRows.Single().ContainedRows.OfType<VandVItemRowViewModel>().Single();

            var parameter = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                ParameterType = new SimpleQuantityKind(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "m" }
            };

            var dropInfo = new Mock<IDropInfo>();
            dropInfo.SetupProperty(x => x.Effects);
            dropInfo.SetupGet(x => x.TargetItem).Returns(itemRow);
            dropInfo.SetupGet(x => x.Payload).Returns(parameter);

            browser.DragOver(dropInfo.Object);

            Assert.That(dropInfo.Object.Effects, Is.EqualTo(DragDropEffects.Copy), "the browser must delegate to the row's own IDropTarget");

            dropInfo.SetupGet(x => x.TargetItem).Returns(browser.RequirementRows.Single());
            browser.DragOver(dropInfo.Object);

            Assert.That(dropInfo.Object.Effects, Is.EqualTo(DragDropEffects.None), "a requirement row is not a coverage target");
        }

        private VandVBrowserViewModel CreateBrowser()
        {
            return new VandVBrowserViewModel(this.iteration, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, this.dialogNavigationService.Object, null);
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

        private void SetStatus(Requirement vandVItem, string status)
        {
            this.SetAttribute(vandVItem, "vnv_status", status);
        }

        private void SetAttribute(Requirement vandVItem, string parameterTypeShortName, string value)
        {
            var simpleParameterValue = new SimpleParameterValue(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                ParameterType = new TextParameterType(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = parameterTypeShortName },
                Value = new ValueArray<string>(new[] { value })
            };

            vandVItem.ParameterValue.Add(simpleParameterValue);
        }

        private void AddVerifiesRelationship(Requirement source, Requirement target)
        {
            var relationship = new BinaryRelationship(Guid.NewGuid(), this.assembler.Cache, this.uri) { Source = source, Target = target };
            relationship.Category.Add(this.verifiesCategory);
            this.iteration.Relationship.Add(relationship);
        }

        private void AddSimpleParameterValue(Requirement requirement, string parameterTypeShortName, string value)
        {
            var parameterType = new TextParameterType(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = parameterTypeShortName, Name = parameterTypeShortName };
            var simpleParameterValue = new SimpleParameterValue(Guid.NewGuid(), this.assembler.Cache, this.uri) { ParameterType = parameterType, Value = new ValueArray<string>(new[] { value }) };
            requirement.ParameterValue.Add(simpleParameterValue);
        }

        /// <summary>
        /// Helper that sets the <see cref="Thing.RevisionNumber"/> via reflection so message-bus reactions fire.
        /// </summary>
        private sealed class PropertyInfoHolder
        {
            private readonly System.Reflection.PropertyInfo revisionNumber = typeof(Thing).GetProperty("RevisionNumber");

            public void Set(Thing thing, int value)
            {
                this.revisionNumber.SetValue(thing, value);
            }
        }
    }
}
