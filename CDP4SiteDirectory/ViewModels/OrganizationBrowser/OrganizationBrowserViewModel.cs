// -------------------------------------------------------------------------------------------------
// <copyright file="OrganizationBrowserViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.ViewModels
{
    using System;
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4CommonView;
    using CDP4Composition;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;
    using CDP4Dal;
    using CDP4Dal.Events;
    using ReactiveUI;

    /// <summary>
    /// The view-model for the <see cref="OrganizationBrowser"/>
    /// </summary>
    public class OrganizationBrowserViewModel : BrowserViewModelBase<SiteDirectory>, IPanelViewModel
    {
        /// <summary>
        /// Backing field for <see cref="CanCreateOrganization"/>
        /// </summary>
        private bool canCreateOrganization;

        /// <summary>
        /// The Panel Caption
        /// </summary>
        private const string PanelCaption = "Organizations";

        /// <summary>
        /// Initializes a new instance of the <see cref="OrganizationBrowserViewModel"/> class
        /// </summary>
        /// <param name="session">The <see cref="ISession"/> containing the given <see cref="SiteDirectory"/></param>
        /// <param name="siteDir">The <see cref="SiteDirectory"/> containing the data of this browser</param>
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService"/> that allows to navigate to <see cref="Thing"/> dialog view models</param>
        /// <param name="panelNavigationService">The <see cref="IPanelNavigationService"/> that allows to navigate to Panels</param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/></param>
        /// <param name="pluginSettingsService">
        /// The <see cref="IPluginSettingsService"/> used to read and write plugin setting files.
        /// </param>
        public OrganizationBrowserViewModel(ISession session, SiteDirectory siteDir, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
            : base(siteDir, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService, pluginSettingsService)
        {
            this.Caption = string.Format("{0}, {1}", PanelCaption, this.Thing.Name);
            this.ToolTip = string.Format("{0}\n{1}\n{2}", this.Thing.Name, this.Thing.IDalUri, this.Session.ActivePerson.Name);

            this.Organizations = new ReactiveList<OrganizationRowViewModel>();
            this.ComputeOrganizationRows();
        }

        /// <summary>
        /// Gets the List of <see cref="OrganizationRowViewModel"/>
        /// </summary>
        public ReactiveList<OrganizationRowViewModel> Organizations { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the create command is enable
        /// </summary>
        public bool CanCreateOrganization
        {
            get { return this.canCreateOrganization; }
            private set { this.RaiseAndSetIfChanged(ref this.canCreateOrganization, value); }
        }

        /// <summary>
        /// Update the Organization rows
        /// </summary>
        private void ComputeOrganizationRows()
        {
            var currentOrganizations = this.Organizations.Select(x => x.Thing).ToList();
            var definedOrganizations = this.Thing.Organization;

            var removedOrganization = currentOrganizations.Except(definedOrganizations).ToList();
            var addedOrganization = this.Thing.Organization.Except(currentOrganizations).ToList();

            foreach (var organization in removedOrganization)
            {
                this.RemoveOrganization(organization);
            }

            foreach (var organization in addedOrganization)
            {
                this.AddOrganization(organization);
            }

            var updatedOrganizations = currentOrganizations.Intersect(definedOrganizations).ToList();
            foreach (var updatedOrganization in updatedOrganizations)
            {
                var row = this.Organizations.Single(x => x.Thing == updatedOrganization);
                this.UpdatePersonRows(row);
            }
        }

        /// <summary>
        /// Update the person rows for an organization
        /// </summary>
        /// <param name="organizationRow">The organization row to update</param>
        private void UpdatePersonRows(OrganizationRowViewModel organizationRow)
        {
            var currentPersons = organizationRow.ContainedRows.Select(x => x.Thing).OfType<Person>().ToList();
            var updatedPerson = this.Thing.Person.Where(x => x.Organization == organizationRow.Thing).ToList();

            var addedPersons = updatedPerson.Except(currentPersons).ToList();
            var removedPersons = currentPersons.Except(updatedPerson).ToList();

            foreach (var addedPerson in addedPersons)
            {
                var row = new OrganizationBrowser.PersonRowViewModel(addedPerson, this.Session, this);
                organizationRow.ContainedRows.Add(row);
            }

            foreach (var removedPerson in removedPersons)
            {
                var row = organizationRow.ContainedRows.SingleOrDefault(x => x.Thing == removedPerson);
                if (row != null)
                {
                    row.Dispose();
                    organizationRow.ContainedRows.Remove(row);
                }
            }

            foreach (var row in organizationRow.ContainedRows.OfType<OrganizationBrowser.PersonRowViewModel>())
            {
                row.RowStatus = (row.Thing.IsActive && !row.Thing.IsDeprecated)
                    ? RowStatusKind.Active
                    : RowStatusKind.Inactive;
            }
        }

        /// <summary>
        /// Add a row representing an <see cref="Organization"/>
        /// </summary>
        /// <param name="organization"> The <see cref="Organization"/> to add
        /// </param>
        private void AddOrganization(Organization organization)
        {
            var row = new OrganizationRowViewModel(organization, this.Session, this);
            this.UpdatePersonRows(row);
            this.Organizations.Add(row);
        }

        /// <summary>
        /// Remove a row representing an <see cref="Organization"/>
        /// </summary>
        /// <param name="organization"> The <see cref="Organization"/> to remove 
        /// </param>
        private void RemoveOrganization(Organization organization)
        {
            var row = this.Organizations.SingleOrDefault(x => x.Thing == organization);
            if (row != null)
            {
                this.Organizations.Remove(row);
                row.Dispose();
            }
        }

        /// <summary>
        /// Initialize the <see cref="ReactiveCommand"/>s of the current view-model
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            this.CreateCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanCreateOrganization));
            this.CreateCommand.Subscribe(_ => this.ExecuteCreateCommand<Organization>(this.Thing));
        }

        /// <summary>
        /// The <see cref="ObjectChangedEvent"/> handler called upon an update of the current thing
        /// </summary>
        /// <param name="objectChange">The <see cref="ObjectChangedEvent"/></param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.ComputeOrganizationRows();
        }

        /// <summary>
        /// Compute the permissions
        /// </summary>
        public override void ComputePermission()
        {
            base.ComputePermission();
            this.CanCreateOrganization = this.PermissionService.CanWrite(ClassKind.Organization, this.Thing);
        }

        /// <summary>
        /// Populate the context menu
        /// </summary>
        public override void PopulateContextMenu()
        {
            base.PopulateContextMenu();

            if (this.SelectedThing == null || this.SelectedThing.Thing is Organization)
            {
                this.ContextMenu.Add(new ContextMenuItemViewModel("Create an Organization", "", this.CreateCommand, MenuItemKind.Create, ClassKind.Organization));
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
            foreach (var organization in this.Organizations)
            {
                organization.Dispose();
            }
        }
    }
}