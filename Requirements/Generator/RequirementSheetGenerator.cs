// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequirementSheetGenerator.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Generator
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Dal;
    using CDP4OfficeInfrastructure.Excel;
    using CDP4Requirements.Assemblers;
    using NetOffice.ExcelApi;
    using NetOffice.ExcelApi.Enums;
    using NLog;

    /// <summary>
    /// The purpose of the <see cref="RequirementSheetGenerator"/> is generate a spreadsheet
    /// in a Workbook containing the <see cref="Requirement"/>s that are contained by a <see cref="RequirementsSpecification"/>
    /// </summary>
    public class RequirementSheetGenerator
    {   
        /// <summary>
        /// The NLog logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The array that contains the content of the header section of the <see cref="Requirement"/> sheet
        /// </summary>
        private object[,] headerContent;

        /// <summary>
        /// The array that contains the formatting settings of the header section of the <see cref="Requirement"/> sheet
        /// </summary>
        private object[,] headerFormat;

        /// <summary>
        /// The array that contains the content of the parameter section of the <see cref="Requirement"/> sheet
        /// </summary>
        private object[,] requirementContent;

        /// <summary>
        /// Generates the requirements contained in the subject <see cref="iteration"/>
        /// </summary>
        /// <param name="session">
        /// The active <see cref="ISession"/>
        /// </param>
        /// <param name="application">
        /// The active excel application
        /// </param>
        /// <param name="workbook">
        /// The target <see cref="Workbook"/>
        /// </param>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> that contains the <see cref="Requirement"/>s that need to be generated
        /// </param>
        public void Generate(ISession session, Application application, Workbook workbook, Iteration iteration)
        {
            if (session == null)
            {
                throw new ArgumentNullException($"The {nameof(session)} may not be null");
            }

            if (application == null)
            {
                throw new ArgumentNullException($"The {nameof(application)} may not be null");
            }

            if (workbook == null)
            {
                throw new ArgumentNullException($"The {nameof(workbook)} may not be null");
            }

            if (iteration == null)
            {
                throw new ArgumentNullException($"The {nameof(iteration)} may not be null");
            }

            var sw = Stopwatch.StartNew();
            application.StatusBar = "Generating Requirements sheet";
            
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

                var requirementsSheet = workbook.RetrieveWorksheet($"REQ-ITERATION-{iteration.IterationSetup.IterationNumber}", true);

                var engineeringModel = iteration.Container as EngineeringModel;
                var activeParticipant = engineeringModel.EngineeringModelSetup.Participant.Single(x => x.Person == session.ActivePerson);

                this.PopulateSheetArrays(session, iteration, activeParticipant);
                this.WriteRequirementsSheet(requirementsSheet);
                
                application.StatusBar = $"CDP4: Requirements Sheet rebuilt in {sw.ElapsedMilliseconds} [ms]";
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                application.StatusBar = $"CDP4: The following error occured while generating the Requirements sheet: {ex.Message}";
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
        /// collect the information that is to be written to the <see cref="Requirement"/> sheet
        /// </summary>
        /// <param name="session">
        /// The active <see cref="ISession"/>
        /// </param>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> that contains the <see cref="Requirement"/>s that need to be generated
        /// </param>
        /// <param name="participant">
        /// The <see cref="Participant"/> that is generating the <see cref="Worksheet"/>
        /// </param>
        private void PopulateSheetArrays(ISession session, Iteration iteration, Participant participant)
        {
            var requirements = iteration.RequirementsSpecification.SelectMany(x => x.Requirement).Where(x => !x.IsDeprecated); ;
            var requirementArrayAssembler = new RequirementArrayAssembler(requirements);
            this.requirementContent = requirementArrayAssembler.ContentArray;

            var headerArrayAssembler = new HeaderArrayAssembler(session, iteration, participant);
            this.headerFormat = headerArrayAssembler.FormatArray;
            this.headerContent = headerArrayAssembler.HeaderArray;
        }

        /// <summary>
        /// Write the data to the <see cref="Requirement"/>s sheet
        /// </summary>
        private void WriteRequirementsSheet(Worksheet worksheet)
        {
            this.WriteHeader(worksheet);
            this.WriteRequirements(worksheet);
        }

        /// <summary>
        /// Write the header info to the <see cref="Requirement"/>s sheet
        /// </summary>
        private void WriteHeader(Worksheet worksheet)
        {
            var numberOfRows = this.headerContent.GetLength(0);
            var numberOfColumns = this.headerContent.GetLength(1);

            var range = worksheet.Range(worksheet.Cells[1, 1], worksheet.Cells[numberOfRows, numberOfColumns]);
            range.HorizontalAlignment = XlHAlign.xlHAlignLeft;
            range.NumberFormat = this.headerFormat;
            range.Value = this.headerContent;
            range.Interior.ColorIndex = 8;
        }

        /// <summary>
        /// Write the content of the <see cref="requirementContent"/> array to the requirements sheet
        /// </summary>
        private void WriteRequirements(Worksheet worksheet)
        {
            var numberOfRows = this.requirementContent.GetLength(0);
            var numberOfColumns = this.requirementContent.GetLength(1);

            var startrow = this.headerContent.GetLength(0) + 2;
            var endrow = startrow + numberOfRows - 1;

            var requirementRange = worksheet.Range(worksheet.Cells[startrow, 1], worksheet.Cells[endrow, numberOfColumns]);
            requirementRange.Value = this.requirementContent;
            
            var formattedrange = worksheet.Range(worksheet.Cells[startrow - 1, 1], worksheet.Cells[startrow, numberOfColumns]);
            formattedrange.Interior.ColorIndex = 34;
            formattedrange.HorizontalAlignment = XlHAlign.xlHAlignCenter;
            formattedrange.Font.Bold = true;
            formattedrange.Font.Underline = true;
            formattedrange.Font.Size = 11;
        }
    }
}