// -------------------------------------------------------------------------------------------------
// <copyright file="DerivedQuantityKindDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2017 RHEA System S.A.
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
    using CDP4Common.Operations;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Permission;
    using ReactiveUI;

    /// <summary>
    /// dialog-view-model class representing a <see cref="DerivedQuantityKind"/>
    /// </summary>
    public partial class DerivedQuantityKindDialogViewModel : QuantityKindDialogViewModel<DerivedQuantityKind>
    {
        /// <summary>
        /// Backing field for <see cref="SelectedQuantityKindFactor"/>
        /// </summary>
        private QuantityKindFactorRowViewModel selectedQuantityKindFactor;


        /// <summary>
        /// Initializes a new instance of the <see cref="DerivedQuantityKindDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public DerivedQuantityKindDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DerivedQuantityKindDialogViewModel"/> class
        /// </summary>
        /// <param name="derivedQuantityKind">
        /// The <see cref="DerivedQuantityKind"/> that is the subject of the current view-model. This is the object
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
        public DerivedQuantityKindDialogViewModel(DerivedQuantityKind derivedQuantityKind, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(derivedQuantityKind, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
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
        /// Gets or sets the selected <see cref="QuantityKindFactorRowViewModel"/>
        /// </summary>
        public QuantityKindFactorRowViewModel SelectedQuantityKindFactor
        {
            get { return this.selectedQuantityKindFactor; }
            set { this.RaiseAndSetIfChanged(ref this.selectedQuantityKindFactor, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="QuantityKindFactor"/>
        /// </summary>
        public ReactiveList<QuantityKindFactorRowViewModel> QuantityKindFactor { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a QuantityKindFactor
        /// </summary>
        public ReactiveCommand<object> CreateQuantityKindFactorCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a QuantityKindFactor
        /// </summary>
        public ReactiveCommand<object> DeleteQuantityKindFactorCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a QuantityKindFactor
        /// </summary>
        public ReactiveCommand<object> EditQuantityKindFactorCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a QuantityKindFactor
        /// </summary>
        public ReactiveCommand<object> InspectQuantityKindFactorCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Move-Up <see cref="ICommand"/> to move up the order of a QuantityKindFactor 
        /// </summary>
        public ReactiveCommand<object> MoveUpQuantityKindFactorCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Move-Down <see cref="ICommand"/> to move down the order of a QuantityKindFactor
        /// </summary>
        public ReactiveCommand<object> MoveDownQuantityKindFactorCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            
            var canExecuteCreateQuantityKindFactorCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedQuantityKindFactorCommand = this.WhenAny(vm => vm.SelectedQuantityKindFactor, v => v.Value != null);
            var canExecuteEditSelectedQuantityKindFactorCommand = this.WhenAny(vm => vm.SelectedQuantityKindFactor, v => v.Value != null && !this.IsReadOnly);

            this.CreateQuantityKindFactorCommand = ReactiveCommand.Create(canExecuteCreateQuantityKindFactorCommand);
            this.CreateQuantityKindFactorCommand.Subscribe(_ => this.ExecuteCreateCommand<QuantityKindFactor>(this.PopulateQuantityKindFactor));

            this.DeleteQuantityKindFactorCommand = ReactiveCommand.Create(canExecuteEditSelectedQuantityKindFactorCommand);
            this.DeleteQuantityKindFactorCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedQuantityKindFactor.Thing, this.PopulateQuantityKindFactor));

            this.EditQuantityKindFactorCommand = ReactiveCommand.Create(canExecuteEditSelectedQuantityKindFactorCommand);
            this.EditQuantityKindFactorCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedQuantityKindFactor.Thing, this.PopulateQuantityKindFactor));

            this.InspectQuantityKindFactorCommand = ReactiveCommand.Create(canExecuteInspectSelectedQuantityKindFactorCommand);
            this.InspectQuantityKindFactorCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedQuantityKindFactor.Thing));

            this.MoveUpQuantityKindFactorCommand = ReactiveCommand.Create(canExecuteEditSelectedQuantityKindFactorCommand);
            this.MoveUpQuantityKindFactorCommand.Subscribe(_ => this.ExecuteMoveUpCommand(this.QuantityKindFactor, this.SelectedQuantityKindFactor));

            this.MoveDownQuantityKindFactorCommand = ReactiveCommand.Create(canExecuteEditSelectedQuantityKindFactorCommand);
            this.MoveDownQuantityKindFactorCommand.Subscribe(_ => this.ExecuteMoveDownCommand(this.QuantityKindFactor, this.SelectedQuantityKindFactor));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;


            if (!clone.QuantityKindFactor.SortedItems.Values.SequenceEqual(this.QuantityKindFactor.Select(x => x.Thing)))
            {
                var itemCount = this.QuantityKindFactor.Count;
                for (var i = 0; i < itemCount; i++)
                {
                    var item = this.QuantityKindFactor[i].Thing;
                    var currentIndex = clone.QuantityKindFactor.IndexOf(item);

                    if (currentIndex != i)
                    {
                        clone.QuantityKindFactor.Move(currentIndex, i);
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
            this.QuantityKindFactor = new ReactiveList<QuantityKindFactorRowViewModel>();
            this.PopulatePossibleContainer();
        }

        /// <summary>
        /// Populate the possible containers for a <see cref="DerivedQuantityKind"/>
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
            this.PopulateQuantityKindFactor();
        }

        /// <summary>
        /// Populates the <see cref="QuantityKindFactor"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateQuantityKindFactor()
        {
            this.QuantityKindFactor.Clear();
            foreach (QuantityKindFactor thing in this.Thing.QuantityKindFactor.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new QuantityKindFactorRowViewModel(thing, this.Session, this);
                this.QuantityKindFactor.Add(row);
                row.Index = this.Thing.QuantityKindFactor.IndexOf(thing);
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
            foreach(var quantityKindFactor in this.QuantityKindFactor)
            {
                quantityKindFactor.Dispose();
            }
        }
    }
}
