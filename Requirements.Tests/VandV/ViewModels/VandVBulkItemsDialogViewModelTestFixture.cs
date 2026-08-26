// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVBulkItemsDialogViewModelTestFixture.cs" company="Starion Group S.A.">
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
    using System.Windows.Input;

    using CDP4Requirements.Rdl;
    using CDP4Requirements.ViewModels;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="VandVBulkItemsDialogViewModel"/>, the requirement offer and the OK gating.
    /// </summary>
    [TestFixture]
    public class VandVBulkItemsDialogViewModelTestFixture
    {
        private readonly Uri uri = new Uri("http://test.com");

        private CDPMessageBus messageBus;
        private Assembler assembler;
        private Mock<ISession> session;
        private Iteration iteration;
        private RequirementsSpecification specification;
        private RequirementsSpecification vandVSpecification;
        private Requirement activity;
        private DomainOfExpertise domain;
        private Category vnvItemCategory;
        private Category performedByCategory;
        private Category verifiesCategory;

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.messageBus = new CDPMessageBus();
            this.assembler = new Assembler(this.uri, this.messageBus);
            this.session = new Mock<ISession>();

            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "SYS", Name = "System" };

            var modelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            modelSetup.ActiveDomain.Add(this.domain);

            var model = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.uri) { EngineeringModelSetup = modelSetup };
            this.iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri);
            model.Iteration.Add(this.iteration);

            this.specification = new RequirementsSpecification(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "SPEC", Owner = this.domain };
            this.iteration.RequirementsSpecification.Add(this.specification);

            this.vandVSpecification = new RequirementsSpecification(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "VNV", Owner = this.domain };
            this.iteration.RequirementsSpecification.Add(this.vandVSpecification);

            this.vnvItemCategory = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = VandVCategory.VnVItem };
            this.performedByCategory = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = VandVCategory.PerformedBy };
            this.verifiesCategory = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = VandVCategory.Verifies };

            var activityCategory = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = VandVCategory.VnVActivity };
            this.activity = new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "ACT_1", Name = "Produce mass budget", Owner = this.domain };
            this.activity.Category.Add(activityCategory);
            this.vandVSpecification.Requirement.Add(this.activity);

            this.session
                .Setup(x => x.OpenIterations)
                .Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>> { { this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, null) } });
        }

        [Test]
        public void VerifyThatRequirementsAlreadyPerformedByTheActivityAreNotOffered()
        {
            var performed = this.AddRequirement("REQ-1");
            var covered = this.AddRequirement("REQ-2");
            this.AddRequirement("REQ-3");

            this.AddItem("VNV-1", performed, this.activity);
            this.AddItem("VNV-2", covered, null);

            var vm = new VandVBulkItemsDialogViewModel(this.activity, this.iteration, this.session.Object);

            Assert.That(vm.PossibleRequirements.Select(x => ((Requirement)x.Thing).ShortName), Is.EqualTo(new[] { "REQ-2", "REQ-3" }));
            Assert.That(vm.PossibleRequirements.First().Display, Does.Contain("already covered by 1 item(s)"), "existing coverage is stated, not hidden");
        }

        [Test]
        public void VerifyThatOkRequiresATickedRequirement()
        {
            this.AddRequirement("REQ-1");
            this.AddRequirement("REQ-2");

            var vm = new VandVBulkItemsDialogViewModel(this.activity, this.iteration, this.session.Object);

            Assert.That(vm.LinkType, Is.EqualTo(VandVItemDialogViewModel.VerifiesLink));
            Assert.That(vm.Owner, Is.EqualTo(this.domain));
            Assert.That(((ICommand)vm.OkCommand).CanExecute(null), Is.False, "nothing is ticked yet");

            vm.AcceptanceCriteria = "as per the mass budget";

            ((ICommand)vm.SelectAllCommand).Execute(null);

            Assert.That(vm.SelectedRequirements, Has.Count.EqualTo(2));
            Assert.That(((ICommand)vm.OkCommand).CanExecute(null), Is.True);

            ((ICommand)vm.ClearSelectionCommand).Execute(null);

            Assert.That(vm.SelectedRequirements, Is.Empty);
            Assert.That(((ICommand)vm.OkCommand).CanExecute(null), Is.False);
        }

        [Test]
        public void VerifyThatItemsCannotBeCreatedWithoutAcceptanceCriteria()
        {
            this.AddRequirement("REQ-1");

            var vm = new VandVBulkItemsDialogViewModel(this.activity, this.iteration, this.session.Object);

            ((ICommand)vm.SelectAllCommand).Execute(null);

            Assert.That(((ICommand)vm.OkCommand).CanExecute(null), Is.False, "a V&V item with no acceptance criteria saved cleanly and then read as incomplete");
            Assert.That(vm.Validation, Does.Contain("acceptance criteria"));

            vm.AcceptanceCriteria = "as per the mass budget";

            Assert.That(((ICommand)vm.OkCommand).CanExecute(null), Is.True);
            Assert.That(vm.Validation, Is.Empty);
        }

        [Test]
        public void VerifyThatARequirementOverridesTheDefaultAcceptanceCriteria()
        {
            var first = this.AddRequirement("REQ-1");
            this.AddRequirement("REQ-2");

            var vm = new VandVBulkItemsDialogViewModel(this.activity, this.iteration, this.session.Object);
            vm.AcceptanceCriteria = "the default";

            ((ICommand)vm.SelectAllCommand).Execute(null);

            vm.PossibleRequirements.Single(row => row.Requirement == first).AcceptanceCriteria = "its own";

            Assert.That(vm.AcceptanceCriteriaByRequirement[first.Iid], Is.EqualTo("its own"));
            Assert.That(vm.AcceptanceCriteriaByRequirement.Values, Does.Contain("the default"), "a requirement stating nothing of its own falls back to the default");
        }

        [Test]
        public void VerifyThatAParametricConstraintIsOfferedButNotAppliedUntilAsked()
        {
            var requirement = this.AddRequirement("REQ-1");

            var constraint = new ParametricConstraint(Guid.NewGuid(), this.assembler.Cache, this.uri);

            var expression = new NotExpression(Guid.NewGuid(), this.assembler.Cache, this.uri);
            constraint.Expression.Add(expression);
            constraint.TopExpression = expression;
            requirement.ParametricConstraint.Add(constraint);

            var vm = new VandVBulkItemsDialogViewModel(this.activity, this.iteration, this.session.Object);
            var row = vm.PossibleRequirements.Single();

            Assert.That(row.HasParametricConstraints, Is.True);
            Assert.That(row.AcceptanceCriteria, Is.Null, "the constraint is offered, never written on the user's behalf");

            ((ICommand)row.UseParametricConstraintCommand).Execute(null);

            Assert.That(row.AcceptanceCriteria, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void VerifyThatARequirementWithoutAConstraintOffersNoPicker()
        {
            this.AddRequirement("REQ-1");

            var vm = new VandVBulkItemsDialogViewModel(this.activity, this.iteration, this.session.Object);

            Assert.That(vm.PossibleRequirements.Single().HasParametricConstraints, Is.False);
        }

        [Test]
        public void VerifyThatFillBlanksWithDefaultLeavesOwnCriteriaAlone()
        {
            var first = this.AddRequirement("REQ-1");
            this.AddRequirement("REQ-2");

            var vm = new VandVBulkItemsDialogViewModel(this.activity, this.iteration, this.session.Object);
            vm.AcceptanceCriteria = "the default";

            ((ICommand)vm.SelectAllCommand).Execute(null);

            vm.PossibleRequirements.Single(row => row.Requirement == first).AcceptanceCriteria = "its own";

            ((ICommand)vm.ApplyDefaultCommand).Execute(null);

            Assert.That(vm.PossibleRequirements.Single(row => row.Requirement == first).AcceptanceCriteria, Is.EqualTo("its own"));
            Assert.That(vm.PossibleRequirements.Select(row => row.AcceptanceCriteria), Does.Contain("the default"));
        }

        private Requirement AddRequirement(string shortName)
        {
            var requirement = new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = shortName, Name = shortName, Owner = this.domain };
            this.specification.Requirement.Add(requirement);

            return requirement;
        }

        private void AddItem(string shortName, Requirement covered, Requirement performedBy)
        {
            var item = new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = shortName, Name = shortName, Owner = this.domain };
            item.Category.Add(this.vnvItemCategory);
            this.vandVSpecification.Requirement.Add(item);

            var verifies = new BinaryRelationship(Guid.NewGuid(), this.assembler.Cache, this.uri) { Source = item, Target = covered };
            verifies.Category.Add(this.verifiesCategory);
            this.iteration.Relationship.Add(verifies);

            if (performedBy != null)
            {
                var link = new BinaryRelationship(Guid.NewGuid(), this.assembler.Cache, this.uri) { Source = item, Target = performedBy };
                link.Category.Add(this.performedByCategory);
                this.iteration.Relationship.Add(link);
            }
        }
    }
}
