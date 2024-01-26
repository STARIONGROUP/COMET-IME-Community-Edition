// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Shell.xaml.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary, Jaime Bernar
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMET
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Composition;
    using System.Reactive.Linq;

    using CDP4Composition.Events;
    using CDP4Composition.Ribbon;

    using CDP4Dal;
    using DevExpress.Xpf.Ribbon;
    using Hardcodet.Wpf.TaskbarNotification;

    using ReactiveUI;

    /// <summary>
    /// Interaction logic for Shell.xaml
    /// </summary>
    [Export(typeof(Shell))]
    public partial class Shell : DXRibbonWindow
    {
        /// <summary>
        /// The subscription to display the taskbar notification
        /// </summary>
        private IDisposable subscription;

        /// <summary>
        /// Initializes a new instance of the <see cref="Shell"/> class.
        /// </summary>
        /// <param name="viewModel">The <see cref="ShellViewModel"/> for this view</param>
        /// <param name="ribbonContentBuilder">The <see cref="IRibbonContentBuilder"/></param>
        [ImportingConstructor]
        public Shell(ShellViewModel viewModel, IRibbonContentBuilder ribbonContentBuilder)
        {
            this.InitializeComponent();

            this.DataContext = viewModel;

            ribbonContentBuilder.BuildAndAppendToRibbon(this.Ribbon);

            this.subscription = this.messageBus.Listen<TaskbarNotificationEvent>()
                                .ObserveOn(RxApp.MainThreadScheduler)
                                .Subscribe(this.ShowTaskBarNotification);

            this.Ribbon.MouseUp += (sender, e) => 
            {
                if (e.OriginalSource is RibbonApplicationButtonControl)
                {
                    viewModel.OpenAboutCommand.Execute().GetAwaiter().Wait();
                }
            };
        }

        /// <summary>
        /// The <see cref="CancelEventArgs"/> event handler
        /// </summary>
        /// <param name="e">The <see cref="CancelEventArgs"/></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            if (this.subscription != null)
            {
                this.subscription.Dispose();
            }

            var vm = this.DataContext as ShellViewModel;
            vm?.Dispose();
        }

        /// <summary>
        /// Show the task-bar notification
        /// </summary>
        /// <param name="notificationEvent">The <see cref="TaskbarNotificationEvent"/> containing the data to show</param>
        private void ShowTaskBarNotification(TaskbarNotificationEvent notificationEvent)
        {
            BalloonIcon icon;
            switch (notificationEvent.NotificationKind)
            {
                case NotificationKind.BASIC:
                    icon = BalloonIcon.None;
                    break;
                case NotificationKind.INFO:
                    icon = BalloonIcon.Info;
                    break;
                case NotificationKind.WARNING:
                    icon = BalloonIcon.Warning;
                    break;
                case NotificationKind.ERROR:
                    icon = BalloonIcon.Error;
                    break;
                default:
                    icon = BalloonIcon.None;
                    break;
            }

            this.CdpTaskBarIcon.ShowBalloonTip(notificationEvent.Title, notificationEvent.Message, icon);
        }
    }
}
