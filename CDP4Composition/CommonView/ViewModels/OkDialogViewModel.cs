// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OkDialogViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2022 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4CommonView.ViewModels
{
    using System.Reactive;
    using System.Windows.Input;

    using CDP4Composition.Navigation;
    using CDP4Composition.Mvvm;

    using ReactiveUI;

    /// <summary>
    /// The view-model of a dialog giving information to the user
    /// </summary>
    public class OkDialogViewModel : DialogViewModelBase
    {
        /// <summary>
        /// Initializes the new instance of the <see cref="OkDialogViewModel"/> class
        /// </summary>
        /// <param name="title">Title for the dialog</param>
        /// <param name="message">The message to be displayed in the dialog</param>
        public OkDialogViewModel(string title, string message)
        {
            this.Title = title;
            this.Message = message;
            this.OkCommand = ReactiveCommandCreator.Create(this.ExecuteOkCommand);
        }

        /// <summary>
        /// Gets the title for the dialog
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// Gets the message for the dialog
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to confirm 
        /// </summary>
        public ReactiveCommand<Unit, Unit> OkCommand { get; private set; }

        /// <summary>
        /// Executes the <see cref="OkCommand"/>
        /// </summary>
        private void ExecuteOkCommand()
        {
            this.DialogResult = new BaseDialogResult(true);
        }
    }
}
