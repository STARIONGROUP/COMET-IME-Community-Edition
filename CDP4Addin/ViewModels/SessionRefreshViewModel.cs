// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SessionRefreshViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2026 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Rowan de Voogt
//
//    This file is part of CDP4-COMET IME Community Edition.
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

namespace CDP4AddinCE.ViewModels
{
    using System;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Windows.Threading;

    using CDP4Composition.Mvvm;
    using CDP4Composition.Utilities;

    using CDP4Dal;

    using NLog;

    using ReactiveUI;

    /// <summary>
    /// Backs the auto-refresh controls that live directly on the Excel add-in ribbon. It holds the auto-refresh
    /// state and a <see cref="DispatcherTimer"/> that refreshes the active <see cref="ISession"/> in the background,
    /// and exposes the on-demand Refresh and Reload commands. This is the add-in counterpart of the auto-refresh
    /// control that is available on the ribbon of the CDP4-COMET IME.
    /// </summary>
    public class SessionRefreshViewModel : ReactiveObject, IDisposable
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The default auto refresh interval expressed in seconds
        /// </summary>
        public const uint DefaultRefreshInterval = 60;

        /// <summary>
        /// The minimum auto refresh interval expressed in seconds
        /// </summary>
        public const uint MinimumRefreshInterval = 5;

        /// <summary>
        /// The maximum auto refresh interval expressed in seconds
        /// </summary>
        public const uint MaximumRefreshInterval = 300;

        /// <summary>
        /// The timer used to drive the automatic refresh
        /// </summary>
        private DispatcherTimer timer;

        /// <summary>
        /// Backing field for the <see cref="AutoRefreshInterval"/> property
        /// </summary>
        private uint autoRefreshInterval;

        /// <summary>
        /// Backing field for the <see cref="AutoRefreshSecondsLeft"/> property
        /// </summary>
        private uint autoRefreshSecondsLeft;

        /// <summary>
        /// Backing field for the <see cref="IsAutoRefreshEnabled"/> property
        /// </summary>
        private bool isAutoRefreshEnabled;

        /// <summary>
        /// Backing field for the <see cref="LastUpdateDateTime"/> property
        /// </summary>
        private DateTime lastUpdateDateTime;

        /// <summary>
        /// Backing field for the <see cref="ErrorMessage"/> property
        /// </summary>
        private string errorMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionRefreshViewModel"/> class.
        /// </summary>
        /// <param name="session">
        /// The <see cref="ISession"/> that is refreshed or reloaded
        /// </param>
        public SessionRefreshViewModel(ISession session)
        {
            this.Session = session ?? throw new ArgumentNullException(nameof(session));
            this.AutoRefreshInterval = DefaultRefreshInterval;

            this.Refresh = ReactiveCommandCreator.Create(this.ExecuteRefresh);
            this.Reload = ReactiveCommandCreator.Create(this.ExecuteReload);

            this.WhenAnyValue(
                    x => x.IsAutoRefreshEnabled,
                    x => x.AutoRefreshInterval)
                .Subscribe(_ => this.SetTimer());
        }

        /// <summary>
        /// Gets the <see cref="ISession"/> that is encapsulated by the current <see cref="SessionRefreshViewModel"/>.
        /// </summary>
        public ISession Session { get; }

        /// <summary>
        /// Gets the Refresh <see cref="ReactiveCommand{TParam,TResult}"/> that refreshes the encapsulated <see cref="ISession"/>
        /// (current revision + delta).
        /// </summary>
        public ReactiveCommand<Unit, Unit> Refresh { get; }

