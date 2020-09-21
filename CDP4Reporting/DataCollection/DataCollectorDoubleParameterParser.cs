// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataCollectorDoubleParameterParser.cs" company="RHEA System S.A.">
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
    using System.Globalization;

    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// Utility class that helps to convert a <see cref="IValueSet"/> string to a double value according to ECSS-TM-10-25 rules.
    /// </summary>
    public class DataCollectorDoubleParameterParser
    {
        /// <summary>
        /// Convert a string value to a double according to ECSS-TM-10-25 rules.
        /// </summary>
        /// <param name="value">The <see cref="string"/> value</param>
        /// <param name="parameterBase">The <see cref="ParameterBase"/> used to </param>
        /// <returns>The converted <see cref="double"/></returns>
        public double Parse(string value, ParameterBase parameterBase)
        {
            var calculatedValue = value;

            if (value != null && parameterBase != null)
            {
                calculatedValue = CDP4Common.Helpers.ValueSetConverter.ToValueSetString(calculatedValue, parameterBase.ParameterType);
            }

            double.TryParse(calculatedValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var invariantNum);

            return invariantNum;
        }
    }
}
