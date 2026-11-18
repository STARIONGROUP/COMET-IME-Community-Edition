// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorkbookSelectionDialogResult.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2026 Starion Group S.A.
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
    using System.Linq;

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
        /// <param name="generateCalculationSheet">
        /// Flag that indicates whether the calculation sheet is to be written as well.
        /// </param>
        /// <param name="summedParameterTypes">
        /// The parameter types whose columns on the calculation sheet sum the rows below a row without a value of its own.
        /// </param>
        /// <param name="options">
        /// The selected <see cref="Option"/>s; none for every option on a single calculation sheet.
        /// </param>
        /// <param name="followProductTree">
        /// Flag that indicates the selection covered the whole product tree, which later rebuilds then follow.
        /// </param>
        public WorkbookSelectionDialogResult(
            bool? res,
            Workbook workbook,
            IEnumerable<ElementDefinition> savedElementsDefinitions,
            IEnumerable<ParameterType> savedParameterTypes,
            Dictionary<string, string> savedChangedValues,
            bool persistValues,
            bool generateCalculationSheet = false,
            IEnumerable<ParameterType> summedParameterTypes = null,
            IEnumerable<Option> options = null,
            bool followProductTree = false)
            : base(res)
        {
            this.FollowProductTree = followProductTree;
            this.Options = options?.ToList() ?? new List<Option>();
            this.Workbook = workbook;
            this.WorkbookElements = savedElementsDefinitions;
            this.WorkbookParameterType = savedParameterTypes;
            this.WorkbookChangedValues = savedChangedValues;
            this.PersistValues = persistValues;
            this.GenerateCalculationSheet = generateCalculationSheet;
            this.SummedParameterTypes = summedParameterTypes?.ToList() ?? new List<ParameterType>();
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

        /// <summary>
        /// Gets a value indicating whether the calculation sheet is to be written as well.
        /// </summary>
        public bool GenerateCalculationSheet { get; private set; }

        /// <summary>
        /// Gets the parameter types whose columns on the calculation sheet sum the rows below a row without a value of its own.
        /// </summary>
        public IReadOnlyList<ParameterType> SummedParameterTypes { get; private set; }

        /// <summary>
        /// Gets the selected <see cref="Option"/>s; empty for every option on a single calculation sheet.
        /// </summary>
        public IReadOnlyList<Option> Options { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the selection covered the whole product tree, which later rebuilds then follow.
        /// </summary>
        public bool FollowProductTree { get; private set; }
    }
}
