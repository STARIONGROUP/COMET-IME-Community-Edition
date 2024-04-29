// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SubmitConfirmationViewModel.cs" company="Starion Group S.A.">
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
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive;
    using System.Windows.Input;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Validation;

    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Utilities;

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
        private ReactiveList<SubmitParameterRowViewModel> selectedParameters;

        /// <summary>
        /// Backing field for the <see cref="SelectedSubscriptions"/>
        /// </summary>
        private ReactiveList<SubmitParameterRowViewModel> selectedSubscriptions;

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
            this.SelectedParameters = new ReactiveList<SubmitParameterRowViewModel>();
            this.SelectedSubscriptions = new ReactiveList<SubmitParameterRowViewModel>();

            this.CancelCommand = ReactiveCommandCreator.Create(this.ExecuteCancel);
 
            this.SelectedParameters.CountChanged.Subscribe(_ => this.ToggleParameterSelection());
            this.SelectedParameters.AddRange(this.ParameterOrOverrideSubmitParameterRowViewModels.Where(r => !r.HasValidationError));

            this.SelectedSubscriptions.CountChanged.Subscribe(_ => this.ToggleSubscriptionsSelection());
            this.SelectedSubscriptions.AddRange(this.ParameterSubscriptionSubmitParameterRowViewModels.Where(r => !r.HasValidationError));

            var canOk = this.WhenAnyValue(vm => vm.OkCanExecute);
            this.OkCommand = ReactiveCommandCreator.Create(this.ExecuteOk, canOk);

            if (this.ProcessedValueSets.Any() && this.ProcessedValueSets.Values.Any(x => x.ValidationResult != ValidationResultKind.Valid))
            {
                this.InformationMessage = "Warning: There are invalid values found. These values will not be submitted even if selected!";
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
        public ReactiveCommand<Unit, Unit> OkCommand { get; private set; }

        /// <summary>
        /// Gets the Cancel <see cref="ICommand"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> CancelCommand { get; private set; }

        /// <summary>
        /// Gets or sets the human readable message that describes the changes that have been made.
        /// </summary>
        public string SubmitMessage
        {
            get => this.submitMessage;
            set => this.RaiseAndSetIfChanged(ref this.submitMessage, value);
        }

        /// <summary>
        /// Gets or sets the selected Parameters.
        /// </summary>
        public ReactiveList<SubmitParameterRowViewModel> SelectedParameters
        {
            get => this.selectedParameters;
            set => this.RaiseAndSetIfChanged(ref this.selectedParameters, value);
        }

        /// <summary>
        /// Gets or sets the selected ParameterSubscriptions.
        /// </summary>
        public ReactiveList<SubmitParameterRowViewModel> SelectedSubscriptions
        {
            get => this.selectedSubscriptions;
            set => this.RaiseAndSetIfChanged(ref this.selectedSubscriptions, value);
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
            this.OkCanExecute = this.ParameterOrOverrideSubmitParameterRowViewModels.Any(row => row.IsSelected) || this.ParameterSubscriptionSubmitParameterRowViewModels.Any(row => row.IsSelected);
        }

        /// <summary>
        /// Executes the <see cref="OkCommand"/>
        /// </summary>
        private void ExecuteOk()
        {
            var parameterOrOverrides = this.ParameterOrOverrideSubmitParameterRowViewModels.Where(x => x.IsSelected && !x.HasValidationError).Select(x => x.Thing).ToList();
            var parameterSubscriptions = this.ParameterSubscriptionSubmitParameterRowViewModels.Where(x => x.IsSelected && !x.HasValidationError).Select(x => x.Thing).ToList();

            var clones = parameterOrOverrides.Concat(parameterSubscriptions);

            this.DialogResult = new SubmitConfirmationDialogResult(true, this.submitMessage, clones);
        }

        /// <summary>
        /// Toggles the Parameter or ParameterOverride selection state based on the grid selection.
        /// </summary>
        private void ToggleParameterSelection()
        {
            foreach (var item in this.ParameterOrOverrideSubmitParameterRowViewModels)
            {
                item.IsSelected = this.SelectedParameters.Contains(item);
            }
        }

        /// <summary>
        /// Toggles the ParameterSubscriptions selection state based on the grid selection.
        /// </summary>
        private void ToggleSubscriptionsSelection()
        {
            foreach (var item in this.ParameterSubscriptionSubmitParameterRowViewModels)
            {
                item.IsSelected = this.SelectedSubscriptions.Contains(item);
            }
        }
    }
}
