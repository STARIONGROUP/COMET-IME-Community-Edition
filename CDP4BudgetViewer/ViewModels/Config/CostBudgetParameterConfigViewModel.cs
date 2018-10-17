// -------------------------------------------------------------------------------------------------
// <copyright file="CostBudgetParameterConfigViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Budget.ViewModels
{
    using System;
    using System.Collections.Generic;
    using CDP4Common.SiteDirectoryData;
    using ReactiveUI;
    using Services;

    /// <summary>
    /// The view-model used to setup configuration for the budget view
    /// </summary>
    public class CostBudgetParameterConfigViewModel : BudgetParameterConfigViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CostBudgetParameterConfigViewModel"/> class
        /// </summary>
        /// <param name="possibleParameterTypes">The possible <see cref="QuantityKind"/></param>
        /// <param name="validateMainForm">The main form validator to call on any changes</param>
        public CostBudgetParameterConfigViewModel(IReadOnlyList<QuantityKind> possibleParameterTypes, Action validateMainForm) : base(validateMainForm)
        {
            this.GenericConfig = new ParameterConfigViewModel(possibleParameterTypes, validateMainForm);
        }

        /// <summary>
        /// Gets the <see cref="ParameterConfigViewModel"/>
        /// </summary>
        public ParameterConfigViewModel GenericConfig { get; }

        /// <summary>
        /// Gets the <see cref="BudgetKind"/> associated to this <see cref="CostBudgetParameterConfigViewModel"/>
        /// </summary>
        public override BudgetKind BudgetKind => BudgetKind.Cost;

        /// <summary>
        /// Asserts whether the view-model is valid
        /// </summary>
        /// <returns>True if it is</returns>
        public override bool IsFormValid()
        {
            return this.GenericConfig.SelectedParameterType != null;
        }
    }
}
