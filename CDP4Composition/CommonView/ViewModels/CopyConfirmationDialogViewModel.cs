// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CopyConfirmationDialogViewModel.cs" company="Starion Group S.A.">
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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows.Input;
    
    using CDP4Common.CommonData;
    
    using CDP4Composition.Navigation;
    
    using ReactiveUI;

    using System.Reactive;

    using CDP4Composition.Mvvm;

    /// <summary>
    /// The copy confirmation dialog view model to copy a <see cref="Thing"/>
    /// </summary>
    public class CopyConfirmationDialogViewModel : DialogViewModelBase
    {
        /// <summary>
        /// Backing field for <see cref="IsDetailVisible"/>
        /// </summary>
        private bool isDetailVisible;

        /// <summary>
        /// Backing field for <see cref="CanProceed"/> property
        /// </summary>
        private bool canProceed;

        /// <summary>
        /// Backing field for <see cref="CopyPermissionMessage"/>
        /// </summary>
        private string copyPermissionMessage;

        /// <summary>
        /// Backing field for <see cref="CopyPermissionDetails"/>
        /// </summary>
        private string copyPermissionDetails;

        /// <summary>
        /// Backing field for <see cref="Title"/>
        /// </summary>
        private string title;

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyConfirmationDialogViewModel"/> class
        /// </summary>
        /// <param name="copyableThing">
        /// The <see cref="IEnumerable{Thing}"/> containing the <see cref="Thing"/>s that can be copied
        /// </param>
        /// <param name="errors">
        /// The <see cref="IReadOnlyDictionary{Thing, String}"/> containing the errors associated with the <see cref="Thing"/>s that cannot be copied
        /// </param>
        public CopyConfirmationDialogViewModel(IEnumerable<Thing> copyableThing, IReadOnlyDictionary<Thing, string> errors)
        {
            this.Title = "Copy Confirmation";

            this.ProceedCommand = ReactiveCommandCreator.Create(this.ExecuteProceedCommand, this.WhenAnyValue(x => x.CanProceed));
            this.CancelCommand = ReactiveCommandCreator.Create(this.ExecuteCancelCommand);

            var copyablethings = copyableThing.ToList();
            this.CopyPermissionMessage = copyablethings.Any() ? "A partial copy will be performed." : "The copy operation cannot be performed.";
            this.CanProceed = copyablethings.Any();

            var copyPermissionDetailsStringBuilder = new StringBuilder();

            foreach (var thing in copyablethings)
            {
                copyPermissionDetailsStringBuilder.Append($"The {thing.ClassKind} with id {thing.Iid} will be copied.\n");
            }

            foreach (var error in errors.Values)
            {
                copyPermissionDetailsStringBuilder.Append($"{error}\n");
            }

            this.CopyPermissionDetails = copyPermissionDetailsStringBuilder.ToString();
        }

        /// <summary>
        /// Gets the title for the associated dialog
        /// </summary>
        public string Title
        {
            get { return this.title; }
            private set { this.RaiseAndSetIfChanged(ref this.title, value); }
        }

        /// <summary>
        /// Gets the copy permission result message
        /// </summary>
        public string CopyPermissionMessage
        {
            get { return this.copyPermissionMessage; }
            private set { this.RaiseAndSetIfChanged(ref this.copyPermissionMessage, value); }
        }

        /// <summary>
        /// Gets the copy permission details
        /// </summary>
        public string CopyPermissionDetails
        {
            get { return this.copyPermissionDetails; }
            private set { this.RaiseAndSetIfChanged(ref this.copyPermissionDetails, value); }
        }

        /// <summary>
        /// Gets or sets a value
        /// </summary>
        public bool IsDetailVisible
        {
            get { return this.isDetailVisible; }
            set { this.RaiseAndSetIfChanged(ref this.isDetailVisible, value); }
        }

        /// <summary>
        /// Gets a value indicating whether it is possible to proceed with the copy operation
        /// </summary>
        public bool CanProceed
        {
            get { return this.canProceed; }
            private set { this.RaiseAndSetIfChanged(ref this.canProceed, value); }
        }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to proceed
        /// </summary>
        public ReactiveCommand<Unit, Unit> ProceedCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to cancel
        /// </summary>
        public ReactiveCommand<Unit, Unit> CancelCommand { get; private set; }

        /// <summary>
        /// Executes the <see cref="ProceedCommand"/>
        /// </summary>
        private void ExecuteProceedCommand()
        {
            this.DialogResult = new BaseDialogResult(true);
        }

        /// <summary>
        /// Executes the <see cref="CancelCommand"/>
        /// </summary>
        private void ExecuteCancelCommand()
        {
            this.DialogResult = new BaseDialogResult(false);
        }
    }
}