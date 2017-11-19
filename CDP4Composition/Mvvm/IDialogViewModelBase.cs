// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDialogViewModelBase.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Mvvm
{
    using System.ComponentModel;
    using System.Reactive;
    using CDP4Common.CommonData;
    using CDP4Composition.Services;
    using ReactiveUI;

    /// <summary>
    /// The interface for the dialog-view-model
    /// </summary>
    /// <typeparam name="T">The <see cref="Thing"/> represented by the row</typeparam>
    public interface IDialogViewModelBase<out T> : IViewModelBase<T>, IDataErrorInfo where T : Thing
    {
        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> that represents an "confirmation" of the dialog
        /// </summary>
        ReactiveCommand<Unit> OkCommand { get; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> that represents an "cancellation" of the dialog
        /// </summary>
        ReactiveCommand<object> CancelCommand { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="OkCommand"/> can be executed
        /// </summary>
        bool OkCanExecute { get; set; }

        /// <summary>
        /// Gets the container
        /// </summary>
        Thing Container { get; }

        /// <summary>
        /// Gets or sets the list of <see cref="ValidationService.ValidationRule"/>s that are violated.
        /// </summary>
        ReactiveList<ValidationService.ValidationRule> ValidationErrors { get; set; }
    }
}