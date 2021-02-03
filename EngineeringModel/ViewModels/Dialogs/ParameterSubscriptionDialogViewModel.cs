// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterSubscriptionDialogViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Nathanael Smiechowski, Ahmed Ahmed.
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
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//    Lesser General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4CommonView;

    using CDP4Composition.Attributes;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Mvvm.Types;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.Operations;

    using CDP4EngineeringModel.ViewModels.Dialogs;

    using ReactiveUI;

    /// <summary>
    /// The dialog-view model to create, edit or inspect a <see cref="ParameterSubscription" />
    /// </summary>
    [ThingDialogViewModelExport(ClassKind.ParameterSubscription)]
    public class ParameterSubscriptionDialogViewModel : CDP4CommonView.ParameterSubscriptionDialogViewModel, IThingDialogViewModel
    {
        /// <summary>
        /// Backing field for the <see cref="ModelCode" /> property.
        /// </summary>
        private string modelCode;

        /// <summary>
        /// Backing field for <see cref="AvailableValueSets" />
        /// </summary>
        private ReactiveList<ValueSetRowViewModel> availableValueSets;

        /// <summary>
        /// Backing field for <see cref="DisplayedValueSet" />
        /// </summary>
        private ValueSetRowViewModel displayedValueSet;

        /// <summary>
        /// Backing field for <see cref="IsValueSetEditable" />
        /// </summary>
        private bool isValueSetEditable;

        /// <summary>
        /// Backing field for <see cref="SelectedGroupSelection" />
        /// </summary>
        private GroupSelectionViewModel selectedGroupSelection;

        /// <summary>
        /// Backing field for <see cref="SelectedValueSet" />
        /// </summary>
        private ParameterRowViewModel selectedValueSet;

        /// <summary>
        /// Backing field for <see cref="selectedValueSetGridViewModel" />
        /// </summary>
        private SampledFunctionParameterTypeValueSetGridViewModel selectedValueSetGridViewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterSubscriptionDialogViewModel" /> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public ParameterSubscriptionDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterSubscriptionDialogViewModel" /> class
        /// </summary>
        /// <param name="parameterSubscription">
        /// The parameterSubscription.
        /// </param>
        /// <param name="transaction">
        /// The <see cref="ThingTransaction" /> that contains the log of recorded changes.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession" /> in which the current <see cref="Thing" /> is to be added or updated
        /// </param>
        /// <param name="isRoot">
        /// Assert if this <see cref="DialogViewModelBase{T}" /> is the root of all <see cref="DialogViewModelBase{T}" />
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation this <see cref="DialogViewModelBase{T}" /> performs
        /// </param>
        /// <param name="thingDialogNavigationService">
        /// The <see cref="IThingDialogNavigationService" /> that is used to navigate to a dialog of a specific
        /// <see cref="Thing" />.
        /// </param>
        /// <param name="container">
        /// The Container <see cref="Thing" /> of the created <see cref="MultiRelationshipRule" />
        /// </param>
        /// <param name="chainOfContainers">
        /// The optional chain of containers that contains the <paramref name="container" /> argument
        /// </param>
        public ParameterSubscriptionDialogViewModel(ParameterSubscription parameterSubscription, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
            : base(parameterSubscription, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            this.WhenAnyValue(vm => vm.DisplayedValueSet).Where(x => x != null).Subscribe(_ => this.LoadValueSetGrid());
            this.IsNameVisible = this.Thing.ParameterType is CompoundParameterType || this.Thing.IsOptionDependent || this.Thing.StateDependence != null;
            this.CheckValueValidation();
        }

        /// <summary>
        /// Gets a value indicating whether is owner visible.
        /// </summary>
        public bool IsOwnerVisible
        {
            get { return true; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether is name visible.
        /// </summary>
        public bool IsNameVisible { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the ParameterType is SampledFunctionParameter.
        /// </summary>
        public bool IsSampledFunctionParameter
        {
            get { return this.Thing.ParameterType is SampledFunctionParameterType; }
        }

        /// <summary>
        /// Gets the available value sets
        /// </summary>
        public ReactiveList<ValueSetRowViewModel> AvailableValueSets
        {
            get { return this.availableValueSets; }
            set { this.RaiseAndSetIfChanged(ref this.availableValueSets, value); }
        }

        /// <summary>
        /// Gets or sets the selected <see cref="ParameterValueSetRowViewModel" />
        /// </summary>
        public ValueSetRowViewModel DisplayedValueSet
        {
            get { return this.displayedValueSet; }
            set { this.RaiseAndSetIfChanged(ref this.displayedValueSet, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="ParameterSubscriptionValueSet" />
        /// </summary>
        public DisposableReactiveList<Dialogs.ParameterSubscriptionRowViewModel> ValueSet { get; protected set; }

        /// <summary>
        /// Gets the parameter that contains the subscription represented by this view-model.
        /// </summary>
        public string SubcribedParameter
        {
            get { return string.Format("{0} ({1})", ((ParameterOrOverrideBase)this.Thing.Container).ParameterType.Name, this.Thing.Container.GetContainerOfType<ElementDefinition>().Name); }
        }

        /// <summary>
        /// Gets the list of value set grid view models
        /// </summary>
        public ReactiveList<SampledFunctionParameterTypeValueSetGridViewModel> ValueSetGridViewModels { get; private set; }

        /// <summary>
        /// Gets the selected value set grid view model
        /// </summary>
        public SampledFunctionParameterTypeValueSetGridViewModel SelectedValueSetGridViewModel
        {
            get { return this.selectedValueSetGridViewModel; }
            set { this.RaiseAndSetIfChanged(ref this.selectedValueSetGridViewModel, value); }
        }

        /// <summary>
        /// Gets a value indicating whether the parameter Subscription is state dependent.
        /// </summary>
        public bool IsStateDependent
        {
            get { return ((ParameterOrOverrideBase)this.Container).StateDependence != null; }
        }

        /// <summary>
        /// Gets a value indicating whether the parameter is option dependent.
        /// </summary>
        public bool IsOptionDependent
        {
            get { return ((ParameterOrOverrideBase)this.Container).IsOptionDependent; }
        }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand" /> to inspect the <see cref="SubcribedParameter" />
        /// </summary>
        public ReactiveCommand<object> InspectSubcribedParameterCommand { get; protected set; }

        /// <summary>
        /// Gets a value indicating if the scale shall be made visible
        /// </summary>
        public bool IsScaleVisible
        {
            get { return this.Thing.ParameterType is CompoundParameterType; }
        }

        public string ModelCode
        {
            get { return this.modelCode; }
            set { this.RaiseAndSetIfChanged(ref this.modelCode, value); }
        }

        /// <summary>
        /// Selects the correct value set grid viewmodel to be the data context for display
        /// </summary>
        private void LoadValueSetGrid()
        {
            var vm = this.ValueSetGridViewModels.FirstOrDefault(v => v.ValueSet == this.DisplayedValueSet.ValueSet);

            if (vm != null)
            {
                this.SelectedValueSetGridViewModel = vm;
            }
        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.ValueSet = new DisposableReactiveList<Dialogs.ParameterSubscriptionRowViewModel>();

            this.AvailableValueSets = new ReactiveList<ValueSetRowViewModel>
            {
                ChangeTrackingEnabled = true
            };

            this.ValueSetGridViewModels = new ReactiveList<SampledFunctionParameterTypeValueSetGridViewModel>
            {
                ChangeTrackingEnabled = true
            };
        }

        /// <summary>
        /// Initializes the <see cref="ICommand" />s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            var canExecuteInspectSelectedParameterCommand = this.WhenAny(vm => vm.SubcribedParameter, v => v.Value != null);
            this.InspectSubcribedParameterCommand = ReactiveCommand.Create(canExecuteInspectSelectedParameterCommand);
            this.InspectSubcribedParameterCommand.Subscribe(_ => this.ExecuteInspectCommand(this.Container));
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.SelectedParameterType = ((ParameterOrOverrideBase)this.Thing.Container).ParameterType;
            this.PossibleOwner.Add(this.SelectedOwner);
            this.PopulateValueSet();

            this.ModelCode = this.Thing.ModelCode();
        }

        /// <summary>
        /// Populates the <see cref="ValueSet" /> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected void PopulateValueSet()
        {
            this.ValueSet.ClearAndDispose();
            this.AvailableValueSets.Clear();

            if (this.IsSampledFunctionParameter)
            {
                this.ValueSetGridViewModels.Clear();

                foreach (var parameterValueSet in this.Thing.ValueSet)
                {
                    var newGridVm = new SampledFunctionParameterTypeValueSetGridViewModel(parameterValueSet, (SampledFunctionParameterType)this.Thing.ParameterType, !this.IsReadOnly);
                    newGridVm.PopulateSampledFunctionParameterTypeValueGrid();

                    this.ValueSetGridViewModels.Add(newGridVm);
                    this.AvailableValueSets.Add(new ValueSetRowViewModel(parameterValueSet));
                }

                this.DisplayedValueSet = this.AvailableValueSets.FirstOrDefault();

                return;
            }

            var row = new Dialogs.ParameterSubscriptionRowViewModel(this.Thing, this.Session, this, this.IsReadOnly);

            this.ValueSet.Add(row);
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();

            for (var i = 0; i < this.Thing.ValueSet.Count; i++)
            {
                this.Thing.ValueSet[i] = this.Thing.ValueSet[i].Clone(false);
            }

            if (this.Thing.ParameterType is SampledFunctionParameterType)
            {
                foreach (var parameterValueSet in this.Thing.ValueSet.ToList())
                {
                    var gridVm = this.ValueSetGridViewModels.SingleOrDefault(vm => ((Thing)vm.ValueSet).Iid.Equals(parameterValueSet.Iid));

                    gridVm?.UpdateSampledFunctionParameterValueSet(parameterValueSet);
                    this.transaction.CreateOrUpdate(parameterValueSet);
                }
            }
            else
            {
                this.ValueSet.First().UpdateValueSets(this.Thing);

                foreach (var parameterSubscriptionValueSet in this.Thing.ValueSet)
                {
                    this.transaction.CreateOrUpdate(parameterSubscriptionValueSet);
                }
            }
        }

        /// <summary>
        /// Check the validation status of the value rows
        /// </summary>
        private void CheckValueValidation()
        {
            foreach (var valueRow in this.ValueSet)
            {
                valueRow.CheckValues(this.Thing.Scale);
            }
        }
    }
}
