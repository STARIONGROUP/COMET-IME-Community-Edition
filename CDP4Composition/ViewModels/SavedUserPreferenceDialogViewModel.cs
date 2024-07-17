// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SavedConfigurationDialogViewModel.cs" company="Starion Group S.A.">
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

namespace CDP4Composition.ViewModels
{
    using System;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Services.FilterEditorService;
    using CDP4Composition.ViewModels.DialogResult;

    using ReactiveUI;

    /// <summary>
    /// ViewModel for the dialog to save <see cref="UserPreference"/>s
    /// </summary>
    public class SavedUserPreferenceDialogViewModel : DialogViewModelBase
    {
        /// <summary>
        /// The <see cref="ISavedUserPreference"/>
        /// </summary>
        private readonly ISavedUserPreference savedUserPreference;

        /// <summary>
        /// The <see cref="Action"/> to perform when data needs to be stored
        /// </summary>
        private readonly Action saveAction;

        /// <summary>
        /// Backing field for the <see cref="Name"/> property.
        /// </summary>
        private string name;

        /// <summary>
        /// Backing field for the <see cref="Description"/> property.
        /// </summary>
        private string description;

        /// <summary>
        /// Gets or sets the name of the <see cref="ISavedUserPreference"/>.
        /// </summary>
        public string Name
        {
            get => this.name;
            set => this.RaiseAndSetIfChanged(ref this.name, value);
        }

        /// <summary>
        /// Gets or sets the description of the <see cref="ISavedUserPreference"/>.
        /// </summary>
        public string Description
        {
            get => this.description;
            set => this.RaiseAndSetIfChanged(ref this.description, value);
        }

        /// <summary>
        /// Gets the Ok Command
        /// </summary>
        public ReactiveCommand<Unit, Unit> OkCommand { get; private set; }

        /// <summary>
        /// Gets the Cancel Command
        /// </summary>
        public ReactiveCommand<Unit, Unit> CancelCommand { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SavedUserPreferenceDialogViewModel"/> class.
        /// </summary>
        /// <param name="savedUserPreference">The <see cref="ISavedUserPreference"/> that should be saved</param>
        /// <param name="saveAction">The <see cref="Action"/> to be performed on saving</param>
        public SavedUserPreferenceDialogViewModel(ISavedUserPreference savedUserPreference, Action saveAction)
        {
            this.savedUserPreference = savedUserPreference;
            this.saveAction = saveAction;

            // reset the loading indicator
            this.IsBusy = false;

            var canOk = this.WhenAnyValue(
                vm => vm.Name,
                vm => vm.Description,
                (n, d) =>
                    !string.IsNullOrEmpty(n) && !string.IsNullOrEmpty(d));

            this.OkCommand = ReactiveCommandCreator.CreateAsyncTask(this.ExecuteOk, canOk, RxApp.MainThreadScheduler);

            this.OkCommand.ThrownExceptions.Select(ex => ex).Subscribe(x => { this.ErrorMessage = x.Message; });

            this.CancelCommand = ReactiveCommandCreator.Create(this.ExecuteCancel);

            this.Name = savedUserPreference.Name;
            this.Description = savedUserPreference.Description;
        }

        /// <summary>
        /// Executes the Ok Command and by executing the <see cref="Action"/> stored in <see cref="saveAction"/>
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task ExecuteOk()
        {
            this.savedUserPreference.Name = this.Name;
            this.savedUserPreference.Description = this.Description;

            this.IsBusy = true;

            try
            {
                this.LoadingMessage = "Saving...";

                await Task.Run(this.saveAction);

                this.DialogResult = new SavedConfigurationResult(true);
            }
            catch (Exception ex)
            {
                this.ErrorMessage = ex.Message;
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        /// <summary>
        /// Executes the Cancel Command
        /// </summary>
        private void ExecuteCancel()
        {
            this.DialogResult = new SavedConfigurationResult(false);
        }
    }
}
