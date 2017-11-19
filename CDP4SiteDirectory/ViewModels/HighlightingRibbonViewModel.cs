// -------------------------------------------------------------------------------------------------
// <copyright file="HighlightingRibbonViewModel.cs" company="RHEA System S.A.">
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
    /// The highlighting ribbon viewmodel.
    /// </summary>
    public class HighlightingRibbonViewModel : ReactiveObject
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
        /// Initializes a new instance of the <see cref="ShowDeprecatedBrowserRibbonViewModel"/> class
        /// </summary>
        public HighlightingRibbonViewModel()
        {
            this.openSessions = new ReactiveList<ISession> { ChangeTrackingEnabled = true };
            this.openSessions.CountChanged.Select(x => x != 0).ToProperty(this, x => x.HasSession, out this.hasSession);

            CDPMessageBus.Current.Listen<SessionEvent>().Subscribe(this.SessionChangeEventHandler);

            this.ClearHighlightingCommand = ReactiveCommand.Create();
            this.ClearHighlightingCommand.Subscribe(_ => this.ExecuteClearHighlightingCommand());
        }

        /// <summary>
        /// Gets a value indicating whether there are open <see cref="ISession"/>s
        /// </summary>
        public bool HasSession
        {
            get { return this.hasSession.Value; }
        }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> to clear highlighting.
        /// </summary>
        public ReactiveCommand<object> ClearHighlightingCommand { get; private set; }

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
            CDPMessageBus.Current.SendMessage(new CancelHighlightEvent());
        }
    }
}