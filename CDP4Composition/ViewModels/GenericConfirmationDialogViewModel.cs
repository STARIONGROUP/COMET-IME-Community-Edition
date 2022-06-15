// -------------------------------------------------------------------------------------------------
// <copyright file="GenericConfirmationDialogViewModel.cs" company="RHEA System S.A.">
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

namespace CDP4Composition.ViewModels
{
    using System;    
    using System.Reactive;

    using CDP4Composition.Navigation;
    using CDP4Composition.Mvvm;

    using ReactiveUI;
    
    /// <summary>
    /// The view model for a generic confirmation dialog
    /// </summary>
    public class GenericConfirmationDialogViewModel : DialogViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericConfirmationDialogViewModel"/> class
        /// </summary>
        public GenericConfirmationDialogViewModel(string title, string message)
        {
            this.Title = title;
            this.Message = message;
            this.YesCommand = ReactiveCommandCreator.Create(this.ExecuteYesCommand);
            this.NoCommand = ReactiveCommandCreator.Create(this.ExecuteNoCommand);
        }

        /// <summary>
        /// Gets the title for the dialog
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// Gets the message of the dialog
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to confirm
        /// </summary>
        public ReactiveCommand<Unit, Unit> YesCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to cancel
        /// </summary>
        public ReactiveCommand<Unit, Unit> NoCommand { get; private set; }

        /// <summary>
        /// Executes the <see cref="YesCommand"/>
        /// </summary>
        private void ExecuteYesCommand()
        {
            this.DialogResult = new BaseDialogResult(true);
        }

        /// <summary>
        /// Executes the <see cref="NoCommand"/>
        /// </summary>
        private void ExecuteNoCommand()
        {
            this.DialogResult = new BaseDialogResult(false);
        }
    }
}