// -------------------------------------------------------------------------------------------------
// <copyright file="LogInfoControlsViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2023 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
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

namespace CDP4LogInfo.ViewModels
{
    using System;
    using System.Reactive;
    using System.Reactive.Linq;
    using CDP4Composition;
    using CDP4Composition.Mvvm;
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
            this.LogInfoPanel = new LogInfoPanelViewModel(dialogNavigationService);

            this.OpenClosePanelCommand = ReactiveCommandCreator.Create(this.ExecuteOpenClosePanel);
            
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
        public ReactiveCommand<Unit, Unit> OpenClosePanelCommand { get; private set; }

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