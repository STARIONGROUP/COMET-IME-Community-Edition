// -------------------------------------------------------------------------------------------------
// <copyright file="OkDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4CommonView.ViewModels
{
    using System;
    using System.Windows.Input;
    using CDP4Composition.Navigation;
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
            this.OkCommand = ReactiveCommand.Create();
            this.OkCommand.Subscribe(_ => this.ExecuteOkCommand());
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
        public ReactiveCommand<Object> OkCommand { get; private set; }

        /// <summary>
        /// Executes the <see cref="OkCommand"/>
        /// </summary>
        private void ExecuteOkCommand()
        {
            this.DialogResult = new BaseDialogResult(true);
        }
    }
}
