// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorkbookSessionDal.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4OfficeInfrastructure.OfficeDal
{
    using NetOffice.ExcelApi;
    using NLog;

    /// <summary>
    /// The purpose of the <see cref="WorkbookSessionDal"/> class is to read and write the <see cref="WorkbookSession"/>
    /// custom XML part to and from a <see cref="Workbook"/>.
    /// </summary>
    public class WorkbookSessionDal : CustomOfficeDataDal<WorkbookSession>
    {
        /// <summary>
        /// The current logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkbookSessionDal"/> class.
        /// </summary>
        /// <param name="workbook">
        /// The <see cref="Workbook"/> that is to be read from or written to.
        /// </param>
        public WorkbookSessionDal(Workbook workbook)
            : base(workbook)
        {
        }
    }
}
