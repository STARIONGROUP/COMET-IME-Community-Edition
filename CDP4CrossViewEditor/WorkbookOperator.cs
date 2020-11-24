// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorkbookOperator.cs" company="RHEA System S.A.">
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

namespace CDP4CrossViewEditor
{
    using System;

    using CDP4Composition.Navigation;

    using NetOffice.ExcelApi;

    /// <summary>
    /// The purpose of the <see cref="WorkbookOperator"/> is to delegate the operations on a <see cref="Workbook"/>
    /// to the proper classes
    /// </summary>
    public class WorkbookOperator
    {

        /// <summary>
        /// The <see cref="Workbook"/> that is being managed by the current <see cref="WorkbookOperator"/>
        /// </summary>
        private readonly Workbook workbook;

        /// <summary>
        /// The excel <see cref="Application"/> that is being managed by the current <see cref="WorkbookOperator"/>
        /// </summary>
        private readonly Application application;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkbookOperator"/> class.
        /// </summary>
        /// <param name="application">
        /// The excel application object that contains the <see cref="Workbook"/> that is being operated on.
        /// </param>
        /// <param name="workbook">
        /// The <see cref="Workbook"/> that is being operated on.
        /// </param>
        /// <param name="dialogNavigationService">
        /// The instance of <see cref="IDialogNavigationService"/> that orchestrates navigation to dialogs
        /// </param>
        public WorkbookOperator(Application application, Workbook workbook, IDialogNavigationService dialogNavigationService)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application), "the Excel application may not be null");
            }

            if (workbook == null)
            {
                throw new ArgumentNullException(nameof(workbook), "the Excel workbook may not be null");
            }

            workbook.Activate();

            this.workbook = workbook;
            this.application = application;
            this.DialogNavigationService = dialogNavigationService;
        }

        /// <summary>
        /// Gets the <see cref="IDialogNavigationService"/> used to navigate to dialogs
        /// </summary>
        public IDialogNavigationService DialogNavigationService { get; private set; }
    }
}
