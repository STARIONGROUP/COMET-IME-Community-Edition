﻿// -------------------------------------------------------------------------------------------------
// <copyright file="LogarithmicScaleDialogViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2017 Starion Group S.A.
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
    
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
	using CDP4Dal.Operations;
    using CDP4Dal.Permission;
    using ReactiveUI;
    using System.Reactive;

    /// <summary>
    /// dialog-view-model class representing a <see cref="LogarithmicScale"/>
    /// </summary>
    public partial class LogarithmicScaleDialogViewModel : MeasurementScaleDialogViewModel<LogarithmicScale>
    {
        /// <summary>
        /// Backing field for <see cref="LogarithmBase"/>
        /// </summary>
        private LogarithmBaseKind logarithmBase;

        /// <summary>
        /// Backing field for <see cref="Factor"/>
        /// </summary>
        private string factor;

        /// <summary>
        /// Backing field for <see cref="Exponent"/>
        /// </summary>
        private string exponent;

        /// <summary>
        /// Backing field for <see cref="SelectedReferenceQuantityKind"/>
        /// </summary>
        private QuantityKind selectedReferenceQuantityKind;

        /// <summary>
        /// Backing field for <see cref="SelectedReferenceQuantityValue"/>
        /// </summary>
        private ScaleReferenceQuantityValueRowViewModel selectedReferenceQuantityValue;


        /// <summary>
        /// Initializes a new instance of the <see cref="LogarithmicScaleDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public LogarithmicScaleDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogarithmicScaleDialogViewModel"/> class
        /// </summary>
        /// <param name="logarithmicScale">
        /// The <see cref="LogarithmicScale"/> that is the subject of the current view-model. This is the object
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
        public LogarithmicScaleDialogViewModel(LogarithmicScale logarithmicScale, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(logarithmicScale, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
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
        /// Gets or sets the LogarithmBase
        /// </summary>
        public virtual LogarithmBaseKind LogarithmBase
        {
            get { return this.logarithmBase; }
            set { this.RaiseAndSetIfChanged(ref this.logarithmBase, value); }
        }

        /// <summary>
        /// Gets or sets the Factor
        /// </summary>
        public virtual string Factor
        {
            get { return this.factor; }
            set { this.RaiseAndSetIfChanged(ref this.factor, value); }
        }

        /// <summary>
        /// Gets or sets the Exponent
        /// </summary>
        public virtual string Exponent
        {
            get { return this.exponent; }
            set { this.RaiseAndSetIfChanged(ref this.exponent, value); }
        }

        /// <summary>
        /// Gets or sets the SelectedReferenceQuantityKind
        /// </summary>
        public virtual QuantityKind SelectedReferenceQuantityKind
        {
            get { return this.selectedReferenceQuantityKind; }
            set { this.RaiseAndSetIfChanged(ref this.selectedReferenceQuantityKind, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="QuantityKind"/>s for <see cref="SelectedReferenceQuantityKind"/>
        /// </summary>
        public ReactiveList<QuantityKind> PossibleReferenceQuantityKind { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="ScaleReferenceQuantityValueRowViewModel"/>
        /// </summary>
        public ScaleReferenceQuantityValueRowViewModel SelectedReferenceQuantityValue
        {
            get { return this.selectedReferenceQuantityValue; }
            set { this.RaiseAndSetIfChanged(ref this.selectedReferenceQuantityValue, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="ScaleReferenceQuantityValue"/>
        /// </summary>
        public ReactiveList<ScaleReferenceQuantityValueRowViewModel> ReferenceQuantityValue { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedReferenceQuantityKind"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectSelectedReferenceQuantityKindCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a ScaleReferenceQuantityValue
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateReferenceQuantityValueCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a ScaleReferenceQuantityValue
        /// </summary>
        public ReactiveCommand<Unit, Unit> DeleteReferenceQuantityValueCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a ScaleReferenceQuantityValue
        /// </summary>
        public ReactiveCommand<Unit, Unit> EditReferenceQuantityValueCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a ScaleReferenceQuantityValue
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectReferenceQuantityValueCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            
            var canExecuteCreateReferenceQuantityValueCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedReferenceQuantityValueCommand = this.WhenAny(vm => vm.SelectedReferenceQuantityValue, v => v.Value != null);
            var canExecuteEditSelectedReferenceQuantityValueCommand = this.WhenAny(vm => vm.SelectedReferenceQuantityValue, v => v.Value != null && !this.IsReadOnly);

            this.CreateReferenceQuantityValueCommand = ReactiveCommandCreator.Create(canExecuteCreateReferenceQuantityValueCommand);
            this.CreateReferenceQuantityValueCommand.Subscribe(_ => this.ExecuteCreateCommand<ScaleReferenceQuantityValue>(this.PopulateReferenceQuantityValue));

            this.DeleteReferenceQuantityValueCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedReferenceQuantityValueCommand);
            this.DeleteReferenceQuantityValueCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedReferenceQuantityValue.Thing, this.PopulateReferenceQuantityValue));

            this.EditReferenceQuantityValueCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedReferenceQuantityValueCommand);
            this.EditReferenceQuantityValueCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedReferenceQuantityValue.Thing, this.PopulateReferenceQuantityValue));

            this.InspectReferenceQuantityValueCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedReferenceQuantityValueCommand);
            this.InspectReferenceQuantityValueCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedReferenceQuantityValue.Thing));
            var canExecuteInspectSelectedReferenceQuantityKindCommand = this.WhenAny(vm => vm.SelectedReferenceQuantityKind, v => v.Value != null);
            this.InspectSelectedReferenceQuantityKindCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedReferenceQuantityKindCommand);
            this.InspectSelectedReferenceQuantityKindCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedReferenceQuantityKind));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.LogarithmBase = this.LogarithmBase;
            clone.Factor = this.Factor;
            clone.Exponent = this.Exponent;
            clone.ReferenceQuantityKind = this.SelectedReferenceQuantityKind;
        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PossibleReferenceQuantityKind = new ReactiveList<QuantityKind>();
            this.ReferenceQuantityValue = new ReactiveList<ScaleReferenceQuantityValueRowViewModel>();
            this.PopulatePossibleContainer();
        }

        /// <summary>
        /// Populate the possible containers for a <see cref="LogarithmicScale"/>
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
            this.LogarithmBase = this.Thing.LogarithmBase;
            this.Factor = this.Thing.Factor;
            this.Exponent = this.Thing.Exponent;
            this.SelectedReferenceQuantityKind = this.Thing.ReferenceQuantityKind;
            this.PopulatePossibleReferenceQuantityKind();
            this.PopulateReferenceQuantityValue();
        }

        /// <summary>
        /// Populates the <see cref="ReferenceQuantityValue"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateReferenceQuantityValue()
        {
            this.ReferenceQuantityValue.Clear();
            foreach (var thing in this.Thing.ReferenceQuantityValue.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new ScaleReferenceQuantityValueRowViewModel(thing, this.Session, this);
                this.ReferenceQuantityValue.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="PossibleReferenceQuantityKind"/> property
        /// </summary>
        protected virtual void PopulatePossibleReferenceQuantityKind()
        {
            this.PossibleReferenceQuantityKind.Clear();
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
            foreach(var referenceQuantityValue in this.ReferenceQuantityValue)
            {
                referenceQuantityValue.Dispose();
            }
        }
    }
}
