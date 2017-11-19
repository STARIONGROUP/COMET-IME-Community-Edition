// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ValueValidator.cs" company="RHEA S.A.">
//   Copyright (c) 2015 RHEA S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ParameterSheetGenerator.Validation
{
    using System;
    using System.Globalization;
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4ParameterSheetGenerator.Generator;
    using NLog;

    /// <summary>
    /// The compound result of <see cref="Parameter"/> value validation that carries the <see cref="ValidationResultKind"/> 
    /// and an optional message.
    /// </summary>
    public struct ValidationResult
    {
        /// <summary>
        /// The actual result of a validation
        /// </summary>
        public ValidationResultKind ResultKind;

        /// <summary>
        /// An optional message to provide more detail regarding the validation result. When the
        /// <see cref="ResultKind"/> is Valid the message is empty
        /// </summary>
        public string Message;
    }

    /// <summary>
    /// The purpose of the <see cref="ValueValidator"/> is to validate the value of a <see cref="Parameter"/> with respect to 
    /// it's referenced <see cref="ParameterType"/>. The default value is a hyphen "-", which is a valid value for all <see cref="ParameterType"/>s.
    /// </summary>
    public static class ValueValidator
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The default value that is valid for all <see cref="ParameterType"/>s
        /// </summary>
        public const string DefaultValue = "-";

        /// <summary>
        /// Valid <see cref="Boolean"/> values
        /// </summary>
        public static readonly string[] ValidBoolan = { "-", "true", "false", "True", "False", "1", "-1" };

        /// <summary>
        /// Validates the  to check whether the <paramref name="value"/> is valid with respect to the <paramref name="parameterType"/>
        /// </summary>
        /// <param name="parameterType">
        /// A <see cref="BooleanParameterType"/>
        /// </param>
        /// <param name="value">
        /// The string value that is to be validated
        /// </param>
        /// <param name="measurementScale">
        /// The measurement Scale.
        /// </param>
        /// <returns>
        /// a <see cref="ValidationResult"/> that carries the <see cref="ValidationResultKind"/> and an optional message.
        /// </returns>
        public static ValidationResult Validate(this ParameterType parameterType, string value, MeasurementScale measurementScale = null)
        {
            ValidationResult result;

            if (value == DefaultValue)
            {
                result.ResultKind = ValidationResultKind.Valid;
                result.Message = string.Empty;
                return result;
            }

            switch (parameterType.ClassKind)
            {
                case ClassKind.BooleanParameterType:
                    var booleanParameter = (BooleanParameterType)parameterType;
                    return booleanParameter.Validate(value);
                case ClassKind.DateParameterType:
                    var dateParameterType = (DateParameterType)parameterType;                   
                    return dateParameterType.Validate(value);
                case ClassKind.DateTimeParameterType:
                    var dateTimeParameterType = (DateTimeParameterType)parameterType;
                    return dateTimeParameterType.Validate(value);
                case ClassKind.EnumerationParameterType:
                    var enumerationParameterType = (EnumerationParameterType)parameterType;
                    return enumerationParameterType.Validate(value);
                case ClassKind.QuantityKind:
                case ClassKind.SimpleQuantityKind:
                case ClassKind.DerivedQuantityKind:
                case ClassKind.SpecializedQuantityKind:                
                    var quantityKind = (QuantityKind)parameterType;
                    return quantityKind.Validate(measurementScale, value);
                case ClassKind.TextParameterType:
                    var textParameterType = (TextParameterType)parameterType;
                    return textParameterType.Validate(value);
                case ClassKind.TimeOfDayParameterType:
                    var timeOfDayParameterType = (TimeOfDayParameterType)parameterType;
                    return timeOfDayParameterType.Validate(value);
                default:
                    throw new NotSupportedException(string.Format("The Validate method is not suported for parameterType: {0}", parameterType));
            }
        }

        /// <summary>
        /// Validates the <param name="value"></param> to check whether it is a <see cref="Boolean"/>
        /// </summary>
        /// <param name="parameterType">
        /// A <see cref="BooleanParameterType"/>
        /// </param>
        /// <param name="value">
        /// the string representation of a <see cref="Boolean"/> value
        /// </param>
        /// <returns>
        /// a <see cref="ValidationResult"/> that carries the <see cref="ValidationResultKind"/> and an optional message.
        /// </returns>
        public static ValidationResult Validate(this BooleanParameterType parameterType, string value)
        {
            ValidationResult result;

            var lowerCaseValue = value.ToLower();

            if (ValidBoolan.Contains(lowerCaseValue))
            {
                result.ResultKind = ValidationResultKind.Valid;
                result.Message = string.Empty;
                return result;
            }

            bool booleanResult = false;
            bool.TryParse(value, out booleanResult);

            if (booleanResult)
            {
                result.ResultKind = ValidationResultKind.Valid;
                result.Message = string.Empty;
                return result;
            }

            result.ResultKind = ValidationResultKind.Invalid;
            result.Message = string.Format("{0} is not a valid boolean, valid values are: {1}", value, string.Join(",", ValidBoolan));
            return result;
        }

        /// <summary>
        /// Validates the <param name="value"></param> to check whether it is a <see cref="DateTime"/> that does not contain any time data.
        /// </summary>
        /// <param name="parameterType">
        /// A <see cref="DateParameterType"/>
        /// </param>
        /// <param name="value">
        /// the string representation of a <see cref="DateTime"/> value that only contains date data and not any time data.
        /// </param>
        /// <returns>
        /// a <see cref="ValidationResult"/> that carries the <see cref="ValidationResultKind"/> and an optional message.
        /// </returns>
        public static ValidationResult Validate(this DateParameterType parameterType, string value)
        {
            ValidationResult result;

            if (value == DefaultValue)
            {
                result.ResultKind = ValidationResultKind.Valid;
                result.Message = string.Empty;
                return result;
            }

            DateTime dateTime;
            var isDateTime = DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime);

            if (isDateTime)
            {
                result.ResultKind = ValidationResultKind.Valid;
                result.Message = string.Empty;
                return result;
            }
            else
            {
                result.ResultKind = ValidationResultKind.Invalid;
                result.Message = string.Format("{0} is not a valid Date, valid dates are specified in  ISO 8601 YYYY-MM-DD", value);
                return result;                    
            }            
        }

        /// <summary>
        /// Validates the <param name="value"></param> to check whether it is a <see cref="DateTime"/>
        /// </summary>
        /// <param name="parameterType">
        /// A <see cref="DateTimeParameterType"/>
        /// </param>
        /// <param name="value">
        /// the string representation of a <see cref="DateTime"/> value
        /// </param>
        /// <returns>
        /// a <see cref="ValidationResult"/> that carries the <see cref="ValidationResultKind"/> and an optional message.
        /// </returns>
        public static ValidationResult Validate(this DateTimeParameterType parameterType, string value)
        {
            ValidationResult result;

            if (value == DefaultValue)
            {
                result.ResultKind = ValidationResultKind.Valid;
                result.Message = string.Empty;
                return result;
            }

            try
            {
                var dateTime = DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.None);

                result.ResultKind = ValidationResultKind.Valid;
                result.Message = string.Empty;
                return result;
            }
            catch (Exception ex)
            {
                result.ResultKind = ValidationResultKind.Invalid;
                result.Message = string.Format("{0} is not a valid DateTime, valid date-times are specified in ISO 8601, see http://www.w3.org/TR/xmlschema-2/#dateTime.", value);
                return result;
            }
        }

        /// <summary>
        /// Validates the <param name="value"></param> to check whether it is a valid <see cref="EnumerationValueDefinition"/> of the <see cref="EnumerationParameterType"/>
        /// </summary>
        /// <param name="parameterType">
        /// A <see cref="EnumerationParameterType"/>
        /// </param>
        /// <param name="value">
        /// the string representation of a <see cref="EnumerationValueDefinition"/> value
        /// </param>
        /// <returns>
        /// a <see cref="ValidationResult"/> that carries the <see cref="ValidationResultKind"/> and an optional message.
        /// </returns>
        public static ValidationResult Validate(this EnumerationParameterType parameterType, string value)
        {
            ValidationResult result;

            if (value == DefaultValue)
            {
                result.ResultKind = ValidationResultKind.Valid;
                result.Message = string.Empty;
                return result;
            }

            var values = value.Split(',');

            if (!parameterType.AllowMultiSelect && values.Count() > 1)
            {
                result.ResultKind = ValidationResultKind.Invalid;
                result.Message = string.Format("The {0} Enumeration Parametertype does not allow multi-selection, multiple values seem to be selected", parameterType.Name);
                return result;
            }

            foreach (var enumerationValue in values)
            {
                var any = parameterType.ValueDefinition.Any(x => x.ShortName == enumerationValue);
                if (!any)
                {
                    var joinedShortnames = string.Empty;
                    var sortedItems = parameterType.ValueDefinition.SortedItems.Values;
                    foreach (var enumerationValueDefinition in sortedItems)
                    {
                        if (joinedShortnames == string.Empty)
                        {
                            joinedShortnames = enumerationValueDefinition.ShortName;
                        }
                        else
                        {
                            joinedShortnames = joinedShortnames + ", " + enumerationValueDefinition.ShortName;
                        }
                    }

                    result.ResultKind = ValidationResultKind.Invalid;
                    result.Message = string.Format("The {0} Enumeration Parametertype does not contain the following value definition {1}, allowed valuse are: {2}", parameterType.Name, enumerationValue, joinedShortnames);
                    return result;
                }
            }

            result.ResultKind = ValidationResultKind.Valid;
            result.Message = string.Empty;
            return result;            
        }

        /// <summary>
        /// Validates the  to check whether it is a <see cref="ScalarParameterType"/>
        /// </summary>
        /// <param name="quantityKind">
        /// A <see cref="QuantityKind"/>
        /// </param>
        /// <param name="scale">
        /// The <see cref="MeasurementScale"/>
        /// </param>
        /// <param name="value">
        /// the string representation of a <see cref="ScalarParameterType"/> value
        /// </param>
        /// <returns>
        /// a <see cref="ValidationResult"/> that carries the <see cref="ValidationResultKind"/> and an optional message.
        /// </returns>
        public static ValidationResult Validate(this QuantityKind quantityKind, MeasurementScale scale, string value)
        {
            ValidationResult result;

            if (value == DefaultValue)
            {
                result.ResultKind = ValidationResultKind.Valid;
                result.Message = string.Empty;
                return result;
            }

            result = scale.Validate(value);

            return result;
        }

        /// <summary>
        /// Validates the <param name="value"></param> to check whether it is Text.
        /// </summary>
        /// <param name="parameterType">
        /// A <see cref="TextParameterType"/>
        /// </param>
        /// <param name="value">
        /// the text value
        /// </param>
        /// <returns>
        /// a <see cref="ValidationResult"/> that carries the <see cref="ValidationResultKind"/> and an optional message.
        /// </returns>
        public static ValidationResult Validate(this TextParameterType parameterType, string value)
        {
            ValidationResult result;

            result.ResultKind = ValidationResultKind.Valid;
            result.Message = string.Empty;
            return result;            
        }

        /// <summary>
        /// Validates the <param name="value"></param> to check whether it is a valid time of day.
        /// </summary>
        /// <param name="parameterType">
        /// A <see cref="TimeOfDayParameterType"/>
        /// </param>
        /// <param name="value">
        /// the time of day value
        /// </param>
        /// <returns>
        /// a <see cref="ValidationResult"/> that carries the <see cref="ValidationResultKind"/> and an optional message.
        /// </returns>
        public static ValidationResult Validate(this TimeOfDayParameterType parameterType, string value)
        {
            ValidationResult result;

            if (value == DefaultValue)
            {
                result.ResultKind = ValidationResultKind.Valid;
                result.Message = string.Empty;
                return result;
            }

            //// when DateTimeStyles.NoCurrentDateDefault is specified an no date part is specified, the date is assumed to be 1-1-1. If the
            //// date of the dateTime variable is not 1-1-1 the user provided an invalid date. The loophole here is that when a user provides a
            //// value that includes 1-1-1, it will be validated as being valid.
            
            DateTime dateTime;            
            var isDateTime = DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind | DateTimeStyles.NoCurrentDateDefault, out dateTime);

            if (isDateTime && dateTime.Year == 1 && dateTime.Month == 1 && dateTime.Day == 1)
            {
                result.ResultKind = ValidationResultKind.Valid;
                result.Message = string.Empty;
                return result;
            }
            else
            {
                result.ResultKind = ValidationResultKind.Invalid;
                result.Message = string.Format("{0} is not a valid Time of Day, for valid Time Of Day formats see http://www.w3.org/TR/xmlschema-2/#time.", value);
                return result;
            }  
        }

        /// <summary>
        /// Validates whether the provided value is valid with respect to the <see cref="MeasurementScale"/>
        /// </summary>
        /// <param name="measurementScale">
        /// The <see cref="MeasurementScale"/> 
        /// </param>
        /// <param name="value">
        /// The value that is to be validated
        /// </param>
        /// <returns>
        /// a <see cref="ValidationResult"/> that carries the <see cref="ValidationResultKind"/> and an optional message.
        /// </returns>
        public static ValidationResult Validate(this MeasurementScale measurementScale, string value)
        {
            ValidationResult result;

            bool isMaximumPermissibleValue;
            bool isMinimumPermissibleValue;

            switch (measurementScale.NumberSet)
            {
                case NumberSetKind.INTEGER_NUMBER_SET:

                    int integer;
                    var isInteger = int.TryParse(value, NumberStyles.Integer, null, out integer);

                    if (!isInteger)
                    {
                        result.ResultKind = ValidationResultKind.Invalid;
                        result.Message = string.Format("\"{0}\" is not a member of the INTEGER NUMBER SET", value);
                        return result;
                    }

                    if (!string.IsNullOrWhiteSpace(measurementScale.MaximumPermissibleValue))
                    {
                        int intMaximumPermissibleValue;
                        isMaximumPermissibleValue = int.TryParse(measurementScale.MaximumPermissibleValue, NumberStyles.Integer, null, out intMaximumPermissibleValue);
                        if (isMaximumPermissibleValue)
                        {
                            if (measurementScale.IsMaximumInclusive && integer > intMaximumPermissibleValue)
                            {
                                result.ResultKind = ValidationResultKind.OutOfBounds;
                                result.Message = string.Format("The value \"{0}\" is greater than the maximium permissible value of \"{1}\"", integer, intMaximumPermissibleValue);
                                return result;
                            }

                            if (!measurementScale.IsMaximumInclusive && integer >= intMaximumPermissibleValue)
                            {
                                result.ResultKind = ValidationResultKind.OutOfBounds;
                                result.Message = string.Format("The value \"{0}\" is greater than or equal to the maximium permissible value of \"{1}\"", integer, intMaximumPermissibleValue);
                                return result;
                            }
                        }
                        else
                        {
                            Logger.Warn("The MaximumPermissibleValue \"{0}\" of MeasurementScale \"{1}\" is not a member of the INTEGER NUMBER SET", measurementScale.MaximumPermissibleValue, measurementScale.Iid);
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(measurementScale.MinimumPermissibleValue))
                    {
                        int intMinimumPermissibleValue;
                        isMinimumPermissibleValue = int.TryParse(measurementScale.MinimumPermissibleValue, NumberStyles.Integer, null, out intMinimumPermissibleValue);
                        if (isMinimumPermissibleValue)
                        {
                            if (measurementScale.IsMinimumInclusive && integer < intMinimumPermissibleValue)
                            {
                                result.ResultKind = ValidationResultKind.OutOfBounds;
                                result.Message = string.Format("The value \"{0}\" is smaller than the minimum permissible value of \"{1}\"", integer, intMinimumPermissibleValue);
                                return result;
                            }

                            if (!measurementScale.IsMinimumInclusive && integer <= intMinimumPermissibleValue)
                            {
                                result.ResultKind = ValidationResultKind.OutOfBounds;
                                result.Message = string.Format("The value \"{0}\" is smaller than or equal to the minimum permissible value of \"{1}\"", integer, intMinimumPermissibleValue);
                                return result;
                            }
                        }
                        else
                        {
                            Logger.Warn("The MinimumPermissibleValue \"{0}\" of MeasurementScale \"{1}\" is not a member of the INTEGER NUMBER SET", measurementScale.MinimumPermissibleValue, measurementScale.Iid);
                        }
                    }

                    result.ResultKind = ValidationResultKind.Valid;
                    result.Message = string.Empty;
                    return result;

                case NumberSetKind.NATURAL_NUMBER_SET:

                    int natural;
                    var isNatural = int.TryParse(value, NumberStyles.Integer, null, out natural);

                    if (!isNatural)
                    {
                        result.ResultKind = ValidationResultKind.Invalid;
                        result.Message = string.Format("\"{0}\" is not a member of the NATURAL NUMBER SET", value);
                        return result;
                    }

                    if (natural < 0)
                    {
                        result.ResultKind = ValidationResultKind.Invalid;
                        result.Message = string.Format("\"{0}\" is not a member of the NATURAL NUMBER SET", value);
                        return result;
                    }

                    if (!string.IsNullOrWhiteSpace(measurementScale.MaximumPermissibleValue))
                    {
                        int naturalMaximumPermissibleValue;
                        isMaximumPermissibleValue = int.TryParse(measurementScale.MaximumPermissibleValue, NumberStyles.Integer, null, out naturalMaximumPermissibleValue);
                        if (isMaximumPermissibleValue)
                        {
                            if (measurementScale.IsMaximumInclusive && natural > naturalMaximumPermissibleValue)
                            {
                                result.ResultKind = ValidationResultKind.OutOfBounds;
                                result.Message = string.Format("The value \"{0}\" is greater than the maximium permissible value of \"{1}\"", natural, naturalMaximumPermissibleValue);
                                return result;
                            }

                            if (!measurementScale.IsMaximumInclusive && natural >= naturalMaximumPermissibleValue)
                            {
                                result.ResultKind = ValidationResultKind.OutOfBounds;
                                result.Message = string.Format("The value \"{0}\" is greater than or equal to the maximium permissible value of \"{1}\"", natural, naturalMaximumPermissibleValue);
                                return result;
                            }
                        }
                        else
                        {
                            Logger.Warn("The MaximumPermissibleValue \"{0}\" of MeasurementScale \"{1}\" is not a member of the INTEGER NUMBER SET", measurementScale.MaximumPermissibleValue, measurementScale.Iid);
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(measurementScale.MinimumPermissibleValue))
                    {
                        int naturalMinimumPermissibleValue;
                        isMinimumPermissibleValue = int.TryParse(measurementScale.MinimumPermissibleValue, NumberStyles.Integer, null, out naturalMinimumPermissibleValue);
                        if (isMinimumPermissibleValue)
                        {
                            if (measurementScale.IsMinimumInclusive && natural < naturalMinimumPermissibleValue)
                            {
                                result.ResultKind = ValidationResultKind.OutOfBounds;
                                result.Message = string.Format("The value \"{0}\" is smaller than the minimum permissible value of \"{1}\"", natural, naturalMinimumPermissibleValue);
                                return result;
                            }

                            if (!measurementScale.IsMinimumInclusive && natural <= naturalMinimumPermissibleValue)
                            {
                                result.ResultKind = ValidationResultKind.OutOfBounds;
                                result.Message = string.Format("The value \"{0}\" is smaller than or equal to the minimum permissible value of \"{1}\"", natural, naturalMinimumPermissibleValue);
                                return result;
                            }
                        }
                        else
                        {
                            Logger.Warn("The MinimumPermissibleValue \"{0}\" of MeasurementScale \"{1}\" is not a member of the INTEGER NUMBER SET", measurementScale.MinimumPermissibleValue, measurementScale.Iid);
                        }
                    }

                    result.ResultKind = ValidationResultKind.Valid;
                    result.Message = string.Empty;
                    return result;

                case NumberSetKind.RATIONAL_NUMBER_SET:

                    Logger.Warn("RATIONAL NUMBER SET currently not validated and always returns ValidationResultKind.Valid");

                    result.ResultKind = ValidationResultKind.Valid;
                    result.Message = "RATIONAL NUMBER SET are not validated";
                    return result;

                case NumberSetKind.REAL_NUMBER_SET:

                    double real;
                    var isReal = double.TryParse(value, NumberStyles.Float, null, out real);

                    if (!isReal)
                    {
                        result.ResultKind = ValidationResultKind.Invalid;
                        result.Message = string.Format("\"{0}\" is not a member of the REAL NUMBER SET", value);
                        return result;
                    }

                    if (!string.IsNullOrWhiteSpace(measurementScale.MaximumPermissibleValue))
                    {
                        double realMaximumPermissibleValue;
                        isMaximumPermissibleValue = double.TryParse(measurementScale.MaximumPermissibleValue, NumberStyles.Float, null, out realMaximumPermissibleValue);
                        if (isMaximumPermissibleValue)
                        {
                            if (measurementScale.IsMaximumInclusive && real > realMaximumPermissibleValue)
                            {
                                result.ResultKind = ValidationResultKind.OutOfBounds;
                                result.Message = string.Format("The value \"{0}\" is greater than the maximium permissible value of \"{1}\"", real, realMaximumPermissibleValue);
                                return result;
                            }

                            if (!measurementScale.IsMaximumInclusive && real >= realMaximumPermissibleValue)
                            {
                                result.ResultKind = ValidationResultKind.OutOfBounds;
                                result.Message = string.Format("The value \"{0}\" is greater than or equal to the maximium permissible value of \"{1}\"", real, realMaximumPermissibleValue);
                                return result;
                            }
                        }
                        else
                        {
                            Logger.Warn("The MaximumPermissibleValue \"{0}\" of MeasurementScale \"{1}\" is not a member of the INTEGER NUMBER SET", measurementScale.MaximumPermissibleValue, measurementScale.Iid);
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(measurementScale.MinimumPermissibleValue))
                    {
                        double realMinimumPermissibleValue;
                        isMinimumPermissibleValue = double.TryParse(measurementScale.MinimumPermissibleValue, NumberStyles.Integer, null, out realMinimumPermissibleValue);
                        if (isMinimumPermissibleValue)
                        {
                            if (measurementScale.IsMinimumInclusive && real < realMinimumPermissibleValue)
                            {
                                result.ResultKind = ValidationResultKind.OutOfBounds;
                                result.Message = string.Format("The value \"{0}\" is smaller than the minimum permissible value of \"{1}\"", real, realMinimumPermissibleValue);
                                return result;
                            }

                            if (!measurementScale.IsMinimumInclusive && real <= realMinimumPermissibleValue)
                            {
                                result.ResultKind = ValidationResultKind.OutOfBounds;
                                result.Message = string.Format("The value \"{0}\" is smaller than or equal to the minimum permissible value of \"{1}\"", real, realMinimumPermissibleValue);
                                return result;
                            }
                        }
                        else
                        {
                            Logger.Warn("The MinimumPermissibleValue \"{0}\" of MeasurementScale \"{1}\" is not a member of the INTEGER NUMBER SET", measurementScale.MinimumPermissibleValue, measurementScale.Iid);
                        }
                    }
                    
                    result.ResultKind = ValidationResultKind.Valid;
                    result.Message = string.Empty;
                    return result;

                default:
                    throw new Exception("Invalid NumberSetKind");
            }
        }
    }
}
