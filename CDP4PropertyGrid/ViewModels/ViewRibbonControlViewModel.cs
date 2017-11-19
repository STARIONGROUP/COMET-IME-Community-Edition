// -------------------------------------------------------------------------------------------------
// <copyright file="ViewRibbonControlViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4PropertyGrid.ViewModels
{
    using System;
    using System.Reactive.Linq;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Events;
    using CDP4Dal;
    using Microsoft.Practices.ServiceLocation;
    using ReactiveUI;

    /// <summary>
    /// The view-model for the PropertyGrid Controls
    /// </summary>
    public class ViewRibbonControlViewModel : ReactiveObject
    {
        /// <summary>
        /// Backing field for <see cref="IsChecked"/>
        /// </summary>
        private bool isChecked;

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewRibbonControlViewModel"/> class
        /// </summary>
        public ViewRibbonControlViewModel()
        {
            this.OpenClosePanelCommand = ReactiveCommand.Create();
            this.OpenClosePanelCommand.Subscribe(_ => this.ExecuteOpenClosePanel());

            CDPMessageBus.Current.Listen<NavigationPanelEvent>()
                .Where(x => x.ViewModel.GetType() == typeof(PropertyGridViewModel) && x.PanelStatus == PanelStatus.Closed)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => this.HandleClosedPanel());
        }

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
        public ReactiveCommand<object> OpenClosePanelCommand { get; private set; }

        /// <summary>
        /// Executes the Open or Close panel command
        /// </summary>
        private void ExecuteOpenClosePanel()
        {
            var panelNavigationService = ServiceLocator.Current.GetInstance<IPanelNavigationService>();

            if (this.IsChecked)
            {
                panelNavigationService.Open(new PropertyGridViewModel(), true);
            }
            else
            {
                panelNavigationService.Close(typeof(PropertyGridViewModel));
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