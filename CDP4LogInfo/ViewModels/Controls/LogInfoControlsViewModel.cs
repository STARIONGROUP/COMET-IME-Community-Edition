// -------------------------------------------------------------------------------------------------
// <copyright file="LogInfoControlsViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4LogInfo.ViewModels
{
    using System;
    using System.Reactive.Linq;
    using CDP4Composition;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Events;
    using CDP4Dal;
    using CDP4LogInfo.Views;
    using CommonServiceLocator;
    using ReactiveUI;

    /// <summary>
    /// The view-model of the <see cref="LogInfoControls"/>
    /// </summary>
    public class LogInfoControlsViewModel : ReactiveObject
    {
        /// <summary>
        /// Backing field for <see cref="IsChecked"/>
        /// </summary>
        private bool isChecked;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogInfoControlsViewModel"/> class
        /// </summary>
        public LogInfoControlsViewModel(IDialogNavigationService dialogNavigationService)
        {
            object NoOp(object param) => param;

            this.LogInfoPanel = new LogInfoPanelViewModel(dialogNavigationService);

            this.OpenClosePanelCommand = ReactiveCommand.Create<object, object>(NoOp);
            this.OpenClosePanelCommand.Subscribe(_ => this.ExecuteOpenClosePanel());
            
            CDPMessageBus.Current.Listen<NavigationPanelEvent>()
                .Where(x => x.ViewModel == (IPanelViewModel)this.LogInfoPanel && x.PanelStatus == PanelStatus.Closed)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => this.HandleClosedPanel());
        }

        /// <summary>
        /// Gets the <see cref="LogInfoPanelViewModel"/> to open
        /// </summary>
        public LogInfoPanelViewModel LogInfoPanel { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the BarCheckItem is checked
        /// </summary>
        public bool IsChecked
        {
            get { return this.isChecked; }
            set { this.RaiseAndSetIfChanged(ref this.isChecked, value); }
        }

        /// <summary>
        /// Gets the open or close Log Panel
        /// </summary>
        public ReactiveCommand<object, object> OpenClosePanelCommand { get; private set; }

        /// <summary>
        /// Executes the Open or Close panel command
        /// </summary>
        private void ExecuteOpenClosePanel()
        {
            var panelNavigationService = ServiceLocator.Current.GetInstance<IPanelNavigationService>();

            if (this.IsChecked)
            {
                panelNavigationService.OpenInDock(this.LogInfoPanel);
            }
            else
            {
                panelNavigationService.CloseInDock(this.LogInfoPanel);
            }
        }

        /// <summary>
        /// Handles the log closed panel event
        /// </summary>
        private void HandleClosedPanel()
        {
            this.IsChecked = false;
        }
    }
}