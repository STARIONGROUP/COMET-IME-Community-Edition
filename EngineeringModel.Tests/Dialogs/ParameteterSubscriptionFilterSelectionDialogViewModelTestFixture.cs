// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameteterSubscriptionFilterSelectionDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Tests.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using CDP4Common.SiteDirectoryData;

    using CDP4EngineeringModel.ViewModels.Dialogs;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="ParameteterSubscriptionFilterSelectionDialogViewModel"/> class
    /// </summary>
    [TestFixture]
    public class ParameteterSubscriptionFilterSelectionDialogViewModelTestFixture
    {
        private List<Category> categories;
        private List<DomainOfExpertise> domains;
        private List<ParameterType> parameterTypes;

        [SetUp]
        public void SetUp()
        {
            var equipments = new Category { ShortName = "EQT", Name = "Equipments" };
            var batteries = new Category { ShortName = "BAT", Name = "Batteries" };
            this.categories = new List<Category> {equipments, batteries};

            var system = new DomainOfExpertise { ShortName = "SYS", Name = "System Engineering" };
            var poweer = new DomainOfExpertise { ShortName = "PWR", Name = "Power Engineering" };
            this.domains = new List<DomainOfExpertise> {system, poweer};

            var mass = new SimpleQuantityKind {Name = "mass", ShortName = "m"};
            var lenght = new SimpleQuantityKind { Name = "length", ShortName = "l" };
            this.parameterTypes = new List<ParameterType> { mass, lenght };
        }

        [Test]
        public void Verify_that_constructor_does_expected_setup()
        {
            var vm = new ParameteterSubscriptionFilterSelectionDialogViewModel(this.parameterTypes, this.categories, this.domains);

            CollectionAssert.AreEquivalent(this.categories, vm.PossibleCategories);
            CollectionAssert.AreEquivalent(this.domains, vm.PossibleOwner);
            CollectionAssert.AreEquivalent(this.parameterTypes, vm.PossibleParameterTypes);

            Assert.That(vm.OkCommand.CanExecute(null), Is.False);
            Assert.That(vm.CancelCommand.CanExecute(null), Is.True);
        }

        [Test]
        public void Verify_that_when_a_selection_is_made_canok_can_execute()
        {
            var vm = new ParameteterSubscriptionFilterSelectionDialogViewModel(this.parameterTypes, this.categories, this.domains);

            Assert.That(vm.OkCommand.CanExecute(null), Is.False);

            vm.SelectedCategories.Add(this.categories.First());
            vm.SelectedOwners.Add(this.domains.First());
            vm.SelectedParameterTypes.Add(this.parameterTypes.First());

            Assert.That(vm.OkCommand.CanExecute(null), Is.True);
        }

        [Test]
        public void Verify_that_when_OkCommand_is_executed_the_Result_is_as_expected()
        {
            var vm = new ParameteterSubscriptionFilterSelectionDialogViewModel(this.parameterTypes, this.categories, this.domains);

            vm.SelectedCategories.Add(this.categories.First());
            vm.SelectedOwners.Add(this.domains.First());
            vm.SelectedParameterTypes.Add(this.parameterTypes.First());

            vm.OkCommand.Execute(null);
            var dialogResult = vm.DialogResult;

            Assert.That(dialogResult.Result.HasValue, Is.True);

            var subscriptionFilterSelectionResult = dialogResult as ParameteterSubscriptionFilterSelectionResult;

            CollectionAssert.AreEquivalent(vm.SelectedCategories, subscriptionFilterSelectionResult.Categories);
            CollectionAssert.AreEquivalent(vm.SelectedOwners, subscriptionFilterSelectionResult.DomainOfExpertises);
            CollectionAssert.AreEquivalent(vm.SelectedParameterTypes, subscriptionFilterSelectionResult.ParameterTypes);
        }

        [Test]
        public void Verify_that_when_CancelCommand_is_executed_the_Result_is_as_expected()
        {
            var vm = new ParameteterSubscriptionFilterSelectionDialogViewModel(this.parameterTypes, this.categories, this.domains);

            vm.SelectedCategories.Add(this.categories.First());
            vm.SelectedOwners.Add(this.domains.First());
            vm.SelectedParameterTypes.Add(this.parameterTypes.First());

            vm.CancelCommand.Execute(null);
            var dialogResult = vm.DialogResult;

            Assert.That(dialogResult.Result.HasValue, Is.True);

            Assert.That(dialogResult.Result, Is.False);
        }
    }
}
