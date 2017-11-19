// -------------------------------------------------------------------------------------------------
// <copyright file="WorkbookRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4ParameterSheetGenerator.ViewModels
{
    using System;
    using NetOffice.ExcelApi;

    /// <summary>
    /// Represents a workbook row that is open in an Excel application
    /// </summary>
    public class WorkbookRowViewModel
    {        
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkbookRowViewModel"/> class.
        /// </summary>
        /// <param name="workbook">
        /// The <see cref="Workbook"/> that is represented by the current row.
        /// </param>
        public WorkbookRowViewModel(Workbook workbook)
        {
            if (workbook == null)
            {
                throw new ArgumentNullException("workbook", "The workbook may not be null");
            }

            this.Workbook = workbook;
            this.Name = workbook.Name;
            this.Path = workbook.Path == string.Empty ? "<New Workbook>" : workbook.Path;            
        }

        /// <summary>
        /// Gets the <see cref="Workbook"/> that is represented by the current row view-model.
        /// </summary>
        public Workbook Workbook { get; private set; }

        /// <summary>
        /// Gets the Name of the <see cref="Workbook"/> that is represented by the current row view-model.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the Path of the <see cref="Workbook"/> that is represented by the current row view-model.
        /// </summary>
        public string Path { get; private set; }
    }
}
