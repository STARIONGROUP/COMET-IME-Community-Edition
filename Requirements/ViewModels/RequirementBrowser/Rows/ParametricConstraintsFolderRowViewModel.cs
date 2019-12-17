// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParametricConstraintsFolderRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace CDP4Requirements.ViewModels.RequirementBrowser.Rows
{
    using CDP4Common.CommonData;

    using CDP4Composition;
    using CDP4Composition.Mvvm;

    using CDP4Dal;

    using ReactiveUI;

    /// <summary>
    /// Specific FolderRowViewModel for showing the ParametricConstraint's top folder in the RequirementBrowser 
    /// </summary>
    public class ParametricConstraintsFolderRowViewModel : FolderRowViewModel, IHaveWritableRequirementStateOfCompliance
    {
        /// <summary>
        /// Backing field for <see cref="ParametricConstraintsFolderRowViewModel.RequirementStateOfCompliance"/> 
        /// </summary>
        private RequirementStateOfCompliance requirementStateOfCompliance;

        /// <summary>
        /// Initializes a new instance of the <see cref="FolderRowViewModel"/> class
        /// </summary>
        /// <param name="shortname">The short-name for this folder</param>
        /// <param name="name">The Name of the folder</param>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <param name="containerViewModel">The view-model that contains this row</param>
        public ParametricConstraintsFolderRowViewModel(string shortname, string name, ISession session, IViewModelBase<Thing> containerViewModel) : base(shortname, name, session, containerViewModel)
        {
        }

        /// <summary>
        /// Gets or sets the RequirementStateOfCompliance
        /// </summary>
        public RequirementStateOfCompliance RequirementStateOfCompliance
        {
            get => this.requirementStateOfCompliance;
            set => this.RaiseAndSetIfChanged(ref this.requirementStateOfCompliance, value);
        }
    }
}
