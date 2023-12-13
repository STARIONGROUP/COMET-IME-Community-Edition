// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RibbonButtonEngineeringModelDependentViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2023 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski, Ahmed Abulwafa Ahmed
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-COMET IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
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
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Mvvm
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Windows.Input;

    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;
    using CDP4Dal.Events;

    using ReactiveUI;

    /// <summary>
    /// The base class representing Ribbon button that depends on available <see cref="EngineeringModel"/> to show a <see cref="IPanelView"/>
    /// </summary>
    public abstract class RibbonButtonEngineeringModelDependentViewModel : ReactiveObject
    {
        /// <summary>
        /// The Function returning an instance of <see cref="IPanelViewModel"/>
        /// </summary>
        protected readonly Func<EngineeringModel, ISession, IThingDialogNavigationService, IPanelNavigationService, IDialogNavigationService, IPluginSettingsService, IPanelViewModel> InstantiatePanelViewModelFunction;

        /// <summary>
        /// Backing field for <see cref="HasSessions"/>
        /// </summary>
        protected ObservableAsPropertyHelper<bool> hasSessions;

        /// <summary>
        /// Initializes a new instance of the <see cref="RibbonButtonEngineeringModelDependentViewModel"/> class
        /// </summary>
        /// <param name="instantiatePanelViewModelFunction">
        /// The instantiate Panel View Model Function.
        /// </param>
        protected RibbonButtonEngineeringModelDependentViewModel(Func<EngineeringModel, ISession, IThingDialogNavigationService, IPanelNavigationService, IDialogNavigationService, IPluginSettingsService, IPanelViewModel> instantiatePanelViewModelFunction)
        {
            this.InstantiatePanelViewModelFunction = instantiatePanelViewModelFunction;

            this.EngineeringModels = new ReactiveList<SessionEngineeringModelMenuGroupViewModel>();
            this.Sessions = new List<ISession>();
            this.EngineeringModels.CountChanged.Select(x => x != 0).ToProperty(this, x => x.HasSessions, out this.hasSessions, scheduler: RxApp.MainThreadScheduler);

            CDPMessageBus.Current.Listen<SessionEvent>().Subscribe(this.SessionChangeEventHandler);

            CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(EngineeringModel))
                .Where(x => x.EventKind == EventKind.Added)
                .Select(x => x.ChangedThing as EngineeringModel)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(this.EngineeringModelAddedEventHandler);

            CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(EngineeringModel))
                .Where(x => x.EventKind == EventKind.Removed)
                .Select(x => x.ChangedThing as EngineeringModel)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(this.EngineeringModelRemovedEventHandler);
        }

        /// <summary>
        /// Gets a value indicating whether there are open sessions
        /// </summary>
        public bool HasSessions => this.hasSessions.Value;

        /// <summary>
        /// Gets the list of <see cref="EngineeringModels"/> based on the ones available in the application.
        /// </summary>
        public ReactiveList<SessionEngineeringModelMenuGroupViewModel> EngineeringModels { get; private set; }

        /// <summary>
        /// Gets the List of <see cref="ISession"/> that are opened
        /// </summary>
        public List<ISession> Sessions { get; private set; }

        /// <summary>
        /// The event-handler that is invoked by the subscription that listens for <see cref="EngineeringModel"/>s added
        /// </summary>
        /// <param name="engineeringModel">the engineering model setup.</param>
        protected virtual void EngineeringModelAddedEventHandler(EngineeringModel engineeringModel)
        {
            var session = this.Sessions.SingleOrDefault(s => s.Assembler.Cache == engineeringModel.Cache);

            if (session == null)
            {
                // no session associated found in this ribbon
                return;
            }

            var sessionEngineeringModelSetupMenuGroupViewModel = this.EngineeringModels.SingleOrDefault(x => x.Thing == engineeringModel);

            if (sessionEngineeringModelSetupMenuGroupViewModel == null)
            {
                sessionEngineeringModelSetupMenuGroupViewModel = new SessionEngineeringModelMenuGroupViewModel(engineeringModel, session);
                this.EngineeringModels.Add(sessionEngineeringModelSetupMenuGroupViewModel);
            }

            var menuItem = new RibbonMenuItemEngineeringModelDependentViewModel(engineeringModel, session, this.InstantiatePanelViewModelFunction);
            sessionEngineeringModelSetupMenuGroupViewModel.EngineeringModels.Add(menuItem);

            // Sort the list alphabetically
            sessionEngineeringModelSetupMenuGroupViewModel.EngineeringModels.Sort((x,y) => x.MenuItemContent.CompareTo(y.MenuItemContent));
        }

        /// <summary>
        /// The event-handler that is invoked by the subscription that listens for <see cref="Iteration"/>s removed
        /// </summary>
        /// <param name="engineeringModel">the engineering model</param>
        protected virtual void EngineeringModelRemovedEventHandler(EngineeringModel engineeringModel)
        {
            var sessionEngineeringModelSetupMenuGroupViewModel = this.EngineeringModels.SingleOrDefault(x => x.Thing == engineeringModel.Container);

            if (sessionEngineeringModelSetupMenuGroupViewModel == null)
            {
                return;
            }

            var menuItemToRemove = sessionEngineeringModelSetupMenuGroupViewModel.EngineeringModels.Single(x => x.Model == engineeringModel);
            sessionEngineeringModelSetupMenuGroupViewModel.EngineeringModels.Remove(menuItemToRemove);

            // removes the group if there are no more of its iterations opened
            if (sessionEngineeringModelSetupMenuGroupViewModel.EngineeringModels.Count == 0)
            {
                this.EngineeringModels.Remove(sessionEngineeringModelSetupMenuGroupViewModel);
            }

            ((ICommand)menuItemToRemove.ClosePanelsCommand).Execute(default);
        }

        /// <summary>
        /// The method will remove the group under a closed session
        /// </summary>
        /// <param name="session">the session that has been removed</param>
        protected virtual void SessionRemoveGroup(ISession session)
        {
            var sessionEngineeringModelSetupMenuGroupViewModel = this.EngineeringModels.SingleOrDefault(x => x.Session == session);

            if (sessionEngineeringModelSetupMenuGroupViewModel == null)
            {
                return;
            }

            foreach (var menuItemToRemove in sessionEngineeringModelSetupMenuGroupViewModel.EngineeringModels.ToList())
            {
                sessionEngineeringModelSetupMenuGroupViewModel.EngineeringModels.Remove(menuItemToRemove);

                // removes the group if there are no more of its iterations opened
                if (sessionEngineeringModelSetupMenuGroupViewModel.EngineeringModels.Count == 0)
                {
                    this.EngineeringModels.Remove(sessionEngineeringModelSetupMenuGroupViewModel);
                }

                ((ICommand)menuItemToRemove.ClosePanelsCommand).Execute(default);
            }
        }

        /// <summary>
        /// The event-handler that is invoked by the subscription that listens for updates
        /// on the <see cref="Session"/> that is being represented by the view-model
        /// </summary>
        /// <param name="sessionChange">
        /// The payload of the event that is being handled
        /// </param>
        private void SessionChangeEventHandler(SessionEvent sessionChange)
        {
            if (!this.IsVersionSupported(sessionChange.Session))
            {
                return;
            }

            if (sessionChange.Status == SessionStatus.Open)
            {
                this.Sessions.Add(sessionChange.Session);
            }
            else if (sessionChange.Status == SessionStatus.Closed)
            {
                this.Sessions.Remove(sessionChange.Session);
                this.SessionRemoveGroup(sessionChange.Session);
            }
        }

        /// <summary>
        /// Asserts whether the version of the current view-model is supported by the <see cref="ISession"/>
        /// </summary>
        /// <param name="session">
        /// The <see cref="ISession"/> that is used to assert compatibility
        /// </param>
        /// <returns>
        /// true when the version is supported by the <paramref name="session"/>, false if not supported
        /// </returns>
        private bool IsVersionSupported(ISession session)
        {
            var supportedVersion = this.QueryCdpVersion();
            return session.IsVersionSupported(supportedVersion);
        }
    }
}
