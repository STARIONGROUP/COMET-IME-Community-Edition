// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementThingSelector.xaml.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Cozmin Velciu, Adrian Chivu
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

namespace CDP4CrossViewEditor.Views.UserControls
{
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.SiteDirectoryData;

    using CDP4CrossViewEditor.ViewModels;

    using DevExpress.Data.Filtering;
    using DevExpress.Xpf.Core.FilteringUI;
    using DevExpress.Xpf.Data;
    using DevExpress.Xpf.Grid;

    using CustomUniqueValuesEventArgs = DevExpress.Xpf.Grid.CustomUniqueValuesEventArgs;

    /// <summary>
    /// Interaction logic for ElementThingSelector.xaml
    /// </summary>
    public partial class ElementThingSelector : ThingUserControl
    {
        /// <summary>
        /// The name of the IsMemberOfCategoryName filter operator
        /// </summary>
        private const string IsMemberOfCategoryName = "IsMemberOfCategory";

        /// <summary>
        /// The name of the IsMemberOfSuperCategory filter operator
        /// </summary>
        private const string HasCategoryApplied = "HasCategoryApplied";

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementThingSelector"/> class.
        /// </summary>
        public ElementThingSelector()
        {
            this.InitializeComponent();
            CreateAndRegisterCustomFunctionOperators();
        }

        /// <summary>
        /// Customize filter editor operation group
        /// </summary>
        /// <param name="sender">Associated control <see cref="FilterEditorControl"/></param>
        /// <param name="e">Associated event <see cref="QueryGroupOperationsEventArgs"/></param>
        private void OnQueryGroupOperations(object sender, QueryGroupOperationsEventArgs e)
        {
            e.AllowAddGroup = false;
            e.AllowAddCustomExpression = false;
        }

        /// <summary>
        ///Customize filter editor operators
        /// </summary>
        /// <param name="sender">Associated control <see cref="FilterEditorControl"/></param>
        /// <param name="e">Associated event <see cref="FilterEditorQueryOperatorsEventArgs"/></param>
        private void OnQueryOperators(object sender, FilterEditorQueryOperatorsEventArgs e)
        {
            if (e.FieldName != "Categories")
            {
                return;
            }

            e.Operators.Clear();

            e.Operators.Add(
                new FilterEditorOperatorItem(IsMemberOfCategoryName) { Caption = "Member of Category" });

            e.Operators.Add(
                new FilterEditorOperatorItem(HasCategoryApplied) { Caption = "Has Category Applied" });
        }

        /// <summary>
        /// Customize categories unique values
        /// </summary>
        /// <param name="sender">Associated control <see cref="GridControl"/></param>
        /// <param name="e">Assoicated event <see cref="CustomUniqueValuesEventArgs"/></param>
        private void GridControl_OnCustomUniqueValues(object sender, CustomUniqueValuesEventArgs e)
        {
            if (e.Column.FieldName != "Categories")
            {
                return;
            }

            if (!(this.DataContext is ElementDefinitionSelectorViewModel viewModel))
            {
                return;
            }

            e.UniqueValues = viewModel.Categories.Select(x => x.Name).Distinct().ToArray();

            e.UniqueValuesAndCounts = e.UniqueValues.GroupBy(x => x)
                .Select(x => new ValueAndCount(x.Key, x.Count()))
                .ToArray();

            e.Handled = true;
        }

        /// <summary>
        /// Creates and registers the custom functions used to filter the category column
        /// </summary>
        private static void CreateAndRegisterCustomFunctionOperators()
        {
            var isMemberOfCategoryFunction = CustomFunctionFactory.Create(
                IsMemberOfCategoryName,
                (IEnumerable<Category> categories, string categoryName) =>
                {
                    return categories?.Any(x => x.Name.ToString().Equals(categoryName) || x.AllSuperCategories().Any(y => y.Name.ToString().Equals(categoryName))) ?? false;
                });

            CriteriaOperator.RegisterCustomFunction(isMemberOfCategoryFunction);

            var hasCategoryAppliedFunction = CustomFunctionFactory.Create(
                HasCategoryApplied,
                (IEnumerable<Category> categories, string categoryName) =>
                {
                    return categories?.Any(x => x.Name.ToString().Equals(categoryName)) ?? false;
                });

            CriteriaOperator.RegisterCustomFunction(hasCategoryAppliedFunction);
        }
    }
}
