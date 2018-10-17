// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Shell.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4IME
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Composition;
    using System.Reactive.Linq;
    using System.Windows;
    using System.Windows.Input;
    using CDP4Composition.Events;
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
        public Shell()
        {
            this.InitializeComponent();
            this.subscription = CDPMessageBus.Current.Listen<TaskbarNotificationEvent>()
                                .ObserveOn(RxApp.MainThreadScheduler)
                                .Subscribe(this.ShowTaskBarNotification);
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
