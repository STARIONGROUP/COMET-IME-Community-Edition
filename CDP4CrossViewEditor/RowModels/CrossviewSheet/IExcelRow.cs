// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IExcelRow.cs" company="RHEA System S.A.">
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

namespace CDP4CrossViewEditor.RowModels.CrossviewSheet
{
    using System.Collections.Generic;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// The interface that is used to populate the ParameterSheet
    /// </summary>
    /// <typeparam name="T">
    /// The type parameter
    /// </typeparam>
    /// <remarks>
    /// A Covariant interface that allows the <see cref="Thing"/> to be of a more generic type
    /// </remarks>
    public interface IExcelRow<out T> where T : Thing
    {
        /// <summary>
        /// Gets the <see cref="Thing"/> that is represented by the current <see cref="IExcelRow{T}"/>
        /// </summary>
        T Thing { get; }

        /// <summary>
        /// Gets the type of <see cref="Thing"/> that this row represents
        /// </summary>
        string Type { get; }

        /// <summary>
        /// Gets the level that this row is located at.
        /// </summary>
        /// <remarks>
        /// The Level property is used to apply grouping in Excel
        /// </remarks>
        int Level { get; }

        /// <summary>
        /// Gets the human readable Name of the <see cref="Parameter"/>, <see cref="ParameterOverride"/> or <see cref="ParameterSubscription"/> that is represented by the Row
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the human readable short name
        /// </summary>
        string ShortName { get; }

        /// <summary>
        /// Gets the model code of the <see cref="Thing"/> that is represented by the current row.
        /// </summary>
        string ModelCode { get; set; }

        /// <summary>
        /// Gets the short-name of the owning <see cref="DomainOfExpertise"/>
        /// </summary>
        string Owner { get; }

        /// <summary>
        /// Gets the short-name of the <see cref="Category"/>s that the <see cref="ICategorizableThing"/> the current
        /// row represents is a member of.
        /// </summary>
        string Categories { get; }

        /// <summary>
        /// Gets the unique id if the <see cref="Thing"/> that is represented by the row
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets or sets the <see cref="Container"/> property.
        /// </summary>
        IExcelRow<Thing> Container { get; set; }

        /// <summary>
        /// Queries the current row for the contained rows
        /// </summary>
        /// <returns>
        /// the rows that are contained by the current row
        /// </returns>
        IEnumerable<IExcelRow<Thing>> GetContainedRows();

        /// <summary>
        /// Queries the current row for all the rows in the subtree
        /// </summary>
        /// <returns>
        /// the rows that are contained by the current row and its subtree
        /// </returns>
        IEnumerable<IExcelRow<Thing>> GetContainedRowsDeep();
    }
}
