// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVItemDialogViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace CDP4VandV.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Windows.Input;

    using CDP4VandV.ViewModels;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="VandVItemDialogViewModel"/>, validation, link type and edit mode.
    /// </summary>
    [TestFixture]
    public class VandVItemDialogViewModelTestFixture
    {
        private readonly Uri uri = new Uri("http://test.com");

        private CDPMessageBus messageBus;
        private Assembler assembler;
        private Mock<ISession> session;
        private SiteReferenceDataLibrary srdl;
        private Iteration iteration;
        private RequirementsSpecification specification;
        private Requirement requirement;
        private DomainOfExpertise domain;

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.messageBus = new CDPMessageBus();
            this.assembler = new Assembler(this.uri, this.messageBus);
            this.session = new Mock<ISession>();

            this.srdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "SITERDL" };
            var mrdl = new ModelReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "MRDL", RequiredRdl = this.srdl };
            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "SYS", Name = "System" };

            var modelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            modelSetup.RequiredRdl.Add(mrdl);
            modelSetup.ActiveDomain.Add(this.domain);

            var model = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.uri) { EngineeringModelSetup = modelSetup };
            this.iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri);
            model.Iteration.Add(this.iteration);

            this.specification = new RequirementsSpecification(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "SPEC", Owner = this.domain };
            this.requirement = new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "REQ-1", Name = "A requirement", Owner = this.domain };
            this.specification.Requirement.Add(this.requirement);
            this.specification.Requirement.Add(new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "VNV_EXISTING", Name = "already taken", Owner = this.domain });
            this.iteration.RequirementsSpecification.Add(this.specification);

            this.session
                .Setup(x => x.OpenIterations)
                .Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>> { { this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, null) } });
        }

        [Test]
        public void VerifyThatANewDialogDefaultsToVerifiesAndASuggestedShortName()
        {
            var vm = new VandVItemDialogViewModel(this.requirement, this.session.Object);

            Assert.That(vm.IsEditMode, Is.False);
            Assert.That(vm.Title, Is.EqualTo("Create V&V Item"));
            Assert.That(vm.LinkType, Is.EqualTo(VandVItemDialogViewModel.VerifiesLink));
            Assert.That(vm.PossibleLinkTypes, Is.EqualTo(new[] { "verifies", "validates" }));
            Assert.That(vm.ShortName, Is.EqualTo("VNV_REQ_1_1"));
            Assert.That(vm.Owner, Is.EqualTo(this.domain));
        }

        [Test]
        public void VerifyThatASecondItemForTheSameRequirementIsNumbered()
        {
            var first = new VandVItemDialogViewModel(this.requirement, this.session.Object);
            Assert.That(first.Name, Is.EqualTo("Verify REQ-1"), "the first item is not numbered");

            this.AddCoveringItem("verifies");

            var second = new VandVItemDialogViewModel(this.requirement, this.session.Object);
            Assert.That(second.Name, Is.EqualTo("Verify REQ-1 (2)"), "a second item must not repeat the first item's name");
            Assert.That(second.ShortName, Is.Not.EqualTo(first.ShortName));
        }

        [Test]
        public void VerifyThatSwitchingToValidatesRenamesTheItem()
        {
            var vm = new VandVItemDialogViewModel(this.requirement, this.session.Object);
            Assert.That(vm.Name, Is.EqualTo("Verify REQ-1"));

            vm.LinkType = VandVItemDialogViewModel.ValidatesLink;

            Assert.That(vm.Name, Is.EqualTo("Validate REQ-1"), "the suggested name follows the link type");
        }

        [Test]
        public void VerifyThatAUserEditedNameSurvivesALinkTypeChange()
        {
            var vm = new VandVItemDialogViewModel(this.requirement, this.session.Object);
            vm.Name = "Sea trial endurance run";

            vm.LinkType = VandVItemDialogViewModel.ValidatesLink;

            Assert.That(vm.Name, Is.EqualTo("Sea trial endurance run"), "a name the user typed is never overwritten");
        }

        [Test]
        public void VerifyThatAParametricConstraintCanBeCopiedIntoTheAcceptanceCriteria()
        {
            this.AddConstraint("mass", RelationalOperatorKind.LT, "100");

            var vm = new VandVItemDialogViewModel(this.requirement, this.session.Object);

            Assert.That(vm.HasParametricConstraints, Is.True);
            Assert.That(vm.PossibleParametricConstraints, Has.Count.EqualTo(1), "a single-expression constraint offers just the whole constraint");

            var choice = vm.PossibleParametricConstraints.Single();
            Assert.That(choice.Display, Is.Not.Empty, "the picker must render text; ParametricConstraint itself has no Name");

            vm.Acceptance = "Existing text";
            vm.SelectedParametricConstraint = choice;
            ((ICommand)vm.UseParametricConstraintCommand).Execute(null);

            Assert.That(vm.Acceptance, Does.StartWith("Existing text"), "existing text is preserved, not replaced");
            Assert.That(vm.Acceptance, Does.Contain("mass"), "the expression is appended");
        }

        [Test]
        public void VerifyThatAMultiExpressionConstraintOffersEachExpressionSeparately()
        {
            var constraint = this.AddConstraint("mass", RelationalOperatorKind.LT, "100");
            constraint.Expression.Add(new RelationalExpression(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                ParameterType = new TextParameterType(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "power", Name = "power" },
                RelationalOperator = RelationalOperatorKind.LE,
                Value = new ValueArray<string>(new[] { "50" })
            });

            var vm = new VandVItemDialogViewModel(this.requirement, this.session.Object);

            Assert.That(vm.PossibleParametricConstraints, Has.Count.EqualTo(3), "the whole constraint plus one entry per expression");
            Assert.That(vm.PossibleParametricConstraints.Count(x => x.Expression != null), Is.EqualTo(2));
            Assert.That(
                vm.PossibleParametricConstraints.Any(x => x.Expression != null && x.ExpressionText.Contains("power")),
                Is.True,
                "each expression can be verified on its own");
        }

        [Test]
        public void VerifyThatUsingAConstraintBoundToAParameterAlsoSetsTheCoverage()
        {
            var constraint = this.AddConstraint("mass", RelationalOperatorKind.LT, "100");
            var expression = constraint.Expression.OfType<RelationalExpression>().Single();

            var elementDefinition = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "ED1", Name = "Element", Owner = this.domain };
            var parameter = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.uri) { Owner = this.domain };
            elementDefinition.Parameter.Add(parameter);
            this.iteration.Element.Add(elementDefinition);

            var relationship = new BinaryRelationship(Guid.NewGuid(), this.assembler.Cache, this.uri) { Source = parameter, Target = expression };
            this.iteration.Relationship.Add(relationship);
            this.assembler.Cache.TryAdd(new CacheKey(relationship.Iid, this.iteration.Iid), new Lazy<Thing>(() => relationship));

            var vm = new VandVItemDialogViewModel(this.requirement, this.session.Object);
            vm.SelectedParametricConstraint = vm.PossibleParametricConstraints.First();
            ((ICommand)vm.UseParametricConstraintCommand).Execute(null);

            Assert.That(vm.SelectedParameter, Is.EqualTo(parameter), "the parameter the constraint binds to is coupled automatically");
            Assert.That(vm.SelectedElementDefinition, Is.EqualTo(elementDefinition), "and so is its element definition");
        }

        [Test]
        public void VerifyThatOptionAndStatePickersAppearOnlyForADependentParameter()
        {
            var elementDefinition = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "ED1", Name = "Element", Owner = this.domain };

            var plain = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.uri) { Owner = this.domain };

            var stateList = new ActualFiniteStateList(Guid.NewGuid(), this.assembler.Cache, this.uri);
            stateList.ActualState.Add(new ActualFiniteState(Guid.NewGuid(), this.assembler.Cache, this.uri));
            this.iteration.ActualFiniteStateList.Add(stateList);

            var dependent = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Owner = this.domain,
                IsOptionDependent = true,
                StateDependence = stateList
            };

            elementDefinition.Parameter.Add(plain);
            elementDefinition.Parameter.Add(dependent);
            this.iteration.Element.Add(elementDefinition);
            this.iteration.Option.Add(new Option(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "OPT", Name = "Option 1" });

            var vm = new VandVItemDialogViewModel(this.requirement, this.session.Object);

            vm.SelectedElementDefinition = elementDefinition;
            vm.SelectedParameter = plain;

            Assert.That(vm.IsParameterOptionDependent, Is.False);
            Assert.That(vm.IsParameterStateDependent, Is.False);
            Assert.That(vm.PossibleOptions, Is.Empty);
            Assert.That(vm.PossibleStates, Is.Empty);

            vm.SelectedParameter = dependent;

            Assert.That(vm.IsParameterOptionDependent, Is.True);
            Assert.That(vm.IsParameterStateDependent, Is.True);
            Assert.That(vm.PossibleOptions, Has.Count.EqualTo(1));
            Assert.That(vm.PossibleStates, Has.Count.EqualTo(1));

            vm.PossibleOptions.Single().IsSelected = true;
            vm.PossibleStates.Single().IsSelected = true;

            Assert.That(vm.SelectedOptions, Has.Count.EqualTo(1), "ticking a row selects the option");
            Assert.That(vm.SelectedStates, Has.Count.EqualTo(1));

            vm.SelectedParameter = plain;

            Assert.That(vm.SelectedOptions, Is.Empty, "a selection that no longer applies is dropped");
        }

        [Test]
        public void VerifyThatThereAreNoParametricConstraintsToOfferWhenTheRequirementHasNone()
        {
            var vm = new VandVItemDialogViewModel(this.requirement, this.session.Object);

            Assert.That(vm.HasParametricConstraints, Is.False);
            Assert.That(((ICommand)vm.UseParametricConstraintCommand).CanExecute(null), Is.False);
        }

        [Test]
        public void VerifyThatTheRequirementCaptionDoesNotUseAnEmDash()
        {
            var vm = new VandVItemDialogViewModel(this.requirement, this.session.Object);

            // no dash character outside plain ASCII may reach the UI
            Assert.That(vm.RequirementCaption.Any(c => c > 0x7F), Is.False, "the caption must stay plain ASCII");
            Assert.That(vm.RequirementCaption, Is.EqualTo("REQ-1: A requirement"));
        }

        [Test]
        public void VerifyThatMandatoryFieldsAreReportedAndBlockTheOkCommand()
        {
            var vm = new VandVItemDialogViewModel(this.requirement, this.session.Object);

            Assert.That(vm["Method"], Is.Not.Empty, "method is mandatory and not yet set");
            Assert.That(vm["Stage"], Is.Not.Empty);
            Assert.That(vm["Acceptance"], Is.Not.Empty);
            Assert.That(((ICommand)vm.OkCommand).CanExecute(null), Is.False);

            vm.Method = "Test";
            vm.Stage = "FAT";
            vm.Acceptance = "Endurance >= 24h";

            Assert.That(vm["Method"], Is.Empty);
            Assert.That(((ICommand)vm.OkCommand).CanExecute(null), Is.True);
        }

        [Test]
        public void VerifyThatAnInvalidOrDuplicateShortNameIsRejected()
        {
            var vm = new VandVItemDialogViewModel(this.requirement, this.session.Object)
            {
                Method = "Test", Stage = "FAT", Acceptance = "x"
            };

            vm.ShortName = string.Empty;
            Assert.That(vm["ShortName"], Does.Contain("mandatory"));

            vm.ShortName = "bad name!";
            Assert.That(vm["ShortName"], Does.Contain("only letters, digits and underscores"));

            vm.ShortName = "10R_VNV";
            Assert.That(vm["ShortName"], Is.Empty, "a leading digit is legal, the model itself allows short names like 10R");

            vm.ShortName = "VNV_EXISTING";
            Assert.That(vm["ShortName"], Does.Contain("already uses this short name"));
            Assert.That(((ICommand)vm.OkCommand).CanExecute(null), Is.False);
        }

        [Test]
        public void VerifyThatEditModeLoadsTheExistingItem()
        {
            var item = new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "VNV_REQ_1_1", Name = "Verify endurance", Owner = this.domain };
            this.AddAttribute(item, "vnv_method", "Analysis");
            this.AddAttribute(item, "vnv_stage", "PDR");
            this.AddAttribute(item, "vnv_acceptance", "Margin > 10%");
            this.AddAttribute(item, "vnv_status", "Passed");
            this.AddAttribute(item, "vnv_planned_date", "2026-03-01");
            this.specification.Requirement.Add(item);

            var vm = new VandVItemDialogViewModel(this.requirement, this.session.Object, item, VandVItemDialogViewModel.ValidatesLink);

            Assert.That(vm.IsEditMode, Is.True);
            Assert.That(vm.Title, Is.EqualTo("Edit V&V Item"));
            Assert.That(vm.OkButtonCaption, Is.EqualTo("OK"));
            Assert.That(vm.LinkType, Is.EqualTo("validates"));
            Assert.That(vm.Method, Is.EqualTo("Analysis"));
            Assert.That(vm.Status, Is.EqualTo("Passed"));
            Assert.That(vm.PlannedDate, Is.EqualTo(new DateTime(2026, 3, 1)));
            Assert.That(vm["ShortName"], Is.Empty, "its own short name is not a duplicate of itself");
            Assert.That(((ICommand)vm.OkCommand).CanExecute(null), Is.True);
        }

        [Test]
        public void VerifyThatEditModeKeepsClearedFieldsSoTheyCanBeRemoved()
        {
            var item = new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "VNV_REQ_1_1", Name = "n", Owner = this.domain };
            this.AddAttribute(item, "vnv_method", "Analysis");
            this.AddAttribute(item, "vnv_stage", "PDR");
            this.AddAttribute(item, "vnv_acceptance", "a");
            this.AddAttribute(item, "vnv_facility", "Lab A");
            this.specification.Requirement.Add(item);

            var vm = new VandVItemDialogViewModel(this.requirement, this.session.Object, item, "verifies");
            vm.Facility = string.Empty;

            var attributes = vm.BuildAttributes();

            Assert.That(attributes.ContainsKey("vnv_facility"), Is.True, "an emptied field must survive as empty so the update deletes it");
            Assert.That(attributes["vnv_facility"], Is.Empty);
        }

        [Test]
        public void VerifyThatCreateModeOmitsEmptyAttributes()
        {
            var vm = new VandVItemDialogViewModel(this.requirement, this.session.Object)
            {
                Method = "Test", Stage = "FAT", Acceptance = "x"
            };

            var attributes = vm.BuildAttributes();

            Assert.That(attributes.ContainsKey("vnv_facility"), Is.False, "blank fields are not written on create");
            Assert.That(attributes["vnv_method"], Is.EqualTo("Test"));
        }

        [Test]
        public void VerifyThatPickListsComeFromTheRdlWhenPresent()
        {
            var stageType = new EnumerationParameterType(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "vnv_stage", Name = "stage" };
            stageType.ValueDefinition.Add(new EnumerationValueDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "GATE-A", ShortName = "GATE_A" });
            this.srdl.ParameterType.Add(stageType);

            var vm = new VandVItemDialogViewModel(this.requirement, this.session.Object);

            Assert.That(vm.PossibleStages, Is.EqualTo(new[] { "GATE-A" }));
            Assert.That(vm.PossibleMethods, Does.Contain("Test"), "methods still fall back to the manifest");
        }

        private ParametricConstraint AddConstraint(string parameterTypeShortName, RelationalOperatorKind relationalOperator, string value)
        {
            var constraint = new ParametricConstraint(Guid.NewGuid(), this.assembler.Cache, this.uri);

            constraint.Expression.Add(new RelationalExpression(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                ParameterType = new TextParameterType(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = parameterTypeShortName, Name = parameterTypeShortName },
                RelationalOperator = relationalOperator,
                Value = new ValueArray<string>(new[] { value })
            });

            this.requirement.ParametricConstraint.Add(constraint);
            return constraint;
        }

        private void AddCoveringItem(string linkCategoryShortName)
        {
            var category = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = linkCategoryShortName, Name = linkCategoryShortName };
            category.PermissibleClass.Add(ClassKind.BinaryRelationship);

            var item = new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "VNV_REQ_1_1", Name = "Verify REQ-1", Owner = this.domain };
            this.specification.Requirement.Add(item);

            var relationship = new BinaryRelationship(Guid.NewGuid(), this.assembler.Cache, this.uri) { Source = item, Target = this.requirement };
            relationship.Category.Add(category);
            this.iteration.Relationship.Add(relationship);
        }

        private void AddAttribute(Requirement item, string shortName, string value)
        {
            var parameterType = new TextParameterType(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = shortName, Name = shortName };
            item.ParameterValue.Add(new SimpleParameterValue(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                ParameterType = parameterType,
                Value = new ValueArray<string>(new[] { value })
            });
        }
    }
}
