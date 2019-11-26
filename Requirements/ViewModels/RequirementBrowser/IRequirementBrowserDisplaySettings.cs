// ------------------------------------------------------------------------------------------------
// <copyright file="IRequirementBrowserDisplaySettings.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// -----------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels.RequirementBrowser
{
    /// <summary>
    /// Interface that is used for hiding/showing certain rows in the RequirementBrowser
    /// </summary>
    public interface IRequirementBrowserDisplaySettings
    {
        bool IsSimpleParameterTypeDisplayed { get; set; }
        bool IsParametricConstraintDisplayed { get; set; }
    }
}
