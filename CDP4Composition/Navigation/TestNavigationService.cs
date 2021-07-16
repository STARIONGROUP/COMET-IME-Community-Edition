// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PanelNavigationService.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
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
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Navigation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel.Composition;
    using System.Diagnostics;
    using System.Linq;

    using CDP4Common.CommonData;

    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Events;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.Services;

    using CDP4Dal;
    using CDP4Dal.Composition;
    using DevExpress.Xpf.Docking;
    using Microsoft.Practices.Prism.Regions;

    using NLog;

    /// <summary>
    /// The panel navigation service class that provides services to open a docking panel given a <see cref="Thing"/> or a <see cref="IPanelViewModel"/>
    /// </summary>
    [Export(typeof(IPanelNavigationService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TestNavigationService : IPanelNavigationService
    {
        private readonly DockLayoutViewModel dockLayoutViewModel;

        /// <summary>
        /// The (injected) <see cref="IFilterStringService"/>
        /// </summary>
        private readonly IFilterStringService filterStringService;

        /// <summary>
        /// The logger for the current class
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="PanelNavigationService"/> class
        /// </summary>
        /// <param name="panelViewKinds">
        /// The injected list of <see cref="IPanelView"/> that can be navigated to.
        /// </param>
        /// <param name="panelViewModelKinds">
        /// The MEF injected Panel view models that can be navigated to.
        /// </param>
        /// <param name="regionManager">
        /// the <see cref="IRegionManager"/> that is used to manage the regions
        /// </param>
        /// <param name="panelViewModelDecorated">
        /// The MEF injected <see cref="IPanelViewModel"/> which are decorated with <see cref="INameMetaData"/> and can be navigated to.
        /// </param>
        /// <param name="filterStringService">The MEF injected <see cref="IFilterStringService"/></param>
        /// <param name="regionCollectionSearcher">The MEF injected <see cref="IRegionCollectionSearcher"/></param>
        [ImportingConstructor]
        public TestNavigationService(
            [ImportMany] IEnumerable<Lazy<IPanelViewModel, INameMetaData>> panelViewModelDecorated,
            [Import] DockLayoutViewModel dockLayoutViewModel,
            IFilterStringService filterStringService)
        {
            var sw = new Stopwatch();
            sw.Start();
            logger.Debug("Instantiating the PanelNavigationService");
            this.dockLayoutViewModel = dockLayoutViewModel;
            this.filterStringService = filterStringService;

            this.dockLayoutViewModel.DockPanelViewModels.ItemsRemoved.Subscribe(ItemsRemoved);

            this.PanelViewModelDecorated = new Dictionary<string, Lazy<IPanelViewModel, INameMetaData>>();

            foreach (var panelViewModel in panelViewModelDecorated)
            {
                var panelViewModelName = panelViewModel.Value.ToString();

                var panelViewModelDescribeName = panelViewModel.Metadata.Name;
                this.PanelViewModelDecorated.Add(panelViewModelDescribeName, panelViewModel);

                logger.Trace("Add panelViewModel {0} ", panelViewModelName);
            }

            sw.Stop();
            logger.Debug("The PanelNavigationService was instantiated in {0} [ms]", sw.ElapsedMilliseconds);
        }

        private void ItemsRemoved(IMVVMDockingProperties obj)
        {
            CleanUpPanelsAndSendCloseEvent((IPanelViewModel)obj);
        }

        /// <summary>
        /// Gets the list of the <see cref="IPanelViewModel"/> which are decorated with <see cref="INameMetaData"/>.
        /// </summary>
        public Dictionary<string, Lazy<IPanelViewModel, INameMetaData>> PanelViewModelDecorated { get; private set; }

        /// <summary>
        /// Opens the <see cref="IPanelView"/> associated to the <see cref="IPanelViewModel"/>
        /// </summary>
        /// <param name="viewModel">The <see cref="IPanelViewModel"/></param>
        private void Open(IPanelViewModel viewModel)
        {
            var sw = Stopwatch.StartNew();

            this.dockLayoutViewModel.AddDockPanelViewModel(viewModel);

            //TODO: refactor so there's no dependency on the view. Use databinding from the View to ViewModel
            this.filterStringService.RegisterForService(viewModel);

            logger.Trace("Navigated to Panel {0} in {1} [ms]", viewModel, sw.ElapsedMilliseconds);
        }

        /// <summary>
        /// Opens the view associated to the provided view-model
        /// </summary>
        /// <param name="viewModel">
        /// The <see cref="IPanelViewModel"/> for which the associated view needs to be opened
        /// </param>
        /// <param name="useRegionManager">
        /// A value indicating whether handling the opening of the view shall be handled by the region manager. In case this region manager does not handle
        /// this it will be event-based using the <see cref="CDPMessageBus"/>.
        /// </param>
        /// <remarks>
        /// The data context of the view is the <see cref="IPanelViewModel"/>
        /// </remarks>
        public void Open(IPanelViewModel viewModel, bool useRegionManager)
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException(nameof(viewModel), "The IPanelViewModel may not be null");
            }

            this.Open(viewModel);
        }

        /// <summary>
        /// Opens the view associated to a view-model. The view-model is identified by its <see cref="INameMetaData.Name"/>.
        /// </summary>
        /// <param name="viewModelName">The name we want to compare to the <see cref="INameMetaData.Name"/> of the view-models.</param>
        /// <param name="session">The <see cref="ISession"/> associated.</param>
        /// <param name="useRegionManager">A value indicating whether handling the opening of the view shall be handled by the region manager.
        /// In case this region manager does not handle this, it will be event-based using the <see cref="CDPMessageBus"/>.</param>
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService"/>.</param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/>.</param>
        public void Open(
            string viewModelName,
            ISession session,
            bool useRegionManager,
            IThingDialogNavigationService thingDialogNavigationService,
            IDialogNavigationService dialogNavigationService)
        {
            
            if (!this.PanelViewModelDecorated.TryGetValue(viewModelName, out var returned))
            {
                throw new ArgumentOutOfRangeException($"The ViewModel with the human readable name {viewModelName} could not be found");
            }

            var siteDirectory = session.RetrieveSiteDirectory();

            // TODO T2429 : check that the view model is associated to a site directory
            var parameters = new object[]
                { session, siteDirectory, thingDialogNavigationService, this, dialogNavigationService };

            var viewModel = Activator.CreateInstance(returned.Value.GetType(), parameters) as IPanelViewModel;

            this.Open(viewModel, useRegionManager);
        }
        
        /// <summary>
        /// Re-opens an exisiting View associated to the provided view-model, or opens a new View
        /// Re-opening is done by sending a <see cref="CDPMessageBus"/> event.
        /// This event can be handled by more specific code,  for example in the addin, where some
        /// ViewModels should not close at all. For those viewmodels visibility is toggled on every
        /// <see cref="NavigationPanelEvent"/> event that has <see cref="PanelStatus.Open"/> set.
        /// </summary>
        /// <param name="viewModel">
        /// The <see cref="IPanelViewModel"/> for which the associated view needs to be opened, or closed
        /// </param>
        /// <param name="useRegionManager">
        /// A value indicating whether handling the opening of the view shall be message-based or not. In case it is
        /// NOT message-based, the <see cref="IRegionManager"/> handles opening and placement of the view.
        /// </param>
        /// <remarks>
        /// The data context of the view is the <see cref="IPanelViewModel"/>
        /// </remarks>
        public void OpenExistingOrOpen(IPanelViewModel viewModel, bool useRegionManager)
        {
            this.Open(viewModel, useRegionManager);
        }

        /// <summary>
        /// Closes the <see cref="IPanelView"/> associated to the <see cref="IPanelViewModel"/>
        /// </summary>
        /// <param name="viewModel">The <see cref="IPanelViewModel"/></param>
        private void CloseByRegion(IPanelViewModel viewModel)
        {            
            logger.Debug("Starting to Close view-model {0} of type {1}", viewModel.Caption, viewModel);

            this.dockLayoutViewModel.DockPanelViewModels.Remove(viewModel);

            logger.Debug("Closed view-model {0} of type {1}", viewModel.Caption, viewModel);
        }

        /// <summary>
        /// Closes the <see cref="IPanelView"/> associated to the <see cref="IPanelViewModel"/>
        /// </summary>
        /// <param name="viewModel">
        /// The view-model that is to be closed.
        /// </param>
        /// <param name="useRegionManager">
        /// A value indicating whether handling the opening of the view shall be handled by the region manager. In case this region manager does not handle
        /// this it will be event-based using the <see cref="CDPMessageBus"/>.
        /// </param>
        public void Close(IPanelViewModel viewModel, bool useRegionManager)
        {
            this.CloseByRegion(viewModel);
        }

        /// <summary>
        /// Closes all the <see cref="IPanelView"/> associated to a data-source
        /// </summary>
        /// <param name="datasourceUri">The string representation of the data-source's uri</param>
        public void Close(string datasourceUri)
        {
            logger.Debug("Starting to close all view-models related to data-source {0}", datasourceUri);

            var openViewModels = this.dockLayoutViewModel
                .DockPanelViewModels
                .OfType<IPanelViewModel>()
                .Where(x => x.DataSource == datasourceUri)
                .ToList();

            foreach (var panelViewModel in openViewModels)
            {
                this.CloseByRegion(panelViewModel);
            }

            logger.Debug("All view-models related to data-source {0} closed", datasourceUri);
        }

        /// <summary>
        /// Closes all the <see cref="IPanelView"/> which associated <see cref="IPanelViewModel"/> is of a certain Type
        /// </summary>
        /// <param name="viewModelType">The <see cref="Type"/> of the <see cref="IPanelViewModel"/> to close</param>
        public void Close(Type viewModelType)
        {
            var viewModels = this.dockLayoutViewModel
                                 .DockPanelViewModels
                                 .Where(vm => vm.GetType() == viewModelType)
                                 .ToList();

            foreach (var vm in viewModels)
            {
                this.CloseByRegion((IPanelViewModel)vm);
            }
        }

        /// <summary>
        /// removes the view and view-model from the <see cref="ViewModelViewPairs"/> and send a panel close event
        /// </summary>
        /// <param name="panelViewModel">
        /// The <see cref="IPanelViewModel"/> that needs to be cleaned up
        /// </param>
        private void CleanUpPanelsAndSendCloseEvent(IPanelViewModel panelViewModel)
        {
            var closePanelEvent = new NavigationPanelEvent(panelViewModel, null, PanelStatus.Closed);
            CDPMessageBus.Current.SendMessage(closePanelEvent);

            panelViewModel.Dispose();

            // unregister from filter string service
            this.filterStringService.UnregisterFromService(panelViewModel);
        }
    }
}
