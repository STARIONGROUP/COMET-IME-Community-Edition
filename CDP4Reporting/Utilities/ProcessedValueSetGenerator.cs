// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProcessedValueSetGenerator.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2023 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Reporting.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Helpers;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Validation;
    
    using CDP4Composition.ViewModels;

    using CDP4Reporting.DataCollection;
    using CDP4Reporting.SubmittableParameterValues;

    using CommonServiceLocator;

    /// <summary>
    /// Helper class to create <see cref="ProcessedValueSet"/>s using data from an <see cref="IIterationDependentDataCollector"/>.
    /// </summary>
    public class ProcessedValueSetGenerator
    {
        /// <summary>
        ///Gets or sets the <see cref="IIterationDependentDataCollector"/> to be used.
        /// </summary>
        public IIterationDependentDataCollector IterationDependentDataCollector { get; private set; }

        /// <summary>
        /// Instantiates an instance of <see cref="ProcessedValueSetGenerator"/>
        /// </summary>
        /// <param name="iterationDependentDataCollector">
        ///The <see cref="IIterationDependentDataCollector"/> to be used.
        /// </param>
        public ProcessedValueSetGenerator(IIterationDependentDataCollector iterationDependentDataCollector)
        {
            this.IterationDependentDataCollector = iterationDependentDataCollector;
        }

        /// <summary>
        /// Try to create/change a <see cref="ProcessedValueSet"/> for a <see cref="Parameter"/>, <see cref="ParameterOverride"/>, or <see cref="ParameterSubscription"/>
        /// found by searching for a specific <see cref="NestedParameter.Path"/>.
        /// </summary>
        /// <param name="option">
        /// The <see cref="Option"/> to be used.
        /// </param>
        /// <param name="allNestedParameters">
        /// An <see cref="IEnumerable{NestedParameter}"/> that contains all nested parameters for a specific ProductTree.
        /// </param>
        /// <param name="ownedNestedParameters">
        /// An <see cref="IEnumerable{NestedParameter}"/> that contains only the nested parameters for a specific ProductTree
        /// that are available to a specific <see cref="DomainOfExpertise"/>.
        /// </param>
        /// <param name="submittableParameterValue">
        /// The <see cref="SubmittableParameterValue"/> that contains 
        /// </param>
        /// <param name="processedValueSets">A <see cref="Dictionary{Guid, ProcessedValueSet}"/> that contains all <see cref="ProcessedValueSet"/> found until now.</param>
        /// <param name="errorText">
        /// A <see cref="string"/> that contains information about problems that occured during the process.
        /// </param>
        /// <returns>
        /// true if <see cref="NestedParameter.Path"/> was found and the <see cref="ProcessedValueSet"/> was succesfully created and processed, otherwise false.
        /// </returns>
        public bool TryGetProcessedValueSet(Option option, IEnumerable<NestedParameter> allNestedParameters, IEnumerable<NestedParameter> ownedNestedParameters, SubmittableParameterValue submittableParameterValue, ref Dictionary<Guid, ProcessedValueSet> processedValueSets, out string errorText)
        {
            if (!this.ValueSetWriteAllowed(submittableParameterValue, option))
            {
                // This value should not be written, but in this case no error should be returned as it is as expected
                errorText = "";
                return true;
            }

            var path = ReportingUtilities.ConvertToOptionPath(submittableParameterValue.Path, option);

            var nestedParameters = option.GetNestedParameterValueSetsByPath(path, allNestedParameters).ToList();

            if (nestedParameters.Count == 0)
            {
                errorText = $"Path {path} was not found in Product Tree for Option {option.ShortName}";
                return false;
            }

            nestedParameters = option.GetNestedParameterValueSetsByPath(path, ownedNestedParameters).ToList();

            if (nestedParameters.Count == 0)
            {
                errorText = $"Domain {this.IterationDependentDataCollector.DomainOfExpertise.Name} does not have access to Path {path}.";
                return false;
            }

            if (nestedParameters.Count > 1)
            {
                errorText = $"Multiple parameters found for Path {path}. Cannot write the value back to the model.";
                return false;
            }

            var nestedParameter = nestedParameters.Single();

            if (nestedParameter.AssociatedParameter.ParameterType.NumberOfValues > 1)
            {
                errorText = $"ParameterType {nestedParameter.AssociatedParameter.ParameterType.Name} is of a compound parameter type. Writing back values to the model for Compound parameter types is currently not supported.";
                return false;
            }

            if (!this.IterationDependentDataCollector.Session.PermissionService.CanWrite(nestedParameter.AssociatedParameter))
            {
                errorText = $"Domain {this.IterationDependentDataCollector.DomainOfExpertise.Name} does not have write access to Path {path}.";
                return false;
            }

            if (nestedParameter.AssociatedParameter is ParameterOrOverrideBase parameterOrOverrideBase && nestedParameter.ValueSet is ParameterValueSetBase parameterValueSetBase)
            {
                var processedValueSet = this.ProcessValueSet(parameterValueSetBase, parameterOrOverrideBase, 0, submittableParameterValue.Text, ref processedValueSets, null, out errorText);
                return processedValueSet.ValidationResult == ValidationResultKind.Valid;
            }

            if (nestedParameter.AssociatedParameter is ParameterSubscription parameterSubscription && nestedParameter.ValueSet is ParameterSubscriptionValueSet parameterSubscriptionValueSet)
            {
                var processedValueSet = this.ProcessValueSet(parameterSubscriptionValueSet, parameterSubscription, 0, submittableParameterValue.Text, ref processedValueSets, null, out errorText);
                return processedValueSet.ValidationResult == ValidationResultKind.Valid;
            }

            errorText = $"Update Path {path} failed due to unexpected valueSet type: {nestedParameter.ValueSet.GetType()}";

            return false;
        }
        
        /// <summary>
        /// Checks if a data in a <see cref="SubmittableParameterValue"/> should be written to a ValueSet if found.
        /// </summary>
        /// <param name="submittableParameterValue">The <see cref="SubmittableParameterValue"/></param>
        /// <param name="option">The <see cref="Option"/> used to check if writing a value is allowed</param>
        /// <returns>true is write is allowed, otherwise false</returns>
        public bool ValueSetWriteAllowed(SubmittableParameterValue submittableParameterValue, Option option)
        {            
            var currentOptionPath = ReportingUtilities.ConvertToOptionPath(submittableParameterValue.Path, option);

            if (submittableParameterValue.IsExactOptionPath)
            {
                //Option should be defined in SubmittableParameterValue. Does the Option match the expected value based on the current Option?
                return submittableParameterValue.Path == currentOptionPath;
            }

            return true;
        }

        /// <summary>
        /// Process the <see cref="ParameterValueSet"/> .
        /// </summary>
        /// <param name="parameterValueSet">
        /// The <see cref="ParameterValueSetBase"/> to be processed.
        /// </param>
        /// <param name="parameterOrOverrideBase">
        /// The <see cref="ParameterOrOverrideBase"/> the <see cref="ParameterValueSetBase"/ belongs to.
        /// </param>
        /// <param name="componentIndex">
        /// The index of the <see cref="ParameterTypeComponent"/>.
        /// </param>
        /// <param name="computedValue">
        /// The manual value of the <see cref="ParameterValueSet"/>.
        /// </param>
        /// <param name="processedValueSets">
        /// A <see cref="Dictionary{Guid,ProcessedValueSet}"/> of ProcessedValueSe that capture the updated <see cref="Thing"/>s with its value validation result
        /// </param>
        /// <param name="provider">
        /// The <see cref="IFormatProvider"/> used to validate.
        /// </param>
        /// <param name="validationErrorText">
        /// <see cref="String"/> that holds the validation error text.
        /// </param>
        /// <returns>
        /// The added or changed <see cref="ProcessedValueSet"/>
        /// </returns>
        private ProcessedValueSet ProcessValueSet(ParameterValueSetBase parameterValueSet, ParameterOrOverrideBase parameterOrOverrideBase, int componentIndex, string computedValue, ref Dictionary<Guid, ProcessedValueSet> processedValueSets, IFormatProvider provider, out string validationErrorText)
        {
            var validationResult = ValidationResultKind.InConclusive;

            var switchKind = parameterValueSet.ValueSwitch;
            var measurementScale = parameterOrOverrideBase.Scale;
            var parameterType = parameterOrOverrideBase.ParameterType;

            validationErrorText = string.Empty;

            if (parameterType != null)
            {
                if (ValueSetConverter.TryParseDouble(computedValue, parameterType, out var convertedComputedValue))
                {
                    computedValue = convertedComputedValue.ToString(CultureInfo.InvariantCulture);
                }

                computedValue = computedValue?.ToValueSetObject(parameterType).ToValueSetString(parameterType) ?? parameterValueSet.Computed[componentIndex];

                var validManualValue = parameterType.Validate(computedValue, measurementScale, provider);

                if (validManualValue.ResultKind > validationResult)
                {
                    validationResult = validManualValue.ResultKind;
                    validationErrorText = validManualValue.Message;
                }
            }

            var valueSetExists = processedValueSets.TryGetValue(parameterValueSet.Iid, out var processedValueSet);

            if (!valueSetExists)
            {
                processedValueSet = new ProcessedValueSet(parameterValueSet, validationResult);
            }

            if (processedValueSet.IsDirty(componentIndex, parameterType, switchKind, parameterValueSet.Manual[componentIndex], computedValue, parameterValueSet.Reference[componentIndex], computedValue, out var valueSetValues))
            {
                processedValueSet.UpdateClone(valueSetValues);

                if (!valueSetExists)
                {
                    processedValueSets.Add(parameterValueSet.Iid, processedValueSet);
                }                
            }

            return processedValueSet;
        }

        /// <summary>
        /// Process the <see cref="ParameterValueSet"/> .
        /// </summary>
        /// <param name="parameterSubscriptionValueSet">
        /// The <see cref="ParameterSubscriptionValueSet"/> to be processed.
        /// </param>
        /// <param name="parameterSubscription">
        /// The <see cref="ParameterSubscription"/> the <see cref="ParameterSubscriptionValueSet"/ belongs to.
        /// </param>
        /// <param name="componentIndex">
        /// The index of the <see cref="ParameterTypeComponent"/>.
        /// </param>
        /// <param name="computedValue">
        /// The manual value of the <see cref="ParameterValueSet"/>.
        /// </param>
        /// <param name="processedValueSets">
        /// A <see cref="Dictionary{Guid,ProcessedValueSet}"/> of ProcessedValueSe that capture the updated <see cref="Thing"/>s with its value validation result
        /// </param>
        /// <param name="provider">
        /// The <see cref="IFormatProvider"/> used to validate.
        /// </param>
        /// <param name="validationErrorText">
        /// <see cref="String"/> that holds the validation error text.
        /// </param>
        /// <returns>
        /// The added or changed <see cref="ProcessedValueSet"/>
        /// </returns>
        private ProcessedValueSet ProcessValueSet(ParameterSubscriptionValueSet parameterSubscriptionValueSet, ParameterSubscription parameterSubscription, int componentIndex, string computedValue, ref Dictionary<Guid, ProcessedValueSet> processedValueSets, IFormatProvider provider, out string validationErrorText)
        {
            var validationResult = ValidationResultKind.InConclusive;

            var switchKind = parameterSubscriptionValueSet.ValueSwitch;
            var measurementScale = parameterSubscription.Scale;
            var parameterType = parameterSubscription.ParameterType;

            validationErrorText = string.Empty;

            if (parameterType != null)
            {
                computedValue = computedValue.ToValueSetObject(parameterType).ToValueSetString(parameterType);

                var validManualValue = parameterType.Validate(computedValue, measurementScale, provider);

                if (validManualValue.ResultKind > validationResult)
                {
                    validationResult = validManualValue.ResultKind;
                    validationErrorText = validManualValue.Message;
                }
            }

            var valueSetExists = processedValueSets.TryGetValue(parameterSubscriptionValueSet.Iid, out var processedValueSet);

            if (!valueSetExists)
            {
                processedValueSet = new ProcessedValueSet(parameterSubscriptionValueSet, validationResult);
            }

            if (processedValueSet.IsDirty(componentIndex, parameterType, switchKind, computedValue, null, null, null, out var valueSetValues))
            {
                processedValueSet.UpdateClone(valueSetValues);

                if (!valueSetExists)
                {
                    processedValueSets.Add(parameterSubscriptionValueSet.Iid, processedValueSet);
                }                
            }

            return processedValueSet;
        }
    }
}
