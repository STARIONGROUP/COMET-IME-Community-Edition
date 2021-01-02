// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SessionViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Ahmed Abulwafa Ahmed
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
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
//    along with this program. If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace CDP4IME.ViewModels
{
    using System;
    using System.Globalization;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using System.Windows.Threading;

    using CDP4Composition;
    using CDP4Composition.Events;
    using CDP4Composition.Navigation;

    using CDP4Dal;

    using Microsoft.Practices.ServiceLocation;
    
    using ReactiveUI;

    /// <summary>
    /// The <see cref="SessionViewModel"/> is a view-model for the <see cref="Session"/> object.
    /// </summary>
    public class SessionViewModel : ReactiveObject
    {
        /// <summary>
        /// Out property for the <see cref="LastUpdateDateTimeHint"/> property
        /// </summary>
        private readonly ObservableAsPropertyHelper<string> lastUpdateDateTimeHint;

        /// <summary>
        /// The default auto refresh interval
        /// </summary>
        private const uint DefaultRefreshInterval = 60;

        /// <summary>
        /// Out property for <see cref="HasError"/>
        /// </summary>
        private readonly ObservableAsPropertyHelper<bool> hasError;

        /// <summary>
        /// Backing field for <see cref="ErrorMsg"/>
        /// </summary>
        private string errorMsg;

        /// <summary>
        /// The timer
        /// </summary>
        private DispatcherTimer timer = new DispatcherTimer();

        /// <summary>
        /// Backing field for <see cref="AutoRefreshInterval"/>
        /// </summary>
        private uint autoRefreshInterval;

        /// <summary>
        /// Backing field for <see cref="AutoRefreshSecondsLeft"/>
        /// </summary>
        private uint autoRefreshSecondsLeft;

        /// <summary>
        /// Backing field for the <see cref="IsAutoRefreshEnabled"/>
        /// </summary>
        private bool isAutoRefreshEnabled;

        /// <summary>
        /// Backing field for the <see cref="IsClosed"/> property.
        /// </summary>
        private bool isClosed;

        /// <summary>
        /// Backing field for the <see cref="LastUpdateDateTime"/> property.
        /// </summary>
        private DateTime lastUpdateDateTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionViewModel"/> class.
        /// </summary>
        /// <param name="session">
        /// The session that is encapsulated
        /// </param>
        public SessionViewModel(ISession session)
        {
            this.Session = session;
            this.AutoRefreshInterval = DefaultRefreshInterval;

            this.Close = ReactiveCommand.CreateAsyncTask(async _ => await this.ExecuteClose());
            this.Refresh = ReactiveCommand.CreateAsyncTask(async _ => await this.ExecuteRefresh());
            this.Reload = ReactiveCommand.CreateAsyncTask(async _ => await this.ExecuteReload());

            this.HideAll = ReactiveCommand.Create();
            this.HideAll.Subscribe(_ => this.ExecuteHideAll());
            
            this.WhenAnyValue(x => x.LastUpdateDateTime).Select(x => x.ToString(CultureInfo.InvariantCulture)).ToProperty(this, x => x.LastUpdateDateTimeHint, out this.lastUpdateDateTimeHint);

            this.WhenAnyValue(x => x.ErrorMsg)
                .Select(x => x != null)
                .ToProperty(this, x => x.HasError, out this.hasError);

            this.WhenAnyValue(x => x.IsAutoRefreshEnabled)
                .Subscribe(_ => this.SetTimer());

            this.WhenAnyValue(x => x.AutoRefreshInterval)
                .Subscribe(_ => this.SetTimer());
        }

        /// <summary>
        /// Gets the <see cref="Session"/> object that is encapsulated by the current <see cref="SessionViewModel"/>.
        /// </summary>
        public ISession Session { get; private set; }

        /// <summary>
        /// Gets the Close <see cref="ICommand"/> that closes the encapsulated <see cref="Session"/>
        /// </summary>
        public ReactiveCommand<Unit> Close { get; private set; }

        /// <summary>
        /// Gets the Hide All <see cref="ICommand"/> that closes all the panels associated to this <see cref="Session"/>
        /// </summary>
        public ReactiveCommand<object> HideAll { get; private set; } 

        /// <summary>
        /// Gets the Refresh <see cref="ICommand"/> that refreshes the encapsulated <see cref="Session"/>
        /// </summary>
        /// <remarks>
        /// a refresh loads the latest new data from the data-source
        /// </remarks>
        public ReactiveCommand<Unit> Refresh { get; private set; }

        /// <summary>
        /// Gets the Refresh <see cref="ICommand"/> that reloads the encapsulated <see cref="Session"/>
        /// </summary>
        /// <remarks>
        /// a reload loads all the data from the data-source
        /// </remarks>
        public ReactiveCommand<Unit> Reload { get; private set; }
        
        /// <summary>
        /// Gets the <see cref="Uri"/> of the current <see cref="Session"/>
        /// </summary>
        public string DataSourceUri => this.Session.DataSourceUri;

        /// <summary>
        /// Gets the name of the current <see cref="ISession"/>
        /// </summary>
        public string SessionName => this.Session.Name;

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="Session"/> object has it's
        /// auto refresh property enabled or disabled.
        /// </summary>
        public bool IsAutoRefreshEnabled
        {
            get => this.isAutoRefreshEnabled;
            set => this.RaiseAndSetIfChanged(ref this.isAutoRefreshEnabled, value);
        }

        /// <summary>
        /// Gets or sets the auto-refresh interval when applicable
        /// </summary>
        public uint AutoRefreshInterval
        {
            get => this.autoRefreshInterval;
            set => this.RaiseAndSetIfChanged(ref this.autoRefreshInterval, value);
        }

        /// <summary>
        /// Gets or sets the auto-refresh seconds left.
        /// </summary>
        public uint AutoRefreshSecondsLeft
        {
            get => this.autoRefreshSecondsLeft;
            set => this.RaiseAndSetIfChanged(ref this.autoRefreshSecondsLeft, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="Session"/> object has been closed.
        /// </summary>
        public bool IsClosed
        {
            get => this.isClosed;

            set => this.RaiseAndSetIfChanged(ref this.isClosed, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="DateTime"/> that the session was refreshed or reloaded data from the server
        /// </summary>
        public DateTime LastUpdateDateTime
        {
            get => this.lastUpdateDateTime;

            set => this.RaiseAndSetIfChanged(ref this.lastUpdateDateTime, value);
        }

        /// <summary>
        /// Gets the hint displayed on the data-source Refresh button
        /// </summary>
        public string LastUpdateDateTimeHint => "Last Updated at " + this.lastUpdateDateTimeHint.Value;

        /// <summary>
        /// Gets a value indicating whether an Error was produced
        /// </summary>
        public bool HasError => this.hasError.Value;

        /// <summary>
        /// Gets or sets the error message 
        /// </summary>
        public string ErrorMsg
        {
            get => this.errorMsg;
            set => this.RaiseAndSetIfChanged(ref this.errorMsg, value);
        }

        /// <summary>
        /// Executes the <see cref="Close"/> Command and closes the <see cref="Session"/>
        /// </summary>
        /// <returns>A <see cref="Task"/></returns>
        private async Task ExecuteClose()
        {
            await this.Session.Close();
            this.IsClosed = true;
        }

        /// <summary>
        /// Executes the <see cref="Refresh"/> Command and refreshes the <see cref="Session"/>
        /// </summary>
        /// <returns>A <see cref="Task"/></returns>
        private async Task ExecuteRefresh()
        {
            try
            {
                this.errorMsg = null;
                await this.Session.Refresh();
                this.LastUpdateDateTime = DateTime.Now;
            }
            catch (Exception e)
            {
                this.ErrorMsg = e.Message;
            }
        }

        /// <summary>
        /// Executes the <see cref="Reload"/> Command and reloads the <see cref="Session"/>
        /// </summary>
        /// <returns>A <see cref="Task"/></returns>
        private async Task ExecuteReload()
        {
            try
            {
                CDPMessageBus.Current.SendMessage(new IsBusyEvent(true, "Reloading..."));
                this.errorMsg = null;
                await this.Session.Reload();
                this.LastUpdateDateTime = DateTime.Now;
            }
            catch (Exception e)
            {
                this.ErrorMsg = e.Message;
            }
            finally
            {
                CDPMessageBus.Current.SendMessage(new IsBusyEvent(false));
            }
        }
        
        /// <summary>
        /// Sets the timer according to the appropriate setting
        /// </summary>
        private void SetTimer()
        {
            if (this.IsAutoRefreshEnabled)
            {
                //dispose of previous timer
                this.timer?.Stop();

                this.AutoRefreshSecondsLeft = this.AutoRefreshInterval;

                this.timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
                this.timer.Tick += this.OntTimerElapsed;
                this.timer.Start();
            }
            else
            {
                this.timer.Stop();
            }
        }

        /// <summary>
        /// The eventhandler to handle elapse of one second.
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The event arguments.</param>
        private void OntTimerElapsed(object sender, EventArgs e)
        {
            this.AutoRefreshSecondsLeft -= 1;

            if (this.AutoRefreshSecondsLeft == 0)
            {
                this.timer.Stop();
                this.Session.Refresh();

                this.AutoRefreshSecondsLeft = this.AutoRefreshInterval;
                this.timer.Start();
            }
        }

        /// <summary>
        /// Executes the <see cref="HideAll"/> Command and closes all the associated <see cref="IPanelView"/>
        /// </summary>
        private void ExecuteHideAll()
        {
            var navigation = ServiceLocator.Current.GetInstance<IPanelNavigationService>();
            navigation.Close(this.Session.DataSourceUri);
        }
    }
}