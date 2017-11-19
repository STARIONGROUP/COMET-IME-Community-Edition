// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ValueSetValues.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ParameterSheetGenerator.Generator.ParameterSheet
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// The <see cref="ValueSetValues"/> is a struct used to temporatily store the values of a <see cref="ParameterValueSet"/>, 
    /// <see cref="ParameterOverrideValueSet"/> or <see cref="ParameterSubscriptionValueSet"/> for a specific component index,
    /// which in the case of a <see cref="ScalarParameterType"/> is zero
    /// </summary>
    public class ValueSetValues
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValueSetValues"/> class
        /// </summary>
        /// <param name="componentIndex">
        /// The index of the <see cref="ParameterTypeComponent"/> of the value
        /// </param>
        /// <param name="parameterType">
        /// The <see cref="ParameterType"/> of the container <see cref="Parameter"/>, <see cref="ParameterOverride"/> or <see cref="ParameterSubscription"/>
        /// </param>
        /// <param name="switchKind">
        /// The <see cref="SwitchKind"/> of the valueset
        /// </param>
        /// <param name="manualValue">
        /// The manual value
        /// </param>
        /// <param name="computedValue">
        /// The computed value
        /// </param>
        /// <param name="referenceValue">
        /// the reference value
        /// </param>
        /// <param name="formulaValue">
        /// The formula value
        /// </param>
        public ValueSetValues(int componentIndex, ParameterType parameterType, ParameterSwitchKind switchKind, string manualValue, string computedValue, string referenceValue, string formulaValue)
        {
            this.ComponentIndex = componentIndex;
            this.ParameterType = parameterType;
            this.SwitchKind = switchKind;
            this.ManualValue = manualValue;
            this.ComputedValue = computedValue;
            this.ReferenceValue = referenceValue;
            this.FormulaValue = formulaValue;
        }

        /// <summary>
        /// Gets the component index, which in the case of a <see cref="ScalarParameterType"/> is always zero.
        /// </summary>
        public int ComponentIndex { get; private set; }

        /// <summary>
        /// Gets te <see cref="Parameter"/>
        /// </summary>
        public ParameterType ParameterType { get; private set; }

        /// <summary>
        /// Gets the <see cref="ParameterSwitchKind"/>
        /// </summary>
        public ParameterSwitchKind SwitchKind { get; private set; }

        /// <summary>
        /// Gets the string representation of the manual value
        /// </summary>
        public string ManualValue { get; private set; }

        /// <summary>
        /// Gets the string representation of the computed value
        /// </summary>
        public string ComputedValue { get; private set; }

        /// <summary>
        /// Gets the string representation of the reference value
        /// </summary>
        public string ReferenceValue { get; private set; }

        /// <summary>
        /// Gets the string representation of the formula value
        /// </summary>
        public string FormulaValue { get; private set; }
    }
}
