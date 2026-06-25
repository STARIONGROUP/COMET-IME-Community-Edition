// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MeasurementUnitsBrowserViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2026 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Rowan de Voogt
//
//    This file is part of CDP4-COMET IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
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
    using System.Threading.Tasks;

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
    using CDP4Dal.Operations;

    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="MeasurementUnitsBrowserViewModel"/> is to represent the view-model for <see cref="MeasurementUnit"/>s
    /// </summary>
    public class MeasurementUnitsBrowserViewModel : BrowserViewModelBase<SiteDirectory>, IPanelViewModel,
        IDeprecatableBrowserViewModel
    {
        /// <summary>
        /// The Panel Caption
        /// </summary>
        private const string PanelCaption = "Measurement Units";

        /// <summary>
        /// Backing field for the <see cref="MeasurementUnits"/> property
        /// </summary>
        private readonly DisposableReactiveList<MeasurementUnitRowViewModel> measurementUnits =
            new DisposableReactiveList<MeasurementUnitRowViewModel>();

        /// <summary>
        /// Backing field for <see cref="CanCreateRdlElement"/>
        /// </summary>
        private bool canCreateRdlElement;

        /// <summary>
        /// Backing field for <see cref="CanSetAsBaseUnit"/>
        /// </summary>
        private bool canSetAsBaseUnit;

        /// <summary>
        /// Initializes a new instance of the <see cref="MeasurementUnitsBrowserViewModel"/> class.
        /// </summary>
        /// <param name="session">the associated session</param>
        /// <param name="siteDir">The unique <see cref="SiteDirectory"/></param>
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService"/> that is used to navigate to a dialog of a specific <see cref="Thing"/></param>
        /// <param name="panelNavigationService">The <see cref="IPanelNavigationService"/> that is used to navigate to a panel</param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/> that is used to navigate to a panel</param>
        /// <param name="pluginSettingsService">
        /// The <see cref="IPluginSettingsService"/> used to read and write plugin setting files.
        /// </param>
        public MeasurementUnitsBrowserViewModel(
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
        /// Gets the <see cref="MeasurementUnitRowViewModel"/> that are contained by this view-model
        /// </summary>
        public DisposableReactiveList<MeasurementUnitRowViewModel> MeasurementUnits => this.measurementUnits;

        /// <summary>
        /// Gets a value indicating whether a RDL element may be created
        /// </summary>
        public bool CanCreateRdlElement
        {
            get => this.canCreateRdlElement;
            private set => this.RaiseAndSetIfChanged(ref this.canCreateRdlElement, value);
        }

        /// <summary>
        /// Gets a value indicating whether the selected <see cref="MeasurementUnit"/> may be set or unset as a base unit
        /// </summary>
        public bool CanSetAsBaseUnit
        {
            get => this.canSetAsBaseUnit;
            private set => this.RaiseAndSetIfChanged(ref this.canSetAsBaseUnit, value);
        }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to set or unset the selected <see cref="MeasurementUnit"/> as a base unit
        /// </summary>
        public ReactiveCommand<Unit, Unit> SetAsBaseUnitCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to create a <see cref="SimpleUnit"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateSimpleUnit { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to create a <see cref="DerivedUnit"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateDerivedUnit { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to create a <see cref="LinearConversionUnit"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateLinearConversionUnit { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to create a <see cref="PrefixedUnit"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreatePrefixedUnit { get; private set; }

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
                this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(MeasurementUnit))
                    .Where(
                        objectChange => objectChange.EventKind == EventKind.Added &&
                                        objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                    .Select(x => x.ChangedThing as MeasurementUnit)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(this.AddMeasurementUnitRowViewModel);

            this.Disposables.Add(addListener);

            var removeListener =
                this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(MeasurementUnit))
                    .Where(
                        objectChange => objectChange.EventKind == EventKind.Removed &&
                                        objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                    .Select(x => x.ChangedThing as MeasurementUnit)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(this.RemoveMeasurementUnitRowViewModel);

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

            var rdlOpenedListener = this.CDPMessageBus.Listen<SessionEvent>()
                .Where(sessionEvent => sessionEvent.Session == this.Session && sessionEvent.Status == SessionStatus.RdlOpened)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.PopulateMeasurementUnits());

            this.Disposables.Add(rdlOpenedListener);
        }

        /// <summary>
        /// Adds a <see cref="MeasurementUnitRowViewModel"/>
        /// </summary>
        /// <param name="measurementUnit">The associated <see cref="MeasurementUnit"/></param>
        private void AddMeasurementUnitRowViewModel(MeasurementUnit measurementUnit)
        {
            if (this.MeasurementUnits.Any(x => x.Thing.Iid == measurementUnit.Iid))
            {
                return;
            }

            if (!this.IsInOpenReferenceDataLibrary(measurementUnit))
            {
                return;
            }

            var row = new MeasurementUnitRowViewModel(measurementUnit, this.Session, this);
            this.MeasurementUnits.Add(row);
        }

        /// <summary>
        /// Removes a <see cref="MeasurementUnitRowViewModel"/> from the view model
        /// </summary>
        /// <param name="measurementUnit">
        /// The <see cref="MeasurementUnit"/> for which the row view model has to be removed
        /// </param>
        private void RemoveMeasurementUnitRowViewModel(MeasurementUnit measurementUnit)
        {
            var rows = this.MeasurementUnits.Where(rowViewModel => rowViewModel.Thing.Iid == measurementUnit.Iid).ToList();

            foreach (var row in rows)
            {
                this.MeasurementUnits.RemoveAndDispose(row);
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
            foreach (var measurementUnit in this.measurementUnits)
            {
                if (measurementUnit.Thing.Container != rdl)
                {
                    continue;
                }

                if (measurementUnit.ContainerRdl != rdl.ShortName)
                {
                    measurementUnit.ContainerRdl = rdl.ShortName;
                }

                measurementUnit.IsBaseUnit = rdl.BaseUnit.Any(u => u.Iid == measurementUnit.Thing.Iid);
            }
        }

        /// <summary>
        /// Compute the permissions
        /// </summary>
        public override void ComputePermission()
        {
            base.ComputePermission();
            this.CanCreateRdlElement = this.Session.OpenReferenceDataLibraries.Any();

            if (this.SelectedThing is MeasurementUnitRowViewModel selectedRow)
            {
                var rdl = selectedRow.Thing.Container as ReferenceDataLibrary;
                this.CanSetAsBaseUnit = rdl != null && this.Session.PermissionService.CanWrite(rdl);
            }
            else
            {
                this.CanSetAsBaseUnit = false;
            }
        }

        /// <summary>
        /// Populate the <see cref="ContextMenuItemViewModel"/>s of the current browser
        /// </summary>
        public override void PopulateContextMenu()
        {
            base.PopulateContextMenu();

            if (this.SelectedThing is MeasurementUnitRowViewModel selectedRow)
            {
                this.ContextMenu.Add(new ContextMenuItemViewModel(
                    selectedRow.IsBaseUnit ? "Unset as Base Unit" : "Set as Base Unit",
                    "",
                    this.SetAsBaseUnitCommand,
                    MenuItemKind.None));
            }

            this.ContextMenu.Add(
                new ContextMenuItemViewModel(
                    "Create a Simple Unit",
                    "",
                    this.CreateSimpleUnit,
                    MenuItemKind.Create,
                    ClassKind.SimpleUnit));

            this.ContextMenu.Add(
                new ContextMenuItemViewModel(
                    "Create a Derived Unit",
                    "",
                    this.CreateDerivedUnit,
                    MenuItemKind.Create,
                    ClassKind.DerivedUnit));

            this.ContextMenu.Add(
                new ContextMenuItemViewModel(
                    "Create a Linear Conversion Unit",
                    "",
                    this.CreateLinearConversionUnit,
                    MenuItemKind.Create,
                    ClassKind.LinearConversionUnit));

            this.ContextMenu.Add(
                new ContextMenuItemViewModel(
                    "Create a Prefixed Unit",
                    "",
                    this.CreatePrefixedUnit,
                    MenuItemKind.Create,
                    ClassKind.PrefixedUnit));
        }

        /// <summary>
        /// Initializes the create <see cref="ReactiveCommand"/> that allow a user to create the different kinds of <see cref="MeasurementUnit"/>s
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            this.SetAsBaseUnitCommand = ReactiveCommandCreator.Create(
                this.ExecuteSetAsBaseUnitCommand,
                this.WhenAnyValue(x => x.CanSetAsBaseUnit));

            this.CreateSimpleUnit = ReactiveCommandCreator.Create(() => this.ExecuteCreateCommand<SimpleUnit>(), this.WhenAnyValue(x => x.CanCreateRdlElement));

            this.CreateDerivedUnit = ReactiveCommandCreator.Create(() => this.ExecuteCreateCommand<DerivedUnit>(), this.WhenAnyValue(x => x.CanCreateRdlElement));

            this.CreateLinearConversionUnit = ReactiveCommandCreator.Create(() => this.ExecuteCreateCommand<LinearConversionUnit>(), this.WhenAnyValue(x => x.CanCreateRdlElement));

            this.CreatePrefixedUnit = ReactiveCommandCreator.Create(() => this.ExecuteCreateCommand<PrefixedUnit>(), this.WhenAnyValue(x => x.CanCreateRdlElement));
        }

        /// <summary>
        /// Executes the <see cref="SetAsBaseUnitCommand"/> by toggling the base-unit membership of the selected
        /// <see cref="MeasurementUnit"/> on the container <see cref="ReferenceDataLibrary"/>.
        /// </summary>
        private async void ExecuteSetAsBaseUnitCommand()
        {
            if (!(this.SelectedThing is MeasurementUnitRowViewModel selectedRow))
            {
                return;
            }

            var unit = selectedRow.Thing;
            var rdl = unit.Container as ReferenceDataLibrary;

            if (rdl == null)
            {
                return;
            }

            var rdlClone = rdl.Clone(false);

            if (selectedRow.IsBaseUnit)
            {
                rdlClone.BaseUnit.RemoveAll(u => u.Iid == unit.Iid);
            }
            else
            {
                rdlClone.BaseUnit.Add(unit);
            }

            var transactionContext = TransactionContextResolver.ResolveContext(this.Thing);
            var transaction = new ThingTransaction(transactionContext, rdlClone);
            await this.DalWrite(transaction);
        }

        /// <summary>
        /// Loads the <see cref="Thing"/>s from the cache when the browser is instantiated.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            this.PopulateMeasurementUnits();
        }

        /// <summary>
        /// Adds a row for every <see cref="MeasurementUnit"/> contained in the currently open <see cref="ReferenceDataLibrary"/>s.
        /// </summary>
        private void PopulateMeasurementUnits()
        {
            var openDataLibrariesIids = this.Session.OpenReferenceDataLibraries.Select(y => y.Iid).ToList();

            foreach (var referenceDataLibrary in this.Thing.AvailableReferenceDataLibraries()
                         .Where(x => openDataLibrariesIids.Contains(x.Iid)))
            {
                foreach (var measurementUnit in referenceDataLibrary.Unit)
                {
                    this.AddMeasurementUnitRowViewModel(measurementUnit);
                }
            }
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

            foreach (var unit in this.MeasurementUnits)
            {
                unit.Dispose();
            }
        }
    }
}
