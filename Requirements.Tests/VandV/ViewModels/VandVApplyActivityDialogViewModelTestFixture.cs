// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVApplyActivityDialogViewModelTestFixture.cs" company="Starion Group S.A.">
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
    using CDP4Common.Types;

    using CDP4Dal;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="VandVApplyActivityDialogViewModel"/>, prefill, guards and close-out gating.
    /// </summary>
    [TestFixture]
    public class VandVApplyActivityDialogViewModelTestFixture
    {
        private readonly Uri uri = new Uri("http://test.com");

        private CDPMessageBus messageBus;
        private Assembler assembler;
        private Mock<ISession> session;
        private Iteration iteration;
        private RequirementsSpecification vandVSpecification;
        private Requirement activity;
        private DomainOfExpertise domain;
        private Category vnvItemCategory;
        private Category performedByCategory;

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

            this.vandVSpecification = new RequirementsSpecification(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "VNV", Owner = this.domain };
            this.iteration.RequirementsSpecification.Add(this.vandVSpecification);

            this.vnvItemCategory = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = VandVCategory.VnVItem };
            this.performedByCategory = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = VandVCategory.PerformedBy };

            var activityCategory = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = VandVCategory.VnVActivity };
            this.activity = new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "ACT_1", Name = "Produce mass budget", Owner = this.domain };
            this.activity.Category.Add(activityCategory);
            this.vandVSpecification.Requirement.Add(this.activity);

            this.SetAttribute(this.activity, VandVParameter.Status, "Passed");
            this.SetAttribute(this.activity, VandVParameter.Result, "mass budget issue 3");
            this.SetAttribute(this.activity, VandVParameter.EvidenceReference, "TR-101");
        }

        [Test]
        public void VerifyThatTheDialogPrefillsFromTheActivity()
        {
            this.AddPerformedItem("VNV-1");

            var vm = new VandVApplyActivityDialogViewModel(this.activity, this.iteration, this.session.Object);

            Assert.That(vm.Status, Is.EqualTo("Passed"));
            Assert.That(vm.Result, Is.EqualTo("mass budget issue 3"));
            Assert.That(vm.EvidenceReference, Is.EqualTo("TR-101"));
            Assert.That(vm.Compliance, Is.EqualTo("Compliant"), "closing out defaults to compliant, subject to the human confirming");
            Assert.That(vm.CloseOutReason, Does.Contain("ACT_1"));
        }

        [Test]
        public void VerifyThatGuardedItemsStartOutUnticked()
        {
            var clean = this.AddPerformedItem("VNV-1");
            var closed = this.AddPerformedItem("VNV-2");
            this.SetAttribute(closed, VandVParameter.Closed, "true");

            var vm = new VandVApplyActivityDialogViewModel(this.activity, this.iteration, this.session.Object);

            Assert.That(vm.Items, Has.Count.EqualTo(2));
            Assert.That(vm.Items.Single(x => x.Thing == clean).IsSelected, Is.True);

            var closedRow = vm.Items.Single(x => x.Thing == closed);
            Assert.That(closedRow.IsSelected, Is.False, "a closed item needs a human look, not a bulk gesture");
            Assert.That(closedRow.Display, Does.Contain("already closed"));

            Assert.That(vm.SelectedItems, Is.EqualTo(new[] { clean }));
        }

        [Test]
        public void VerifyThatBuildAttributesIncludesTheCloseOutOnlyWhenRequested()
        {
            this.AddPerformedItem("VNV-1");

            var vm = new VandVApplyActivityDialogViewModel(this.activity, this.iteration, this.session.Object);

            Assert.That(vm.BuildAttributes().ContainsKey(VandVParameter.Closed), Is.False, "closing out is a judgement, not a side effect");

            vm.AlsoCloseOut = true;

            var attributes = vm.BuildAttributes();
            Assert.That(attributes[VandVParameter.Closed], Is.EqualTo("true"));
            Assert.That(attributes[VandVParameter.Compliance], Is.EqualTo("Compliant"));
            Assert.That(attributes[VandVParameter.CloseOutReason], Is.Not.Empty);
        }

        [Test]
        public void VerifyThatClosingOutRequiresAReason()
        {
            this.AddPerformedItem("VNV-1");

            var vm = new VandVApplyActivityDialogViewModel(this.activity, this.iteration, this.session.Object);

            Assert.That(((ICommand)vm.OkCommand).CanExecute(null), Is.True, "applying the execution record needs no reason");

            vm.AlsoCloseOut = true;
            vm.CloseOutReason = "  ";

            Assert.That(((ICommand)vm.OkCommand).CanExecute(null), Is.False, "a close-out without a stated reason cannot be accepted");

            vm.CloseOutReason = "verified by the mass budget";

            Assert.That(((ICommand)vm.OkCommand).CanExecute(null), Is.True);
        }

        private Requirement AddPerformedItem(string shortName)
        {
            var item = new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = shortName, Name = shortName, Owner = this.domain };
            item.Category.Add(this.vnvItemCategory);
            this.vandVSpecification.Requirement.Add(item);

            var link = new BinaryRelationship(Guid.NewGuid(), this.assembler.Cache, this.uri) { Source = item, Target = this.activity };
            link.Category.Add(this.performedByCategory);
            this.iteration.Relationship.Add(link);

            return item;
        }

        private void SetAttribute(Requirement requirement, string parameterTypeShortName, string value)
        {
            var simpleParameterValue = new SimpleParameterValue(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                ParameterType = new TextParameterType(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = parameterTypeShortName },
                Value = new ValueArray<string>(new[] { value })
            };

            requirement.ParameterValue.Add(simpleParameterValue);
        }
    }
}
