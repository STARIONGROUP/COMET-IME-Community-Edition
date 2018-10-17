// -------------------------------------------------------------------------------------------------
// <copyright file="PowerBudgetParameterConfigViewModel.cs" company="RHEA System S.A.">
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
    /// TODO
    //public class PowerBudgetParameterConfigViewModel : BudgetParameterConfigViewModelBase
    //{
    //    /// <summary>
    //    /// Initializes a new instance of the <see cref="PowerBudgetParameterConfigViewModel"/> class
    //    /// </summary>
    //    /// <param name="possibleParameterTypes">The possible <see cref="QuantityKind"/></param>
    //    /// <param name="validateMainForm">The main form validator</param>
    //    public PowerBudgetParameterConfigViewModel(IReadOnlyList<QuantityKind> possibleParameterTypes, Action validateMainForm) : base(validateMainForm)
    //    {
    //        this.GenericConfig = new ParameterConfigViewModel(possibleParameterTypes, validateMainForm);
    //    }

    //    /// <summary>
    //    /// Gets the <see cref="ParameterConfigViewModel"/> for this <see cref="PowerBudgetParameterConfigViewModel"/>
    //    /// </summary>
    //    public ParameterConfigViewModel GenericConfig { get; }

    //    /// <summary>
    //    /// Gets the <see cref="BudgetKind"/> associated to this <see cref="PowerBudgetParameterConfigViewModel"/>
    //    /// </summary>
    //    public override BudgetKind BudgetKind => BudgetKind.Power;

    //    /// <summary>
    //    /// Asserts whether the view-model is valid
    //    /// </summary>
    //    /// <returns>True if it is</returns>
    //    public override bool IsFormValid()
    //    {
    //        return this.GenericConfig.SelectedParameterType != null;
    //    }
    //}
}
