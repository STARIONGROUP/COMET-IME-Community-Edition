// -------------------------------------------------------------------------------------------------
// <copyright file="BudgetParameterConfigDataTemplateSelector.cs" company="Starion Group S.A.">
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
    public class BudgetParameterConfigDataTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// Selects the template for a <see cref="BudgetParameterConfigViewModelBase"/>
        /// </summary>
        /// <param name="item">The <see cref="BudgetParameterConfigViewModelBase"/></param>
        /// <param name="container">the dependency-object</param>
        /// <returns>The <see cref="DataTemplate"/> to use</returns>
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var vm = item as BudgetParameterConfigViewModelBase;
            if (vm == null)
            {
                return base.SelectTemplate(item, container);
            }

            switch (vm.BudgetKind)
            {
                case BudgetKind.Mass:
                    return this.MassBudgetDataTemplate;
                case BudgetKind.Generic:
                default:
                    return this.GenericBudgetDataTemplate;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="DataTemplate"/> for a mass-budget template
        /// </summary>
        public DataTemplate MassBudgetDataTemplate { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DataTemplate"/> for a generic-budget
        /// </summary>
        public DataTemplate GenericBudgetDataTemplate { get; set; }
    }
}
