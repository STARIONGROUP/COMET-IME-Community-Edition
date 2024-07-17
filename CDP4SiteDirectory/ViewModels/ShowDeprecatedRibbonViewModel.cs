﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ShowDeprecatedBrowserRibbonViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2021 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Patxi Ozkoidi, Alexander van Delft
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//    Lesser General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.ViewModels
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;

    using CDP4Composition;
    using CDP4Composition.Events;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Services;

    using CDP4Dal;
    using CDP4Dal.Events;

    using CommonServiceLocator;

    using ReactiveUI;

    /// <summary>
    /// The Team-Composition Ribbon view-model 
    /// </summary>
    public class ShowDeprecatedBrowserRibbonViewModel : ReactiveObject, IDeprecatableToggleViewModel
    {
        /// <summary name="messageBus">
        /// The <see cref="ICDPMessageBus"/>
        /// </summary>
        private readonly ICDPMessageBus messageBus;

        /// <summary>
        /// The (injected) <see cref="IFilterStringService"/>
        /// </summary>
        private IFilterStringService filterStringService;

        /// <summary>
        /// Backing field for <see cref="HasSession"/>
        /// </summary>
        private readonly ObservableAsPropertyHelper<bool> hasSession;

        /// <summary>
        /// The open <see cref="ISession"/>s
        /// </summary>
        private readonly ReactiveList<ISession> openSessions;

        /// <summary>
        /// The backing field for <see cref="ShowDeprecatedThings"/> property.
        /// </summary>
        private bool showDeprecatedThings;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShowDeprecatedBrowserRibbonViewModel"/> class
        /// </summary>
        /// <param name="messageBus">
        /// The <see cref="ICDPMessageBus"/>
        /// </param>
        public ShowDeprecatedBrowserRibbonViewModel(ICDPMessageBus messageBus)
        {
            this.messageBus = messageBus;
            this.filterStringService = ServiceLocator.Current.GetInstance<IFilterStringService>();

            this.openSessions = new ReactiveList<ISession>();
            this.openSessions.CountChanged.Select(x => x != 0).ToProperty(this, x => x.HasSession, out this.hasSession, scheduler: RxApp.MainThreadScheduler);

            this.messageBus.Listen<SessionEvent>().Subscribe(this.SessionChangeEventHandler);

            this.WhenAnyValue(vm => vm.ShowDeprecatedThings)
                .Subscribe(_ => this.RefreshAndSendShowDeprecatedThingsEvent());
        }

        /// <summary>
        /// Gets a value indicating whether there are open <see cref="ISession"/>s
        /// </summary>
        public bool HasSession => this.hasSession.Value;

        /// <summary>
        /// Gets or sets a value indicating whether to display deprecated items.
        /// </summary>
        public bool ShowDeprecatedThings
        {
            get => this.showDeprecatedThings;
            set => this.RaiseAndSetIfChanged(ref this.showDeprecatedThings, value);
        }

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
        /// Updates and calls the <see cref="FilterStringService"/> and
        /// Sends the show deprecated things event and refreshes the controls registered to the <see cref="FilterStringService"/>.
        /// </summary>
        private void RefreshAndSendShowDeprecatedThingsEvent()
        {
            this.filterStringService.ShowDeprecatedThings = this.ShowDeprecatedThings;
            this.filterStringService.RefreshDeprecatableFilterAll();

            this.messageBus.SendMessage(new ToggleDeprecatedThingEvent(this.ShowDeprecatedThings));
        }
    }
}
