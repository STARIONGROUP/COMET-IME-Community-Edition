// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TeamCompositionBrowserViewModel.cs" company="Starion Group S.A.">
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

namespace CDP4SiteDirectory.ViewModels
{
    using System;
    using System.Linq;
    using System.Reactive;
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
        /// Gets the <see cref="ReactiveCommand"/> to create multiple <see cref="Participant"/>s at once
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateMultipleParticipantsCommand { get; private set; }

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

            this.CreateMultipleParticipantsCommand = ReactiveCommandCreator.CreateAsyncTask(
                this.ExecuteCreateMultipleParticipantsCommand,
                this.WhenAnyValue(x => x.CanCreateParticipant));
        }

        /// <summary>
        /// Executes the <see cref="CreateMultipleParticipantsCommand"/> by presenting the bulk participant creation
        /// dialog and persisting the selected <see cref="Participant"/>s in a single transaction.
        /// </summary>
        /// <returns>
        /// an awaitable <see cref="Task"/>
        /// </returns>
        private async Task ExecuteCreateMultipleParticipantsCommand()
        {
            var modelSetup = this.Thing;

            var siteDirectory = this.Session.RetrieveSiteDirectory();

            var eligiblePersons = siteDirectory.Person
                .Where(person => !person.IsDeprecated)
                .Except(modelSetup.Participant.Select(p => p.Person))
                .ToList();

            if (!eligiblePersons.Any())
            {
                return;
            }

            var dialogViewModel = new BulkParticipantCreationDialogViewModel(eligiblePersons, siteDirectory.ParticipantRole, modelSetup.ActiveDomain);

            var result = this.DialogNavigationService.NavigateModal(dialogViewModel) as BulkParticipantCreationResult;

            if (result?.Result != true || result.Participants == null || !result.Participants.Any())
            {
                return;
            }

            var context = TransactionContextResolver.ResolveContext(modelSetup);
            var modelSetupClone = modelSetup.Clone(false);
            var transaction = new ThingTransaction(context, modelSetupClone);

            foreach (var row in result.Participants)
            {
                var participant = new Participant
                {
                    Iid = Guid.NewGuid(),
                    Person = row.Person,
                    Role = row.SelectedRole,
                    SelectedDomain = row.SelectedDomain,
                    IsActive = row.IsActive
                };

                participant.Domain.Add(row.SelectedDomain);

                modelSetupClone.Participant.Add(participant);
                transaction.Create(participant, modelSetupClone);
            }

            await this.Session.Write(transaction.FinalizeTransaction());
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
            this.ContextMenu.Add(new ContextMenuItemViewModel("Create Multiple Participants", "", this.CreateMultipleParticipantsCommand, MenuItemKind.Create, ClassKind.Participant));
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