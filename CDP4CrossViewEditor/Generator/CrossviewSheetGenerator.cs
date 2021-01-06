
namespace CDP4CrossViewEditor.Generator
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.ViewModels;

    using CDP4CrossViewEditor.RowModels;

    using NetOffice.ExcelApi;
    using NetOffice.ExcelApi.Enums;

    using NLog;

    using Application = NetOffice.ExcelApi.Application;
    using Exception = System.Exception;

    using CDP4Dal;
    using CDP4CrossViewEditor.Assemblers;

    /// <summary>
    /// The purpose of the <see cref="CrossviewSheetGenerator"/> is to generate in Excel
    /// the Crossview sheet that contains the ElementDefinitions, Parameters, ParameterOverrides, and Subscriptions
    /// of the <see cref="DomainOfExpertise"/> of the active <see cref="Participant"/>.
    /// </summary>
    public class CrossviewSheetGenerator
    {
        /// <summary>
        /// the string that is used as the list separator.
        /// </summary>
        private string listSeparator;

        /// <summary>
        /// The NLog logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

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
        /// The country code of the application (language version)
        /// </summary>
        private int xlCountryCode;

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
        /// The array that contains the content of the parameter section of the parameter sheet
        /// </summary>
        private object[,] parameterContent;

        /// <summary>
        /// The array that contains the lock settings of the parameter section of the parameter sheet
        /// </summary>
        private object[,] parameterLock;

        /// <summary>
        /// A string that contains the switch values used as validation formula
        /// </summary>
        private string switchFormula;

        /// <summary>
        /// Gets the <see cref="ISession"/> that is active
        /// </summary>
        private ISession session;

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
        /// <param name="processedValueSets"></param>
        public void Rebuild(Application application, Workbook workbook, IEnumerable<ElementDefinition> elementDefinitions,
            IEnumerable<ParameterType> parameterTypes)
        {
            var sw = new Stopwatch();
            sw.Start();

            this.excelApplication = application;

            this.excelApplication.StatusBar = "Rebuilding Crossview Sheet";

            this.listSeparator = (string)application.International(XlApplicationInternational.xlListSeparator);
            this.xlCountryCode = Convert.ToInt32(application.International(XlApplicationInternational.xlCountryCode));
            this.SetLanguageSpecificVariables();
            this.SetSwitchString();

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

                this.crossviewSheet = CrossviewSheetUtilities.RetrieveParameterSheet(workbook, true);

                CrossviewSheetUtilities.ApplyLocking(this.crossviewSheet, false);

                this.PopulateSheetArrays(elementDefinitions, parameterTypes);
                this.WriteParameterSheet();
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
        /// Sets the language specific variables used by the parameter sheet generator
        /// </summary>
        private void SetLanguageSpecificVariables()
        {
        }

        /// <summary>
        /// Set the <see cref="switchFormula"/> that is used as formula for cell validation
        /// </summary>
        private void SetSwitchString()
        {
            this.switchFormula = string.Join(this.listSeparator, Enum.GetNames(typeof(ParameterSwitchKind)));
        }

        /// <summary>
        /// collect the information that is to be written to the Parameter sheet
        /// </summary>
        /// <param name="processedValueSets">IReadOnlyDictionary<Guid, ProcessedValueSet> processedValueSets</param>
        private void PopulateSheetArrays(IEnumerable<ElementDefinition> elementDefinitions, IEnumerable<ParameterType> parameterTypes)
        {
            var selectedDomainOfExpertise = this.session.QuerySelectedDomainOfExpertise(this.iteration);

            // Instantiate the different rows
            var assembler = new CrossviewSheetRowAssembler(this.iteration, selectedDomainOfExpertise);
            assembler.Assemble(elementDefinitions);
            this.excelRows = assembler.ExcelRows;

            // Use the instantiated rows to populate the excel array
            var parameterArrayAssembler = new CrossviewArrayAssembler(this.excelRows, elementDefinitions, parameterTypes);
            this.parameterContent = parameterArrayAssembler.ContentArray;
            this.parameterFormat = parameterArrayAssembler.FormatArray;
            this.parameterLock = parameterArrayAssembler.LockArray;

            var headerArrayAssembler = new CrossviewHeaderArrayAssembler(this.session, this.iteration, this.participant);
            this.headerFormat = headerArrayAssembler.FormatArray;
            this.headerContent = headerArrayAssembler.HeaderArray;
            this.headerLock = headerArrayAssembler.LockArray;
        }

        /// <summary>
        /// Write the data to the Parameter sheet
        /// </summary>
        private void WriteParameterSheet()
        {
            this.WriteHeader();
            this.WriteParameters();
            this.UpdateParameterCells();
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
        }

        /// <summary>
        /// Write the content of the <see cref="parameterContent"/> to the parameter sheet
        /// </summary>
        private void WriteParameters()
        {
            var numberOfRows = this.parameterContent.GetLength(0);
            var numberOfColumns = this.parameterContent.GetLength(1);

            var startrow = this.headerContent.GetLength(0) + 2;
            var endrow = startrow + numberOfRows - 1;

            var parameterRange = this.crossviewSheet.Range(this.crossviewSheet.Cells[startrow, 1], this.crossviewSheet.Cells[endrow, numberOfColumns]);
            parameterRange.Name = "Crossview";

            Console.WriteLine(parameterRange.Rows.Count);
            Console.WriteLine(this.parameterContent.GetLength(0));

            parameterRange.NumberFormat = this.parameterFormat;
            parameterRange.Value = this.parameterContent;
            parameterRange.Locked = this.parameterLock;

            var formattedrange = this.crossviewSheet.Range(this.crossviewSheet.Cells[startrow - 1, 1], this.crossviewSheet.Cells[startrow, numberOfColumns]);
            formattedrange.Interior.ColorIndex = 34;
            formattedrange.HorizontalAlignment = XlHAlign.xlHAlignCenter;
            formattedrange.Font.Bold = true;
            formattedrange.Font.Underline = true;
            formattedrange.Font.Size = 11;

            this.ApplyCellNames(startrow, endrow);

            this.crossviewSheet.Cells[startrow + 1, 1].Select();
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
        private void ApplyCellNames(int beginRow, int endRow)
        {
            try
            {
                var range =
                    this.crossviewSheet.Range(
                        this.crossviewSheet.Cells[beginRow + 2, CrossviewSheetConstants.ActualValueColumn],
                        this.crossviewSheet.Cells[endRow, CrossviewSheetConstants.ModelCodeColumn]);

                range.CreateNames(top: false, left: false, bottom: false, right: true);
            }
            catch (Exception ex)
            {
                logger.Error("Could not apply cell names", ex);
            }
        }

        /// <summary>
        /// Apply row validation using the information from the <see cref="IExcelRow{Thing}"/> and
        /// </summary>
        /// <param name="excelRow">
        /// An instance of <see cref="IExcelRow{Thing}"/> that contains the information used to apply the proper validation
        /// </param>
        /// <param name="rownumber">
        /// The number of the row in the parameter sheet
        /// </param>
        private void ApplyRowValidation(IExcelRow<Thing> excelRow, int rownumber)
        {
        }

        /// <summary>
        /// Apply grouping to the excel rows to allow expanding and collapsing in excel
        /// </summary>
        /// <param name="excelRow">
        /// The <see cref="IExcelRow{Thing}"/> that contains the level information
        /// </param>
        /// <param name="rownumber">
        /// The row in excel that needs the grouping to be applied
        /// </param>
        private void ApplyRowGrouping(IExcelRow<Thing> excelRow, int rownumber)
        {
            //var outlineLevel = excelRow.Level + 1;

            //this.crossviewSheet.Cells[rownumber, 14].Value = outlineLevel;

            //if (outlineLevel < 8)
            //{
            //    this.crossviewSheet.Rows[rownumber].EntireRow.OutlineLevel = outlineLevel;
            //}
            //else
            //{
            //    this.crossviewSheet.Rows[rownumber].EntireRow.OutlineLevel = 8;
            //}
        }

        /// <summary>
        /// Apply the formula to the actual value column
        /// </summary>
        /// <param name="excelRow">
        /// The <see cref="IExcelRow{Thing}"/> that contains the level information
        /// </param>
        /// <param name="rownumber">
        /// The row in excel that needs the formula applied
        /// </param>
        private void ApplyActualValueFormula(IExcelRow<Thing> excelRow, int rownumber)
        {
        }

        /// <summary>
        /// Apply the formula to the computed-column
        /// </summary>
        /// <param name="excelRow">
        /// The <see cref="IExcelRow{Thing}"/> that contains the formula that is to be applied
        /// </param>
        /// <param name="rownumber">
        /// The row in excel that needs the formula applied
        /// </param>
        private void ApplyFormulaValueToComputedValueCell(IExcelRow<Thing> excelRow, int rownumber)
        {
        }

        /// <summary>
        /// Update the content of specific parameter related cells
        /// </summary>
        private void UpdateParameterCells()
        {
            var row = this.headerContent.GetLength(0) + 4;

            foreach (var excelRow in this.excelRows)
            {
                this.ApplyRowValidation(excelRow, row);
                this.ApplyRowGrouping(excelRow, row);
                this.ApplyActualValueFormula(excelRow, row);
                this.ApplyFormulaValueToComputedValueCell(excelRow, row);

                row++;
            }
        }
    }
}
