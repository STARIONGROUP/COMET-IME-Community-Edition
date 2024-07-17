// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RibbonButtonSessionDependentViewModel.cs" company="Starion Group S.A.">
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

namespace CDP4Composition.Mvvm
{
    using System;
    using System.Linq;
    using System.Reactive;
    using System.Windows.Input;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;
    using CDP4Dal.Events;

    using ReactiveUI;

    /// <summary>
    /// The base class representing Ribbon button that depends on <see cref="ISession"/> to open a <see cref="IPanelView"/>
    /// </summary>
    public abstract class RibbonButtonSessionDependentViewModel : ReactiveObject
    {
        /// <summary>
        /// The Function returning an instance of <see cref="IPanelViewModel"/>
        /// </summary>
        protected readonly Func<ISession, IThingDialogNavigationService, IPanelNavigationService, IDialogNavigationService, IPluginSettingsService, IPanelViewModel> InstantiatePanelViewModelFunction;

        /// <summary>
        /// Backing field for <see cref="HasSession"/>
        /// </summary>
        private bool hasSession = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="RibbonButtonSessionDependentViewModel"/> class
        /// </summary>
        protected RibbonButtonSessionDependentViewModel(Func<ISession, IThingDialogNavigationService, IPanelNavigationService, IDialogNavigationService, IPluginSettingsService, IPanelViewModel> instantiatePanelViewModel, ICDPMessageBus messageBus)
        {
            this.InstantiatePanelViewModelFunction = instantiatePanelViewModel;

            this.OpenSessions = new ReactiveList<RibbonMenuItemViewModelBase>();
            this.OpenSessions.CountChanged.Subscribe(x => this.HasSession = x != 0);

            this.OpenSingleBrowserCommand = ReactiveCommandCreator.Create(this.ExecuteOpenSingleBrowser);

            messageBus.Listen<SessionEvent>().Subscribe(this.SessionChangeEventHandler);
        }

        /// <summary>
        /// Gets a value indicating whether there are open sessions
        /// </summary>
        public bool HasSession
        {
            get => this.hasSession;
            set => this.RaiseAndSetIfChanged(ref this.hasSession, value);
        }

        /// <summary>
        /// Gets the open or close the browser
        /// </summary>
        public ReactiveCommand<Unit, Unit> OpenSingleBrowserCommand { get; private set; }

        /// <summary>
        /// Gets the list of open sessions
        /// </summary>
        public ReactiveList<RibbonMenuItemViewModelBase> OpenSessions { get; private set; }

        /// <summary>
        /// Removes the <see cref="RibbonMenuItemViewModelBase"/> associated with the closed <see cref="ISession"/>
        /// </summary>
        /// <param name="session">The closed <see cref="ISession"/></param>
        protected void RemoveOpenSession(ISession session)
        {
            var currentSession = this.OpenSessions.Single(x => x.Session == session);
            ((ICommand)currentSession.ClosePanelsCommand).Execute(default);

            var sessionToRemove = this.OpenSessions.SingleOrDefault(x => x.Session == session);

            if (sessionToRemove != null)
            {
                this.OpenSessions.Remove(sessionToRemove);
            }
        }

        /// <summary>
        /// Executes the <see cref="OpenSingleBrowserCommand"/> command
        /// </summary>
        private void ExecuteOpenSingleBrowser()
        {
            if (this.OpenSessions.Count != 1)
            {
                return;
            }

            ((ICommand)this.OpenSessions.Single().ShowPanelCommand).Execute(default);
        }

        /// <summary>
        /// The event-handler that is invoked by the subscription that listens for updates
        /// on the <see cref="ISession"/> that is being represented by the view-model
        /// </summary>
        /// <param name="sessionChange">
        /// The payload of the event that is being handled
        /// </param>
        protected virtual void SessionChangeEventHandler(SessionEvent sessionChange)
        {
            if (sessionChange.Status == SessionStatus.Open)
            {
                this.OpenSessions.Add(new RibbonMenuItemSessionDependentViewModel(sessionChange.Session.Name, sessionChange.Session, this.InstantiatePanelViewModelFunction));
            }
            else if (sessionChange.Status == SessionStatus.Closed)
            {
                this.RemoveOpenSession(sessionChange.Session);
            }
        }
    }
}
