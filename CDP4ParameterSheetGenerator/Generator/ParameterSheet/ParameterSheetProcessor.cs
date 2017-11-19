// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterSheetProcessor.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ParameterSheetGenerator.Generator
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Validation;
    using CDP4Dal;
    using CDP4OfficeInfrastructure;
    using CDP4OfficeInfrastructure.Excel;

    using CDP4ParameterSheetGenerator.Generator.ParameterSheet;
    using CDP4ParameterSheetGenerator.ParameterSheet;

    using DevExpress.Mvvm.POCO;

    using NetOffice.ExcelApi;
    using NetOffice.ExcelApi.Enums;
    using NLog;

    using Application = NetOffice.ExcelApi.Application;
    using Exception = System.Exception;

    /// <summary>
    /// The purpose <see cref="ParameterSheetProcessor"/> is to process the contents of the Parameter sheet
    /// </summary>
    public class ParameterSheetProcessor
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The <see cref="Worksheet"/> that the parameters are written to.
        /// </summary>
        private Worksheet parameterSheet;

        /// <summary>
        /// The array that contains the content of the parameter section of the parameter sheet
        /// </summary>
        private object[,] parameterContent;

        /// <summary>
        /// The array that contains the formula of the parameter section of the parameter sheet
        /// </summary>
        private object[,] parameterFormula;
        
        /// <summary>
        /// The <see cref="ISession"/> that is specific to the <see cref="Workbook"/> that is being processed.
        /// </summary>
        private readonly ISession workbookSession;

        /// <summary>
        /// The <see cref="Iteration"/> for the current <see cref="ParameterSheetProcessor"/>
        /// </summary>
        private readonly Iteration iteration;

        /// <summary>
        /// A string that holds an error message that occurred while processing the sheet.
        /// </summary>
        private string errorMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterSheetProcessor"/> class. 
        /// </summary>       
        /// <param name="workbookSession">
        /// An instance of <see cref="ISession"/> that is specific to the <see cref="Workbook"/>.
        /// </param>
        /// <param name="iteration">
        /// The current <see cref="Iteration"/>
        /// </param>
        public ParameterSheetProcessor(ISession workbookSession, Iteration iteration)
        {
            this.workbookSession = workbookSession;
            this.iteration = iteration;
        }

        /// <summary>
        /// Validate the values on the Parameter sheet and check for any changes that have been made by the user
        /// </summary>
        /// <param name="application">
        /// The excel application object that contains the <see cref="Workbook"/> in which the parameter sheet is to be processed.
        /// </param>
        /// <param name="workbook">
        /// The <see cref="Workbook"/> that contains the Parameter sheet that is being processed
        /// </param>
        /// <param name="processedValueSets">
        /// A <see cref="List{ProcessedValueSet}"/> of clones that capture the updated <see cref="Thing"/>s with its value validation result
        /// </param>
        /// <remarks>
        /// Changed cells will be highlighted, cells containing invalid or out-of-bounds data will be marked.
        /// </remarks>
        public void ValidateValuesAndCheckForChanges(Application application, Workbook workbook, out IReadOnlyDictionary<Guid, ProcessedValueSet> processedValueSets)
        {
            var sw = new Stopwatch();
            sw.Start();
            
            application.Cursor = XlMousePointer.xlWait;

            application.StatusBar = "Processing Parameter sheet";

            this.parameterSheet = ParameterSheetUtilities.RetrieveParameterSheet(workbook);
            
            var temporaryProcessedValueSets = new Dictionary<Guid, ProcessedValueSet>();
            
            var numberFormatInfo = this.QuerayNumberFormatInfo(application);
            
            try
            {
                ParameterSheetUtilities.ApplyLocking(this.parameterSheet, false);

                var parameterRange = this.parameterSheet.Range(ParameterSheetConstants.ParameterRangeName);

                this.parameterContent = (object[,])parameterRange.Value;
                this.parameterFormula = (object[,])parameterRange.Formula;

                var currentRow = parameterRange.Row;

                application.StatusBar = string.Format("Processing Parameter sheet - row: {0}", currentRow);

                var parameterContentRows = this.parameterContent.GetLength(0) + 1;

                for (var i = 1; i < parameterContentRows; i++)
                {
                    var rowType = string.Empty;

                    try
                    {
                        rowType = (string)this.parameterContent[i, ParameterSheetConstants.TypeColumn];
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex);
                    }

                    if (rowType != string.Empty
                        && (rowType == ParameterSheetConstants.PVS || rowType == ParameterSheetConstants.PVSCT
                            || rowType == ParameterSheetConstants.POVS || rowType == ParameterSheetConstants.POVSCT
                            || rowType == ParameterSheetConstants.PSVS || rowType == ParameterSheetConstants.PSVSCT))
                    {
                        var rowIid = Convert.ToString(this.parameterContent[i, ParameterSheetConstants.IdColumn]);

                        var computedValue = this.QueryComputedValue(i);
                        var formulaValue = this.QueryFormulaValue(i);
                        var manualValue = this.QueryManualValue(i);
                        var referenceValue = this.QueryReferenceValue(i);
                        var switchValue = this.QuerySwitchValue(i);                        
                        var actualValue = this.QueryActualValue(i);

                        var rowIidChar = rowIid.Split(':');
                        var thingIid = rowIidChar[0];
                        var componentIndex = 0;

                        if (rowIidChar.Length > 1)
                        {
                            componentIndex = int.Parse(rowIidChar[1]);
                        }

                        var lazyThing = this.workbookSession.Assembler.Cache.Select(item => item.Value).SingleOrDefault(item => item.Value.Iid == Guid.Parse(thingIid));
                        if (lazyThing != null)
                        {
                            var thing = lazyThing.Value;
                        
                            if (rowType == ParameterSheetConstants.PVS || rowType == ParameterSheetConstants.PVSCT)
                            {
                                var parameterValueSet = (ParameterValueSet)thing;
                                this.ProcessValueSet(parameterValueSet, componentIndex, currentRow, manualValue, computedValue, referenceValue, actualValue, switchValue, formulaValue, ref temporaryProcessedValueSets, numberFormatInfo);
                            }

                            if (rowType == ParameterSheetConstants.POVS || rowType == ParameterSheetConstants.POVSCT)
                            {
                                var parameterOverrideValueSet = (ParameterOverrideValueSet)thing;
                                this.ProcessValueSet(parameterOverrideValueSet, componentIndex, currentRow, manualValue, computedValue, referenceValue, actualValue, switchValue, formulaValue, ref temporaryProcessedValueSets, numberFormatInfo);
                            }

                            if (rowType == ParameterSheetConstants.PSVS || rowType == ParameterSheetConstants.PSVSCT)
                            {
                                var parameterSubscriptionValueSet = (ParameterSubscriptionValueSet)thing;
                                this.ProcessValueSet(parameterSubscriptionValueSet, componentIndex, currentRow, manualValue, switchValue, ref temporaryProcessedValueSets, numberFormatInfo);
                            }
                        }
                        else
                        {
                            logger.Warn("The Thing of RowType {0} with unique id {1} could not be found in the workbook cache", rowType, thingIid);
                        }
                    }

                    currentRow++;
                }

                ParameterSheetUtilities.ApplyLocking(this.parameterSheet, true);
            }
            catch (Exception ex)
            {
                this.errorMessage = ex.Message;
                logger.Error(ex);
            }
            finally
            {
                application.Cursor = XlMousePointer.xlDefault;

                sw.Stop();

                if (string.IsNullOrEmpty(this.errorMessage))
                {
                    application.StatusBar = string.Format("CDP4: Parameter sheet processed in {0} [ms]", sw.ElapsedMilliseconds);
                }
                else
                {
                    application.StatusBar = string.Format("CDP4: The following error occured while processing the sheet: {0}", this.errorMessage);
                }

                processedValueSets = new Dictionary<Guid, ProcessedValueSet>(temporaryProcessedValueSets);
            }   
        }
        
        /// <summary>
        /// Queries the appropriate <see cref="NumberFormatInfo"/> based on the decimal separator and thousands separator of the excel application
        /// </summary>
        /// <param name="application">
        /// The current excel application object
        /// </param>
        /// <returns>
        /// an instance of <see cref="NumberFormatInfo"/>
        /// </returns>
        private NumberFormatInfo QuerayNumberFormatInfo(Application application)
        {
            NumberFormatInfo numberFormatInfo = null;
            if (application.UseSystemSeparators)
            {
                numberFormatInfo = ExcelNumberFormatProvider.CreateExcelNumberFormatInfo(true);
            }
            else
            {
                var decimalSeparator = (string)application.International(XlApplicationInternational.xlDecimalSeparator);
                var thousandsSeparator = (string)application.International(XlApplicationInternational.xlThousandsSeparator);

                numberFormatInfo = ExcelNumberFormatProvider.CreateExcelNumberFormatInfo(false, decimalSeparator, thousandsSeparator);
            }

            return numberFormatInfo;
        }

        /// <summary>
        /// Queries the computed-value at the row in the <see cref="parameterContent"/> array
        /// </summary>
        /// <param name="i">
        /// The row in the <see cref="parameterContent"/> array
        /// </param>
        /// <returns>
        /// an object that represents the computed value
        /// </returns>
        private object QueryComputedValue(int i)
        {
            object computedValue;

            var computedObject = this.parameterContent[i, ParameterSheetConstants.ComputedColumn];
            if (computedObject != null && !ExcelErrors.IsXLCVErr(computedObject))
            {
                computedValue = computedObject;
            }
            else
            {
                computedValue = "-";
            }

            return computedValue;
        }

        /// <summary>
        /// Queries the formula-value at the row in the <see cref="parameterFormula"/> array
        /// </summary>
        /// <param name="i">
        /// The row in the <see cref="parameterFormula"/> array
        /// </param>
        /// <returns>
        /// a string representing the formula value
        /// </returns>
        private string QueryFormulaValue(int i)
        {
            string formulaValue = "-";

            var formulaObject = this.parameterFormula[i, ParameterSheetConstants.ComputedColumn];
            if (formulaObject != null && !ExcelErrors.IsXLCVErr(formulaObject))
            {
                formulaValue = Convert.ToString(formulaObject);
            }

            return formulaValue;
        }

        /// <summary>
        /// Queries the manual-value at the row in the <see cref="parameterContent"/> array
        /// </summary>
        /// <param name="i">
        /// The row in the <see cref="parameterContent"/> array
        /// </param>
        /// <returns>
        /// an object that represents the manual value
        /// </returns>
        private object QueryManualValue(int i)
        {
            object manualValue;

            var manualObject = this.parameterContent[i, ParameterSheetConstants.ManualColumn];
            if (manualObject != null && !ExcelErrors.IsXLCVErr(manualObject))
            {
                manualValue = manualObject;
            }
            else
            {
                manualValue = "-";
            }

            return manualValue;
        }

        /// <summary>
        /// Queries the reference-value at the row in the <see cref="parameterContent"/> array
        /// </summary>
        /// <param name="i">
        /// The row in the <see cref="parameterContent"/> array
        /// </param>
        /// <returns>
        /// an object that represents the reference value
        /// </returns>
        private object QueryReferenceValue(int i)
        {
            object referenceValue;

            var referenceObject = this.parameterContent[i, ParameterSheetConstants.ReferenceColumn];
            if (referenceObject != null && !ExcelErrors.IsXLCVErr(referenceObject))
            {
                referenceValue = referenceObject;
            }
            else
            {
                referenceValue = "-";
            }

            return referenceValue;
        }

        /// <summary>
        /// Queries the switch-value at the row in the <see cref="parameterContent"/> array
        /// </summary>
        /// <param name="i">
        /// The row in the <see cref="parameterContent"/> array
        /// </param>
        /// <returns>
        /// an string that represents the switch value
        /// </returns>
        private string QuerySwitchValue(int i)
        {
            var switchValue = "-";

            switchValue = Convert.ToString(this.parameterContent[i, ParameterSheetConstants.SwitchColumn]);

            return switchValue;
        }

        /// <summary>
        /// Queries the actual-value at the row in the <see cref="parameterContent"/> array
        /// </summary>
        /// <param name="i">
        /// The row in the <see cref="parameterContent"/> array
        /// </param>
        /// <returns>
        /// an object that represents the actual value
        /// </returns>
        private object QueryActualValue(int i)
        {
            object actualValue;

            var actualObject = this.parameterContent[i, ParameterSheetConstants.ActualValueColumn];
            if (actualObject != null && !ExcelErrors.IsXLCVErr(actualObject))
            {
                actualValue = actualObject;
            }
            else
            {
                actualValue = "-";
            }

            return actualValue;
        }

        /// <summary>
        /// Process the <see cref="ParameterValueSet"/> .
        /// </summary>
        /// <param name="parameterValueSet">
        /// The <see cref="ParameterValueSet"/> to be processed.
        /// </param>
        /// <param name="componentIndex">
        /// The index of the <see cref="ParameterTypeComponent"/>.
        /// </param>
        /// <param name="currentRow">
        /// The row in the Parameter sheet that contains the <see cref="ParameterValueSet"/>.
        /// </param>
        /// <param name="manualValue">
        /// The manual value of the <see cref="ParameterValueSet"/>.
        /// </param>
        /// <param name="computedValue">
        /// The result of the computed value of the <see cref="ParameterValueSet"/>.
        /// </param>
        /// <param name="referenceValue">
        /// The reference value of the <see cref="ParameterValueSet"/>.
        /// </param>
        /// <param name="actualValue">
        /// The actual value of the <see cref="ParameterValueSet"/>.
        /// </param>
        /// <param name="switchValue">
        /// The string value of the <see cref="ParameterSwitchKind"/> of the <see cref="ParameterValueSet"/>
        /// </param>
        /// <param name="formulaValue">
        /// The formula that is used to set the computed property of the <see cref="ParameterValueSet"/>
        /// </param>
        /// <param name="processedValueSets">
        /// A <see cref="Dictionary{Guid,ProcessedValueSet}"/> of ProcessedValueSe that capture the updated <see cref="Thing"/>s with its value validation result
        /// </param>
        /// <param name="provider">
        /// The <see cref="IFormatProvider"/> used to validate.
        /// </param>
        private void ProcessValueSet(ParameterValueSet parameterValueSet, int componentIndex, int currentRow, object manualValue, object computedValue, object referenceValue, object actualValue, string switchValue, string formulaValue, ref Dictionary<Guid, ProcessedValueSet> processedValueSets, IFormatProvider provider)
        {
            var validationResult = ValidationResultKind.InConclusive;

            ParameterSwitchKind switchKind;
            ParameterType parameterType = null;
            MeasurementScale measurementScale = null;

            ParameterSheetUtilities.QueryParameterTypeAndScale(parameterValueSet, componentIndex, out parameterType, out measurementScale);

            ValidationResult validSwitch;            
            var isValidSwitchKind = Enum.IsDefined(typeof(ParameterSwitchKind), switchValue);
            if (isValidSwitchKind)
            {
                switchKind = (ParameterSwitchKind)Enum.Parse(typeof(ParameterSwitchKind), switchValue);
                validSwitch = new ValidationResult { ResultKind = ValidationResultKind.Valid, Message = string.Empty };
            }
            else
            {
                switchKind = ParameterSwitchKind.MANUAL;
                validSwitch = new ValidationResult { ResultKind = ValidationResultKind.Invalid, Message = string.Format("{0} is not a valid Parameter Switch Kind", switchValue) };                    
            }

            if (validSwitch.ResultKind > validationResult)
            {
                validationResult = validSwitch.ResultKind;
            }

            var validManualValue = new ValidationResult
            {
                ResultKind = ValidationResultKind.InConclusive,
                Message = string.Empty
            };

            var validComputedValue = new ValidationResult
            {
                ResultKind = ValidationResultKind.InConclusive,
                Message = string.Empty
            };

            var validReferenceValue = new ValidationResult
            {
                ResultKind = ValidationResultKind.InConclusive,
                Message = string.Empty
            };

            var validActualValue = new ValidationResult
            {
                ResultKind = ValidationResultKind.InConclusive,
                Message = string.Empty
            };

            if (parameterType != null)
            {
                if (parameterType is TimeOfDayParameterType)
                {
                    ParameterSheetUtilities.ConvertDoubleToDateTimeObject(ref manualValue, parameterType);
                    ParameterSheetUtilities.ConvertDoubleToDateTimeObject(ref computedValue, parameterType);
                    ParameterSheetUtilities.ConvertDoubleToDateTimeObject(ref referenceValue, parameterType);
                    ParameterSheetUtilities.ConvertDoubleToDateTimeObject(ref actualValue, parameterType);
                }

                if (parameterType is EnumerationParameterType)
                {
                    ParameterSheetUtilities.ConvertObjectToString(ref manualValue);
                    ParameterSheetUtilities.ConvertObjectToString(ref computedValue);
                    ParameterSheetUtilities.ConvertObjectToString(ref referenceValue);
                    ParameterSheetUtilities.ConvertObjectToString(ref actualValue);
                }

                validManualValue = parameterType.Validate(manualValue, measurementScale, provider);
                if (validManualValue.ResultKind > validationResult)
                {
                    validationResult = validManualValue.ResultKind;
                }

                validComputedValue = parameterType.Validate(computedValue, measurementScale, provider);
                if (validComputedValue.ResultKind > validationResult)
                {
                    validationResult = validComputedValue.ResultKind;
                }

                validReferenceValue = parameterType.Validate(referenceValue, measurementScale, provider);
                if (validReferenceValue.ResultKind > validationResult)
                {
                    validationResult = validReferenceValue.ResultKind;
                }

                validActualValue = parameterType.Validate(actualValue, measurementScale, provider);
                if (validActualValue.ResultKind > validationResult)
                {
                    validationResult = validActualValue.ResultKind;
                }
            }

            ParameterSheetUtilities.Decorate(validSwitch, this.parameterSheet, currentRow, ParameterSheetConstants.SwitchColumn);
            ParameterSheetUtilities.Decorate(validManualValue, this.parameterSheet, currentRow, ParameterSheetConstants.ManualColumn);
            ParameterSheetUtilities.Decorate(validComputedValue, this.parameterSheet, currentRow, ParameterSheetConstants.ComputedColumn);
            ParameterSheetUtilities.Decorate(validReferenceValue, this.parameterSheet, currentRow, ParameterSheetConstants.ReferenceColumn);
            ParameterSheetUtilities.Decorate(validActualValue, this.parameterSheet, currentRow, ParameterSheetConstants.ActualValueColumn);

            ProcessedValueSet processedValueSet;
            var valueSetExists = processedValueSets.TryGetValue(parameterValueSet.Iid, out processedValueSet);
            if (!valueSetExists)
            {
                processedValueSet = new ProcessedValueSet(parameterValueSet, validationResult);
            }
            
            ValueSetValues valueSetValues;
            if (processedValueSet.IsDirty(componentIndex, parameterType, switchKind, manualValue, computedValue, referenceValue, formulaValue, out valueSetValues))
            {
                processedValueSet.UpdateClone(valueSetValues);
                if (!valueSetExists)
                {
                    processedValueSets.Add(parameterValueSet.Iid, processedValueSet);
                }                
            }
        }

        /// <summary>
        /// Process the <see cref="ParameterOverrideValueSet"/> .
        /// </summary>
        /// <param name="parameterOverrideValueSet">
        /// The <see cref="ParameterOverrideValueSet"/> to be processed.
        /// </param>
        /// <param name="componentIndex">
        /// The index of the <see cref="ParameterTypeComponent"/>.
        /// </param>
        /// <param name="currentRow">
        /// The row in the Parameter sheet that contains the <see cref="ParameterValueSet"/>.
        /// </param>
        /// <param name="manualValue">
        /// The manual value of the <see cref="ParameterValueSet"/>.
        /// </param>
        /// <param name="computedValue">
        /// The result of the computed value of the <see cref="ParameterValueSet"/>.
        /// </param>
        /// <param name="referenceValue">
        /// The reference value of the <see cref="ParameterValueSet"/>.
        /// </param>
        /// <param name="actualValue">
        /// The actual value of the <see cref="ParameterValueSet"/>.
        /// </param>
        /// <param name="switchValue">
        /// The string value of the <see cref="ParameterSwitchKind"/> of the <see cref="ParameterOverrideValueSet"/>
        /// </param>
        /// <param name="formulaValue">
        /// The formula that is used to set the computed property of the <see cref="ParameterValueSet"/>
        /// </param>
        /// <param name="processedValueSets">
        /// A <see cref="Dictionary{Guid,ProcessedValueSet}"/> of ProcessedValueSe that capture the updated <see cref="Thing"/>s with its value validation result
        /// </param>
        /// <param name="provider">
        /// The <see cref="IFormatProvider"/> used to validate.
        /// </param>
        private void ProcessValueSet(ParameterOverrideValueSet parameterOverrideValueSet, int componentIndex, int currentRow, object manualValue, object computedValue, object referenceValue, object actualValue, string switchValue, string formulaValue, ref Dictionary<Guid, ProcessedValueSet> processedValueSets, IFormatProvider provider)
        {
            var validationResult = ValidationResultKind.InConclusive;

            var switchKind = ParameterSwitchKind.MANUAL;
            ParameterType parameterType = null;
            MeasurementScale measurementScale = null;

            ParameterSheetUtilities.QueryParameterTypeAndScale(parameterOverrideValueSet, componentIndex, out parameterType, out measurementScale);

            ValidationResult validSwitch;
            var isValidSwitchKind = Enum.IsDefined(typeof(ParameterSwitchKind), switchValue);
            if (isValidSwitchKind)
            {
                switchKind = (ParameterSwitchKind)Enum.Parse(typeof(ParameterSwitchKind), switchValue);
                validSwitch = new ValidationResult { ResultKind = ValidationResultKind.Valid, Message = string.Empty };
            }
            else
            {
                switchKind = ParameterSwitchKind.MANUAL;
                validSwitch = new ValidationResult { ResultKind = ValidationResultKind.Invalid, Message = string.Format("{0} is not a valid Parameter Switch Kind", switchValue) };
            }

            if (validSwitch.ResultKind > validationResult)
            {
                validationResult = validSwitch.ResultKind;
            }

            var validManualValue = new ValidationResult
            {
                ResultKind = ValidationResultKind.InConclusive,
                Message = string.Empty
            };

            var validComputedValue = new ValidationResult
            {
                ResultKind = ValidationResultKind.InConclusive,
                Message = string.Empty
            };

            var validReferenceValue = new ValidationResult
            {
                ResultKind = ValidationResultKind.InConclusive,
                Message = string.Empty
            };

            var validActualValue = new ValidationResult
            {
                ResultKind = ValidationResultKind.InConclusive,
                Message = string.Empty
            };

            if (parameterType != null)
            {
                if (parameterType is TimeOfDayParameterType)
                {
                    ParameterSheetUtilities.ConvertDoubleToDateTimeObject(ref manualValue, parameterType);
                    ParameterSheetUtilities.ConvertDoubleToDateTimeObject(ref computedValue, parameterType);
                    ParameterSheetUtilities.ConvertDoubleToDateTimeObject(ref referenceValue, parameterType);
                    ParameterSheetUtilities.ConvertDoubleToDateTimeObject(ref actualValue, parameterType);
                }

                validManualValue = parameterType.Validate(manualValue, measurementScale, provider);
                if (validManualValue.ResultKind > validationResult)
                {
                    validationResult = validManualValue.ResultKind;
                }

                validComputedValue = parameterType.Validate(computedValue, measurementScale, provider);
                if (validComputedValue.ResultKind > validationResult)
                {
                    validationResult = validComputedValue.ResultKind;
                }

                validReferenceValue = parameterType.Validate(referenceValue, measurementScale, provider);
                if (validReferenceValue.ResultKind > validationResult)
                {
                    validationResult = validReferenceValue.ResultKind;
                }

                validActualValue = parameterType.Validate(actualValue, measurementScale, provider);
                if (validActualValue.ResultKind > validationResult)
                {
                    validationResult = validActualValue.ResultKind;
                }
            }

            ParameterSheetUtilities.Decorate(validManualValue, this.parameterSheet, currentRow, ParameterSheetConstants.ManualColumn);
            ParameterSheetUtilities.Decorate(validComputedValue, this.parameterSheet, currentRow, ParameterSheetConstants.ComputedColumn);
            ParameterSheetUtilities.Decorate(validReferenceValue, this.parameterSheet, currentRow, ParameterSheetConstants.ReferenceColumn);
            ParameterSheetUtilities.Decorate(validActualValue, this.parameterSheet, currentRow, ParameterSheetConstants.ActualValueColumn);

            ProcessedValueSet processedValueSet;
            var valueSetExists = processedValueSets.TryGetValue(parameterOverrideValueSet.Iid, out processedValueSet);
            if (!valueSetExists)
            {
                processedValueSet = new ProcessedValueSet(parameterOverrideValueSet, validationResult);
            }
            
            ValueSetValues valueSetValues;
            if (processedValueSet.IsDirty(componentIndex, parameterType, switchKind, manualValue, computedValue, referenceValue, formulaValue, out valueSetValues))
            {
                processedValueSet.UpdateClone(valueSetValues);
                if (!valueSetExists)
                {
                    processedValueSets.Add(parameterOverrideValueSet.Iid, processedValueSet);
                }                
            }
        }

        /// <summary>
        /// Process the <see cref="ParameterSubscriptionValueSet"/> .
        /// </summary>
        /// <param name="parameterSubscriptionValueSet">
        /// The <see cref="ParameterSubscriptionValueSet"/> to be processed.
        /// </param>
        /// <param name="componentIndex">
        /// The index of the <see cref="ParameterTypeComponent"/>.
        /// </param>
        /// <param name="currentRow">
        /// The row in the Parameter sheet that contains the <see cref="ParameterValueSet"/>.
        /// </param>
        /// <param name="manualValue">
        /// The manual value of the <see cref="ParameterValueSet"/>.
        /// </param>
        /// <param name="switchValue">
        /// The string value of the <see cref="ParameterSwitchKind"/> of the <see cref="ParameterSubscriptionValueSet"/>
        /// </param>
        /// <param name="valuesets">
        /// A <see cref="Dictionary{Guid,ProcessedValueSet}"/> of <see cref="ProcessedValueSet"/>s that capture the updated <see cref="Thing"/>s with its value validation result
        /// </param>
        /// <param name="provider">
        /// The <see cref="IFormatProvider"/> used to validate.
        /// </param>
        /// <returns>
        /// The <see cref="ValidationResultKind"/> for the <see cref="ParameterSubscriptionValueSet"/>
        /// </returns>
        private void ProcessValueSet(ParameterSubscriptionValueSet parameterSubscriptionValueSet, int componentIndex, int currentRow, object manualValue, string switchValue, ref Dictionary<Guid, ProcessedValueSet> valuesets, IFormatProvider provider)
        {
            var validationResult = ValidationResultKind.InConclusive;

            var switchKind = ParameterSwitchKind.MANUAL;
            ParameterType parameterType = null;
            MeasurementScale measurementScale = null;

            ParameterSheetUtilities.QueryParameterTypeAndScale(parameterSubscriptionValueSet, componentIndex, out parameterType, out measurementScale);

            ValidationResult validSwitch;
            var isValidSwitchKind = Enum.IsDefined(typeof(ParameterSwitchKind), switchValue);
            if (isValidSwitchKind)
            {
                switchKind = (ParameterSwitchKind)Enum.Parse(typeof(ParameterSwitchKind), switchValue);
                validSwitch = new ValidationResult { ResultKind = ValidationResultKind.Valid, Message = string.Empty };
            }
            else
            {
                switchKind = ParameterSwitchKind.MANUAL;
                validSwitch = new ValidationResult { ResultKind = ValidationResultKind.Invalid, Message = string.Format("{0} is not a valid Parameter Switch Kind", switchValue) };
            }

            if (validSwitch.ResultKind > validationResult)
            {
                validationResult = validSwitch.ResultKind;
            }

            var validManualValue = new ValidationResult
            {
                ResultKind = ValidationResultKind.InConclusive,
                Message = string.Empty
            };

            if (parameterType != null)
            {
                if (parameterType is TimeOfDayParameterType)
                {
                    ParameterSheetUtilities.ConvertDoubleToDateTimeObject(ref manualValue, parameterType);
                }

                validManualValue = parameterType.Validate(manualValue, measurementScale, provider);
                if (validManualValue.ResultKind > validationResult)
                {
                    validationResult = validManualValue.ResultKind;
                }
            }

            ParameterSheetUtilities.Decorate(validManualValue, this.parameterSheet, currentRow, ParameterSheetConstants.ManualColumn);

            ProcessedValueSet processedValueSet;
            var valueSetExists = valuesets.TryGetValue(parameterSubscriptionValueSet.Iid, out processedValueSet);
            if (!valueSetExists)
            {
                processedValueSet = new ProcessedValueSet(parameterSubscriptionValueSet, validationResult);
            }
            
            ValueSetValues valueSetValues;
            if (processedValueSet.IsDirty(componentIndex, parameterType, switchKind, manualValue, null, null, null, out valueSetValues))
            {
                processedValueSet.UpdateClone(valueSetValues);
                if (!valueSetExists)
                {
                    valuesets.Add(parameterSubscriptionValueSet.Iid, processedValueSet);
                }                
            }
        }
    }
}
