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
    using CDP4Composition.Services;

    using CDP4Dal;
    using CDP4Dal.Events;
    using ReactiveUI;

    /// <summary>
    /// The Team-Composition Ribbon view-model 
    /// </summary>
    public class ShowDeprecatedBrowserRibbonViewModel : ReactiveObject, IDeprecatableToggleViewModel
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
        /// The backing field for <see cref="ShowDeprecatedThings"/> property.
        /// </summary>
        private bool showDeprecatedThings;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShowDeprecatedBrowserRibbonViewModel"/> class
        /// </summary>
        public ShowDeprecatedBrowserRibbonViewModel()
        {
            this.openSessions = new ReactiveList<ISession> { ChangeTrackingEnabled = true };
            this.openSessions.CountChanged.Select(x => x != 0).ToProperty(this, x => x.HasSession, out this.hasSession);

            CDPMessageBus.Current.Listen<SessionEvent>().Subscribe(this.SessionChangeEventHandler);

            // register this viewmodel as the toggle control for the visiility of deprecatable things
            FilterStringService.FilterString.RegisterDeprecatableToggleViewModel(this);

            this.WhenAnyValue(vm => vm.ShowDeprecatedThings)
                .Subscribe(_ => this.RefreshAndSendShowDeprecatedThingsEvent());
        }

        /// <summary>
        /// Gets a value indicating whether there are open <see cref="ISession"/>s
        /// </summary>
        public bool HasSession
        {
            get { return this.hasSession.Value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to display deprecated items.
        /// </summary>
        public bool ShowDeprecatedThings
        {
            get { return this.showDeprecatedThings; }
            set { this.RaiseAndSetIfChanged(ref this.showDeprecatedThings, value); }
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
        /// Sends the show deprecated things event and refreshes the controls registered to the <see cref="FilterStringService"/>.
        /// </summary>
        private void RefreshAndSendShowDeprecatedThingsEvent()
        {
            FilterStringService.FilterString.RefreshDeprecatableFilterAll();
            CDPMessageBus.Current.SendMessage(new ToggleDeprecatedThingEvent(this.ShowDeprecatedThings));
        }
    }
}