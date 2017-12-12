// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LogarithmicScaleDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BasicRdl.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common;
    using CDP4Common.CommonData;
    using CDP4Dal.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="LogarithmicScaleDialogViewModel"/> is to provide a dialog view model
    /// for a <see cref="LogarithmicScale"/>
    /// </summary>
    [ThingDialogViewModelExport(ClassKind.LogarithmicScale)]
    public class LogarithmicScaleDialogViewModel : CDP4CommonView.LogarithmicScaleDialogViewModel, IThingDialogViewModel
    {
        #region Fields
        /// <summary>
        /// The backing field for <see cref="ShortName"/>
        /// </summary>
        private string shortName;

        /// <summary>
        /// The backing field for <see cref="Name"/>
        /// </summary>
        private string name;
        #endregion

        #region constructors
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
        /// Initializes a new instance of the <see cref="LogarithmicScaleDialogViewModel"/> class.
        /// </summary>
        /// <param name="logarithmicScale">
        /// The Simple quantity Kind.
        /// </param>
        /// <param name="transaction">
        /// The <see cref="ThingTransaction"/> that contains the log of recorded changes.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the current <see cref="Thing"/> is to be added or updated
        /// </param>
        /// <param name="isRoot">
        /// Assert if this <see cref="LogarithmicScaleDialogViewModel"/> is the root of all <see cref="IThingDialogViewModel"/>
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation this <see cref="LogarithmicScaleDialogViewModel"/> performs
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
        /// <exception cref="ArgumentException">
        /// The container must be of type <see cref="ReferenceDataLibrary"/>.
        /// </exception>
        public LogarithmicScaleDialogViewModel(LogarithmicScale logarithmicScale, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
            : base(logarithmicScale, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            this.WhenAnyValue(vm => vm.Container).Subscribe(_ => 
                {
                    this.PopulatePossibleUnit();
                    this.PopulatePossibleReferenceQuantityKind();
                });

            this.WhenAnyValue(vm => vm.SelectedUnit).Subscribe(_ => this.UpdateOkCanExecute());
        }
        #endregion

        /// <summary>
        /// Updates the <see cref="LogarithmicScaleDialogViewModel.OkCanExecute"/> property using validation rules
        /// </summary>
        protected override void UpdateOkCanExecute()
        {
            base.UpdateOkCanExecute();
            this.OkCanExecute = this.OkCanExecute && this.SelectedUnit != null;
        }

        /// <summary>
        /// Populates the <see cref="LogarithmicScaleDialogViewModel.PossibleUnit"/> property
        /// </summary>
        protected override void PopulatePossibleUnit()
        {
            base.PopulatePossibleUnit();
            var containerRdl = this.Container as ReferenceDataLibrary;
            if (containerRdl != null)
            {
                var allUnits = new List<MeasurementUnit>(containerRdl.Unit);
                allUnits.AddRange(containerRdl.GetRequiredRdls().SelectMany(rdl => rdl.Unit));
                this.PossibleUnit.AddRange(allUnits.OrderBy(c => c.ShortName));
            }
        }

        /// <summary>
        /// Initializes the <see cref="ReactiveCommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            var canExecuteCreateMappingToReferenceScaleCommand = this.WhenAny(vm => vm.ValueDefinition.Count, v => v.Value > 0 && !this.IsReadOnly);
            this.CreateMappingToReferenceScaleCommand = ReactiveCommand.Create(canExecuteCreateMappingToReferenceScaleCommand);
            this.CreateMappingToReferenceScaleCommand.Subscribe(_ => this.ExecuteCreateCommand<MappingToReferenceScale>(this.PopulateMappingToReferenceScale));

            var canExecuteCreateReferenceQuantityValueCommand = this.WhenAny(vm => vm.ReferenceQuantityValue.Count, v => v.Value == 0 && !this.IsReadOnly);
            this.CreateReferenceQuantityValueCommand = ReactiveCommand.Create(canExecuteCreateReferenceQuantityValueCommand);
            this.CreateReferenceQuantityValueCommand.Subscribe(_ => this.ExecuteCreateCommand<ScaleReferenceQuantityValue>(this.PopulateReferenceQuantityValue));       
        }

        /// <summary>
        /// Populates the <see cref="LogarithmicScaleDialogViewModel.PossibleReferenceQuantityKind"/> property
        /// </summary>
        protected override void PopulatePossibleReferenceQuantityKind()
        {
            base.PopulatePossibleReferenceQuantityKind();
            var containerRdl = this.Container as ReferenceDataLibrary;
            if (containerRdl != null)
            {
                var quantityKinds = containerRdl.ParameterType.OfType<QuantityKind>().ToList();
                quantityKinds.AddRange(containerRdl.GetRequiredRdls().SelectMany(rdl => rdl.ParameterType.OfType<QuantityKind>()));
                this.PossibleReferenceQuantityKind.AddRange(quantityKinds.OrderBy(c => c.Name));
            }
        }

        /// <summary>
        /// Populates the <see cref="LogarithmicScaleDialogViewModel.ReferenceQuantityValue"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected override void PopulateReferenceQuantityValue()
        {
            this.ReferenceQuantityValue.Clear();
            foreach (var thing in this.Thing.ReferenceQuantityValue.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new ScaleReferenceQuantityValueRowViewModel(thing, this.Session, this);
                this.ReferenceQuantityValue.Add(row);
            }
        }

        /// <summary>
        /// Gets or sets the ShortName
        /// </summary>
        [ValidationOverride(true, "RDLShortName")]
        public override string ShortName
        {
            get { return this.shortName; }
            set { this.RaiseAndSetIfChanged(ref this.shortName, value); }
        }

        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        [ValidationOverride(true, "RDLName")]
        public override string Name
        {
            get { return this.name; }
            set { this.RaiseAndSetIfChanged(ref this.name, value); }
        }
    }
}