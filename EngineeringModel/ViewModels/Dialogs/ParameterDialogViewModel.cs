// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterDialogViewModel.cs" company="RHEA System S.A.">
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
    /// The dialog-view model to create, edit or inspect a <see cref="Parameter" />
    /// </summary>
    [ThingDialogViewModelExport(ClassKind.Parameter)]
    public class ParameterDialogViewModel : CDP4CommonView.ParameterDialogViewModel, IThingDialogViewModel
    {
        /// <summary>
        /// Backing field for <see cref="AvailableValueSets" />
        /// </summary>
        private ReactiveList<ValueSetRowViewModel> availableValueSets;

        /// <summary>
        /// Backing field for <see cref="DisplayedValueSet" />
        /// </summary>
        private ValueSetRowViewModel displayedValueSet;

        /// <summary>
        /// Backing field for <see cref="IsStateDependent" />
        /// </summary>
        private bool isStateDependent;

        /// <summary>
        /// Backing field for <see cref="IsValueSetEditable" />
        /// </summary>
        private bool isValueSetEditable;

        /// <summary>
        /// Backing field for the <see cref="ModelCode" /> property.
        /// </summary>
        private string modelCode;

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
        /// Initializes a new instance of the <see cref="ParameterDialogViewModel" /> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public ParameterDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterDialogViewModel" /> class
        /// </summary>
        /// <param name="parameter">
        /// The parameter.
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
        public ParameterDialogViewModel(Parameter parameter, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
            : base(parameter, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            this.WhenAnyValue(vm => vm.SelectedOwner).Subscribe(_ => this.UpdateOkCanExecute());
            this.WhenAnyValue(vm => vm.SelectedScale).Subscribe(_ => this.UpdateOkCanExecute());
            this.WhenAnyValue(vm => vm.IsOptionDependent).Subscribe(_ => this.IsValueSetEditable = this.IsOptionDependent == this.Thing.IsOptionDependent && this.SelectedStateDependence == this.Thing.StateDependence);
            this.WhenAnyValue(vm => vm.SelectedStateDependence).Subscribe(_ => this.IsValueSetEditable = this.IsOptionDependent == this.Thing.IsOptionDependent && this.SelectedStateDependence == this.Thing.StateDependence);
            this.WhenAnyValue(vm => vm.SelectedScale).Where(x => x != null).Subscribe(_ => this.CheckValueValidation());
            this.WhenAnyValue(vm => vm.SelectedGroupSelection).Subscribe(x => this.SelectedGroup = x != null ? x.Thing : null);
            this.WhenAnyValue(vm => vm.DisplayedValueSet).Where(x => x != null).Subscribe(_ => this.LoadValueSetGrid());

            this.IsNameVisible = this.Thing.ParameterType is CompoundParameterType || this.Thing.IsOptionDependent || this.Thing.StateDependence != null;
        }

        /// <summary>
        /// Gets a value indicating if the scale shall be made visible
        /// </summary>
        public bool IsScaleVisible
        {
            get { return this.Thing.ParameterType is CompoundParameterType; }
        }

        /// <summary>
        /// Gets a value indicating whether is owner visible.
        /// </summary>
        public bool IsOwnerVisible
        {
            get { return false; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether is name visible.
        /// </summary>
        public bool IsNameVisible { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the value set may be edited
        /// </summary>
        /// <remarks>
        /// The value shall be set to false when the option-dependency or state-dependency value is changed
        /// </remarks>
        public bool IsValueSetEditable
        {
            get { return this.isValueSetEditable; }
            private set { this.RaiseAndSetIfChanged(ref this.isValueSetEditable, value); }
        }

        /// <summary>
        /// Gets or sets the selected <see cref="GroupSelectionViewModel" />
        /// </summary>
        public GroupSelectionViewModel SelectedGroupSelection
        {
            get { return this.selectedGroupSelection; }
            set { this.RaiseAndSetIfChanged(ref this.selectedGroupSelection, value); }
        }

        /// <summary>
        /// Gets or sets the selected <see cref="ParameterValueSetRowViewModel" />
        /// </summary>
        public ParameterRowViewModel SelectedValueSet
        {
            get { return this.selectedValueSet; }
            set { this.RaiseAndSetIfChanged(ref this.selectedValueSet, value); }
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
        /// Gets or sets the list of <see cref="ParameterValueSet" />
        /// </summary>
        public DisposableReactiveList<Dialogs.ParameterRowViewModel> ValueSet { get; protected set; }

        /// <summary>
        /// Gets the possible groups
        /// </summary>
        public ReactiveList<GroupSelectionViewModel> PossibleGroups { get; private set; }

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
        /// Gets or sets the Inspect <see cref="ICommand" /> to inspect a ParameterValueSet
        /// </summary>
        public ReactiveCommand<object> InspectStateDependenceCommand { get; protected set; }

        /// <summary>
        /// Gets or sets a value indicating whether the parameter is state dependent.
        /// </summary>
        public bool IsStateDependent
        {
            get { return this.isStateDependent; }
            set { this.RaiseAndSetIfChanged(ref this.isStateDependent, value); }
        }

        /// <summary>
        /// Gets a value indicating whether the ParameterType property is ReadOnly.
        /// </summary>
        public bool IsParameterTypeReadOnly
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating whether the ParameterType is SampledFunctionParameter.
        /// </summary>
        public bool IsSampledFunctionParameter
        {
            get { return this.Thing.ParameterType is SampledFunctionParameterType; }
        }

        /// <summary>
        /// Gets or sets a value that represents the ModelCode of the current <see cref="ElementDefinition" />
        /// </summary>
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
            this.ValueSet = new DisposableReactiveList<Dialogs.ParameterRowViewModel>();
            this.PossibleGroups = new ReactiveList<GroupSelectionViewModel>();

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

            var canExecuteInspectStateDependenceCommand = this.WhenAny(vm => vm.SelectedStateDependence, s => s.Value != null);
            this.InspectStateDependenceCommand = ReactiveCommand.Create(canExecuteInspectStateDependenceCommand);
            this.InspectStateDependenceCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedStateDependence));
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();

            this.ModelCode = this.Thing.ModelCode();
            this.IsOptionDependent = this.Thing.IsOptionDependent;
            this.IsStateDependent = this.Thing.StateDependence != null;
            this.SelectedParameterType = this.Thing.ParameterType;
            this.PopulatePossibleParameterType();
            this.SelectedScale = this.Thing.Scale;
            this.PopulatePossibleScale();
            this.SelectedStateDependence = this.Thing.StateDependence;
            this.PopulatePossibleStateDependence();
            this.PopulatePossibleGroup();
            this.PopulateValueSet();
        }

        /// <summary>
        /// Updates the <see cref="OkCanExecute" /> property using validation rules
        /// </summary>
        protected override void UpdateOkCanExecute()
        {
            base.UpdateOkCanExecute();

            this.OkCanExecute = this.OkCanExecute && this.SelectedOwner != null && this.IsSelectedScaleValid();
        }

        /// <summary>
        /// verifies whether the selected scale is valid
        /// </summary>
        /// <returns>
        /// true of valid, false if not
        /// </returns>
        /// <remarks>
        /// When the SelectedParameterType is a QuantityKind then the SelectedScale may not be null.
        /// If the SelectedParameterType is not a QuantityKind, then the SelectedScale must be null.
        /// </remarks>
        private bool IsSelectedScaleValid()
        {
            var quantityKind = this.SelectedParameterType as QuantityKind;

            if (quantityKind != null && this.SelectedScale == null)
            {
                return false;
            }

            if (quantityKind == null && this.SelectedScale != null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Populates the <see cref="PossibleRequestedBy" /> property
        /// </summary>
        protected override void PopulatePossibleRequestedBy()
        {
            base.PopulatePossibleRequestedBy();

            var model = this.Thing.TopContainer as EngineeringModel;

            if (model == null)
            {
                return;
            }

            this.PossibleRequestedBy.AddRange(model.EngineeringModelSetup.ActiveDomain.Except(new[] { this.SelectedOwner }));
        }

        /// <summary>
        /// Populates the <see cref="PossibleParameterType" /> property
        /// </summary>
        protected override void PopulatePossibleParameterType()
        {
            base.PopulatePossibleParameterType();
            var model = this.Thing.TopContainer as EngineeringModel;

            if (model == null)
            {
                return;
            }

            var modelRdl = model.EngineeringModelSetup.RequiredRdl.Single();
            var parametertypes = modelRdl.ParameterType.ToList();

            if (modelRdl.RequiredRdl != null)
            {
                parametertypes.AddRange(modelRdl.RequiredRdl.ParameterType.Except(parametertypes));
            }

            this.PossibleParameterType.AddRange(parametertypes.OrderBy(p => p.ShortName));

            if (this.SelectedParameterType == null)
            {
                this.SelectedParameterType = this.PossibleParameterType.FirstOrDefault();
            }
        }

        /// <summary>
        /// Populates the <see cref="PossibleScale" /> property
        /// </summary>
        protected override void PopulatePossibleScale()
        {
            base.PopulatePossibleScale();
            var quantityKind = this.SelectedParameterType as QuantityKind;

            if (quantityKind == null)
            {
                return;
            }

            var model = this.Thing.TopContainer as EngineeringModel;

            if (model == null)
            {
                return;
            }

            this.PossibleScale.AddRange(quantityKind.AllPossibleScale.OrderBy(p => p.ShortName));
        }

        /// <summary>
        /// Populates the <see cref="PossibleStateDependence" /> property
        /// </summary>
        protected override void PopulatePossibleStateDependence()
        {
            base.PopulatePossibleStateDependence();
            var iteration = this.Thing.Container.Container as Iteration;

            if (iteration == null)
            {
                return;
            }

            this.PossibleStateDependence.AddRange(iteration.ActualFiniteStateList);
        }

        /// <summary>
        /// Populates the <see cref="PossibleGroup" /> property
        /// </summary>
        protected override void PopulatePossibleGroup()
        {
            this.PossibleGroups.Clear();

            var elementDefinition = this.Thing.Container as ElementDefinition;

            if (elementDefinition == null)
            {
                return;
            }

            this.PossibleGroups.AddRange(elementDefinition.ParameterGroup.Select(x => new GroupSelectionViewModel(x)));
            this.SelectedGroupSelection = this.PossibleGroups.SingleOrDefault(x => x.Thing == this.Thing.Group);
        }

        /// <summary>
        /// Populates the <see cref="PossibleOwner" /> property
        /// </summary>
        protected override void PopulatePossibleOwner()
        {
            base.PopulatePossibleOwner();
            var model = this.Thing.TopContainer as EngineeringModel;

            if (model == null)
            {
                return;
            }

            this.PossibleOwner.AddRange(model.EngineeringModelSetup.ActiveDomain.OrderBy(x => x.Name));

            if (this.SelectedOwner == null)
            {
                Tuple<DomainOfExpertise, Participant> tuple;
                this.Session.OpenIterations.TryGetValue(this.Thing.GetContainerOfType<Iteration>(), out tuple);
                this.SelectedOwner = tuple.Item1;
            }
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();

            this.Thing.Group = this.SelectedGroup;
            this.Thing.StateDependence = this.SelectedStateDependence;
            this.Thing.Scale = this.SelectedScale;
            this.Thing.ParameterType = this.SelectedParameterType;
            this.Thing.IsOptionDependent = this.IsOptionDependent;

            if (!this.IsValueSetEditable)
            {
                // no operation shall be done on the value sets
                return;
            }

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
                this.ValueSet.First().UpdateParameterValueSet(this.Thing);

                foreach (var parameterValueSet in this.Thing.ValueSet)
                {
                    this.transaction.CreateOrUpdate(parameterValueSet);
                }
            }
        }

        /// <summary>
        /// Populates the <see cref="ValueSet" /> property with the content of the actual thing and the content of the transaction
        /// </summary>
        private void PopulateValueSet()
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

            var row = new Dialogs.ParameterRowViewModel(this.Thing, this.Session, this, this.IsReadOnly);

            this.ValueSet.Add(row);
        }

        /// <summary>
        /// Check the validation status of the value rows
        /// </summary>
        private void CheckValueValidation()
        {
            foreach (var parameterRowViewModel in this.ValueSet)
            {
                parameterRowViewModel.CheckValues(this.SelectedScale);
            }
        }
    }
}
