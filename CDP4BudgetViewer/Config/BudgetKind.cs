// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BudgetKind.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Budget.Services
{
    /// <summary>
    /// Assertion on the kind of budget to calculate
    /// </summary>
    public enum BudgetKind
    {
        /// <summary>
        /// Asserts that a cost budget shall be computed
        /// </summary>
        Cost,

        /// <summary>
        /// Asserts that a mass budget shall be calculated
        /// </summary>
        Mass,

        /// <summary>
        /// Asserts that a power budget shall be calculated
        /// </summary>
        //Power,
    }
}
