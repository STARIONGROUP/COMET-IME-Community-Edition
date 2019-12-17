// ------------------------------------------------------------------------------------------------
// <copyright file="ContainerViewModelMethods.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// -----------------------------------------------------------------------------------------------

namespace CDP4Requirements.ExtensionMethods
{
    using CDP4Composition.Mvvm;

    using CDP4Requirements.ViewModels.RequirementBrowser;

    /// <summary>
    /// This class contains methods for specific <see cref="IHaveContainerViewModel"/> related functionality 
    /// </summary>
    public static class ContainerViewModelMethods
    {
        /// <summary>
        /// Sets al the <see cref="RequirementStateOfCompliance"/>s for this and all parent viewmodels
        /// </summary>
        /// <param name="containerViewModel">The <see cref="IHaveContainerViewModel"/> that is the starting point for looking up <see cref="IHaveWritableRequirementStateOfCompliance"/>s up the visual tree</param>
        public static void SetNestedParentRequirementStateOfCompliances(this IHaveContainerViewModel containerViewModel)
        {
            while (containerViewModel is IHaveWritableRequirementStateOfCompliance requirementStateOfComplianceViewModel)
            {
                requirementStateOfComplianceViewModel.RequirementStateOfCompliance = RequirementStateOfCompliance.Unknown;

                if (containerViewModel.ContainerViewModel is IHaveContainerViewModel nextContainerViewModel)
                {
                    containerViewModel = nextContainerViewModel;
                }
                else
                {
                    break;
                }
            }
        }
    }
}
