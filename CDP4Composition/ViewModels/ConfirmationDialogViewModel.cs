// -------------------------------------------------------------------------------------------------
// <copyright file="ConfirmationDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.ViewModels
{
    using System;
    using System.Windows.Input;
    using CDP4Common.CommonData;
    using CDP4Composition;
    using CDP4Composition.Converters;
    using CDP4Composition.Navigation;
    using ReactiveUI;

    /// <summary>
    /// The confirmation dialog view model to delete a <see cref="Thing"/>
    /// </summary>
    public class ConfirmationDialogViewModel : DialogViewModelBase
    {
        /// <summary>
        /// Backing field for <see cref="DeletedThingText"/>
        /// </summary>
        private string deletedThingText;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfirmationDialogViewModel"/> class
        /// </summary>
        /// <param name="thingToDelete">The <see cref="Thing"/> to delete</param>
        public ConfirmationDialogViewModel(Thing thingToDelete)
        {
            this.YesCommand = ReactiveCommand.Create();
            this.YesCommand.Subscribe(_ => this.ExecuteYesCommand());

            this.NoCommand = ReactiveCommand.Create();
            this.NoCommand.Subscribe(_ => this.ExecuteNoCommand());

            var converter = new CamelCaseToSpaceConverter();
            this.DeletedThingText = (string)converter.Convert(thingToDelete.ClassKind, null, null, null);
        }

        /// <summary>
        /// Gets or sets the text of the deleted thing
        /// </summary>
        public string DeletedThingText
        {
            get { return this.deletedThingText; }
            set { this.RaiseAndSetIfChanged(ref this.deletedThingText, value); }
        }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to confirm
        /// </summary>
        public ReactiveCommand<object> YesCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to cancel
        /// </summary>
        public ReactiveCommand<object> NoCommand { get; private set; }

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