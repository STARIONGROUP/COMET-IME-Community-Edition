// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorkbookSelectionDialogResult.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2020 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Cozmin Velciu, Adrian Chivu
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
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4CrossViewEditor.ViewModels
{
    using System.Collections.Generic;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Navigation;

    using NetOffice.ExcelApi;

    /// <summary>
    /// The purpose of the <see cref="WorkbookSelectionDialogResult"/> is to return the selected <see cref="Workbook"/>
    /// </summary>
    public class WorkbookSelectionDialogResult : BaseDialogResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkbookSelectionDialogResult"/> class.
        /// </summary>
        /// <param name="res">
        /// The result of the dialog.
        /// </param>
        /// <param name="workbook">
        /// The <see cref="Workbook"/> that is the result of a selection.
        /// </param>
        /// <param name="savedElementsDefinitions">
        /// Elements that has been selected <see cref="ElementDefinition"/>.
        /// </param>
        /// <param name="savedParameterTypes">
        /// Parameter types that has been selected <see cref="ParameterType"/>.
        /// </param>
        /// <param name="savedChangedValues">
        /// Manually saved parameter sheet values.
        /// </param>
        /// <param name="persistValues">
        /// Flag that indicate if values should be saved as workbook metadata.
        /// </param>
        public WorkbookSelectionDialogResult(
            bool? res,
            Workbook workbook,
            IEnumerable<ElementDefinition> savedElementsDefinitions,
            IEnumerable<ParameterType> savedParameterTypes,
            Dictionary<string, string> savedChangedValues,
            bool persistValues)
            : base(res)
        {
            this.Workbook = workbook;
            this.WorkbookElements = savedElementsDefinitions;
            this.WorkbookParameterType = savedParameterTypes;
            this.WorkbookChangedValues = savedChangedValues;
            this.PersistValues = persistValues;
        }

        /// <summary>
        /// Gets or sets the <see cref="Workbook"/>
        /// </summary>
        public Workbook Workbook { get; private set; }

        /// <summary>
        /// Gets or sets workbook element definitions
        /// </summary>
        public IEnumerable<ElementDefinition> WorkbookElements { get; private set; }

        /// <summary>
        /// Gets or sets workbook parameter types
        /// </summary>
        public IEnumerable<ParameterType> WorkbookParameterType { get; private set; }

        /// <summary>
        /// Get or sets workbook manually edited data values
        /// </summary>
        public Dictionary<string, string> WorkbookChangedValues { get; private set; }

        /// <summary>
        /// Gets or sets workbook metadata persistence between sessions.
        /// </summary>
        public bool PersistValues { get; private set; }
    }
}
