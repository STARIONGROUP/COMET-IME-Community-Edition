// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SpecializedQuantityKindDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BasicRdl.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using CDP4Common.CommonData;
    using CDP4Dal.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="SpecializedQuantityKindDialogViewModel"/> is to provide a dialog view model
    /// for a <see cref="SpecializedQuantityKind"/>
    /// </summary>
    [ThingDialogViewModelExport(ClassKind.SpecializedQuantityKind)]
    public class SpecializedQuantityKindDialogViewModel : CDP4CommonView.SpecializedQuantityKindDialogViewModel, IThingDialogViewModel
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SpecializedQuantityKindDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public SpecializedQuantityKindDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpecializedQuantityKindDialogViewModel"/> class.
        /// </summary>
        /// <param name="specializedQuantityKind">
        /// The Specialized quantity Kind.
        /// </param>
        /// <param name="transaction">
        /// The <see cref="ThingTransaction"/> that contains the log of recorded changes.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the current <see cref="Thing"/> is to be added or updated
        /// </param>
        /// <param name="isRoot">
        /// Assert if this <see cref="SpecializedQuantityKindDialogViewModel"/> is the root of all <see cref="IThingDialogViewModel"/>
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation this <see cref="SpecializedQuantityKindDialogViewModel"/> performs
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
        public SpecializedQuantityKindDialogViewModel(SpecializedQuantityKind specializedQuantityKind, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
            : base(specializedQuantityKind, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            this.WhenAnyValue(vm => vm.Container).Subscribe(_ => { this.PopulatePossiblePossibleScales(); this.PopulatePossibleGeneral(); });
            this.WhenAnyValue(vm => vm.SelectedDefaultScale).Subscribe(_ => this.UpdateOkCanExecute());
            this.WhenAnyValue(vm => vm.SelectedGeneral).Subscribe(_ => { this.PopulatePossiblePossibleScales(); this.UpdateOkCanExecute(); });
        }

        #endregion

        #region Properties and Commands
        /// <summary>
        /// Gets the <see cref="MeasurementScale"/> from the general <see cref="QuantityKind"/>
        /// </summary>
        public ReactiveList<MeasurementScale> GeneralizationScale { get; private set; }

        /// <summary>
        /// Gets all the possible <see cref="MeasurementScale"/> for the current <see cref="SpecializedQuantityKind"/>
        /// </summary>
        public ReactiveList<MeasurementScale> AllPossibleScale { get; private set; } 
        #endregion

        #region Methods
        /// <summary>
        /// Initializes the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.GeneralizationScale = new ReactiveList<MeasurementScale>();
            this.AllPossibleScale = new ReactiveList<MeasurementScale>();
        }

        /// <summary>
        /// Updates the <see cref="OkCanExecute"/> property using validation rules
        /// </summary>
        protected override void UpdateOkCanExecute()
        {
            base.UpdateOkCanExecute();
            this.OkCanExecute = this.OkCanExecute && this.SelectedGeneral != null && this.SelectedDefaultScale != null;
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.PopulatePossiblePossibleScales();
            this.PopulateGeneralScale();
        }

        /// <summary>
        /// The populate all possible scales.
        /// </summary>
        private void PopulatePossiblePossibleScales()
        {
            this.PossiblePossibleScale.Clear();

            var generalScale = new List<MeasurementScale>();
            if (this.SelectedGeneral != null)
            {
                generalScale.AddRange(this.SelectedGeneral.PossibleScale);
            }

            var containerRdl = this.Container as ReferenceDataLibrary;
            if (containerRdl == null)
            {
                return;
            }

            var allScales = new List<MeasurementScale>(containerRdl.Scale.Except(generalScale));
            allScales.AddRange(containerRdl.GetRequiredRdls().SelectMany(rdl => rdl.Scale));
            this.PossiblePossibleScale.AddRange(allScales.OrderBy(c => c.ShortName));
        }

        /// <summary>
        /// The populate possible general.
        /// </summary>
        protected override void PopulatePossibleGeneral()
        {
            base.PopulatePossibleGeneral();
            var containerRdl = this.Container as ReferenceDataLibrary;
            if (containerRdl != null)
            {
                var allQuantityKinds = containerRdl.ParameterType.OfType<QuantityKind>().ToList();
                allQuantityKinds.AddRange(containerRdl.GetRequiredRdls().SelectMany(rdl => rdl.ParameterType.OfType<QuantityKind>()));
                this.PossibleGeneral.AddRange(allQuantityKinds.OrderBy(c => c.Name));
            }
        }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            this.WhenAnyValue(x => x.SelectedGeneral).Where(x => x != null).Subscribe(x =>
            {
                this.PopulateGeneralScale();
                this.PopulateAllPossibleDefaultScale();
            });

            this.PossibleScale.ChangeTrackingEnabled = true;
            this.PossibleScale.CountChanged.Subscribe(x => this.PopulateAllPossibleDefaultScale());
        }
        #endregion

        /// <summary>
        /// Populates the <see cref="AllPossibleScale"/> property
        /// </summary>
        private void PopulateAllPossibleDefaultScale()
        {
            this.AllPossibleScale.Clear();
            var allScales = new List<MeasurementScale>(this.PossibleScale);
            allScales.AddRange(this.GeneralizationScale);
            this.AllPossibleScale.AddRange(allScales.OrderBy(c => c.ShortName));
        }

        /// <summary>
        /// Populates the general <see cref="MeasurementScale"/>
        /// </summary>
        private void PopulateGeneralScale()
        {
            this.GeneralizationScale.Clear();
            if (this.SelectedGeneral == null)
            {
                return;
            }

            this.GeneralizationScale.AddRange(this.SelectedGeneral.AllPossibleScale);
        }
    }
}