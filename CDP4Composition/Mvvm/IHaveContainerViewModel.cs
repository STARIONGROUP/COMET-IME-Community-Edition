// ------------------------------------------------------------------------------------------------
// <copyright file="IHaveContainerViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2019 Starion Group S.A.
// </copyright>
// -----------------------------------------------------------------------------------------------

namespace CDP4Composition.Mvvm
{
    using CDP4Common.CommonData;

    /// <summary>
    /// Specific interface for ViewModels that have a ContainerViewModel property
    /// </summary>
    public interface IHaveContainerViewModel
    {
        /// <summary>
        /// Gets the container <see cref="IViewModelBase{T}"/>
        /// </summary>
        IViewModelBase<Thing> ContainerViewModel { get; }
    }
}
