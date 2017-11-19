// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MeasurementScalesBrowserViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BasicRdl.ViewModels
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Events;
    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="MeasurementScalesBrowserViewModel"/> is to represent the view-model for <see cref="MeasurementScale"/>s
    /// </summary>
    public class MeasurementScalesBrowserViewModel : BrowserViewModelBase<SiteDirectory>, IPanelViewModel
    {
        #region Fields

        /// <summary>
        /// The Panel Caption
        /// </summary>
        private const string PanelCaption = "Measurement Scales";

        /// <summary>
        /// Backing field for <see cref="CanCreateRdlElement"/>
        /// </summary>
        private bool canCreateRdlElement;

        /// <summary>
        /// Backing field for the <see cref="MeasurementScales"/> property.
        /// </summary>
        private readonly ReactiveList<MeasurementScaleRowViewModel> measurementScales = new ReactiveList<MeasurementScaleRowViewModel>();
        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MeasurementScalesBrowserViewModel"/> class.
        /// </summary>
        /// <param name="session">the associated session</param>
        /// <param name="siteDir">The unique <see cref="SiteDirectory"/></param>
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService"/> that is used to navigate to a dialog of a specific <see cref="Thing"/></param>
        /// <param name="panelNavigationService">The <see cref="IPanelNavigationService"/> that is used to navigate to a panel</param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/> that is used to navigate to a dialog</param>
        public MeasurementScalesBrowserViewModel(ISession session, SiteDirectory siteDir, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService)
            : base(siteDir, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService)
        {
            this.Caption = string.Format("{0}, {1}", PanelCaption, this.Thing.Name);
            this.ToolTip = string.Format("{0}\n{1}\n{2}", this.Thing.Name, this.Thing.IDalUri, this.Session.ActivePerson.Name);

            this.measurementScales.ChangeTrackingEnabled = true;

            this.AddSubscriptions();
        }

        #endregion Constructors

        #region Public properties

        /// <summary>
        /// Gets the <see cref="MeasurementScaleRowViewModel"/> that are contained by this view-model
        /// </summary>
        public ReactiveList<MeasurementScaleRowViewModel> MeasurementScales
        {
            get
            {
                return this.measurementScales;
            }
        }

        /// <summary>
        /// Gets a value indicating whether a RDL element may be created
        /// </summary>
        public bool CanCreateRdlElement
        {
            get { return this.canCreateRdlElement; }
            private set { this.RaiseAndSetIfChanged(ref this.canCreateRdlElement, value); }
        }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to create a <see cref="IntervalScale"/>
        /// </summary>
        public ReactiveCommand<object> CreateIntervalScale { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to create a <see cref="LogarithmicScale"/>
        /// </summary>
        public ReactiveCommand<object> CreateLogarithmicScale { get; private set; }     

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to create a <see cref="OrdinalScale"/>
        /// </summary>
        public ReactiveCommand<object> CreateOrdinalScale { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to create a <see cref="RatioScale"/>
        /// </summary>
        public ReactiveCommand<object> CreateRatioScale { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to create a <see cref="CyclicRatioScale"/>
        /// </summary>
        public ReactiveCommand<object> CreateCyclicRatioScale { get; private set; } 

        #endregion Public properties

        #region Methods

        /// <summary>
        /// Add the necessary subscriptions for this view model.
        /// </summary>
        private void AddSubscriptions()
        {
            var addListener =
                CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(MeasurementScale))
                    .Where(objectChange => objectChange.EventKind == EventKind.Added && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                    .Select(x => x.ChangedThing as MeasurementScale)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(this.AddMeasurementScaleRowViewModel);
            this.Disposables.Add(addListener);

            var removeListener =
                CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(MeasurementScale))
                    .Where(objectChange => objectChange.EventKind == EventKind.Removed && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                    .Select(x => x.ChangedThing as MeasurementScale)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(this.RemoveMeasurementScaleRowViewModel);
            this.Disposables.Add(removeListener);

            var rdlUpdateListener =
                CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(ReferenceDataLibrary))
                    .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                    .Select(x => x.ChangedThing as ReferenceDataLibrary)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(this.RefreshContainerName);
            this.Disposables.Add(rdlUpdateListener);
        }

        /// <summary>
        /// Adds a <see cref="MeasurementScaleRowViewModel"/>
        /// </summary>
        /// <param name="scale">
        /// The associated <see cref="MeasurementScale"/> for which the row is to be added.
        /// </param>
        private void AddMeasurementScaleRowViewModel(MeasurementScale scale)
        {
            var row = new MeasurementScaleRowViewModel(scale, this.Session, this);
            this.MeasurementScales.Add(row);
        }

        /// <summary>
        /// Removes a <see cref="MeasurementScaleRowViewModel"/> from the view model
        /// </summary>
        /// <param name="scale">
        /// The <see cref="MeasurementScale"/> for which the row view model has to be removed
        /// </param>
        private void RemoveMeasurementScaleRowViewModel(MeasurementScale scale)
        {
            var row = this.MeasurementScales.SingleOrDefault(rowViewModel => rowViewModel.Thing == scale);
            if (row != null)
            {
                this.MeasurementScales.Remove(row);
                row.Dispose();
            }
        }

        /// <summary>
        /// Refresh the displayed container name for the category rows
        /// </summary>
        /// <param name="rdl">
        /// The updated <see cref="ReferenceDataLibrary"/>.
        /// </param>
        private void RefreshContainerName(ReferenceDataLibrary rdl)
        {
            foreach (var scale in this.measurementScales)
            {
                if (scale.Thing.Container != rdl)
                {
                    continue;
                }

                if (scale.ContainerRdl != rdl.ShortName)
                {
                    scale.ContainerRdl = rdl.ShortName;
                }
            }
        }

        /// <summary>
        /// Loads the <see cref="Thing"/>s from the cache when the browser is instantiated.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            var openDataLibrariesIids = this.Session.OpenReferenceDataLibraries.Select(y => y.Iid);
            foreach (var referenceDataLibrary in this.Thing.AvailableReferenceDataLibraries().Where(x => openDataLibrariesIids.Contains(x.Iid)))
            {
                foreach (var scale in referenceDataLibrary.Scale)
                {
                    this.AddMeasurementScaleRowViewModel(scale);
                }
            }
        }

        /// <summary>
        /// Initializes the create <see cref="ReactiveCommand"/> that allow a user to create the different kinds of <see cref="ParameterType"/>s
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            this.CreateRatioScale = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanCreateRdlElement));
            this.CreateRatioScale.Subscribe(_ => this.ExecuteCreateCommand<RatioScale>());

            this.CreateIntervalScale = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanCreateRdlElement));
            this.CreateIntervalScale.Subscribe(_ => this.ExecuteCreateCommand<IntervalScale>());

            this.CreateLogarithmicScale = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanCreateRdlElement));
            this.CreateLogarithmicScale.Subscribe(_ => this.ExecuteCreateCommand<LogarithmicScale>());

            this.CreateOrdinalScale = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanCreateRdlElement));
            this.CreateOrdinalScale.Subscribe(_ => this.ExecuteCreateCommand<OrdinalScale>());

            this.CreateCyclicRatioScale = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanCreateRdlElement));
            this.CreateCyclicRatioScale.Subscribe(_ => this.ExecuteCreateCommand<CyclicRatioScale>());
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// a value indicating whether the class is being disposed of
        /// </param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            foreach (var scale in this.MeasurementScales)
            {
                scale.Dispose();
            }
        }

        /// <summary>
        /// Compute the permissions
        /// </summary>
        public override void ComputePermission()
        {
            base.ComputePermission();
            this.CanCreateRdlElement = this.Session.OpenReferenceDataLibraries.Any();
        }

        /// <summary>
        /// Populate the <see cref="ContextMenuItemViewModel"/>s of the current browser
        /// </summary>
        public override void PopulateContextMenu()
        {
            base.PopulateContextMenu();

            this.ContextMenu.Add(new ContextMenuItemViewModel("Create a Cyclic Ratio Scale", "", this.CreateCyclicRatioScale, MenuItemKind.Create, ClassKind.CyclicRatioScale));
            this.ContextMenu.Add(new ContextMenuItemViewModel("Create an Interval Scale", "", this.CreateIntervalScale, MenuItemKind.Create, ClassKind.IntervalScale));
            this.ContextMenu.Add(new ContextMenuItemViewModel("Create a Logarithmic Scale", "", this.CreateLogarithmicScale, MenuItemKind.Create, ClassKind.LogarithmicScale));
            this.ContextMenu.Add(new ContextMenuItemViewModel("Create an Ordinal Scale", "", this.CreateOrdinalScale, MenuItemKind.Create, ClassKind.OrdinalScale));
            this.ContextMenu.Add(new ContextMenuItemViewModel("Create a Ratio Scale", "", this.CreateRatioScale, MenuItemKind.Create, ClassKind.RatioScale));
        }
        #endregion
    }
}