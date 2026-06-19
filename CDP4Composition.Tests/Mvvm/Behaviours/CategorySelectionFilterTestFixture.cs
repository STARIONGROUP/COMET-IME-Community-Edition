// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CategorySelectionGrid.xaml.cs" company="Starion Group S.A.">
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

namespace CDP4Composition.Tests.Mvvm.Behaviours
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm.Behaviours;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="CategorySelectionFilter"/> class that encodes the deprecation visibility rule
    /// used when assigning <see cref="Category"/>s to a categorizable thing (GitHub issue #1071).
    /// </summary>
    [TestFixture]
    public class CategorySelectionFilterTestFixture
    {
        private Category nonDeprecatedCategory;

        private Category deprecatedCategory;

        private Category anotherDeprecatedCategory;

        [SetUp]
        public void SetUp()
        {
            this.nonDeprecatedCategory = new Category(Guid.NewGuid(), null, null) { ShortName = "ND", Name = "NonDeprecated", IsDeprecated = false };
            this.deprecatedCategory = new Category(Guid.NewGuid(), null, null) { ShortName = "D1", Name = "Deprecated1", IsDeprecated = true };
            this.anotherDeprecatedCategory = new Category(Guid.NewGuid(), null, null) { ShortName = "D2", Name = "Deprecated2", IsDeprecated = true };
        }

        [Test]
        public void VerifyThatNonDeprecatedCategoryIsAlwaysVisible()
        {
            Assert.That(CategorySelectionFilter.IsVisible(this.nonDeprecatedCategory, false, new List<Category>()), Is.True);
            Assert.That(CategorySelectionFilter.IsVisible(this.nonDeprecatedCategory, true, new List<Category>()), Is.True);
        }

        [Test]
        public void VerifyThatDeprecatedCategoryIsVisibleWhenShowDeprecatedThingsIsOn()
        {
            Assert.That(CategorySelectionFilter.IsVisible(this.deprecatedCategory, true, new List<Category>()), Is.True);
        }

        [Test]
        public void VerifyThatUnassignedDeprecatedCategoryIsHiddenWhenShowDeprecatedThingsIsOff()
        {
            Assert.That(CategorySelectionFilter.IsVisible(this.deprecatedCategory, false, new List<Category>()), Is.False);
        }

        [Test]
        public void VerifyThatAssignedDeprecatedCategoryIsVisibleWhenShowDeprecatedThingsIsOff()
        {
            var selected = new List<Category> { this.deprecatedCategory };

            Assert.That(CategorySelectionFilter.IsVisible(this.deprecatedCategory, false, selected), Is.True);
        }

        [Test]
        public void VerifyThatNullCategoryIsNotVisible()
        {
            Assert.That(CategorySelectionFilter.IsVisible(null, true, new List<Category>()), Is.False);
        }

        [Test]
        public void VerifyThatNullSelectionIsHandled()
        {
            Assert.That(CategorySelectionFilter.IsVisible(this.deprecatedCategory, false, null), Is.False);
            Assert.That(CategorySelectionFilter.IsVisible(this.nonDeprecatedCategory, false, null), Is.True);
        }

        [Test]
        public void VerifyThatSelectAllSelectsAllNonDeprecatedCategories()
        {
            var visible = new List<Category> { this.nonDeprecatedCategory, this.deprecatedCategory };
            var current = new List<Category>();

            var result = CategorySelectionFilter.GetSelectAllSelection(visible, current);

            Assert.That(result, Is.EquivalentTo(new[] { this.nonDeprecatedCategory }));
        }

        [Test]
        public void VerifyThatSelectAllNeverSelectsDeprecatedCategories()
        {
            // even when a deprecated category is visible, Select All must not select it
            var visible = new List<Category> { this.nonDeprecatedCategory, this.deprecatedCategory, this.anotherDeprecatedCategory };
            var current = new List<Category>();

            var result = CategorySelectionFilter.GetSelectAllSelection(visible, current);

            Assert.That(result.Any(c => c.IsDeprecated), Is.False);
            Assert.That(result, Is.EquivalentTo(new[] { this.nonDeprecatedCategory }));
        }

        [Test]
        public void VerifyThatSelectAllPreservesAlreadyAssignedDeprecatedCategory()
        {
            // an already-assigned deprecated category stays selected through Select All, but is not newly selected
            var visible = new List<Category> { this.nonDeprecatedCategory, this.deprecatedCategory };
            var current = new List<Category> { this.deprecatedCategory };

            var result = CategorySelectionFilter.GetSelectAllSelection(visible, current);

            Assert.That(result, Is.EquivalentTo(new[] { this.nonDeprecatedCategory, this.deprecatedCategory }));
        }

        [Test]
        public void VerifyThatDeselectAllKeepsAssignedDeprecatedCategories()
        {
            var current = new List<Category> { this.nonDeprecatedCategory, this.deprecatedCategory };

            var result = CategorySelectionFilter.GetDeselectAllSelection(current);

            Assert.That(result, Is.EquivalentTo(new[] { this.deprecatedCategory }));
        }

        [Test]
        public void VerifyThatAreAllSelectableCategoriesSelectedReflectsSelection()
        {
            var visible = new List<Category> { this.nonDeprecatedCategory, this.deprecatedCategory };

            Assert.That(CategorySelectionFilter.AreAllSelectableCategoriesSelected(visible, new List<Category>()), Is.False);
            Assert.That(CategorySelectionFilter.AreAllSelectableCategoriesSelected(visible, new List<Category> { this.nonDeprecatedCategory }), Is.True);
        }

        [Test]
        public void VerifyThatAreAllSelectableCategoriesSelectedIsFalseWhenNoneSelectable()
        {
            var visible = new List<Category> { this.deprecatedCategory };

            Assert.That(CategorySelectionFilter.AreAllSelectableCategoriesSelected(visible, new List<Category>()), Is.False);
        }

        [Test]
        public void VerifyThatSelectAllHelpersHandleNullInput()
        {
            Assert.That(CategorySelectionFilter.GetSelectAllSelection(null, null), Is.Empty);
            Assert.That(CategorySelectionFilter.GetDeselectAllSelection(null), Is.Empty);
            Assert.That(CategorySelectionFilter.AreAllSelectableCategoriesSelected(null, null), Is.False);
        }
    }
}
