// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterSheetUtilities.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ParameterSheetGenerator.Generator
{
    using System;
    using System.Globalization;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Validation;
    using CDP4OfficeInfrastructure.Excel;
    using CDP4ParameterSheetGenerator.ParameterSheet;
    using NetOffice.ExcelApi;
    using NetOffice.ExcelApi.Enums;
    using NLog;

    /// <summary>
    /// Helper methods to interact with the workbook
    /// </summary>
    public static class ParameterSheetUtilities
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Retrieve the parameter sheet form the workbook
        /// </summary>
        /// <param name="workbook">
        /// the <see cref="Workbook"/> to retrieve the parameter sheet from
        /// </param>
        /// <param name="replace">
        /// a value indicating whether the parameter sheet shall be replaced if found
        /// </param>
        /// <returns>
        /// returns the existing parameter sheet, or a new sheet named "parameters" that is added to the provided workbook
        /// </returns>
        public static Worksheet RetrieveParameterSheet(Workbook workbook, bool replace = false)
        {
            var parameterWorksheet = workbook.RetrieveWorksheet(ParameterSheetConstants.ParameterSheetName, replace);
            return parameterWorksheet;
        }

        /// <summary>
        /// Retrieve the option sheet form the workbook
        /// </summary>
        /// <param name="workbook">
        /// the <see cref="Workbook"/> to retrieve the option sheet from
        /// </param>
        /// <param name="option">
        /// The <see cref="Option"/> for which the sheet needs to be returned
        /// </param>
        /// <returns>
        /// returns a new sheet named after the <see cref="Option"/> that is added to the provided workbook
        /// </returns>
        /// <remarks>
        /// In case the option sheet already exists it is replaced with a new <see cref="Option"/> sheet
        /// </remarks>
        public static Worksheet RetrieveOptionSheet(Workbook workbook, Option option)
        {
            var optionSheet = workbook.RetrieveWorksheet(option.ShortName, true);
            return optionSheet;
        }

        /// <summary>
        /// Decorate the appearance of a cell based on a <see cref="ValidationResult"/>.
        /// </summary>
        /// <param name="validationResult">
        /// The <see cref="ValidationResult"/> that determines the appearance of the cell
        /// </param>
        /// <param name="worksheet">
        /// The <see cref="Worksheet"/> that contains the cell
        /// </param>
        /// <param name="row">
        /// The row of the cell to decorate
        /// </param>
        /// <param name="column">
        /// The column of the cell to decorate
        /// </param>
        public static void Decorate(ValidationResult validationResult, Worksheet worksheet, int row, int column)
        {
            var cell = worksheet.Cells[row, column];
            cell.ClearComments();

            switch (validationResult.ResultKind)
            {
                case ValidationResultKind.InConclusive:
                    // do nothing
                    break;
                case ValidationResultKind.Invalid:
                    cell.Interior.Color = XlRgbColor.rgbRed;
                    cell.AddComment(string.Format("CDP: {0}", validationResult.Message));
                    break;
                case ValidationResultKind.Valid:
                    cell.Interior.Color = XlRgbColor.rgbWhite;
                    break;
                case ValidationResultKind.OutOfBounds:
                    cell.Interior.Color = XlRgbColor.rgbOrange;
                    cell.AddComment(string.Format("CDP: {0}", validationResult.Message));
                    break;
            }
        }

        /// <summary>
        /// Reformats a value that is an excel date that may be a string represented as a double, or a <see cref="DateTime"/>.
        /// </summary>
        /// <param name="value">
        /// the value that represents an excel <see cref="DateTime"/>
        /// </param>
        /// <returns>
        /// a string that is reformatted to only contain a date part it the time is 0:00:00.000, else the value is just returned.
        /// </returns>
        public static string ReformatDate(string value)
        {
            var result = value;

            if (value != "-")
            {
                DateTime dt;
                var isDate = DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt);
                if (isDate && dt.Hour == 0 && dt.Minute == 0 && dt.Second == 0 && dt.Millisecond == 0)
                {
                    result = dt.ToString("yyyy-MM-dd");
                    return result;
                }

                double d;
                var isDouble = double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out d);
                if (isDouble)
                {
                    var dd = DateTime.FromOADate(d);
                    result = dd.ToString("yyyy-MM-dd");
                    return result;
                }
            }

            return result;
        }

        /// <summary>
        /// Reformats a value that is an excel date that may be a string represented as a double, or a <see cref="DateTime"/>.
        /// </summary>
        /// <param name="value">
        /// the value that represents an excel <see cref="DateTime"/>
        /// </param>
        /// <returns>
        /// a string that is reformatted to only contain a date part it the time is 0:00:00.000, else the value is just returned.
        /// </returns>
        public static string ReformatDate(object value)
        {
            var result = string.Empty;

            var stringValue = value as string;
            if (stringValue != null && stringValue != "-")
            {
                DateTime dt;
                var isDate = DateTime.TryParse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt);
                if (isDate && dt.Hour == 0 && dt.Minute == 0 && dt.Second == 0 && dt.Millisecond == 0)
                {
                    result = dt.ToString("yyyy-MM-dd");
                    return result;
                }
            }

            if (value is double)
            {
                var doubleValue = Convert.ToDouble(value);
                var dd = DateTime.FromOADate(doubleValue);
                result = dd.ToString("yyyy-MM-dd");
                return result;
            }

            return Convert.ToString(value);
        }

        /// <summary>
        /// Reformats a value that is an excel date that may be a string represented as a double, or a <see cref="DateTime"/>.
        /// </summary>
        /// <param name="value">
        /// the value that represents an excel <see cref="DateTime"/>
        /// </param>
        /// <returns>
        /// a string that is reformatted to only contain a time part.
        /// </returns>
        public static string ReformatTimeOfDay(string value)
        {
            var result = value;

            if (value != "-")
            {
                DateTime dt;
                var isDate = DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt);
                if (isDate && dt.Hour == 0 && dt.Minute == 0 && dt.Second == 0 && dt.Millisecond == 0)
                {
                    result = dt.ToString("HH:mm:ss");
                    return result;
                }

                double d;
                var isDouble = double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out d);
                if (isDouble)
                {
                    var dd = DateTime.FromOADate(d);
                    result = dd.ToString("HH:mm:ss");
                    return result;
                }
            }

            return result;
        }

        /// <summary>
        /// Converts the provided value to it's string representation
        /// </summary>
        /// <param name="value">
        /// The value that is to be converted to a string
        /// </param>
        /// <remarks>
        /// If the value cannot be converted an error is logged and the default value "-" is set
        /// </remarks>
        public static void ConvertObjectToString(ref object value)
        {
            string valueAsString = "-";
            try
            {
                valueAsString = value.ToString();                
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Could not convert {0} to string", value);
            }

            value = valueAsString;
        }

        /// <summary>
        /// Converts an OLE double value to a <see cref="DateTime"/> object
        /// </summary>
        /// <param name="value">
        /// The object to convert
        /// </param>
        /// <param name="parameterType">
        /// The <see cref="ParameterType"/> of the <see cref="NetOffice.ExcelApi.Parameter"/> of which the <paramref name="value"/> is the 
        /// associated value;
        /// </param>
        /// <remarks>
        /// In Excel, when a date object only has a time part specified, the date part is set to 1899-12-30. A
        /// valid CDP4 TimeOfDayParameterType DateTime value has as date part 1-1-1, therefore a <see cref="TimeOfDayParameterType"/>
        /// is converted to a valid CDP4 TimeOfDayParameterType in this conversion.
        /// </remarks>
        public static void ConvertDoubleToDateTimeObject(ref object value, ParameterType parameterType)
        {
            var stringValue = value as string;
            if (stringValue != null)
            {
                if (stringValue == "-")
                {
                    value = "-";
                    return;
                }

                DateTime dt;
                var isDate = DateTime.TryParse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt);
                if (isDate && dt.Hour == 0 && dt.Minute == 0 && dt.Second == 0 && dt.Millisecond == 0)
                {
                    value = dt;
                    return;
                }
            }

            if (value is double)
            {  
                var doubleValue = Convert.ToDouble(value);
                var dd = DateTime.FromOADate(doubleValue);

                // In excel, a DateTime that only specifies a Time part will have as date 1899-12-30
                // We have to set the date part to become 1-1-1 for it to become a valid CDP4 DateTime object.
                if (parameterType is TimeOfDayParameterType)
                {
                    var x = dd.AddYears(-1898);
                    var y = x.AddMonths(-11);
                    var z = y.AddDays(-29);
                    value = z;
                    return;
                }

                value = dd;
            }
        }

        /// <summary>
        /// Reformats a value that is an excel date that may be a string represented as a double, or a <see cref="DateTime"/>.
        /// </summary>
        /// <param name="value">
        /// the value that represents an excel <see cref="DateTime"/>
        /// </param>
        /// <returns>
        /// a string that is reformatted to only contain a time part.
        /// </returns>
        public static string ReformatTimeOfDay(object value)
        {
            var result = "-";

            var stringValue = value as string;
            if (stringValue != null && stringValue != "-")
            {
                DateTime dt;
                var isDate = DateTime.TryParse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt);
                if (isDate && dt.Hour == 0 && dt.Minute == 0 && dt.Second == 0 && dt.Millisecond == 0)
                {
                    result = dt.ToString("HH:mm:ss");
                    return result;
                }
            }

            if (value is double)
            {
                var doubleValue = Convert.ToDouble(value);
                var dd = DateTime.FromOADate(doubleValue);
                result = dd.ToString("HH:mm:ss");
                return result;
            }

            return result;
        }

        /// <summary>
        /// lock or unlock the parameter sheet
        /// </summary>
        /// <param name="parameterSheet">
        /// The <see cref="Worksheet"/> that is to be locked or unlocked.
        /// </param>
        /// <param name="locking">
        /// a value indicating whether locking or unlocking should be applied.
        /// </param>
        public static void ApplyLocking(Worksheet parameterSheet, bool locking)
        {
            if (locking)
            {
                parameterSheet.EnableOutlining = true;

                parameterSheet.Protect(
                    password: null,
                    drawingObjects: null,
                    contents: true,
                    scenarios: null,
                    userInterfaceOnly: true,
                    allowFormattingCells: false,
                    allowFormattingColumns: false,
                    allowFormattingRows: false,
                    allowInsertingColumns: false,
                    allowInsertingRows: false,
                    allowInsertingHyperlinks: false,
                    allowDeletingColumns: false,
                    allowDeletingRows: false,
                    allowSorting: false,
                    allowFiltering: false,                    
                    allowUsingPivotTables: true);
            }
            else
            {
                parameterSheet.Unprotect();
            }
        }

        /// <summary>
        /// Queries the container of the <see cref="ParameterValueSet"/> for the <see cref="ParameterType"/> and <see cref="MeasurementScale"/>
        /// </summary>
        /// <param name="parameterValueSet">
        /// The subject <see cref="ParameterValueSet"/>
        /// </param>
        /// <param name="componentIndex">
        /// The index of the <see cref="ParameterTypeComponent"/>.
        /// </param>
        /// <param name="parameterType">
        /// The resulting <see cref="ParameterType"/>
        /// </param>
        /// <param name="measurementScale">
        /// The resulting <see cref="MeasurementScale"/>, this may be null if the <see cref="ParameterType"/> is not a <see cref="QuantityKind"/>
        /// </param>
        public static void QueryParameterTypeAndScale(ParameterValueSet parameterValueSet, int componentIndex, out ParameterType parameterType, out MeasurementScale measurementScale)
        {
            var parameter = (CDP4Common.EngineeringModelData.Parameter)parameterValueSet.Container;

            var scalarParameterType = parameter.ParameterType as ScalarParameterType;
            if (scalarParameterType != null)
            {
                parameterType = scalarParameterType;
                measurementScale = parameter.Scale;
                return;
            }

            var compoundParameterType = parameter.ParameterType as CompoundParameterType;
            if (compoundParameterType != null)
            {
                var component = compoundParameterType.Component[componentIndex];
                parameterType = component.ParameterType;
                measurementScale = component.Scale;
                return;
            }

            logger.Debug("The ParameterType and MeasurementScale of ParameterValueSet {0} could not be queried", parameterValueSet.Iid);

            parameterType = null;
            measurementScale = null;
        }

        /// <summary>
        /// Queries the container of the <see cref="ParameterOverrideValueSet"/> for the <see cref="ParameterType"/> and <see cref="MeasurementScale"/>
        /// </summary>
        /// <param name="parameterOverrideValueSet">
        /// The subject <see cref="ParameterOverrideValueSet"/>
        /// </param>
        /// <param name="componentIndex">
        /// The index of the <see cref="ParameterTypeComponent"/>.
        /// </param>
        /// <param name="parameterType">
        /// The resulting <see cref="ParameterType"/>
        /// </param>
        /// <param name="measurementScale">
        /// The resulting <see cref="MeasurementScale"/>, this may be null if the <see cref="ParameterType"/> is not a <see cref="QuantityKind"/>
        /// </param>
        public static void QueryParameterTypeAndScale(ParameterOverrideValueSet parameterOverrideValueSet, int componentIndex, out ParameterType parameterType, out MeasurementScale measurementScale)
        {
            var parameterOverride = (ParameterOverride)parameterOverrideValueSet.Container;

            var scalarParameterType = parameterOverride.ParameterType as ScalarParameterType;
            if (scalarParameterType != null)
            {
                parameterType = scalarParameterType;
                measurementScale = parameterOverride.Scale;
                return;
            }

            var compoundParameterType = parameterOverride.ParameterType as CompoundParameterType;
            if (compoundParameterType != null)
            {
                var component = compoundParameterType.Component[componentIndex];
                parameterType = component.ParameterType;
                measurementScale = component.Scale;
                return;
            }

            logger.Debug("The ParameterType and MeasurementScale of ParameterOverrideValueSet {0} could not be queried", parameterOverrideValueSet.Iid);

            parameterType = null;
            measurementScale = null;
        }

        /// <summary>
        /// Queries the container of the <see cref="ParameterValueSet"/> for the <see cref="ParameterType"/> and <see cref="MeasurementScale"/>
        /// </summary>
        /// <param name="parameterSubscriptionValueSet">
        /// The subject <see cref="ParameterSubscriptionValueSet"/>
        /// </param>
        /// <param name="componentIndex">
        /// The index of the <see cref="ParameterTypeComponent"/>.
        /// </param>
        /// <param name="parameterType">
        /// The resulting <see cref="ParameterType"/>
        /// </param>
        /// <param name="measurementScale">
        /// The resulting <see cref="MeasurementScale"/>, this may be null if the <see cref="ParameterType"/> is not a <see cref="QuantityKind"/>
        /// </param>
        public static void QueryParameterTypeAndScale(ParameterSubscriptionValueSet parameterSubscriptionValueSet, int componentIndex, out ParameterType parameterType, out MeasurementScale measurementScale)
        {
            var parameterSubscription = (ParameterSubscription)parameterSubscriptionValueSet.Container;

            var scalarParameterType = parameterSubscription.ParameterType as ScalarParameterType;
            if (scalarParameterType != null)
            {
                parameterType = scalarParameterType;
                measurementScale = parameterSubscription.Scale;
                return;
            }

            var compoundParameterType = parameterSubscription.ParameterType as CompoundParameterType;
            if (compoundParameterType != null)
            {
                var component = compoundParameterType.Component[componentIndex];
                parameterType = component.ParameterType;
                measurementScale = component.Scale;
                return;
            }

            logger.Debug("The ParameterType and MeasurementScale of ParameterSubscriptionValueSet {0} could not be queried", parameterSubscriptionValueSet.Iid);

            parameterType = null;
            measurementScale = null;
        }
    }
}
