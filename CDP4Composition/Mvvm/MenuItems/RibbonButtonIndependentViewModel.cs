//  --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RibbonButtonIndependentViewModel.cs" company="RHEA System S.A.">
//     Copyright (c) 2015-2020 RHEA System S.A.
// 
//     Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft,
//             Nathanael Smiechowski, Kamil Wojnowski
// 
//     This file is part of CDP4-IME Community Edition.
//     The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//     compliant with ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The CDP4-IME Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or any later version.
// 
//     The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//     GNU Affero General Public License for more details.
// 
//     You should have received a copy of the GNU Affero General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Mvvm
{
    using System;
    using System.Reactive.Linq;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Events;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;

    using Microsoft.Practices.ServiceLocation;

    using ReactiveUI;

    /// <summary>
    /// The base class representing Ribbon button that is independent from iterations and options to show a <see cref="IPanelView"/>
    /// </summary>
    public class RibbonButtonIndependentViewModel : ReactiveObject
    {
        /// <summary>
        /// The application's <see cref="IPanelNavigationService"/>
        /// </summary>
        protected readonly IPanelNavigationService PanelNavigationServive;

        /// <summary>
        /// The <see cref="IThingDialogNavigationService"/>
        /// </summary>
        protected readonly IThingDialogNavigationService ThingDialogNavigationService;

        /// <summary>
        /// The <see cref="IDialogNavigationService"/>
        /// </summary>
        protected readonly IDialogNavigationService DialogNavigationService;

        /// <summary>
        /// The <see cref="IPluginSettingsService"/> used to read and write plugin setting files.
        /// </summary>
        protected readonly IPluginSettingsService PluginSettingsService;

        /// <summary>
        /// The Function returning an instance of <see cref="IPanelViewModel"/>
        /// </summary>
        protected readonly Func<IThingDialogNavigationService, IPanelNavigationService, IDialogNavigationService, IPluginSettingsService, IPanelViewModel> InstantiatePanelViewModelFunction;

        /// <summary>
        /// Initializes a new instance of the <see cref="RibbonButtonIndependentViewModel"/> class
        /// </summary>
        /// <param name="instantiatePanelViewModelFunction">
        /// The instantiate Panel View Model Function.
        /// </param>
        protected RibbonButtonIndependentViewModel(Func<IThingDialogNavigationService, IPanelNavigationService, IDialogNavigationService, IPluginSettingsService, IPanelViewModel> instantiatePanelViewModelFunction)
        {
            this.PanelNavigationServive = ServiceLocator.Current.GetInstance<IPanelNavigationService>();
            this.ThingDialogNavigationService = ServiceLocator.Current.GetInstance<IThingDialogNavigationService>();
            this.DialogNavigationService = ServiceLocator.Current.GetInstance<IDialogNavigationService>();
            this.PluginSettingsService = ServiceLocator.Current.GetInstance<IPluginSettingsService>();

            this.ShowOrClosePanelCommand = ReactiveCommand.Create();
            this.ShowOrClosePanelCommand.Subscribe(x => this.ExecuteShowOrHide());

            CDPMessageBus.Current.Listen<NavigationPanelEvent>()
                .Where(x => x.ViewModel == this.PanelViewModel && x.PanelStatus == PanelStatus.Closed)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => this.HandleClosedPanel());

            this.InstantiatePanelViewModelFunction = instantiatePanelViewModelFunction;
        }

        /// <summary>
        /// Gets the <see cref="IPanelViewModel"/> to open associated with this menu item
        /// </summary>
        public IPanelViewModel PanelViewModel { get; private set; }

        /// <summary>
        /// Gets the command to show or close a model browser panel
        /// </summary>
        public ReactiveCommand<object> ShowOrClosePanelCommand { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IPanelViewModel"/>
        /// </summary>
        /// <returns>a new instance of the <see cref="IPanelViewModel"/></returns>
        protected IPanelViewModel InstantiatePanelViewModel()
        {
            return this.InstantiatePanelViewModelFunction(this.ThingDialogNavigationService, this.PanelNavigationServive, this.DialogNavigationService, this.PluginSettingsService);
        }

        /// <summary>
        /// Executes the show or hide panel command
        /// </summary>
        protected void ExecuteShowOrHide()
        {
            if (this.PanelViewModel == null)
            {
                this.PanelViewModel = this.InstantiatePanelViewModel();

                if (this.PanelViewModel == null)
                {
                    throw new InvalidOperationException("The view-model to navigate to is null.");
                }

                this.PanelNavigationServive.Open(this.PanelViewModel, true);
            }
            else
            {
                this.PanelNavigationServive.Close(this.PanelViewModel, true);
            }
        }

        /// <summary>
        /// Handles the close panel event sent by the Navigation Service
        /// </summary>
        private void HandleClosedPanel()
        {
            this.PanelViewModel = null;
        }
    }
}
