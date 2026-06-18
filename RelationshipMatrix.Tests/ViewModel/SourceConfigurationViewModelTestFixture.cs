// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SourceConfigurationViewModelTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2020 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Kamil Wojnowski, Nathanael Smiechowski
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//    Lesser General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4RelationshipMatrix.Tests.ViewModel
{
    using System;
    using System.Collections.Generic;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal.Events;

    using CDP4RelationshipMatrix.ViewModels;

    using NUnit.Framework;

    using CDP4RelationshipMatrix.Settings;

    /// <summary>
    /// Suite of tests for the <see cref="SourceConfigurationViewModelTestFixture"/>
    /// </summary>
    [TestFixture]
    public class SourceConfigurationViewModelTestFixture : ViewModelTestBase
    {
        private RelationshipMatrixViewModel source;
        private List<Category> selectedCategoryList;
        private string categoryStringResult1;
        private string categoryStringResult2;
        private string categoryStringResult3;
        private string categoryStringResult4;


        [SetUp]
        public override void Setup()
        {
            base.Setup();

            this.source = new RelationshipMatrixViewModel(
                this.iteration,
                this.session.Object,
                this.thingDialogNavigationService.Object,
                this.panelNavigationService.Object,
                this.dialogNavigationService.Object,
                this.pluginService.Object);

            this.selectedCategoryList = new List<Category>() { this.catEd1 , this.catEd3 };
            this.categoryStringResult1 = $"{this.catEd1.Name} OR {this.catEd3.Name}";
            this.categoryStringResult2 = $"({this.catEd1.Name} OR {this.catEd3.Name} OR {this.catEd4.Name}) OR ({this.catEd3.Name})";
            this.categoryStringResult3 = $"{this.catEd1.Name} AND {this.catEd3.Name}";
            this.categoryStringResult4 = $"({this.catEd1.Name} OR {this.catEd3.Name} OR {this.catEd4.Name}) AND ({this.catEd3.Name})";
        }

    [Test]
        public void AssertViewModelWorks()
        {
            this.source.SourceXConfiguration.SelectedCategories = this.selectedCategoryList;
            this.source.SourceYConfiguration.SelectedCategories = this.selectedCategoryList;

            this.source.SourceYConfiguration.SelectedBooleanOperatorKind = CategoryBooleanOperatorKind.OR;
            this.source.SourceYConfiguration.IncludeSubcategories = false;
            Assert.AreEqual(this.categoryStringResult1, this.source.SourceYConfiguration.CategoriesString);

            this.source.SourceYConfiguration.IncludeSubcategories = true;
            Assert.AreEqual(this.categoryStringResult2, this.source.SourceYConfiguration.CategoriesString);

            this.source.SourceYConfiguration.SelectedBooleanOperatorKind = CategoryBooleanOperatorKind.AND;
            this.source.SourceYConfiguration.IncludeSubcategories = false;
            Assert.AreEqual(this.categoryStringResult3, this.source.SourceYConfiguration.CategoriesString);

            this.source.SourceYConfiguration.IncludeSubcategories = true;
            Assert.AreEqual(this.categoryStringResult4, this.source.SourceYConfiguration.CategoriesString);

            this.source.Dispose();
        }

        [Test]
        public void VerifyThatOnlyCategorizableClassKindsAreAvailableInTheSelectors()
        {
            this.settings.PossibleClassKinds.Add(ClassKind.Requirement);
            this.settings.PossibleClassKinds.Add(ClassKind.Parameter);
            this.settings.PossibleClassKinds.Add(ClassKind.NestedElement);
            this.settings.PossibleClassKinds.Add(ClassKind.ParametricConstraint);

            var viewModel = new RelationshipMatrixViewModel(
                this.iteration,
                this.session.Object,
                this.thingDialogNavigationService.Object,
                this.panelNavigationService.Object,
                this.dialogNavigationService.Object,
                this.pluginService.Object);

            var possibleClassKinds = viewModel.SourceXConfiguration.PossibleClassKinds;

            // categorizable types remain available
            Assert.That(possibleClassKinds, Does.Contain(ClassKind.ElementDefinition));
            Assert.That(possibleClassKinds, Does.Contain(ClassKind.ElementUsage));
            Assert.That(possibleClassKinds, Does.Contain(ClassKind.Requirement));

            // non-categorizable types are excluded
            Assert.That(possibleClassKinds, Does.Not.Contain(ClassKind.Parameter));
            Assert.That(possibleClassKinds, Does.Not.Contain(ClassKind.NestedElement));
            Assert.That(possibleClassKinds, Does.Not.Contain(ClassKind.ParametricConstraint));

            viewModel.Dispose();
        }

        [Test]
        public void VerifyThatOwnersDefaultToAllWhenClassKindIsSelected()
        {
            var configuration = this.source.SourceXConfiguration;

            configuration.SelectedClassKind = ClassKind.ElementDefinition;

            Assert.That(configuration.PossibleOwners, Is.Not.Empty);
            Assert.That(configuration.SelectedOwners, Is.EquivalentTo(configuration.PossibleOwners));

            this.source.Dispose();
        }

        [Test]
        public void VerifyThatAddedActiveDomainIsSelectedWhenAllOwnersAreSelected()
        {
            var configuration = this.source.SourceXConfiguration;

            configuration.SelectedClassKind = ClassKind.ElementDefinition;

            var addedDomain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "domain2", ShortName = "domain2" };
            this.sitedir.Domain.Add(addedDomain);
            this.engineeringModelSetup.ActiveDomain.Add(addedDomain);
            this.assembler.Cache.TryAdd(new CacheKey(addedDomain.Iid, null), new Lazy<Thing>(() => addedDomain));

            this.rev.SetValue(this.engineeringModelSetup, 10);
            this.messageBus.SendObjectChangeEvent(this.engineeringModelSetup, EventKind.Updated);

            Assert.That(configuration.PossibleOwners, Does.Contain(addedDomain));
            Assert.That(configuration.SelectedOwners, Does.Contain(addedDomain));

            this.source.Dispose();
        }

        [Test]
        public void VerifyThatAddedActiveDomainIsNotSelectedWhenOwnerSelectionIsPartial()
        {
            var configuration = this.source.SourceXConfiguration;

            configuration.SelectedClassKind = ClassKind.ElementDefinition;

            // narrow the selection so that not all owners are selected
            configuration.SelectedOwners = new List<DomainOfExpertise>();

            var addedDomain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "domain2", ShortName = "domain2" };
            this.sitedir.Domain.Add(addedDomain);
            this.engineeringModelSetup.ActiveDomain.Add(addedDomain);
            this.assembler.Cache.TryAdd(new CacheKey(addedDomain.Iid, null), new Lazy<Thing>(() => addedDomain));

            this.rev.SetValue(this.engineeringModelSetup, 10);
            this.messageBus.SendObjectChangeEvent(this.engineeringModelSetup, EventKind.Updated);

            Assert.That(configuration.PossibleOwners, Does.Contain(addedDomain));
            Assert.That(configuration.SelectedOwners, Is.Empty);

            this.source.Dispose();
        }
    }
}
