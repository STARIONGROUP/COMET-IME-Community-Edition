// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ValueSetDialogViewModel.cs" company="Starion Group S.A.">
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

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Utilities;

    using DynamicData;

    using ReactiveUI;

    /// <summary>
    /// Abstract super class that is used for any <see cref="DialogViewModelBase"/> that needs to load value-sets
    /// that have been changed by the user on the parameter sheet.
    /// </summary>
    public abstract class ValueSetDialogViewModel : DialogViewModelBase
    {
        /// <summary>
        /// Backing field for <see cref="UpdateOkCanExecute"/>
        /// </summary>
        private bool okCanExecute;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueSetDialogViewModel"/> class.
        /// </summary>
        /// <param name="processedValueSets">
        /// The <see cref="ProcessedValueSet"/>s that references <see cref="Thing"/>s that have been changed on the workbook.
        /// </param>
        /// <param name="valueSetKind">
        /// assertion that is used to determine the visibilty of the <see cref="ParameterOrOverrideBase"/> and <see cref="ParameterSubscription"/>
        /// </param>
        protected ValueSetDialogViewModel(IReadOnlyDictionary<Guid, ProcessedValueSet> processedValueSets, ValueSetKind valueSetKind)
        {
            this.ProcessValueSetKind(valueSetKind);

            this.ParameterOrOverrideSubmitParameterRowViewModels = new TrackedReactiveList<SubmitParameterRowViewModel>();
            this.ParameterOrOverrideSubmitParameterRowViewModels.Changed.Subscribe(_ => this.UpdateOkCanExecute());
            this.ParameterOrOverrideSubmitParameterRowViewModels.ItemChanged.WhenAnyPropertyChanged().Subscribe(_ => this.UpdateOkCanExecute());

            this.ParameterSubscriptionSubmitParameterRowViewModels = new TrackedReactiveList<SubmitParameterRowViewModel>();
            this.ParameterSubscriptionSubmitParameterRowViewModels.Changed.Subscribe(_ => this.UpdateOkCanExecute());
            this.ParameterSubscriptionSubmitParameterRowViewModels.ItemChanged.WhenAnyPropertyChanged().Subscribe(_ => this.UpdateOkCanExecute());

            this.ProcessedValueSets = processedValueSets;

            this.PopulateClones();
        }

        /// <summary>
        /// processes the <see cref="ValueSetKind"/> to set the <see cref="IsParameterOrOVerrideVisible"/> and <see cref="IsParameterSubscriptionVisible"/> properties.
        /// </summary>
        /// <param name="valueSetKind">
        /// The <see cref="ValueSetKind"/> that is processed.
        /// </param>
        private void ProcessValueSetKind(ValueSetKind valueSetKind)
        {
            switch (valueSetKind)
            {
                  case  ValueSetKind.All:
                    this.IsParameterOrOVerrideVisible = true;
                    this.IsParameterSubscriptionVisible = true;                    
                    break;
                case ValueSetKind.ParameterAndOrverride:
                    this.IsParameterOrOVerrideVisible = true;
                    this.IsParameterSubscriptionVisible = false;                    
                    break;
                case ValueSetKind.ParameterSubscription:
                    this.IsParameterOrOVerrideVisible = false;
                    this.IsParameterSubscriptionVisible = true;
                    break;
            }
        }

        /// <summary>
        /// Gets the <see cref="SubmitParameterRowViewModel"/> that represent parameters and overrides that have been changed in the workbook.
        /// </summary>
        public TrackedReactiveList<SubmitParameterRowViewModel> ParameterOrOverrideSubmitParameterRowViewModels { get; private set; }

        /// <summary>
        /// Gets the <see cref="SubmitParameterRowViewModel"/> that represent subscriptions that have been changed in the workbook.
        /// </summary>
        public TrackedReactiveList<SubmitParameterRowViewModel> ParameterSubscriptionSubmitParameterRowViewModels { get; private set; }
    
        /// <summary>
        /// Gets the <see cref="Thing"/>s that have been changed on the parameter sheet.
        /// </summary>
        protected IReadOnlyDictionary<Guid, ProcessedValueSet> ProcessedValueSets { get; private set; }

        /// <summary>
        /// Populate the updated <see cref="Thing"/>s
        /// </summary>
        private void PopulateClones()
        {
            foreach (var processedValueSet in this.ProcessedValueSets.Values)
            {
                if (processedValueSet.ClonedThing is ParameterValueSet parameterValueSet)
                {
                    var parameter = (Parameter) parameterValueSet.Container;

                    if (parameter.ParameterType is ScalarParameterType)
                    {
                        var parameterValueSetRow = new SubmitParameterRowViewModel(parameterValueSet, 0, processedValueSet.ValidationResult);
                        this.ParameterOrOverrideSubmitParameterRowViewModels.Add(parameterValueSetRow);
                    }

                    if (parameter.ParameterType is CompoundParameterType compoundParameterType)
                    {
                        for (var i = 0; i < compoundParameterType.Component.Count; i++)
                        {
                            var parameterValueSetRow = new SubmitParameterRowViewModel(parameterValueSet, i, processedValueSet.ValidationResult);
                            this.ParameterOrOverrideSubmitParameterRowViewModels.Add(parameterValueSetRow);
                        }
                    }
                }

                if (processedValueSet.ClonedThing is ParameterOverrideValueSet parameterOverrideValueSet)
                {
                    var parameterOverride = (ParameterOverride) parameterOverrideValueSet.Container;

                    if (parameterOverride.ParameterType is ScalarParameterType)
                    {
                        var parameterValueSetRow = new SubmitParameterRowViewModel(parameterOverrideValueSet, 0, processedValueSet.ValidationResult);
                        this.ParameterOrOverrideSubmitParameterRowViewModels.Add(parameterValueSetRow);
                    }

                    if (parameterOverride.ParameterType is CompoundParameterType compoundParameterType)
                    {
                        for (var i = 0; i < compoundParameterType.Component.Count; i++)
                        {
                            var parameterValueSetRow = new SubmitParameterRowViewModel(parameterOverrideValueSet, i, processedValueSet.ValidationResult);
                            this.ParameterOrOverrideSubmitParameterRowViewModels.Add(parameterValueSetRow);
                        }
                    }
                }

                if (processedValueSet.ClonedThing is ParameterSubscriptionValueSet parameterSubscriptionValueSet)
                {
                    var parameterSubscription = (ParameterSubscription) parameterSubscriptionValueSet.Container;

                    if (parameterSubscription.ParameterType is ScalarParameterType)
                    {
                        var parameterValueSetRow = new SubmitParameterRowViewModel(parameterSubscriptionValueSet, 0, processedValueSet.ValidationResult);
                        this.ParameterSubscriptionSubmitParameterRowViewModels.Add(parameterValueSetRow);
                    }

                    if (parameterSubscription.ParameterType is CompoundParameterType compoundParameterType)
                    {
                        for (var i = 0; i < compoundParameterType.Component.Count; i++)
                        {
                            var parameterValueSetRow = new SubmitParameterRowViewModel(parameterSubscriptionValueSet, i, processedValueSet.ValidationResult);
                            this.ParameterSubscriptionSubmitParameterRowViewModels.Add(parameterValueSetRow);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns whether it is possible to close the current dialog by clicking the OK button
        /// </summary>
        protected virtual void UpdateOkCanExecute()
        {
            this.OkCanExecute = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="OkCommand"/> can be executed
        /// </summary>
        public bool OkCanExecute
        {
            get => this.okCanExecute;
            set => this.RaiseAndSetIfChanged(ref this.okCanExecute, value);
        }

        /// <summary>
        /// Gets a value that indicates whether the <see cref="ParameterOrOVerride"/> are visible
        /// </summary>
        public bool IsParameterOrOVerrideVisible { get; private set; }

        /// <summary>
        /// Gets a value that indicates whether the <see cref="ParameterSubscription"/> are visible
        /// </summary>
        public bool IsParameterSubscriptionVisible { get; private set; }
    }
}
