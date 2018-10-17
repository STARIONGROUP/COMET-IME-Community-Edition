// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BudgetParameterTypeTuple.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Budget.Config
{
    using CDP4Common.SiteDirectoryData;

    public struct BudgetParameterMarginPair
    {
        /// <summary>
        /// Initializes a new Parameter-Type/Margin pair
        /// </summary>
        /// <param name="main">The parameter-type representing the quantity to compute</param>
        /// <param name="margin">The associated margin <see cref="QuantityKind"/></param>
        public BudgetParameterMarginPair(QuantityKind main, QuantityKind margin)
        {
            this.MainParameterType = main;
            this.MarginParameterType = margin;
        }

        /// <summary>
        /// Gets the main parameter type in the budget computation
        /// </summary>
        public QuantityKind MainParameterType { get; private set; }

        /// <summary>
        /// Gets the margin parameter-type in the budget computation
        /// </summary>
        public QuantityKind MarginParameterType { get; private set; }
    }
}
