// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorkbookSelectionDialogResult.cs" company="RHEA System S.A.">
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
        /// The result of the dialog
        /// </param>
        /// <param name="workbook">
        /// The <see cref="Workbook"/> that is the result of a selection.
        /// </param>
        /// <param name="manuallySavedElementsDefinitionValues">
        /// Elements that has been selected <see cref="ElementDefinition"/>
        /// </param>
        /// <param name="manuallySavedParameterTypesValues">
        /// Parameter types that has been selected <see cref="ParameterType"/>
        /// </param>
        /// <param name="manuallySavedChangedValues">
        /// Manually saved parameter sheet values
        /// </param>
        public WorkbookSelectionDialogResult(
            bool? res,
            Workbook workbook,
            IEnumerable<ElementDefinition> manuallySavedElementsDefinitionValues,
            IEnumerable<ParameterType> manuallySavedParameterTypesValues,
            Dictionary<string, string> manuallySavedChangedValues,
            bool persistValues)
            : base(res)
        {
            this.Workbook = workbook;
            this.WorkbookElements = manuallySavedElementsDefinitionValues;
            this.WorkbookParameterType = manuallySavedParameterTypesValues;
            this.WorkbookChangedValues = manuallySavedChangedValues;
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
        ///
        /// </summary>
        public bool PersistValues { get; private set; }
    }
}
