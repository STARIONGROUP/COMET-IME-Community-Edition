// -------------------------------------------------------------------------------------------------
// <copyright file="ValueSetDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4ParameterSheetGenerator.ViewModels
{
    using System;
    using System.Collections.Generic;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;    
    using CDP4Composition.Navigation;
    using CDP4ParameterSheetGenerator.Generator.ParameterSheet;
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

            this.ParameterOrOverrideWorkbookRebuildRowViewModels = new ReactiveList<WorkbookRebuildRowViewModel>
                                                    {
                                                        ChangeTrackingEnabled = true
                                                    };

            this.ParameterOrOverrideWorkbookRebuildRowViewModels.Changed.Subscribe(_ => this.UpdateOkCanExecute());
            this.ParameterOrOverrideWorkbookRebuildRowViewModels.ItemChanged.Subscribe(_ => this.UpdateOkCanExecute());

            this.ParameterSubscriptionWorkbookRebuildRowViewModels = new ReactiveList<WorkbookRebuildRowViewModel>
            {
                ChangeTrackingEnabled = true
            };


            this.ParameterSubscriptionWorkbookRebuildRowViewModels.Changed.Subscribe(_ => this.UpdateOkCanExecute());
            this.ParameterSubscriptionWorkbookRebuildRowViewModels.ItemChanged.Subscribe(_ => this.UpdateOkCanExecute());

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
        /// Gets the <see cref="WorkbookRebuildRowViewModel"/> that represent parameters and overrides that have been changed in the workbook.
        /// </summary>
        public ReactiveList<WorkbookRebuildRowViewModel> ParameterOrOverrideWorkbookRebuildRowViewModels { get; private set; }

        /// <summary>
        /// Gets the <see cref="WorkbookRebuildRowViewModel"/> that represent subscriptions that have been changed in the workbook.
        /// </summary>
        public ReactiveList<WorkbookRebuildRowViewModel> ParameterSubscriptionWorkbookRebuildRowViewModels { get; private set; }
    
        /// <summary>
        /// Gets the <see cref="Thing"/>s that have been changed on the parameter sheet.
        /// </summary>
        protected IReadOnlyDictionary<Guid, ProcessedValueSet> ProcessedValueSets { get; private set; }

        /// <summary>
        /// Populate the updated <see cref="Thing"/>s
        /// </summary>
        private void PopulateClones()
        {
            var processedValueSets = this.ProcessedValueSets.Values;

            foreach (var processedValueSet in processedValueSets)
            {
                var parameterValueSet = processedValueSet.ClonedThing as ParameterValueSet;
                if (parameterValueSet != null)
                {
                    var parameter = (Parameter)parameterValueSet.Container;

                    var scalarParameterType = parameter.ParameterType as ScalarParameterType;
                    if (scalarParameterType != null)
                    {
                        var parameterValueSetRow = new WorkbookRebuildRowViewModel(parameterValueSet, 0, processedValueSet.ValidationResult);
                        this.ParameterOrOverrideWorkbookRebuildRowViewModels.Add(parameterValueSetRow);
                    }

                    var compoundParameterType = parameter.ParameterType as CompoundParameterType;
                    if (compoundParameterType != null)
                    {
                        for (var i = 0; i < compoundParameterType.Component.Count; i++)
                        {
                            var parameterValueSetRow = new WorkbookRebuildRowViewModel(parameterValueSet, i, processedValueSet.ValidationResult);
                            this.ParameterOrOverrideWorkbookRebuildRowViewModels.Add(parameterValueSetRow);
                        }
                    }
                }

                var parameterOverrideValueSet = processedValueSet.ClonedThing as ParameterOverrideValueSet;
                if (parameterOverrideValueSet != null)
                {
                    var parameterOverride = (ParameterOverride)parameterOverrideValueSet.Container;

                    var scalarParameterType = parameterOverride.ParameterType as ScalarParameterType;
                    if (scalarParameterType != null)
                    {
                        var parameterValueSetRow = new WorkbookRebuildRowViewModel(parameterOverrideValueSet, 0, processedValueSet.ValidationResult);
                        this.ParameterOrOverrideWorkbookRebuildRowViewModels.Add(parameterValueSetRow);
                    }

                    var compoundParameterType = parameterOverride.ParameterType as CompoundParameterType;
                    if (compoundParameterType != null)
                    {
                        for (var i = 0; i < compoundParameterType.Component.Count; i++)
                        {
                            var parameterValueSetRow = new WorkbookRebuildRowViewModel(parameterOverrideValueSet, i, processedValueSet.ValidationResult);
                            this.ParameterOrOverrideWorkbookRebuildRowViewModels.Add(parameterValueSetRow);
                        }
                    }
                }

                var parameterSubscriptionValueSet = processedValueSet.ClonedThing as ParameterSubscriptionValueSet;
                if (parameterSubscriptionValueSet != null)
                {
                    var parameterSubscription = (ParameterSubscription)parameterSubscriptionValueSet.Container;

                    var scalarParameterType = parameterSubscription.ParameterType as ScalarParameterType;
                    if (scalarParameterType != null)
                    {
                        var parameterValueSetRow = new WorkbookRebuildRowViewModel(parameterSubscriptionValueSet, 0, processedValueSet.ValidationResult);
                        this.ParameterSubscriptionWorkbookRebuildRowViewModels.Add(parameterValueSetRow);
                    }

                    var compoundParameterType = parameterSubscription.ParameterType as CompoundParameterType;
                    if (compoundParameterType != null)
                    {
                        for (var i = 0; i < compoundParameterType.Component.Count; i++)
                        {
                            var parameterValueSetRow = new WorkbookRebuildRowViewModel(parameterSubscriptionValueSet, i, processedValueSet.ValidationResult);
                            this.ParameterSubscriptionWorkbookRebuildRowViewModels.Add(parameterValueSetRow);
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
            get { return this.okCanExecute; }
            set { this.RaiseAndSetIfChanged(ref this.okCanExecute, value); }
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
