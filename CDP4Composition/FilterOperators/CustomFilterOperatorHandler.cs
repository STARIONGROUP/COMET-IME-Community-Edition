// -------------------------------------------------------------------------------------------------
// <copyright file="CustomFilterOperatorHandler.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2020 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smieckowski
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

    using CDP4Common.CommonData;

    using CDP4Composition.Mvvm;

    using DevExpress.Xpf.Core.FilteringUI;

    /// <summary>
    /// Base class for all custom filter operator handlers
    /// </summary>
    public abstract class CustomFilterOperatorHandler : ICustomFilterOperatorHandler
    {
        /// <summary>
        /// The <see cref="IEnumerable{T}"/> of type <see cref="IRowViewModelBase{Thing}"/> that contains the rows to be searched.
        /// </summary>
        private readonly IEnumerable<IRowViewModelBase<Thing>> rowViewModels;

        /// <summary>
        /// The propertyname (FieldName from a GridView column's perspective) for which we want to collect values.
        /// </summary>
        public string FieldName { get;}

        /// <summary>
        /// Instanciates a new <see cref="CustomFilterOperatorHandler"/>
        /// </summary>
        /// <param name="rowViewModels">
        /// The <see cref="IEnumerable{T}"/> of type <see cref="IRowViewModelBase{Thing}"/> that contains the rows to be searched.
        /// </param>
        /// <param name="fieldName">
        /// The propertyname (FieldName from a GridView column's perspective) for which we want to collect values.
        /// </param>
        protected CustomFilterOperatorHandler(IEnumerable<IRowViewModelBase<Thing>> rowViewModels, string fieldName)
        {
            this.rowViewModels = rowViewModels;
            this.FieldName = fieldName;
        }

        /// <summary>
        /// Changes the <see cref="FilterEditorQueryOperatorsEventArgs.Operators"/> list of filter operators
        /// </summary>
        /// <param name="filterEditorQueryOperatorsEventArgs">
        /// The <see cref="FilterEditorQueryOperatorsEventArgs"/>
        /// </param>
        public abstract void SetOperators(FilterEditorQueryOperatorsEventArgs filterEditorQueryOperatorsEventArgs);

        /// <summary>
        /// Gets the values needed for the inherited <see cref="CustomFilterOperatorHandler"/>
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerable{Object}"/> containing the needed values
        /// </returns>
        public abstract IEnumerable<object> GetValues();

        /// <summary>
        /// Gets values from <see cref="rowViewModels"/>
        /// </summary>
        /// <typeparam name="T">
        /// The type of object the <see cref="List{T}"/> holds data for.
        /// </typeparam>
        /// <returns>
        /// A <see cref="List{T}"/> containing all found values.
        /// </returns>
        protected List<T> GetValuesFromRowViewModel<T>()
        {
            var values = new List<T>();
            this.TryAddValuesFromRowViewModels(this.rowViewModels, ref values);

            return values;
        }

        /// <summary>
        /// Tries to add values to a <see cref="List{T}"/> from a <see cref="IRowViewModelBase{Thing}"/>
        /// and its childrows
        /// </summary>
        /// <typeparam name="T">
        /// The type of object the <see cref="List{T}"/> holds data for.
        /// </typeparam>
        /// <param name="rowViewModel">
        /// The <see cref="IRowViewModelBase{Thing}"/> to be searched.
        /// </param>
        /// <param name="values">
        /// The <see cref="List{T}"/> containing all the found values.
        /// </param>
        private void TryAddValuesFromRowViewModel<T>(IRowViewModelBase<Thing> rowViewModel, ref List<T> values)
        {
            var propInfo = rowViewModel.GetType().GetProperty(this.FieldName);

            if (propInfo != null)
            {
                var value = propInfo.GetValue(rowViewModel);

                if (value is T typedValue)
                {
                    values.Add(typedValue);
                }
            }

            this.TryAddValuesFromRowViewModels(rowViewModel.ContainedRows, ref values);
        }

        /// <summary>
        /// Tries to add values to a <see cref="List{T}"/> from an <see cref="IEnumerable{T}"/> of type <see cref="IRowViewModelBase{Thing}"/>
        /// </summary>
        /// <typeparam name="T">
        /// The type of object the <see cref="List{T}"/> holds data for.
        /// </typeparam>
        /// <param name="rowViewModelsToBeSearched">
        /// The <see cref="IEnumerable{T}"/> of type <see cref="IRowViewModelBase{Thing}"/> that contains the rows to be searched.
        /// </param>
        /// <param name="values">
        /// The <see cref="List{T}"/> containing all the found values.
        /// </param>
        private void TryAddValuesFromRowViewModels<T>(IEnumerable<IRowViewModelBase<Thing>> rowViewModelsToBeSearched, ref List<T> values)
        {
            foreach (var rowViewModel in rowViewModelsToBeSearched)
            {
                this.TryAddValuesFromRowViewModel(rowViewModel, ref values);
            }
        }
    }
}
