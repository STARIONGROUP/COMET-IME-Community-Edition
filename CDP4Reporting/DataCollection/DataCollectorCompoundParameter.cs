// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataCollectorCompoundParameter.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft
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
    using System.Data;
    using System.Linq;

    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// Abstract base class from which parameter columns, that result in seperate <see cref="DataRow"/>s per state, need to derive.
    /// </summary>
    public class DataCollectorCompoundParameter<TRow> : DataCollectorParameter<TRow, string> where TRow : DataCollectorRow, new()
    {
        /// <summary>
        /// Populates with data the <see cref="DataTable.Columns"/> associated with this object
        /// in the given <paramref name="row"/>.
        /// </summary>
        /// <param name="table">
        /// The <see cref="DataTable"/> to which the <paramref name="row"/> belongs to.
        /// </param>
        /// <param name="row">
        /// The <see cref="DataRow"/> to be populated.
        /// </param>
        public override void Populate(DataTable table, DataRow row)
        {
            if (this.HasValueSets)
            {
                var componentNumber = 0;

                if (this.ParameterBase.ParameterType is CompoundParameterType compoundParameterType)
                {
                    var valueSet = this.ValueSets.Distinct().FirstOrDefault();
                
                    foreach (ParameterTypeComponent childParameter in compoundParameterType.Component)
                    {
                        var columnName = this.FieldName + "_" + childParameter.ShortName;

                        if (!table.Columns.Contains(columnName))
                        {
                            table.Columns.Add(columnName, typeof(string));
                        }

                        row[columnName] = this.GetValueSetValue(valueSet, componentNumber);

                        componentNumber++;
                    }
                }
            }
        }

        /// <summary>
        /// Parses a parameter value as double.
        /// </summary>
        /// <param name="value">
        /// The parameter value to be parsed.
        /// </param>
        /// <returns>
        /// The parsed value.
        /// </returns>
        public override string Parse(string value)
        {
            return value;
        }
    }
}
