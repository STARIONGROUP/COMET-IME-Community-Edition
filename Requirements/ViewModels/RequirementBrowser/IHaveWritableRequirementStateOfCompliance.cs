// ------------------------------------------------------------------------------------------------
// <copyright file="IHaveWritableRequirementStateOfCompliance.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2019 Starion Group S.A.
// </copyright>
// -----------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels.RequirementBrowser
{
    using CDP4RequirementsVerification;

    /// <summary>
    /// Specification of the <see cref="IHaveWritableRequirementStateOfCompliance"/> interface.
    /// </summary>
    public interface IHaveWritableRequirementStateOfCompliance
    {
        /// <summary>
        /// The current <see cref="CDP4RequirementsVerification.RequirementStateOfCompliance"/>>
        /// </summary>
        RequirementStateOfCompliance RequirementStateOfCompliance { get; set; }
    }
}
