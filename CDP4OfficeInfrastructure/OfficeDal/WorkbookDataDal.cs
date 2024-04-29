// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorkbookDataDal.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4OfficeInfrastructure.OfficeDal
{
    using NetOffice.ExcelApi;

    /// <summary>
    /// The purpose of the <see cref="WorkbookDataDal"/> class is to read and write the Custom XML part related
    /// to <see cref="WorkbookData"/>
    /// </summary>
    public class WorkbookDataDal : CustomOfficeDataDal<WorkbookData>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkbookDataDal"/> class.
        /// </summary>
        /// <param name="workbook">
        /// The <see cref="Workbook"/> that is to be read from or written to.
        /// </param>
        public WorkbookDataDal(Workbook workbook)
            : base(workbook)
        {
        }
    }
}