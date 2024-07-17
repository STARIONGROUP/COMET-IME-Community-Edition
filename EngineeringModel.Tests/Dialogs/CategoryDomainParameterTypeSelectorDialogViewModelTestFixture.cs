// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameteterSubscriptionFilterSelectionDialogViewModelTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2022 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Tests.Dialogs
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;

    using CDP4Common.SiteDirectoryData;

    using CDP4EngineeringModel.ViewModels.Dialogs;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="CategoryDomainParameterTypeSelectorDialogViewModel"/> class
    /// </summary>
    [TestFixture]
    public class CategoryDomainParameterTypeSelectorDialogViewModelTestFixture
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
            var vm = new CategoryDomainParameterTypeSelectorDialogViewModel(this.parameterTypes, this.categories, this.domains);

            CollectionAssert.AreEquivalent(this.categories, vm.PossibleCategories);
            CollectionAssert.AreEquivalent(this.domains, vm.PossibleOwner);
            CollectionAssert.AreEquivalent(this.parameterTypes, vm.PossibleParameterTypes);

            Assert.That(((ICommand)vm.OkCommand).CanExecute(null), Is.False);
            Assert.That(((ICommand)vm.CancelCommand).CanExecute(null), Is.True);
        }

        [Test]
        public void Verify_that_when_a_selection_is_made_canok_can_execute()
        {
            var vm = new CategoryDomainParameterTypeSelectorDialogViewModel(this.parameterTypes, this.categories, this.domains);

            Assert.That(((ICommand)vm.OkCommand).CanExecute(null), Is.False);

            vm.SelectedCategories.Add(this.categories.First());
            vm.SelectedOwners.Add(this.domains.First());
            vm.SelectedParameterTypes.Add(this.parameterTypes.First());

            Assert.That(((ICommand)vm.OkCommand).CanExecute(null), Is.True);
        }

        [Test]
        public async Task Verify_that_when_OkCommand_is_executed_the_Result_is_as_expected()
        {
            var vm = new CategoryDomainParameterTypeSelectorDialogViewModel(this.parameterTypes, this.categories, this.domains);

            vm.SelectedCategories.Add(this.categories.First());
            vm.SelectedOwners.Add(this.domains.First());
            vm.SelectedParameterTypes.Add(this.parameterTypes.First());

            await vm.OkCommand.Execute();
            var dialogResult = vm.DialogResult;

            Assert.That(dialogResult.Result.HasValue, Is.True);

            var subscriptionFilterSelectionResult = dialogResult as CategoryDomainParameterTypeSelectorResult;

            CollectionAssert.AreEquivalent(vm.SelectedCategories, subscriptionFilterSelectionResult.Categories);
            CollectionAssert.AreEquivalent(vm.SelectedOwners, subscriptionFilterSelectionResult.DomainOfExpertises);
            CollectionAssert.AreEquivalent(vm.SelectedParameterTypes, subscriptionFilterSelectionResult.ParameterTypes);
        }

        [Test]
        public async Task Verify_that_when_CancelCommand_is_executed_the_Result_is_as_expected()
        {
            var vm = new CategoryDomainParameterTypeSelectorDialogViewModel(this.parameterTypes, this.categories, this.domains);

            vm.SelectedCategories.Add(this.categories.First());
            vm.SelectedOwners.Add(this.domains.First());
            vm.SelectedParameterTypes.Add(this.parameterTypes.First());

            await vm.CancelCommand.Execute();
            var dialogResult = vm.DialogResult;

            Assert.That(dialogResult.Result.HasValue, Is.True);

            Assert.That(dialogResult.Result, Is.False);
        }
    }
}
