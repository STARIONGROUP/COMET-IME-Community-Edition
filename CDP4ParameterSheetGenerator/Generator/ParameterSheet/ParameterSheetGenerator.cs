// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterSheetGenerator.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2020 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Cozmin Velciu, Adrian Chivu
//
//    This file is part of CDP4-IME Community Edition.
//    The CDP4-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4ParameterSheetGenerator.ParameterSheet
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Validation;

    using CDP4Composition.ViewModels;

    using CDP4ParameterSheetGenerator.RowModels;
    
    using NetOffice.ExcelApi;
    using NetOffice.ExcelApi.Enums;
    
    using NLog;

    using Application = NetOffice.ExcelApi.Application;
    using Exception = System.Exception;
    
    using CDP4Dal;

    using CDP4ParameterSheetGenerator.Generator;

    using Range = NetOffice.ExcelApi.Range;

    /// <summary>
    /// The purpose of the <see cref="ParameterSheetGenerator"/> is to generate in Excel
    /// the Parameter sheet that contains the Parameters, ParameterOverrides, and Subscriptions
    /// of the <see cref="DomainOfExpertise"/> of the active <see cref="Participant"/>.
    /// </summary>
    public class ParameterSheetGenerator
    {
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
        /// The conditional function in the language of the excel application (IF, WENN...)
        /// </summary>
        private string conditionalFunction;

        /// <summary>
        /// The value of the column excel variable in the language of the application
        /// </summary>
        private string columnMarker;

        /// <summary>
        /// The value of the row excel variable in the language of the application
        /// </summary>
        private string rowMarker;

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
        /// The <see cref="IExcelRow{Thing}"/> that make up the content of the parameter sheet
        /// </summary>
        private IEnumerable<IExcelRow<Thing>> excelRows;

        /// <summary>
        /// The <see cref="Worksheet"/> that the parameters are written to.
        /// </summary>
        private Worksheet parameterSheet;

        /// <summary>
        /// The array that contains the content of the header section of the parameter sheet
        /// </summary>
        private object[,] headerContent;

        /// <summary>
        /// The array that contains the lock settings of the header section of the parameter sheet
        /// </summary>
        private object[,] headerLock;

        /// <summary>
        /// The array that contains the formatting settings of the header section of the parameter sheet
        /// </summary>
        private object[,] headerFormat;

        /// <summary>
        /// The array that contains the formatting of the parameter section of the parameter sheet
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
        /// Initializes a new instance of the <see cref="ParameterSheetGenerator"/> class. 
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
        public ParameterSheetGenerator(ISession session, Iteration iteration, Participant participant)
        {
            this.session = session;
            this.iteration = iteration;
            this.participant = participant;
        }

        /// <summary>
        /// rebuild the parameter sheet, replace it if it already exists.
        /// </summary>
        /// <param name="application">
        /// The excel application object that contains the <see cref="Workbook"/> in which the parameter sheet is to be rebuilt.
        /// </param>
        /// <param name="workbook">
        /// The <see cref="Workbook"/> in which the parameter sheet is to be rebuilt.
        /// </param>
        /// <param name="clones">
        /// The <see cref="Thing"/>s that have been changed on the Parameter sheet that need to be kept
        /// </param>
        public void Rebuild(Application application, Workbook workbook, IReadOnlyDictionary<Guid, ProcessedValueSet> processedValueSets)
        {
            var sw = new Stopwatch();
            sw.Start();

            this.excelApplication = application;

            this.excelApplication.StatusBar = "Rebuilding Parameter Sheet";

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

                this.parameterSheet = ParameterSheetUtilities.RetrieveParameterSheet(workbook, true);

                ParameterSheetUtilities.ApplyLocking(this.parameterSheet, false);

                this.PopulateSheetArrays(processedValueSets);
                this.WriteParameterSheet();
                this.ApplySheetSettings();

                ParameterSheetUtilities.ApplyLocking(this.parameterSheet, true);

                this.excelApplication.StatusBar = $"CDP4: Parameter Sheet rebuilt in {sw.ElapsedMilliseconds} [ms]";
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
            var outline = this.parameterSheet.Outline;
            outline.AutomaticStyles = false;
            outline.SummaryRow = XlSummaryRow.xlSummaryAbove;
            outline.SummaryColumn = XlSummaryColumn.xlSummaryOnRight;

            this.parameterSheet.Cells.Select();
            this.parameterSheet.Cells.EntireColumn.AutoFit();

            this.parameterSheet.Rows.RowHeight = 12.75;
            this.parameterSheet.Columns[ParameterSheetConstants.SwitchColumn].ColumnWidth = 10.30;

            this.parameterSheet.Columns[ParameterSheetConstants.IdColumn].EntireColumn.OutlineLevel = 2;
            this.parameterSheet.Columns[ParameterSheetConstants.ShortNameColumn].EntireColumn.OutlineLevel = 2;
            this.parameterSheet.Outline.ShowLevels(8, 1);

            this.excelApplication.ActiveWindow.DisplayGridlines = false;       
        }

        /// <summary>
        /// Sets the language specific variables used by the parameter sheet generator
        /// </summary>
        private void SetLanguageSpecificVariables()
        {
            switch (this.xlCountryCode)
            {
                case 1: // english
                    this.conditionalFunction = "IF";
                    this.rowMarker = "R";
                    this.columnMarker = "C";
                    break;
                case 7: // russian
                    this.conditionalFunction = "ЕСЛИ";
                    this.rowMarker = "R";
                    this.columnMarker = "C";
                    break;
                case 31: // dutch
                    this.conditionalFunction = "ALS";
                    this.rowMarker = "R";
                    this.columnMarker = "K";
                    break;
                case 33: // french
                    this.conditionalFunction = "SI";
                    this.rowMarker = "L";
                    this.columnMarker = "C";
                    break;
                case 39: // italian
                    this.conditionalFunction = "SE";
                    this.rowMarker = "R";
                    this.columnMarker = "C";
                    break;
                case 49: // german
                    this.conditionalFunction = "WENN";
                    this.rowMarker = "Z";
                    this.columnMarker = "S";
                    break;
                case 55: // portuguese-brazil
                    this.conditionalFunction = "SE";
                    this.rowMarker = "L";
                    this.columnMarker = "C";
                    break;
                case 351: // portuguese-portugal
                    this.conditionalFunction = "SE";
                    this.rowMarker = "L";
                    this.columnMarker = "C";
                    break;
                default:
                    this.conditionalFunction = "IF";
                    this.rowMarker = "R";
                    this.columnMarker = "C";
                    logger.Info("The Parameter Sheet Generator does not support the following country code {0}", this.xlCountryCode);
                    break;
            }
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
        /// <param name="clones">
        /// The <see cref="Thing"/>s that have been changed on the Parameter sheet that need to be kept
        /// </param>
        private void PopulateSheetArrays(IReadOnlyDictionary<Guid, ProcessedValueSet> processedValueSets)
        {
            var selectedDomainOfExpertise = this.session.QuerySelectedDomainOfExpertise(this.iteration);
            
            // Instantiate the different rows
            var assembler = new ParameterSheetRowAssembler(this.iteration, selectedDomainOfExpertise);
            assembler.Assemble(processedValueSets);
            this.excelRows = assembler.ExcelRows;

            // Use the instantiated rows to populate the excel array
            var parameterArrayAssembler = new ParameterArrayAssembler(this.excelRows);
            this.parameterContent = parameterArrayAssembler.ContentArray;
            this.parameterFormat = parameterArrayAssembler.FormatArray;
            this.parameterLock = parameterArrayAssembler.LockArray;

            var headerArrayAssembler = new HeaderArrayAssembler(this.session, this.iteration, this.participant);
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

            var range = this.parameterSheet.Range(this.parameterSheet.Cells[1, 1], this.parameterSheet.Cells[numberOfRows, numberOfColumns]);
            range.HorizontalAlignment = XlHAlign.xlHAlignLeft;
            range.NumberFormat = this.headerFormat;
            range.Locked = this.headerLock;

            range.Name = ParameterSheetConstants.ParameterHeaderName;
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

            var parameterRange = this.parameterSheet.Range(this.parameterSheet.Cells[startrow, 1], this.parameterSheet.Cells[endrow, numberOfColumns]);
            parameterRange.Name = ParameterSheetConstants.ParameterRangeName;
            
            Console.WriteLine(parameterRange.Rows.Count);
            Console.WriteLine(this.parameterContent.GetLength(0));

            parameterRange.NumberFormat = this.parameterFormat;
            parameterRange.Value = this.parameterContent;
            parameterRange.Locked = this.parameterLock;
            
            var formattedrange = this.parameterSheet.Range(this.parameterSheet.Cells[startrow - 1, 1], this.parameterSheet.Cells[startrow, numberOfColumns]);
            formattedrange.Interior.ColorIndex = 34;
            formattedrange.HorizontalAlignment = XlHAlign.xlHAlignCenter;
            formattedrange.Font.Bold = true;
            formattedrange.Font.Underline = true;
            formattedrange.Font.Size = 11;

            this.ApplyCellNames(startrow, endrow);

            this.parameterSheet.Cells[startrow + 1, 1].Select();
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
                    this.parameterSheet.Range(
                        this.parameterSheet.Cells[beginRow + 2, ParameterSheetConstants.ActualValueColumn],
                        this.parameterSheet.Cells[endRow, ParameterSheetConstants.ModelCodeColumn]);

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
            Validation validation;

            if (excelRow is ComponentExcelRow)
            {
                var componentnumber = excelRow.Id.Split(':');
                var parameterRowNumber = (rownumber - 1) - int.Parse(componentnumber[1]);

                this.parameterSheet.Cells[rownumber, ParameterSheetConstants.SwitchColumn].FormulaR1C1 = string.Format("=R{0}C{1}", parameterRowNumber, ParameterSheetConstants.SwitchColumn);                
            }
            else
            {
                // switch            
                if (excelRow.Type != ParameterSheetConstants.ED && excelRow.Type != ParameterSheetConstants.PG)
                {
                    validation = this.parameterSheet.Cells[rownumber, ParameterSheetConstants.SwitchColumn].Validation;
                    ApplyValidation(validation, this.switchFormula);
                }
            }

            if (excelRow.ParameterType == null)
            {
                logger.Debug("The parametertype of excelrow at address {0} is not specified: ", rownumber);
            }

            var booleanParameterType = excelRow.ParameterType as BooleanParameterType;
            if (booleanParameterType != null)
            {
                var formula = string.Format("-{0}FALSE{0}TRUE", this.listSeparator);

                // manual value
                validation = this.parameterSheet.Cells[rownumber, ParameterSheetConstants.ManualColumn].Validation;
                ApplyValidation(validation, formula);

                // reference value
                validation = this.parameterSheet.Cells[rownumber, ParameterSheetConstants.ReferenceColumn].Validation;
                ApplyValidation(validation, formula);
            }

            var enumerationParameterType = excelRow.ParameterType as EnumerationParameterType;
            if (enumerationParameterType != null)
            {
                var formula = this.ValueDefinitionValidationFormula(enumerationParameterType);
                if (formula != string.Empty)
                {
                    // manual value
                    validation = this.parameterSheet.Cells[rownumber, ParameterSheetConstants.ManualColumn].Validation;
                    ApplyValidation(validation, formula);

                    // reference value
                    validation = this.parameterSheet.Cells[rownumber, ParameterSheetConstants.ReferenceColumn].Validation;
                    ApplyValidation(validation, formula);
                }
                else
                {
                    var range = this.parameterSheet.Cells[rownumber, ParameterSheetConstants.ManualColumn];
                    ApplyComment(range, "CDP4: /r/n/ No ManualValue Definitions have been defined /r/n Please update The Parameter Type", false);
                }
            }
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
            var outlineLevel = excelRow.Level + 1;

            this.parameterSheet.Cells[rownumber, 14].Value = outlineLevel;

            if (outlineLevel < 8)
            {
                this.parameterSheet.Rows[rownumber].EntireRow.OutlineLevel = outlineLevel;
            }
            else
            {
                this.parameterSheet.Rows[rownumber].EntireRow.OutlineLevel = 8;
            }
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
            if (excelRow is ElementDefinitionExcelRow || excelRow is ElementUsageExcelRow || excelRow is ParameterGroupExcelRow || excelRow is ParameterSubscriptionValuesetExcelRow)
            {
                return;
            }

            var parameterRow = excelRow as ParameterExcelRow;
            if (parameterRow != null && parameterRow.Thing.ParameterType is CompoundParameterType)
            {
                return;
            }

            var parameterValueSetRow = excelRow as ParameterValueSetExcelRow;
            if (parameterValueSetRow != null)
            {
                var parameter = (CDP4Common.EngineeringModelData.Parameter)parameterValueSetRow.Thing.Container;
                if (parameter.ParameterType is CompoundParameterType)
                {
                    return;
                }
            }

            var switchaddress = string.Format("R{0}C{1}", rownumber, ParameterSheetConstants.SwitchColumn);
            var manualAddress = string.Format("R{0}C{1}", rownumber, ParameterSheetConstants.ManualColumn);
            var computedAddress = string.Format("R{0}C{1}", rownumber, ParameterSheetConstants.ComputedColumn);
            var referenceAddress = string.Format("R{0}C{1}", rownumber, ParameterSheetConstants.ReferenceColumn);

            var formula = string.Format("={0}({1}=\"MANUAL\"{5}{2}{5}{0}({1}=\"COMPUTED\"{5}{3}{5}{4}))", this.conditionalFunction, switchaddress, manualAddress, computedAddress, referenceAddress, this.listSeparator);

            try
            {
                this.parameterSheet.Cells[rownumber, ParameterSheetConstants.ActualValueColumn].FormulaR1C1Local = formula;            
            }
            catch (Exception ex)
            {
                logger.Error(ex, "The formula {0} could not be written", formula);
            }            
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
            if (excelRow is ElementDefinitionExcelRow || excelRow is ElementUsageExcelRow || excelRow is ParameterGroupExcelRow)
            {
                return;
            }

            var parameterRow = excelRow as ParameterExcelRow;
            if (parameterRow != null && parameterRow.Thing.ParameterType is CompoundParameterType)
            {
                return;
            }

            var parameterValueSetRow = excelRow as ParameterValueSetExcelRow;
            if (parameterValueSetRow != null)
            {
                var parameter = (CDP4Common.EngineeringModelData.Parameter)parameterValueSetRow.Thing.Container;
                if (parameter.ParameterType is CompoundParameterType)
                {
                    return;
                }
            }

            var subscriptionValueSetRow = excelRow as ParameterSubscriptionExcelRow;
            if (subscriptionValueSetRow != null)
            {
                return;
            }

            var parameterSubscriptionValueSetExcelRow = excelRow as ParameterSubscriptionValuesetExcelRow;
            if (parameterSubscriptionValueSetExcelRow != null)
            {
                return;
            }

            var componentExcelRow = excelRow as ComponentExcelRow;
            if (componentExcelRow != null)
            {
                if (componentExcelRow.Container is ParameterSubscriptionExcelRow || componentExcelRow.Container is ParameterSubscriptionValuesetExcelRow)
                {
                    return;
                }
            }

            try
            {
                this.parameterSheet.Cells[rownumber, ParameterSheetConstants.ComputedColumn].Formula = excelRow.Formula;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "The formula {0} could not be written", excelRow.Formula);
            }  
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

        /// <summary>
        /// Update the validation object with the provided formula
        /// </summary>
        /// <param name="validation">
        /// The <see cref="Validation"/> object of a specific cell
        /// </param>
        /// <param name="formula">
        /// The string that contains the content of the validation object
        /// </param>
        private static void ApplyValidation(Validation validation, string formula)
        {
            validation.Delete();
            validation.Add(XlDVType.xlValidateList, XlDVAlertStyle.xlValidAlertStop, XlFormatConditionOperator.xlBetween, formula);
            validation.InCellDropdown = true;
        }

        /// <summary>
        /// Apply a comment to the provided range
        /// </summary>
        /// <param name="range">
        /// A <see cref="Range"/> object to which the comment has to be applied
        /// </param>
        /// <param name="comment">
        /// The content of the comment
        /// </param>
        /// <param name="visible">
        /// a value indicating whether the comment should be visible or not
        /// </param>
        private static void ApplyComment(Range range, string comment, bool visible = false)
        {
            range.ClearComments();
            range.AddComment(comment);
            range.Comment.Visible = visible;
        }

        /// <summary>
        /// Queries the <see cref="EnumerationValueDefinition"/> of the provided <see cref="EnumerationParameterType"/>
        /// and returns a formula containing the short-names as a string 
        /// </summary>
        /// <param name="enumerationParameterType">
        /// The <see cref="EnumerationParameterType"/> for which the <see cref="EnumerationValueDefinition"/> short-names are queried
        /// </param>
        /// <returns>
        /// a string containing the short-names of the <see cref="EnumerationValueDefinition"/>s, the string will be empty if the 
        /// <see cref="EnumerationParameterType"/> does not contain any <see cref="EnumerationValueDefinition"/>s
        /// </returns>
        private string ValueDefinitionValidationFormula(EnumerationParameterType enumerationParameterType)
        {
            if (enumerationParameterType.ValueDefinition.Count != 0)
            {
                var valueDefinitions = new List<string> { "-" };
                foreach (var enumerationValueDefinition in enumerationParameterType.ValueDefinition)
                {
                    valueDefinitions.Add(((EnumerationValueDefinition)enumerationValueDefinition).ShortName);
                }

                return string.Join(this.listSeparator, valueDefinitions.ToArray());
            }

            return string.Empty;
        }
    }
}
