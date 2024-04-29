// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SampledFunctionParameterTypeDialogViewModel.cs" company="Starion Group S.A.">
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
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BasicRdl.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive;

    using BasicRdl.ViewModels.Dialogs.Rows;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4CommonView;

    using CDP4Composition.Attributes;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.Operations;

    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="CompoundParameterTypeDialogViewModel"/> is to provide a dialog view model
    /// for a <see cref="CompoundParameterType"/>
    /// </summary>
    [ThingDialogViewModelExport(ClassKind.SampledFunctionParameterType)]
    public class SampledFunctionParameterTypeDialogViewModel : ParameterTypeDialogViewModel<SampledFunctionParameterType>, IThingDialogViewModel
    {
        /// <summary>
        /// Backing field for <see cref="DegreeOfInterpolation"/>
        /// </summary>
        private int? degreeOfInterpolation;

        /// <summary>
        /// Backing field for <see cref="SelectedIndependentParameterType"/>
        /// </summary>
        private IndependentParameterTypeAssignmentRowViewModel selectedIndependentParameterType;

        /// <summary>
        /// Backing field for <see cref="SelectedDependentParameterType"/>
        /// </summary>
        private DependentParameterTypeAssignmentRowViewModel selectedDependentParameterType;

        /// <summary>
        /// Initializes a new instance of the <see cref="SampledFunctionParameterType"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public SampledFunctionParameterTypeDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SampledFunctionParameterType"/> class.
        /// </summary>
        /// <param name="sampledFunctionParameterType">The <see cref="SampledFunctionParameterType"/></param>
        /// <param name="transaction">The <see cref="ThingTransaction"/> that contains the log of recorded changes</param>
        /// <param name="session">The <see cref="ISession"/> in which the current <see cref="Thing"/> is to be added or updated</param>
        /// <param name="isRoot">Assert if this <see cref="BooleanParameterTypeDialogViewModel"/> is the root of all <see cref="IThingDialogViewModel"/></param>
        /// <param name="dialogKind">The kind of operation this <see cref="BooleanParameterTypeDialogViewModel"/> performs</param>
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService"/> that is used to navigate to a dialog of a specific <see cref="Thing"/></param>
        /// <param name="container">The <see cref="Thing"/> that contains the created <see cref="Thing"/> in this Dialog</param>
        /// <param name="chainOfContainers">The optional chain of containers that contains the <paramref name="container"/> argument</param>
        /// <exception cref="ArgumentException">
        /// The container must be of type <see cref="ReferenceDataLibrary"/>.
        /// </exception>
        public SampledFunctionParameterTypeDialogViewModel(SampledFunctionParameterType sampledFunctionParameterType, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
            : base(sampledFunctionParameterType, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            if (container != null)
            {
                var containerThing = container as ReferenceDataLibrary;
                if (containerThing == null)
                {
                    var errorMessage = $"The sampled function parameter is of type {container.GetType()}, it shall be of type ReferenceDataLibrary";
                    throw new ArgumentException(errorMessage);
                }
            }
        }

        /// <summary>
        /// Gets the possible <see cref="ParameterType"/> to be used for the components
        /// </summary>
        public ReactiveList<ParameterType> PossibleParameterTypes { get; private set; }

        /// <summary>
        /// Gets or sets the DegreeOfInterpolation
        /// </summary>
        public virtual int? DegreeOfInterpolation
        {
            get { return this.degreeOfInterpolation; }
            set { this.RaiseAndSetIfChanged(ref this.degreeOfInterpolation, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="IndependentParameterTypeAssignment"/>
        /// </summary>
        public ReactiveList<IndependentParameterTypeAssignmentRowViewModel> IndependentParameterTypes { get; protected set; }

        /// <summary>
        /// Gets or sets the selected <see cref="IndependentParameterTypeAssignment"/>
        /// </summary>
        public IndependentParameterTypeAssignmentRowViewModel SelectedIndependentParameterType
        {
            get { return this.selectedIndependentParameterType; }
            set { this.RaiseAndSetIfChanged(ref this.selectedIndependentParameterType, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="DependentParameterTypeAssignment"/>
        /// </summary>
        public ReactiveList<DependentParameterTypeAssignmentRowViewModel> DependentParameterTypes { get; protected set; }

        /// <summary>
        /// Gets or sets the selected <see cref="DependentParameterTypeAssignment"/>
        /// </summary>
        public DependentParameterTypeAssignmentRowViewModel SelectedDependentParameterType
        {
            get { return this.selectedDependentParameterType; }
            set { this.RaiseAndSetIfChanged(ref this.selectedDependentParameterType, value); }
        }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a <see cref="IndependentParameterTypeAssignment"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateIndependentParameterTypeCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to delete a <see cref="IndependentParameterTypeAssignment"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> DeleteIndependentParameterTypeCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a <see cref="DependentParameterTypeAssignment"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateDependentParameterTypeCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to delete a <see cref="DependentParameterTypeAssignment"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> DeleteDependentParameterTypeCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to move up a <see cref="IndependentParameterTypeAssignment"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> MoveUpIndependentParameterTypeCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to move down a <see cref="IndependentParameterTypeAssignment"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> MoveDownIndependentParameterTypeCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to move up a <see cref="DependentParameterTypeAssignment"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> MoveUpDependentParameterTypeCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to move down a <see cref="DependentParameterTypeAssignment"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> MoveDownDependentParameterTypeCommand { get; protected set; }

        /// <summary>
        /// Update the <see cref="OkCanExecute"/> property
        /// </summary>
        public void UpdateOkCanExecuteStatus()
        {
            this.UpdateOkCanExecute();
        }

        /// <summary>
        /// Returns true if this is a Create dialog
        /// </summary>
        /// <returns>True if this is create dialog</returns>
        public bool IsCreateDialog()
        {
            return this.dialogKind == ThingDialogKind.Create;
        }

        /// <summary>
        /// Initializes the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.IndependentParameterTypes = new ReactiveList<IndependentParameterTypeAssignmentRowViewModel>();

            this.DependentParameterTypes = new ReactiveList<DependentParameterTypeAssignmentRowViewModel>();

            this.PossibleParameterTypes = new ReactiveList<ParameterType>();
            this.PopulatePossibleContainer();
        }

        /// <summary>
        /// Initializes the commands and listeners
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            var canExecuteCreateParameterTypeAssignmentCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v && this.dialogKind == ThingDialogKind.Create);
            var canExecuteEditSelectedIndependentParameterTypeAssignmentCommand = this.WhenAny(vm => vm.SelectedIndependentParameterType, v => v.Value != null && !this.IsReadOnly && this.dialogKind == ThingDialogKind.Create);
            var canExecuteEditSelectedDependentParameterTypeAssignmentCommand = this.WhenAny(vm => vm.SelectedDependentParameterType, v => v.Value != null && !this.IsReadOnly && this.dialogKind == ThingDialogKind.Create);

            this.CreateIndependentParameterTypeCommand = ReactiveCommandCreator.Create(this.ExecuteCreateIndependentParameterType, canExecuteCreateParameterTypeAssignmentCommand);

            this.DeleteIndependentParameterTypeCommand = ReactiveCommandCreator.Create(this.ExecuteDeleteIndependentParameterType, canExecuteEditSelectedIndependentParameterTypeAssignmentCommand);

            this.CreateDependentParameterTypeCommand = ReactiveCommandCreator.Create(this.ExecuteCreateDependentParameterType, canExecuteCreateParameterTypeAssignmentCommand);

            this.DeleteDependentParameterTypeCommand = ReactiveCommandCreator.Create(this.ExecuteDeleteDependentParameterType, canExecuteEditSelectedDependentParameterTypeAssignmentCommand);

            this.MoveUpIndependentParameterTypeCommand = ReactiveCommandCreator.Create(() => this.ExecuteMoveUpCommand(this.IndependentParameterTypes, this.SelectedIndependentParameterType), canExecuteEditSelectedIndependentParameterTypeAssignmentCommand);

            this.MoveDownIndependentParameterTypeCommand = ReactiveCommandCreator.Create(() => this.ExecuteMoveDownCommand(this.IndependentParameterTypes, this.SelectedIndependentParameterType), canExecuteEditSelectedIndependentParameterTypeAssignmentCommand);

            this.MoveUpDependentParameterTypeCommand = ReactiveCommandCreator.Create(() => this.ExecuteMoveUpCommand(this.DependentParameterTypes, this.SelectedDependentParameterType), canExecuteEditSelectedDependentParameterTypeAssignmentCommand);
            
            this.MoveDownDependentParameterTypeCommand = ReactiveCommandCreator.Create(() => this.ExecuteMoveDownCommand(this.DependentParameterTypes, this.SelectedDependentParameterType), canExecuteEditSelectedDependentParameterTypeAssignmentCommand);

            this.WhenAnyValue(x => x.Container).Subscribe(_ =>
            {
                this.PopulateParameterType();
                this.UpdateOkCanExecute();
            });

            this.Disposables.Add(this.IndependentParameterTypes.CountChanged.Subscribe(_ => this.UpdateOkCanExecute()));
            this.Disposables.Add(this.DependentParameterTypes.CountChanged.Subscribe(_ => this.UpdateOkCanExecute()));
        }

        /// <summary>
        /// Populate the possible containers for a <see cref="CompoundParameterType"/>
        /// </summary>
        private void PopulatePossibleContainer()
        {
            this.PossibleContainer.Clear();

            // When creating a new Rule, it can be contained by any ReferenceDataLibrary that is currently loaded
            if (this.dialogKind == ThingDialogKind.Create)
            {
                this.PossibleContainer.AddRange(this.Session.OpenReferenceDataLibraries.Where(x => this.PermissionService.CanWrite(x)).Select(x => x.Clone(false)));
                this.Container = this.PossibleContainer.FirstOrDefault();
                return;
            }

            // When inspecting an existing Rule, only it's container needs to be added to the PossibleContainer property (it cannot be changed)
            if (this.dialogKind == ThingDialogKind.Inspect)
            {
                this.PossibleContainer.Add(this.Thing.Container);
                this.Container = this.Thing.Container;
                return;
            }

            // When updating a Rule, the possible ReferenceDataLibrary can only be the ReferenceDataLibrary in the current chain of ReferenceDataLibrary of the Rule
            if (this.dialogKind == ThingDialogKind.Update)
            {
                var containerRdl = (ReferenceDataLibrary)this.Container;
                this.PossibleContainer.Add(containerRdl);
                var chainOfRdls = containerRdl.GetRequiredRdls();
                this.PossibleContainer.AddRange(chainOfRdls.Where(x => this.PermissionService.CanWrite(x)).Select(x => x.Clone(false)));
            }
        }

        /// <summary>
        /// Populate the parameter type assignments
        /// </summary>
        protected void PopulateParameterTypeAssignments()
        {
            this.IndependentParameterTypes.Clear();

            foreach (IndependentParameterTypeAssignment thing in this.Thing.IndependentParameterType)
            {
                var rowIndex = this.Thing.IndependentParameterType.IndexOf(thing);

                var interpolation = string.Empty;

                try
                {
                    interpolation = this.Thing.InterpolationPeriod[rowIndex];
                }
                catch (Exception)
                {
                    logger.Warn($"Interpolation period at index {rowIndex} value array is incorrect.");
                }

                var row = new IndependentParameterTypeAssignmentRowViewModel(thing, interpolation ?? string.Empty, this.Session, this);

                row.Index = rowIndex;
                this.IndependentParameterTypes.Add(row);
            }

            this.DependentParameterTypes.Clear();

            foreach (DependentParameterTypeAssignment thing in this.Thing.DependentParameterType)
            {
                var rowIndex = this.Thing.DependentParameterType.IndexOf(thing);

                var row = new DependentParameterTypeAssignmentRowViewModel(thing, this.Session, this);

                row.Index = rowIndex;
                this.DependentParameterTypes.Add(row);
            }
        }

        /// <summary>
        /// Update the transaction
        /// </summary>
        /// <remarks>
        /// Besides the <see cref="CompoundParameterType"/> properties, the <see cref="ParameterTypeComponent"/> are also included here
        /// </remarks>
        protected override void UpdateTransaction()
        {
            foreach (var dependentParameterTypeAssignment in this.DependentParameterTypes)
            {
                // a new component was added in this scope
                if (dependentParameterTypeAssignment.Thing.Iid == Guid.Empty)
                {
                    this.Thing.DependentParameterType.Add(dependentParameterTypeAssignment.Thing);
                    dependentParameterTypeAssignment.Thing.ParameterType = dependentParameterTypeAssignment.ParameterType;
                    dependentParameterTypeAssignment.Thing.MeasurementScale = dependentParameterTypeAssignment.Scale;
                    this.transaction.Create(dependentParameterTypeAssignment.Thing);
                }
                else if (dependentParameterTypeAssignment.Thing.ParameterType != dependentParameterTypeAssignment.ParameterType ||
                         dependentParameterTypeAssignment.Thing.MeasurementScale != dependentParameterTypeAssignment.Scale)
                {
                    var clone = dependentParameterTypeAssignment.Thing.Clone(false);
                    clone.ParameterType = dependentParameterTypeAssignment.ParameterType;
                    clone.MeasurementScale = dependentParameterTypeAssignment.Scale;
                    this.transaction.CreateOrUpdate(clone);
                }
            }

            var deletedDependentParameterTypeAssignments =
                this.Thing.DependentParameterType.Where(x => !this.DependentParameterTypes.Select(y => y.Thing.Iid).Contains(x.Iid)).ToList();
            foreach (var deletedDependentParameterTypeAssignment in deletedDependentParameterTypeAssignments)
            {
                var cloneToDelete = deletedDependentParameterTypeAssignment.Clone(false);
                this.transaction.Delete(cloneToDelete, this.Thing);
                // remove from the ContainerList in the case of an existing thing
            }

            foreach (var independentParameterTypeAssignment in this.IndependentParameterTypes)
            {
                // a new component was added in this scope
                if (independentParameterTypeAssignment.Thing.Iid == Guid.Empty)
                {
                    this.Thing.IndependentParameterType.Add(independentParameterTypeAssignment.Thing);
                    independentParameterTypeAssignment.Thing.ParameterType = independentParameterTypeAssignment.ParameterType;
                    independentParameterTypeAssignment.Thing.MeasurementScale = independentParameterTypeAssignment.Scale;
                    this.transaction.Create(independentParameterTypeAssignment.Thing);
                }
                else if (independentParameterTypeAssignment.Thing.ParameterType != independentParameterTypeAssignment.ParameterType ||
                         independentParameterTypeAssignment.Thing.MeasurementScale != independentParameterTypeAssignment.Scale)
                {
                    var clone = independentParameterTypeAssignment.Thing.Clone(false);
                    clone.ParameterType = independentParameterTypeAssignment.ParameterType;
                    clone.MeasurementScale = independentParameterTypeAssignment.Scale;
                    this.transaction.CreateOrUpdate(clone);
                }
            }

            var deletedIndependentParameterTypeAssignments =
                this.Thing.IndependentParameterType.Where(x => !this.IndependentParameterTypes.Select(y => y.Thing.Iid).Contains(x.Iid)).ToList();
            foreach (var deletedIndependentParameterTypeAssignment in deletedIndependentParameterTypeAssignments)
            {
                var cloneToDelete = deletedIndependentParameterTypeAssignment.Clone(false);
                this.transaction.Delete(cloneToDelete, this.Thing);
                // remove from the ContainerList in the case of an existing thing
            }

            this.Thing.InterpolationPeriod = new ValueArray<string>(this.IndependentParameterTypes.Select(x => x.InterpolationPeriod ?? string.Empty).ToList());
            this.Thing.DegreeOfInterpolation = this.DegreeOfInterpolation;

            // this is called at the end so that any component order change may be taken into account after it was added/removed
            base.UpdateTransaction();
        }

        /// <summary>
        /// Update the <see cref="OkCanExecute"/> property
        /// </summary>
        protected override void UpdateOkCanExecute()
        {
            base.UpdateOkCanExecute();
            var rdl = (ReferenceDataLibrary)this.Container;
            var rdls = new List<ReferenceDataLibrary>(rdl.GetRequiredRdls()) { rdl };

            var parametertypes = rdls.SelectMany(x => x.ParameterType)
                .OfType<ScalarParameterType>()
                .OrderBy(x => x.Name).ToList();

            var isAllIndependentParameterTypeAssignmentsInRdl = this.IndependentParameterTypes.Select(x => x.ParameterType).All(parametertypes.Contains);
            var isAllIndependentParameterTypeAssignmentsValidated = this.IndependentParameterTypes.All(x => x.ParameterType != null && (!(x.ParameterType is QuantityKind) || x.Scale != null) && !x.HasError);

            var isAllDependentParameterTypeAssignmentsInRdl = this.DependentParameterTypes.Select(x => x.ParameterType).All(parametertypes.Contains);
            var isAllDependentParameterTypeAssignmentsValidated = this.DependentParameterTypes.All(x => x.ParameterType != null && (!(x.ParameterType is QuantityKind) || x.Scale != null) && !x.HasError);

            this.OkCanExecute = this.OkCanExecute &&
                                this.IndependentParameterTypes.Count > 0 &&
                                this.DependentParameterTypes.Count > 0 &&
                                isAllIndependentParameterTypeAssignmentsInRdl &&
                                isAllIndependentParameterTypeAssignmentsValidated &&
                                isAllDependentParameterTypeAssignmentsInRdl &&
                                isAllDependentParameterTypeAssignmentsValidated;
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.DegreeOfInterpolation = this.Thing.DegreeOfInterpolation;

            this.PopulateParameterTypeAssignments();
        }

        /// <summary>
        /// Populate the possible <see cref="ScalarParameterType"/> to be used as <see cref="ParameterTypeComponent"/>
        /// </summary>
        private void PopulateParameterType()
        {
            this.PossibleParameterTypes.Clear();
            var rdl = (ReferenceDataLibrary)this.Container;
            var rdls = new List<ReferenceDataLibrary>(rdl.GetRequiredRdls()) {rdl};

            var parametertypes = rdls.SelectMany(x => x.ParameterType)
                .OfType<ScalarParameterType>()
                .OrderBy(x => x.Name).ToList();

            this.PossibleParameterTypes.AddRange(parametertypes);
        }

        /// <summary>
        /// Execute the <see cref="CreateIndependentParameterTypeCommand"/>
        /// </summary>
        private void ExecuteCreateIndependentParameterType()
        {
            var assignment = new IndependentParameterTypeAssignment();

            var row = new IndependentParameterTypeAssignmentRowViewModel(assignment, string.Empty, this.Session, this);

            this.IndependentParameterTypes.Add(row);
        }

        /// <summary>
        /// Execute the <see cref="DeleteIndependentParameterTypeCommand"/>
        /// </summary>
        private void ExecuteDeleteIndependentParameterType()
        {
            this.IndependentParameterTypes.Remove(this.SelectedIndependentParameterType);
        }

        /// <summary>
        /// Execute the <see cref="CreateDependentParameterTypeCommand"/>
        /// </summary>
        private void ExecuteCreateDependentParameterType()
        {
            var assignment = new DependentParameterTypeAssignment();

            var row = new DependentParameterTypeAssignmentRowViewModel(assignment, this.Session, this);

            this.DependentParameterTypes.Add(row);
        }

        /// <summary>
        /// Execute the <see cref="DeleteDependentParameterTypeCommand"/>
        /// </summary>
        private void ExecuteDeleteDependentParameterType()
        {
            this.DependentParameterTypes.Remove(this.SelectedDependentParameterType);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// a value indicating whether the class is being disposed of
        /// </param>
        /// <remarks>
        /// This method is called by the <see cref="ThingDialogNavigationService"/> when the Dialog is closed
        /// </remarks>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            foreach (var pta in this.IndependentParameterTypes)
            {
                pta.Dispose();
            }

            foreach (var pta in this.DependentParameterTypes)
            {
                pta.Dispose();
            }
        }
    }
}
