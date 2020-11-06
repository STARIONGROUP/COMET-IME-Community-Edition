// -------------------------------------------------------------------------------------------------
// <copyright file="CategoryFilterOperatorHandler.cs" company="RHEA System S.A.">
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
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.FilterOperators
{
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm;

    using DevExpress.Data.Filtering;
    using DevExpress.Xpf.Core.FilteringUI;
    using DevExpress.Xpf.Grid;

    /// <summary>
    /// Handles the setting operators and collection of data for a <see cref="Category"/> column in a <see cref="DataViewBase"/>
    /// </summary>
    public class CategoryFilterOperatorHandler : CustomFilterOperatorHandler
    {
        /// <summary>
        /// The name of the IsMemberOfCategoryName filter operator
        /// </summary>
        private const string IsMemberOfCategoryName = "IsMemberOfCategory";

        /// <summary>
        /// The name of the IsMemberOfSuperCategory filter operator
        /// </summary>
        private const string IsMemberOfSuperCategoryName = "IsMemberOfSuperCategory";

        /// <summary>
        /// Constructor which starts the static registration of the custom filter operators
        /// </summary>
        static CategoryFilterOperatorHandler()
        {
            CreateAndRegisterCustomFunctionOperators();
        }

        /// <summary>
        /// Instanciates a new <see cref="CategoryFilterOperatorHandler"/>
        /// </summary>
        /// <param name="rowViewModels">
        /// The <see cref="IEnumerable{T}"/> of type <see cref="IRowViewModelBase{Thing}"/>
        /// that contains rows where to collect data for.
        /// </param>
        /// <param name="fieldName">
        /// The name of the property (fieldname from a column's perspective) we want to collect the data from.
        /// </param>
        public CategoryFilterOperatorHandler(
            IEnumerable<IRowViewModelBase<Thing>> rowViewModels, string fieldName)
            : base(rowViewModels, fieldName)
        {
        }

        /// <summary>
        /// Set the <see cref="FilterEditorQueryOperatorsEventArgs.Operators"/> property.
        /// </summary>
        /// <param name="filterEditorQueryOperatorsEventArgs">
        /// The <see cref="FilterEditorQueryOperatorsEventArgs"/>.
        /// </param>
        public override void SetOperators(FilterEditorQueryOperatorsEventArgs filterEditorQueryOperatorsEventArgs)
        {
            filterEditorQueryOperatorsEventArgs.Operators.Clear();

            filterEditorQueryOperatorsEventArgs.Operators.Add(
                new FilterEditorOperatorItem(IsMemberOfCategoryName) { Caption = "Member of Category" });

            filterEditorQueryOperatorsEventArgs.Operators.Add(
                new FilterEditorOperatorItem(IsMemberOfSuperCategoryName) { Caption = "Member of SuperCategory" });
        }

        /// <summary>
        /// Get the values to be used in the <see cref="FilterEditorControl"/>'s combobox
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerable{object}"/> containing all the found values
        /// </returns>
        public override IEnumerable<object> GetValues()
        {
            var categories =
                this.GetValuesFromRowViewModel<IEnumerable<Category>>()
                    .SelectMany(x => x)
                    .ToList();

            var superCategories =
                categories
                    .Distinct()
                    .SelectMany(x => x.AllSuperCategories())
                    .ToList();

            return categories
                .Union(superCategories)
                .Select(x => x.Name)
                .OrderBy(x => x)
                .ToList();
        }

        /// <summary>
        /// Creates and registers the custom functions used to filter the category column which typically is of type <see cref="IEnumerable{Category}"/>
        /// </summary>
        private static void CreateAndRegisterCustomFunctionOperators()
        {
            var isMemberOfCategoryFunction = CustomFunctionFactory.Create(
                IsMemberOfCategoryName,
                (IEnumerable<Category> categories, string categoryName) =>
                {
                    return categories?.Any(x => x.Name.ToString().Equals(categoryName)) ?? false;
                });

            CriteriaOperator.RegisterCustomFunction(isMemberOfCategoryFunction);

            var isMemberOfSuperCategoryFunction = CustomFunctionFactory.Create(
                IsMemberOfSuperCategoryName,
                (IEnumerable<Category> categories, string categoryName) =>
                {
                    return categories?.Any(x => x.Name.ToString().Equals(categoryName) || x.AllSuperCategories().Any(y => y.Name.ToString().Equals(categoryName))) ?? false;
                });

            CriteriaOperator.RegisterCustomFunction(isMemberOfSuperCategoryFunction);
        }
    }
}
