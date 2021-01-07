// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CrossviewSheetGenerator.cs" company="RHEA System S.A.">
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

namespace CDP4CrossViewEditor.Generator
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4CrossViewEditor.Assemblers;
    using CDP4CrossViewEditor.RowModels.CrossviewSheet;

    using CDP4Dal;

    using NetOffice.ExcelApi;
    using NetOffice.ExcelApi.Enums;

    using NLog;

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// The purpose of the <see cref="CrossviewSheetGenerator"/> is to generate in Excel
    /// the Crossview sheet that contains the ElementDefinitions, Parameters, ParameterOverrides, and Subscriptions
    /// of the <see cref="DomainOfExpertise"/> of the active <see cref="Participant"/>.
    /// </summary>
    public class CrossviewSheetGenerator
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The current excel application
        /// </summary>
        private Application excelApplication;

        /// <summary>
        /// The <see cref="Iteration"/> for which the Parameter-Sheet needs to be generated.
        /// </summary>
        private readonly Iteration iteration;

        /// <summary>
        /// The <see cref="Participant"/> for which the Parameter-Sheet needs to be generated.
        /// </summary>
        private readonly Participant participant;

        /// <summary>
        /// The <see cref="IExcelRow{T}"/> that make up the content of the parameter sheet
        /// </summary>
        private IEnumerable<IExcelRow<Thing>> excelRows;

        /// <summary>
        /// The <see cref="Worksheet"/> that the parameters are written to.
        /// </summary>
        private Worksheet crossviewSheet;

        /// <summary>
        /// The array that contains the content of the header section of the crossview sheet
        /// </summary>
        private object[,] headerContent;

        /// <summary>
        /// The array that contains the lock settings of the header section of the crossview sheet
        /// </summary>
        private object[,] headerLock;

        /// <summary>
        /// The array that contains the formatting settings of the header section of the crossview sheet
        /// </summary>
        private object[,] headerFormat;

        /// <summary>
        /// The array that contains the formatting of the parameter section of the crossview sheet
        /// </summary>
        private object[,] parameterFormat;

        /// <summary>
        /// The array that contains the content of the parameter section of the crossview sheet
        /// </summary>
        private object[,] parameterContent;

        /// <summary>
        /// The array that contains the lock settings of the parameter section of the crossview sheet
        /// </summary>
        private object[,] parameterLock;

        /// <summary>
        /// Gets the <see cref="ISession"/> that is active
        /// </summary>
        private readonly ISession session;

        /// <summary>
        /// Initializes a new instance of the <see cref="CrossviewSheetGenerator"/> class.
        /// </summary>
        /// <param name="session">
        /// The <see cref="ISession"/> for which the parameter sheet is generated.
        /// </param>
        /// <param name="iteration">
        /// The iteration that contains the <see cref="ParameterValueSet"/>s that will be generated on the Parameter sheet
        /// </param>
        /// <param name="participant">
        /// The participant for which the sheet is generated.
        /// </param>
        public CrossviewSheetGenerator(ISession session, Iteration iteration, Participant participant)
        {
            this.session = session;
            this.iteration = iteration;
            this.participant = participant;
        }

        /// <summary>
        /// Rebuild the crossview sheet
        /// </summary>
        /// <param name="application">
        /// The excel application object that contains the <see cref="Workbook"/>
        /// </param>
        /// <param name="workbook">
        /// The current <see cref="Workbook"/> when Crossview sheet will be rebuild.
        /// </param>
        /// <param name="elementDefinitions"></param>
        /// <param name="parameterTypes"></param>
        public void Rebuild(Application application, Workbook workbook, IEnumerable<ElementDefinition> elementDefinitions,
            IEnumerable<ParameterType> parameterTypes)
        {
            var sw = new Stopwatch();
            sw.Start();

            this.excelApplication = application;

            this.excelApplication.StatusBar = "Rebuilding Crossview Sheet";

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

                this.crossviewSheet = CrossviewSheetUtilities.RetrieveSheet(workbook, true);

                CrossviewSheetUtilities.ApplyLocking(this.crossviewSheet, false);

                this.PopulateSheetArrays(elementDefinitions, parameterTypes);
                this.WriteSheet();
                this.ApplySheetSettings();

                CrossviewSheetUtilities.ApplyLocking(this.crossviewSheet, true);

                this.excelApplication.StatusBar = $"CDP4: Crossview Sheet rebuilt in {sw.ElapsedMilliseconds} [ms]";
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                this.excelApplication.StatusBar = $"CDP4: The following error occured while rebuilding the sheet: {ex.Message}";
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
        /// Apply formatting settings to the Parameter sheet
        /// </summary>
        private void ApplySheetSettings()
        {
            var outline = this.crossviewSheet.Outline;
            outline.AutomaticStyles = false;
            outline.SummaryRow = XlSummaryRow.xlSummaryAbove;
            outline.SummaryColumn = XlSummaryColumn.xlSummaryOnRight;

            this.excelApplication.ActiveWindow.DisplayGridlines = false;
        }

        /// <summary>
        /// collect the information that is to be written to the Parameter sheet
        /// </summary>
        /// <param name="elementDefinitions"></param>
        /// <param name="parameterTypes"></param>
        private void PopulateSheetArrays(IEnumerable<ElementDefinition> elementDefinitions, IEnumerable<ParameterType> parameterTypes)
        {
            var selectedDomainOfExpertise = this.session.QuerySelectedDomainOfExpertise(this.iteration);

            // Instantiate the different rows
            var assembler = new CrossviewSheetRowAssembler(selectedDomainOfExpertise);
            assembler.Assemble(elementDefinitions);
            this.excelRows = assembler.ExcelRows;

            // Use the instantiated rows to populate the excel array
            var parameterArrayAssembler = new CrossviewArrayAssembler(this.excelRows, parameterTypes);
            this.parameterContent = parameterArrayAssembler.ContentArray;
            this.parameterFormat = parameterArrayAssembler.FormatArray;
            this.parameterLock = parameterArrayAssembler.LockArray;

            // Instantiate header
            var headerArrayAssembler = new CrossviewHeaderArrayAssembler(this.session, this.iteration, this.participant, this.parameterContent.GetLength(1));
            this.headerFormat = headerArrayAssembler.FormatArray;
            this.headerContent = headerArrayAssembler.HeaderArray;
            this.headerLock = headerArrayAssembler.LockArray;
        }

        /// <summary>
        /// Write the data to the Parameter sheet
        /// </summary>
        private void WriteSheet()
        {
            this.WriteHeader();
            this.WriteRows();
        }

        /// <summary>
        /// Write the header info to the parameter sheet
        /// </summary>
        private void WriteHeader()
        {
            var numberOfRows = this.headerContent.GetLength(0);
            var numberOfColumns = this.headerContent.GetLength(1);

            var range = this.crossviewSheet.Range(this.crossviewSheet.Cells[1, 1], this.crossviewSheet.Cells[numberOfRows, numberOfColumns]);
            range.HorizontalAlignment = XlHAlign.xlHAlignLeft;
            range.NumberFormat = this.headerFormat;
            range.Locked = this.headerLock;
            range.Name = "Header";
            range.Value = this.headerContent;
            range.Interior.ColorIndex = 8;
            range.EntireColumn.AutoFit();
        }

        /// <summary>
        /// Write the content of the <see cref="parameterContent"/> to the parameter sheet
        /// </summary>
        private void WriteRows()
        {
            var numberOfRows = this.parameterContent.GetLength(0);
            var numberOfColumns = this.parameterContent.GetLength(1);

            var startrow = this.headerContent.GetLength(0) + 2;
            var endrow = startrow + numberOfRows - 1;

            var parameterRange = this.crossviewSheet.Range(this.crossviewSheet.Cells[startrow, 1], this.crossviewSheet.Cells[endrow, numberOfColumns]);
            parameterRange.Name = "Crossview";
            parameterRange.NumberFormat = this.parameterFormat;
            parameterRange.Value = this.parameterContent;
            parameterRange.Locked = this.parameterLock;
            parameterRange.EntireColumn.AutoFit();

            var formattedrange = this.crossviewSheet.Range(this.crossviewSheet.Cells[startrow - 1, 1], this.crossviewSheet.Cells[startrow, numberOfColumns]);
            formattedrange.Interior.ColorIndex = 34;
            formattedrange.HorizontalAlignment = XlHAlign.xlHAlignCenter;
            formattedrange.Font.Bold = true;
            formattedrange.Font.Underline = true;
            formattedrange.Font.Size = 11;
            formattedrange.EntireColumn.AutoFit();

            this.ApplyCellNames(startrow, endrow);

            this.crossviewSheet.Cells[startrow + 1, 1].Select();
            this.excelApplication.ActiveWindow.FreezePanes = true;
        }

        /// <summary>
        /// Apply cell names to the actual value column
        /// </summary>
        /// <param name="beginRow">
        /// The row at which the range begins
        /// </param>
        /// <param name="endRow">
        /// The row at which the range ends
        /// </param>
        private void ApplyCellNames(int beginRow, int endRow)
        {
            var range =
                this.crossviewSheet.Range(
                    this.crossviewSheet.Cells[beginRow + 2, CrossviewSheetConstants.ActualValueColumn],
                    this.crossviewSheet.Cells[endRow, CrossviewSheetConstants.ModelCodeColumn]);

            range.CreateNames(top: false, left: false, bottom: false, right: true);
        }
    }
}
