// ------------------------------------------------------------------------------------------------
// <copyright file="WorkbookSelectionDialogResult.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// -----------------------------------------------------------------------------------------------

namespace CDP4ParameterSheetGenerator.ViewModels
{
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
        /// <param name="workook">
        /// The <see cref="Workbook"/> that is the result of a selection.
        /// </param>
        public WorkbookSelectionDialogResult(bool? res, Workbook workook)
            : base(res)
        {
            this.Workbook = workook;
        }

        /// <summary>
        /// Gets the <see cref="Workbook"/> 
        /// </summary>
        public Workbook Workbook { get; private set; }
    }
}
