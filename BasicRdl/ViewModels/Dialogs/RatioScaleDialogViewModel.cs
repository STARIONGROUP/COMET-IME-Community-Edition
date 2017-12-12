// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RatioScaleDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BasicRdl.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Dal.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4CommonView;
    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="RatioScaleDialogViewModel"/> is to provide a dialog view model
    /// for a <see cref="RatioScale"/>
    /// </summary>
    [ThingDialogViewModelExport(ClassKind.RatioScale)]
    public class RatioScaleDialogViewModel : CDP4CommonView.RatioScaleDialogViewModel, IThingDialogViewModel
    {
        /// <summary>
        /// The backing field for <see cref="ShortName"/>
        /// </summary>
        private string shortName;

        /// <summary>
        /// The backing field for <see cref="Name"/>
        /// </summary>
        private string name;

        /// <summary>
        /// Initializes a new instance of the <see cref="RatioScaleDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public RatioScaleDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RatioScaleDialogViewModel"/> class.
        /// </summary>
        /// <param name="ratioScale">
        /// The Simple quantity Kind.
        /// </param>
        /// <param name="transaction">
        /// The <see cref="ThingTransaction"/> that contains the log of recorded changes.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the current <see cref="Thing"/> is to be added or updated
        /// </param>
        /// <param name="isRoot">
        /// Assert if this <see cref="RatioScaleDialogViewModel"/> is the root of all <see cref="IThingDialogViewModel"/>
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation this <see cref="RatioScaleDialogViewModel"/> performs
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
        public RatioScaleDialogViewModel(RatioScale ratioScale, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
            : base(ratioScale, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            this.WhenAnyValue(vm => vm.Container).Subscribe(_ => this.PopulatePossibleUnit());
            this.WhenAnyValue(vm => vm.SelectedUnit).Subscribe(_ => this.UpdateOkCanExecute());
        }

        /// <summary>
        /// Updates the <see cref="OkCanExecute"/> property using validation rules
        /// </summary>
        protected override void UpdateOkCanExecute()
        {
            base.UpdateOkCanExecute();
            this.OkCanExecute = this.OkCanExecute && this.SelectedUnit != null;
        }

        /// <summary>
        /// Populates the <see cref="MeasurementScaleDialogViewModel{T}.PossibleUnit"/> property
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