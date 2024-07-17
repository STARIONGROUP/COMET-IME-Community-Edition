// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MassBudgetSummaryViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2018 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Budget.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm;

    using CDP4Dal;
    using Config;
    using Exceptions;
    using NLog;
    using ReactiveUI;
    using Services;

    /// <summary>
    /// The view-model that is responsible for showing the computed total budget for a <see cref="ParameterType"/>
    /// </summary>
    public sealed class MassBudgetSummaryViewModel : BudgetSummaryViewModel
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        #region Backing fields
        /// <summary>
        /// Backing field for <see cref="DryTotal"/>
        /// </summary>
        private float dryTotal;

        /// <summary>
        /// Backing field for <see cref="DryTotalWithSystemMargin"/>
        /// </summary>
        private float dryTotalWithSystemMargin;

        /// <summary>
        /// Backing field for <see cref="WetTotal"/>
        /// </summary>
        private float wetTotal;

        /// <summary>
        /// Backing field for <see cref="Scale"/>
        /// </summary>
        private string scale;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="MassBudgetSummaryViewModel"/> class
        /// </summary>
        /// <param name="budgetConfig">The <see cref="BudgetConfig"/></param>
        /// <param name="rootElement">The root <see cref="ElementDefinition"/></param>
        /// <param name="option">The current <see cref="Option"/></param>
        /// <param name="session">The <see cref="ISession"/></param>
        public MassBudgetSummaryViewModel(BudgetConfig budgetConfig, ElementDefinition rootElement, Option option, ISession session, Action refreshOptionOverview) : base(budgetConfig, rootElement, option, session, refreshOptionOverview)
        {
            this.WhenAnyValue(x => x.WetTotal).Subscribe(x => this.SummaryTotal = x);
            this.ExtraMassContributions = new ReactiveList<ExtraMassContributionRowViewModel>();
            this.ComputeBudget();
            this.ComputeTotal();
        }

        /// <summary>
        /// Gets the total-dry mass of all sub-systems
        /// </summary>
        public float DryTotal
        {
            get { return this.dryTotal; }
            private set { this.RaiseAndSetIfChanged(ref this.dryTotal, value); }
        }

        /// <summary>
        /// Gets the total wet-mass of all sub-systems
        /// </summary>
        public float WetTotal
        {
            get { return this.wetTotal; }
            private set { this.RaiseAndSetIfChanged(ref this.wetTotal, value); }
        }

        /// <summary>
        /// Gets the total wet-mass including a overall system margin of all sub-systems
        /// </summary>
        public float DryTotalWithSystemMargin
        {
            get { return this.dryTotalWithSystemMargin; }
            private set { this.RaiseAndSetIfChanged(ref this.dryTotalWithSystemMargin, value); }
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
        /// Gets the extra-mass contributions
        /// </summary>
        public ReactiveList<ExtraMassContributionRowViewModel> ExtraMassContributions { get; private set; }

        /// <summary>
        /// Compute the system total
        /// </summary>
        protected override void ComputeTotal()
        {
            this.DryTotal = 0;
            foreach (MassBudgetRowViewModel budgetRowViewModelBase in this.SubSystemSummary)
            {
                this.DryTotal = this.DryTotal + budgetRowViewModelBase.DryMassTotal;

                try
                {
                    checked
                    {
                        this.DryTotalWithSystemMargin = this.DryTotal * (1f + this.SystemMargin / 100f);
                    }
                }
                catch (OverflowException)
                {
                    Logger.Error("An overflow exception occurred in the sub-system computation. Value set to 0.");
                    this.DryTotalWithSystemMargin = 0;
                }
            }

            foreach (MassBudgetRowViewModel budgetRowViewModelBase in this.SubSystemSummary)
            {
                budgetRowViewModelBase.SetTotalDryMassRatio(this.DryTotal);
            }

            try
            {
                checked
                {
                    this.WetTotal = this.DryTotalWithSystemMargin + this.ExtraMassContributions.Select(x => x.ContributionTotalWithMargin).Sum();
                }
            }
            catch (OverflowException)
            {
                Logger.Error("An overflow exception occurred in the sub-system computation. Value set to 0.");
                this.DryTotalWithSystemMargin = 0;
            }
        }

        /// <summary>
        /// Generate the budget
        /// </summary>
        protected override void ComputeBudget()
        {
            try
            {
                var computer = new MassBudgetGenerator();
                var iteration = (Iteration)this.CurrentOption.Container;

                var results = computer.ComputeResult(this.BudgetConfig, this.RootElement, this.CurrentOption, this.Session.QuerySelectedDomainOfExpertise(iteration));

                foreach (SubSystemMassBudgetResult subSystemBudgetResult in results)
                {
                    var row = new MassBudgetRowViewModel(subSystemBudgetResult, subSystemBudgetResult.SubSystem, this.ComputeTotal);
                    this.SubSystemSummary.Add(row);
                }

                var extraMassContributions = computer.GetExtraMassContributions(this.BudgetConfig, this.RootElement, this.CurrentOption, this.Session.QuerySelectedDomainOfExpertise(iteration));
                foreach (var extraMassContribution in extraMassContributions)
                {
                    this.ExtraMassContributions.Add(new ExtraMassContributionRowViewModel(extraMassContribution));
                }

                this.Scale = results.Select(x => x.Scale).FirstOrDefault()?.Name ?? extraMassContributions.FirstOrDefault().Scale?.Name ?? "-";
            }
            catch (BudgetComputationException e)
            {
                Logger.Error("Budget Computation Error: {0}", e.Message);
            }
        }
    }
}
