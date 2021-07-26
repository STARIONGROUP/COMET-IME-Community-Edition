// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CustomFilterEditorDialogViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Simon Wood
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
//    along with this program. If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive;
    using System.Threading.Tasks;
    using System.Windows.Input;

    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.DragDrop;
    using CDP4Composition.Events;
    using CDP4Composition.FilterOperators;
    using CDP4Composition.Navigation;
    using CDP4Composition.Services.FilterEditorService;
    using CDP4Composition.Mvvm;

    using CDP4Dal;

    using DevExpress.Xpf.Core.FilteringUI;
    using DevExpress.Xpf.Grid;

    using Microsoft.Practices.ServiceLocation;

    using ReactiveUI;

    /// <summary>
    /// The custom filter editor dialog view model
    /// </summary>
    public class CustomFilterEditorDialogViewModel : DialogViewModelBase
    {
        /// <summary>
        /// The <see cref="IDialogNavigationService"/>
        /// </summary>
        private readonly IDialogNavigationService dialogNavigationService;

        /// <summary>
        /// The <see cref="DataViewBase"/> which this Filter Editor will use to filter data
        /// </summary>
        private readonly DataViewBase dataViewBase;

        /// <summary>
        /// The <see cref="ISession"/> to be used for data store updates.
        /// </summary>
        private ISession session;

        /// <summary>
        /// Backing field for <see cref="FilteringContext"/>
        /// </summary>
        private FilteringUIContext filteringContext;

        /// <summary>
        /// The viewmodel that implements <see cref="IHaveCustomFilterOperators"/> that is the parent of this dialog
        /// </summary>
        private readonly IHaveCustomFilterOperators customFilterOperatorsViewModel;

        /// <summary>
        /// The <see cref="ISavedUserPreferenceService"/> used to communicate with the <see cref="ISession"/>
        /// </summary>
        private readonly ISavedUserPreferenceService savedUserPreferenceService = ServiceLocator.Current.GetInstance<ISavedUserPreferenceService>();

        /// <summary>
        /// Backing field for <see cref="FilterEditorSavedUserPreferences"/>
        /// </summary>
        private ReactiveList<FilterEditorSavedUserPreference> filterEditorSavedUserPreferences = new ReactiveList<FilterEditorSavedUserPreference>();

        /// <summary>
        /// Backing field for <see cref="SelectedFilterEditorSavedUserPreference"/>
        /// </summary>
        private FilterEditorSavedUserPreference selectedFilterEditorSavedUserPreference;

        /// <summary>
        /// Backing field for <see cref="IsSaveUserPreferenceAllowed"/>
        /// </summary>
        private bool isSaveUserPreferenceAllowed;

        /// <summary>
        /// Gets the <see cref="IView"/> for which filter settings can be stored
        /// </summary>
        private readonly IView currentView;

        /// <summary>
        /// The <see cref="List{T}"/> that contains unfiltered <see cref="FilterEditorSavedUserPreference"/>s.
        /// Unfiltered means that it contains data for all <see cref="DataViewBase"/> controls in a View (UserControl, DXWindow, etc.).
        /// </summary>
        private List<FilterEditorSavedUserPreference> unfilteredFilterEditorSavedUserPreferences;

        /// <summary>
        /// Gets or sets a value indicating whether the FilterEditor Settings panel is visible
        /// </summary>
        public bool IsSaveUserPreferenceAllowed
        {
            get => this.isSaveUserPreferenceAllowed;
            set => this.RaiseAndSetIfChanged(ref this.isSaveUserPreferenceAllowed, value);
        }

        /// <summary>
        /// Changes a list of QueryOperators in a FilterEditorControl
        /// </summary>
        public ReactiveCommand<Unit> QueryOperatorsCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to apply and close
        /// </summary>
        public ReactiveCommand<object> OkCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to cancel
        /// </summary>
        public ReactiveCommand<object> CancelCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to apply
        /// </summary>
        public ReactiveCommand<object> ApplyCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to save the <see cref="FilterEditorSavedUserPreference"/> in <see cref="SelectedFilterEditorSavedUserPreference"/>
        /// </summary>
        public ReactiveCommand<object> SaveCurrentFilterEditorSavedUserPreferenceCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to manage the <see cref="FilterEditorSavedUserPreference"/> in <see cref="FilterEditorSavedUserPreferences"/>
        /// </summary>
        public ReactiveCommand<object> ManageSavedFilterEditorSavedUserPreferencesCommand { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfirmationDialogViewModel"/> class
        /// </summary>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/></param>
        /// <param name="dataViewBase">The <see cref="GridDataControlBase"/> where the filter should be done for</param>
        public CustomFilterEditorDialogViewModel(IDialogNavigationService dialogNavigationService, DataViewBase dataViewBase)
        {
            this.dialogNavigationService = dialogNavigationService;
            this.dataViewBase = dataViewBase;
            this.currentView = this.dataViewBase?.GetVisualAncestor<IView>();

            this.FilteringContext = dataViewBase?.DataControl?.FilteringContext;
            this.customFilterOperatorsViewModel = dataViewBase?.DataContext as IHaveCustomFilterOperators;
            this.InitializeCommands();
            this.InitializeFilterEditorSettings();
        }

        /// <summary>
        /// Initializes the data for the filter editor settings panel
        /// </summary>
        private void InitializeFilterEditorSettings()
        {
            this.IsSaveUserPreferenceAllowed = false;

            if (this.dataViewBase == null)
            {
                return;
            }

            if (this.dataViewBase.DataControl.DataContext is IISession iSession)
            {
                this.session = iSession.Session;
            }
            else
            {
                return;
            }

            if (this.currentView == null)
            {
                return;
            }

            this.IsSaveUserPreferenceAllowed = true;

            this.ReloadUserPreferences();
        }

        /// <summary>
        /// (Re)loads the correct preferences from the data store
        /// </summary>
        private void ReloadUserPreferences()
        {
            this.IsBusy = true;

            this.FilterEditorSavedUserPreferences.Clear();

            this.unfilteredFilterEditorSavedUserPreferences =
                this.savedUserPreferenceService.GetUserPreference<IEnumerable<FilterEditorSavedUserPreference>>(this.session, this.GetUserPreferenceKey())
                    ?.ToList() ?? new List<FilterEditorSavedUserPreference>();

            this.FilterEditorSavedUserPreferences.AddRange(
                this.unfilteredFilterEditorSavedUserPreferences
                    .Where(x => x.DataControlName?.Equals(this.dataViewBase.Name) ?? true));

            this.IsBusy = false;
        }

        /// <summary>
        /// Gets the key of the <see cref="UserPreference"/>.
        /// </summary>
        /// <returns></returns>
        private string GetUserPreferenceKey()
        {
            return $"FilterEditorSettings_{this.currentView?.GetType().FullName}";
        }

        /// <summary>
        /// Gets or Sets the <see cref="FilteringUIContext"/> used in the <see cref="FilterEditorControl"/>
        /// </summary>
        public FilteringUIContext FilteringContext
        {
            get => this.filteringContext;
            set => this.RaiseAndSetIfChanged(ref this.filteringContext, value);
        }

        /// <summary>
        /// List containing the already saved <see cref="FilterEditorSavedUserPreference"/>s
        /// </summary>
        public ReactiveList<FilterEditorSavedUserPreference> FilterEditorSavedUserPreferences
        {
            get => this.filterEditorSavedUserPreferences;
            set => this.RaiseAndSetIfChanged(ref this.filterEditorSavedUserPreferences, value);
        }

        /// <summary>
        /// The selected <see cref="FilterEditorSavedUserPreference"/>
        /// </summary>
        public FilterEditorSavedUserPreference SelectedFilterEditorSavedUserPreference
        {
            get => this.selectedFilterEditorSavedUserPreference;
            set => this.RaiseAndSetIfChanged(ref this.selectedFilterEditorSavedUserPreference, value);
        }

        /// <summary>
        /// Initializes all <see cref="ICommand"/>s
        /// </summary>
        private void InitializeCommands()
        {
            this.QueryOperatorsCommand = ReactiveCommand.CreateAsyncTask(this.ExecuteQueryOperatorsCommand, RxApp.MainThreadScheduler);

            this.OkCommand = ReactiveCommand.Create();
            this.OkCommand.Subscribe(_ => this.ExecuteOkCommand());

            this.CancelCommand = ReactiveCommand.Create();
            this.CancelCommand.Subscribe(_ => this.ExecuteCancelCommand());

            this.ApplyCommand = ReactiveCommand.Create();
            this.ApplyCommand.Subscribe(_ => this.ExecuteApplyCommand());

            this.SaveCurrentFilterEditorSavedUserPreferenceCommand = ReactiveCommand.Create();
            this.SaveCurrentFilterEditorSavedUserPreferenceCommand.Subscribe(_ => this.ExecuteSaveCurrentFilterEditorSettingCommand());

            this.ManageSavedFilterEditorSavedUserPreferencesCommand = ReactiveCommand.Create();
            this.ManageSavedFilterEditorSavedUserPreferencesCommand.Subscribe(_ => this.ExecuteManageSavedFilterEditorSavedUserPreferencesCommand());

            this.WhenAnyValue(x => x.SelectedFilterEditorSavedUserPreference).Subscribe(_ => this.SetFilterString());
        }

        /// <summary>
        /// Executes the <see cref="ManageSavedFilterEditorSavedUserPreferencesCommand"/> command.
        /// </summary>
        private void ExecuteManageSavedFilterEditorSavedUserPreferencesCommand()
        {
            var currentFilterEditorSavedUserPreferences = this.FilterEditorSavedUserPreferences;
            var currentSession = this.session;
            var currentUserPreferenceKey = this.GetUserPreferenceKey();

            var manageSavedUserPreferencesDialogViewModel = new ManageSavedUserPreferencesDialogViewModel<FilterEditorSavedUserPreference>(
                currentFilterEditorSavedUserPreferences,
                x =>
                {
                    this.savedUserPreferenceService.SaveUserPreference(currentSession, currentUserPreferenceKey, x)
                        .GetAwaiter()
                        .GetResult();
                });

            var result = this.dialogNavigationService.NavigateModal(manageSavedUserPreferencesDialogViewModel);

            if (result?.Result == null || !result.Result.Value)
            {
                return;
            }

            this.ReloadUserPreferences();
        }

        /// <summary>
        /// Executes the <see cref="SaveCurrentFilterEditorSavedUserPreferenceCommand"/> command.
        /// </summary>
        private void ExecuteSaveCurrentFilterEditorSettingCommand()
        {
            if (this.currentView == null)
            {
                throw new NotSupportedException($"Parent window not found for this {nameof(GridViewBase)}.");
            }

            this.ApplyFilter();

            var savedUserPreference =
                this.SelectedFilterEditorSavedUserPreference ?? new FilterEditorSavedUserPreference(this.dataViewBase.Name, "", "");

            savedUserPreference.Value = this.dataViewBase?.DataControl.FilterString;

            var currentFilterEditorSavedUserPreferences = this.unfilteredFilterEditorSavedUserPreferences;
            var currentSession = this.session;
            var currentUserPreferenceKey = this.GetUserPreferenceKey();

            var savedUserPreferenceDialogViewModel = new SavedUserPreferenceDialogViewModel(
                savedUserPreference,
                () =>
                {
                    if (!currentFilterEditorSavedUserPreferences.Contains(savedUserPreference))
                    {
                        currentFilterEditorSavedUserPreferences.Add(savedUserPreference);
                    }

                    this.savedUserPreferenceService.SaveUserPreference(currentSession, currentUserPreferenceKey, currentFilterEditorSavedUserPreferences)
                        .GetAwaiter()
                        .GetResult();
                });

            var result = this.dialogNavigationService.NavigateModal(savedUserPreferenceDialogViewModel);

            if (result?.Result == null || !result.Result.Value)
            {
                return;
            }

            this.ReloadUserPreferences();
        }

        /// <summary>
        /// Replaces the linked <see cref="DataViewBase"/>'s <see cref="DataControlBase"/> <see cref="DataControlBase.FilterString"/> property
        /// with the one from the <see cref="SelectedFilterEditorSavedUserPreference"/>.
        /// </summary>
        private void SetFilterString()
        {
            if (this.dataViewBase == null)
            {
                return;
            }

            var filterString = this.SelectedFilterEditorSavedUserPreference?.Value;

            if (filterString == null)
            {
                return;
            }

            var refreshFilteringContext = this.FilteringContext;
            this.FilteringContext = null;

            this.dataViewBase.DataControl.FilterString = filterString;

            this.FilteringContext = refreshFilteringContext;
        }

        /// <summary>
        /// Executes the <see cref="ApplyCommand"/> command.
        /// </summary>
        private void ExecuteApplyCommand()
        {
            this.ApplyFilter();
        }

        /// <summary>
        /// Executes the <see cref="CancelCommand"/> command.
        /// </summary>
        private void ExecuteCancelCommand()
        {
            this.Close();
        }

        /// <summary>
        /// Executes the <see cref="OkCommand"/> command.
        /// </summary>
        private void ExecuteOkCommand()
        {
            this.ApplyFilter();
            this.Close();
        }

        /// <summary>
        /// Executes the <see cref="QueryOperatorsCommand"/> command.
        /// </summary>
        private async Task ExecuteQueryOperatorsCommand(object eventArgs)
        {
            if (this.customFilterOperatorsViewModel == null)
            {
                return;
            }

            if (!(eventArgs is FilterEditorQueryOperatorsEventArgs filterEditorQueryOperatorsEventArgs))
            {
                throw new NotSupportedException($"{eventArgs?.GetType().Name} is not supported as {nameof(eventArgs)}.");
            }

            var filterOperatiors = this.customFilterOperatorsViewModel.CustomFilterOperators;

            if (!filterOperatiors.ContainsKey(this.dataViewBase))
            {
                return;
            }

            var filterOperatorsByFieldName = filterOperatiors[this.dataViewBase];

            if (!filterOperatorsByFieldName.ContainsKey(filterEditorQueryOperatorsEventArgs.FieldName))
            {
                return;
            }

            var (customFilterOperatorType, rowViewModelBases) = filterOperatorsByFieldName[filterEditorQueryOperatorsEventArgs.FieldName];

            var filterHandler = CustomFilterOperatorHandlerFactory.CreateFilterOperatorHandler(
                customFilterOperatorType,
                rowViewModelBases,
                filterEditorQueryOperatorsEventArgs.FieldName);

            filterHandler.SetOperators(filterEditorQueryOperatorsEventArgs);
        }

        /// <summary>
        /// Closes the dialog
        /// </summary>
        private void Close()
        {
            this.DialogResult = new BaseDialogResult(true);
        }

        /// <summary>
        /// Applies the current filter settings to the linked <see cref="DataViewBase"/>
        /// </summary>
        private void ApplyFilter()
        {
            CDPMessageBus.Current.SendMessage(new ApplyFilterEvent());
        }
    }
}
