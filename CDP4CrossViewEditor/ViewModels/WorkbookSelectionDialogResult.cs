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
        /// <param name="res">The result of the dialog</param>
        /// <param name="workbook">The <see cref="Workbook"/> that is the result of a selection.</param>
        /// <param name="workbookElements">Elements that has been selected <see cref="ElementDefinition"/></param>
        /// <param name="workbookParameterTypes">Parameter types that has been selected <see cref="ParameterType"/></param>
        public WorkbookSelectionDialogResult(bool? res, Workbook workbook,
            IEnumerable<ElementDefinition> workbookElements,
            IEnumerable<ParameterType> workbookParameterTypes)
            : base(res)
        {
            this.Workbook = workbook;
            this.WorkbookElements = workbookElements;
            this.WorkbookParameterType = workbookParameterTypes;
        }

        /// <summary>
        /// Gets or sets the <see cref="Workbook"/>
        /// </summary>
        public Workbook Workbook { get; private set; }

        /// <summary>
        /// Gets or sets workbook elements
        /// </summary>
        public IEnumerable<ElementDefinition> WorkbookElements { get; private set; }

        public IEnumerable<ParameterType> WorkbookParameterType { get; private set; }
    }
}
