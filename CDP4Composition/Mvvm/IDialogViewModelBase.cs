// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDialogViewModelBase.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
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
        /// Gets the <see cref="ReactiveCommand{Unit, Unit}"/> that represents an "confirmation" of the dialog
        /// </summary>
        ReactiveCommand<Unit, Unit> OkCommand { get; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand{Unit, Unit}"/> that represents an "cancellation" of the dialog
        /// </summary>
        ReactiveCommand<Unit, Unit> CancelCommand { get; }

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