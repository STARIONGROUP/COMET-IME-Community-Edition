// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVLinkItemsDialogViewModelTestFixture.cs" company="Starion Group S.A.">
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

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="VandVLinkItemsDialogViewModel"/>, the bulk linking of existing V&amp;V items
    /// to a shared activity.
    /// </summary>
    [TestFixture]
    public class VandVLinkItemsDialogViewModelTestFixture
    {
        private readonly Uri uri = new Uri("http://test.com");

        private CDPMessageBus messageBus;
        private Assembler assembler;
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

            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "SYS", Name = "System" };

            var srdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "SITERDL" };
            var mrdl = new ModelReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "MRDL", RequiredRdl = srdl };

            var modelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            modelSetup.RequiredRdl.Add(mrdl);
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

            this.SetAttribute(this.activity, VandVParameter.Method, "Analysis");
            this.SetAttribute(this.activity, VandVParameter.Stage, "CDR");
        }

        [Test]
        public void VerifyThatTheDialogListsTheItemsTheActivityDoesNotPerform()
        {
            var requirement = this.AddRequirement("REQ-1");
            var free = this.AddItem("VNV-1", requirement, null);
            var performed = this.AddItem("VNV-2", requirement, this.activity);

            var vm = new VandVLinkItemsDialogViewModel(this.activity, this.iteration);

            Assert.That(vm.Items.Select(x => x.Item), Is.EqualTo(new[] { free }), "an item this activity already performs is not offered");
            Assert.That(vm.Items.Single().Display, Does.Not.Contain("moves from"), "an unlinked item is not described as moving");
            Assert.That(performed.ShortName, Is.EqualTo("VNV-2"));
        }

        [Test]
        public void VerifyThatAnItemStatingItsOwnPlanningIsFlaggedButStillLinkable()
        {
            var requirement = this.AddRequirement("REQ-1");

            var differing = this.AddItem("VNV-1", requirement, null);
            this.SetAttribute(differing, VandVParameter.Method, "Test");

            var following = this.AddItem("VNV-2", requirement, null);

            var vm = new VandVLinkItemsDialogViewModel(this.activity, this.iteration);

            var differingRow = vm.Items.Single(x => x.Item == differing);
            Assert.That(differingRow.Display, Does.Contain("states its own Test"), "a method of its own that differs from the activity's is flagged, since linking keeps it");

            var followingRow = vm.Items.Single(x => x.Item == following);
            Assert.That(followingRow.Display, Does.Not.Contain("states its own"));

            Assert.That(vm.ClearOwnPlanning, Is.False, "nothing is overwritten unless asked");
        }

        [Test]
        public void VerifyThatOkRequiresATickedItem()
        {
            var requirement = this.AddRequirement("REQ-1");
            this.AddItem("VNV-1", requirement, null);

            var vm = new VandVLinkItemsDialogViewModel(this.activity, this.iteration);

            Assert.That(((ICommand)vm.OkCommand).CanExecute(null), Is.False);

            ((ICommand)vm.SelectAllCommand).Execute(null);

            Assert.That(vm.SelectedItems, Has.Count.EqualTo(1));
            Assert.That(((ICommand)vm.OkCommand).CanExecute(null), Is.True);

            ((ICommand)vm.ClearSelectionCommand).Execute(null);

            Assert.That(((ICommand)vm.OkCommand).CanExecute(null), Is.False);
        }

        [Test]
        public void VerifyThatADialogWithNothingToOfferIsConstructedWithoutThrowing()
        {
            Assert.DoesNotThrow(() =>
            {
                var vm = new VandVLinkItemsDialogViewModel(this.activity, this.iteration);
                Assert.That(vm.Items, Is.Empty);
            });
        }

        private Requirement AddRequirement(string shortName)
        {
            var requirement = new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = shortName, Name = shortName, Owner = this.domain };
            this.specification.Requirement.Add(requirement);

            return requirement;
        }

        private Requirement AddItem(string shortName, Requirement covered, Requirement performedBy)
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
