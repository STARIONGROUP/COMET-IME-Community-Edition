// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReportingDataSourceRowRepresentation.cs" company="RHEA System S.A.">
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

namespace CDP4Composition.Reporting
{
    /// <summary>
    /// Abstract base class from which all row representations for a <see cref="ReportingDataSourceClass{T}"/> need to derive.
    /// </summary>
    public abstract class ReportingDataSourceRowRepresentation
    {
        /// <summary>
        /// The Element name, fully qualified with the path to the top element.
        /// </summary>
        internal string ElementName;

        /// <summary>
        /// Flag indicating whether the row matches the filtered criteria defined in <see cref="CategoryHierarchy"/>.
        /// Note that when this is false, all values will be null on the row.
        /// </summary>
        internal bool IsVisible;
    }
}
