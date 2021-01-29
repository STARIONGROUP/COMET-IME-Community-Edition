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
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4CrossViewEditor.Assemblers;

    using CDP4Dal;

    using NetOffice.ExcelApi;
    using NetOffice.ExcelApi.Enums;

    using NLog;

    /// <summary>
    /// The purpose of the <see cref="CrossviewSheetGenerator"/> is to generate in Excel
    /// the crossview sheet that contains the selected <see cref="ElementDefinition"/>s, <see cref="ElementUsage"/>s,
    /// and for each <see cref="ParameterType"/> display the value of the <see cref="CDP4Common.EngineeringModelData.Parameter"/> and/or <see cref="ParameterOverride"/>
    /// for the active <see cref="Participant"/>.
    /// </summary>
    [ExcludeFromCodeCoverage]
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
        /// The <see cref="Iteration"/> for which the crossview sheet needs to be generated.
        /// </summary>
        private readonly Iteration iteration;

        /// <summary>
        /// The <see cref="Participant"/> for which the crossview sheet needs to be generated.
        /// </summary>
        private readonly Participant participant;

        /// <summary>
        /// The crossview <see cref="Worksheet"/>.
        /// </summary>
        private Worksheet crossviewSheet;

        /// <summary>
        /// The <see cref="CrossviewArrayAssembler"/>
        /// </summary>
        private CrossviewArrayAssembler crossviewArrayAssember;

        /// <summary>
        /// The <see cref="CrossviewHeaderArrayAssembler"/>
        /// </summary>
        private CrossviewHeaderArrayAssembler headerArrayAssembler;

        /// <summary>
        /// Gets the <see cref="ISession"/> that is active
        /// </summary>
        private readonly ISession session;

        /// <summary>
        /// Initializes a new instance of the <see cref="CrossviewSheetGenerator"/> class.
        /// </summary>
        /// <param name="session">
        /// The <see cref="ISession"/> for which the crossview sheet is generated.
        /// </param>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> for which the crossview sheet is generated.
        /// </param>
        /// <param name="participant">
        /// The <see cref="Participant"/> for which the crossview sheet is generated.
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
        /// The current <see cref="Workbook"/> when crossview sheet will be rebuild.
        /// </param>
        /// <param name="workbookMetadata">
        /// The current <see cref="WorkbookMetadata"/> associated.
        /// </param>
        public void Rebuild(Application application, Workbook workbook, WorkbookMetadata workbookMetadata)
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

                this.PopulateSheetArrays(workbookMetadata.ElementDefinitions, workbookMetadata.ParameterTypes);
                this.WriteSheet(workbookMetadata.ParameterValues);
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
        /// Apply formatting settings to the crossview sheet
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
        /// collect the information that is to be written to the crossview sheet
        /// </summary>
        /// <param name="elementDefinitions">
        /// Selected element definition list
        /// </param>
        /// <param name="parameterTypes">
        /// Selected parameter types list
        /// </param>
        private void PopulateSheetArrays(IEnumerable<ElementDefinition> elementDefinitions, IEnumerable<ParameterType> parameterTypes)
        {
            // Instantiate the different rows
            var sheetRowAssembler = new CrossviewSheetRowAssembler();
            sheetRowAssembler.Assemble(elementDefinitions);
            var excelRows = sheetRowAssembler.ExcelRows;

            // Use the instantiated rows to populate the excel array
            this.crossviewArrayAssember = new CrossviewArrayAssembler(excelRows, parameterTypes);

            // Instantiate header
            this.headerArrayAssembler = new CrossviewHeaderArrayAssembler(
                this.session,
                this.iteration,
                this.participant,
                this.crossviewArrayAssember.ContentArray.GetLength(1));
        }

        /// <summary>
        /// Write the data to the crossview sheet
        /// </summary>
        private void WriteSheet(Dictionary<string, string> changedValues)
        {
            this.WriteHeader();
            this.WriteRows(changedValues);
        }

        /// <summary>
        /// Write the header info to the crossview sheet
        /// </summary>
        private void WriteHeader()
        {
            var numberOfRows = this.headerArrayAssembler.HeaderArray.GetLength(0);
            var numberOfColumns = this.headerArrayAssembler.HeaderArray.GetLength(1);

            var range = this.crossviewSheet.Range(this.crossviewSheet.Cells[1, 1], this.crossviewSheet.Cells[numberOfRows, numberOfColumns]);
            range.HorizontalAlignment = XlHAlign.xlHAlignLeft;
            range.NumberFormat = this.headerArrayAssembler.FormatArray;
            range.Locked = this.headerArrayAssembler.LockArray;
            range.Name = CrossviewSheetConstants.HeaderName;
            range.Value = this.headerArrayAssembler.HeaderArray;
            range.Interior.ColorIndex = 8;
            range.EntireColumn.AutoFit();
        }

        /// <summary>
        /// Write the content of the crossview sheet
        /// </summary>
        private void WriteRows(Dictionary<string, string> changedValues)
        {
            var numberOfHeaderRows = this.headerArrayAssembler.HeaderArray.GetLength(0);

            var numberOfBodyRows = this.crossviewArrayAssember.ContentArray.GetLength(0);
            var numberOfColumns = this.crossviewArrayAssember.ContentArray.GetLength(1);

            var dataStartRow = numberOfHeaderRows + CrossviewSheetConstants.HeaderDepth;
            var dataEndRow = numberOfHeaderRows + numberOfBodyRows;

            var parameterRange = this.crossviewSheet.Range(
                this.crossviewSheet.Cells[numberOfHeaderRows + 1, 1],
                this.crossviewSheet.Cells[dataEndRow, numberOfColumns]);

            parameterRange.Name = CrossviewSheetConstants.RangeName;
            parameterRange.NumberFormat = this.crossviewArrayAssember.FormatArray;
            parameterRange.Value = this.crossviewArrayAssember.ContentArray;
            parameterRange.Locked = this.crossviewArrayAssember.LockArray;
            parameterRange.EntireColumn.AutoFit();

            var formattedRange = this.crossviewSheet.Range(
                this.crossviewSheet.Cells[numberOfHeaderRows + 1, 1],
                this.crossviewSheet.Cells[dataStartRow, numberOfColumns]);

            formattedRange.Interior.ColorIndex = 34;
            formattedRange.HorizontalAlignment = XlHAlign.xlHAlignCenter;
            formattedRange.VerticalAlignment = XlVAlign.xlVAlignCenter;
            formattedRange.Font.Bold = true;
            formattedRange.Font.Underline = true;
            formattedRange.Font.Size = 11;
            formattedRange.EntireColumn.AutoFit();

            this.PrettifyBodyHeader();

            // add names to parameter value cells
            for (var i = 0; i < numberOfBodyRows; ++i)
            {
                for (var j = 0; j < numberOfColumns; ++j)
                {
                    if (this.crossviewArrayAssember.NamesArray[i, j] == null)
                    {
                        continue;
                    }

                    var cellName = this.crossviewArrayAssember.NamesArray[i, j].ToString();
                    var cellObject = this.crossviewSheet.Cells[numberOfHeaderRows + i + 1, j + 1];
                    cellObject.Name = cellName;

                    if (changedValues.ContainsKey(cellName))
                    {
                        cellObject.Value = changedValues[cellName];
                        var range = this.crossviewSheet.Range(cellObject, cellObject);
                        range.Font.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Blue);
                    }
                }
            }

            this.crossviewSheet.Cells[dataStartRow + 1, 1].Select();
            this.excelApplication.ActiveWindow.FreezePanes = true;
        }

        /// <summary>
        /// Prettify body header
        /// </summary>
        private void PrettifyBodyHeader()
        {
            var numberOfHeaderRows = this.headerArrayAssembler.HeaderArray.GetLength(0);
            var numberOfColumns = this.crossviewArrayAssember.ContentArray.GetLength(1);
            var dataStartRow = numberOfHeaderRows + CrossviewSheetConstants.HeaderDepth;

            // format fixed columns
            for (var i = 1; i <= CrossviewSheetConstants.FixedColumns; ++i)
            {
                this.crossviewSheet.Range(
                        this.crossviewSheet.Cells[numberOfHeaderRows + 1, i],
                        this.crossviewSheet.Cells[dataStartRow, i])
                    .Merge();
            }

            // collapse empty header rows
            for (var i = 0; i < CrossviewSheetConstants.HeaderDepth; ++i)
            {
                var collapse = true;

                for (var j = CrossviewSheetConstants.FixedColumns; j < numberOfColumns; ++j)
                {
                    if ((string)this.crossviewArrayAssember.ContentArray[i, j] == "")
                    {
                        continue;
                    }

                    collapse = false;
                    break;
                }

                if (collapse)
                {
                    this.crossviewSheet.Cells[numberOfHeaderRows + i + 1, 1].EntireRow.Hidden = true;
                }
            }

            // group horizontal parameter columns
            var bodyHeaderDictionary = this.crossviewArrayAssember.headerDictionary;

            foreach (var parameterTypeShortName in bodyHeaderDictionary.Keys.ToList())
            {
                var ptcDictionary = bodyHeaderDictionary[parameterTypeShortName];

                var minPt = numberOfColumns;
                var maxPt = 0;

                foreach (var parameterTypeComponentShortName in ptcDictionary.Keys.ToList())
                {
                    var msDictionary = ptcDictionary[parameterTypeComponentShortName];

                    var minPtc = numberOfColumns;
                    var maxPtc = 0;

                    foreach (var measurementScaleShortName in msDictionary.Keys.ToList())
                    {
                        var oDictionary = msDictionary[measurementScaleShortName];

                        var minMs = numberOfColumns;
                        var maxMs = 0;

                        foreach (var optionShortName in oDictionary.Keys.ToList())
                        {
                            var afslDictionary = oDictionary[optionShortName];

                            var minO = numberOfColumns;
                            var maxO = 0;

                            foreach (var actualFiniteStateListShortName in afslDictionary.Keys.ToList())
                            {
                                var afsDictionary = afslDictionary[actualFiniteStateListShortName];

                                var minAfsl = afsDictionary.Values.Min();
                                var maxAfsl = afsDictionary.Values.Max();

                                minO = Math.Min(minO, minAfsl);
                                maxO = Math.Max(maxO, maxAfsl);

                                minMs = Math.Min(minMs, minAfsl);
                                maxMs = Math.Max(maxMs, maxAfsl);

                                minPtc = Math.Min(minPtc, minAfsl);
                                maxPtc = Math.Max(maxPtc, maxAfsl);

                                minPt = Math.Min(minPt, minAfsl);
                                maxPt = Math.Max(maxPt, maxAfsl);

                                if (minAfsl == maxAfsl)
                                {
                                    continue;
                                }

                                this.crossviewSheet.Range(
                                        this.crossviewSheet.Cells[numberOfHeaderRows + 5, minAfsl + 1],
                                        this.crossviewSheet.Cells[numberOfHeaderRows + 5, maxAfsl + 1])
                                    .Merge();
                            }

                            if (minO == maxO)
                            {
                                continue;
                            }

                            this.crossviewSheet.Range(
                                    this.crossviewSheet.Cells[numberOfHeaderRows + 4, minO + 1],
                                    this.crossviewSheet.Cells[numberOfHeaderRows + 4, maxO + 1])
                                .Merge();
                        }

                        if (minMs == maxMs)
                        {
                            continue;
                        }

                        this.crossviewSheet.Range(
                                this.crossviewSheet.Cells[numberOfHeaderRows + 3, minMs + 1],
                                this.crossviewSheet.Cells[numberOfHeaderRows + 3, maxMs + 1])
                            .Merge();
                    }

                    if (minPtc == maxPtc)
                    {
                        continue;
                    }

                    this.crossviewSheet.Range(
                            this.crossviewSheet.Cells[numberOfHeaderRows + 2, minPtc + 1],
                            this.crossviewSheet.Cells[numberOfHeaderRows + 2, maxPtc + 1])
                        .Merge();
                }

                if (minPt == maxPt)
                {
                    continue;
                }

                this.crossviewSheet.Range(
                        this.crossviewSheet.Cells[numberOfHeaderRows + 1, minPt + 1],
                        this.crossviewSheet.Cells[numberOfHeaderRows + 1, maxPt + 1])
                    .Merge();
            }
        }
    }
}
