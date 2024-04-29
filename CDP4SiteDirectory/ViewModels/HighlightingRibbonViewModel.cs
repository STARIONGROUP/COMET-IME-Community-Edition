// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HighlightingRibbonViewModel.cs" company="Starion Group S.A.">
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

namespace CDP4SiteDirectory.ViewModels
{
    using System;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;

    using CDP4Composition.Events;
    using CDP4Composition.Mvvm;

    using CDP4Dal;
    using CDP4Dal.Events;

    using ReactiveUI;

    /// <summary>
    /// The highlighting ribbon viewmodel.
    /// </summary>
    public class HighlightingRibbonViewModel : ReactiveObject
    {
        /// <summary name="messageBus">
        /// The <see cref="ICDPMessageBus"/>
        /// </summary>
        private readonly ICDPMessageBus messageBus;

        /// <summary>
        /// Backing field for <see cref="HasSession"/>
        /// </summary>
        private readonly ObservableAsPropertyHelper<bool> hasSession;

        /// <summary>
        /// The open <see cref="ISession"/>s
        /// </summary>
        private readonly ReactiveList<ISession> openSessions;

        /// <summary>
        /// Initializes a new instance of the <see cref="HighlightingRibbonViewModel"/> class
        /// </summary>
        /// <param name="messageBus">
        /// The <see cref="ICDPMessageBus"/>
        /// </param>
        public HighlightingRibbonViewModel(ICDPMessageBus messageBus)
        {
            this.messageBus = messageBus;
            this.openSessions = new ReactiveList<ISession>();
            this.openSessions.CountChanged.Select(x => x != 0).ToProperty(this, x => x.HasSession, out this.hasSession, scheduler: RxApp.MainThreadScheduler);

            this.messageBus.Listen<SessionEvent>().Subscribe(this.SessionChangeEventHandler);

            this.ClearHighlightingCommand = ReactiveCommandCreator.Create(this.ExecuteClearHighlightingCommand);
        }

        /// <summary>
        /// Gets a value indicating whether there are open <see cref="ISession"/>s
        /// </summary>
        public bool HasSession => this.hasSession.Value;

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> to clear highlighting.
        /// </summary>
        public ReactiveCommand<Unit, Unit> ClearHighlightingCommand { get; private set; }

        /// <summary>
        /// The event-handler that is invoked by the subscription that listens for updates
        /// on the <see cref="ISession"/> that is being represented by the view-model
        /// </summary>
        /// <param name="sessionChange">
        /// The payload of the event that is being handled
        /// </param>
        private void SessionChangeEventHandler(SessionEvent sessionChange)
        {
            switch (sessionChange.Status)
            {
                case SessionStatus.Open:
                    this.openSessions.Add(sessionChange.Session);
                    break;
                case SessionStatus.Closed:
                {
                    var sessionToRemove = this.openSessions.SingleOrDefault(x => x == sessionChange.Session);

                    if (sessionToRemove != null)
                    {
                        this.openSessions.Remove(sessionToRemove);
                    }
                }

                    break;
            }
        }

        /// <summary>
        /// Executes the <see cref="ExecuteClearHighlightingCommand"/> to clear all row highlighting.
        /// </summary>
        private void ExecuteClearHighlightingCommand()
        {
            // clear all highlights
            this.messageBus.SendMessage(new CancelHighlightEvent());
        }
    }
}
