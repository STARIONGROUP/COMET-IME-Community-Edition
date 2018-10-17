// -------------------------------------------------------------------------------------------------
// <copyright file="IBudgetRowViewModelBase.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Budget.ViewModels
{
    using Services;

    /// <summary>
    /// An interface for the budget-row
    /// </summary>
    public interface IBudgetRowViewModelBase
    {
        /// <summary>
        /// Gets or sets the system level to use for the budget calculation
        /// </summary>
         SystemLevelKind SelectedSystemLevel { get; set; }

        /// <summary>
        /// Gets the subsystem name
        /// </summary>
        string SubSystemName { get; }
    }
}
