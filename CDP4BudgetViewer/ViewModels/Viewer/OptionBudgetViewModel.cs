// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OptionBudgetViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Budget.ViewModels
{
    using System;
    using CDP4Common.EngineeringModelData;
    using CDP4Dal;
    using Config;
    using Exceptions;
    using ReactiveUI;
    using Services;

    /// <summary>
    /// The view-model for the option dependent-budget
    /// </summary>
    public class OptionBudgetViewModel : ReactiveObject
    {
        public const string MASS_BUDGET_TITLE = "Mass Budget";
        public const string GENERIC_BUDGET_TITLE = "Generic Budget";
        public const string POWER_BUDGET_TITLE = "Power Budget";

        /// <summary>
        /// Backing field for <see cref="GroupTitle"/>
        /// </summary>
        private string groupTitle;

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionBudgetViewModel"/> class
        /// </summary>
        /// <param name="option">The option</param>
        /// <param name="budgetConfig">The current <see cref="BudgetConfig"/></param>
        /// <param name="session">The current <see cref="ISession"/></param>
        public OptionBudgetViewModel(Option option, BudgetConfig budgetConfig, ISession session, Action refreshOptionOverview)
        {
            this.BudgetSummary = new ReactiveList<BudgetSummaryViewModel>();
            this.Option = option;
            foreach (var budgetConfigElement in budgetConfig.Elements)
            {
                if (budgetConfig.BudgetParameterConfig is MassBudgetParameterConfig)
                {
                    this.GroupTitle = $"{MASS_BUDGET_TITLE}: {option.Name}";
                    this.BudgetSummary.Add(new MassBudgetSummaryViewModel(budgetConfig, budgetConfigElement, option, session, refreshOptionOverview));
                }
                else if (budgetConfig.BudgetParameterConfig is GenericBudgetParameterConfig)
                {
                    this.GroupTitle = $"{GENERIC_BUDGET_TITLE}: {option.Name}";
                    this.BudgetSummary.Add(new GenericBudgetSummaryViewModel(budgetConfig, budgetConfigElement, option, session, refreshOptionOverview));
                }
                else if (budgetConfig.BudgetParameterConfig is PowerBudgetParameterConfig)
                {
                    this.GroupTitle = $"{POWER_BUDGET_TITLE}: {option.Name}";
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="Option"/>
        /// </summary>
        public Option Option { get; }

        /// <summary>
        /// Gets the option name for this view-model
        /// </summary>
        public string GroupTitle
        {
            get { return this.groupTitle; }
            private set { this.RaiseAndSetIfChanged(ref this.groupTitle, value); }
        }

        /// <summary>
        /// Gets the view-models representing total calculated values
        /// </summary>
        public ReactiveList<BudgetSummaryViewModel> BudgetSummary { get; private set; }
    }
}
