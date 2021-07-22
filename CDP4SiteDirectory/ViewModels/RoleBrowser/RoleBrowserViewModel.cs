// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RoleBrowserViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2020 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.ViewModels
{
    using System;
    using System.Linq;
    using System.Windows.Controls;
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
    /// The purpose of the <see cref="RoleBrowserViewModel"/> is to represent the view-model for <see cref="CDP4Common.SiteDirectoryData.Rule"/>s
    /// </summary>
    public class RoleBrowserViewModel : BrowserViewModelBase<SiteDirectory>, IPanelViewModel, IDeprecatableBrowserViewModel
    {
        /// <summary>
        /// The <see cref="CDP4Common.SiteDirectoryData.PersonRole"/> folder row
        /// </summary>
        private FolderRowViewModel personRoleRow;

        /// <summary>
        /// The <see cref="CDP4Common.SiteDirectoryData.ParticipantRole"/> folder row
        /// </summary>
        private FolderRowViewModel participantRoleRow;

        /// <summary>
        /// The Panel Caption
        /// </summary>
        private const string PanelCaption = "Roles";

        /// <summary>
        /// Backing field for <see cref="CanCreatePersonRole"/>
        /// </summary>
        private bool canCreatePersonRole;

        /// <summary>
        /// Backing field for <see cref="canCreateParticipantRole"/>
        /// </summary>
        private bool canCreateParticipantRole;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoleBrowserViewModel"/> class.
        /// </summary>
        /// <param name="session">the associated session</param>
        /// <param name="siteDir">The unique <see cref="SiteDirectory"/></param>
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService"/> that allows to navigate to <see cref="CDP4Common.CommonData.Thing"/> dialog view models</param>
        /// <param name="panelNavigationService">The <see cref="IPanelNavigationService"/> that allows to navigate to Panels</param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/></param>
        /// <param name="pluginSettingsService">
        /// The <see cref="IPluginSettingsService"/> used to read and write plugin setting files.
        /// </param>
        public RoleBrowserViewModel(ISession session, SiteDirectory siteDir, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
            : base(siteDir, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService, pluginSettingsService)
        {
            this.Caption = string.Format("{0}, {1}", PanelCaption, this.Thing.Name);
            this.ToolTip = string.Format("{0}\n{1}\n{2}", this.Thing.Name, this.Thing.IDalUri, this.Session.ActivePerson.Name);
        }

        /// <summary>
        /// Gets the <see cref="FolderRowViewModel"/> that are displayed by the TreeListControl
        /// </summary>
        public DisposableReactiveList<FolderRowViewModel> Roles { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="ISession.ActivePerson"/> can create <see cref="PersonRole"/>s.
        /// </summary>
        public bool CanCreatePersonRole
        {
            get { return this.canCreatePersonRole; }
            set { this.RaiseAndSetIfChanged(ref this.canCreatePersonRole, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="ISession.ActivePerson"/> can create <see cref="ParticipantRole"/>s.
        /// </summary>
        public bool CanCreateParticipantRole
        {
            get { return this.canCreateParticipantRole; }
            set { this.RaiseAndSetIfChanged(ref this.canCreateParticipantRole, value); }
        }

        /// <summary>
        /// Gets or sets the dock layout group target name to attach this panel to on opening
        /// </summary>
        public string TargetName { get; set; } = LayoutGroupNames.RightGroup;

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to create a <see cref="CDP4Common.SiteDirectoryData.PersonRole"/>
        /// </summary>
        public ReactiveCommand<object> CreatePersonRoleCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to create a <see cref="CDP4Common.SiteDirectoryData.ParticipantRole"/>
        /// </summary>
        public ReactiveCommand<object> CreateParticipantRoleCommand { get; private set; }

        /// <summary>
        /// Initialize the browser
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.participantRoleRow = new FolderRowViewModel("", "Participant Role", this.Session, this);
            this.personRoleRow = new FolderRowViewModel("", "Person Role", this.Session, this);
            this.Roles = new DisposableReactiveList<FolderRowViewModel> { this.personRoleRow, this.participantRoleRow };
            this.PopulateParticipantRoles();
            this.PopulatePersonRoles();
        }

        /// <summary>
        /// Compute the permission
        /// </summary>
        public override void ComputePermission()
        {
            base.ComputePermission();
            this.CanCreateParticipantRole = this.PermissionService.CanWrite(ClassKind.ParticipantRole, this.Thing);
            this.CanCreatePersonRole = this.PermissionService.CanWrite(ClassKind.PersonRole, this.Thing);
        }

        /// <summary>
        /// Populate the <see cref="ContextMenu"/> of the current browser
        /// </summary>
        public override void PopulateContextMenu()
        {
            base.PopulateContextMenu();

            if (this.SelectedThing is PersonPermissionRowViewModel ||
               this.SelectedThing is ParticipantPermissionRowViewModel)
            {
                this.ContextMenu.Clear();
                return;
            }

            this.ContextMenu.Add(new ContextMenuItemViewModel("Create a Person Role", "", this.CreatePersonRoleCommand, MenuItemKind.Create, ClassKind.PersonRole));
            this.ContextMenu.Add(new ContextMenuItemViewModel("Create a Participant Role", "", this.CreateParticipantRoleCommand, MenuItemKind.Create, ClassKind.ParticipantRole));
        }

        /// <summary>
        /// Initialize the <see cref="ReactiveCommand"/>s of the current view-model
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            var isCreatePersonRoleAllowed = this.WhenAnyValue(vm => vm.CanCreatePersonRole);
            this.CreatePersonRoleCommand = ReactiveCommand.Create(isCreatePersonRoleAllowed);
            this.CreatePersonRoleCommand.Subscribe(_ => this.ExecuteCreateCommand<PersonRole>(this.Thing));

            var isCreateParticipantRoleAllowed = this.WhenAnyValue(vm => vm.CanCreateParticipantRole);
            this.CreateParticipantRoleCommand = ReactiveCommand.Create(isCreateParticipantRoleAllowed);
            this.CreateParticipantRoleCommand.Subscribe(_ => this.ExecuteCreateCommand<ParticipantRole>(this.Thing));
        }

        /// <summary>
        /// The update event-handler for this <see cref="SiteDirectory"/>
        /// </summary>
        /// <param name="objectChange">The <see cref="ObjectChangedEvent"/></param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.PopulateParticipantRoles();
            this.PopulatePersonRoles();
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
            foreach (var session in this.Roles)
            {
                session.Dispose();
            }
        }

        /// <summary>
        /// Populate the <see cref="PersonRole"/> rows
        /// </summary>
        private void PopulatePersonRoles()
        {
            var current = this.personRoleRow.ContainedRows.Select(x => (PersonRole)x.Thing).ToList();
            var updated = this.Session.RetrieveSiteDirectory().PersonRole;

            var newRole = updated.Except(current).ToList();
            var oldRole = current.Except(updated).ToList();

            foreach (var personRole in oldRole)
            {
                var row = this.personRoleRow.ContainedRows.Single(x => x.Thing == personRole);
                this.personRoleRow.ContainedRows.RemoveAndDispose(row);
            }

            foreach (var personRole in newRole)
            {
                var row = new PersonRoleRowViewModel(personRole, this.Session, this);
                this.personRoleRow.ContainedRows.Add(row);
            }

            var orderedCollection = this.personRoleRow.ContainedRows.OfType<PersonRoleRowViewModel>().OrderBy(x => x.Thing.Name).ToArray();
            this.personRoleRow.ContainedRows.ClearWithoutDispose();
            this.personRoleRow.ContainedRows.AddRange(orderedCollection);
        }

        /// <summary>
        /// Populate the <see cref="ParticipantRole"/> rows
        /// </summary>
        private void PopulateParticipantRoles()
        {
            var current = this.participantRoleRow.ContainedRows.Select(x => (ParticipantRole)x.Thing).ToList();
            var updated = this.Session.RetrieveSiteDirectory().ParticipantRole;

            var newRole = updated.Except(current).ToList();
            var oldRole = current.Except(updated).ToList();

            foreach (var role in oldRole)
            {
                var row = this.participantRoleRow.ContainedRows.Single(x => x.Thing == role);
                this.participantRoleRow.ContainedRows.RemoveAndDispose(row);
            }

            foreach (var role in newRole)
            {
                var row = new ParticipantRoleRowViewModel(role, this.Session, this);
                this.participantRoleRow.ContainedRows.Add(row);
            }

            var orderedCollection = this.participantRoleRow.ContainedRows.OfType<ParticipantRoleRowViewModel>().OrderBy(x => x.Thing.Name).ToArray();
            this.participantRoleRow.ContainedRows.ClearWithoutDispose();
            this.participantRoleRow.ContainedRows.AddRange(orderedCollection);
        }
    }
}