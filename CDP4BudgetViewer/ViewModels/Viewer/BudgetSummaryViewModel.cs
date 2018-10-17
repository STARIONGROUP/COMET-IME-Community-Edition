// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BudgetSummaryViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Budget.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Linq;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Dal;
    using CDP4Dal.Events;
    using Config;
    using Exceptions;
    using ReactiveUI;
    using Services;

    /// <summary>
    /// The view-model that is responsible for showing the computed total budget for a <see cref="ParameterType"/>
    /// </summary>
    public abstract class BudgetSummaryViewModel : ReactiveObject, IDisposable
    {
        /// <summary>
        /// Backing field for <see cref="SystemMargin"/>
        /// </summary>
        private float systemMargin;

        /// <summary>
        /// Backing field for <see cref="ElementName"/>
        /// </summary>
        private string elementName;

        /// <summary>
        /// Backing field for <see cref="SummaryTotal"/>
        /// </summary>
        private float summaryTotal;

        /// <summary>
        /// Initializes a new instance of the <see cref="BudgetSummaryViewModel"/> class
        /// </summary>
        protected BudgetSummaryViewModel(BudgetConfig budgetConfig, ElementDefinition element, Option option, ISession session)
        {
            this.BudgetConfig = budgetConfig;
            this.Disposable = new List<IDisposable>();
            this.SubSystemSummary = new ReactiveList<IBudgetRowViewModelBase>();
            this.CurrentOption = option;
            this.Session = session;
            this.RootElement = element;
            this.WhenAnyValue(x => x.SystemMargin).Skip(1).Subscribe(_ => this.ComputeTotal());
            this.ElementName = this.RootElement.Name;
        }

        /// <summary>
        /// Gets or sets the overall system margin (%)
        /// </summary>
        public float SystemMargin
        {
            get { return this.systemMargin; }
            set { this.RaiseAndSetIfChanged(ref this.systemMargin, value); }
        }

        /// <summary>
        /// Gets the element-name
        /// </summary>
        public string ElementName
        {
            get { return this.elementName; }
            private set { this.RaiseAndSetIfChanged(ref this.elementName, value); }
        }

        /// <summary>
        /// Gets the summary total
        /// </summary>
        public float SummaryTotal
        {
            get { return this.summaryTotal; }
            protected set { this.RaiseAndSetIfChanged(ref this.summaryTotal, value); }
        }

        /// <summary>
        /// Gets the <see cref="IDisposable"/>
        /// </summary>
        protected List<IDisposable> Disposable { get; private set; }

        /// <summary>
        /// Gets the current <see cref="ISession"/>
        /// </summary>
        protected ISession Session { get; }

        /// <summary>
        /// Gets the current option
        /// </summary>
        protected Option CurrentOption { get; }

        /// <summary>
        /// Gets the sub-system summary
        /// </summary>
        public ReactiveList<IBudgetRowViewModelBase> SubSystemSummary { get; }

        /// <summary>
        /// The <see cref="Config.BudgetConfig"/>
        /// </summary>
        protected BudgetConfig BudgetConfig { get; }

        /// <summary>
        /// Gets the root element for this computation
        /// </summary>
        public ElementDefinition RootElement { get; }

        /// <summary>
        /// Compute the totals
        /// </summary>
        protected abstract void ComputeTotal();

        /// <summary>
        /// Compute the sub-system budgets
        /// </summary>
        protected abstract void ComputeBudget();

        /// <summary>
        /// Disposes of the <see cref="IDisposable"/>
        /// </summary>
        public void Dispose()
        {
            foreach (var disposable in this.Disposable)
            {
                disposable?.Dispose();
            }
        }
    }
}
