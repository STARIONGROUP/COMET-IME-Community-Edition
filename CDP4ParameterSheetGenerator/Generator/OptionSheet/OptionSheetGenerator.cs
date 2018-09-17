// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OptionSheetGenerator.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ParameterSheetGenerator.OptionSheet
{
    using System;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Dal;
    using CDP4OfficeInfrastructure.Generator;
    using CDP4ParameterSheetGenerator.Generator;
    using NetOffice.ExcelApi;
    using NetOffice.ExcelApi.Enums;
    using NLog;

    /// <summary>
    /// The purpose of the <see cref="OptionSheetGenerator"/> is to generate a fully decomposed
    /// architecture per <see cref="Option"/> in a dedicated <see cref="Option"/>sheet in the 
    /// workbook.
    /// </summary>
    public class OptionSheetGenerator : SheetGenerator
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionSheetGenerator"/> class.
        /// </summary>
        /// <param name="session">
        /// The <see cref="ISession"/> for which the Sheet is generated
        /// </param>
        /// <param name="iteration">
        /// The iteration.
        /// </param>
        /// <param name="participant">
        /// The participant.
        /// </param>
        public OptionSheetGenerator(ISession session, Iteration iteration, Participant participant)
            : base(session, iteration, participant)
        {
        }

        /// <summary>
        /// Write the <see cref="Option"/> sheets to the <see cref="Workbook"/>
        /// </summary>
        /// <param name="workbook">
        /// The <see cref="Workbook"/> in which the <see cref="Option"/> sheets are to be rebuilt.
        /// </param>
        protected override void Write(Workbook workbook)
        {
            foreach (var item in this.iteration.Option)
            {
                var option = (Option)item;
                Logger.Info("Writing Option sheet {0} to the workbook", option.ShortName);

                var optionSheet = ParameterSheetUtilities.RetrieveOptionSheet(workbook, option);
                ParameterSheetUtilities.ApplyLocking(optionSheet, false);

                var headerEndRow = this.PopulateAndWriteHeaderArray(optionSheet, option);
                var startrow = headerEndRow + 2;
                this.PopulateAndWriteParameterArray(optionSheet, option, startrow);

                this.ApplySheetSettings(optionSheet);

                ParameterSheetUtilities.ApplyLocking(optionSheet, true);
            }
        }

        /// <summary>
        /// Collect the information that is to be written to the header of the option sheet
        /// </summary>
        /// <param name="optionSheet">
        /// The <see cref="Worksheet"/> to which the option data is written
        /// </param>
        /// <param name="option">
        /// The <see cref="Option"/> for which the arrays are to be populated
        /// </param>
        /// <returns>
        /// The row number that is the last row on the sheet containing header content
        /// </returns>
        private int PopulateAndWriteHeaderArray(Worksheet optionSheet, Option option)
        {
            var headerArrayAssembler = new HeaderArrayAssembler(this.session, this.iteration, this.participant, option);
            var headerFormat = headerArrayAssembler.FormatArray;
            var headerContent = headerArrayAssembler.HeaderArray;
            var headerLock = headerArrayAssembler.LockArray;

            var numberOfRows = headerContent.GetLength(0);
            var numberOfColumns = headerContent.GetLength(1);

            var range = optionSheet.Range(optionSheet.Cells[1, 1], optionSheet.Cells[numberOfRows, numberOfColumns]);
            range.HorizontalAlignment = XlHAlign.xlHAlignLeft;
            range.NumberFormat = headerFormat;
            range.Locked = headerLock;

            range.Name = OptionSheetConstants.OptionHeaderName;
            range.Value = headerContent;
            range.Interior.ColorIndex = 8;

            var endRow = headerContent.GetLength(0);
            return endRow;
        }

        /// <summary>
        /// Collect the information that is to be written to the Parameter sheet
        /// </summary>
        /// <param name="optionSheet">
        /// The <see cref="Worksheet"/> to which the option data is written
        /// </param>
        /// <param name="option">
        /// The <see cref="Option"/> for which the arrays are to be populated
        /// </param>
        /// <param name="startRow">
        /// The row number at which the content needs to be written
        /// </param>
        private void PopulateAndWriteParameterArray(Worksheet optionSheet, Option option, int startRow)
        {
            var owner = this.session.QuerySelectedDomainOfExpertise(this.iteration);

            if (owner != null)
            {
                Logger.Debug("Option Sheet Generator Owner: {0}:{1}:{2}", owner.ShortName, owner.Name, owner.Iid);
            }
            
            var assembler = new OptionSheetRowAssembler(this.iteration, option, owner);
            assembler.Assemble();
            var excelRows = assembler.ExcelRows;

            var nestedElementParameterArrayAssembler = new NestedElementParameterArrayAssembler(excelRows);
            var contentArray = nestedElementParameterArrayAssembler.ContentArray;
            var formatArray = nestedElementParameterArrayAssembler.FormatArray;
            var formulaArray = nestedElementParameterArrayAssembler.FormulaArray;
            var lockArray = nestedElementParameterArrayAssembler.LockArray;

            var numberOfRows = contentArray.GetLength(0);
            var numberOfColumns = contentArray.GetLength(1);
            var endrow = startRow + numberOfRows - 1;

            var nestedElementRange = optionSheet.Range(optionSheet.Cells[startRow, 1], optionSheet.Cells[endrow, numberOfColumns]);

            nestedElementRange.NumberFormat = formatArray;
            nestedElementRange.Value = contentArray;
            nestedElementRange.Locked = lockArray;

            var formattedrange = optionSheet.Range(optionSheet.Cells[startRow - 1, 1], optionSheet.Cells[startRow, numberOfColumns]);
            formattedrange.Interior.ColorIndex = 34;
            formattedrange.HorizontalAlignment = XlHAlign.xlHAlignCenter;
            formattedrange.Font.Bold = true;
            formattedrange.Font.Underline = true;
            formattedrange.Font.Size = 11;

            this.ApplyCellNames(optionSheet, startRow, endrow);

            optionSheet.Cells[startRow + 1, 1].Select();
            this.excelApplication.ActiveWindow.FreezePanes = true;
        }

        /// <summary>
        /// apply cell names to the actual value column
        /// </summary>
        /// <param name="beginRow">
        /// The row at which the range begins
        /// </param>
        /// <param name="endRow">
        /// The row at which the range ends
        /// </param>
        private void ApplyCellNames(Worksheet optionSheet, int beginRow, int endRow)
        {
            try
            {
                var range =
                    optionSheet.Range(
                        optionSheet.Cells[endRow, OptionSheetConstants.ModelCodeColumn],
                        optionSheet.Cells[beginRow + 2, OptionSheetConstants.ActualValueColumn]
                        );

                range.CreateNames(top: false, left: true, bottom: false, right: false);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Could not apply cell names");
            }
        }

        /// <summary>
        /// Apply formatting settings to the option sheet
        /// </summary>
        /// <param name="optionSheet">
        /// The <see cref="Worksheet"/> to which the sheet settings are applied
        /// </param>
        private void ApplySheetSettings(Worksheet optionSheet)
        {
            var outline = optionSheet.Outline;
            outline.AutomaticStyles = false;
            outline.SummaryRow = XlSummaryRow.xlSummaryAbove;
            outline.SummaryColumn = XlSummaryColumn.xlSummaryOnRight;

            optionSheet.Cells.Select();
            optionSheet.Cells.EntireColumn.AutoFit();

            optionSheet.Rows.RowHeight = 12.75;

            optionSheet.Columns[OptionSheetConstants.IdColumn].EntireColumn.OutlineLevel = 2;
            optionSheet.Columns[OptionSheetConstants.ModelCodeColumn].EntireColumn.OutlineLevel = 2;
            optionSheet.Outline.ShowLevels(8, 1);

            this.excelApplication.ActiveWindow.DisplayGridlines = false;
        }
    }
}