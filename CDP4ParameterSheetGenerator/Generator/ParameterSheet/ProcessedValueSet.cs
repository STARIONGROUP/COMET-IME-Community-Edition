// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProcessedValueSet.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ParameterSheetGenerator.Generator.ParameterSheet
{
    using System;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Helpers;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Validation;
    using NLog;

    /// <summary>
    /// The purpose of the <see cref="ProcessedValueSet"/> is to perform the comparison of the data coming from the Parameter Sheet
    /// with the <see cref="ParameterValueSet"/>, <see cref="ParameterOverrideValueSet"/> or <see cref="ParameterSubscriptionValueSet"/>
    /// that is retrieved from the workbook session cache. When the data on the sheet is different from the data in the workbook session cache
    /// a the clone of the <see cref="ParameterValueSet"/>, <see cref="ParameterOverrideValueSet"/> or <see cref="ParameterSubscriptionValueSet"/>
    /// is updated with that data so that the change be persisted in the data-source.
    /// </summary>
    public class ProcessedValueSet
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessedValueSet"/> class.
        /// </summary>
        /// <param name="thing">
        /// The <see cref="Thing"/> that is processed by the current <see cref="ProcessedValueSet"/>
        /// </param>        
        /// <param name="validationResult">
        /// the valiation result
        /// </param>
        public ProcessedValueSet(Thing thing, ValidationResultKind validationResult)
        {
            if (thing is ParameterValueSet || thing is ParameterOverrideValueSet || thing is ParameterSubscriptionValueSet)
            {
                this.OriginalThing = thing;                
                this.ValidationResult = validationResult;
            }
            else
            {
                throw new ArgumentException("The thing must be a ParameterValueSet or a ParameterOverrideValueSet or a ParameterSubscriptionValueSet", "thing");
            }            
        }

        /// <summary>
        /// Asserts whether the <see cref="ParameterValueSet"/>, <see cref="ParameterOverrideValueSet"/> or <see cref="ParameterSubscriptionValueSet"/> that is represented
        /// by the current <see cref="ProcessedValueSet"/> is Dirty or not
        /// </summary>
        /// <param name="componentIndex">
        /// The index of the component of the <see cref="CompoundParameterType"/>
        /// </param>
        /// <param name="parameterType">
        /// The <see cref="ParameterType"/> that is referenced by the container <see cref="Parameter"/>, <see cref="ParameterOverride"/>, or <see cref="ParameterSubscription"/>
        /// </param>
        /// <param name="switchKind">
        /// The <see cref="ParameterSwitchKind"/> that is read from the Parameter sheet
        /// </param>
        /// <param name="manualValue">
        /// The manual value that is read from the Parameter sheet
        /// </param>
        /// <param name="computedValue">
        /// The computed value that is read from the Parameter sheet
        /// </param>
        /// <param name="referenceValue">
        /// The reference value that is read from the Parameter sheet
        /// </param>
        /// <param name="formulaValue">
        /// The formula value that is read from the Parameter sheet
        /// </param>
        /// <param name="valueSetValues">
        /// The <see cref="ValueSetValues"/> that is created and returned as an out when the return value is true.
        /// </param>
        /// <returns>
        /// True when the values in the Parameter sheet are different from the values of the the <see cref="ParameterValueSet"/>, <see cref="ParameterOverrideValueSet"/> 
        /// or <see cref="ParameterSubscriptionValueSet"/> that is being represented by the current <see cref="ProcessedValueSet"/>
        /// </returns>
        public bool IsDirty(int componentIndex, ParameterType parameterType, ParameterSwitchKind switchKind, object manualValue, object computedValue, object referenceValue, string formulaValue, out ValueSetValues valueSetValues)
        {
            var parameterValueSet = this.OriginalThing as ParameterValueSet;
            if (parameterValueSet != null)
            {
                return this.IsDirty(parameterValueSet, componentIndex, parameterType, switchKind, manualValue, computedValue, referenceValue, formulaValue, out valueSetValues);
            }

            var parameterOverrideValueSet = this.OriginalThing as ParameterOverrideValueSet;
            if (parameterOverrideValueSet != null)
            {
                return this.IsDirty(parameterOverrideValueSet, componentIndex, parameterType, switchKind, manualValue, computedValue, referenceValue, formulaValue, out valueSetValues);
            }

            var parameterSubscriptionValueSet = this.OriginalThing as ParameterSubscriptionValueSet;            
            return this.IsDirty(parameterSubscriptionValueSet, componentIndex, parameterType, switchKind, manualValue, out valueSetValues);
        }

        /// <summary>
        /// Asserts whether the <see cref="ParameterValueSet"/> that is represented by the current <see cref="ProcessedValueSet"/> is Dirty or not
        /// </summary>
        /// <param name="original">
        /// The <see cref="ParameterValueSet"/> whose values are being compared to assert the dirtyness
        /// </param>
        /// <param name="componentIndex">
        /// The index of the component of the <see cref="CompoundParameterType"/>
        /// </param>
        /// <param name="parameterType">
        /// The <see cref="ParameterType"/> that is referenced by the container <see cref="Parameter"/>
        /// </param>
        /// <param name="switchKind">
        /// The <see cref="ParameterSwitchKind"/> that is read from the Parameter sheet
        /// </param>
        /// <param name="manualValue">
        /// The manual value that is read from the Parameter sheet
        /// </param>
        /// <param name="computedValue">
        /// The computed value that is read from the Parameter sheet
        /// </param>
        /// <param name="referenceValue">
        /// The reference value that is read from the Parameter sheet
        /// </param>
        /// <param name="formulaValue">
        /// The formula value that is read from the Parameter sheet
        /// </param>
        /// <param name="valueSetValues">
        /// The <see cref="ValueSetValues"/> that is created and returned as an out when the return value is true.
        /// </param>
        /// <returns>
        /// True when the values in the Parameter sheet are different from the values of the the <see cref="ParameterValueSet"/> that is being represented by the current <see cref="ProcessedValueSet"/>
        /// </returns>
        private bool IsDirty(ParameterValueSet original, int componentIndex, ParameterType parameterType, ParameterSwitchKind switchKind, object manualValue, object computedValue, object referenceValue, string formulaValue, out ValueSetValues valueSetValues)
        {
            // here we use CultureInfo.InvariantCulture, these are values that are not shown to the user
            // but serve the purpose to update the data-source.
            var stringManualValue = manualValue.ToValueSetString(parameterType);
            var stringComputedValue = computedValue.ToValueSetString(parameterType);
            var stringReferenceValue = referenceValue.ToValueSetString(parameterType);

            bool isManualValueDirty;
            try
            {
                isManualValueDirty = original.Manual[componentIndex] != stringManualValue;
            }
            catch (ArgumentOutOfRangeException)
            {
                isManualValueDirty = true;
                logger.Debug("The ParameterValueSet.Manual ValueArray has an incorrect number of slots {0}", original.Iid);
            }
            catch (Exception exception)
            {
                throw exception;
            }

            bool isFormualValueDirty;          
            try
            {
                isFormualValueDirty = original.Formula[componentIndex] != formulaValue;
            }
            catch (ArgumentOutOfRangeException)
            {
                isFormualValueDirty = true;
                logger.Debug("The ParameterValueSet.Formula ValueArray has an incorrect number of slots {0}", original.Iid);
            }
            catch (Exception exception)
            {
                throw exception;
            }

            bool isComputedValueDirty;
            try
            {
                isComputedValueDirty = original.Computed[componentIndex] != stringComputedValue;
            }
            catch (ArgumentOutOfRangeException)
            {
                isComputedValueDirty = true;
                logger.Debug("The ParameterValueSet.Computed ValueArray has an incorrect number of slots {0}", original.Iid);
            }
            catch (Exception exception)
            {
                throw exception;
            }

            bool isReferenceValueDirty;
            try
            {
                isReferenceValueDirty = original.Reference[componentIndex] != stringReferenceValue;
            }
            catch (ArgumentOutOfRangeException)
            {
                isReferenceValueDirty = true;
                logger.Debug("The ParameterValueSet.Reference ValueArray has an incorrect number of slots {0}", original.Iid);
            }
            catch (Exception exception)
            {
                throw exception;
            }

            var isSwitchKindDirty = original.ValueSwitch != switchKind;
            
            if (isManualValueDirty || isFormualValueDirty || isComputedValueDirty || isReferenceValueDirty || isSwitchKindDirty)
            {
                valueSetValues = new ValueSetValues(componentIndex, parameterType, switchKind, stringManualValue, stringComputedValue, stringReferenceValue, formulaValue);
                return true;
            }

            valueSetValues = null;
            return false;
        }

        /// <summary>
        /// Asserts whether the <see cref="ParameterOverrideValueSet"/> that is represented by the current <see cref="ProcessedValueSet"/> is Dirty or not
        /// </summary>
        /// <param name="original">
        /// The <see cref="ParameterOverrideValueSet"/> whose values are being compared to assert the dirtyness
        /// </param>
        /// <param name="componentIndex">
        /// The index of the component of the <see cref="CompoundParameterType"/>
        /// </param>
        /// <param name="parameterType">
        /// The <see cref="ParameterType"/> that is referenced by the container <see cref="ParameterOverride"/>
        /// </param>
        /// <param name="switchKind">
        /// The <see cref="ParameterSwitchKind"/> that is read from the Parameter sheet
        /// </param>
        /// <param name="manualValue">
        /// The manual value that is read from the Parameter sheet
        /// </param>
        /// <param name="computedValue">
        /// The computed value that is read from the Parameter sheet
        /// </param>
        /// <param name="referenceValue">
        /// The reference value that is read from the Parameter sheet
        /// </param>
        /// <param name="formulaValue">
        /// The formula value that is read from the Parameter sheet
        /// </param>
        /// <param name="valueSetValues">
        /// The <see cref="ValueSetValues"/> that is created and returned as an out when the return value is true.
        /// </param>
        /// <returns>
        /// True when the values in the Parameter sheet are different from the values of the the <see cref="ParameterOverrideValueSet"/> that is being represented by the current <see cref="ProcessedValueSet"/>
        /// </returns>
        private bool IsDirty(ParameterOverrideValueSet original, int componentIndex, ParameterType parameterType, ParameterSwitchKind switchKind, object manualValue, object computedValue, object referenceValue, string formulaValue, out ValueSetValues valueSetValues)
        {
            // here we use CultureInfo.InvariantCulture, these are values that are not shown to the user
            // but serve the purpose to update the data-source.
            var stringManualValue = manualValue.ToValueSetString(parameterType);
            var stringComputedValue = computedValue.ToValueSetString(parameterType);
            var stringReferenceValue = referenceValue.ToValueSetString(parameterType);

            bool isManualValueDirty;
            try
            {
                isManualValueDirty = original.Manual[componentIndex] != stringManualValue;
            }
            catch (ArgumentOutOfRangeException e)
            {
                isManualValueDirty = true;
                logger.Debug("The ParameterOverrideValueSet.Manual ValueArray has an incorrect number of slots {0}", original.Iid);
            }
            catch (Exception e)
            {
                throw e;
            }
            
            bool isFormualValueDirty;
            try
            {
                isFormualValueDirty = original.Formula[componentIndex] != formulaValue;
            }
            catch (ArgumentOutOfRangeException e)
            {
                isFormualValueDirty = true;
                logger.Debug("The ParameterOverrideValueSet.Formula ValueArray has an incorrect number of slots {0}", original.Iid);
            }
            catch (Exception e)
            {
                throw e;
            }
            
            bool isComputedValueDirty;            
            try
            {
                isComputedValueDirty = original.Computed[componentIndex] != stringComputedValue;
            }
            catch (ArgumentOutOfRangeException e)
            {
                isComputedValueDirty = true;
                logger.Debug("The ParameterOverrideValueSet.Computed ValueArray has an incorrect number of slots {0}", original.Iid);
            }
            catch (Exception e)
            {
                throw e;
            }

            bool isReferenceValueDirty;            
            try
            {
                isReferenceValueDirty = original.Reference[componentIndex] != stringReferenceValue;
            }
            catch (ArgumentOutOfRangeException e)
            {
                isReferenceValueDirty = true;
                logger.Debug("The ParameterOverrideValueSet.Reference ValueArray has an incorrect number of slots {0}", original.Iid);
            }
            catch (Exception e)
            {
                throw e;
            }

            var isSwitchKindDirty = original.ValueSwitch != switchKind;

            if (isManualValueDirty || isFormualValueDirty || isComputedValueDirty || isReferenceValueDirty || isSwitchKindDirty)
            {
                valueSetValues = new ValueSetValues(componentIndex, parameterType, switchKind, stringManualValue, stringComputedValue, stringReferenceValue, formulaValue);
                return true;
            }

            valueSetValues = null;
            return false;
        }

        /// <summary>
        /// Asserts whether the <see cref="ParameterSubscriptionValueSet"/> that is represented by the current <see cref="ProcessedValueSet"/> is Dirty or not
        /// </summary>
        /// <param name="original">
        /// The <see cref="ParameterSubscriptionValueSet"/> whose values are being compared to assert the dirtyness
        /// </param>
        /// <param name="componentIndex">
        /// The index of the component of the <see cref="CompoundParameterType"/>
        /// </param>
        /// <param name="parameterType">
        /// The <see cref="ParameterType"/> that is referenced by the container <see cref="ParameterSubscriptionValueSet"/>
        /// </param>
        /// <param name="switchKind">
        /// The <see cref="ParameterSwitchKind"/> that is read from the Parameter sheet
        /// </param>
        /// <param name="manualValue">
        /// The manual value that is read from the Parameter sheet
        /// </param>
        /// <param name="valueSetValues">
        /// The <see cref="ValueSetValues"/> that is created and returned as an out when the return value is true.
        /// </param>
        /// <returns>
        /// True when the values in the Parameter sheet are different from the values of the the <see cref="ParameterSubscriptionValueSet"/> that is being represented by the current <see cref="ProcessedValueSet"/>
        /// </returns>
        private bool IsDirty(ParameterSubscriptionValueSet original, int componentIndex, ParameterType parameterType, ParameterSwitchKind switchKind, object manualValue, out ValueSetValues valueSetValues)
        {
            // here we use CultureInfo.InvariantCulture, these are values that are not shown to the user
            // but serve the purpose to update the data-source.
            var stringManualValue = manualValue.ToValueSetString(parameterType);

            bool isManualValueDirty;
            try
            {
                isManualValueDirty = original.Manual[componentIndex] != stringManualValue;
            }
            catch (ArgumentOutOfRangeException e)
            {
                isManualValueDirty = true;
                logger.Debug("The ParameterSubscriptionValueSet.Manual ValueArray has an incorrect number of slots {0}", original.Iid);
            }
            catch (Exception e)
            {
                throw e;
            }

            var isSwitchKindDirty = original.ValueSwitch != switchKind;

            if (isManualValueDirty || isSwitchKindDirty)
            {
                valueSetValues = new ValueSetValues(componentIndex, parameterType, switchKind, stringManualValue, null, null, null);
                return true;
            }

            valueSetValues = null;
            return false;
        }
        
        /// <summary>
        /// Gets the <see cref="Thing"/> that is represented by the current <see cref="ProcessedValueSet"/>. It can only be a
        /// <see cref="ParameterValueSet"/>, <see cref="ParameterOverrideValueSet"/> or <see cref="ParameterSubscriptionValueSet"/>
        /// </summary>
        /// <remarks>
        /// This is the original <see cref="Thing"/>, not a clone of that <see cref="Thing"/>.
        /// </remarks>
        public Thing OriginalThing { get; private set; }

        /// <summary>
        /// Gets the clone of the <see cref="Thing"/> that is represented by the current <see cref="ProcessedValueSet"/>. It can only be a
        /// <see cref="ParameterValueSet"/>, <see cref="ParameterOverrideValueSet"/> or <see cref="ParameterSubscriptionValueSet"/>
        /// </summary>
        /// <remarks>
        /// This is the clone of the <see cref="OriginalThing"/>.
        /// </remarks>
        public Thing ClonedThing { get; private set; }
        
        /// <summary>
        /// Gets the result of the validation of the data
        /// </summary>
        public ValidationResultKind ValidationResult { get; private set; }

        /// <summary>
        /// Assert whether the Manual value of the <see cref="ClonedThing"/> is different from the <see cref="OriginalThing"/>
        /// </summary>
        /// <param name="componentIndex">
        /// The <see cref="ParameterTypeComponent"/> index
        /// </param>
        /// <returns>
        /// true of dirty, false if not
        /// </returns>
        public bool IsManualValueDirty(int componentIndex)
        {
            if (this.ClonedThing == null)
            {
                throw new InvalidOperationException("The ClonedThing is null");
            }

            var originalvalueset = this.OriginalThing as IValueSet;
            var clonedValueset = this.ClonedThing as IValueSet;
            
            try
            {
                if (originalvalueset.Manual[componentIndex] != clonedValueset.Manual[componentIndex])
                {
                    return true;
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                logger.Debug("The IValueSet.Manual ValueArray has an incorrect number of slots");
                return true;
            }
            catch (Exception exception)
            {
                throw exception;
            }
            
            return false;
        }

        /// <summary>
        /// Assert whether the Computed value of the <see cref="ClonedThing"/> is different from the <see cref="OriginalThing"/>
        /// </summary>
        /// <param name="componentIndex">
        /// The <see cref="ParameterTypeComponent"/> index
        /// </param>
        /// <returns>
        /// true of dirty, false if not
        /// </returns>
        public bool IsComputedValueDirty(int componentIndex)
        {
            if (this.ClonedThing == null)
            {
                throw new InvalidOperationException("The ClonedThing is null");
            }

            if (this.OriginalThing is ParameterSubscriptionValueSet)
            {
                return false;
            }

            var originalvalueset = this.OriginalThing as IValueSet;
            var clonedValueset = this.ClonedThing as IValueSet;

            try
            {
                if (originalvalueset.Computed[componentIndex] != clonedValueset.Computed[componentIndex])
                {
                    return true;
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                logger.Debug("The IValueSet.Computed ValueArray has an incorrect number of slots");
                return true;
            }
            catch (Exception exception)
            {
                throw exception;
            }
            
            return false;
        }

        /// <summary>
        /// Assert whether the Reference value of the <see cref="ClonedThing"/> is different from the <see cref="OriginalThing"/>
        /// </summary>
        /// <param name="componentIndex">
        /// The <see cref="ParameterTypeComponent"/> index
        /// </param>
        /// <returns>
        /// true of dirty, false if not
        /// </returns>
        public bool IsReferenceValueDirty(int componentIndex)
        {
            if (this.ClonedThing == null)
            {
                throw new InvalidOperationException("The ClonedThing is null");
            }

            if (this.OriginalThing is ParameterSubscriptionValueSet)
            {
                return false;
            }

            var originalvalueset = this.OriginalThing as IValueSet;
            var clonedValueset = this.ClonedThing as IValueSet;

            try
            {
                if (originalvalueset.Reference[componentIndex] != clonedValueset.Reference[componentIndex])
                {
                    return true;
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                logger.Debug("The IValueSet.Reference ValueArray has an incorrect number of slots");
                return true;
            }
            catch (Exception exception)
            {
                throw exception;
            }
            
            return false;
        }

        /// <summary>
        /// Assert whether the Formula value of the <see cref="ClonedThing"/> is different from the <see cref="OriginalThing"/>
        /// </summary>
        /// <param name="componentIndex">
        /// The <see cref="ParameterTypeComponent"/> index
        /// </param>
        /// <returns>
        /// true of dirty, false if not
        /// </returns>
        public bool IsFormulaValueDirty(int componentIndex)
        {
            if (this.ClonedThing == null)
            {
                throw new InvalidOperationException("The ClonedThing is null");
            }

            if (this.OriginalThing is ParameterSubscriptionValueSet)
            {
                return false;
            }

            var originalvalueset = this.OriginalThing as IValueSet;
            var clonedValueset = this.ClonedThing as IValueSet;

            try
            {
                if (originalvalueset.Formula[componentIndex] != clonedValueset.Formula[componentIndex])
                {
                    return true;
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                logger.Debug("The IValueSet.Formula ValueArray has an incorrect number of slots");
                return true;
            }
            catch (Exception exception)
            {
                throw exception;
            }
            
            return false;
        }

        /// <summary>
        /// Assert whether the Formula value of the <see cref="ClonedThing"/> is different from the <see cref="OriginalThing"/>
        /// </summary>
        /// <returns>
        /// true of dirty, false if not
        /// </returns>
        public bool IsValueSwitchDirty()
        {
            if (this.ClonedThing == null)
            {
                throw new InvalidOperationException("The ClonedThing is null");
            }

            var originalvalueset = this.OriginalThing as IValueSet;
            var clonedValueset = this.ClonedThing as IValueSet;
            
            if (originalvalueset.ValueSwitch != clonedValueset.ValueSwitch)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Update the values, formula and switch of the provided <see cref="ParameterValueSet"/>
        /// </summary>
        /// <param name="valueSetValues">
        /// The <see cref="ValueSetValues"/> used to update the <see cref="ParameterValueSet"/> clone with
        /// </param>
        public void UpdateClone(ValueSetValues valueSetValues)
        {
            if (this.ClonedThing == null)
            {
                this.SetClonedThing();
            }

            var clonedParameterValueSet = this.ClonedThing as ParameterValueSet;
            if (clonedParameterValueSet != null)
            {
                this.UpdateClone(clonedParameterValueSet, valueSetValues);
                return;
            }

            var clonedParameterOverrideValueSet = this.ClonedThing as ParameterOverrideValueSet;
            if (clonedParameterOverrideValueSet != null)
            {
                this.UpdateClone(clonedParameterOverrideValueSet, valueSetValues);
                return;
            }

            var clonedParameterSubscriptionValueSet = this.ClonedThing as ParameterSubscriptionValueSet;
            if (clonedParameterSubscriptionValueSet != null)
            {
                this.UpdateClone(clonedParameterSubscriptionValueSet, valueSetValues);
            }
        }

        /// <summary>
        /// Create a clone of the <see cref="OriginalThing"/> 
        /// </summary>
        private void SetClonedThing() 
        {
            var parameterValueSet = this.OriginalThing as ParameterValueSet;
            if (parameterValueSet != null)
            {
                this.ClonedThing = parameterValueSet.Clone(false);
            }

            var parameterOverrideValueSet = this.OriginalThing as ParameterOverrideValueSet;
            if (parameterOverrideValueSet != null)
            {
                this.ClonedThing = parameterOverrideValueSet.Clone(false);
            }

            var parameterSubscriptionValueSet = this.OriginalThing as ParameterSubscriptionValueSet;
            if (parameterSubscriptionValueSet != null)
            {
                this.ClonedThing = parameterSubscriptionValueSet.Clone(false);
            }
        }

        /// <summary>
        /// Update the values, formula and switch of the provided <see cref="ParameterValueSet"/>
        /// </summary>
        /// <param name="clone">
        /// The clone of the <see cref="ParameterValueSet"/> that is to be updated.
        /// </param>
        /// <param name="valueSetValues">
        /// The <see cref="ValueSetValues"/> used to update the <see cref="ParameterValueSet"/> clone with
        /// </param>
        private void UpdateClone(ParameterValueSet clone, ValueSetValues valueSetValues)
        {
            try
            {
                clone.Manual[valueSetValues.ComponentIndex] = valueSetValues.ManualValue;
            }
            catch (ArgumentOutOfRangeException)
            {
                clone.ResetManual();
                clone.Manual[valueSetValues.ComponentIndex] = valueSetValues.ManualValue;
            }
            catch (Exception exception)
            {
                throw exception;
            }
            
            try
            {
                clone.Formula[valueSetValues.ComponentIndex] = valueSetValues.FormulaValue;
            }
            catch (ArgumentOutOfRangeException)
            {
                clone.ResetFormula();
                clone.Formula[valueSetValues.ComponentIndex] = valueSetValues.FormulaValue;
            }
            catch (Exception exception)
            {
                throw exception;
            }

            try
            {
                clone.Computed[valueSetValues.ComponentIndex] = valueSetValues.ComputedValue;
            }
            catch (ArgumentOutOfRangeException)
            {
                clone.ResetComputed();
                clone.Computed[valueSetValues.ComponentIndex] = valueSetValues.ComputedValue;
            }
            catch (Exception exception)
            {
                throw exception;
            }

            try
            {
                clone.Reference[valueSetValues.ComponentIndex] = valueSetValues.ReferenceValue;
            }
            catch (ArgumentOutOfRangeException)
            {
                clone.ResetReference();
                clone.Reference[valueSetValues.ComponentIndex] = valueSetValues.ReferenceValue;
            }
            catch (Exception exception)
            {
                throw exception;
            }
            
            clone.ValueSwitch = valueSetValues.SwitchKind;
        }

        /// <summary>
        /// Update the values, formula and switch of the provided <see cref="ParameterOverrideValueSet"/>
        /// </summary>
        /// <param name="clone">
        /// The clone of the <see cref="ParameterOverrideValueSet"/> that is to be updated.
        /// </param>
        /// <param name="valueSetValues">
        /// The <see cref="ValueSetValues"/> used to update the <see cref="ParameterOverrideValueSet"/> clone with
        /// </param>
        private void UpdateClone(ParameterOverrideValueSet clone, ValueSetValues valueSetValues)
        {
            try
            {
                clone.Manual[valueSetValues.ComponentIndex] = valueSetValues.ManualValue;
            }
            catch (ArgumentOutOfRangeException)
            {
                clone.ResetManual();
                clone.Manual[valueSetValues.ComponentIndex] = valueSetValues.ManualValue;
            }
            catch (Exception exception)
            {
                throw exception;
            }
            
            try
            {
                clone.Computed[valueSetValues.ComponentIndex] = valueSetValues.ComputedValue;
            }
            catch (ArgumentOutOfRangeException)
            {
                clone.ResetComputed();
                clone.Computed[valueSetValues.ComponentIndex] = valueSetValues.ComputedValue;
            }
            catch (Exception exception)
            {
                throw exception;
            }
            
            try
            {
                clone.Formula[valueSetValues.ComponentIndex] = valueSetValues.FormulaValue;
            }
            catch (ArgumentOutOfRangeException)
            {
                clone.ResetFormula();
                clone.Formula[valueSetValues.ComponentIndex] = valueSetValues.FormulaValue;
            }
            catch (Exception exception)
            {
                throw exception;
            }
            
            try
            {
                clone.Reference[valueSetValues.ComponentIndex] = valueSetValues.ReferenceValue;
            }
            catch (ArgumentOutOfRangeException)
            {
                clone.ResetReference();
                clone.Reference[valueSetValues.ComponentIndex] = valueSetValues.ReferenceValue;
            }
            catch (Exception exception)
            {
                throw exception;
            }

            clone.ValueSwitch = valueSetValues.SwitchKind;
        }

        /// <summary>
        /// Update the values, formula and switch of the provided <see cref="ParameterSubscriptionValueSet"/>
        /// </summary>
        /// <param name="clone">
        /// The clone of the <see cref="ParameterSubscriptionValueSet"/> that is to be updated.
        /// </param>
        /// <param name="valueSetValues">
        /// The <see cref="ValueSetValues"/> used to update the <see cref="ParameterSubscriptionValueSet"/> clone with
        /// </param>
        private void UpdateClone(ParameterSubscriptionValueSet clone, ValueSetValues valueSetValues)
        {
            try
            {
                clone.Manual[valueSetValues.ComponentIndex] = valueSetValues.ManualValue;
            }
            catch (ArgumentOutOfRangeException)
            {
                clone.ResetManual();
                clone.Manual[valueSetValues.ComponentIndex] = valueSetValues.ManualValue;
            }
            catch (Exception exception)
            {
                throw exception;
            }
            
            clone.ValueSwitch = valueSetValues.SwitchKind;
        }
    }
}