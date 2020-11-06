// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ManageUserPreferencesDialogViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Kamil Wojnowski
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    using CDP4Composition.Navigation;
    using CDP4Composition.Services.FilterEditorService;
    using CDP4Composition.ViewModels.DialogResult;

    using ReactiveUI;

    /// <summary>
    /// ViewModel for the dialog to manage a list of <see cref="ISavedUserPreference"/>s
    /// </summary>
    public class ManageSavedUserPreferencesDialogViewModel<T> : DialogViewModelBase where T : ISavedUserPreference
    {
        private readonly Action<IEnumerable<T>> saveAction;

        /// <summary>
        /// Backing field for <see cref="SelectedSavedUserPreference"/>
        /// </summary>
        private T selectedSavedUserPreference;
  
        /// <summary>
        /// Gets or sets the list of <see cref="ISavedUserPreference"/>s.
        /// </summary>
        public ReactiveList<T> SavedUserPreferences { get; private set; }

        /// <summary>
        /// Gets or sets the selected <see cref="ISavedUserPreference"/>
        /// </summary>
        public T SelectedSavedUserPreference
        {
            get => this.selectedSavedUserPreference;
            set => this.RaiseAndSetIfChanged(ref this.selectedSavedUserPreference, value);
        }
              
        /// <summary>
        /// Gets the Ok Command
        /// </summary>
        public ReactiveCommand<Unit> OkCommand { get; private set; }

        /// <summary>
        /// Gets the Cancel Command
        /// </summary>
        public ReactiveCommand<object> CancelCommand { get; private set; }

        /// <summary>
        /// Gets the Delete selected command
        /// </summary>
        public ReactiveCommand<object> DeleteSelectedCommand { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManageSavedUserPreferencesDialogViewModel{T}"/> class.
        /// </summary>
        /// <param name="savedUserPreferences">The <see cref="IEnumerable{ISavedUserPreference}"/></param>
        /// <param name="saveAction"><see cref="Action{}"/> if <see cref="IEnumerable{T}>"/> to be executed on OkCommand execution</param>
        public ManageSavedUserPreferencesDialogViewModel(IEnumerable<T> savedUserPreferences, Action<IEnumerable<T>> saveAction) 
        {
            this.saveAction = saveAction;

            this.IsBusy = false;

            this.SavedUserPreferences = new ReactiveList<T>
            {
                ChangeTrackingEnabled = true
            };

            this.SavedUserPreferences.AddRange(savedUserPreferences);

            this.OkCommand = ReactiveCommand.CreateAsyncTask(x => this.ExecuteOk(), RxApp.MainThreadScheduler);
            this.OkCommand.ThrownExceptions.Select(ex => ex).Subscribe(x => { this.ErrorMessage = x.Message; });

            this.CancelCommand = ReactiveCommand.Create();
            this.CancelCommand.Subscribe(_ => this.ExecuteCancel());

            var canDelete = this.WhenAny(vm => vm.SelectedSavedUserPreference, sc => sc.Value != null);
            this.DeleteSelectedCommand = ReactiveCommand.Create(canDelete);
            this.DeleteSelectedCommand.Subscribe(_ => this.ExecuteDeleteSelected());
        }

        /// <summary>
        /// Executes the Ok Command
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task ExecuteOk()
        {
            this.IsBusy = true;

            try
            {
                this.LoadingMessage = "Saving...";
                await Task.Run(() => this.saveAction(this.SavedUserPreferences));

                this.DialogResult = new ManageConfigurationsResult(true);
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
            this.DialogResult = new ManageConfigurationsResult(false);
        }

        /// <summary>
        /// Executes the delete selected command
        /// </summary>
        private void ExecuteDeleteSelected()
        {
            if (this.SelectedSavedUserPreference == null)
            {
                return;
            }

            this.SavedUserPreferences.Remove(this.SelectedSavedUserPreference);
            this.SelectedSavedUserPreference = default;
        }
    }
}
