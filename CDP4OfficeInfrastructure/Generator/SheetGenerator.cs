// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SheetGenerator.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4OfficeInfrastructure.Generator
{
    using System;
    using System.Diagnostics;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using NetOffice.ExcelApi;
    using NetOffice.ExcelApi.Enums;
    using NLog;

    /// <summary>
    /// Abstract super class from which all Sheet Generators derive
    /// </summary>
    public abstract class SheetGenerator
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The <see cref="ISession"/> for which the Parameter-Sheet needs to be generated.
        /// </summary>
        protected readonly ISession session;

        /// <summary>
        /// The <see cref="Iteration"/> for which the Parameter-Sheet needs to be generated.
        /// </summary>
        protected readonly Iteration iteration;

        /// <summary>
        /// The <see cref="Participant"/> for which the Parameter-Sheet needs to be generated.
        /// </summary>
        protected readonly Participant participant;

        /// <summary>
        /// The current excel application
        /// </summary>
        protected Application excelApplication;

        /// <summary>
        /// the string that is used as the list separator.
        /// </summary>
        private string listSeparator;

        /// <summary>
        /// The string that is used as the validation separator.
        /// </summary>
        private string validationSeparator;

        /// <summary>
        /// The country code of the application (language version)
        /// </summary>
        private int xlCountryCode;

        /// <summary>
        /// Initializes a new instance of the <see cref="SheetGenerator"/> class.
        /// </summary>
        /// <param name="session">
        /// The <see cref="ISession"/> for which the Sheet is generated
        /// </param>
        /// <param name="iteration">
        /// The iteration that contains the data that is to be generated
        /// </param>
        /// <param name="participant">
        /// The active <see cref="Participant"/>
        /// </param>
        protected SheetGenerator(ISession session, Iteration iteration, Participant participant)
        {
            if (session == null)
            {
                throw new ArgumentNullException("The session may not be null", "session");
            }

            if (iteration == null)
            {
                throw new ArgumentNullException("The iteration may not be null", "iteration");
            }

            if (participant == null)
            {
                throw new ArgumentNullException("The participant may not be null", "participant");
            }

            this.session = session;
            this.iteration = iteration;
            this.participant = participant;
        }

        /// <summary>
        /// Rebuild the sheets, replace it they already exists.
        /// </summary>
        /// <param name="application">
        /// The excel application object that contains the <see cref="Workbook"/> in which the sheet is to be rebuilt.
        /// </param>
        /// <param name="workbook">
        /// The <see cref="Workbook"/> in which the sheet is to be rebuilt.
        /// </param>
        public void Rebuild(Application application, Workbook workbook)
        {
            var sw = new Stopwatch();
            sw.Start();

            this.excelApplication = application;

            this.excelApplication.StatusBar = "Rebuilding Option Sheets";

            this.listSeparator = (string)application.International(XlApplicationInternational.xlListSeparator);
            this.xlCountryCode = Convert.ToInt32(application.International(XlApplicationInternational.xlCountryCode));

            var enabledEvents = application.EnableEvents;
            var displayAlerts = application.DisplayAlerts;
            var screenupdating = application.ScreenUpdating;
            var calculation = application.Calculation;

            application.EnableEvents = false;
            application.DisplayAlerts = false;
            application.Calculation = XlCalculation.xlCalculationManual;
            application.ScreenUpdating = false;

            try
            {
                application.Cursor = XlMousePointer.xlWait;

                this.Write(workbook);       
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                this.excelApplication.StatusBar = string.Format("CDP4: The following error occured while rebuilding the Option sheets: {0}", ex.Message);
            }
            finally
            {
                application.EnableEvents = enabledEvents;
                application.DisplayAlerts = displayAlerts;
                application.Calculation = calculation;
                application.ScreenUpdating = screenupdating;

                application.Cursor = XlMousePointer.xlDefault;
            }
        }

        /// <summary>
        /// Write the generator specific sheet to the <see cref="Workbook"/>
        /// </summary>
        /// <param name="workbook">
        /// The <see cref="Workbook"/> in which the sheet is to be rebuilt.
        /// </param>
        protected virtual void Write(Workbook workbook)
        {
            logger.Debug("The write method has not been implemented for {0}", this.GetType());
        }
    }
}
