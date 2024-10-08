﻿// -------------------------------------------------------------------------------------------------
// <copyright file="ICustomFilterOperatorHandler.cs" company="Starion Group S.A.">
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

    using DevExpress.Xpf.Core.FilteringUI;

    public interface ICustomFilterOperatorHandler
    {
        /// <summary>
        /// The propertyname (FieldName from a GridView column's perspective) for which we want to collect values.
        /// </summary>
        string FieldName { get; }

        /// <summary>
        /// Changes the <see cref="FilterEditorQueryOperatorsEventArgs.Operators"/> list of filter operators
        /// </summary>
        /// <param name="filterEditorQueryOperatorsEventArgs">
        /// The <see cref="FilterEditorQueryOperatorsEventArgs"/>
        /// </param>
        void SetOperators(FilterEditorQueryOperatorsEventArgs filterEditorQueryOperatorsEventArgs);

        /// <summary>
        /// Gets the values needed for the inherited <see cref="CustomFilterOperatorHandler"/>
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerable{Object}"/> containing the needed values
        /// </returns>
        IEnumerable<object> GetValues();
    }
}
