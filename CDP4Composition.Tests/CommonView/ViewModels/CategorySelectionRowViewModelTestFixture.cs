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

namespace CDP4Composition.Tests.CommonView.ViewModels
{
    using System;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4CommonView.ViewModels;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="CategorySelectionRowViewModel"/> that backs the category selection grid
    /// (GitHub issues #72 and #1043).
    /// </summary>
    [TestFixture]
    public class CategorySelectionRowViewModelTestFixture
    {
        private SiteReferenceDataLibrary siteReferenceDataLibrary;

        private Category superCategory;

        private Category category;

        private Category deprecatedCategory;

        [SetUp]
        public void SetUp()
        {
            this.siteReferenceDataLibrary = new SiteReferenceDataLibrary { ShortName = "GENERIC" };

            this.superCategory = new Category { Name = "Product", ShortName = "PROD" };
            this.siteReferenceDataLibrary.DefinedCategory.Add(this.superCategory);

            this.category = new Category { Name = "Element Definition Category", ShortName = "ED_CAT" };
            this.category.SuperCategory.Add(this.superCategory);
            this.category.Definition.Add(new Definition { LanguageCode = "en-GB", Content = "the definition" });
            this.siteReferenceDataLibrary.DefinedCategory.Add(this.category);

            this.deprecatedCategory = new Category { Name = "Deprecated Category", ShortName = "DEP_CAT", IsDeprecated = true };
            this.siteReferenceDataLibrary.DefinedCategory.Add(this.deprecatedCategory);
        }

        [Test]
        public void VerifyThatPropertiesAreSetWhenRowViewModelIsConstructed()
        {
            var row = new CategorySelectionRowViewModel(this.category);

            Assert.That(row.Category, Is.EqualTo(this.category));
            Assert.That(row.Name, Is.EqualTo("Element Definition Category"));
            Assert.That(row.ShortName, Is.EqualTo("ED_CAT"));
            Assert.That(row.SuperCategory, Is.EqualTo("PROD"));
            Assert.That(row.ContainerRdl, Is.EqualTo("GENERIC"));
            Assert.That(row.Definition, Is.EqualTo("the definition"));
            Assert.That(row.IsDeprecated, Is.False);
        }

        [Test]
        public void VerifyThatSuperCategoryAndDefinitionAreEmptyWhenNotSet()
        {
            var row = new CategorySelectionRowViewModel(this.superCategory);

            Assert.That(row.SuperCategory, Is.Empty);
            Assert.That(row.Definition, Is.Empty);
        }

        [Test]
        public void VerifyThatConstructorThrowsOnNullCategory()
        {
            Assert.Throws<ArgumentNullException>(() => new CategorySelectionRowViewModel(null));
        }

        [Test]
        public void VerifyThatNonDeprecatedCategoryIsSelectable()
        {
            var row = new CategorySelectionRowViewModel(this.category);

            Assert.That(row.IsSelectable, Is.True);
        }

        [Test]
        public void VerifyThatUnassignedDeprecatedCategoryIsNotSelectable()
        {
            var row = new CategorySelectionRowViewModel(this.deprecatedCategory) { IsSelected = false };

            Assert.That(row.IsSelectable, Is.False);
        }

        [Test]
        public void VerifyThatAssignedDeprecatedCategoryIsSelectableSoItCanBeDeselected()
        {
            var row = new CategorySelectionRowViewModel(this.deprecatedCategory) { IsSelected = true };

            Assert.That(row.IsSelectable, Is.True);
        }

        [Test]
        public void VerifyThatReadOnlyRowIsNotSelectable()
        {
            var row = new CategorySelectionRowViewModel(this.category) { IsReadOnly = true };

            Assert.That(row.IsSelectable, Is.False);
        }

        [Test]
        public void VerifyThatChangingSelectionRaisesIsSelectableNotification()
        {
            var row = new CategorySelectionRowViewModel(this.deprecatedCategory);
            var raised = false;
            row.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(CategorySelectionRowViewModel.IsSelectable))
                {
                    raised = true;
                }
            };

            row.IsSelected = true;

            Assert.That(raised, Is.True);
        }

        [Test]
        public void VerifyThatRowsAreOrderedBySelectionThenName()
        {
            var alpha = new CategorySelectionRowViewModel(new Category { Name = "Alpha", ShortName = "A" }) { IsSelected = false };
            var zeta = new CategorySelectionRowViewModel(new Category { Name = "Zeta", ShortName = "Z" }) { IsSelected = true };
            var beta = new CategorySelectionRowViewModel(new Category { Name = "Beta", ShortName = "B" }) { IsSelected = false };
            var gamma = new CategorySelectionRowViewModel(new Category { Name = "Gamma", ShortName = "G" }) { IsSelected = true };

            var ordered = CategorySelectionRowViewModel.OrderBySelectionThenName(new[] { alpha, zeta, beta, gamma }).ToList();

            Assert.That(ordered, Is.EqualTo(new[] { gamma, zeta, alpha, beta }));
        }
    }
}
