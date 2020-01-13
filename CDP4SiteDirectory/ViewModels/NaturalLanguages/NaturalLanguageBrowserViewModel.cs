// -------------------------------------------------------------------------------------------------
// <copyright file="NaturalLanguageBrowserViewModel.cs" company="RHEA System S.A.">
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
    /// The view-model for the <see cref="NaturalLanguageBrowser"/>
    /// </summary>
    public class NaturalLanguageBrowserViewModel : BrowserViewModelBase<SiteDirectory>, IPanelViewModel
    {
        /// <summary>
        /// The Panel Caption
        /// </summary>
        private const string PanelCaption = "Natural Language Browser";

        /// <summary>
        /// Backing field for <see cref="CanCreateLanguage"/>
        /// </summary>
        private bool canCreateLanguage;

        /// <summary>
        /// Initializes a new instance of the <see cref="NaturalLanguageBrowserViewModel"/> class
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
        public NaturalLanguageBrowserViewModel(ISession session, SiteDirectory siteDir, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
            : base(siteDir, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService, pluginSettingsService)
        {
            this.Caption = PanelCaption + ", " + this.Thing.Name;
            this.ToolTip = string.Format("{0}\n{1}\n{2}", this.Thing.Name, this.Thing.IDalUri, this.Session.ActivePerson.Name);

            this.NaturalLanguageRowViewModels = new ReactiveList<NaturalLanguageRowViewModel>();

            this.ComputeNaturalLanguageRows();
        }
        
        /// <summary>
        /// Gets the <see cref="NaturalLanguageRowViewModel"/>s
        /// </summary>
        public ReactiveList<NaturalLanguageRowViewModel> NaturalLanguageRowViewModels { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the create command is enable
        /// </summary>
        public bool CanCreateLanguage
        {
            get { return this.canCreateLanguage; }
            private set { this.RaiseAndSetIfChanged(ref this.canCreateLanguage, value); }
        }
        
        /// <summary>
        /// Handles the <see cref="ObjectChangedEvent"/>
        /// </summary>
        /// <param name="objectChange">the event</param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.ComputeNaturalLanguageRows();
            this.Caption = PanelCaption + ", " + this.Thing.Name;
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
            foreach (var language in this.NaturalLanguageRowViewModels)
            {
                language.Dispose();
            }
        }

        /// <summary>
        /// Computes the permissions
        /// </summary>
        public override void ComputePermission()
        {
            base.ComputePermission();
            this.CanCreateLanguage = this.PermissionService.CanWrite(ClassKind.NaturalLanguage, this.Thing);
        }

        /// <summary>
        /// Populate the context menu
        /// </summary>
        public override void PopulateContextMenu()
        {
            base.PopulateContextMenu();
            
            this.ContextMenu.Add(new ContextMenuItemViewModel("Create a Natural Language", "", this.CreateCommand, MenuItemKind.Create, ClassKind.NaturalLanguage));
        }

        /// <summary>
        /// Initialize the <see cref="ReactiveCommand"/>s of the current view-model
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            this.CreateCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanCreateLanguage));
            this.CreateCommand.Subscribe(_ => this.ExecuteCreateCommand<NaturalLanguage>(this.Thing));
        }
        
        /// <summary>
        /// Add subscriptions to message bus events
        /// </summary>
        private void ComputeNaturalLanguageRows()
        {
            var currentLanguage = this.NaturalLanguageRowViewModels.Select(x => x.Thing).ToList();
            var updatedLanguage = this.Thing.NaturalLanguage.ToList();

            var added = updatedLanguage.Except(currentLanguage).ToList();
            var deleted = currentLanguage.Except(updatedLanguage).ToList();

            foreach (var naturalLanguage in deleted)
            {
                this.RemoveNaturalLanguageRowViewModel(naturalLanguage);
            }

            foreach (var naturalLanguage in added)
            {
                this.AddNaturalLanguageRowViewModel(naturalLanguage);
            }
        }

        /// <summary>
        /// Adds a <see cref="NaturalLanguageRowViewModel"/>
        /// </summary>
        /// <param name="naturalLanguage">The associated <see cref="NaturalLanguage"/></param>
        private void AddNaturalLanguageRowViewModel(NaturalLanguage naturalLanguage)
        {
            var row = new NaturalLanguageRowViewModel(naturalLanguage, this.Session, this);
            this.NaturalLanguageRowViewModels.Add(row);
        }

        /// <summary>
        /// Removes a <see cref="NaturalLanguageRowViewModel"/>
        /// </summary>
        /// <param name="naturalLanguage">The associated <see cref="NaturalLanguage"/></param>
        private void RemoveNaturalLanguageRowViewModel(NaturalLanguage naturalLanguage)
        {
            var row = this.NaturalLanguageRowViewModels.SingleOrDefault(x => x.Thing == naturalLanguage);
            if (row != null)
            {
                this.NaturalLanguageRowViewModels.Remove(row);
                row.Dispose();
            }
        }
    }
}