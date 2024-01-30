// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PanelNavigationService.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2024 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4Composition.Navigation
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Diagnostics;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Windows.Threading;

    using CDP4Common.CommonData;

    using CDP4Composition.Navigation.Events;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.Services;
    using CDP4Composition.ViewModels;

    using CDP4Dal;
    using CDP4Dal.Composition;

    using NLog;

    /// <summary>
    /// The panel navigation service class that provides services to open a docking panel given a <see cref="Thing"/> or a <see cref="IPanelViewModel"/>
    /// </summary>
    [Export(typeof(IPanelNavigationService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class PanelNavigationService : IPanelNavigationService
    {
        /// <summary>
        /// The view model that represents the main docking panel
        /// </summary>
        private readonly DockLayoutViewModel dockLayoutViewModel;

        /// <summary>
        /// The (injected) <see cref="IFilterStringService"/>
        /// </summary>
        private readonly IFilterStringService filterStringService;

        /// <summary name="messageBus">
        /// The <see cref="ICDPMessageBus"/>
        /// </summary>
        private readonly ICDPMessageBus messageBus;

        /// <summary>
        /// The logger for the current class
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Gets the list of <see cref="IPanelView"/> in the application
        /// </summary>
        public Dictionary<string, IPanelView> PanelViewKinds { get; private set; }

        /// <summary>
        /// Gets the list of <see cref="IPanelView"/> in the application
        /// </summary>
        public Dictionary<string, IPanelViewModel> PanelViewModelKinds { get; private set; }

        /// <summary>
        /// Gets the <see cref="IPanelViewModel"/>, <see cref="IPanelView"/>} pairs that are in the AddIn regions
        /// </summary>
        public Dictionary<IPanelViewModel, IPanelView> AddInViewModelViewPairs { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PanelNavigationService"/> class
        /// </summary>
        /// <param name="panelViewKinds">
        /// The injected list of <see cref="IPanelView"/> that can be navigated to.
        /// </param>
        /// <param name="panelViewModelKinds">
        /// The MEF injected Panel view models that can be navigated to.
        /// </param>
        /// <param name="panelViewModelDecorated">
        /// The MEF injected <see cref="IPanelViewModel"/> which are decorated with <see cref="INameMetaData"/> and can be navigated to.
        /// </param>
        /// <param name="filterStringService">The MEF injected <see cref="IFilterStringService"/></param>
        /// <param name="messageBus">
        /// The <see cref="ICDPMessageBus"/>
        /// </param>
        [ImportingConstructor]
        public PanelNavigationService(
            [ImportMany] IEnumerable<IPanelView> panelViewKinds,
            [ImportMany] IEnumerable<IPanelViewModel> panelViewModelKinds,
            [ImportMany] IEnumerable<Lazy<IPanelViewModel, INameMetaData>> panelViewModelDecorated,
            DockLayoutViewModel dockLayoutViewModel,
            IFilterStringService filterStringService,
            ICDPMessageBus messageBus)
        {
            var sw = new Stopwatch();
            sw.Start();
            logger.Debug("Instantiating the PanelNavigationService");
            this.dockLayoutViewModel = dockLayoutViewModel;
            this.filterStringService = filterStringService;
            this.messageBus = messageBus;

            this.dockLayoutViewModel.DockPanelViewModels.ItemsRemoved.Subscribe(this.CleanUpPanelsAndSendCloseEvent);

            this.PanelViewKinds = new Dictionary<string, IPanelView>();

            // TODO T2428 : PanelViewModelKinds seems to be always empty and is used only one time in the Open(Thing thing, ISession session) method. We should probably refactor this part of the code.
            this.PanelViewModelKinds = new Dictionary<string, IPanelViewModel>();
            this.AddInViewModelViewPairs = new Dictionary<IPanelViewModel, IPanelView>();
            this.PanelViewModelDecorated = new Dictionary<string, Lazy<IPanelViewModel, INameMetaData>>();

            foreach (var panelView in panelViewKinds)
            {
                var panelViewName = panelView.ToString();

                this.PanelViewKinds.Add(panelViewName.ToString(), panelView);
                logger.Trace($"Add panelView {panelViewName} ");
            }

            foreach (var panelViewModel in panelViewModelKinds)
            {
                var panelViewModelName = panelViewModel.ToString();

                this.PanelViewModelKinds.Add(panelViewModelName, panelViewModel);
                logger.Trace($"Add panelViewModel {panelViewModelName} ");
            }

            foreach (var panelViewModel in panelViewModelDecorated)
            {
                var panelViewModelName = panelViewModel.Value.ToString();

                var panelViewModelDescribeName = panelViewModel.Metadata.Name;
                this.PanelViewModelDecorated.Add(panelViewModelDescribeName, panelViewModel);

                logger.Trace($"Add panelViewModel {panelViewModelName} ");
            }

            sw.Stop();
            logger.Debug($"The PanelNavigationService was instantiated in {sw.ElapsedMilliseconds} [ms] ");
        }

        /// <summary>
        /// Gets the list of the <see cref="IPanelViewModel"/> which are decorated with <see cref="INameMetaData"/>.
        /// </summary>
        public Dictionary<string, Lazy<IPanelViewModel, INameMetaData>> PanelViewModelDecorated { get; private set; }

        /// <summary>
        /// Opens the view associated to the provided view-model in the dock
        /// </summary>
        /// <param name="viewModel">
        /// The <see cref="IPanelViewModel"/> for which the associated view needs to be opened
        /// </param>
        /// <remarks>
        /// The data context of the view is the <see cref="IPanelViewModel"/>
        /// </remarks>
        public void OpenInDock(IPanelViewModel viewModel)
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException(nameof(viewModel), $"The {nameof(IPanelViewModel)} may not be null");
            }

            var sw = Stopwatch.StartNew();

            this.dockLayoutViewModel.AddDockPanelViewModel(viewModel);

            this.filterStringService.RegisterForService(viewModel);

            logger.Trace("Navigated to Panel {0} in {1} [ms]", viewModel, sw.ElapsedMilliseconds);
        }

        /// <summary>
        /// Opens the view associated to a view-model. The view-model is identified by its <see cref="INameMetaData.Name"/>.
        /// </summary>
        /// <param name="viewModelName">The name we want to compare to the <see cref="INameMetaData.Name"/> of the view-models.</param>
        /// <param name="session">The <see cref="ISession"/> associated.</param>
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService"/>.</param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/>.</param>
        public void OpenInDock(
            string viewModelName,
            ISession session,
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

            this.OpenInDock(viewModel);
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
        public void OpenExistingOrOpenInAddIn(IPanelViewModel viewModel)
        {
            if (this.AddInViewModelViewPairs.TryGetValue(viewModel, out var view))
            {
                var openPanelEvent = new NavigationPanelEvent(viewModel, view, PanelStatus.Open);
                this.messageBus.SendMessage(openPanelEvent);
            }
            else
            {
                this.OpenInAddIn(viewModel);
            }
        }

        /// <summary>
        /// Closes the <see cref="IPanelView"/> associated to the <see cref="IPanelViewModel"/>
        /// </summary>
        /// <param name="viewModel">
        /// The view-model that is to be closed.
        /// </param>
        public void CloseInDock(IPanelViewModel viewModel)
        {
            logger.Debug("Starting to Close view-model {0} of type {1}", viewModel.Caption, viewModel);

            this.dockLayoutViewModel.DockPanelViewModels.Remove(viewModel);

            logger.Debug("Closed view-model {0} of type {1}", viewModel.Caption, viewModel);
        }

        /// <summary>
        /// Closes all the <see cref="IPanelView"/> associated to a data-source
        /// </summary>
        /// <param name="datasourceUri">The string representation of the data-source's uri</param>
        public void CloseInDock(string datasourceUri)
        {
            logger.Debug("Starting to close all view-models related to data-source {0}", datasourceUri);

            var openViewModels = this.dockLayoutViewModel
                .DockPanelViewModels
                .Where(x => x.DataSource == datasourceUri)
                .ToList();

            foreach (var panelViewModel in openViewModels)
            {
                this.CloseInDock(panelViewModel);
            }

            logger.Debug("All view-models related to data-source {0} closed", datasourceUri);
        }

        /// <summary>
        /// Closes all the <see cref="IPanelView"/> which associated <see cref="IPanelViewModel"/> is of a certain Type
        /// </summary>
        /// <param name="viewModelType">The <see cref="Type"/> of the <see cref="IPanelViewModel"/> to close</param>
        public void CloseInDock(Type viewModelType)
        {
            var viewModels = this.dockLayoutViewModel
                .DockPanelViewModels
                .Where(vm => vm.GetType() == viewModelType)
                .ToList();

            foreach (var vm in viewModels)
            {
                this.CloseInDock(vm);
            }
        }

        /// <summary>
        /// removes the view and view-model from the <see cref="AddInViewModelViewPairs"/> and send a panel close event
        /// </summary>
        /// <param name="panelViewModel">
        /// The <see cref="IPanelViewModel"/> that needs to be cleaned up
        /// </param>
        /// <param name="panelView">
        /// The <see cref="IPanelView"/> that needs to be cleaned up
        /// </param>
        private void CleanUpPanelsAndSendCloseEvent(IPanelViewModel panelViewModel, IPanelView panelView)
        {
            this.AddInViewModelViewPairs.Remove(panelViewModel);

            var closePanelEvent = new NavigationPanelEvent(panelViewModel, panelView, PanelStatus.Closed);
            this.messageBus.SendMessage(closePanelEvent);

            panelView.DataContext = null;
            panelViewModel.Dispose();

            // unregister from filter string service
            this.filterStringService.UnregisterFromService(panelViewModel);

            this.OptimizeMemoryUsage();
        }

        /// <summary>
        /// Finalizes the <see cref="IPanelViewModel"/> on close        
        /// </summary>
        /// <param name="panelViewModel">
        /// The <see cref="IPanelViewModel"/> that needs to be cleaned up
        /// </param>
        private void CleanUpPanelsAndSendCloseEvent(IPanelViewModel panelViewModel)
        {
            var closePanelEvent = new NavigationPanelEvent(panelViewModel, null, PanelStatus.Closed);
            this.messageBus.SendMessage(closePanelEvent);

            panelViewModel.Dispose();

            // unregister from filter string service
            this.filterStringService.UnregisterFromService(panelViewModel);

            this.OptimizeMemoryUsage();
        }

        /// <summary>
        /// Optimize the application's memory usage
        /// </summary>
        private void OptimizeMemoryUsage()
        {
            Dispatcher.CurrentDispatcher.InvokeAsync(GC.Collect, DispatcherPriority.Background);
        }

        /// <summary>
        /// Opens the view associated with the <see cref="IPanelViewModel"/> in the AddIn
        /// </summary>
        /// <param name="viewModel">The <see cref="IPanelViewModel"/> to open</param>
        public void OpenInAddIn(IPanelViewModel viewModel)
        {
            var viewType = this.GetViewType(viewModel);

            var parameters = new object[] { true };
            var view = Activator.CreateInstance(viewType, parameters) as IPanelView;

            if (view != null)
            {
                view.DataContext = viewModel;
                this.AddInViewModelViewPairs.Add(viewModel, view);

                // register for Filter Service
                this.filterStringService.RegisterForService(viewModel);
            }

            var openPanelEvent = new NavigationPanelEvent(viewModel, view, PanelStatus.Open);
            this.messageBus.SendMessage(openPanelEvent);
        }

        /// <summary>
        /// Closes the view associated with the <see cref="IPanelViewModel"/> in the AddIn
        /// </summary>
        /// <param name="viewModel">The <see cref="IPanelViewModel"/> to close</param>
        public void CloseInAddIn(IPanelViewModel viewModel)
        {
            if (this.AddInViewModelViewPairs.TryGetValue(viewModel, out var view))
            {
                this.CleanUpPanelsAndSendCloseEvent(viewModel, view);
            }
        }

        /// <summary>
        /// Gets the fully qualified name of the <see cref="IPanelView"/> associated to the <see cref="IPanelViewModel"/>
        /// </summary>
        /// <remarks>
        /// We assume here that for a <see cref="IPanelViewModel"/> with a fully qualified name xxx.yyy.ViewModels.DialogViewModel, the counterpart view is xxx.yyy.Views.Dialog
        /// </remarks>
        /// <param name="viewModel">The <see cref="IPanelViewModel"/></param>
        /// <returns>The Fully qualified Name</returns>
        private Type GetViewType(IPanelViewModel viewModel)
        {
            var fullyQualifiedName = viewModel.ToString().Replace(".ViewModels.", ".Views.");

            // remove "ViewModel" from the name to get the View Name
            var viewName = Regex.Replace(fullyQualifiedName, "ViewModel$", string.Empty);

            if (!this.PanelViewKinds.TryGetValue(viewName, out var viewInstance))
            {
                throw new ArgumentOutOfRangeException($"The View associated to the viewModel {viewModel} could not be found\nMake sure the view has the proper attributes");
            }

            return viewInstance.GetType();
        }
    }
}
