// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CostBudgetSummaryViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Budget.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Dal;
    using Config;
    using Exceptions;
    using NLog;
    using ReactiveUI;
    using Services;

    /// <summary>
    /// The view-model that is responsible for showing the computed total cost-budget for a <see cref="ParameterType"/>
    /// </summary>
    public sealed class CostBudgetSummaryViewModel : BudgetSummaryViewModel
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        #region Backing fields
        /// <summary>
        /// Backing field for <see cref="Total"/>
        /// </summary>
        private float total;

        /// <summary>
        /// Backing field for <see cref="TotalWithSystemMargin"/>
        /// </summary>
        private float totalWithSystemMargin;

        /// <summary>
        /// Backing field for <see cref="Scale"/>
        /// </summary>
        private string scale;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="CostBudgetSummaryViewModel"/> class
        /// </summary>
        /// <param name="budgetConfig">The <see cref="BudgetConfig"/></param>
        /// <param name="rootElement">The root <see cref="ElementDefinition"/></param>
        /// <param name="option">The current <see cref="Option"/></param>
        /// <param name="session">The <see cref="ISession"/></param>
        public CostBudgetSummaryViewModel(BudgetConfig budgetConfig, ElementDefinition rootElement, Option option, ISession session) : base(budgetConfig, rootElement, option, session)
        {
            this.ComputeBudget();
            this.ComputeTotal();
        }

        /// <summary>
        /// Gets the total-dry mass of all sub-systems
        /// </summary>
        public float Total
        {
            get { return this.total; }
            private set { this.RaiseAndSetIfChanged(ref this.total, value); }
        }

        /// <summary>
        /// Gets the total wet-mass including a overall system margin of all sub-systems
        /// </summary>
        public float TotalWithSystemMargin
        {
            get { return this.totalWithSystemMargin; }
            private set { this.RaiseAndSetIfChanged(ref this.totalWithSystemMargin, value); }
        }

        /// <summary>
        /// Gets the scale used in this summary
        /// </summary>
        public string Scale
        {
            get { return this.scale; }
            private set { this.RaiseAndSetIfChanged(ref this.scale, value); }
        }

        /// <summary>
        /// Compute the system total
        /// </summary>
        protected override void ComputeTotal()
        {
            this.Total = 0;
            foreach (CostBudgetRowViewModel budgetRowViewModelBase in this.SubSystemSummary)
            {
                this.Total = this.Total + budgetRowViewModelBase.CostTotal;

                try
                {
                    checked
                    {
                        this.TotalWithSystemMargin = this.Total * (1f + this.SystemMargin / 100f);
                    }
                }
                catch (OverflowException e)
                {
                    Logger.Error("An overflow exception occurred in the sub-system computation. Value set to 0.");
                    this.TotalWithSystemMargin = 0;
                }
            }

            foreach (CostBudgetRowViewModel budgetRowViewModelBase in this.SubSystemSummary)
            {
                budgetRowViewModelBase.SetTotalCostRatio(this.Total);
            }
        }

        /// <summary>
        /// Generate the budget
        /// </summary>
        protected override void ComputeBudget()
        {
            try
            {
                var computer = new CostBudgetGenerator();
                var iteration = (Iteration)this.CurrentOption.Container;

                var results = computer.ComputeResult(this.BudgetConfig, this.RootElement, this.CurrentOption, this.Session.QuerySelectedDomainOfExpertise(iteration));

                foreach (SubSystemCostBudgetResult subSystemBudgetResult in results)
                {
                    var row = new CostBudgetRowViewModel(subSystemBudgetResult, subSystemBudgetResult.SubSystem, this.ComputeTotal);
                    this.SubSystemSummary.Add(row);
                }

                this.Scale = results.Select(x => x.Scale).FirstOrDefault()?.Name ?? "-";
            }
            catch (BudgetComputationException e)
            {
                Logger.Error("Budget Computation Error: {0}", e.Message);
            }
        }
    }
}
