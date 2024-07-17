﻿// -------------------------------------------------------------------------------------------------
// <copyright file="IHaveCustomFilterOperators.cs" company="Starion Group S.A.">
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

    using DevExpress.Xpf.Grid;

    /// <summary>
    /// Defines the interface for viewmodels, typically of type <see cref="BrowserViewModelBase{T}"/> that could use custom filter operators
    /// </summary>
    public interface IHaveCustomFilterOperators
    {
        /// <summary>
        /// A <see cref="Dictionary{TKey,TValue}"/> where a <see cref="DataViewBase"/> is the key for another
        /// <see cref="Dictionary{TKey,TValue}"/> where the key is a Field/Propertyname and holds a <see cref="ValueTuple"/>
        /// consisting of a <see cref="CustomFilterOperatorType"/> and an <see cref="IEnumerable{T}"/> of type
        /// <see cref="IRowViewModelBase{Thing}"/>.
        ///
        /// Visual structure example:
        /// - DataBaseView1
        ///    - Category
        ///       - (CustomFilterOperatorType.Category, RowViewModels)
        ///    - AnotherField
        ///       - (CustomFilterOperatorType.AnotherCustomFilterOperatorType, RowViewModels)
        /// - DataBaseView2
        ///    - AnotherField
        ///       - (CustomFilterOperatorType.AnotherCustomFilterOperatorType, RowViewModels)
        /// 
        /// </summary>
        Dictionary<DataViewBase, Dictionary<string, (CustomFilterOperatorType, IEnumerable<IRowViewModelBase<Thing>>)>> CustomFilterOperators { get; } 
    }
}
