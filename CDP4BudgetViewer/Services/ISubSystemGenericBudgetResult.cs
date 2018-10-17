// -------------------------------------------------------------------------------------------------
// <copyright file="ISubSystemGenericBudgetResult.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Budget.Services
{
    public interface ISubSystemGenericBudgetResult : ISubSystemBudgetResult
    {
        /// <summary>
        /// Gets the value from the sub-system
        /// </summary>
        float? ValueFromSubSystem { get; }

        /// <summary>
        /// Gets the margin-ratio from the sub-system
        /// </summary>
        float? ValueMarginRatioFromSubSystem { get; }

        /// <summary>
        /// Gets the from the sub-system equipments
        /// </summary>
        float? ValueFromEquipment { get; }

        /// <summary>
        /// Gets the margin from the sub-system equipments
        /// </summary>
        float? ValueWithMarginFromEquipment { get; }

        /// <summary>
        /// Gets the margin-ratio from the sub-system equipments
        /// </summary>
        float? ValueMarginRatioFromEquipment { get; }
    }
}
