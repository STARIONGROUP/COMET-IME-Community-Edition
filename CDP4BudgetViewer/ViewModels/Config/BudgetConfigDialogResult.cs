// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BudgetConfigDialogResult.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2018 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Budget.ViewModels
{
    using CDP4Composition.Navigation;
    using Config;

    /// <summary>
    /// the result of the budget-config dialog
    /// </summary>
    public class BudgetConfigDialogResult : IDialogResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BudgetConfigDialogResult"/> class
        /// </summary>
        /// <param name="result">The result of the dialog operation</param>
        /// <param name="config">The resulting <see cref="BudgetConfig"/></param>
        public BudgetConfigDialogResult(bool result, BudgetConfig config)
        {
            this.Result = result;
            this.BudgetConfig = config;
        }

        /// <summary>
        /// Gets the generated <see cref="BudgetConfig"/>
        /// </summary>
        public BudgetConfig BudgetConfig { get; }

        /// <summary>
        /// Gets the result of the dialog operation
        /// </summary>
        public bool? Result { get; }
    }
}
