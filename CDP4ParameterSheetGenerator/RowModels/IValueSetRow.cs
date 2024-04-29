// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IValueSetRow.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ParameterSheetGenerator.RowModels
{
    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// The <see cref="IValueSetRow"/> defines the properties that are required to populate
    /// excel rows that represent the ValueSet(s) of <see cref="Parameter"/>s, <see cref="ParameterOverride"/>s or <see cref="ParameterSubscription"/>s
    /// </summary>
    public interface IValueSetRow
    {
        /// <summary>
        /// Gets the manual value of the <see cref="ParameterValueSet"/>, <see cref="ParameterOverrideValueSet "/> or <see cref="ParameterSubscriptionValueSet"/> that this row represents
        /// </summary>
        object ManualValue { get; }

        /// <summary>
        /// Gets the computed value of the <see cref="ParameterValueSet"/>, <see cref="ParameterOverrideValueSet "/> or <see cref="ParameterSubscriptionValueSet"/> that this row represents
        /// </summary>
        object ComputedValue { get;  }

        /// <summary>
        /// Gets the reference value of the <see cref="ParameterValueSet"/>, <see cref="ParameterOverrideValueSet "/> or <see cref="ParameterSubscriptionValueSet"/> that this row represents
        /// </summary>
        object ReferenceValue { get; }

        /// <summary>
        /// Gets the switch of the <see cref="ParameterValueSet"/>, <see cref="ParameterOverrideValueSet "/> or <see cref="ParameterSubscriptionValueSet"/> that this row represents
        /// </summary>
        string Switch { get;  }

        /// <summary>
        /// Gets the actual value of the <see cref="ParameterValueSet"/>, <see cref="ParameterOverrideValueSet "/> or <see cref="ParameterSubscriptionValueSet"/> that this row represents
        /// </summary>
        object ActualValue { get; }

        /// <summary>
        /// Gets the name of the ParameterTypeShortName and Scale, if relevant, of the <see cref="ParameterValueSet"/>, <see cref="ParameterOverrideValueSet "/> or <see cref="ParameterSubscriptionValueSet"/> that this row represents
        /// </summary>
        string ParameterTypeScaleName { get; }

        /// <summary>
        /// Gets the model code or short-name path of the <see cref="ParameterValueSet"/>, <see cref="ParameterOverrideValueSet "/> or <see cref="ParameterSubscriptionValueSet"/> that this row represents
        /// </summary>
        string ModelCode { get; }
    }
}
