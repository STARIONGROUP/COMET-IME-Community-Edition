// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RibbonButtonIterationDependentViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2024 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
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
//    along with this program. If not, see http://www.gnu.org/licenses/.
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

    using CDP4Composition.PluginSettingService;
    
    using CDP4Dal;
    using CDP4Dal.Events;
    
    using Navigation;
    using Navigation.Interfaces;
    
    using ReactiveUI;

    /// <summary>
    /// The base class representing Ribbon button that depends on open <see cref="Iteration"/> to show a <see cref="IPanelView"/>
    /// </summary>
    public class RibbonButtonIterationDependentViewModel : ReactiveObject 
    {
        /// <summary>
        /// The Function returning an instance of <see cref="IPanelViewModel"/>
        /// </summary>
        protected readonly Func<Iteration, ISession, IThingDialogNavigationService, IPanelNavigationService, IDialogNavigationService, IPluginSettingsService, IPanelViewModel> InstantiatePanelViewModelFunction;

        /// <summary>
        /// Backing field for <see cref="HasModels"/>
        /// </summary>
        private bool hasModels;

        /// <summary>
        /// Initializes a new instance of the <see cref="RibbonButtonIterationDependentViewModel"/> class
        /// </summary>
        /// <param name="instantiatePanelViewModelFunction">
        /// The instantiate Panel View Model Function.
        /// </param>
        /// <param name="messageBus">
        /// The <see cref="ICDPMessageBus"/>
        /// </param>
        protected RibbonButtonIterationDependentViewModel(Func<Iteration, ISession, IThingDialogNavigationService, IPanelNavigationService, IDialogNavigationService, IPluginSettingsService, IPanelViewModel> instantiatePanelViewModelFunction, ICDPMessageBus messageBus)
        {
            this.InstantiatePanelViewModelFunction = instantiatePanelViewModelFunction;

            this.OpenModels = new ReactiveList<EngineeringModelMenuGroupViewModel>();
            this.OpenModels.CountChanged.Subscribe(x => this.HasModels = x != 0);

            this.Sessions = new List<ISession>();

            messageBus.Listen<SessionEvent>().Subscribe(this.SessionChangeEventHandler);

            messageBus.Listen<ObjectChangedEvent>(typeof(Iteration))
                .Where(x => x.EventKind == EventKind.Added)
                .Select(x => x.ChangedThing as Iteration)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(this.IterationAddedEventHandler);

            messageBus.Listen<ObjectChangedEvent>(typeof(Iteration))
                .Where(x => x.EventKind == EventKind.Removed)
                .Select(x => x.ChangedThing as Iteration)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(this.IterationRemovedEventHandler);
        }

        /// <summary>
        /// Gets a value indicating whether there are open sessions
        /// </summary>
        public bool HasModels
        {
            get => this.hasModels;
            set => this.RaiseAndSetIfChanged(ref this.hasModels, value);
        }

        /// <summary>
        /// Gets the list of groups of <see cref="EngineeringModel"/> based on the ones available in the application
        /// </summary>
        public ReactiveList<EngineeringModelMenuGroupViewModel> OpenModels { get; }

        /// <summary>
        /// Gets the List of <see cref="ISession"/> that are opened
        /// </summary>
        public List<ISession> Sessions { get; private set; }

        /// <summary>
        /// The event-handler that is invoked by the subscription that listens for <see cref="Iteration"/>s added
        /// </summary>
        /// <param name="iteration">the Iteration</param>
        protected virtual void IterationAddedEventHandler(Iteration iteration)
        {
            var session = this.Sessions.SingleOrDefault(s => s.Assembler.Cache == iteration.Cache);
            if (session == null)
            {
                // no session associated found in this ribbon
                // version is not supported
                return;
            }

            var modelGroupViewModel = this.OpenModels.SingleOrDefault(x => x.Thing == iteration.Container);
            if (modelGroupViewModel == null)
            {
                modelGroupViewModel = new EngineeringModelMenuGroupViewModel(iteration, session);
                this.OpenModels.Add(modelGroupViewModel);
            }

            var menuItem = new RibbonMenuItemIterationDependentViewModel(iteration, session, this.InstantiatePanelViewModelFunction);
            modelGroupViewModel.SelectedIterations.Add(menuItem);
        }

        /// <summary>
        /// The event-handler that is invoked by the subscription that listens for <see cref="Iteration"/>s removed
        /// </summary>
        /// <param name="iteration">the Iteration</param>
        protected virtual void IterationRemovedEventHandler(Iteration iteration)
        {
            var engineeringModelGroupViewmodel = this.OpenModels.SingleOrDefault(x => x.Thing == iteration.Container);

            if (engineeringModelGroupViewmodel == null)
            {
                return;
            }

            var menuItemToRemove = engineeringModelGroupViewmodel.SelectedIterations.Single(x => x.Iteration == iteration);
            ((ICommand)menuItemToRemove.ClosePanelsCommand).Execute(default);

            engineeringModelGroupViewmodel.SelectedIterations.Remove(menuItemToRemove);

            // removes the group if there are no more of its iterations opened
            if (engineeringModelGroupViewmodel.SelectedIterations.Count == 0)
            {
                this.OpenModels.Remove(engineeringModelGroupViewmodel);
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
            }
        }

        /// <summary>
        /// Asserts whether the version of the current view-model is supported by the <see cref="ISession"/>
        /// </summary>
        /// <param name="session">
        /// The <see cref="ISession"/> that is used to assert compatibility
        /// </param>
        /// <returns>
        /// true when the version is suppported by the <paramref name="session"/>, false if not supported
        /// </returns>
        private bool IsVersionSupported(ISession session)
        {
            var supportedVersion = this.QueryCdpVersion();
            return session.IsVersionSupported(supportedVersion);
        }
    }
}