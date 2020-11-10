// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataCollector.cs" company="RHEA System S.A.">
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
    using System.Collections.Generic;

    /// <summary>
    /// The abstract base class that implements the <see cref="IDataCollector"/>
    /// </summary>
    public abstract class DataCollector : IDataCollector
    {
        /// <summary>
        /// Gets all Dynamic table fields
        /// </summary>
        public Dictionary<string, Dictionary<string, string>> DynamicTableFields { get; } = new Dictionary<string, Dictionary<string, string>>();

        /// <summary>
        /// Add a field to the <see cref="DynamicTableFields"/> property
        /// </summary>
        /// <param name="tableName">The name of the Table in the report</param>
        /// <param name="fieldName">The name of the datasource's field to show.</param>
        /// <param name="columnHeader">The column header of the datasource's field</param>
        public void AddDynamicTableField(string tableName, string fieldName, string columnHeader)
        {
            if (!this.DynamicTableFields.ContainsKey(tableName))
            {
                this.DynamicTableFields.Add(tableName, new Dictionary<string, string>());
            }

            if (!this.DynamicTableFields[tableName].ContainsKey(fieldName))
            {
                this.DynamicTableFields[tableName].Add(fieldName, columnHeader);
            }
            else
            {
                this.DynamicTableFields[tableName][fieldName] = columnHeader;
            }
        }

        /// <summary>
        /// Creates a new data collection instance.
        /// </summary>
        /// <returns>
        /// An object instance.
        /// </returns>
        public abstract object CreateDataObject();
    }
}
