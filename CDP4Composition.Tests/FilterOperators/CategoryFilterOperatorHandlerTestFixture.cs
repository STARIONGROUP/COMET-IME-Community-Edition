// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CategoryFilterOperatorHandlerTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smieckowski
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

namespace CDP4Composition.Tests.FilterOperators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.FilterOperators;
    using CDP4Composition.Mvvm;

    using CDP4Dal;

    using DevExpress.Data.Filtering;
    using DevExpress.Xpf.Core.FilteringUI;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="CategoryFilterOperatorHandler"/> class
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class CategoryFilterOperatorHandlerTestFixture
    {
        private const string CategoryName1 = "Category1";
        private const string CategoryName2 = "Category2";

        private CategoryFilterOperatorHandler categoryFilterOperatorHandler;
        private CategoryTestRowViewModel parentRow;
        private CategoryTestRowViewModel childRow;
        private Category category1;
        private Category category2;
        private Mock<ISession> session;
        private FilterEditorQueryOperatorsEventArgs filterEditorQueryOperatorsEventArgs;
        private FilterEditorOperatorItemList filterEditorOperatorItemList;

        [SetUp]
        public void SetUp()
        {
            this.session = new Mock<ISession>();

            this.category1 = new Category(Guid.NewGuid(), null, null)
            {
                Name = CategoryName1
            };

            this.category2 = new Category(Guid.NewGuid(), null, null)
            {
                Name = CategoryName2
            };

            this.parentRow = new CategoryTestRowViewModel(this.category1, this.session.Object, null);
            this.childRow = new CategoryTestRowViewModel(this.category2, this.session.Object, this.parentRow);

            this.parentRow.ContainedRows.Add(this.childRow);

            this.filterEditorQueryOperatorsEventArgs = 
                this.CreateInstance<FilterEditorQueryOperatorsEventArgs>(CriteriaOperator.And(), nameof(CategoryTestRowViewModel.Category));

            var filterEditorOperatorItem = new List<FilterEditorOperatorItem>
            {
                new FilterEditorOperatorItem(FilterEditorOperatorType.AboveAverage),
                new FilterEditorOperatorItem(FilterEditorOperatorType.Equal),
                new FilterEditorOperatorItem(FilterEditorOperatorType.Greater),
                new FilterEditorOperatorItem(FilterEditorOperatorType.AnyOf)
            };

            this.filterEditorOperatorItemList = this.CreateInstance<FilterEditorOperatorItemList>(filterEditorOperatorItem);
            this.filterEditorQueryOperatorsEventArgs.Operators = this.filterEditorOperatorItemList;
        }

        [Test]
        public void VerifyThatCorrectCategoriesAreReturnedForParentRow()
        {
            var rowViewModels = new[] { this.parentRow };

            this.categoryFilterOperatorHandler = new CategoryFilterOperatorHandler(rowViewModels, nameof(CategoryTestRowViewModel.Category));
            var values = this.categoryFilterOperatorHandler.GetValues().ToList();

            Assert.AreEqual(2, values.Count);

            var expected = new[] { CategoryName1, CategoryName2 };

            CollectionAssert.AreEquivalent(expected, values);
        }

        [Test]
        public void VerifyThatCorrectCategoriesAreReturnedForChildRow()
        {
            var rowViewModels = new[] { this.childRow };

            this.categoryFilterOperatorHandler = new CategoryFilterOperatorHandler(rowViewModels, nameof(CategoryTestRowViewModel.Category));

            var values = this.categoryFilterOperatorHandler.GetValues().ToList();

            Assert.AreEqual(1, values.Count);

            var expected = new[] { CategoryName2 };

            CollectionAssert.AreEquivalent(expected, values);
        }

        [Test]
        public void VerifyThatSetOperatorsWorks()
        {
            var rowViewModels = new[] { this.parentRow };
            this.categoryFilterOperatorHandler = new CategoryFilterOperatorHandler(rowViewModels, nameof(CategoryTestRowViewModel.Category));
            this.categoryFilterOperatorHandler.SetOperators(this.filterEditorQueryOperatorsEventArgs);

            Assert.AreEqual(2, this.filterEditorOperatorItemList.Count);

            Assert.AreEqual(1, 
                this.filterEditorOperatorItemList.Count(
                    x => x.CustomFunctionName== CategoryFilterOperatorHandler.IsMemberOfCategoryName));

            Assert.AreEqual(1, 
                this.filterEditorOperatorItemList.Count(
                    x => x.CustomFunctionName== CategoryFilterOperatorHandler.HasCategoryApplied));
        }

        private T CreateInstance<T>(params object[] args)
        {
            var type = typeof (T);

            var instance = type.Assembly.CreateInstance(
                type.FullName, false,
                BindingFlags.Instance | BindingFlags.NonPublic,
                null, args, null, null);

            return (T) instance;
        }

        private class CategoryTestRowViewModel : RowViewModelBase<Category>
        {
            public IEnumerable<Category> Category => new Category[] { this.Thing };

            public CategoryTestRowViewModel(Category thing, ISession session, IViewModelBase<Thing> containerViewModel) : base(thing, session, containerViewModel)
            {
            }
        }
    }
}
