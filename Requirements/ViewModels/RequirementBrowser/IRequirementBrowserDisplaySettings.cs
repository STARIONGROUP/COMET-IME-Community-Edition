// ------------------------------------------------------------------------------------------------
// <copyright file="IRequirementBrowserDisplaySettings.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2019 Starion Group S.A.
// </copyright>
// -----------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels.RequirementBrowser
{
    /// <summary>
    /// Interface that is used for hiding/showing certain rows in the RequirementBrowser
    /// </summary>
    public interface IRequirementBrowserDisplaySettings
    {
        /// <summary>
        /// Gets or sets a value whether SimpleParameterValues things are displayed
        /// </summary>
        bool IsSimpleParameterValuesDisplayed { get; set; }

        /// <summary>
        /// Gets or sets a value whether Parametric Constraints are displayed
        /// </summary>
        bool IsParametricConstraintDisplayed { get; set; }
    }
}
