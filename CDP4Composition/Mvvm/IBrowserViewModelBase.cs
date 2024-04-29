// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IBrowserViewModelBase.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Mvvm
{
    using CDP4Common.CommonData;
    using CDP4Composition.DragDrop;
    using CDP4Composition.Navigation;
    using ReactiveUI;

    /// <summary>
    /// The interface for the browser-view-model
    /// </summary>
    /// <typeparam name="T">The <see cref="Thing"/> represented by the row</typeparam>
    public interface IBrowserViewModelBase<out T> : IDragSource, IViewModelBase<T> where T : Thing
    {
        /// <summary>
        /// Gets the <see cref="IDialogNavigationService"/> used to navigate to a <see cref="IDialogViewModel"/>
        /// </summary>
        IDialogNavigationService DialogNavigationService { get; }

        /// <summary>
        /// Populate the context menu.
        /// </summary>
        void PopulateContextMenu();

        /// <summary>
        /// Compute the permissions for the current user
        /// </summary>
        void ComputePermission();

        /// <summary>
        /// Gets the context menu view-model
        /// </summary>
        ReactiveList<ContextMenuItemViewModel> ContextMenu { get; }
    }
}