// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GlossaryBrowserViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BasicRdl.ViewModels
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

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
    
    using NLog;
    
    using ReactiveUI;

    /// <summary>
    /// The <see cref="GlossaryBrowserViewModel"/> is a View Model that is responsible for managing the data and interactions with that data for a view
    /// that shows all the <see cref="Glossary"/>s contained by a data-source following the containment tree that is modeled in 10-25 and the CDP4 extensions.
    /// </summary>
    public class GlossaryBrowserViewModel : BrowserViewModelBase<SiteDirectory>, IPanelViewModel, IDropTarget,
        IDeprecatableBrowserViewModel
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The Panel Caption
        /// </summary>
        private const string PanelCaption = "Glossaries";

        /// <summary>
        /// Backing field for <see cref="CanCreateTerm"/>
        /// </summary>
        private bool canCreateTerm;

        /// <summary>
        /// Backing field for <see cref="CanCreateGlossary"/>
        /// </summary>
        private bool canCreateGlossary;

        /// <summary>
        /// Initializes a new instance of the <see cref="GlossaryBrowserViewModel"/> class.
        /// </summary>
        /// <param name="session">the associated session</param>
        /// <param name="siteDir">The unique <see cref="SiteDirectory"/></param>
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService"/></param>
        /// <param name="panelNavigationService">The <see cref="IPanelNavigationService"/></param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/></param>
        /// <param name="pluginSettingsService">
        /// The <see cref="IPluginSettingsService"/> used to read and write plugin setting files.
        /// </param>
        public GlossaryBrowserViewModel(ISession session, SiteDirectory siteDir,
            IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService,
            IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
            : base(siteDir, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService,
                pluginSettingsService)
        {
            this.Caption = $"{PanelCaption}, {this.Thing.Name}";
            this.ToolTip = $"{this.Thing.Name}\n{this.Thing.IDalUri}\n{this.Session.ActivePerson.Name}";

            this.AddSubscriptions();
        }

        /// <summary>
        /// Gets the <see cref="GlossaryRowViewModel"/> that are contained by this view-model
        /// </summary>
        public DisposableReactiveList<GlossaryRowViewModel> Glossaries { get; private set; }

        /// <summary>
        /// Gets a value indicating whether a <see cref="Term"/> can be created
        /// </summary>
        public bool CanCreateTerm
        {
            get => this.canCreateTerm;
            private set => this.RaiseAndSetIfChanged(ref this.canCreateTerm, value);
        }

        /// <summary>
        /// Gets a value indicating whether a <see cref="Glossary"/> can be created
        /// </summary>
        public bool CanCreateGlossary
        {
            get => this.canCreateGlossary;
            private set => this.RaiseAndSetIfChanged(ref this.canCreateGlossary, value);
        }

        /// <summary>
        /// Gets the <see cref="Term"/> Creation <see cref="ICommand"/>
        /// </summary>
        public ReactiveCommand<object> CreateTermCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="Glossary"/> Creation <see cref="ICommand"/>
        /// </summary>
        public ReactiveCommand<object> CreateGlossaryCommand { get; private set; }

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
                CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(Glossary))
                    .Where(objectChange => objectChange.EventKind == EventKind.Added &&
                                           objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                    .Select(x => x.ChangedThing as Glossary)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(this.AddGlossaryRowViewModel);
            this.Disposables.Add(addListener);

            var removeListener =
                CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(Glossary))
                    .Where(objectChange => objectChange.EventKind == EventKind.Removed &&
                                           objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                    .Select(x => x.ChangedThing as Glossary)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(this.RemoveGlossaryRowViewModel);
            this.Disposables.Add(removeListener);

            var rdlUpdateListener =
                CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(ReferenceDataLibrary))
                    .Where(objectChange => objectChange.EventKind == EventKind.Updated &&
                                           objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                    .Select(x => x.ChangedThing as ReferenceDataLibrary)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(this.RefreshContainerName);
            this.Disposables.Add(rdlUpdateListener);
        }

        /// <summary>
        /// Adds a <see cref="GlossaryRowViewModel"/>
        /// </summary>
        /// <param name="glossary">The associated <see cref="Glossary"/></param>
        private void AddGlossaryRowViewModel(Glossary glossary)
        {
            if (this.Glossaries.Any(rowViewModel => rowViewModel.Thing == glossary))
            {
                return;
            }

            var row = new GlossaryRowViewModel(glossary, this.Session, this);
            this.Glossaries.Add(row);
        }

        /// <summary>
        /// Removes a <see cref="GlossaryRowViewModel"/>
        /// </summary>
        /// <param name="glossary">The associated <see cref="Glossary"/></param>
        private void RemoveGlossaryRowViewModel(Glossary glossary)
        {
            var row = this.Glossaries.SingleOrDefault(x => x.Thing == glossary);
            if (row != null)
            {
                this.Glossaries.RemoveAndDispose(row);
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
            foreach (var glossary in this.Glossaries)
            {
                if (glossary.Thing.Container != rdl)
                {
                    continue;
                }

                if (glossary.ContainerRdlShortName != rdl.ShortName)
                {
                    glossary.ContainerRdlShortName = rdl.ShortName;
                }
            }
        }

        /// <summary>
        /// Loads the <see cref="Glossary"/>s from the cache when the browser is instantiated.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.Glossaries = new DisposableReactiveList<GlossaryRowViewModel>();

            var openDataLibrariesIids = this.Session.OpenReferenceDataLibraries.Select(y => y.Iid);
            foreach (var referenceDataLibrary in this.Thing.AvailableReferenceDataLibraries()
                .Where(x => openDataLibrariesIids.Contains(x.Iid)))
            {
                foreach (var glossary in referenceDataLibrary.Glossary)
                {
                    this.AddGlossaryRowViewModel(glossary);
                }
            }
        }

        /// <summary>
        /// Initialize the <see cref="ReactiveCommand"/>s
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            this.CreateTermCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanCreateTerm));
            this.CreateTermCommand.Subscribe(_ =>
                this.ExecuteCreateCommand<Term>(this.SelectedThing.Thing as Glossary ??
                                                this.SelectedThing.Thing.GetContainerOfType<Glossary>()));

            this.CreateGlossaryCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanCreateGlossary));
            this.CreateGlossaryCommand.Subscribe(_ => this.ExecuteCreateCommand<Glossary>());
        }

        /// <summary>
        /// Execute the <see cref="BrowserViewModelBase{T}.ExportCommand"/>
        /// </summary>
        protected override void ExecuteExportCommand()
        {
            var dialogViewModel =
                new HtmlExportGlossarySelectionDialogViewModel(this.Glossaries.Select(grvm => grvm.Thing).ToList());
            this.DialogNavigationService.NavigateModal(dialogViewModel);
        }

        /// <summary>
        /// Compute the permissions
        /// </summary>
        public override void ComputePermission()
        {
            base.ComputePermission();
            this.CanCreateGlossary = this.Session.OpenReferenceDataLibraries.Any();

            if (this.SelectedThing == null)
            {
                return;
            }

            this.CanCreateTerm = this.PermissionService.CanWrite(ClassKind.Term,
                this.SelectedThing.Thing as Glossary ?? this.SelectedThing.Thing.GetContainerOfType<Glossary>());
        }

        /// <summary>
        /// Populate the context menu
        /// </summary>
        public override void PopulateContextMenu()
        {
            base.PopulateContextMenu();

            this.ContextMenu.Add(new ContextMenuItemViewModel("Create a Glossary", "", this.CreateGlossaryCommand,
                MenuItemKind.Create, ClassKind.Glossary));

            if (this.SelectedThing == null)
            {
                return;
            }

            this.ContextMenu.Add(new ContextMenuItemViewModel("Create a Term", "", this.CreateTermCommand,
                MenuItemKind.Create, ClassKind.Term));
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
            foreach (var glossary in this.Glossaries)
            {
                glossary.Dispose();
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
            if (droptarget != null)
            {
                droptarget.DragOver(dropInfo);
            }
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
            if (droptarget != null)
            {
                try
                {
                    this.IsBusy = true;
                    await droptarget.Drop(dropInfo);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    this.Feedback = ex.Message;
                }
                finally
                {
                    this.IsBusy = false;
                }
            }
        }
    }
}