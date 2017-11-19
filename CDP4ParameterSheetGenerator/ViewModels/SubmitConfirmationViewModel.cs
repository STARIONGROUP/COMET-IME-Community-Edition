// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SubmitConfirmationViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ParameterSheetGenerator.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;    
    using CDP4Common.Validation;
    using CDP4Composition.Navigation;
    using Generator.ParameterSheet;
    using ReactiveUI;
    
    /// <summary>
    /// The purpose of the <see cref="SubmitConfirmationViewModel"/> is to present the changed <see cref="Thing"/>s
    /// before they are submitted as well as offering the opportunity to add a log entry or cancel.
    /// </summary>
    public class SubmitConfirmationViewModel : ValueSetDialogViewModel
    {
        /// <summary>
        /// Backing field for the <see cref="SubmitMessage"/>.
        /// </summary>
        private string submitMessage;

        /// <summary>
        /// Backing field for the <see cref="SelectedParameters"/>
        /// </summary>
        private ReactiveList<WorkbookRebuildRowViewModel> selectedParameters;

        /// <summary>
        /// Backing field for the <see cref="SelectedSubscriptions"/>
        /// </summary>
        private ReactiveList<WorkbookRebuildRowViewModel> selectedSubscriptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubmitConfirmationViewModel"/> class.
        /// </summary>
        /// <param name="processedValueSets">
        /// The <see cref="Thing"/>s that have been changed on the workbook.
        /// </param>
        /// <param name="valueSetKind">
        /// assertion that is used to determine the visibilty of the <see cref="ParameterOrOverrideBase"/> and <see cref="ParameterSubscription"/>
        /// </param>
        public SubmitConfirmationViewModel(IReadOnlyDictionary<Guid, ProcessedValueSet> processedValueSets, ValueSetKind valueSetKind)
            : base(processedValueSets, valueSetKind)
        {
            this.DialogTitle = "Submit Changes...";
            this.SelectedParameters = new ReactiveList<WorkbookRebuildRowViewModel>();
            this.SelectedSubscriptions = new ReactiveList<WorkbookRebuildRowViewModel>();

            this.CancelCommand = ReactiveCommand.Create();
            this.CancelCommand.Subscribe(_ => this.ExecuteCancel());

            this.SelectedParameters.ChangeTrackingEnabled = true;
            this.SelectedParameters.CountChanged.Subscribe(_ => this.ToggleParameterSelection());

            this.SelectedParameters.AddRange(this.ParameterOrOverrideWorkbookRebuildRowViewModels.Where(r=>!r.HasValidationError));

            this.SelectedSubscriptions.ChangeTrackingEnabled = true;
            this.SelectedSubscriptions.CountChanged.Subscribe(_ => this.ToggleSubscriptionsSelection());

            this.SelectedSubscriptions.AddRange(this.ParameterSubscriptionWorkbookRebuildRowViewModels.Where(r => !r.HasValidationError));

            var canOk = this.WhenAnyValue(vm => vm.OkCanExecute);
            this.OkCommand = ReactiveCommand.Create(canOk);
            this.OkCommand.Subscribe(_ => this.ExecuteOk());

            if (this.ProcessedValueSets.Any() && this.ProcessedValueSets.Values.Any(x => x.ValidationResult != ValidationResultKind.Valid))
            {
                this.InformationMessage = "Warning: There are invalid values in the sheet. These values will not be submitted even if selected!";
                this.IsInformationMessageVisible = true;
            }
        }

        /// <summary>
        /// Gets the dialog box title
        /// </summary>
        public string DialogTitle { get; private set; }

        /// <summary>
        /// Gets the information message
        /// </summary>
        public string InformationMessage { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the information message is visible
        /// </summary>
        public bool IsInformationMessageVisible { get; private set; }

        /// <summary>
        /// Gets the Select <see cref="ICommand"/>
        /// </summary>
        public ReactiveCommand<object> OkCommand { get; private set; }

        /// <summary>
        /// Gets the Cancel <see cref="ICommand"/>
        /// </summary>
        public ReactiveCommand<object> CancelCommand { get; private set; }

        /// <summary>
        /// Gets or sets the human readable message that describes the changes that have been made.
        /// </summary>
        public string SubmitMessage
        {
            get { return this.submitMessage; }
            set { this.RaiseAndSetIfChanged(ref this.submitMessage, value); }
        }

        /// <summary>
        /// Gets or sets the selected Parameters.
        /// </summary>
        public ReactiveList<WorkbookRebuildRowViewModel> SelectedParameters
        {
            get { return this.selectedParameters; }
            set { this.RaiseAndSetIfChanged(ref this.selectedParameters, value); }
        }

        /// <summary>
        /// Gets or sets the selected ParameterSubscriptions.
        /// </summary>
        public ReactiveList<WorkbookRebuildRowViewModel> SelectedSubscriptions
        {
            get { return this.selectedSubscriptions; }
            set { this.RaiseAndSetIfChanged(ref this.selectedSubscriptions, value); }
        }

        /// <summary>
        /// Executes the <see cref="CancelCommand"/>
        /// </summary>
        private void ExecuteCancel()
        {
            this.DialogResult = new BaseDialogResult(false);
        }

        /// <summary>
        /// Returns whether it is possible to close the current dialog by clicking the OK button
        /// </summary>
        protected override void UpdateOkCanExecute()
        {
            this.OkCanExecute = this.ParameterOrOverrideWorkbookRebuildRowViewModels.Any(row => row.IsSelected) || this.ParameterSubscriptionWorkbookRebuildRowViewModels.Any(row => row.IsSelected);
        }

        /// <summary>
        /// Executes the <see cref="OkCommand"/>
        /// </summary>
        private void ExecuteOk()
        {
            var parameterOrOverrides = this.ParameterOrOverrideWorkbookRebuildRowViewModels.Where(x => x.IsSelected && !x.HasValidationError).Select(x => x.Thing).ToList();
            var parameterSubscriptions = this.ParameterSubscriptionWorkbookRebuildRowViewModels.Where(x => x.IsSelected && !x.HasValidationError).Select(x => x.Thing).ToList();

            var clones = parameterOrOverrides.Concat(parameterSubscriptions);

            this.DialogResult = new SubmitConfirmationDialogResult(true, this.submitMessage, clones);
        }

        /// <summary>
        /// Toggles the Parameter or ParameterOverride selection state based on the grid selection.
        /// </summary>
        private void ToggleParameterSelection()
        {
            foreach (var item in this.ParameterOrOverrideWorkbookRebuildRowViewModels)
            {
                item.IsSelected = this.SelectedParameters.Contains(item);
            }
        }

        /// <summary>
        /// Toggles the ParameterSubscriptions selection state based on the grid selection.
        /// </summary>
        private void ToggleSubscriptionsSelection()
        {
            foreach (var item in this.ParameterSubscriptionWorkbookRebuildRowViewModels)
            {
                item.IsSelected = this.SelectedSubscriptions.Contains(item);
            }
        }
    }
}
