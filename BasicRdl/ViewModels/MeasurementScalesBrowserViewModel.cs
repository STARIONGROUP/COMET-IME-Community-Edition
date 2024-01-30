// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MeasurementScalesBrowserViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2024 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BasicRdl.ViewModels
{
    using System;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Mvvm.Types;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;
    using CDP4Dal.Events;

    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="MeasurementScalesBrowserViewModel"/> is to represent the view-model for <see cref="MeasurementScale"/>s
    /// </summary>
    public class MeasurementScalesBrowserViewModel : BrowserViewModelBase<SiteDirectory>, IPanelViewModel,
        IDeprecatableBrowserViewModel
    {
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
        private readonly DisposableReactiveList<MeasurementScaleRowViewModel> measurementScales =
            new DisposableReactiveList<MeasurementScaleRowViewModel>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MeasurementScalesBrowserViewModel"/> class.
        /// </summary>
        /// <param name="session">the associated session</param>
        /// <param name="siteDir">The unique <see cref="SiteDirectory"/></param>
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService"/> that is used to navigate to a dialog of a specific <see cref="Thing"/></param>
        /// <param name="panelNavigationService">The <see cref="IPanelNavigationService"/> that is used to navigate to a panel</param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/> that is used to navigate to a dialog</param>
        /// <param name="pluginSettingsService">
        /// The <see cref="IPluginSettingsService"/> used to read and write plugin setting files.
        /// </param>
        public MeasurementScalesBrowserViewModel(
            ISession session,
            SiteDirectory siteDir,
            IThingDialogNavigationService thingDialogNavigationService,
            IPanelNavigationService panelNavigationService,
            IDialogNavigationService dialogNavigationService,
            IPluginSettingsService pluginSettingsService)
            : base(
                siteDir,
                session,
                thingDialogNavigationService,
                panelNavigationService,
                dialogNavigationService,
                pluginSettingsService)
        {
            this.Caption = $"{PanelCaption}, {this.Thing.Name}";
            this.ToolTip = $"{this.Thing.Name}\n{this.Thing.IDalUri}\n{this.Session.ActivePerson.Name}";

            this.AddSubscriptions();
        }

        /// <summary>
        /// Gets the <see cref="MeasurementScaleRowViewModel"/> that are contained by this view-model
        /// </summary>
        public DisposableReactiveList<MeasurementScaleRowViewModel> MeasurementScales => this.measurementScales;

        /// <summary>
        /// Gets a value indicating whether a RDL element may be created
        /// </summary>
        public bool CanCreateRdlElement
        {
            get => this.canCreateRdlElement;
            private set => this.RaiseAndSetIfChanged(ref this.canCreateRdlElement, value);
        }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to create a <see cref="IntervalScale"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateIntervalScale { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to create a <see cref="LogarithmicScale"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateLogarithmicScale { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to create a <see cref="OrdinalScale"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateOrdinalScale { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to create a <see cref="RatioScale"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateRatioScale { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to create a <see cref="CyclicRatioScale"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateCyclicRatioScale { get; private set; }

        /// <summary>
        /// Gets or sets the dock layout group target name to attach this panel to on opening
        /// </summary>
        public string TargetName { get; set; } = LayoutGroupNames.LeftGroup;

        /// <summary>
        /// Add the necessary subscriptions for this view model.
        /// </summary>
        private void AddSubscriptions()
        {
            var addListener =
                this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(MeasurementScale))
                    .Where(
                        objectChange => objectChange.EventKind == EventKind.Added &&
                                        objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                    .Select(x => x.ChangedThing as MeasurementScale)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(this.AddMeasurementScaleRowViewModel);

            this.Disposables.Add(addListener);

            var removeListener =
                this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(MeasurementScale))
                    .Where(
                        objectChange => objectChange.EventKind == EventKind.Removed &&
                                        objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                    .Select(x => x.ChangedThing as MeasurementScale)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(this.RemoveMeasurementScaleRowViewModel);

            this.Disposables.Add(removeListener);

            var rdlUpdateListener =
                this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(ReferenceDataLibrary))
                    .Where(
                        objectChange => objectChange.EventKind == EventKind.Updated &&
                                        objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
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
                this.MeasurementScales.RemoveAndDispose(row);
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

            foreach (var referenceDataLibrary in this.Thing.AvailableReferenceDataLibraries()
                         .Where(x => openDataLibrariesIids.Contains(x.Iid)))
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

            this.CreateRatioScale = ReactiveCommandCreator.Create(() => this.ExecuteCreateCommand<RatioScale>(), this.WhenAnyValue(x => x.CanCreateRdlElement));

            this.CreateIntervalScale = ReactiveCommandCreator.Create(() => this.ExecuteCreateCommand<IntervalScale>(), this.WhenAnyValue(x => x.CanCreateRdlElement));

            this.CreateLogarithmicScale = ReactiveCommandCreator.Create(() => this.ExecuteCreateCommand<LogarithmicScale>(), this.WhenAnyValue(x => x.CanCreateRdlElement));

            this.CreateOrdinalScale = ReactiveCommandCreator.Create(() => this.ExecuteCreateCommand<OrdinalScale>(), this.WhenAnyValue(x => x.CanCreateRdlElement));

            this.CreateCyclicRatioScale = ReactiveCommandCreator.Create(() => this.ExecuteCreateCommand<CyclicRatioScale>(), this.WhenAnyValue(x => x.CanCreateRdlElement));
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

            this.ContextMenu.Add(
                new ContextMenuItemViewModel(
                    "Create a Cyclic Ratio Scale",
                    "",
                    this.CreateCyclicRatioScale,
                    MenuItemKind.Create,
                    ClassKind.CyclicRatioScale));

            this.ContextMenu.Add(
                new ContextMenuItemViewModel(
                    "Create an Interval Scale",
                    "",
                    this.CreateIntervalScale,
                    MenuItemKind.Create,
                    ClassKind.IntervalScale));

            this.ContextMenu.Add(
                new ContextMenuItemViewModel(
                    "Create a Logarithmic Scale",
                    "",
                    this.CreateLogarithmicScale,
                    MenuItemKind.Create,
                    ClassKind.LogarithmicScale));

            this.ContextMenu.Add(
                new ContextMenuItemViewModel(
                    "Create an Ordinal Scale",
                    "",
                    this.CreateOrdinalScale,
                    MenuItemKind.Create,
                    ClassKind.OrdinalScale));

            this.ContextMenu.Add(
                new ContextMenuItemViewModel(
                    "Create a Ratio Scale",
                    "",
                    this.CreateRatioScale,
                    MenuItemKind.Create,
                    ClassKind.RatioScale));
        }
    }
}
