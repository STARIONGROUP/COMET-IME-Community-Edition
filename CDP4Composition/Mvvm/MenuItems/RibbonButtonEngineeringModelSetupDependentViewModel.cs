// -------------------------------------------------------------------------------------------------
// <copyright file="RibbonButtonEngineeringModelSetupDependentViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2017 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Mvvm
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;

    using CDP4Common.SiteDirectoryData;
    using CDP4Dal;
    using CDP4Dal.Events;
    using Navigation;
    using Navigation.Interfaces;
    using ReactiveUI;

    /// <summary>
    /// The base class representing Ribbon button that depends on available <see cref="EngineeringModelSetup"/> to show a <see cref="IPanelView"/>
    /// </summary>
    public abstract class RibbonButtonEngineeringModelSetupDependentViewModel : ReactiveObject 
    {
        /// <summary>
        /// The Function returning an instance of <see cref="IPanelViewModel"/>
        /// </summary>
        protected readonly Func<EngineeringModelSetup, ISession, IThingDialogNavigationService, IPanelNavigationService, IDialogNavigationService, IPanelViewModel> InstantiatePanelViewModelFunction;

        /// <summary>
        /// Backing field for <see cref="HasSessions"/>
        /// </summary>
        protected ObservableAsPropertyHelper<bool> hasSessions;

        /// <summary>
        /// Initializes a new instance of the <see cref="RibbonButtonEngineeringModelSetupDependentViewModel"/> class
        /// </summary>
        /// <param name="instantiatePanelViewModelFunction">
        /// The instantiate Panel View Model Function.
        /// </param>
        protected RibbonButtonEngineeringModelSetupDependentViewModel(Func<EngineeringModelSetup, ISession, IThingDialogNavigationService, IPanelNavigationService, IDialogNavigationService, IPanelViewModel> instantiatePanelViewModelFunction)
        {
            this.InstantiatePanelViewModelFunction = instantiatePanelViewModelFunction;

            this.EngineeringModelSetups = new ReactiveList<SessionEngineeringModelSetupMenuGroupViewModel>();
            this.Sessions = new List<ISession>();
            this.EngineeringModelSetups.ChangeTrackingEnabled = true;
            this.EngineeringModelSetups.CountChanged.Select(x => x != 0).ToProperty(this, x => x.HasSessions, out this.hasSessions);

            CDPMessageBus.Current.Listen<SessionEvent>().Subscribe(this.SessionChangeEventHandler);

            CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(EngineeringModelSetup))
                .Where(x => x.EventKind == EventKind.Added)
                .Select(x => x.ChangedThing as EngineeringModelSetup)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(this.EngineeringModelSetupAddedEventHandler);

            CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(EngineeringModelSetup))
                .Where(x => x.EventKind == EventKind.Removed)
                .Select(x => x.ChangedThing as EngineeringModelSetup)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(this.EngineeringModelSetupRemovedEventHandler);
        }

        /// <summary>
        /// Gets a value indicating whether there are open sessions
        /// </summary>
        public bool HasSessions
        {
            get { return this.hasSessions.Value; }
        }

        /// <summary>
        /// Gets the list of <see cref="EngineeringModelSetups"/> based on the ones available in the application.
        /// </summary>
        public ReactiveList<SessionEngineeringModelSetupMenuGroupViewModel> EngineeringModelSetups { get; private set; }

        /// <summary>
        /// Gets the List of <see cref="ISession"/> that are opened
        /// </summary>
        public List<ISession> Sessions { get; private set; }

        /// <summary>
        /// The event-handler that is invoked by the subscription that listens for <see cref="EngineeringModelSetup"/>s added
        /// </summary>
        /// <param name="engineeringModelSetup">the engineering model setup.</param>
        protected virtual void EngineeringModelSetupAddedEventHandler(EngineeringModelSetup engineeringModelSetup)
        {
            var session = this.Sessions.SingleOrDefault(s => s.Assembler.Cache == engineeringModelSetup.Cache);
            if (session == null)
            {
                // no session associated found in this ribbon
                return;
            }

            var sessionEngineeringModelSetupMenuGroupViewModel = this.EngineeringModelSetups.SingleOrDefault(x => x.Thing == engineeringModelSetup.Container);
            if (sessionEngineeringModelSetupMenuGroupViewModel == null)
            {
                sessionEngineeringModelSetupMenuGroupViewModel = new SessionEngineeringModelSetupMenuGroupViewModel(engineeringModelSetup.Container as SiteDirectory, session);
                this.EngineeringModelSetups.Add(sessionEngineeringModelSetupMenuGroupViewModel);
            }

            var menuItem = new RibbonMenuItemEngineeringModelSetupDependentViewModel(engineeringModelSetup, session, this.InstantiatePanelViewModelFunction);
            sessionEngineeringModelSetupMenuGroupViewModel.EngineeringModelSetups.Add(menuItem);
        }

        /// <summary>
        /// The event-handler that is invoked by the subscription that listens for <see cref="Iteration"/>s removed
        /// </summary>
        /// <param name="engineeringModelSetup">the engineering model setup</param>
        protected virtual void EngineeringModelSetupRemovedEventHandler(EngineeringModelSetup engineeringModelSetup)
        {
            var sessionEngineeringModelSetupMenuGroupViewModel = this.EngineeringModelSetups.SingleOrDefault(x => x.Thing == engineeringModelSetup.Container);
            if (sessionEngineeringModelSetupMenuGroupViewModel == null)
            {
                return;
            }

            var menuItemToRemove = sessionEngineeringModelSetupMenuGroupViewModel.EngineeringModelSetups.Single(x => x.EngineeringModelSetup == engineeringModelSetup);
            sessionEngineeringModelSetupMenuGroupViewModel.EngineeringModelSetups.Remove(menuItemToRemove);

            // removes the group if there are no more of its iterations opened
            if (sessionEngineeringModelSetupMenuGroupViewModel.EngineeringModelSetups.Count == 0)
            {
                this.EngineeringModelSetups.Remove(sessionEngineeringModelSetupMenuGroupViewModel);
            }

            menuItemToRemove.IsChecked = false;
            menuItemToRemove.ShowOrClosePanelCommand.Execute(null);
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
