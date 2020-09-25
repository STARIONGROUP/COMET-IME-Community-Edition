// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataCollectionDoubleParameter.cs" company="RHEA System S.A.">
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
//    along with this program. If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Reporting.DataCollection
{
    using CDP4Common.Helpers;

    /// <summary>
    /// Abstract base class from which all double parameter columns
    /// for a <see cref="DataCollectorRow"/> need to derive.
    /// </summary>
    /// <typeparam name="TRow">
    /// The type of the associated <see cref="DataCollectorRow"/>.
    /// </typeparam>
    public class DataCollectorDoubleParameter<TRow> : DataCollectorParameter<TRow, double>
        where TRow : DataCollectorRow, new()
    {
        /// <summary>
        /// Parses a parameter value as double.
        /// </summary>
        /// <param name="value">
        /// The parameter value to be parsed.
        /// </param>
        /// <returns>
        /// The parsed value.
        /// </returns>
        public override double Parse(string value)
        {
            ValueSetConverter.TryParseDouble(value, this.ParameterBase?.ParameterType, out var parsedValue);
            return parsedValue;
        }
    }
}
