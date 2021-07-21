// -------------------------------------------------------------------------------------------------
// <copyright file="SiteRdlBrowserViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2020 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.ViewModels
{
    using System;
    using System.Linq;
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
    /// The view-model for the <see cref="SiteRdlBrowserViewModel"/>
    /// </summary>
    public class SiteRdlBrowserViewModel : BrowserViewModelBase<SiteDirectory>, IPanelViewModel,
        IDeprecatableBrowserViewModel
    {
        /// <summary>
        /// Backing field for <see cref="CanCreateSiteRdl"/>
        /// </summary>
        private bool canCreateSiteRdl;

        /// <summary>
        /// The Panel Caption
        /// </summary>
        private const string PanelCaption = "Site RDLs";

        /// <summary>
        /// Initializes a new instance of the <see cref="SiteRdlBrowserViewModel"/> class
        /// </summary>
        /// <param name="session">The <see cref="ISession"/> containing the given <see cref="SiteDirectory"/></param>
        /// <param name="siteDir">The <see cref="SiteDirectory"/> containing the data of this browser</param>
        /// <param name="thingDialogNavigationService">
        /// The <see cref="IThingDialogNavigationService"/> that allows to navigate to <see cref="Thing"/> dialog view models
        /// </param>
        /// <param name="panelNavigationService">The <see cref="IPanelNavigationService"/>
        /// The <see cref="IPanelNavigationService"/> that allows to navigate to Panels
        /// </param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/></param>
        /// <param name="pluginSettingsService">
        /// The <see cref="IPluginSettingsService"/> used to read and write plugin setting files.
        /// </param>
        public SiteRdlBrowserViewModel(ISession session, SiteDirectory siteDir,
            IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService,
            IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
            : base(siteDir, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService,
                pluginSettingsService)
        {
            this.Caption = string.Format("{0}, {1}", PanelCaption, this.Thing.Name);
            this.ToolTip = string.Format("{0}\n{1}\n{2}", this.Thing.Name, this.Thing.IDalUri,
                this.Session.ActivePerson.Name);

            this.SiteRdls = new DisposableReactiveList<SiteRdlRowViewModel>();
            this.ComputeSiteRdlRows();
        }

        /// <summary>
        /// Gets the List of <see cref="SiteRdlRowViewModel"/>
        /// </summary>
        public DisposableReactiveList<SiteRdlRowViewModel> SiteRdls { get; private set; }

        /// <summary>
        /// Gest a value indicating whether the create command is enabled
        /// </summary>
        public bool CanCreateSiteRdl
        {
            get { return this.canCreateSiteRdl; }
            private set { this.RaiseAndSetIfChanged(ref this.canCreateSiteRdl, value); }
        }

        /// <summary>
        /// Gets or sets the dock layout group target name to attach this panel to on opening
        /// </summary>
        public string TargetName { get; set; } = LayoutGroupNames.LeftGroup;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// a value indicating whether the class is being disposed of
        /// </param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            foreach (var siteRdl in this.SiteRdls)
            {
                siteRdl.Dispose();
            }
        }

        /// <summary>
        /// Initialize the <see cref="ReactiveCommand"/>s of the current view-model
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            this.CreateCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanCreateSiteRdl));
            this.CreateCommand.Subscribe(_ => this.ExecuteCreateCommand<SiteReferenceDataLibrary>(this.Thing));
        }

        /// <summary>
        /// Computes the permission
        /// </summary>
        public override void ComputePermission()
        {
            base.ComputePermission();
            this.CanCreateSiteRdl = this.PermissionService.CanWrite(ClassKind.SiteReferenceDataLibrary, this.Thing);
        }

        /// <summary>
        /// Populate the context menu
        /// </summary>
        public override void PopulateContextMenu()
        {
            base.PopulateContextMenu();
            
            this.ContextMenu.Add(new ContextMenuItemViewModel("Create a Site Rdl", "", this.CreateCommand,
                MenuItemKind.Create, ClassKind.SiteReferenceDataLibrary));
        }

        /// <summary>
        /// The <see cref="ObjectChangedEvent"/> event-handler
        /// </summary>
        /// <param name="objectChange">The <see cref="ObjectChangedEvent"/></param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.ComputeSiteRdlRows();
        }

        /// <summary>
        /// Compute the <see cref="SiteReferenceDataLibrary"/> rows
        /// </summary>
        private void ComputeSiteRdlRows()
        {
            var current = this.SiteRdls.Select(x => x.Thing).ToList();
            var updated = this.Thing.SiteReferenceDataLibrary.ToList();

            var added = updated.Except(current).ToList();
            var removed = current.Except(updated).ToList();
            foreach (var siteReferenceDataLibrary in added)
            {
                this.AddSiteRdl(siteReferenceDataLibrary);
            }

            foreach (var srdl in removed)
            {
                this.RemoveSiteRdl(srdl);
            }
        }

        /// <summary>
        /// Add a row representing an <see cref="SiteReferenceDataLibrary"/>
        /// </summary>
        /// <param name="siteRdl"> The <see cref="SiteReferenceDataLibrary"/> to add
        /// </param>
        private void AddSiteRdl(SiteReferenceDataLibrary siteRdl)
        {
            var row = new SiteRdlRowViewModel(siteRdl, this.Session, this);
            this.SiteRdls.Add(row);
        }

        /// <summary>
        /// Remove a row representing an <see cref="SiteReferenceDataLibrary"/>
        /// </summary>
        /// <param name="siteRdl"> The <see cref="SiteReferenceDataLibrary"/> to remove
        /// </param>
        private void RemoveSiteRdl(SiteReferenceDataLibrary siteRdl)
        {
            var row = this.SiteRdls.SingleOrDefault(x => x.Thing == siteRdl);
            if (row != null)
            {
                this.SiteRdls.RemoveAndDispose(row);
            }
        }
    }
}