// -------------------------------------------------------------------------------------------------
// <copyright file="DerivedUnitDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2017 RHEA S.A.
// </copyright>
// <summary>
//   This is an auto-generated class. Any manual changes on this file will be overwritten!
// </summary>
// -------------------------------------------------------------------------------------------------

namespace CDP4CommonView
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using CDP4Common;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
	using CDP4Dal.Operations;
    using CDP4Dal.Permission;
    using ReactiveUI;
    using System.Reactive;

    /// <summary>
    /// dialog-view-model class representing a <see cref="DerivedUnit"/>
    /// </summary>
    public partial class DerivedUnitDialogViewModel : MeasurementUnitDialogViewModel<DerivedUnit>
    {
        /// <summary>
        /// Backing field for <see cref="SelectedUnitFactor"/>
        /// </summary>
        private UnitFactorRowViewModel selectedUnitFactor;


        /// <summary>
        /// Initializes a new instance of the <see cref="DerivedUnitDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public DerivedUnitDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DerivedUnitDialogViewModel"/> class
        /// </summary>
        /// <param name="derivedUnit">
        /// The <see cref="DerivedUnit"/> that is the subject of the current view-model. This is the object
        /// that will be either created, or edited.
        /// </param>
        /// <param name="transaction">
        /// The <see cref="ThingTransaction"/> that contains the log of recorded changes.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the current <see cref="Thing"/> is to be added or updated
        /// </param>
        /// <param name="isRoot">
        /// Assert if this <see cref="DialogViewModelBase{T}"/> is the root of all <see cref="DialogViewModelBase{T}"/>
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation this <see cref="DialogViewModelBase{T}"/> performs
        /// </param>
        /// <param name="thingDialogNavigationService">
        /// The <see cref="IThingDialogNavigationService"/> that is used to navigate to a dialog of a specific <see cref="Thing"/>.
        /// </param>
        /// <param name="container">
        /// The <see cref="Thing"/> that contains the created <see cref="Thing"/> in this Dialog
        /// </param>
        /// <param name="chainOfContainers">
        /// The optional chain of containers that contains the <paramref name="container"/> argument
        /// </param>
        public DerivedUnitDialogViewModel(DerivedUnit derivedUnit, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(derivedUnit, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            if(container != null)
            {
                var containerThing = container as ReferenceDataLibrary;
                if(containerThing == null)
                {
                    var errorMessage =
                        string.Format(
                            "The container parameter is of type {0}, it shall be of type ReferenceDataLibrary",
                            container.GetType());
                    throw new ArgumentException(errorMessage);
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the selected <see cref="UnitFactorRowViewModel"/>
        /// </summary>
        public UnitFactorRowViewModel SelectedUnitFactor
        {
            get { return this.selectedUnitFactor; }
            set { this.RaiseAndSetIfChanged(ref this.selectedUnitFactor, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="UnitFactor"/>
        /// </summary>
        public ReactiveList<UnitFactorRowViewModel> UnitFactor { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a UnitFactor
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateUnitFactorCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a UnitFactor
        /// </summary>
        public ReactiveCommand<Unit, Unit> DeleteUnitFactorCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a UnitFactor
        /// </summary>
        public ReactiveCommand<Unit, Unit> EditUnitFactorCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a UnitFactor
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectUnitFactorCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Move-Up <see cref="ICommand"/> to move up the order of a UnitFactor 
        /// </summary>
        public ReactiveCommand<Unit, Unit> MoveUpUnitFactorCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Move-Down <see cref="ICommand"/> to move down the order of a UnitFactor
        /// </summary>
        public ReactiveCommand<Unit, Unit> MoveDownUnitFactorCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            
            var canExecuteCreateUnitFactorCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedUnitFactorCommand = this.WhenAny(vm => vm.SelectedUnitFactor, v => v.Value != null);
            var canExecuteEditSelectedUnitFactorCommand = this.WhenAny(vm => vm.SelectedUnitFactor, v => v.Value != null && !this.IsReadOnly);

            this.CreateUnitFactorCommand = ReactiveCommandCreator.Create(() => this.ExecuteCreateCommand<UnitFactor>(this.PopulateUnitFactor), canExecuteCreateUnitFactorCommand);

            this.DeleteUnitFactorCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedUnitFactorCommand);
            this.DeleteUnitFactorCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedUnitFactor.Thing, this.PopulateUnitFactor));

            this.EditUnitFactorCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedUnitFactorCommand);
            this.EditUnitFactorCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedUnitFactor.Thing, this.PopulateUnitFactor));

            this.InspectUnitFactorCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedUnitFactorCommand);
            this.InspectUnitFactorCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedUnitFactor.Thing));

            this.MoveUpUnitFactorCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedUnitFactorCommand);
            this.MoveUpUnitFactorCommand.Subscribe(_ => this.ExecuteMoveUpCommand(this.UnitFactor, this.SelectedUnitFactor));

            this.MoveDownUnitFactorCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedUnitFactorCommand);
            this.MoveDownUnitFactorCommand.Subscribe(_ => this.ExecuteMoveDownCommand(this.UnitFactor, this.SelectedUnitFactor));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;


            if (!clone.UnitFactor.SortedItems.Values.SequenceEqual(this.UnitFactor.Select(x => x.Thing)))
            {
                var itemCount = this.UnitFactor.Count;
                for (var i = 0; i < itemCount; i++)
                {
                    var item = this.UnitFactor[i].Thing;
                    var currentIndex = clone.UnitFactor.IndexOf(item);

                    if (currentIndex != i)
                    {
                        clone.UnitFactor.Move(currentIndex, i);
                    }
                }
            }
            
        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.UnitFactor = new ReactiveList<UnitFactorRowViewModel>();
            this.PopulatePossibleContainer();
        }

        /// <summary>
        /// Populate the possible containers for a <see cref="DerivedUnit"/>
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
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.PopulateUnitFactor();
        }

        /// <summary>
        /// Populates the <see cref="UnitFactor"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateUnitFactor()
        {
            this.UnitFactor.Clear();
            foreach (UnitFactor thing in this.Thing.UnitFactor.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new UnitFactorRowViewModel(thing, this.Session, this);
                this.UnitFactor.Add(row);
                row.Index = this.Thing.UnitFactor.IndexOf(thing);
            }
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
            foreach(var unitFactor in this.UnitFactor)
            {
                unitFactor.Dispose();
            }
        }
    }
}
