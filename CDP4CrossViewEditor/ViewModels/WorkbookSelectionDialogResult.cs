// ------------------------------------------------------------------------------------------------
// <copyright file="WorkbookSelectionDialogResult.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -----------------------------------------------------------------------------------------------

namespace CDP4CrossViewEditor.ViewModels
{
    using System;
    using System.Collections.Generic;

    using CDP4Common.CommonData;
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
        /// <param name="workbookElements"></param>
        /// <param name="workbookParameterTypes"></param>
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
        /// Gets the <see cref="Workbook"/>
        /// </summary>
        public Workbook Workbook { get; private set; }

        public IEnumerable<ElementDefinition> WorkbookElements { get; private set; }

        public IEnumerable<ParameterType> WorkbookParameterType { get; private set; }
    }
}
