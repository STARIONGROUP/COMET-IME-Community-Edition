// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CustomFilterEditorDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2024 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4Composition.Tests.FilterOperators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Composition;
    using CDP4Composition.FilterOperators;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Services.FilterEditorService;
    using CDP4Composition.ViewModels;

    using CDP4Dal;

    using CommonServiceLocator;

    using DevExpress.Data.Filtering;
    using DevExpress.Xpf.Core.FilteringUI;
    using DevExpress.Xpf.Grid;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="CustomFilterEditorDialogViewModel"/> class
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class CustomFilterEditorDialogViewModelTestFixture
    {
        private const string CategoryName1 = "Category1";
        private const string CategoryName2 = "Category2";

        private CategoryTestRowViewModel parentRow;
        private CategoryTestRowViewModel childRow;
        private Category category1;
        private Category category2;
        private Mock<IServiceLocator> serviceLocator;
        private Mock<ISession> session;
        private FilterEditorQueryOperatorsEventArgs filterEditorQueryOperatorsEventArgs;
        private FilterEditorOperatorItemList filterEditorOperatorItemList;
        private Mock<IDialogNavigationService> dialogNavigationService;
        private Mock<ISavedUserPreferenceService> savedUserPreferenceService;
        private Mock<IHaveCustomFilterOperators> customFilterOperatorsViewModel;
        private DataViewBase dataViewBase;

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            RxApp.DefaultExceptionHandler = new RxAppObservableExceptionHandler();
            this.serviceLocator = new Mock<IServiceLocator>();

            var treeList = new TreeListControl { View = new TreeListView() };
            this.dataViewBase = treeList.View;
            this.dataViewBase.Name = "DataViewBase";

            this.dialogNavigationService = new Mock<IDialogNavigationService>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IDialogNavigationService>()).Returns(this.dialogNavigationService.Object);

            this.savedUserPreferenceService = new Mock<ISavedUserPreferenceService>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<ISavedUserPreferenceService>()).Returns(this.savedUserPreferenceService.Object);

            this.session = new Mock<ISession>();
            this.session.Setup(x => x.CDPMessageBus).Returns(new CDPMessageBus());

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

            this.customFilterOperatorsViewModel = new Mock<IHaveCustomFilterOperators>();

            var customFilterOperators = new Dictionary<DataViewBase, Dictionary<string, (CustomFilterOperatorType, IEnumerable<IRowViewModelBase<Thing>>)>>();
            var browserDictionary = new Dictionary<string, (CustomFilterOperatorType, IEnumerable<IRowViewModelBase<Thing>>)>();
            browserDictionary.Add(nameof(CategoryTestRowViewModel.Category), (CustomFilterOperatorType.Category, new[] { this.parentRow }));

            customFilterOperators.Add(this.dataViewBase, browserDictionary);

            this.customFilterOperatorsViewModel.Setup(x => x.CustomFilterOperators).Returns(customFilterOperators);

            this.dataViewBase.DataContext = this.customFilterOperatorsViewModel.Object;
        }

        [Test]
        public async Task VerifyThatQueryOperatorsCommandWorks()
        {
            var vm = new CustomFilterEditorDialogViewModel(this.dialogNavigationService.Object, this.dataViewBase);

            Assert.CatchAsync<NotSupportedException>(async () => await vm.QueryOperatorsCommand.Execute(default));

            await vm.QueryOperatorsCommand.Execute(this.filterEditorQueryOperatorsEventArgs);

            Assert.AreEqual(2, this.filterEditorQueryOperatorsEventArgs.Operators.Count);

            Assert.AreEqual(1,
                this.filterEditorOperatorItemList.Count(
                    x => x.CustomFunctionName == CategoryFilterOperatorHandler.IsMemberOfCategoryName));

            Assert.AreEqual(1,
                this.filterEditorOperatorItemList.Count(
                    x => x.CustomFunctionName == CategoryFilterOperatorHandler.HasCategoryApplied));
        }

        private T CreateInstance<T>(params object[] args)
        {
            var type = typeof(T);

            var instance = type.Assembly.CreateInstance(
                type.FullName, false,
                BindingFlags.Instance | BindingFlags.NonPublic,
                null, args, null, null);

            return (T)instance;
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
