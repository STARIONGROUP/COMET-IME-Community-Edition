// -------------------------------------------------------------------------------------------------
// <copyright file="CustomFilterOperatorHandlerFactory.cs" company="Starion Group S.A.">
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
    using System;
    using System.Collections.Generic;

    using CDP4Common.CommonData;

    using CDP4Composition.Mvvm;

    /// <summary>
    /// A class that constructs a <see cref="ICustomFilterOperatorHandler"/> by its corresponding <see cref="CustomFilterOperatorType"/>
    /// which is typically defined in XAML.
    /// </summary>
    public static class CustomFilterOperatorHandlerFactory
    {
        /// <summary>
        /// Constructs a <see cref="ICustomFilterOperatorHandler"/> by its corresponding <see cref="CustomFilterOperatorType"/>.
        /// </summary>
        /// <param name="customFilterOperator">
        /// The <see cref="CustomFilterOperatorType"/>
        /// </param>
        /// <param name="rowViewModels">
        /// The <see cref="IEnumerable{T}"/> of type <see cref="IRowViewModelBase{Thing}"/> where to search values for.
        /// </param>
        /// <param name="fieldName">
        /// The propertyname (FieldName from the GridViewColumn's perspective) to serach for.
        /// </param>
        /// <returns>
        /// The constructed <see cref="ICustomFilterOperatorHandler"/>.
        /// </returns>
        public static ICustomFilterOperatorHandler CreateFilterOperatorHandler(CustomFilterOperatorType customFilterOperator, IEnumerable<IRowViewModelBase<Thing>> rowViewModels, string fieldName)
        {
            if (customFilterOperator == CustomFilterOperatorType.Category)
            {
                return new CategoryFilterOperatorHandler(rowViewModels, fieldName);
            }

            throw new NotSupportedException($"{nameof(CustomFilterOperatorHandler)} {customFilterOperator} is not supported by {nameof(CustomFilterOperatorHandlerFactory)}.");
        }
    }
}
