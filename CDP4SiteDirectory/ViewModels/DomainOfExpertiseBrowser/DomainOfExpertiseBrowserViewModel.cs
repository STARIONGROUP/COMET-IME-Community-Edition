// -------------------------------------------------------------------------------------------------
// <copyright file="DomainOfExpertiseBrowserViewModel.cs" company="RHEA System S.A.">
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
    /// represents a view-model for the <see cref="DomainOfExpertise"/>s in a <see cref="SiteDirectory"/>
    /// </summary>
    public class DomainOfExpertiseBrowserViewModel : BrowserViewModelBase<SiteDirectory>, IPanelViewModel,
        IDeprecatableBrowserViewModel
    {
        #region Fields

        /// <summary>
        /// The row comparer
        /// </summary>
        private static DomainRowComparer rowComparer = new DomainRowComparer();

        /// <summary>
        /// Backing field for <see cref="CanCreateDomain"/>
        /// </summary>
        private bool canCreateDomain;

        /// <summary>
        /// The Panel Caption
        /// </summary>
        private const string PanelCaption = "Domains of Expertise";

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainOfExpertiseBrowserViewModel"/> class
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
        public DomainOfExpertiseBrowserViewModel(ISession session, SiteDirectory siteDir,
            IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService,
            IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
            : base(siteDir, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService,
                pluginSettingsService)
        {
            this.Caption = string.Format("{0}, {1}", PanelCaption, this.Thing.Name);
            this.ToolTip = string.Format("{0}\n{1}\n{2}", this.Thing.Name, this.Thing.IDalUri,
                this.Session.ActivePerson.Name);

            this.DomainOfExpertises = new ReactiveList<DomainOfExpertiseRowViewModel>();
            this.ComputeDomains();
        }

        #region Properties

        /// <summary>
        /// Gets a value indicating whether the create command is enable
        /// </summary>
        public bool CanCreateDomain
        {
            get { return this.canCreateDomain; }
            private set { this.RaiseAndSetIfChanged(ref this.canCreateDomain, value); }
        }

        /// <summary>
        /// Gets the List of <see cref="DomainOfExpertiseRowViewModel"/>
        /// </summary>
        public ReactiveList<DomainOfExpertiseRowViewModel> DomainOfExpertises { get; private set; }

        #endregion

        #region browser base

        /// <summary>
        /// Initialize the <see cref="ReactiveCommand"/>s of the current view-model
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            this.CreateCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanCreateDomain));
            this.CreateCommand.Subscribe(_ => this.ExecuteCreateCommand<DomainOfExpertise>(this.Thing));
        }

        /// <summary>
        /// The <see cref="ObjectChangedEvent"/> event-handler
        /// </summary>
        /// <param name="objectChange">The <see cref="ObjectChangedEvent"/></param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.ComputeDomains();
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
            foreach (var domain in this.DomainOfExpertises)
            {
                domain.Dispose();
            }
        }

        /// <summary>
        /// Compute the permission for the commands
        /// </summary>
        public override void ComputePermission()
        {
            base.ComputePermission();
            this.CanCreateDomain = this.PermissionService.CanWrite(ClassKind.DomainOfExpertise, this.Thing);
        }

        /// <summary>
        /// Populate the context menu
        /// </summary>
        public override void PopulateContextMenu()
        {
            base.PopulateContextMenu();

            if (this.SelectedThing == null || this.SelectedThing.ContainedRows.Count == 0)
            {
                this.IsExpandRowsEnabled = false;
            }
            else
            {
                this.IsExpandRowsEnabled = true;
            }

            this.ContextMenu.Add(new ContextMenuItemViewModel("Create a Domain of Expertise", "", this.CreateCommand,
                MenuItemKind.Create, ClassKind.DomainOfExpertise));
        }

        #endregion

        /// <summary>
        /// Add a new row that represents a <see cref="DomainOfExpertise"/> to the list of <see cref="DomainOfExpertise"/>.
        /// </summary>
        /// <param name="domainOfExpertise">
        /// The <see cref="DomainOfExpertise"/> that is to be added.
        /// </param>
        private void AddDomainOfExpertise(DomainOfExpertise domainOfExpertise)
        {
            var vm = new DomainOfExpertiseRowViewModel(domainOfExpertise, this.Session, this);
            this.DomainOfExpertises.Add(vm);
        }

        /// <summary>
        /// Remove a row representing a <see cref="DomainOfExpertise"/>
        /// </summary>
        /// <param name="domainOfExpertise">
        /// The <see cref="DomainOfExpertise"/> that is to be removed.
        /// </param>
        private void RemoveDomainOfExpertise(DomainOfExpertise domainOfExpertise)
        {
            var row = this.DomainOfExpertises.SingleOrDefault(x => x.Thing == domainOfExpertise);
            if (row != null)
            {
                this.DomainOfExpertises.Remove(row);
                row.Dispose();
            }
        }

        /// <summary>
        /// computes the domain rows to add
        /// </summary>
        private void ComputeDomains()
        {
            var currentDomain = this.DomainOfExpertises.Select(x => x.Thing).ToArray();
            var updatedDomain = this.Thing.Domain.ToList();

            var removedDomain = currentDomain.Except(updatedDomain).ToArray();
            foreach (var domainOfExpertise in removedDomain)
            {
                this.RemoveDomainOfExpertise(domainOfExpertise);
            }

            var addedDomain = updatedDomain.Except(currentDomain).ToArray();
            foreach (var domainOfExpertise in addedDomain)
            {
                this.AddDomainOfExpertise(domainOfExpertise);
            }

            this.DomainOfExpertises.Sort(rowComparer);
        }
    }
}