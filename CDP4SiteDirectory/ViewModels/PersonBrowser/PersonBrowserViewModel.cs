// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PersonBrowserViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.ViewModels
{
    using System;
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;
    using CDP4Dal;
    using CDP4Dal.Events;
    using ReactiveUI;

    /// <summary>
    /// The <see cref="PersonBrowserViewModel"/> is a View Model that is responsible for managing the data and interactions with that data for a view
    /// that shows the <see cref="Person"/>s and the related <see cref="Participant"/> contained by a <see cref="SiteDirectory"/>
    /// </summary>
    public class PersonBrowserViewModel : BrowserViewModelBase<SiteDirectory>, IPanelViewModel,
        IDeprecatableBrowserViewModel
    {
        /// <summary>
        /// Backing field for <see cref="CanCreatePerson"/>
        /// </summary>
        private bool canCreatePerson;

        /// <summary>
        /// The Panel Caption
        /// </summary>
        private const string PanelCaption = "Persons";

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonBrowserViewModel"/> class.
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
        public PersonBrowserViewModel(ISession session, SiteDirectory siteDir,
            IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService,
            IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
            : base(siteDir, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService,
                pluginSettingsService)
        {
            this.Caption = string.Format("{0}, {1}", PanelCaption, this.Thing.Name);
            this.ToolTip = string.Format("{0}\n{1}\n{2}", this.Thing.Name, this.Thing.IDalUri,
                this.Session.ActivePerson.Name);

            this.PersonRowViewModels = new ReactiveList<PersonRowViewModel>();
            this.UpdatePersonRows();
        }

        /// <summary>
        /// Gets the <see cref="PersonRowViewModel"/> that are contained by the row-view-model
        /// </summary>
        public ReactiveList<PersonRowViewModel> PersonRowViewModels { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the create command is enable
        /// </summary>
        public bool CanCreatePerson
        {
            get { return this.canCreatePerson; }
            private set { this.RaiseAndSetIfChanged(ref this.canCreatePerson, value); }
        }

        /// <summary>
        /// Update the person rows
        /// </summary>
        private void UpdatePersonRows()
        {
            var currentPersons = this.PersonRowViewModels.Select(x => x.Thing).ToList();
            var definedPersons = this.Thing.Person.ToList();

            var removedPersons = currentPersons.Except(definedPersons).ToList();
            foreach (var person in removedPersons)
            {
                this.RemovePerson(person);
            }

            var addedPersons = definedPersons.Except(currentPersons).ToList();
            foreach (var addedPerson in addedPersons)
            {
                this.AddPerson(addedPerson);
            }
        }

        /// <summary>
        /// Add the <see cref="Person"/> to the contained <see cref="PersonRowViewModel"/>s
        /// </summary>
        /// <param name="person">
        /// the <see cref="Person"/> that is to be added
        /// </param>
        private void AddPerson(Person person)
        {
            if (this.PersonRowViewModels.Any(x => x.Thing == person))
            {
                return;
            }

            var row = new PersonRowViewModel(person, this.Session, this);
            this.PersonRowViewModels.Add(row);
        }

        /// <summary>
        /// Remove the <see cref="Person"/> from the contained <see cref="PersonRowViewModel"/>s
        /// </summary>
        /// <param name="person">
        /// the <see cref="Person"/> to be removed
        /// </param>
        private void RemovePerson(Person person)
        {
            var row = this.PersonRowViewModels.SingleOrDefault(x => x.Thing == person);
            if (row != null)
            {
                this.PersonRowViewModels.Remove(row);
                row.Dispose();
            }
        }

        /// <summary>
        /// Initialize the <see cref="ICommand"/>s
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            this.CreateCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanCreatePerson));
            this.CreateCommand.Subscribe(_ => this.ExecuteCreateCommand<Person>(this.Thing));
        }

        /// <summary>
        /// The <see cref="ObjectChangedEvent"/> handler when the <see cref="SiteDirectory"/> is updated
        /// </summary>
        /// <param name="objectChange">The <see cref="ObjectChangedEvent"/></param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdatePersonRows();
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
            foreach (var person in this.PersonRowViewModels)
            {
                person.Dispose();
            }
        }

        /// <summary>
        /// Computes the permission
        /// </summary>
        public override void ComputePermission()
        {
            base.ComputePermission();
            this.CanCreatePerson = this.PermissionService.CanWrite(ClassKind.Person, this.Thing);
        }

        /// <summary>
        /// Populate the <see cref="PersonBrowserViewModel.ContextMenu"/>
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

            this.ContextMenu.Add(new ContextMenuItemViewModel("Create a Person", "", this.CreateCommand,
                MenuItemKind.Create, ClassKind.Person));
        }
    }
}