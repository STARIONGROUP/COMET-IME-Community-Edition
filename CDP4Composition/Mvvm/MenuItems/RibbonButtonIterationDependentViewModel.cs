// -------------------------------------------------------------------------------------------------
// <copyright file="RibbonButtonIterationDependentViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Mvvm
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;    
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
        protected ObservableAsPropertyHelper<bool> hasModels;

        /// <summary>
        /// Initializes a new instance of the <see cref="RibbonButtonIterationDependentViewModel"/> class
        /// </summary>
        /// <param name="instantiatePanelViewModelFunction">
        /// The instantiate Panel View Model Function.
        /// </param>
        protected RibbonButtonIterationDependentViewModel(Func<Iteration, ISession, IThingDialogNavigationService, IPanelNavigationService, IDialogNavigationService, IPluginSettingsService, IPanelViewModel> instantiatePanelViewModelFunction)
        {
            this.InstantiatePanelViewModelFunction = instantiatePanelViewModelFunction;

            this.OpenModels = new ReactiveList<EngineeringModelMenuGroupViewModel>();
            this.Sessions = new List<ISession>();
            this.OpenModels.ChangeTrackingEnabled = true;
            this.OpenModels.CountChanged.Select(x => x != 0).ToProperty(this, x => x.HasModels, out this.hasModels);

            CDPMessageBus.Current.Listen<SessionEvent>().Subscribe(this.SessionChangeEventHandler);

            CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(Iteration))
                .Where(x => x.EventKind == EventKind.Added)
                .Select(x => x.ChangedThing as Iteration)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(this.IterationAddedEventHandler);

            CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(Iteration))
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
            get { return this.hasModels.Value; }
        }

        /// <summary>
        /// Gets the list of groups of <see cref="EngineeringModel"/> based on the ones available in the application
        /// </summary>
        public ReactiveList<EngineeringModelMenuGroupViewModel> OpenModels { get; private set; }

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
            engineeringModelGroupViewmodel.SelectedIterations.Remove(menuItemToRemove);

            // removes the group if there are no more of its iterations opened
            if (engineeringModelGroupViewmodel.SelectedIterations.Count == 0)
            {
                this.OpenModels.Remove(engineeringModelGroupViewmodel);
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