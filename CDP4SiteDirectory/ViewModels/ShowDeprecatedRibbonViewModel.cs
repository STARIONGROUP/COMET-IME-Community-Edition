// -------------------------------------------------------------------------------------------------
// <copyright file="ShowDeprecatedBrowserRibbonViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.ViewModels
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;
    using CDP4Composition;
    using CDP4Composition.Events;
    using CDP4Dal;
    using CDP4Dal.Events;
    using ReactiveUI;

    /// <summary>
    /// The Team-Composition Ribbon view-model 
    /// </summary>
    public class ShowDeprecatedBrowserRibbonViewModel : ReactiveObject
    {
        /// <summary>
        /// Backing field for <see cref="HasSession"/>
        /// </summary>
        private readonly ObservableAsPropertyHelper<bool> hasSession;

        /// <summary>
        /// The open <see cref="ISession"/>s
        /// </summary>
        private readonly ReactiveList<ISession> openSessions;

        /// <summary>
        /// The deprecated filter behavior.
        /// </summary>
        private readonly FilterStringService filterStringService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShowDeprecatedBrowserRibbonViewModel"/> class
        /// </summary>
        public ShowDeprecatedBrowserRibbonViewModel()
        {
            this.filterStringService = FilterStringService.FilterString;
            this.openSessions = new ReactiveList<ISession> { ChangeTrackingEnabled = true };
            this.openSessions.CountChanged.Select(x => x != 0).ToProperty(this, x => x.HasSession, out this.hasSession);

            CDPMessageBus.Current.Listen<SessionEvent>().Subscribe(this.SessionChangeEventHandler);

            this.ShowHideDeprecatedThingsCommand = ReactiveCommand.Create();
            this.ShowHideDeprecatedThingsCommand.Subscribe(_ => this.ExecuteShowHideDeprecatedThingsCommand());
            this.filterStringService.IsFilterActive = true;
        }

        /// <summary>
        /// Gets a value indicating whether there are open <see cref="ISession"/>s
        /// </summary>
        public bool HasSession
        {
            get { return this.hasSession.Value; }
        }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> to open the <see cref="EngineeringModelSetupSelectionDialogViewModel"/>
        /// </summary>
        public ReactiveCommand<object> ShowHideDeprecatedThingsCommand { get; private set; }

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
        /// Executes the <see cref="ShowHideDeprecatedThingsCommand"/> to show or hide <see cref="IDeprecatableThing"/>s
        /// </summary>
        private void ExecuteShowHideDeprecatedThingsCommand()
        {
            this.filterStringService.IsFilterActive = !this.filterStringService.IsFilterActive;
            this.filterStringService.RefreshAll();
            CDPMessageBus.Current.SendMessage(new ToggleDeprecatedThingEvent(!this.filterStringService.IsFilterActive));
        }
    }
}