        /// <summary>
        /// Gets the Reload <see cref="ReactiveCommand{TParam,TResult}"/> that reloads the encapsulated <see cref="ISession"/>
        /// (revision 0 + delta).
        /// </summary>
        public ReactiveCommand<Unit, Unit> Reload { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the automatic refresh of the <see cref="ISession"/> is enabled or disabled.
        /// </summary>
        public bool IsAutoRefreshEnabled
        {
            get => this.isAutoRefreshEnabled;

            set => this.RaiseAndSetIfChanged(ref this.isAutoRefreshEnabled, value);
        }

        /// <summary>
        /// Gets or sets the auto-refresh interval expressed in seconds.
        /// </summary>
        public uint AutoRefreshInterval
        {
            get => this.autoRefreshInterval;

            set => this.RaiseAndSetIfChanged(ref this.autoRefreshInterval, value);
        }

        /// <summary>
        /// Gets or sets the number of seconds left before the next automatic refresh.
        /// </summary>
        public uint AutoRefreshSecondsLeft
        {
            get => this.autoRefreshSecondsLeft;

            set => this.RaiseAndSetIfChanged(ref this.autoRefreshSecondsLeft, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="DateTime"/> at which the session last refreshed or reloaded data from the server
        /// </summary>
        public DateTime LastUpdateDateTime
        {
            get => this.lastUpdateDateTime;

            set => this.RaiseAndSetIfChanged(ref this.lastUpdateDateTime, value);
        }

        /// <summary>
        /// Gets or sets the message of the last error that occurred while refreshing or reloading, if any.
        /// </summary>
        public string ErrorMessage
        {
            get => this.errorMessage;

            set => this.RaiseAndSetIfChanged(ref this.errorMessage, value);
        }

        /// <summary>
        /// Sets the <see cref="AutoRefreshInterval"/> from a user supplied string, clamping it to the allowed range.
        /// Invalid input is ignored and the previous value is kept.
        /// </summary>
        /// <param name="text">
        /// The text entered in the interval edit box
        /// </param>
        public void SetIntervalFromText(string text)
        {
            if (uint.TryParse(text, out var parsed))
            {
                this.AutoRefreshInterval = ClampInterval(parsed);
            }
        }

        /// <summary>
        /// Clamps an interval to the <see cref="MinimumRefreshInterval"/>..<see cref="MaximumRefreshInterval"/> range.
        /// </summary>
        /// <param name="interval">The interval to clamp.</param>
        /// <returns>The clamped interval.</returns>
        private static uint ClampInterval(uint interval)
        {
            if (interval < MinimumRefreshInterval)
            {
                return MinimumRefreshInterval;
            }

            return interval > MaximumRefreshInterval ? MaximumRefreshInterval : interval;
        }

        /// <summary>
        /// Executes the <see cref="Refresh"/> Command and refreshes the <see cref="ISession"/>
        /// </summary>
        private async void ExecuteRefresh()
        {
            try
            {
                this.ErrorMessage = null;
                await this.Session.Refresh();
                this.LastUpdateDateTime = DateTime.Now;
            }
            catch (Exception e)
            {
                this.ErrorMessage = e.Message;
                logger.Error(e, "The session {0} could not be refreshed", this.Session.DataSourceUri);
            }
        }

        /// <summary>
        /// Executes the <see cref="Reload"/> Command and reloads the <see cref="ISession"/>
        /// </summary>
        private async void ExecuteReload()
        {
            try
            {
                this.ErrorMessage = null;
                await this.Session.Reload();
                this.LastUpdateDateTime = DateTime.Now;
            }
            catch (Exception e)
            {
                this.ErrorMessage = e.Message;
                logger.Error(e, "The session {0} could not be reloaded", this.Session.DataSourceUri);
            }
        }

        /// <summary>
        /// Sets the timer according to the current automatic refresh settings
        /// </summary>
        private void SetTimer()
        {
            if (this.IsAutoRefreshEnabled)
            {
                this.timer?.Stop();

                this.AutoRefreshSecondsLeft = this.AutoRefreshInterval;

                this.timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(1)
                };

                this.timer.Tick += this.OnTimerElapsed;

                this.timer.Start();
            }
            else
            {
                this.timer?.Stop();
            }
        }

        /// <summary>
        /// The event-handler that is invoked on every elapsed second of the <see cref="timer"/>.
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The event arguments.</param>
        private async void OnTimerElapsed(object sender, EventArgs e)
        {
            this.AutoRefreshSecondsLeft -= 1;

            if (this.AutoRefreshSecondsLeft == 0)
            {
                this.timer.Stop();

                try
                {
                    LockProvider.EnterLock(LockType.SesionRefresh);
                    this.ErrorMessage = null;
                    await this.Session.Refresh();
                    this.LastUpdateDateTime = DateTime.Now;
                }
                catch (Exception ex)
                {
                    this.ErrorMessage = ex.Message;
                    logger.Error(ex, "The automatic refresh of session {0} failed", this.Session.DataSourceUri);
                }
                finally
                {
                    LockProvider.ExitLock(LockType.SesionRefresh);
                    this.AutoRefreshSecondsLeft = this.AutoRefreshInterval;
                    this.timer.Start();
                }
            }
        }

        /// <summary>
        /// Disposes of this <see cref="SessionRefreshViewModel"/>, stopping the automatic refresh timer.
        /// </summary>
        public void Dispose()
        {
            try
            {
                LockProvider.EnterLock(LockType.SesionRefresh);
                this.timer?.Stop();
            }
            finally
            {
                LockProvider.ExitLock(LockType.SesionRefresh);
            }
        }
    }
}
