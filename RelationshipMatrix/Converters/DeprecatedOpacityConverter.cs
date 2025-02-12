// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeprecatedOpacityConverter.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2025 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4RelationshipMatrix.Converters
{
    using System.Collections.Generic;

    using ViewModels;

    /// <summary>
    /// The converter to retrieve the opacity for the cell to be used for deprecated things.
    /// </summary>
    public class DeprecatedOpacityConverter : BaseMatrixCellViewModelConverter<double>
    {
        /// <summary>
        /// Converts the Row object contents into useable double
        /// </summary>
        /// <param name="row">The row data object.</param>
        /// <param name="fieldName">The field name to be used as key in the row object</param>
        /// <returns>The required display name of the row.</returns>
        protected override double ConvertRowObject(IDictionary<string, MatrixCellViewModel> row, string fieldName)
        {
            var matrixCellViewModel = row[fieldName];
            return matrixCellViewModel.IsDeprecated ? 0.5D : double.PositiveInfinity;
        }
    }
}
