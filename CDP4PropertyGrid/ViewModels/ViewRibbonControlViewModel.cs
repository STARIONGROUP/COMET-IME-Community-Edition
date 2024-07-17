// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ViewRibbonControlViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2024 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
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

namespace CDP4PropertyGrid.ViewModels
{
    using System;
    using System.Reactive;
    using System.Reactive.Linq;

    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Events;

    using CDP4Dal;
    
    using CommonServiceLocator;
    
    using ReactiveUI;

    /// <summary>
    /// The view-model for the PropertyGrid Controls
    /// </summary>
    public class ViewRibbonControlViewModel : ReactiveObject
    {
        /// <summary>
        /// The <see cref="ICDPMessageBus"/>
        /// </summary>
        public ICDPMessageBus CDPMessageBus { get; }

        /// <summary>
        /// Backing field for <see cref="IsChecked"/>
        /// </summary>
        private bool isChecked;

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewRibbonControlViewModel"/> class
        /// </summary>
        /// <param name="messageBus">
        /// The <see cref="ICDPMessageBus"/>
        /// </param>
        public ViewRibbonControlViewModel(ICDPMessageBus messageBus)
        {
            this.CDPMessageBus = messageBus;
            this.OpenClosePanelCommand = ReactiveCommandCreator.Create(this.ExecuteOpenClosePanel);

            this.CDPMessageBus.Listen<NavigationPanelEvent>()
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
        public ReactiveCommand<Unit, Unit> OpenClosePanelCommand { get; private set; }

        /// <summary>
        /// Executes the Open or Close panel command
        /// </summary>
        private void ExecuteOpenClosePanel()
        {
            var panelNavigationService = ServiceLocator.Current.GetInstance<IPanelNavigationService>();

            if (this.IsChecked)
            {
                panelNavigationService.OpenInDock(new PropertyGridViewModel(true, this.CDPMessageBus));
            }
            else
            {
                panelNavigationService.CloseInDock(typeof(PropertyGridViewModel));
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