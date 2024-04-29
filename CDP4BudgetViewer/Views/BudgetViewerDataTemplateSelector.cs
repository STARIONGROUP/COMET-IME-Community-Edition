// -------------------------------------------------------------------------------------------------
// <copyright file="BudgetViewerDataTemplateSelector.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2018 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Budget.ViewModels
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using Services;

    /// <summary>
    /// The <see cref="DataTemplateSelector"/> to select a <see cref="DataTemplate"/> depending on the kind of budget to compute
    /// </summary>
    public class BudgetViewerDataTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// Selects the template for a <see cref="MassBudgetSummaryViewModel"/>
        /// </summary>
        /// <param name="item">The <see cref="MassBudgetSummaryViewModel"/></param>
        /// <param name="container">the dependency-object</param>
        /// <returns>The <see cref="DataTemplate"/> to use</returns>
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var vm = item as MassBudgetSummaryViewModel;
            if (vm != null)
            {
                return this.MassBudgetDataTemplate;
            }

            var costVm = item as GenericBudgetSummaryViewModel;
            if (costVm != null)
            {
                return this.CostBudgetDataTemplate;
            }

            return base.SelectTemplate(item, container);
        }

        /// <summary>
        /// Gets or sets the <see cref="DataTemplate"/> for the mass-budget
        /// </summary>
        public DataTemplate MassBudgetDataTemplate { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DataTemplate"/> for the power-budget
        /// </summary>
        public DataTemplate PowerBudgetDataTemplate { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DataTemplate"/> for the cost-budget
        /// </summary>
        public DataTemplate CostBudgetDataTemplate { get; set; }
    }
}
