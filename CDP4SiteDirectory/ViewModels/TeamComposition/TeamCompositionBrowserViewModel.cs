// -------------------------------------------------------------------------------------------------
// <copyright file="TeamCompositionBrowserViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2020 Starion Group S.A.
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
    /// The view-model  of the Team Composition Browser that shows all the persons that make
    /// up a team of an <see cref="EngineeringModelSetup"/>
    /// </summary>
    public class TeamCompositionBrowserViewModel : BrowserViewModelBase<EngineeringModelSetup>, IPanelViewModel
    {
        /// <summary>
        /// Backing field for the <see cref="CurrentModel"/> property.
        /// </summary>
        private string currentModel;

        /// <summary>
        /// Backing field for the <see cref="Participants"/> property.
        /// </summary>
        private readonly DisposableReactiveList<TeamCompositionCardViewModel> participants = new DisposableReactiveList<TeamCompositionCardViewModel>();

        /// <summary>
        /// Backing field for <see cref="CanCreateParticipant"/>
        /// </summary>
        private bool canCreateParticipant;

        /// <summary>
        /// The Panel Caption
        /// </summary>
        private const string PanelCaption = "Team Composition";

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamCompositionBrowserViewModel"/> class
        /// </summary>
        /// <param name="thing">
        /// The thing.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> containing the given <see cref="SiteDirectory"/>
        /// </param>
        /// <param name="thingDialogNavigationService">
        /// The <see cref="IThingDialogNavigationService"/> that allows to navigate to <see cref="Thing"/> dialog view models
        /// </param>
        /// <param name="panelNavigationService">
        /// The <see cref="IPanelNavigationService"/>
        /// The <see cref="IPanelNavigationService"/> that allows to navigate to Panels
        /// </param>
        /// <param name="dialogNavigationService">
        /// The <see cref="IDialogNavigationService"/>
        /// </param>
        /// <param name="pluginSettingsService">
        /// The <see cref="IPluginSettingsService"/> used to read and write plugin setting files.
        /// </param>

        public TeamCompositionBrowserViewModel(EngineeringModelSetup thing, ISession session, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
            : base(thing, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService, pluginSettingsService)
        {
            this.Caption = string.Format("{0}, {1}", PanelCaption, this.Thing.Name);
            this.ToolTip = string.Format("{0}\n{1}\n{2}", this.Thing.Name, this.Thing.IDalUri, this.Session.ActivePerson.Name);
        }

        /// <summary>
        /// Gets the current model caption to be displayed in the browser
        /// </summary>
        public string CurrentModel
        {
            get { return this.currentModel; }
            private set { this.RaiseAndSetIfChanged(ref this.currentModel, value); }
        }

        /// <summary>
        /// Gets the <see cref="TeamCompositionCardViewModel"/>s that represent the <see cref="Participant"/>s that are contained by the <see cref="EngineeringModelSetup"/>.
        /// </summary>
        public DisposableReactiveList<TeamCompositionCardViewModel> Participants
        {
            get
            {
                return this.participants;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the create command is enabled
        /// </summary>
        public bool CanCreateParticipant
        {
            get { return this.canCreateParticipant; }
            private set { this.RaiseAndSetIfChanged(ref this.canCreateParticipant, value); }
        }

        /// <summary>
        /// Gets or sets the dock layout group target name to attach this panel to on opening
        /// </summary>
        public string TargetName { get; set; } = LayoutGroupNames.DocumentContainer;

        /// <summary>
        /// Loads the <see cref="Participant"/>s from the <see cref="EngineeringModelSetup"/> when
        /// the view-model is initialized.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            this.CurrentModel = this.Thing.Name;
            
            foreach (var participant in this.Thing.Participant)
            {
                this.AddParticipantRowViewModel(participant);
            }
        }

        /// <summary>
        /// Initialize the commands
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            this.CreateCommand = ReactiveCommandCreator.Create(() => this.ExecuteCreateCommand<Participant>(this.Thing), this.WhenAnyValue(x => x.CanCreateParticipant));
        }

        /// <summary>
        /// Compute the permisssion
        /// </summary>
        public override void ComputePermission()
        {
            base.ComputePermission();
            this.CanCreateParticipant = this.PermissionService.CanWrite(ClassKind.Participant, this.Thing);
        }

        /// <summary>
        /// Populate the context menu
        /// </summary>
        public override void PopulateContextMenu()
        {
            base.PopulateContextMenu();

            this.ContextMenu.Add(new ContextMenuItemViewModel("Create a Participant", "", this.CreateCommand, MenuItemKind.Create, ClassKind.Participant));
        }

        /// <summary>
        /// The event-handler that is invoked by the subscription that listens for updates
        /// on the <see cref="Thing"/> that is being represented by the view-model
        /// </summary>
        /// <param name="objectChange">
        /// The payload of the event that is being handled
        /// </param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);

            this.CurrentModel = this.Thing.Name;

            this.UpdateParticipantCards();
        }
        
        /// <summary>
        /// Add and Remove <see cref="TeamCompositionCardViewModel"/>s
        /// </summary>
        private void UpdateParticipantCards()
        {
            var newParticipants = this.Thing.Participant.Except(this.Participants.Select(x => x.Thing)).ToList();
            var oldParticipants = this.Participants.Select(x => x.Thing).Except(this.Thing.Participant).ToList();

            foreach (var participant in newParticipants)
            {
                this.AddParticipantRowViewModel(participant);
            }

            foreach (var participant in oldParticipants)
            {
                this.RemoveParticipantRowViewModel(participant);
            }
        }

        /// <summary>
        /// Add a <see cref="ParticipantRowViewModel"/>.
        /// </summary>
        /// <param name="participant">
        /// The <see cref="Participant"/> for which a row needs to be added.
        /// </param>
        private void AddParticipantRowViewModel(Participant participant)
        {
            var teamCompositionCardViewModel = new TeamCompositionCardViewModel(participant, this.Session, this);
            this.participants.Add(teamCompositionCardViewModel);            
        }

        /// <summary>
        /// Remove a <see cref="ParticipantRowViewModel"/>.
        /// </summary>
        /// <param name="participant">
        /// The <see cref="Participant"/> for which a row needs to be deleted.
        /// </param>
        private void RemoveParticipantRowViewModel(Participant participant)
        {
            var rowViewModel = this.participants.SingleOrDefault(x => x.Thing == participant);
            if (rowViewModel != null)
            {
                this.participants.RemoveAndDispose(rowViewModel);
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
            foreach (var card in this.participants)
            {
                card.Dispose();
            }
        }
    }
}