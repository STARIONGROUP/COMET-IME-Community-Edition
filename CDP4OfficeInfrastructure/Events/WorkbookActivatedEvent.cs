// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorkbookActivatedEvent.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4OfficeInfrastructure.Events
{
    using NetOffice.ExcelApi;

    /// <summary>
    /// The purpose of the <see cref="WorkbookActivatedEvent"/> is to notify any listeners that an Excel workbook
    /// has been activated in the excel application.
    /// </summary>
    public class WorkbookActivatedEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkbookActivatedEvent"/> class.
        /// </summary>
        /// <param name="workbook">
        /// The <see cref="Workbook"/> that has been activated.
        /// </param>
        public WorkbookActivatedEvent(Workbook workbook)
        {
            this.Workbook = workbook;
        }

        /// <summary>
        /// Gets the <see cref="Workbook"/> that has been activated.
        /// </summary>
        public Workbook Workbook { get; private set; }
    }
}
