// ------------------------------------------------------------------------------------------------
// <copyright file="IFloatingDialogViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// -----------------------------------------------------------------------------------------------

namespace CDP4Composition.Navigation.Interfaces
{
    using CDP4Common.CommonData;
    using CDP4Composition.Navigation;

    /// <summary>
    /// The ViewModel interface associated to floating <see cref="IDialogView"/>
    /// </summary>
    public interface IFloatingDialogViewModel<out T> : IDialogViewModel where T : Thing
    {
        /// <summary>
        /// Gets the associated <see cref="T"/>
        /// </summary>
        T Thing { get; }
    }
}