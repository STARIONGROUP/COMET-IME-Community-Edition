﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConstantsBrowserViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2024 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
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
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition;
    using CDP4Composition.DragDrop;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Mvvm.Types;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;
    using CDP4Dal.Events;

    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="ConstantsBrowserViewModel"/> is to represent the view-model for <see cref="Constant"/>s
    /// </summary>
    public class ConstantsBrowserViewModel : BrowserViewModelBase<SiteDirectory>, IPanelViewModel, IDropTarget,
        IDeprecatableBrowserViewModel
    {
        /// <summary>
        /// The Panel Caption
        /// </summary>
        private const string PanelCaption = "Constants";

        /// <summary>
        /// Backing field for the <see cref="Constants"/> property
        /// </summary>
        private readonly DisposableReactiveList<ConstantRowViewModel> constants = new DisposableReactiveList<ConstantRowViewModel>();

        /// <summary>
        /// Backing field for <see cref="CanCreateRdlElement"/>
        /// </summary>
        private bool canCreateRdlElement;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantsBrowserViewModel"/> class.
        /// </summary>
        /// <param name="session">the associated <see cref="ISession"/></param>
        /// <param name="siteDir">The unique <see cref="SiteDirectory"/></param>
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService"/></param>
        /// <param name="panelNavigationService">The <see cref="IPanelNavigationService"/></param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/></param>
        /// <param name="pluginSettingsService">
        /// The <see cref="IPluginSettingsService"/> used to read and write plugin setting files.
        /// </param>
        public ConstantsBrowserViewModel(
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
        /// Gets the <see cref="ConstantRowViewModel"/> that are contained by this view-model
        /// </summary>
        public DisposableReactiveList<ConstantRowViewModel> Constants => this.constants;

        /// <summary>
        /// Gets a value indicating whether a RDL element may be created
        /// </summary>
        public bool CanCreateRdlElement
        {
            get => this.canCreateRdlElement;
            private set => this.RaiseAndSetIfChanged(ref this.canCreateRdlElement, value);
        }

        /// <summary>
        /// Gets or sets the dock layout group target name to attach this panel to on opening
        /// </summary>
        public string TargetName { get; set; } = LayoutGroupNames.LeftGroup;

        /// <summary>
        /// Initializes the Commands that can be executed from this view model. The commands are initialized
        /// before the <see cref="PopulateContextMenu"/> is invoked
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            this.CreateCommand = ReactiveCommandCreator.Create(() => this.ExecuteCreateCommand<Constant>(), this.WhenAnyValue(x => x.CanCreateRdlElement));
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
        /// Populate the context menu
        /// </summary>
        public override void PopulateContextMenu()
        {
            base.PopulateContextMenu();

            this.ContextMenu.Add(
                new ContextMenuItemViewModel(
                    "Create a Constant",
                    "",
                    this.CreateCommand,
                    MenuItemKind.Create,
                    ClassKind.Constant));
        }

        /// <summary>
        /// Loads the <see cref="Constant"/>s from the cache when the browser is instantiated.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            var openDataLibrariesIids = this.Session.OpenReferenceDataLibraries.Select(y => y.Iid);

            foreach (var referenceDataLibrary in this.Thing.AvailableReferenceDataLibraries()
                         .Where(x => openDataLibrariesIids.Contains(x.Iid)))
            {
                foreach (var constant in referenceDataLibrary.Constant)
                {
                    this.AddConstantRowViewModel(constant);
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

            foreach (var constant in this.Constants)
            {
                constant.Dispose();
            }
        }

        /// <summary>
        /// Add the necessary subscriptions for this view model.
        /// </summary>
        private void AddSubscriptions()
        {
            var addListener =
                this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(Constant))
                    .Where(
                        objectChange => objectChange.EventKind == EventKind.Added &&
                                        objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                    .Select(x => x.ChangedThing as Constant)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(this.AddConstantRowViewModel);

            this.Disposables.Add(addListener);

            var removeListener =
                this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(Constant))
                    .Where(
                        objectChange => objectChange.EventKind == EventKind.Removed &&
                                        objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                    .Select(x => x.ChangedThing as Constant)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(this.RemoveConstantRowViewModel);

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
        /// Adds a <see cref="ConstantRowViewModel"/>
        /// </summary>
        /// <param name="constant">
        /// The associated <see cref="Constant"/> for which the row is to be added.
        /// </param>
        private void AddConstantRowViewModel(Constant constant)
        {
            if (this.Constants.Any(x => x.Thing == constant))
            {
                return;
            }

            var row = new ConstantRowViewModel(constant, this.Session, this);
            this.Constants.Add(row);
        }

        /// <summary>
        /// Removes a <see cref="ConstantRowViewModel"/> from the view model
        /// </summary>
        /// <param name="constant">
        /// The <see cref="Constant"/> for which the row view model has to be removed
        /// </param>
        private void RemoveConstantRowViewModel(Constant constant)
        {
            var row = this.Constants.SingleOrDefault(rowViewModel => rowViewModel.Thing == constant);

            if (row != null)
            {
                this.Constants.RemoveAndDispose(row);
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
            foreach (var constant in this.constants)
            {
                if (constant.Thing.Container != rdl)
                {
                    continue;
                }

                if (constant.ContainerRdl != rdl.ShortName)
                {
                    constant.ContainerRdl = rdl.ShortName;
                }
            }
        }

        /// <summary>
        /// Updates the current drag state.
        /// </summary>
        /// <param name="dropInfo">
        ///  Information about the drag operation.
        /// </param>
        /// <remarks>
        /// To allow a drop at the current drag position, the <see cref="DropInfo.Effects"/> property on 
        /// <paramref name="dropInfo"/> should be set to a value other than <see cref="DragDropEffects.None"/>
        /// and <see cref="DropInfo.Payload"/> should be set to a non-null value.
        /// </remarks>
        public void DragOver(IDropInfo dropInfo)
        {
            logger.Trace("drag over {0}", dropInfo.TargetItem);
            var droptarget = dropInfo.TargetItem as IDropTarget;

            if (droptarget == null)
            {
                dropInfo.Effects = DragDropEffects.None;
                return;
            }

            droptarget.DragOver(dropInfo);
        }

        /// <summary>
        /// Performs the drop operation
        /// </summary>
        /// <param name="dropInfo">
        /// Information about the drop operation.
        /// </param>
        public async Task Drop(IDropInfo dropInfo)
        {
            var droptarget = dropInfo.TargetItem as IDropTarget;

            if (droptarget == null)
            {
                return;
            }

            try
            {
                this.IsBusy = true;
                await droptarget.Drop(dropInfo);
            }
            catch (Exception ex)
            {
                this.Feedback = ex.Message;
            }
            finally
            {
                this.IsBusy = false;
            }
        }
    }
}
