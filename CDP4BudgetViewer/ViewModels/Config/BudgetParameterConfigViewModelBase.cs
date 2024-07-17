// -------------------------------------------------------------------------------------------------
// <copyright file="BudgetParameterConfigViewModelBase.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2018 Starion Group S.A.
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
    public abstract class BudgetParameterConfigViewModelBase : ReactiveObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BudgetParameterConfigViewModelBase"/> class
        /// </summary>
        /// <param name="validateMainForm">The action to validate the main form</param>
        protected BudgetParameterConfigViewModelBase(Action validateMainForm)
        {
            this.ValidateMainForm = validateMainForm;
        }

        /// <summary>
        /// The action that validates the main form
        /// </summary>
        protected Action ValidateMainForm { get; }

        /// <summary>
        /// Gets the <see cref="BudgetKind"/> associated to this <see cref="BudgetParameterConfigViewModelBase"/>
        /// </summary>
        public abstract BudgetKind BudgetKind { get; }

        /// <summary>
        /// Asserts whether the view-model is valid
        /// </summary>
        /// <returns>True if it is</returns>
        public abstract bool IsFormValid();
    }
}
